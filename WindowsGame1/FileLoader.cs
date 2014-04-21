using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WindowsGame1
{
    class FileLoader
    {

        public static TutorialAnimation loadTutorial(String filepath)
        {
            using (StreamReader reader = new StreamReader(filepath))
            {
                String line;
                String[] values;
                char[] delimeter = { ' ' };

                long timestamp;
                float x, y, z;
                long firstTimestamp = -1;

                TutorialAnimationCheckpoint tmp;
                List<TutorialAnimationCheckpoint> checkpoints = new List<TutorialAnimationCheckpoint>();

                while ((line = reader.ReadLine()) != null)
                {
                    //interpret line
                    values = line.Split(delimeter);

                    timestamp = long.Parse(values[0]);
                    x = float.Parse(values[1]);
                    y = float.Parse(values[2]);
                    z = float.Parse(values[3]);

                    if (firstTimestamp < 0)
                    {
                        firstTimestamp = timestamp;
                    }

                    tmp = new TutorialAnimationCheckpoint((timestamp - firstTimestamp), x, y, z);
                    checkpoints.Add(tmp);
                }

                TutorialAnimation animation = new TutorialAnimation(checkpoints);

                return animation;
            }
        }
    }
}
