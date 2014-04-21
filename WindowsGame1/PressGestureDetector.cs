using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Kinect;

namespace WindowsGame1
{
    class PressGestureEventArgs : EventArgs
    {
        private bool leftHandPressing;
        private bool rightHandPressing;
        private Person user;

        public PressGestureEventArgs(bool leftHandPressing, Person user)
        {
            this.leftHandPressing = leftHandPressing;
            this.rightHandPressing = !leftHandPressing;

            this.user = user;
        }

        public bool getLeftHandPressing()
        {
            return leftHandPressing;
        }

        public bool getRightHandPressing()
        {
            return rightHandPressing;
        }

        public Person getUser()
        {
            return user;
        }
    }

    class PressGestureDetector
    {
        private enum PressPosition
        {
            None = 0,
            Neutral = 1,
            Extended = 2
        }

        private const float HAND_HEIGHT_THRESHOLD = 0.05f;
        private const float ARM_EXTENDED_THRESHOLD = 0.04f;

        public event EventHandler<PressGestureEventArgs> GestureDetected;

        // We don't actually have to "update" any sense of state. Just easier to conform to this usage
        // in the event that we need to change the implementation to be more complex. Also makes
        // maintaining observers easier, as I'm not familiar with C#'s peculiarities regarding static classes.
        // And, really, the code itself is already dubious.
        public void Update(Person person)
        {

            if (person != null && person.skeletonData != null)
            {
                    if (person.skeletonData.getTrackingState() == SkeletonTrackingState.Tracked)
                    {
                        TrackPress(person, true);
                        TrackPress(person, false);
                    }

                }
        } // end Update()

        // How far from an ideal arm extension we will still consider a press gesture.
        private const float ARM_EXTENSION_THRESHOLD = 0.015f; // Metric measurement.

        private void TrackPress(Person person, bool isLeft)
        {
            /* We define a press gesture as being made up of two properties:
             * 1) whether the user's hand is below their shoulder when they make the gesture and
             * 2) whether the user stretches their arm out to approximately a third of their body height
             *    when making the gesture.
             * We require that the arm be outstretched by a third because the average adult arm is approximately
             * half of that adult's height (measuring from the tips of the fingers). We thus allow the user not to have to
             * fully outstretch their arm but also take into accoutn that we are unable to track their fingers. Having
             * tested this on an honest to God child it /also/ inadvertantly helps to accomodate the fact that children
             * have commicaly disproprotionate arms.
             */
            SkeletonWrapper skeleton = person.skeletonData;

            Joint hand, shoulder, head, foot;
            if (isLeft == true)
            {
                hand = skeleton.getLeftHandJoint();
                shoulder = skeleton.getLeftShoulderJoint();
                foot = skeleton.getLeftFootJoint();
            }
            else
            {
                hand = skeleton.getRightHandJoint();
                shoulder = skeleton.getRightShoulderJoint();
                foot = skeleton.getRightFootJoint();
            }
            head = skeleton.getHeadJoint();

            PressPosition pressPosition = PressPosition.None;

            // TODO: Don't spawn if hands are overlapping (deleting vs spawning mode)

            float metricHeight, metricArmExtension;

            // If anybody turns sideways, I swear to God, I'll kill them.
            if (hand.TrackingState == JointTrackingState.Tracked && head.TrackingState == JointTrackingState.Tracked
                && shoulder.TrackingState == JointTrackingState.Tracked)
            {

                // Height of user torso (in meters) - this is innacurate as hell, typically off by about two feet.
                metricHeight = head.Position.Y - foot.Position.Y;
                // Length of user's extension of arm (in meters) - this is also innacurate as hell but it is /proportionally so/,
                // so we can still use the relationship between them to identify a press.

                    metricArmExtension = -(hand.Position.Z - shoulder.Position.Z);



                // If the user's hand is below the shoulder...
                if (hand.Position.Y < (shoulder.Position.Y + HAND_HEIGHT_THRESHOLD))
                {
                    // If the user has extended their arm to the length of a third of their body,
                    // allowing for the grace threshold...
                    if ((metricHeight / 2.78) < (metricArmExtension + ARM_EXTENSION_THRESHOLD) == true)
                    {
                        // Then they have extended their arm into a press.
                        pressPosition = PressPosition.Extended;
                    }
                    else
                    {
                        pressPosition = PressPosition.Neutral;
                    } // end if-else if-else

                    // If the user has extended their arm...
                    if (pressPosition == PressPosition.Extended)
                    {

                        // Then notify the observers that we have a press. There's going to be a party. There is going to be cake.
                        if (GestureDetected != null)
                        {
                                    GestureDetected(this, new PressGestureEventArgs(isLeft, person));
                        } // end if

                    } // end if

                }

                }
            } // end if
        } // end TrackPress()
    }
