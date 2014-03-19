using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace WindowsGame1
{
    class GuideTutorial : Tutorial
    {
        private static String drawText = "YOU ARE IN THE GUIDE TUTORIAL";
        private const int SWITCH_TIME = 5000;

        public GuideTutorial(DaVinciExhibit stateMachine) : base(stateMachine)
        {
            ghostSkeleton = new SkeletonWrapper();

            ghostSkeleton.setHeadJoint(-.2, .4, 2.0);
            ghostSkeleton.setRightShoulderJoint(-.08, .105, 2.0);
            ghostSkeleton.setLeftShoulderJoint(-.350, .095, 2.0);
            ghostSkeleton.setRightFootJoint(-.005, -.905, 1.550);
            ghostSkeleton.setLeftFootJoint(-.325, -.927, 1.550);
            ghostSkeleton.setRightHandJoint(.1, 0.0, 2.0);
            ghostSkeleton.setLeftHandJoint(-.3, .5, 2.0);
        }

        public override void update(double delta)
        {
            //go through the animation

            if (stopwatch.ElapsedMilliseconds > SWITCH_TIME)
            {
                stop();
                nextState();
            }

        }

        public override String getDrawText()
        {
            StringBuilder builder = new StringBuilder(drawText);
            builder.Append(": ");
            builder.Append(SWITCH_TIME - stopwatch.ElapsedMilliseconds);
            return builder.ToString();
        }

        public override void nextState()
        {
            stateMachine.setTutorialState(new SpawnTutorial(stateMachine));
        }
    }
}
