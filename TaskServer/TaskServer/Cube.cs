using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskServer
{
    public class Cube : GameObject
    {
        public Cube(GameClient owner) : base(2, owner)
        {
        }
        public Cube(GameServer server) : base(2, server)
        {
        }
    }
}