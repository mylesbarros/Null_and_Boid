using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNET = System.Windows;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;

namespace WindowsGame1
{
    class ScissorGestureEventArgs : EventArgs
    {
        private int userID;
        private Boolean diverging;
        private double radiusScale;

        public ScissorGestureEventArgs(int userID, Boolean diverging, double radiusScale)
        {
            this.userID = userID;
            this.diverging = diverging;
            this.radiusScale = radiusScale;
        }

        public int getUserID()
        {
            return userID;
        }

        public Boolean gestureIsOver()
        {
            return diverging;
        }

        public double getRadiusScale()
        {
            return radiusScale;
        }
    }

    class ScissorGestureDetector
    {
        // Defines the state of a ScissorGesture for a user.
        private enum ScissorGestureState
        {
            None,
            Diverging,
            Converging,
            Proximal
        }

        private struct ScissorGestureTracker
        {
            public void reset()
            {
                leftHand = null;
                rightHand = null;

                currState = ScissorGestureState.None;
            }

            public Hand leftHand;
            public Hand rightHand;

            public ScissorGestureState currState;
        }

        private long delta;
        private ScissorGestureTracker[] scissorGestureTrackers;

        private const double CONTACT_SCALE = 1.25;

        public event EventHandler<ScissorGestureEventArgs> GestureDetected;

        public ScissorGestureDetector()
        {
            scissorGestureTrackers = new ScissorGestureTracker[6];
            delta = 0;
        }

        public void updateDelta(long newDelta)
        {
            delta = newDelta;
        }

        public void update(Person subject, int userID)
        {

            if (subject.skeletonData != null)
            {
                if (subject.skeletonData.TrackingState == SkeletonTrackingState.Tracked)
                {
                    // Check the state of the user's left and right hands to determine the relation of each to a 
                    // wave gesture.
                    TrackGesture(subject, userID, ref this.scissorGestureTrackers[userID]);
                }
                // If we can't track the user's motions we'll have to reset our data on them;
                // they may have left and will be replaced by another user.
                else
                {
                    this.scissorGestureTrackers[userID].reset();
                } // end if-else

            }

        }

        private void TrackGesture(Person person, int userID, ref ScissorGestureTracker gestureTracker)
        {
            if (person.leftHand == null || person.rightHand == null)
            {
                gestureTracker.reset();
                return;
            }

            Hand leftHand = person.leftHand;
            Hand rightHand = person.rightHand;

            Vector2 leftHandVelocity = leftHand.getHeading();
            Vector2 rightHandVelocity = rightHand.getHeading();
            Vector2 relativeHeading = Vector2.Subtract(leftHandVelocity, rightHandVelocity);
            Vector2.Normalize(relativeHeading);

            DotNET.Point leftHandPosition = leftHand.getLocation();
            DotNET.Point rightHandPosition = rightHand.getLocation();

            DotNET.Vector tempVec = DotNET.Point.Subtract(rightHandPosition, leftHandPosition);
            Vector2 leftToRight = new Vector2((float) tempVec.X, (float) tempVec.Y);

            double dotProduct = Vector2.Dot(relativeHeading, leftToRight);

            if (dotProduct > 0)
            {
                gestureTracker.currState = ScissorGestureState.Converging;
            }
            else
            {
                gestureTracker.currState = ScissorGestureState.Diverging;
            }

            // If the user's hands are converging...
            if (gestureTracker.currState == ScissorGestureState.Converging)
            {
                double leftHandRadius = leftHand.getRadius();
                double rightHandRadius = rightHand.getRadius();

                double minContactRadius = (leftHandRadius + rightHandRadius) * CONTACT_SCALE;

                // Check if they're proximal
                if (minContactRadius <= leftToRight.Length())
                {
                    gestureTracker.currState = ScissorGestureState.Proximal;
                }
            }

            if (gestureTracker.currState == ScissorGestureState.Proximal)
            {
                // send message
                GestureDetected(this, new ScissorGestureEventArgs(userID, false, CONTACT_SCALE));
            }
            else if (gestureTracker.currState == ScissorGestureState.Diverging)
            {
                GestureDetected(this, new ScissorGestureEventArgs(userID, true, CONTACT_SCALE));
            }
        }
    }
}
