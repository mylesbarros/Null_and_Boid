using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsGame1
{
    class GuideTutorial : Tutorial
    {
        private static String drawText = "YOU ARE IN THE GUIDE TUTORIAL";
        private const int SWITCH_TIME = 5000;

        public GuideTutorial(DaVinciExhibit stateMachine) : base(stateMachine)
        {
            //ghostPerson = new Person(null);
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
