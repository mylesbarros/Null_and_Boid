using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WindowsGame1
{
    class FileWriter
    {

        String filename;
        long firstTimeStamp;

        public FileWriter(String filename)
        {
            this.filename = filename;
            firstTimeStamp = -1;
        }

        public void writeLine(long timestamp, float x, float y, float z)
        {
            using (StreamWriter writer = new StreamWriter(filename, true))
            {
                if (firstTimeStamp < 0)
                {
                    firstTimeStamp = timestamp;
                }

                writer.WriteLine((timestamp - firstTimeStamp) + " " + x + " " + y + " " + z);
            }
        }

    }
}
