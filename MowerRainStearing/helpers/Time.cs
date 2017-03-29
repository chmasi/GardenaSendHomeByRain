using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MowerRainSteering.helpers
{
    public class Time
    {
        public static bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
        {
            // convert datetime to a TimeSpan
            TimeSpan now = datetime.TimeOfDay;
            // see if start comes before end
            if (start < end)
                return start <= now && now <= end;
            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }
    }
}
