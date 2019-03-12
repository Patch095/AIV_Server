using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskServer
{
    public class Avatar : GameObject
    {
        public Avatar(GameClient owner) : base(1, owner)
        {

        }
        public Avatar(GameServer server) : base(1, server)
        {

        }

        public uint Malus { get { return Owner.Malus; } }
        public void IncreaseMalus(uint malusValue = 0)
        {
            Owner.IncreaseMalus(malusValue);
        }
        public void ReduceMalus()
        {
            Owner.ReduceMalus();
        }

        public override void Tick()
        {
            Packet packet = new Packet(3, Id, X, Y, Z);
            packet.OneShot = true;
            Server.SendToAllClients(packet);
        }
    }
}