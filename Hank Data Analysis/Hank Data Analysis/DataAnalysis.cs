using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Hank_Data_Analysis
{
    class DataAnalysis
    {
        private WritingObject wo;

        private List<Frame> frames;
        private readonly int CAR_ID_STARTED_FROM=1000;
        //private readonly float CAR_SPEED = 11.176f;
        // private readonly float CAR_SPEED = 15.6464f;
        private readonly float CAR_SWATH = 2.44f;
        private bool _isTwoPerson = false;

        public DataAnalysis(string inputFile, List<DroppedTime> DroppedList, bool isTwoPerson,bool GazeContent)
        {
            //-------------Making a backup from original file before using Gaze predictor----------------
            File.Copy(inputFile, inputFile + ".org",true);
            if (GazeContent)
            System.Diagnostics.Process.Start(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\ML\\GazePrediction.exe",
                inputFile);
            //------------------------------------------------------------------------------------------- 
                frames = new List<Frame>();
                wo = new WritingObject(inputFile);
                FileToFrames(inputFile, DroppedList);
            List<Trial> P0trials = new List<Trial>();
            List<Trial> P1trials = new List<Trial>();
            _isTwoPerson = isTwoPerson;
            int TrialCounter = 0;
            int startindex = 0;
            List<Frame> snapshot = new List<Frame>();
            for (int i = 0; i < frames.Count; i++)
            {
                snapshot = Spanshot(i);
                for (int j = 0; j < snapshot.Count; j++)
                {
                    if (snapshot[j].ID >= 1000)
                    {
                        startindex = i;
                        break;
                    }
                }
                if (startindex > 0)
                    break;
            }

            /*
             * Finding Trials 
             * We start from time zero and moving forward until pedestrian corssed mid point of the roadway.
             * Then we continue moving forward until the tail car passes the center of the cave.
             * The entire set of frames areconsidered as a trial.
             * */

            if (!_isTwoPerson)
            {
                do
                {
                    Trial t;
                    if (P0trials.Count == 0)
                        t = FindTrial(0, startindex);
                    else
                    {
                        t = FindTrial(0, startindex, P0trials[P0trials.Count - 1].EndTrialIndex);
                    }
                    if (t.EnterRoadwayTime != 0f)
                        P0trials.Add(t);

                    if (((t.ClearCarTime == float.MaxValue) && (t.EnterRoadwayTime == float.MaxValue)) || (t.XatEnterRoadway == 0))
                        break;
                    startindex = t.StartNextTrial;
                    TrialCounter++;

                } while (true);
                WriteToFile(P0trials);
            }
            else
            {
                TrialCounter = 0;
                startindex = 0;
                do
                {
                    Trial t;
                    if (P0trials.Count == 0)
                        t = FindTrial(0, startindex);
                    else
                    {
                        t = FindTrial(0, startindex, P0trials[P0trials.Count - 1].EndTrialIndex);
                    }
                    if (t.EnterRoadwayTime!=0f)
                        P0trials.Add(t);

                    if (((t.ClearCarTime == float.MaxValue) && (t.EnterRoadwayTime == float.MaxValue)) || (t.XatEnterRoadway == 0))
                        break;
                    startindex = t.StartNextTrial;
                    TrialCounter++;

                } while (true);

                TrialCounter = 0;
                startindex = 0;
                do
                {
                    Trial t;
                    if (P1trials.Count == 0)
                        t = FindTrial(1, startindex);
                    else
                    {
                        t = FindTrial(1, startindex, P1trials[P0trials.Count - 1].EndTrialIndex);
                    }
                    if (t.EnterRoadwayTime != 0f)
                        P1trials.Add(t);

                    if (((t.ClearCarTime == float.MaxValue) && (t.EnterRoadwayTime == float.MaxValue)) || (t.XatEnterRoadway == 0))
                        break;
                    startindex = t.StartNextTrial;
                    TrialCounter++;

                } while (true);
                WriteToFile(P0trials, P1trials);
            }
            
        }
        private void WriteToFile(List<Trial> trials)
        {
            for (int i = 0; i < trials.Count; i++)
            {
                wo.Write("== TRIAL " + i + " ==");
                wo.Write("");
                wo.Write("PedEnterRoadwayTime:        " + trials[i].EnterRoadwayTime + "");
                wo.Write("PedXatEnterRoadwayTime:     " + trials[i].XatEnterRoadway + "");
                wo.Write("PedZatEnterRoadwayTime:     " + trials[i].ZatEnterRoadway + "");
                wo.Write("LeadCarXatEnterRoadwayTime: " + trials[i].LeadCarXatEnterRoadway + "");
                wo.Write("TailCarXatEnterRoadwayTime: " + trials[i].TailCarXatEnterRoadway + "");
                wo.Write("");
                wo.Write("PedClearCarTime:            " + trials[i].ClearCarTime + "");
                wo.Write("PedXatClearCarTime:         " + trials[i].XatClearCar + "");
                wo.Write("PedZatClearCarTime:         " + trials[i].ZatClearCar + "");
                wo.Write("LeadCarXatClearCarTime:     " + trials[i].LeadCarXatClear + "");
                wo.Write("TailCarXatClearCarTime:     " + trials[i].TailCarXatClear + "");
                wo.Write("");
                wo.Write("Gaps Seen:");

                string str = "";
                for (int j = 0; j < trials[i].Gaps.Count; j++)
                    str += trials[i].Gaps[j] + ", ";
                if (str.Length > 2)
                    wo.Write(str.Substring(0, str.Length - 2));

                wo.Write("");
          
                wo.Write("Prohibitive Alerts Generated 0:");
                string str_alertSeen = "";

                for(int k = 0; k< trials[i].Alerts_generated.Count; k++)
                {
                    str_alertSeen += trials[i].Alerts_generated[k] + ", ";
                }
                wo.Write(str_alertSeen);


                wo.Write("Prohibitive Alert Messages Sent to Phone 0:");

                string str_prohibAlertMsgSent = "";
                for (int l = 0; l < trials[i].prohibAlertMsg_sent.Count; l++)
                {
                    str_prohibAlertMsgSent += trials[i].prohibAlertMsg_sent[l] + ", ";
                }
                wo.Write(str_prohibAlertMsgSent);

                wo.Write("ClearAlert Messages Sent to Phone 0:");

                string str_clearAlertMsg = "";
                for( int m = 0; m < trials[i].clearAlertMsg_sent.Count; m++)
                {
                    str_clearAlertMsg += trials[i].clearAlertMsg_sent[m] + ", ";
                }
                wo.Write(str_clearAlertMsg);
                wo.Write("");

            }
            wo.close();
        }
        private void WriteToFile(List<Trial> trials1, List<Trial> trials2)
        {
            for (int i = 0; i < trials1.Count; i++)
            {
                wo.Write("== TRIAL " + i + " ==");
                wo.Write("");
                wo.Write("PedEnterRoadwayTime 0:        " + trials1[i].EnterRoadwayTime + "");
                wo.Write("PedXatEnterRoadwayTime 0:     " + trials1[i].XatEnterRoadway + "");
                wo.Write("PedZatEnterRoadwayTime 0:     " + trials1[i].ZatEnterRoadway + "");
                wo.Write("LeadCarXatEnterRoadwayTime 0: " + trials1[i].LeadCarXatEnterRoadway + "");
                wo.Write("TailCarXatEnterRoadwayTime 0: " + trials1[i].TailCarXatEnterRoadway + "");
                wo.Write("");
                wo.Write("PedClearCarTime 0:            " + trials1[i].ClearCarTime + "");
                wo.Write("PedXatClearCarTime 0:         " + trials1[i].XatClearCar + "");
                wo.Write("PedZatClearCarTime 0:         " + trials1[i].ZatClearCar + "");
                wo.Write("LeadCarXatClearCarTime 0:     " + trials1[i].LeadCarXatClear + "");
                wo.Write("TailCarXatClearCarTime 0:     " + trials1[i].TailCarXatClear + "");
                wo.Write("");
                wo.Write("Gaps Seen 0:");
                string str = "";
               
                for (int j = 0; j < trials1[i].Gaps.Count; j++)
                    str += trials1[i].Gaps[j] + ", ";
                if (str.Length > 2)
                    wo.Write(str.Substring(0, str.Length - 2));


                wo.Write("");
                wo.Write("PedEnterRoadwayTime 1:        " + trials2[i].EnterRoadwayTime + "");
                wo.Write("PedXatEnterRoadwayTime 1:     " + trials2[i].XatEnterRoadway + "");
                wo.Write("PedZatEnterRoadwayTime 1:     " + trials2[i].ZatEnterRoadway + "");
                wo.Write("LeadCarXatEnterRoadwayTime 1: " + trials2[i].LeadCarXatEnterRoadway + "");
                wo.Write("TailCarXatEnterRoadwayTime 1: " + trials2[i].TailCarXatEnterRoadway + "");
                wo.Write("");
                wo.Write("PedClearCarTime 1:            " + trials2[i].ClearCarTime + "");
                wo.Write("PedXatClearCarTime 1:         " + trials2[i].XatClearCar + "");
                wo.Write("PedZatClearCarTime 1:         " + trials2[i].ZatClearCar + "");
                wo.Write("LeadCarXatClearCarTime 1:     " + trials2[i].LeadCarXatClear + "");
                wo.Write("TailCarXatClearCarTime 1:     " + trials2[i].TailCarXatClear + "");
                wo.Write("");
                wo.Write("Gaps Seen 1:");
                str = "";
                for (int j = 0; j < trials2[i].Gaps.Count; j++)
                    str += trials2[i].Gaps[j] + ", ";
                if (str.Length > 2)
                    wo.Write(str.Substring(0, str.Length - 2));

                
               

            }
            wo.close();
        }
        private void FileToFrames(string path, List<DroppedTime> DroppedList)
        {
            string line;
            StreamReader sr = new StreamReader(path);
            bool isDropped = false;
            while ((line = sr.ReadLine()) != null)
            {
                Frame f = new Frame(line);
                isDropped = false;
                for (int i = 0; i < DroppedList.Count; i++)
                {
                    if ((f.Time >= DroppedList[i].startTime) && (f.Time <= DroppedList[i].endTime))
                    { 
                        isDropped = true;
                        break;
                    }
                }
                if (!isDropped)
                {
                    if (f.Type == FrameType.Description)
                        wo.Write(f.Description);
                    else
                        frames.Add(f);
                }
            }
            sr.Close();
        }
        private Trial FindTrial(int PedestrianID,int startIndex)
        {
            Trial trial = new Trial();
            trial.StartTrialIndex = startIndex;
            // 1) Finding the index of crossing mid point of roadway
            // We assume the participant is in the negative side
            int indexOfMidPoint = int.MaxValue;
            List<Frame> snapshot;
            float LeadCarPos = float.MaxValue;
            float tailCarPos = float.MinValue;
            int index = 0;
            Frame leadCarFrame = new Frame("0 0 0 Dummy");
            Frame tailCarFrame = new Frame("0 0 0 Dummy");

            for (int i = startIndex; i < frames.Count; i++)
            {
                if ((PedestrianID == frames[i].ID) && (frames[i].Type == FrameType.Moving) && (frames[i].Position.z > 0))
                {
                    indexOfMidPoint = i;
                    break;
                }
  
            }

            if (indexOfMidPoint == int.MaxValue)
            {
                Trial t = new Trial();
                t.ClearCarTime = float.MaxValue;
                t.EnterRoadwayTime = float.MaxValue;
                return trial;
            }
               
            // 2) Find the Entering roadway time/pos
            // One lane roadway width is 3.048 -> curb would be on CAR SWATH/-2 in Z direction
            // Moving backward from mid-point index to find the entry-time
            for (int i = indexOfMidPoint; i >=0; i--)
            {
                //||((frames.Count-1)<i)
                if ((i < 0))
                    break;
                if ((PedestrianID == frames[i].ID) && (frames[i].Position.z < (CAR_SWATH/-2.0f)) && (frames[i].Type == FrameType.Moving))
                {
                    trial.EnterRoadwayTime = frames[i].Time;
                    trial.XatEnterRoadway = frames[i].Position.x;
                    trial.ZatEnterRoadway = frames[i].Position.z;
                    index = i;
                    break;
                }
            }
            snapshot = Spanshot(indexOfMidPoint);
            for (int i=0; i<snapshot.Count;i++)
            {
                if ((snapshot[i].Type == FrameType.Moving) && (snapshot[i].ID >= CAR_ID_STARTED_FROM))
                {
                    if ((snapshot[i].Position.x<=0)&&(snapshot[i].Position.x>tailCarPos))
                    {
                        tailCarFrame = snapshot[i];
                        tailCarPos = snapshot[i].Position.x;
                    }

                    if ((snapshot[i].Position.x > 0) && (snapshot[i].Position.x < LeadCarPos))
                    {
                        leadCarFrame = snapshot[i];
                        LeadCarPos = snapshot[i].Position.x;
                    }
                }
            }
            if (trial.XatEnterRoadway != 0)
            {
                snapshot = Spanshot(index);
                for (int i = 0; i < snapshot.Count; i++)
                {
                    if ((snapshot[i].ID == leadCarFrame.ID)&&(leadCarFrame.ID>1000))
                        trial.LeadCarXatEnterRoadway = snapshot[i].Position.x - (CarLength(leadCarFrame.ID) / 2);
                    if ((snapshot[i].ID==tailCarFrame.ID)&&(tailCarFrame.ID>1000))
                        trial.TailCarXatEnterRoadway = snapshot[i].Position.x + (CarLength(tailCarFrame.ID) / 2);
                }
            }
            else
                return trial;


            // 3) Find the Clearing roadway time/pos
            // One lane roadway width is 3.048 -> curb would be on CAR_SWATH/2 in Z direction
            // Moving forward from mid-point index
            for (int i = indexOfMidPoint; i < frames.Count; i++)
            {
                if ((PedestrianID == frames[i].ID) && (frames[i].Position.z > (CAR_SWATH/2.0f)) && (frames[i].Type == FrameType.Moving))
                {
                    trial.ClearCarTime = frames[i].Time;
                    trial.XatClearCar = frames[i].Position.x;
                    trial.ZatClearCar = frames[i].Position.z;
                    index = i;
                    break;
                }
            }

            if (trial.XatEnterRoadway != 0)
            {
                snapshot = Spanshot(index);
                for (int i = 0; i < snapshot.Count; i++)
                {
                    if ((snapshot[i].ID == leadCarFrame.ID)&& (leadCarFrame.ID > 1000))
                        trial.LeadCarXatClear = snapshot[i].Position.x - (CarLength(leadCarFrame.ID) / 2);
                    if ((snapshot[i].ID == tailCarFrame.ID)&& (tailCarFrame.ID > 1000))
                        trial.TailCarXatClear = snapshot[i].Position.x + (CarLength(tailCarFrame.ID) / 2);
                }
            }
            else
                return trial;

            // 4)find the trials frame array
            //move forward until tail car passed mid-point+10 meters
            for (int i = indexOfMidPoint; i < frames.Count; i++)
                if ((frames[i].ID == tailCarFrame.ID) && (frames[i].Position.x > 10))
                {
                    trial.EndTrialIndex = i;
                    break;
                }
            bool CameBackP0 = false;
            bool CameBackP1 = false;
            for (int i = trial.EndTrialIndex; i < frames.Count; i++)
            {
                CameBackP0 = false;
                CameBackP1 = false;
                snapshot = Spanshot(i);
                for (int j=0; j<snapshot.Count;j++)
                {
                    if (_isTwoPerson)
                    {
                        if ((snapshot[j].ID == 0) && (snapshot[j].Position.z < 0) && (snapshot[j].Type == FrameType.Moving))
                            CameBackP0 = true;
                        else
                            if ((snapshot[j].ID == 0) && (snapshot[j].Position.z >= 0) && (snapshot[j].Type == FrameType.Moving))
                            CameBackP0 = false;
                        if ((snapshot[j].ID == 1) && (snapshot[j].Position.z < 0) && (snapshot[j].Type == FrameType.Moving))
                            CameBackP1 = true;
                        else
                            if ((snapshot[j].ID == 1) && (snapshot[j].Position.z >= 0) && (snapshot[j].Type == FrameType.Moving))
                            CameBackP1 = false;
                    }
                    else
                    {
                        if ((snapshot[j].ID == 0) && (snapshot[j].Position.z < 0) && (snapshot[j].Type == FrameType.Moving))
                            CameBackP0 = true;
                    }
                }

                if (CameBackP0 && CameBackP1 && _isTwoPerson)
                {
                    trial.StartNextTrial = i;
                    break;
                }
                if (CameBackP0 && PedestrianID == 0 && !_isTwoPerson)
                {
                        trial.StartNextTrial = i;
                        break;
                }

            }
                
            // 5) Find gaps
            ///6) Find Alert Dataa
            int tailCarFrameIndex = -1;
            int leadCarFrameIndex = -1;
            /*
            for (int i = trial.StartTrialIndex; i >0; i--)
                if ((frames[i].ID >= CAR_ID_STARTED_FROM) && (frames[i].Type== FrameType.Creation))
                {
                    trial.StartTrialIndex = i;
                    break;
                }
                */
            for (int i = trial.StartTrialIndex; i < trial.EndTrialIndex; i++)
            { 
                if (((leadCarFrameIndex == -1)||(tailCarFrameIndex == -1))
                    && (frames[i].ID >= 1000) && (frames[i].Type == FrameType.Creation))
                {
                    if (leadCarFrameIndex == -1)
                    {
                        leadCarFrame = frames[i];
                        leadCarFrameIndex = i;
                    }
                        
                    else
                    {
                        tailCarFrame = frames[i];
                        tailCarFrameIndex = i;
                    }
                        
                }
                if ((leadCarFrameIndex != -1) && (tailCarFrameIndex != -1))
                {
                    float gap = GapCalculator(leadCarFrameIndex, trial.EndTrialIndex, leadCarFrameIndex, tailCarFrameIndex);
                    if (gap != -1)
                    {
                        trial.Gaps.Add(gap);
                        leadCarFrameIndex = tailCarFrameIndex;
                        tailCarFrameIndex = -1;
                        i = leadCarFrameIndex;
                    }
                    else
                        break;
                }
                if (frames[i].Type == FrameType.AlertMessSent)
                {
                    if( frames[i].timeClearAlertMsgSent == "")
                    {
                        //This is a prohibitive alert message
                        trial.prohibAlertMsg_sent.Add(frames[i].timeProhibMsgSent);

                    } else if(frames[i].timeProhibMsgSent == "")
                    {
                        //This is a clearAlert message
                        trial.clearAlertMsg_sent.Add(frames[i].timeClearAlertMsgSent);
                    }
                    //trial.AlertMessages_sent.Add(frames[i].timeAlertMessSent);
                }
                if (frames[i].Type == FrameType.AlertSeen)
                {
                    trial.Alerts_generated.Add(frames[i].timeAlertGenerated);
                }

            }
                return trial;
        }
        private Trial FindTrial(int PedestrianID, int startIndex, int endIndex)
        {
            Trial trial = new Trial();
            trial.StartTrialIndex = startIndex;
            // 1) Finding the index of crossing mid point of roadway
            // We assume the participant is in the negative side
            int indexOfMidPoint = int.MaxValue;
            List<Frame> snapshot;
            float LeadCarPos = float.MaxValue;
            float tailCarPos = float.MinValue;
            int index = 0;
            Frame leadCarFrame = new Frame("0 0 0 Dummy");
            Frame tailCarFrame = new Frame("0 0 0 Dummy");

            for (int i = startIndex; i < frames.Count; i++)
            {
                if ((PedestrianID == frames[i].ID) && (frames[i].Position.z > 0) && (frames[i].Type == FrameType.Moving))
                {
                    indexOfMidPoint = i;
                    break;
                }
            }

            if (indexOfMidPoint == int.MaxValue)
            {
                Trial t = new Trial();
                t.ClearCarTime = float.MaxValue;
                t.EnterRoadwayTime = float.MaxValue;
                return trial;
            }

            // 2) Find the Entering roadway time/pos
            // One lane roadway width is 3.048 -> curb would be on CAR SWATH/-2 in Z direction
            // Moving backward from mid-point index to find the entry-time
            for (int i = indexOfMidPoint; i >= 0; i--)
            {
                //||((frames.Count-1)<i)
                if ((i < 0)||(i<endIndex))
                    break;
                if ((PedestrianID == frames[i].ID) && (frames[i].Position.z < (CAR_SWATH / -2.0f)) && (frames[i].Type == FrameType.Moving))
                {
                    trial.EnterRoadwayTime = frames[i].Time;
                    trial.XatEnterRoadway = frames[i].Position.x;
                    trial.ZatEnterRoadway = frames[i].Position.z;
                    index = i;
                    break;
                }
            }
            snapshot = Spanshot(indexOfMidPoint);
            for (int i = 0; i < snapshot.Count; i++)
            {
                if ((snapshot[i].Type == FrameType.Moving) && (snapshot[i].ID >= CAR_ID_STARTED_FROM))
                {
                    if ((snapshot[i].Position.x <= 0) && (snapshot[i].Position.x > tailCarPos))
                    {
                        tailCarFrame = snapshot[i];
                        tailCarPos = snapshot[i].Position.x;
                    }

                    if ((snapshot[i].Position.x > 0) && (snapshot[i].Position.x < LeadCarPos))
                    {
                        leadCarFrame = snapshot[i];
                        LeadCarPos = snapshot[i].Position.x;
                    }
                }
            }
            if (trial.XatEnterRoadway != 0)
            {
                snapshot = Spanshot(index);
                for (int i = 0; i < snapshot.Count; i++)
                {
                    if ((snapshot[i].ID == leadCarFrame.ID) && (leadCarFrame.ID > 1000))
                        trial.LeadCarXatEnterRoadway = snapshot[i].Position.x - (CarLength(leadCarFrame.ID) / 2);
                    if ((snapshot[i].ID == tailCarFrame.ID) && (tailCarFrame.ID > 1000))
                        trial.TailCarXatEnterRoadway = snapshot[i].Position.x + (CarLength(tailCarFrame.ID) / 2);
                }
            }
            else
                return trial;


            // 3) Find the Clearing roadway time/pos
            // One lane roadway width is 3.048 -> curb would be on CAR_SWATH/2 in Z direction
            // Moving forward from mid-point index
            for (int i = indexOfMidPoint; i < frames.Count; i++)
            {
                if ((PedestrianID == frames[i].ID) && (frames[i].Position.z > (CAR_SWATH / 2.0f)) && (frames[i].Type == FrameType.Moving))
                {
                    trial.ClearCarTime = frames[i].Time;
                    trial.XatClearCar = frames[i].Position.x;
                    trial.ZatClearCar = frames[i].Position.z;
                    index = i;
                    break;
                }
            }

            if (trial.XatEnterRoadway != 0)
            {
                snapshot = Spanshot(index);
                for (int i = 0; i < snapshot.Count; i++)
                {
                    if ((snapshot[i].ID == leadCarFrame.ID) && (leadCarFrame.ID > 1000))
                        trial.LeadCarXatClear = snapshot[i].Position.x - (CarLength(leadCarFrame.ID) / 2);
                    if ((snapshot[i].ID == tailCarFrame.ID) && (tailCarFrame.ID > 1000))
                        trial.TailCarXatClear = snapshot[i].Position.x + (CarLength(tailCarFrame.ID) / 2);
                }
            }
            else
                return trial;

            // 4)find the trials frame array
            //move forward until tail car passed mid-point+10 meters
            for (int i = indexOfMidPoint; i < frames.Count; i++)
                if ((frames[i].ID == tailCarFrame.ID) && (frames[i].Position.x > 10))
                {
                    trial.EndTrialIndex = i;
                    break;
                }
            bool CameBackP0 = false;
            bool CameBackP1 = false;
            for (int i = trial.EndTrialIndex; i < frames.Count; i++)
            {
                CameBackP0 = false;
                CameBackP1 = false;
                snapshot = Spanshot(i);
                for (int j = 0; j < snapshot.Count; j++)
                {
                    if (_isTwoPerson)
                    {
                        if ((snapshot[j].ID == 0) && (snapshot[j].Position.z < 0) && (snapshot[j].Type == FrameType.Moving))
                            CameBackP0 = true;
                        else
                            if ((snapshot[j].ID == 0) && (snapshot[j].Position.z >= 0) && (snapshot[j].Type == FrameType.Moving))
                            CameBackP0 = false;
                        if ((snapshot[j].ID == 1) && (snapshot[j].Position.z < 0) && (snapshot[j].Type == FrameType.Moving))
                            CameBackP1 = true;
                        else
                            if ((snapshot[j].ID == 1) && (snapshot[j].Position.z >= 0) && (snapshot[j].Type == FrameType.Moving))
                            CameBackP1 = false;
                    }
                    else
                    {
                        if ((snapshot[j].ID == 0) && (snapshot[j].Position.z < 0) && (snapshot[j].Type == FrameType.Moving))
                            CameBackP0 = true;
                    }
                }

                if (CameBackP0 && CameBackP1 && _isTwoPerson)
                {
                    trial.StartNextTrial = i;
                    break;
                }
                if (CameBackP0 && PedestrianID == 0 && !_isTwoPerson)
                {
                    trial.StartNextTrial = i;
                    break;
                }

            }

            // 5) Find gaps
            ///6) Find Alerts Seen and Alert Messages Sent
            int tailCarFrameIndex = -1;
            int leadCarFrameIndex = -1;
            /*
            for (int i = trial.StartTrialIndex; i >0; i--)
                if ((frames[i].ID >= CAR_ID_STARTED_FROM) && (frames[i].Type== FrameType.Creation))
                {
                    trial.StartTrialIndex = i;
                    break;
                }
                */
            for (int i = trial.StartTrialIndex; i < trial.EndTrialIndex; i++)
            {
                if (((leadCarFrameIndex == -1) || (tailCarFrameIndex == -1))
                    && (frames[i].ID >= 1000) && (frames[i].Type == FrameType.Creation))
                {
                    if (leadCarFrameIndex == -1)
                    {
                        leadCarFrame = frames[i];
                        leadCarFrameIndex = i;
                    }

                    else
                    {
                        tailCarFrame = frames[i];
                        tailCarFrameIndex = i;
                    }

                }
                if ((leadCarFrameIndex != -1) && (tailCarFrameIndex != -1))
                {
                    float gap = GapCalculator(leadCarFrameIndex, trial.EndTrialIndex, leadCarFrameIndex, tailCarFrameIndex);
                    if (gap != -1)
                    {
                        trial.Gaps.Add(gap);
                        leadCarFrameIndex = tailCarFrameIndex;
                        tailCarFrameIndex = -1;
                        i = leadCarFrameIndex;
                    }
                    else
                        break;
                }
                ////FOR THE ALERTS
                if(frames[i].Type == FrameType.AlertMessSent)
                {
                    if (frames[i].timeClearAlertMsgSent == "")
                    {
                        //This is a prohibitive alert message
                        trial.prohibAlertMsg_sent.Add(frames[i].timeProhibMsgSent);

                    }
                    else if (frames[i].timeProhibMsgSent == "")
                    {
                        //This is a clearAlert message
                        trial.clearAlertMsg_sent.Add(frames[i].timeClearAlertMsgSent);
                    }
                }
                if(frames[i].Type == FrameType.AlertSeen)
                {
                    trial.Alerts_generated.Add(frames[i].timeAlertGenerated);
                }
            }
            return trial;
        }
        private float GapCalculator(int startTrialIndex, int endTrialIndex, int leadCarFrameIndex, int tailCarFrameIndex)
        {
            Frame leadCarFrame = frames[leadCarFrameIndex];
            Frame tailCarFrame = frames[tailCarFrameIndex];
            bool leadPassedMidPoint = false;
            bool tailPassedMidPoint = false;
            for (int i = startTrialIndex; i < endTrialIndex; i++)
            {
                //&&(leadCarFrame.Position.x<frames[i].Position.x)
                if ((leadCarFrame.ID == frames[i].ID) &&
                    (frames[i].Position.x - (CarLength(frames[i].ID) / 2) <= 0)
                    && (frames[i].Type == FrameType.Moving)
                    && (leadCarFrame.Position.x < frames[i].Position.x)
                    )
                {
                    leadCarFrame = frames[i];

                }
                else
                {
                    if ((frames[i].Type == FrameType.Moving)
                        && (leadCarFrame.ID == frames[i].ID))
                    {
                        if (frames[i].Position.x - (CarLength(frames[i].ID) / 2) > 0)
                        {
                            leadPassedMidPoint = true;
                            break;
                        }

                    }
                }
                 /*   
                else
                {
                    //&& (frames[i].Position.x > 0)
                    if ((frames[leadCarFrameIndex].ID == frames[i].ID)
                        
                        )
                        leadPassedMidPoint = true;
                }
                 */   
                    
            }



            for (int i = startTrialIndex; i < endTrialIndex; i++)
            {
                //&&(leadCarFrame.Position.x<frames[i].Position.x)
                if ((tailCarFrame.ID == frames[i].ID) &&
                    (frames[i].Position.x + (CarLength(frames[i].ID) / 2) <= 0)
                    && (frames[i].Type == FrameType.Moving)
                    && (tailCarFrame.Position.x < frames[i].Position.x)
                    )
                {
                    tailCarFrame = frames[i];

                }
                else
                {
                    if ((frames[i].Type == FrameType.Moving)
                        && (tailCarFrame.ID == frames[i].ID))
                    {
                        if (frames[i].Position.x - (CarLength(frames[i].ID) / 2) > 0)
                        {
                            tailPassedMidPoint = true;
                            break;
                        }

                    }
                }
            }
                if (tailPassedMidPoint && leadPassedMidPoint)
                return tailCarFrame.Time - leadCarFrame.Time;
            else
                return -1;
        }
        public List<Frame> Spanshot(int index)
        {
            List<Frame> array = new List<Frame>();
            int startIndex = index;
            int endIndex = index;
            for (int i = index; i < frames.Count; i++)
            {
                if (frames[index].Time == frames[i].Time)
                    endIndex = i;
                else
                    break;
            }
            for (int i = index; i >0; i--)
            {
                if (frames[index].Time == frames[i].Time)
                    startIndex = i;
                else
                    break;
            }
            for (int i = startIndex; i <= endIndex; i++)
                array.Add(frames[i]);
            return array;
        }
        private float CarLength(int ID)
        {
            /*
            string path = @"C:\Users\Pooya\Desktop\Visualizer\Visualizer\TextVsNontextData\CarLength.txt";
            StreamReader sr = new StreamReader(path);
            List<float> carLen = new List<float>();
            string line = "";
            while ((line = sr.ReadLine()) != null)
                carLen.Add(float.Parse(line.Trim()));
            */
            //if (ID < 1000)
            //    return 0;
            List<float> carLen = new List<float>() {4.892397f,4.726724f,4.534277f,4.663174f,4.653241f };
            return carLen[(ID / 1000)-1];
        }
    }
}
