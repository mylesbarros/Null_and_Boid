using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace WindowsGame1
{
    class DespawnTutorial : Tutorial
    {
        private static String drawText = "TO REMOVE BOIDS MOVE YOUR HANDS TOGETHER AND APART";
        private const int SWITCH_TIME = 6000;

        public DespawnTutorial(DaVinciExhibit stateMachine) : base(stateMachine)
        {
            ghostSkeleton = new SkeletonWrapper();

            ghostSkeleton.setHeadJoint(-.2, .4, 2.0);
            ghostSkeleton.setRightShoulderJoint(-.08, .105, 2.0);
            ghostSkeleton.setLeftShoulderJoint(-.350, .095, 2.0);
            ghostSkeleton.setRightFootJoint(-.005, -.905, 1.550);
            ghostSkeleton.setLeftFootJoint(-.325, -.927, 1.550);
            ghostSkeleton.setRightHandJoint(.15, .2, 2.0);
            ghostSkeleton.setLeftHandJoint(0.0, -.2, 2.0);
        }

        public override void update(double delta)
        {
            SkeletonPoint rightSkelly = rightHandAnimator.getLocationForTimestamp(stopwatch.ElapsedMilliseconds);
            ghostSkeleton.setRightHandJoint(rightSkelly.X, rightSkelly.Y, rightSkelly.Z);

            SkeletonPoint leftSkelly = leftHandAnimator.getLocationForTimestamp(stopwatch.ElapsedMilliseconds);
            ghostSkeleton.setLeftHandJoint(leftSkelly.X, leftSkelly.Y, leftSkelly.Z);

            if (rightHandAnimator.isAnimationFinished() && leftHandAnimator.isAnimationFinished())
            {
                stop();
                nextState();
            }
        }

        public override string getDrawText()
        {
            StringBuilder builder = new StringBuilder(drawText);
            //builder.Append(": ");
            //builder.Append(stopwatch.ElapsedMilliseconds);
            return builder.ToString();
        }

        public override void nextState()
        {
            stateMachine.setTutorialState(new GuideTutorial(stateMachine), DaVinciExhibit.TutorialType.GUIDE);
        }
    }
}
