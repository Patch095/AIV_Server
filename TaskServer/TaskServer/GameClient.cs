using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace TaskServer
{
    public class GameClient
    {
        private EndPoint endPoint;

        private Queue<Packet> sendQueue;

        private Dictionary<uint, Packet> ackTable;

        //GameServer is no longer a static class, now every GameClient have is own server
        private GameServer server;
        public GameServer Server { get { return server; } }

        //Makus is a read-only propherty, this will increase the security
        private uint malus;
        public uint Malus { get { return malus; } }
        private float malusTimeStamp;
        public float MalusTimeStamp { get { return malusTimeStamp; } }
        public bool CanReduceClientMalus { get { return malus > 0 && MalusTimeStamp <= Server.Now; } }

        public void IncreaseMalus(uint malusValue = 0)
        {
            if (malusValue == 0)
                malus++;
            else
                malus += malusValue;

            malusTimeStamp = Server.Now + 300f;
        }

        public void ReduceMalus()
        {
            if (CanReduceClientMalus)
            {
                malus--;
            }
            else
                IncreaseMalus();
        }

        public GameClient(EndPoint endPoint, GameServer server)
        {
            this.endPoint = endPoint;
            sendQueue = new Queue<Packet>();
            ackTable = new Dictionary<uint, Packet>();
            malus = 0;

            this.server = server;
        }

        public void Process()
        {
            int packetsInQueue = sendQueue.Count;
            for (int i = 0; i < packetsInQueue; i++)
            {
                Packet packet = sendQueue.Dequeue();
                // check if the packet con be sent
                if (server.Now >= packet.SendAfter)
                {
                    packet.IncreaseAttempts();
                    if (server.Send(packet, endPoint))
                    {
                        // all fine
                        if (packet.NeedAck)
                        {
                            ackTable[packet.Id] = packet;
                        }
                    }
                    // on error, retry sending only if NOT OneShot
                    else if (!packet.OneShot)
                    {
                        if (packet.Attempts < 3)
                        {
                            // retry sending after 1 second
                            packet.SendAfter = server.Now + 1.0f;
                            sendQueue.Enqueue(packet);
                        }
                    }
                }
                else
                {
                    // it is too early, re-enqueue the packet
                    sendQueue.Enqueue(packet);
                }
            }

            // check ack table
            List<uint> deadPackets = new List<uint>();
            foreach (uint id in ackTable.Keys)
            {
                Packet packet = ackTable[id];
                if (packet.IsExpired(Server.Now))
                {
                    if (packet.Attempts < 3)
                    {
                        sendQueue.Enqueue(packet);
                    }
                    else
                    {
                        deadPackets.Add(id);
                    }
                }
            }

            foreach (uint id in deadPackets)
            {
                ackTable.Remove(id);
            }
        }

        public void Ack(uint packetId)
        {
            if (ackTable.ContainsKey(packetId))
            {
                ackTable.Remove(packetId);
            }
            else
            {
                IncreaseMalus();
            }
        }

        public void Enqueue(Packet packet)
        {
            sendQueue.Enqueue(packet);
        }

        public override string ToString()
        {
            return endPoint.ToString();
        }
    }
}