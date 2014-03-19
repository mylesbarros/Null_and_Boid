using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace WindowsGame1
{
    class SpawnTutorial : Tutorial
    {
        private static String drawText = "YOU ARE IN THE SPAWN TUTORIAL - Elapsed Time: ";
        private const int SWITCH_TIME = 6000;

        public SpawnTutorial(DaVinciExhibit stateMachine) : base(stateMachine)
        {
            ghostSkeleton = new SkeletonWrapper();

            ghostSkeleton.setHeadJoint(-.2, .4, 2.0);
            ghostSkeleton.setRightShoulderJoint(-.08, .105, 2.0);
            ghostSkeleton.setLeftShoulderJoint(-.350, .095, 2.0);
            ghostSkeleton.setRightFootJoint(-.005, -.905, 1.550);
            ghostSkeleton.setLeftFootJoint(-.325, -.927, 1.550);
            ghostSkeleton.setRightHandJoint(.25, .1, 2.0);
            ghostSkeleton.setLeftHandJoint(-.25, .1, 2.0);
        }

        public override void update(double delta)
        {
            SkeletonPoint skelly = animationSys.getLocationForTimestamp(stopwatch.ElapsedMilliseconds);
            ghostSkeleton.setRightHandJoint(skelly.X, skelly.Y, skelly.Z);

            if (animationSys.isAnimationFinished())
            {
                stop();
                nextState();
            }
        }

        public override string getDrawText()
        {
            StringBuilder builder = new StringBuilder(drawText);
            builder.Append(": ");
            builder.Append(stopwatch.ElapsedMilliseconds);
            return builder.ToString();
        }

        public override void nextState()
        {
            stateMachine.setTutorialState(new DespawnTutorial(stateMachine));
        }
    }
}
