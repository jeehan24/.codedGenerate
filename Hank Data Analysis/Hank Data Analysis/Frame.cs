using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hank_Data_Analysis
{
//enum FrameType { Creation, Moving, Termination, Description, Message, Gaze };
    enum FrameType { Creation, Moving, Termination, Description, AlertSeen, AlertMessSent};
    class Frame
    {
        public int Tag { get; set; }
        public float Time { get; set; }
        public int ID {get; set;}
        public Vector3D Position { get; set;}
        public Vector3D Orientation {get; set;}
        public string Description {get; set;}
        public FrameType Type { get; set;}
        public float timeAlertGenerated { get; set;}
       // public string timeAlertMessSent { get; set; }
        public string timeClearAlertMsgSent { get; set; }
        public string timeProhibMsgSent { get; set; }
        //  public bool GazeContent { get; set; }

        public Frame(string str)
        {
            string[] strArr = str.Split(' ');
            Tag = int.Parse(strArr[0]);
            Time = float.Parse(strArr[1]);
            switch (Tag)
            {
                case 0:
                    Type = FrameType.Description;
                    break;
                case 1:
                    Type = FrameType.Creation;
                    break;
                case 2:
                    Type = FrameType.Moving;
                    break;
                case 3:
                    Type = FrameType.Termination;
                    break;
                case 4:
                    Type = FrameType.AlertSeen;
                    break;
                case 5:
                    Type = FrameType.AlertMessSent;
                    break;
            }
            switch (Tag)
            {
                case 0:
                    for (int i = 2; i < strArr.Length; i++)
                        Description += strArr[i] + " ";
                    Description.Trim();
                    break;
                case 1:
                case 2:
                case 3:
                    ID = int.Parse(strArr[2]);
                    float x = float.Parse(strArr[3]), y = float.Parse(strArr[4]), z = float.Parse(strArr[5]);
                    Position = new Vector3D(x, y, z);
                    x = float.Parse(strArr[6]); y = float.Parse(strArr[7]); z = float.Parse(strArr[8]);
                    Orientation = new Vector3D(x, y, z);
                    break;
                case 4:
                    Type = FrameType.AlertSeen;
                    Position = new Vector3D(0, 0, 0);
                    Orientation = new Vector3D(0, 0, 0);
                    //4 0 465.6786 Alert Generated with RTT 0.0070004
                    float rtt = float.Parse(strArr[7]);
                    float alert_generated = float.Parse(strArr[2]);
                    timeAlertGenerated = alert_generated - (rtt / 2);            
                    break;
                case 5:
                    Type = FrameType.AlertMessSent;
                    //GazeContent = true;                   
                    Position = new Vector3D(0, 0, 0);
                    Orientation = new Vector3D(0, 0, 0);
                    //5  0  86.418 CLEARALERTS; sent to phone
                    //5  0 86.39378 PROHIBITIVEALERT; sent to phone
                   // timeAlertMessSent = strArr[2];
                    if (strArr[3] == "CLEARALERTS;")
                    {
                        timeClearAlertMsgSent = strArr[2];
                        timeProhibMsgSent = "";
                    }
                    else if(strArr[3] == "PROHIBITIVEALERT;")
                    {
                        timeProhibMsgSent = strArr[2];
                        timeClearAlertMsgSent = "";
                    }
                    break;
            }

        }
    }
}
