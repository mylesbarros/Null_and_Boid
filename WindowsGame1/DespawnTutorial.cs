using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsGame1
{
    class DespawnTutorial : Tutorial
    {
        private static String drawText = "YOU ARE IN THE DESPAWN TUTORIAL";
        private const int SWITCH_TIME = 6000;

        public DespawnTutorial(DaVinciExhibit stateMachine) : base(stateMachine)
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

        public override string getDrawText()
        {
            StringBuilder builder = new StringBuilder(drawText);
            builder.Append(": ");
            builder.Append(SWITCH_TIME - stopwatch.ElapsedMilliseconds);
            return builder.ToString();
        }

        public override void nextState()
        {
            stateMachine.setTutorialState(new GuideTutorial(stateMachine));
        }
    }
}
