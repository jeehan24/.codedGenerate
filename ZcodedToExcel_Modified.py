#Python script that outputs coded pedestrian data to .csv file
#used for the new simulator data format
#edited by Yuan 10/15/2015
#updated by Yuan 10/26/2016

import sys
import os
import csv
from datetime import datetime
from Tkinter import * #GUI package
import tkMessageBox
import re #regular expression for input format testing
import time

class GUItk:
    def __init__(self, master):
        self.subID = StringVar()
        self.subID.set("NotStartedYet")
        self.totalF = StringVar()
        self.totalF.set("NotStartedYet")
        #top frame for the notices
        self.topframe = Frame (master)
        self.topframe.pack()

        #input notice widget
        self.inText=Text(self.topframe,height=15, width=70)
        self.scrollin = Scrollbar(self.topframe, command=self.inText.yview)
        self.inText.configure(yscrollcommand=self.scrollin.set)
        self.inText.tag_configure('big', font=('Verdana', 20, 'bold'))
        self.inText.tag_configure('color', foreground='#476042',font=('Tempus Sans ITC', 12, 'bold'))
        self.inText.insert(END,'Input Notice:\n', 'big')
        self.inNotice = """
1. Folder of this code has ONLY all the .coded files (single or pair) + this code

In every file:
2. Subject/pair id has to be a number
3. Gender has to be f, m, or F, M
4. Group set to TL, TR, C representing targetLeft/Right/Control. Original "pair group" were added to the end "Condition"
        """
        self.inText.insert(END, self.inNotice, 'color')
        self.inText.grid(row=0,column=0)
        self.inText.pack(side=LEFT)
        self.scrollin.pack(side=LEFT, fill=Y)
        #output notice widget
        self.outText=Text(self.topframe,height=15, width=70)
        self.scrollout = Scrollbar(self.topframe, command=self.outText.yview)
        self.outText.configure(yscrollcommand=self.scrollout.set)
        self.outText.tag_configure('big', font=('Verdana', 20, 'bold'))
        self.outText.tag_configure('color', foreground='#476042',font=('Tempus Sans ITC', 12, 'bold'))
        self.outText.insert(END,'Output Notice:\n', 'big')
        self.outNotice = """
Find 3 output files in the same folder as the input
1. completeinfo.csv: all the info as readable texts
2. codedinfo.csv: most variables coded (detail below)
3. byGapSeen.csv: every gap seen as a row
                  trials without a gap taken are deleted

Detailed Coding info:
--B Target: Target/Control->1 Partner->0
            Adding PairControl: Target/Parner (Pair)->1 Control->0
--D Gender: M->1 F->0
--E Condition: TL->0 TR->1 C->2
--F Trial: eg. Trial 2->2
--X StandingPos: Left->0 Right->1
--AH PickSameGap: Yes->1 No->0 '-'->2
--AK Role: Leader->1 Follower->0 '-'->2 Tie->3

        """
        self.outText.insert(END, self.outNotice, 'color')
        self.outText.pack(side=LEFT)
        self.scrollout.pack(side=RIGHT, fill=Y)


        #bottomframe for program process info and buttons.
        self.bottomframe= Frame(master)
        self.bottomframe.pack(side=BOTTOM)

        self.emptyrow=Label(self.bottomframe,text="")
        self.emptyrow.grid(row=0,column=0)
        self.startlabel=Label(self.bottomframe,text="InputStartTrialNum:")
        self.startlabel.grid(row=1,column=0)
        self.startentry=Entry(self.bottomframe)
        self.startentry.grid(row=1,column=1)
        self.endlabel=Label(self.bottomframe,text="InputEndTrialNum:")
        self.endlabel.grid(row=2,column=0)
        self.endentry=Entry(self.bottomframe)
        self.endentry.grid(row=2,column=1)
        self.runButton= Button(self.bottomframe, command = self.buttonPress, text="I know all the notice. Process data.", bg="light green", fg="purple")
        self.runButton.grid(row=3,column=1)

        #labels showing current processing file, totoal file processed
        self.emptycolumn=Label(self.bottomframe,text="               ")
        self.emptycolumn.grid(row=0,column=2)
        self.curFlabel=Label(self.bottomframe, text="Current processing SubjectID:")
        self.curFlabel.grid(row=1,column=3)
        self.curcnt=Label(self.bottomframe, textvariable=self.subID,bg="lightyellow",fg="black")
        self.curcnt.grid(row=1,column=4)
        self.tolcnt=Label(self.bottomframe, textvariable=self.totalF,bg="lightyellow",fg="black")
        self.tolcnt.grid(row=2,column=4)
        self.tolFlabel=Label(self.bottomframe,text="Total No. of files processed:")
        self.tolFlabel.grid(row=2,column=3)
        self.detaillabel=Label(self.bottomframe,text="Please see the other black exe window for details.",fg="red")
        self.detaillabel.grid(row=3,column=3)
        self.emptyrow1=Label(self.bottomframe,text="")
        self.emptyrow1.grid(row=4,column=0)
    
    def buttonPress(self):
        if 'dataP' in globals():
            dataP.processData()

    def getEntry(self): #get user inputed start and end trial number
        return self.startentry.get(),self.endentry.get()

    def errorMsg(self,Msg):
        tkMessageBox.showinfo("warning", Msg)

    def changesubID(self,IDstr):
        self.subID.set(IDstr)
        root.update()

    def changetotalF(self,totFstr):
        self.totalF.set(totFstr)
        root.update()
        #gui.errorMsg("Start Processing...Please wait until the black exe window stop generating info, or the run button turns green")#has to keep so the GUI starts changing number
    
class dataProcess:
    #function entry, called by button press.
    def processData(self):
        
        gui.changetotalF("started...")
        gui.changesubID("started...")
        self.allLegal=True
        #check start end trial input legal
        self.startT,self.endT=gui.getEntry()
        print "_____________________________________"
        print "-------------------------------------"
        print "From .coded to Excel started..."
        print "Start trial number:"+self.startT
        print "End trial number:"+self.endT
        print "_____________________________________"
        print "-------------------------------------"
        print ""
        print ""
        if(self.startT==""):
            gui.errorMsg("Please input a start trial number.")
            self.allLegal=False
        elif(self.endT==""):
            gui.errorMsg("Please input a end trial number.")
            self.allLegal=False
        elif(int(self.startT) > int(self.endT)):
            gui.errorMsg("The start trial number should be smaller than the end trial number.")
            self.allLegal=False

        #open directory of this code
        if(self.allLegal):
            
            printFileRating = open('completeinfo.csv', 'wb')#creates and opens the output CSV file
            path = os.path.dirname(sys.argv[0]) #path=the current folder of this script
            dirs = os.listdir(path) #creates variable 'dirs' that is a list of all files in 'path' variable created above
#remove cartype add alerts seen, alert messages sent to phone
            columnTitles = ["Subject","Target","Age","Gender","TLTRCGroup",
                            "Trial","PedEnterRoadwayTime","PedXatEnterRoadway","PedZatEnterRoadway","LeadCarXatEnterRoadway",
                            "TailCarXatEnterRoadway","PedClearCarTime","PedXatClearCar","PedYatClearCar","LeadCarXatClearCar",
                            "TailCarXatClearCar","PedClearCarTime", "TailCarAtRoadwayTime", "GapSizeCalc","TimeToLeadCarEnterRoad","TimeToTailCarEnterRoad","RoadCrossingTime",
                            "TimeSpareClearTailCar","Hit","CloseCall","StandingPos","Number Gaps Seen",
                            "Gap Taken","1.5SecGapsSeen","2.0SecGapsSeen","2.5SecGapsSeen","3.0SecGapsSeen","3.5SecGapsSeen",
                            "4.0SecGapsSeen","4.5SecGapsSeen","5.5SecGapsSeen","6.5SecGapsSeen","PickSameGap","GapSeenDifference",
                            "Number Alerts Seen","EnterRoadDifferenceTime","Role","ClearRoadDifferenceTime","absEnterRoadDifferenceTime",
                            "absClearRoadDifferenceTime","Condition","GapTakenRecorded","Description"]
            
            self.writer = csv.writer(printFileRating, delimiter=',', skipinitialspace=True)
            self.writer.writerow(columnTitles) #writes column titles to csv file
            self.startTime = datetime.now()
            totalfilecnt=0
            print "**********"
            print "Step1: .coded to completeinfo.csv..."
            print "**********"
            for element in dirs:
                if element == os.path.basename(sys.argv[0]): #if name of script or output .csv changes, change name here
                    continue
                if element == "completeinfo.csv": 
                    continue
                if element == "codedinfo.csv":
                    continue
                if element =="byGapSeen.csv":
                    continue
                else:
                    #call func #1
                    self.completeInfoOneFile(element)
                    totalfilecnt=totalfilecnt+1
                    
                    gui.changetotalF(str(totalfilecnt))
            printFileRating.close()
            gui.changesubID("working on Coding, generating output #2, wait")
            
            #call func #2
            print "**********"
            print "Step2: completeinfo.csv to codedinfo.csv..."
            print "**********"
            self.codingCompInfo()
            gui.changesubID("working on byGapSeen, generating output #3, wait")

            #call func #3
            print "**********"
            print "Step3: generating byGapSeen from codedinfo.csv..."
            print "**********"
            self.byGapSeen()
            gui.changesubID("FINISHED!")
            print "Use time:"+ str(datetime.now() - self.startTime)
            print ("All done!")

    #used by func #3
    def convertSeen(self,lis): #change ['1','0','1','1','2','1','2'] to ['2.0', '3.0', '3.5', '4.0', '4.0', '4.5', '5.0', '5.0']
                    seen=[]
                    for ind in range(len(lis)):
                            seen.append(float(lis[ind]))
                    print seen
                    return seen

    #used by func #3
    def convertSeenTaken(self,lis,taken): #change ['1.5','2.0', '3.0', '3.5', '4.0', '4.0', '4.5', '5.0', '5.0'] to [('1.5','0'),('2.0', '0'), ('3.0', '0'), ('3.5', '0'), ('4.0', '1'), ('4.0', '0'), ('4.5', '0'), ('5.0', '0'), ('5.0', '0')]
        SeenTaken=[]
        countGaps = 0;
        for ele in lis:
            countGaps = countGaps + 1
            if(str(taken)==str(ele) and countGaps == len(lis)):
                SeenTaken.append((ele,'1'))
            else:
                SeenTaken.append((ele,'0'))
        return SeenTaken

    #called func #3: read from codedinfo.csv, output bygapseen
    def byGapSeen(self):
        outfile = open('byGapSeen.csv', 'wb')#creates and opens the output CSV file
        infilename="codedinfo.csv"
        columnTitles = ['Subject', "PairControl","Target","Age","Gender","Condition","Trial","StandingPosition","PickSameGap","GapSeen","Taken","Role","CarType","Alerts Seen"]  
        writer = csv.writer(outfile, delimiter=',', skipinitialspace=True)
        writer.writerow(columnTitles) #writes column titles to csv file
        with open(infilename,'r') as inputFile:
            for line in inputFile:#every line/trail will be expended to "Gap Seen" lines
                line=line.rstrip('\n')
                content=line.split(',')
                if(content[0] == 'Subject'):
                    continue
                taken=content[26]
                NumGaps = int(content[25])
                GapLimit = NumGaps + 47
                CarLimit = GapLimit+ NumGaps
                if taken=="":
                    continue
                #gapSeenNewlis=content[27:36]#add the 1.5 sec new gap to front
                #gapSeenNewlis.insert(0,content[44])
                gapSeenNewLis = content[47:GapLimit]
                carSeenNewLis = content[GapLimit:CarLimit]
                print gapSeenNewLis
                print carSeenNewLis
                tempList = ' '.join(gapSeenNewLis).replace('"','').split()
                NewCarList = ' '.join(carSeenNewLis).replace('"','').split()
                newGapList = self.convertSeen(tempList)
                seenlis=self.convertSeenTaken(newGapList,taken)
                for seenNo in range(int(len(seenlis))): #for every gap seen
                    outline=content[:7]#copy over subjectid to trial
                    outline.append(content[24])#standingposition
                    outline.append(content[36])#pickSameGap
                    outline.append(seenlis[seenNo][0])
                    outline.append(seenlis[seenNo][1])
                    outline.append(content[39])#Role
                    outline.append(NewCarList[seenNo])
                    writer.writerow(outline)
            inputFile.close()
        outfile.close()
    
    
    #used by func #2. lis to "1,2,3,4" string
    def listostr(self,lis): 
        line=""
        for ele in lis:
            line=line+ele+","
        line=line.rstrip(',')
        return line
    
    #called func #2: read from completeinfo.csv code the variables to number and output to codedinfo.csv
    def codingCompInfo(self):
        infile=open("completeinfo.csv","r")
        outfile=open("codedinfo.csv","wb")

        for line in infile:
            line=line.rstrip('\n')
            print(line)
            lis=line.split(',')

            if lis[0]=="Subject":
                lis.insert(1,"PairControl")
                newline=self.listostr(lis)
                outfile.write(newline+'\n')
            else:
                newlis=[]
                newlis.append(lis[0])
                if lis[1]=="Target":
                    newlis.append("1")#paircontrol
                    newlis.append("1")#target
                elif lis[1]=="Partner":
                    newlis.append("1")
                    newlis.append("0")
                elif lis[1]=="Control":
                    newlis.append("0")
                    newlis.append("1")
                else:
                    print "Warning: CompleteInfo...Target...element other than: target partner control"

                #print "SubjectID "+ lis[0]+"...Trial "+lis[5][6:]
                newlis.append(lis[2])#age
                if "F" in lis[3]:#gender
                    newlis.append("0")
                elif "M" in lis[3]:
                    newlis.append("1")
                else:
                    print "Warning:"
                if "TL" in lis[4]:
                    newlis.append("0")
                elif "TR" in lis[4]:
                    newlis.append("1")
                elif "C" in lis[4]:
                    newlis.append("2")
                else:
                    print "Warning: CompleteInfo...Target...element other than: TL TR C"
                newlis.append(lis[5][6:])#trial number
                newlis.extend(lis[6:23])#6G->22W collumn copy over
                if lis[23]=="Left":#standing pos
                    newlis.append("1")
                elif lis[23]=="Right":
                    newlis.append("0")
                elif lis[23]=="Center":
                    newlis.append("2")
                newlis.extend(lis[24:35])#24Y->32AG copy over
                if lis[35]=="Yes": #picksame gap
                    newlis.append("1")
                elif lis[35]=="No":
                    newlis.append("0")
                elif lis[35]=='-':
                    newlis.append("2")
                newlis.extend(lis[36:38])#copy over
                if lis[38]=="Leader":
                    newlis.append("1")
                elif lis[38]=="Follower":
                    newlis.append("0")
                elif lis[38]=="-":
                    newlis.append("2")
                elif lis[38]=="Tie":
                    newlis.append("3")
                else:
                    print "Warning: CompleteInfo...Target...element other than: Leader Follower -"
                newlis.extend(lis[39:])
                outfile.write(self.listostr(newlis)+'\n')
                
        infile.close()
        outfile.close()

    #called func #1: process one .coded file, generate one or two row for the output file "completeFile", print to writer
    def completeInfoOneFile(self,element):
        
        self.tr1=True #defult omit first few practice trials
        sourceFile = str(element) #find the data file
        inputFile = open(sourceFile, 'r') #open the data file where you read from
        file = inputFile.readlines() #reads each line in data file
        trials = ["","","","","","","","","","","","","","","","","", "", "","","","","","","","","","","","","","","","","","","","","","","","","","","","","","",""] #creates list of correct size for all columns
        trials1= ["","","","","","","","","","","","","","","","","", "", "","","","","","","","","","","","","","","","","","","","","","","","","","","","","","",""] #row for partner
        # index of each trial element
        #  0. "Subject", 1. "Target", 2. "Age", 3. "Gender", 4. "TLTRCGroup", 5."Trial",
               gaps = 0
        cars = 0
        partner =0 #if current processing the partner
        twoPerson=False #defalt with 1 people,became 2 when finding the Age2 exits
        targetOnLeft=True #change when reading standing on the left side...used to judge whether there is 2nd person in next line
        for line in file:
            if "id:" in line:
                currentLine = line.split(':')
                print "Processing input file: "+element+" Subject ID: "+currentLine[1]
                trials.pop(0)
                trials.insert(0, currentLine[1].strip("\n"))
                
                trials1.pop(0)
                trials1.insert(0, currentLine[1].strip("\n"))
                #check if id is a number
                num_format = re.compile("^[ 0-9]*$")
                isnumber = re.match(num_format,currentLine[1])
                if (not isnumber):
                    gui.errorMsg("The current subject id is not a number.\nPlease change it to a number.")
                else: 
                    gui.changesubID(currentLine[1])
                    
            elif "Age" in line:
                currentLine = line.split(':')
                if "Age1" in line:
                    trials.pop(2)
                    trials.insert(2, currentLine[1].strip("\n"))
                elif "Age2" in line:
                    twoPerson=True
                    trials1.pop(2)
                    trials1.insert(2, currentLine[1].strip("\n"))
            elif "Gender" in line:
                currentLine = line.split(':')
                print currentLine
                g=currentLine[1].strip("\n")
                if("f" not in g and "F" not in g and "m" not in g and "M" not in g):
                    gui.errorMsg("Gender needs to be f or F or m or M\nPlease check the titles of the current processing .coded file (subjectID showing on the GUI)\nYou can manully change")
                if "Gender1" in line:
                    trials.pop(3)
                    if("f" in g or "F" in g):
                        trials.insert(3, "F")
                    else:
                        trials.insert(3,"M")
                else:
                    trials1.pop(3)
                    if("f" in g or "F" in g):
                        trials1.insert(3, "F")
                    else:
                        trials1.insert(3,"M")
        
            elif "Group:" in line:
                currentLine = line.split(':')
                trials.pop(4)
                trials.insert(4, currentLine[1].strip("\n"))
            elif "Standing on the" in line:
                currentLine=line.split(':')
                if('True' in currentLine[1].strip("\n")):
                    targetOnLeft=True
                else:
                    targetOnLeft=False
            elif "Description" in line:# end of header, can put in standing location now - modified lakshmi
                currentLine=line.split(':')
                trials.pop(47)
                trials.insert(47, currentLine[1].strip("\n")
                
                if (not twoPerson):#add real standing position left/right/center
                    trials.pop(25)
                    trials.insert(25,"Center")
                    trials.pop(4)
                    trials.insert(4,"C")
                    trials.pop(1)
                    trials.insert(1,"Control")
                else:
                    trials.pop(1)
                    trials.insert(1,"Target")
                    trials1.pop(1)
                    trials1.insert(1,"Partner")
                    if(targetOnLeft):
                        trials.pop(23)
                        trials.insert(23,"Left")
                        trials1.pop(23)
                        trials1.insert(23,"Right")
                        trials.pop(4)
                        trials.insert(4,"TL")
                        trials1.pop(4)
                        trials1.insert(4,"TL")
                    elif(not targetOnLeft):
                        trials.pop(23)
                        trials.insert(23,"Right")
                        trials1.pop(23)
                        trials1.insert(23,"Left")
                        trials.pop(4)
                        trials.insert(4,"TR")
                        trials1.pop(4)
                        trials1.insert(4,"TR")
             #  6. "PedEnterRoadwayTime", 7. "PedXatEnterRoadway", 8. "PedZatEnterRoadway", 9. "LeadCarXatEnterRoadway", 10. "TailCarXatEnterRoadway",
        #  11. "PedClearCarTime", 12. "PedXatClearCar", 13. "PedZatClearCar", 14. "LeadCarXatClearCar", 15. "TailCarXatClearCar",16. "TailCarAtRoadwayTime",
        #  17. "GapSizeCalc", 18. "TimeToLeadCarEnterRoad",
       

            elif "== TRIAL" in line:
                if(str(int(self.startT))in line): # start from trial two
                    self.tr1= False
                if(str(int(self.endT)+1) in line): # untill trial 22.
                    break
                if (self.tr1==False):
                    print line
                trials.pop(5)
                trials.insert(5, line.strip("==\n"))
            elif "PedEnterRoadwayTime" in line:
                currentLine = line.split(':')
                trials.pop(6)
                trials.insert(6, currentLine[1].strip("\n"))
            elif "PedXatEnterRoadwayTime" in line:
                currentLine = line.split(":")
                trials.pop(7)
                trials.insert(7, currentLine[1].strip("\n"))
            elif "PedZatEnterRoadwayTime" in line:
                currentLine = line.split(":")
                trials.pop(8)
                trials.insert(8, currentLine[1].strip("\n"))
            elif "LeadCarXatEnterRoadwayTime" in line:
                currentLine = line.split(":")
                trials.pop(9)
                trials.insert(9, currentLine[1].strip("\n"))
            elif "TailCarXatEnterRoadwayTime" in line:
                currentLine = line.split(":")
                trials.pop(10)
                trials.insert(10, currentLine[1].strip("\n"))
            elif "PedClearCarTime" in line:
                currentLine = line.split(":")
                trials.pop(11)
                trials.insert(11, currentLine[1].strip("\n"))
            elif "PedXatClearCarTime" in line:
                currentLine = line.split(":")
                trials.pop(12)
                trials.insert(12, currentLine[1].strip("\n"))
            elif "PedZatClearCarTime" in line:
                currentLine = line.split(":")
                trials.pop(13)
                trials.insert(13, currentLine[1].strip("\n"))
            elif "LeadCarXatClearCarTime" in line:
                currentLine = line.split(":")
                trials.pop(14)
                trials.insert(14, currentLine[1].strip("\n"))
            elif "TailCarXatClearCarTime" in line:
                currentLine = line.split(":")
                trials.pop(15)
                trials.insert(15, currentLine[1].strip("\n"))
           #19. "TimeToTailCarEnterRoad", 20. "RoadCrossingTime", 21. "TimeSpareClearTailCar", 22. "Hit", 23. "CloseCall", 24. "StandingPos", 25. "Number Gaps Seen",
        #  26.  "Gap Taken", 27. "1.5SecGapsSeen", 28. "2.0SecGapsSeen", 29. "2.5SecGapsSeen", 30. "3.0SecGapsSeen", 31. "3.5SecGapsSeen", 32. "4.0SecGapsSeen", 33. "4.5SecGapsSeen", 34. "5.5SecGapsSeen",
        # 35. "6.5SecGapsSeen", 36. "PickSameGap", 37. "GapSeenDifference", 38. "Number Alerts Seen", 39. "EnterRoadDifferenceTime", 40."Role", 41. "ClearRoadDifferenceTime", 42."absEnterRoadDifferenceTime",
        # 43. "absClearRoadDifferenceTime", 44. "Condition", 45. "GapTakenRecorded",46. "Description", "AlertsSeen"
            elif "Alerts Seen" in line:
                pass
            elif "Alert Messages Sent to Phone" in line:
                pass
            elif "Gaps Seen" in line: #add computed variables basic
                gaps = 1
                #x of the end of the lead car - x of the front of the tail car
                GapSize = (float(trials[9]) - float(trials[10]))/11.176
                trials.pop(17)
                trials.insert(17, GapSize)
    
                TimeToLeadCarEnterRoad = (float(trials[9]) - float(trials[7])) / 11.176
                trials.pop(18)
                trials.insert(18, TimeToLeadCarEnterRoad)

                TimeToTailCarEnterRoad = (float(trials[7]) - float(trials[10])) / 11.176
                trials.pop(19)
                trials.insert(19, TimeToTailCarEnterRoad)

                RoadCrossingTime = float(trials[11]) - float(trials[6])
                trials.pop(20)
                trials.insert(20, RoadCrossingTime)

                TimeSpareClearTailCar = (float(trials[12]) - float(trials[15])) / 11.176
                trials.pop(21)
                trials.insert(21, TimeSpareClearTailCar)

                if TimeSpareClearTailCar < 0:
                    Hit = 1
                else:
                    Hit = 0
                trials.pop(22)
                trials.insert(22, Hit)

                if TimeSpareClearTailCar > 0 and TimeSpareClearTailCar < 1:
                    CloseCall = 1
                else:
                    CloseCall = 0
                trials.pop(23)
                trials.insert(23, CloseCall)
                
            elif gaps == 1:#process gaps
                currentLine = line.split(",")
                print(currentLine)
                if (element.strip() != ""):
                    newLine = []
                    
                    for element in currentLine:
                        takenRecorded=element.strip()
                        newLine.append(gapRounding(element.strip()))                    
                        if newLine.count('-1') !=0: # showing gaps that cannot be rounded
                            gui.errorMsg("A out of range gap (<1.25 or >5.25) showed up in cur file/tr, please double check.")
                            
                    if(partner != 1):
                        trials.pop(45)#GapTakenRecorded - modified lakshmi
                        trials.insert(45,takenRecorded)
                    else:
                        trials1.pop(45)
                        trials1.insert(45,takenRecorded)
                    
                    if (partner == 1):#partener
                        trials1.pop(24)
                        trials1.insert(24, len(newLine))
                        ###Target data being read first. Find Computed Variables extra here
                        if(float(trials1[24])==float(trials[24])):
                            trials.pop(35)#PickSameGap
                            trials.insert(35,"Yes")
                            trials1.pop(35)
                            trials1.insert(35,"Yes")
                            trials.pop(36)#GapSeenDifference=0
                            trials.insert(36,0)
                            trials1.pop(36)
                            trials1.insert(36,0)
                        elif(trials1[24]!=trials[24]):
                            trials.pop(35)
                            trials.insert(35,"No")
                            trials1.pop(35)
                            trials1.insert(35,"No")
                            GapSeenDifference=float(trials[24])-float(trials1[24])
                            if("TL" in trials[4]):#TargetOnLeft condition
                                trials.pop(36)#GapSeenDifference=left-right NumberGapsSeen
                                trials.insert(36,GapSeenDifference)
                                trials1.pop(36)
                                trials1.insert(36,GapSeenDifference)
                            elif("TR"in trials[4]):
                                trials.pop(36)#GapSeenDifference=left-right NumberGapsSeen
                                trials.insert(36,-GapSeenDifference)
                                trials1.pop(36)
                                trials1.insert(36,-GapSeenDifference)
                        EnterRoadDifferenceTime=float(trials[6])-float(trials1[6])
                        ClearRoadDifferenceTime=float(trials[11])-float(trials1[11]) #ClearRoadDifferenceTime=left-right ClearRoadWayTime
                        if("TL" in trials[4]):#EnterRoadDifferenceTime=left-right EnterRoadWayTime
                            trials.pop(37)
                            trials.insert(37,EnterRoadDifferenceTime)
                            trials1.pop(37)
                            trials1.insert(37,EnterRoadDifferenceTime)
                            trials.pop(39)
                            trials.insert(39,ClearRoadDifferenceTime)
                            trials1.pop(39)
                            trials1.insert(39,ClearRoadDifferenceTime)
                        elif("TR" in trials[4]):
                            trials.pop(37)
                            trials.insert(37,-EnterRoadDifferenceTime)
                            trials1.pop(37)
                            trials1.insert(37,-EnterRoadDifferenceTime)
                            trials.pop(39)
                            trials.insert(39,-ClearRoadDifferenceTime)
                            trials1.pop(39)
                            trials1.insert(39,-ClearRoadDifferenceTime)
                        trials.pop(40)
                        trials.insert(40,abs(EnterRoadDifferenceTime))
                        trials1.pop(40)
                        trials1.insert(40,abs(EnterRoadDifferenceTime))
                        trials.pop(41)
                        trials.insert(41,abs(ClearRoadDifferenceTime))
                        trials1.pop(41)
                        trials1.insert(41,abs(ClearRoadDifferenceTime))
                        if(("TL" in trials[4] and trials[37]>=0.00001) or ("TR" in trials[4] and trials[37]<=-0.00001)): #Role:Target lead, enter road first
                            trials.pop(38)
                            trials.insert(38,"Follower")
                            trials1.pop(38)
                            trials1.insert(38,"Leader")
                        elif(("TL" in trials[4] and trials[37]<=-0.00001) or ("TR" in trials[4] and trials[37]>=0.00001)):
                            trials.pop(38)
                            trials.insert(38,"Leader")
                            trials1.pop(38)
                            trials1.insert(38,"Follower")
                        else:
                            trials.pop(38)
                            trials.insert(38,"Tie")
                            trials1.pop(38)
                            trials1.insert(38,"Tie")
                        trials1.pop(25)
                        trials1.insert(25, newLine[-1])
                        
                        trials1.pop(26)
                        twoSecGaps = newLine.count("2.0")
                        trials1.insert(26, twoSecGaps)

                        trials1.pop(27)
                        twoHalfSecGaps = newLine.count("2.5")
                        trials1.insert(27, twoHalfSecGaps)

                        trials1.pop(28)
                        threeSecGaps = newLine.count("3.0")
                        trials1.insert(28, threeSecGaps)

                        trials1.pop(29)
                        threeHalfSecGaps = newLine.count("3.5")
                        trials1.insert(29, threeHalfSecGaps)

                        trials1.pop(30)
                        fourSecGaps = newLine.count("4.0")
                        trials1.insert(30, fourSecGaps)

                        trials1.pop(31)
                        fourHalfSecGaps = newLine.count("4.5")
                        trials1.insert(31, fourHalfSecGaps)
                    
                        trials1.pop(32)
                        fiveSecGaps = newLine.count("5.0")
                        trials1.insert(32, fiveSecGaps)

                        trials1.pop(33)
                        oneHalfSecGaps = newLine.count("1.5")
                        trials1.insert(33, oneHalfSecGaps)
                                            
                        trials1.pop(34)
                        oneSecGaps = newLine.count("1.0")
                        trials1.insert(34, oneSecGaps)
                        if(not self.tr1):
                            self.writer.writerow(trials)
                            self.writer.writerow(trials1)
                        gaps = 0
                        partner=0
                    else:#target or control                           
                        trials.pop(24)
                        trials.insert(24, len(newLine))

                        trials.pop(25)
                        trials.insert(25, newLine[-1])

                        trials.pop(26)
                        twoSecGaps = newLine.count("2.0")
                        trials.insert(26, twoSecGaps)

                        trials.pop(27)
                        twoHalfSecGaps = newLine.count("2.5")
                        trials.insert(27, twoHalfSecGaps)

                        trials.pop(28)
                        threeSecGaps = newLine.count("3.0")
                        trials.insert(28, threeSecGaps)

                        trials.pop(29)
                        threeHalfSecGaps = newLine.count("3.5")
                        trials.insert(29, threeHalfSecGaps)

                        trials.pop(30)
                        fourSecGaps = newLine.count("4.0")
                        trials.insert(30, fourSecGaps)

                        trials.pop(31)
                        fourHalfSecGaps = newLine.count("4.5")
                        trials.insert(31, fourHalfSecGaps)
                    
                        trials.pop(32)
                        fiveSecGaps = newLine.count("5.0")
                        trials.insert(32, fiveSecGaps)
                        
                        trials.pop(33)
                        oneHalfSecGaps = newLine.count("1.5")
                        trials.insert(33, oneHalfSecGaps)
                                            
                        trials.pop(34)
                        oneSecGaps = newLine.count("1.0")
                        trials.insert(34, oneSecGaps)
                        
                        gaps = 0
#print(sourceFile)

def gapRounding(ingap):
    gapRange=[]
    for gapindex in range(0,11):
        gapsize=1.5+gapindex*0.5
        gapRange.append((gapsize-0.25,gapsize+0.25)) #2.25 to 2.74 set to gap 2.5

    curGap=float(ingap)
    for pair in gapRange:
        print(str(pair[0])+' '+str(curGap)+' '+str(pair[1]))
        if pair[0]<=curGap<pair[1]:
            print('hi')
            return str(pair[0]+0.25)
    return '-1'
#add condition " if the commas end: return"
#add functions that put values under Jeehan's new headings into an array
    
root = Tk()
root.wm_title("CodedToExcel")
gui=GUItk(root)
dataP=dataProcess()

root.mainloop()
