using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace WindowsGame1
{
    public abstract class Tutorial
    {
        protected DaVinciExhibit stateMachine;
        protected Tutorial nxtState;
        protected Stopwatch stopwatch;
        protected SkeletonWrapper ghostSkeleton;

        protected TutorialAnimation rightHandAnimator;
        protected TutorialAnimation leftHandAnimator;

        public Tutorial(DaVinciExhibit stateMachine)
        {
            this.stateMachine = stateMachine;

            stopwatch = new Stopwatch();
            start();
        }

        public abstract void update(double delta);
        public abstract String getDrawText();
        public abstract void nextState();

        public void start()
        {
            //this.rightHandAnimator.restartAnimation();
            //this.leftHandAnimator.restartAnimation();

            stopwatch.Reset();
            stopwatch.Start();
        }

        public void stop()
        {
            stopwatch.Stop();
        }
        
        public void setNextState(Tutorial next)
        {
            this.nxtState = next;
        }

        public SkeletonWrapper getGhostSkeleton()
        {
            return ghostSkeleton;
        }

        public void setRightAnimation(TutorialAnimation animation)
        {
            this.rightHandAnimator = animation;
            rightHandAnimator.restartAnimation();
        }

        public void setLeftAnimation(TutorialAnimation animation)
        {
            this.leftHandAnimator = animation;
            leftHandAnimator.restartAnimation();
        }
    }
}
