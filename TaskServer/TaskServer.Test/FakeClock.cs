using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TaskServer.Test
{
    public class FakeClock : IMonotonicClock
    {
        private float timeStamp;
        public float GetNow()
        {
            return timeStamp;
        }

        public FakeClock(float timeStamp = 0)
        {
            this.timeStamp = timeStamp;
        }

        public void ClockUpdate()
        {
        }

        public void IncreaseTimeStamp(float delta = 0)
        {
            if (delta <= 0)
            {
                throw new Exception("invalid delta value");
            }
            else
                timeStamp += delta;
        }
        public void SetFakeTime(float time)
        {
            if(time <= 0)
                timeStamp = time;
        }
    }
}
