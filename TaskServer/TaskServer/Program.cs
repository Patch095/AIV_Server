using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskServer
{
    class Program
    {
        static void Main(string[] args)
        {
            GameTransportIPv4 transport = new GameTransportIPv4();
            transport.Bind("192.168.10.1", 9999);

            GameServer server = new GameServer(transport, null);

            Cube cube001 = new Cube(server);
            cube001.SetPosition(0, 0, 5);

            Cube cube002 = new Cube(server);
            cube002.SetPosition(0, 3, 0);

            Cube cube003 = new Cube(server);
            cube003.SetPosition(8, 0, 0);

            server.Start();
        }
    }
}