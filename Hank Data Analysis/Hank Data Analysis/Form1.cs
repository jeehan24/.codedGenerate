using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hank_Data_Analysis
{
    public partial class Form1 : Form
    {
        List<DroppedTime> droppedTime;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd=new OpenFileDialog();
            if ((fd.ShowDialog()== DialogResult.OK))
            {
                textBoxAddress.Text = fd.FileName;
            }
            else
                textBoxAddress.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.timer1.Start();
            droppedTime = new List<DroppedTime>();
            string[] droppedPair = textBoxDropped.Text.Trim().Split('|');
            string[] pair = new string[2];
            for (int i = 0; i < droppedPair.Length; i++)
            {
                pair = droppedPair[i].Split('-');
                if ((pair[0].Length != 0) && (pair[1].Length != 0))
                {
                    float start = float.Parse(pair[0]);
                    float end = float.Parse(pair[i]);
                    droppedTime.Add(new DroppedTime(start, end));
                }
            }
            DataAnalysis da;
            if (checkBox2.Checked)
            {
                System.IO.FileInfo fileinfo = new System.IO.FileInfo(textBoxAddress.Text);
                long length = fileinfo.Length;
                timer1.Interval = (int)length/100;
                da = new DataAnalysis(textBoxAddress.Text, droppedTime, false, true);
                timer1.Interval = 100;
            }
            else
            {
                if (checkBox1.Checked)
                    da = new DataAnalysis(textBoxAddress.Text, droppedTime, true,false);
                else
                    da = new DataAnalysis(textBoxAddress.Text, droppedTime, false,false);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Increment(1);
        }
    }
}
