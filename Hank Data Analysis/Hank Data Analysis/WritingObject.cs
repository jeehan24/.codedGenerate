using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Hank_Data_Analysis
{
    class WritingObject
    {
        private StreamWriter sw;

        public WritingObject(string path)
        {
            sw = new StreamWriter(path+".coded");
            //sw.AutoFlush = true;
        }
        public bool Write(string str)
        {
            if (sw != null)
            {
                sw.WriteLine(str);
                sw.Flush();
                return true;
            }
            else
                return false;
        }

        internal void close()
        {
            sw.Close();
        }
    }
}
