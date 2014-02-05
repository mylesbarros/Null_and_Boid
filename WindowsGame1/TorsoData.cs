using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace WindowsGame1
{
    public struct TorsoData
    {
        public TorsoData(SkeletonPoint torsoTopPosition, SkeletonPoint torsoBottomPosition)
        {
            torsoTop = torsoTopPosition;
            torsoBottom = torsoBottomPosition;
        }

        public SkeletonPoint torsoTop;
        public SkeletonPoint torsoBottom;
    }
}
