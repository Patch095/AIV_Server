using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TaskServer.Test
{
    public class TestGameServer
    {
        public TestGameServer()
        {
        }

        private FakeTransport transport;
        private FakeClock clock;
        private GameServer server;

        //Called before every [Test]
        [SetUp]
        public void SetUp()
        {
            transport = new FakeTransport();
            clock = new FakeClock();
            server = new GameServer(transport, clock);
        }

        //Test if a new server has a serverCloak at zero
        [Test]
        public void TestGameServerGreenLigthZero()
        {
            Assert.That(server.Now, Is.EqualTo(0));
        }
        [Test]
        public void TestGameServerRedLigthZero()
        {
            Assert.That(server.Now, Is.Not.EqualTo(1));
        }

        //Test if a new server has no GameClient alredy logged in
        [Test]
        public void TestGreenLigthClientOnStart()
        {
            Assert.That(server.NumClients, Is.EqualTo(0));
        }
        [Test]
        public void TestRedLigthClientOnStart()
        {
            Assert.That(server.NumClients, Is.Not.EqualTo(1));
        }

        //Test if a new server has no GameObj alredy in
        [Test]
        public void TestGreenLigthObjOnStart()
        {
            Assert.That(server.NumClients, Is.EqualTo(0));
        }
        [Test]
        public void TestRedLigthObjOnStart()
        {
            Assert.That(server.NumClients, Is.Not.EqualTo(1));
        }

        //Test join number of client
        [Test]
        public void TestGreenLigthJoinNumOfClients()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            Assert.That(server.NumClients, Is.EqualTo(1));
        }
        [Test]
        public void TestRedLigthJoinNumOfClients()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            Assert.That(server.NumClients, Is.Not.EqualTo(0));
        }

        //Test join number of game objects
        [Test]
        public void TestGreenLigthJoinNumOfGameObj()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            Assert.That(server.NumGameObjs, Is.EqualTo(1));
        }
        [Test]
        public void TestRedLigthJoinNumOfGameObj()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            Assert.That(server.NumGameObjs, Is.Not.EqualTo(0));
        }

        //Test Welcome
        [Test]
        public void TestWelcomeAfterJoinGreenLigth()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            FakeData welcome = transport.ClientDequeue();
            Assert.That(welcome.data[0], Is.EqualTo(1));
        }
        [Test]
        public void TestWelcomeAfterJoinRedLigth()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            FakeData welcome = transport.ClientDequeue();
            Assert.That(welcome.data[0], Is.Not.EqualTo(0));
        }

        //Test Avatar creation after Join
        [Test]
        public void TestSpawnAvatarAfterJoinGreenLigth()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientDequeue();
            Assert.That(() => transport.ClientDequeue(), Throws.InstanceOf<FakeQueueEmpty>());
        }

        //Test multiple Join from the same Address
        [Test]
        public void TestJoinSameAddressMultipleClientGreenLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "tester", 1);
            server.SingleStep();
            Assert.That(server.NumClients, Is.EqualTo(2));
        }
        [Test]
        public void TestJoinSameAddressMultipleClientRedLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "tester", 1);
            server.SingleStep();
            Assert.That(server.NumClients, Is.Not.EqualTo(1));
        }

        //Test multiple Join from the same Door
        [Test]
        public void TestJoinSamePortMultipleClientGreenLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "foobar", 0);
            server.SingleStep();
            Assert.That(server.NumClients, Is.EqualTo(2));
        }
        [Test]
        public void TestJoinSamePortMultipleClientRedLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "foobar", 0);
            server.SingleStep();
            Assert.That(server.NumClients, Is.Not.EqualTo(1));
        }

        //Test multiple Join from the same Address & Door
        [Test]
        public void TestJoinSameAddressSamePortMultipleClientGreenLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            Assert.That(server.NumClients, Is.EqualTo(1));
        }
        [Test]
        public void TestJoinSameAddressSamePortMultipleClientRedLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            Assert.That(server.NumClients, Is.Not.EqualTo(0));
        }

        //Test multiple Join
        [Test]
        public void TestJoinTwoClientsWelcomeGreenLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();

            Assert.That(transport.ClientQueueCount, Is.EqualTo(5));

            Assert.That(transport.ClientDequeue().endPoint.Address, Is.EqualTo("tester"));
            Assert.That(transport.ClientDequeue().endPoint.Address, Is.EqualTo("tester"));
            Assert.That(transport.ClientDequeue().endPoint.Address, Is.EqualTo("tester"));
            Assert.That(transport.ClientDequeue().endPoint.Address, Is.EqualTo("foobar"));
            Assert.That(transport.ClientDequeue().endPoint.Address, Is.EqualTo("foobar"));

        }
        [Test]
        public void TestJoinTwoClientsWelcomeRedLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();

            Assert.That(transport.ClientQueueCount, Is.EqualTo(5));

            Assert.That(transport.ClientDequeue().endPoint.Address, Is.Not.EqualTo("foobar"));
            Assert.That(transport.ClientDequeue().endPoint.Address, Is.Not.EqualTo("foobar"));
            Assert.That(transport.ClientDequeue().endPoint.Address, Is.Not.EqualTo("foobar"));
            Assert.That(transport.ClientDequeue().endPoint.Address, Is.Not.EqualTo("tester"));
            Assert.That(transport.ClientDequeue().endPoint.Address, Is.Not.EqualTo("tester"));
        }

        //Test move an obj that isn't yours
        [Test]
        public void TestEvilUpdateGreenLight()
        {
            // TODO get the id from the welcome packets
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);
            //retrive GameObj from avatrId and store its position
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();

            //retrive GameObj from avatarId and check its previous position
            GameObject testerObj = server.GetGameObj(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            //get base position
            float startX = testerObj.X;
            float startY = testerObj.Y;
            float startZ = testerObj.Z;
            float offsetX = 10.0f;
            float offsetY = 20.0f;
            float offsetZ = 30.0f;
            Packet movePacket = new Packet(3, avatarId, offsetX, offsetY, offsetZ);
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();
            //get new position after the moviment
            float newX = testerObj.X;
            float newY = testerObj.Y;
            float newZ = testerObj.Z;
            //check position previus and after the movent; the valor should be the same because the movemnt shouldn't happened
            Assert.That(startX, Is.EqualTo(newX));
            Assert.That(startY, Is.EqualTo(newY));
            Assert.That(startZ, Is.EqualTo(newZ));
        }
        [Test]
        public void TestEvilUpdateRedLight()
        {
            // TODO get the id from the welcome packets
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);
            //retrive GameObj from avatrId and store its position
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();

            //retrive GameObj from avatarId and check its previous position
            GameObject testerObj = server.GetGameObj(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            //get base position
            float startX = testerObj.X;
            float startY = testerObj.Y;
            float startZ = testerObj.Z;
            float offsetX = 10.0f;
            float offsetY = 20.0f;
            float offsetZ = 30.0f;
            Packet movePacket = new Packet(3, avatarId, offsetX, offsetY, offsetZ);
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();
            //get new position after the moviment
            float newX = testerObj.X;
            float newY = testerObj.Y;
            float newZ = testerObj.Z;
            //check position previus and after the movent; the valor should be the same because the movemnt shouldn't happened
            Assert.That(newX, Is.Not.EqualTo(startX + offsetX));
            Assert.That(newY, Is.Not.EqualTo(startY + offsetY));
            Assert.That(newZ, Is.Not.EqualTo(startZ + offsetZ));
        }

        //Test correct movement of an obj that isn't yours and malusfrom it's owner
        [Test]
        public void TestMoveUpdateGreenLight()
        {
            // TODO get the id from the welcome packets
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);
            //retrive GameObj from avatrId and store its position

            //retrive GameObj from avatarId and check its previous position
            GameObject testerObj = server.GetGameObj(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            //get base position
            float startX = testerObj.X;
            float startY = testerObj.Y;
            float startZ = testerObj.Z;
            float offsetX = 10.0f;
            float offsetY = 20.0f;
            float offsetZ = 30.0f;
            Packet movePacket = new Packet(3, avatarId, offsetX, offsetY, offsetZ);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            //get new position after the moviment
            float newX = testerObj.X;
            float newY = testerObj.Y;
            float newZ = testerObj.Z;
            float endX = startX + offsetX;
            //check position previus and after the movent; the valor should be the same because the movemnt shouldn't happened
            Assert.That(newX, Is.EqualTo(endX));
            //Assert.That(newX, Is.EqualTo(startX + offsetX));
            Assert.That(newY, Is.EqualTo(startY + offsetY));
            Assert.That(newZ, Is.EqualTo(startZ + offsetZ));
        }
        [Test]
        public void TestMoveUpdateRedLight()
        {
            // TODO get the id from the welcome packets
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);
            //retrive GameObj from avatrId and store its position
            //retrive GameObj from avatarId and check its previous position
            GameObject testerObj = server.GetGameObj(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            //get base position
            float startX = testerObj.X;
            float startY = testerObj.Y;
            float startZ = testerObj.Z;
            float offsetX = 10.0f;
            float offsetY = 20.0f;
            float offsetZ = 30.0f;
            Packet movePacket = new Packet(3, avatarId, offsetX, offsetY, offsetZ);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            //get new position after the moviment
            float newX = testerObj.X;
            float newY = testerObj.Y;
            float newZ = testerObj.Z;
            //check position previus and after the movent; the valor should be the same because the movemnt shouldn't happened
            Assert.That(newX, Is.Not.EqualTo(startX));
            Assert.That(newY, Is.Not.EqualTo(startY));
            Assert.That(newZ, Is.Not.EqualTo(startZ));
        }

        //Test malus
        [Test]
        public void TestMalusUpdateGreenLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);

            //retrive GameObj from avatarId and check its previous position
            Avatar testerObj = (Avatar)server.GetGameObj(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            //check GameClient malus on creation
            uint testerMalus = testerObj.Malus;
            Assert.That(testerMalus, Is.EqualTo(0));
            //check GameClient malus on creation
            uint malusValue = 3;
            testerObj.IncreaseMalus(malusValue);
            uint newTesterMalus = testerObj.Malus;
            Assert.That(newTesterMalus, Is.EqualTo(testerMalus + malusValue));
        }
        [Test]
        public void TestMalusUpdateRedLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);

            //retrive GameObj from avatarId and check its previous position
            Avatar testerObj = (Avatar)server.GetGameObj(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            //check GameClient malus on creation
            uint testerMalus = testerObj.Malus;
            Assert.That(testerMalus, Is.EqualTo(0));
            //check GameClient malus on creation
            uint malusValue = 3;
            testerObj.IncreaseMalus(malusValue);
            uint newTesterMalus = testerObj.Malus;
            Assert.That(newTesterMalus, Is.Not.EqualTo(testerMalus));
        }

        //Test malus increase after a bad behaviour
        [Test]
        public void TestEvilMalusUpdateGreenLight()
        {
            // TODO get the id from the welcome packets
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);
            //retrive GameObj from avatrId and store its position
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();

            //retrive GameObj from avatarId and check its previous position
            Avatar testerObj = (Avatar)server.GetGameObj(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            //check GameClient malus on creation
            uint testerMalus = testerObj.Malus;
            Assert.That(testerMalus, Is.EqualTo(0));
            //get base position
            float startX = testerObj.X;
            float startY = testerObj.Y;
            float startZ = testerObj.Z;
            float offsetX = 10.0f;
            float offsetY = 20.0f;
            float offsetZ = 30.0f;
            Packet movePacket = new Packet(3, avatarId, offsetX, offsetY, offsetZ);
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();
            //get new position after the moviment
            float newX = testerObj.X;
            float newY = testerObj.Y;
            float newZ = testerObj.Z;
            //check position previus and after the movent; the valor should be the same because the movemnt shouldn't happened
            Assert.That(startX, Is.EqualTo(newX));
            Assert.That(startY, Is.EqualTo(newY));
            Assert.That(startZ, Is.EqualTo(newZ));
            //check GameClient malus after a bad behaviour
            uint newTesterMalus = testerObj.Malus;
            Assert.That(newTesterMalus, Is.EqualTo(testerMalus + 1));
        }
        [Test]
        public void TestEvilMalusUpdateRedLight()
        {
            // TODO get the id from the welcome packets
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);
            //retrive GameObj from avatrId and store its position
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();

            //retrive GameObj from avatarId and check its previous position
            Avatar testerObj = (Avatar)server.GetGameObj(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            //check GameClient malus on creation
            uint testerMalus = testerObj.Malus;
            Assert.That(testerMalus, Is.EqualTo(0));
            //get base position
            float startX = testerObj.X;
            float startY = testerObj.Y;
            float startZ = testerObj.Z;
            float offsetX = 10.0f;
            float offsetY = 20.0f;
            float offsetZ = 30.0f;
            Packet movePacket = new Packet(3, avatarId, offsetX, offsetY, offsetZ);
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();
            //get new position after the moviment
            float newX = testerObj.X;
            float newY = testerObj.Y;
            float newZ = testerObj.Z;
            //check position previus and after the movent; the valor should be the same because the movemnt shouldn't happened
            Assert.That(newX, Is.Not.EqualTo(startX + offsetX));
            Assert.That(newY, Is.Not.EqualTo(startY + offsetY));
            Assert.That(newZ, Is.Not.EqualTo(startZ + offsetZ));
            //check GameClient malus after a bad behaviour
            uint newTesterMalus = testerObj.Malus;
            Assert.That(newTesterMalus, Is.Not.EqualTo(testerMalus));
        }

        //Test malus reduction
        [Test]
        public void TestMalusReductionUpdateGreenLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);

            //retrive GameObj from avatarId and check its previous position
            Avatar testerObj = (Avatar)server.GetGameObj(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            //check GameClient malus on creation
            uint testerMalus = testerObj.Malus;
            Assert.That(testerMalus, Is.EqualTo(0));
            //check GameClient malus on creation
            uint malusValue = 3;
            testerObj.IncreaseMalus(malusValue);
            uint testerMalusAfterBadBehaviour = testerObj.Malus;
            Assert.That(testerMalusAfterBadBehaviour, Is.EqualTo(testerMalus + malusValue));
            clock.IncreaseTimeStamp(400f);
            //server.SingleStep();
            server.CheckMalus();
            uint newMalus = testerObj.Malus;
            Assert.That(newMalus, Is.EqualTo(testerMalusAfterBadBehaviour - 1));
            clock.IncreaseTimeStamp(30f);
            Assert.That(testerObj.Malus, Is.EqualTo(newMalus));
        }
        [Test]
        public void TestMalusReductionUpdateRedLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);

            //retrive GameObj from avatarId and check its previous position
            Avatar testerObj = (Avatar)server.GetGameObj(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            //check GameClient malus on creation
            uint testerMalus = testerObj.Malus;
            Assert.That(testerMalus, Is.EqualTo(0));
            //check GameClient malus on creation
            uint malusValue = 3;
            testerObj.IncreaseMalus(malusValue);
            uint testerMalusAfterBadBehaviour = testerObj.Malus;
            Assert.That(testerMalusAfterBadBehaviour, Is.EqualTo(testerMalus + malusValue));
            clock.IncreaseTimeStamp(400f);
            //server.SingleStep();
            server.CheckMalus();
            uint newMalus = testerObj.Malus;
            Assert.That(newMalus, Is.Not.EqualTo(testerMalusAfterBadBehaviour));
            clock.IncreaseTimeStamp(30f);
            Assert.That(testerObj.Malus, Is.EqualTo(newMalus));
        }

    }
}

/*  IDEE TEST
 *  -Check Move
 * 
 */