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
        private int userID;

        public PressGestureEventArgs(bool leftHandPressing, int userID)
        {
            this.leftHandPressing = leftHandPressing;
            this.rightHandPressing = !leftHandPressing;

            this.userID = userID;
        }

        public bool getLeftHandPressing()
        {
            return leftHandPressing;
        }

        public bool getRightHandPressing()
        {
            return rightHandPressing;
        }

        public int getUserID()
        {
            return userID;
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
        public void Update(Skeleton skeleton, int userID)
        {

            if (skeleton != null)
            {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        TrackPress(skeleton, userID, true);
                        TrackPress(skeleton, userID, false);
                    }

                }
        } // end Update()

        // How far from an ideal arm extension we will still consider a press gesture.
        private const float ARM_EXTENSION_THRESHOLD = 0.015f; // Metric measurement.

        private void TrackPress(Skeleton skeleton, int userID, bool isLeft)
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

            JointType handJointId = (isLeft) ? JointType.HandLeft : JointType.HandRight;
            JointType shoulderJointId = (isLeft) ? JointType.ShoulderLeft : JointType.ShoulderRight;
            JointType headJointId = JointType.Head;
            JointType footJointId = (isLeft) ? JointType.FootLeft : JointType.FootRight;

            Joint hand = skeleton.Joints[handJointId];
            Joint shoulder = skeleton.Joints[shoulderJointId];
            Joint head = skeleton.Joints[headJointId];
            Joint foot = skeleton.Joints[footJointId];

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
                                    GestureDetected(this, new PressGestureEventArgs(isLeft, userID));
                        } // end if

                    } // end if

                }

                }
            } // end if
        } // end TrackPress()
    }
