using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TaskServer
{
    public class ServerClock : IMonotonicClock
    {
        private float currentClock;
        public float GetNow()
        {
            return currentClock;
        }

        public ServerClock()
        {
            currentClock = 0;
        }

        public void ClockUpdate()
        {
            currentClock = Stopwatch.GetTimestamp() / (float)Stopwatch.Frequency;
        }

        public void IncrementFakeTimer(float value = 0)
        {
        }

        public void SetFakeTime(float time)
        {
        }
    }
}