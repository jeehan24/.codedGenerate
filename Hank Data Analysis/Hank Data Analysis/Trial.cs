using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hank_Data_Analysis
{
    class Trial
    {
        public int StartTrialIndex { get; set; }
        public int EndTrialIndex { get; set; }
        public float EnterRoadwayTime { get; set; }
        public float XatEnterRoadway { get; set; }
        public float ZatEnterRoadway { get; set; }
        public float LeadCarXatEnterRoadway { get; set; }
        public float TailCarXatEnterRoadway { get; set; }
        public float ClearCarTime { get; set; }
        public float XatClearCar { get; set; }
        public float ZatClearCar { get; set; }
        public float LeadCarXatClear { get; set; }
        public float TailCarXatClear { get; set; }
        public List<float> Gaps { get; set; }
        public int StartNextTrial { get; set; }
        public List<float> Alerts_generated { get; set; }
       // public List<string> AlertMessages_sent { get; set; }
        public List<string> clearAlertMsg_sent { get; set; }
        public List<string> prohibAlertMsg_sent { get; set; }

        public Trial()
        {
            Gaps = new List<float>();
            Alerts_generated = new List<float>();
            //AlertMessages_sent = new List<string>();
            clearAlertMsg_sent = new List<string>();
            prohibAlertMsg_sent = new List<string>();
        }
    }
}
