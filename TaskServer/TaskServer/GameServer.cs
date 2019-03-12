using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;

namespace TaskServer
{
    //Stat
    public class GameServer
    {
        private delegate void GameCommand(byte[] data, EndPoint sender);

        private Dictionary<byte, GameCommand> commandsTable;

        private Dictionary<EndPoint, GameClient> clientsTable;
        private Dictionary<uint, GameObject> gameObjectsTable;

        public void Join(byte[] data, EndPoint sender)
        {
            // check if the client has already joined
            if (clientsTable.ContainsKey(sender))
            {
                GameClient badClient = clientsTable[sender];
                badClient.IncreaseMalus();
                return;
            }

            GameClient newClient = new GameClient(sender, this);
            clientsTable[sender] = newClient;
            //Avatar avatar = new Avatar(newClient.Server);
            Avatar avatar = Spawn<Avatar>();
            avatar.SetOwner(newClient);
            Packet welcome = new Packet(1, avatar.ObjectType, avatar.Id, avatar.X, avatar.Y, avatar.Z);
            welcome.NeedAck = true;
            newClient.Enqueue(welcome);

            // spawn all server's objects in the new client
            foreach (GameObject gameObject in gameObjectsTable.Values)
            {
                //ignore myself
                if (gameObject == avatar)
                    continue;
                Packet spawn = new Packet(2, gameObject.ObjectType, gameObject.Id, gameObject.X, gameObject.Y, gameObject.Z);//1 - 5*4
                spawn.NeedAck = true;
                newClient.Enqueue(spawn);
            }

            // informs the other clients about the new one
            Packet newClientSpawned = new Packet(2, avatar.ObjectType, avatar.Id, avatar.X, avatar.Y, avatar.Z);
            newClientSpawned.NeedAck = true;
            SendToAllClientsExceptOne(newClientSpawned, newClient);

            Console.WriteLine("client {0} joined with avatar {1}", newClient, avatar.Id);
        }

        public void Ack(byte[] data, EndPoint sender)
        {
            if (!clientsTable.ContainsKey(sender))
            {
                return;
            }

            GameClient client = clientsTable[sender];
            uint packetId = BitConverter.ToUInt32(data, 1);
            client.Ack(packetId);
        }

        public void Update(byte[] data, EndPoint sender)
        {
            if (!clientsTable.ContainsKey(sender))
            {
                return;
            }
            GameClient client = clientsTable[sender];
            uint netId = BitConverter.ToUInt32(data, 1);
            if (gameObjectsTable.ContainsKey(netId))
            {
                GameObject gameObject = gameObjectsTable[netId];
                if (gameObject.IsOwnedBy(client))
                {
                    float x = BitConverter.ToSingle(data, 5);
                    float y = BitConverter.ToSingle(data, 9);
                    float z = BitConverter.ToSingle(data, 13);
                    gameObject.SetPosition(x, y, z);
                }
                //bad behaviour, malus update
                else
                {
                    client.IncreaseMalus();
                }
            }
        }


        public void Exit(byte[] data, EndPoint sender)
        {
            // check if the client has already joined
            if (clientsTable.ContainsKey(sender))
            {
                GameClient badClient = clientsTable[sender];
                badClient.IncreaseMalus();
                return;
            }

            GameClient logOutClient = clientsTable[sender];
            // remove all item of the client
            foreach (GameObject gameObject in gameObjectsTable.Values)
            {
                if (gameObject.Owner == logOutClient)
                {
                    gameObjectsTable.Remove(gameObject.Id);
                    //4 = destroy item 
                    Packet removeItem = new Packet(4, gameObject);
                    removeItem.NeedAck = true;
                    SendToAllClients(removeItem);
                }
            }

            clientsTable.Remove(sender);
            //5 = log out client
            Packet removeClient = new Packet(5, logOutClient);
            removeClient.NeedAck = true;
            SendToAllClients(removeClient);

            //Console.WriteLine("client {0} joined with avatar {1}", newClient, avatar.Id);
        }


        private IMonotonicClock serverClock;
        public float Now { get { return serverClock.GetNow(); } }
        private float currentNow;

        private IGameTransport transport;

        public uint NumClients { get { return (uint)clientsTable.Count; } }
        public uint NumGameObjs { get { return (uint)gameObjectsTable.Count; } }

        public GameServer(IGameTransport transport, IMonotonicClock clock)
        {
            clientsTable = new Dictionary<EndPoint, GameClient>();
            gameObjectsTable = new Dictionary<uint, GameObject>();
            commandsTable = new Dictionary<byte, GameCommand>();
            commandsTable[0] = Join;
            commandsTable[3] = Update;

            commandsTable[7] = Exit;

            commandsTable[255] = Ack;

            serverClock = clock;
            this.transport = transport;
        }

        public void Start()
        {
            Console.WriteLine("server started");
            while (true)
            {
                SingleStep();
            }
        }

        private float checkMalusTimer = 0.0f;
        private float timeToWaitBeforeCheckClientsMalus = 25.0f;
        public void CheckMalus()
        {
            if (checkMalusTimer <= Now)
            {
                foreach (GameClient client in clientsTable.Values)
                {
                    uint serverMalus = client.Malus;
                    if (serverMalus > 0)
                    {
                        if (serverMalus >= 7)
                        {
                            //remove badClient with too much malus value
                        }
                        else if (client.CanReduceClientMalus)
                            client.ReduceMalus();
                    }
                    else
                        continue;
                }
            }
            checkMalusTimer = Now + timeToWaitBeforeCheckClientsMalus;
        }

        public void SingleStep()
        {
            currentNow = serverClock.GetNow();
            EndPoint sender = transport.CreateEndPoint();
            byte[] data = transport.Recv(256, ref sender);
            if (data != null)
            {
                byte gameCommand = data[0];
                if (commandsTable.ContainsKey(gameCommand))
                {
                    commandsTable[gameCommand](data, sender);
                }
            }

            foreach (GameClient client in clientsTable.Values)
            {
                client.Process();
            }

            foreach (GameObject gameObject in gameObjectsTable.Values)
            {
                gameObject.Tick();
            }

            if (checkMalusTimer <= Now)
            {
                CheckMalus();
            }
        }

        public bool Send(Packet packet, EndPoint endPoint)
        {
            return transport.Send(packet.GetData(), endPoint);
        }
        public void SendToAllClients(Packet packet)
        {
            foreach (GameClient client in clientsTable.Values)
            {
                client.Enqueue(packet);
            }
        }
        public void SendToAllClientsExceptOne(Packet packet, GameClient except)
        {
            foreach (GameClient client in clientsTable.Values)
            {
                if (client != except)
                    client.Enqueue(packet);
            }
        }

        public void RegisterGameObject(GameObject gameObject)
        {
            if (gameObjectsTable.ContainsKey(gameObject.Id))
                throw new Exception("GameObject already registered");
            gameObjectsTable[gameObject.Id] = gameObject;

        }

        public T Spawn<T>() where T : GameObject
        {
            object[] ctorParams = { this };

            T newGameObject = Activator.CreateInstance(typeof(T), ctorParams) as T;
            RegisterGameObject(newGameObject);
            return newGameObject;
        }

        public GameObject GetGameObj(uint objId)
        {
            if (gameObjectsTable.ContainsKey(objId))
                return gameObjectsTable[objId];
            else
                return null;
        }
    }
}

/* AGGIUNTE:
    - malus, gestione via avatar/server

    - WIP GameClient LogOut
    - WIP GameObj removal
*/