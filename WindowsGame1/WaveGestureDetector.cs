using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;

namespace WindowsGame1
{

    class WaveGestureEventArgs : EventArgs
    {
        private int userID;

        public WaveGestureEventArgs(int userID)
        {
            this.userID = userID;
        }

        public int getUserID()
        {
            return userID;
        }
    }

    class WaveGestureDetector
    {
        // Defines the state of a WaveGesture for a user.
        private enum WaveGestureState
        {
            None = 0,
            Success = 1,
            Failure = 2,
            InProgress = 3
        }
        // Defines the position of their wave gesture - whether they have waved to the
        // left, to the right, or simply have their hand held up.
        private enum WavePosition
        {
            None = 0,
            Left = 1,
            Right = 2,
            Neutral = 3
        }

        // Keeps track of an individual user's wave gesture.
        private struct WaveGestureTracker
        {
            public void Reset()
            {
                State = WaveGestureState.None;
                Timestamp = 0;
                StartPosition = WavePosition.None;
                CurrentPosition = WavePosition.None;
                IterationCount = 0;
            }

            public void UpdateState(WaveGestureState state, long timestamp)
            {
                State = state;
                Timestamp = timestamp;
            }

            public void UpdatePosition(WavePosition position, long timestamp)
            {

                if (CurrentPosition != position)
                {
                    if (position == WavePosition.Left || position == WavePosition.Right)
                    {
                        if (State != WaveGestureState.InProgress)
                        {
                            State = WaveGestureState.InProgress;
                            StartPosition = position;
                            IterationCount = 0;
                        } // end if

                        IterationCount += 1;
                    } // end if
                } // end if

                CurrentPosition = position;
                Timestamp = timestamp;
            }

            public int IterationCount;
            public WavePosition StartPosition;
            public WavePosition CurrentPosition;
            public WaveGestureState State;
            public long Timestamp;
        }

        // How far from ideal a user is allowed to be for us to still identify
        // their motion as a wave.
        private const float WAVE_THRESHOLD = 0.014f; // Measured in meters.
        private const int WAVE_MOVEMENT_TIMEOUT = 4600;
        // Defines how many times the user needs to wave back and forth before we accept it
        // as a wave /gesture/. Dance for me, User.
        // Note that there's a fundamental contradiction between identifying whether a user has
        // waved to the left or to the right and the use of wave iterations. If iterations
        // is greater than 1 then we need to modify WaveGestureArgs, as wave direction no
        // longer has any meaning. I hope none of my future employers ever see this code.
        private const int REQUIRED_ITERATIONS = 1;

        // Stores a WaveGestureTracker for the left and right hands of each user.
        private WaveGestureTracker[] playerWaveTracker;
        private Stopwatch stopwatch;
        private KinectSensor sensor;

        public event EventHandler<WaveGestureEventArgs> GestureDetected;

        public WaveGestureDetector(KinectSensor kSensor)
        {
            playerWaveTracker = new WaveGestureTracker[6];
            stopwatch = new Stopwatch();

            sensor = kSensor;
        }

        public void Update(SkeletonWrapper skeleton, int userID)
        {
            stopwatch.Stop();
            long frameTimestamp = stopwatch.ElapsedMilliseconds;

            if (skeleton != null)
            {
                    if (skeleton.getTrackingState() == SkeletonTrackingState.Tracked)
                    {
                        // Check the state of the user's left and right hands to determine the relation of each to a 
                        // wave gesture.
                        TrackWave(skeleton, userID, ref this.playerWaveTracker[userID], frameTimestamp);
                    }
                    // If we can't track the user's motions we'll have to reset our data on them;
                    // they may have left and will be replaced by another user.
                    else
                    {
                        this.playerWaveTracker[userID].Reset();
                    } // end if-else
                    
                }

            stopwatch.Start();
        } // end Update()

        private const double HAND_DISTANCE_THRESHHOLD_SQ = 0.042f;

        private void TrackWave(SkeletonWrapper skeleton, int userID, ref WaveGestureTracker tracker, long timestamp)
        {
            // The check for a user wave in this implementation is fairly simple. If the user raises their hand directly above their
            // shoulder then they are merely primed for a wave. If they raise it above and to the left or right of their shoulder,
            // or if they move that hand left or right after having primed it, then we count that as a wave. For such an implementation
            // the WaveGestureState is not genuinley necessary however if we choose to change our approach (eg, require three motions to
            // constitute a valid wave) then the use of this struct will render these changes much simpler.

            // Retrieve the relevent hand and shoulder for the user depending on which hand/shoulder pair that we
            // are checking for a wave.
            Joint leftHand = skeleton.getLeftHandJoint();
            Joint rightHand = skeleton.getRightHandJoint();

            Joint centerOfShoulders = skeleton.getCenterShoulderJoint();

            // Metric measurement
            double distanceBetweenHandsSq = (Math.Pow(leftHand.Position.X - rightHand.Position.X, 2) + Math.Pow(leftHand.Position.Y - rightHand.Position.Y, 2));

            // If the user joints are tracked...
            if (leftHand.TrackingState == JointTrackingState.Tracked && rightHand.TrackingState == JointTrackingState.Tracked)
            {
                // If the user is not taking forever...
                if (tracker.State == WaveGestureState.InProgress && tracker.Timestamp + WAVE_MOVEMENT_TIMEOUT < timestamp)
                {
                    tracker.UpdateState(WaveGestureState.Failure, timestamp);
                }
                // If the user's hands are overlapping...
                else if (distanceBetweenHandsSq <= HAND_DISTANCE_THRESHHOLD_SQ)
                {
                    // If the user is waving to the left...
                    // (We assume that, through the act of overlapping their hands, users will naturally center their hands over their torso.)
                    if (leftHand.Position.X < (centerOfShoulders.Position.X + WAVE_THRESHOLD))
                    {
                        tracker.UpdatePosition(WavePosition.Left, timestamp);
                    }
                    // Check if the user is waving to the right...
                    else if (leftHand.Position.X > (centerOfShoulders.Position.X - WAVE_THRESHOLD))
                    {
                        tracker.UpdatePosition(WavePosition.Right, timestamp);
                    }
                    else
                    {
                        tracker.UpdatePosition(WavePosition.Neutral, timestamp);
                    } // end if-else if-else

                    // If the user has not yet achieved a wave but have satisfied the number of wave iterations...
                    if (tracker.State != WaveGestureState.Success && tracker.IterationCount == REQUIRED_ITERATIONS)
                    {
                        // Then they're waving.
                        tracker.UpdateState(WaveGestureState.Success, timestamp);

                        // If we have observers...
                        if (GestureDetected != null)
                        {
                            // Notify them and let them know which direction the user waved and with which hand they waved.
                            GestureDetected(this, new WaveGestureEventArgs(userID));
                        } // end if

                    } // end if

                }
                else
                {
                    if (tracker.State == WaveGestureState.InProgress)
                    {
                        tracker.UpdateState(WaveGestureState.Failure, timestamp);
                    }
                    else
                    {
                        tracker.Reset();
                    } // end if-else
                } // end if-else if-else
            } // end if
        } // end TrackWave()
    } // end class
} // end namespace
