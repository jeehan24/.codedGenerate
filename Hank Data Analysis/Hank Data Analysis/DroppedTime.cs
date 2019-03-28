using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hank_Data_Analysis
{
    class DroppedTime
    {
        public DroppedTime(float v1, float v2)
        {
            this.startTime = v1;
            this.endTime = v2;
        }

        public float startTime { get; set; }
        public float endTime { get; set; }
    }
}
