using Microsoft.Kinect;
using DotNET = System.Windows;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Collections = System.Collections.Generic;

namespace WindowsGame1
{
	public class TutorialAnimation
	{
        private Collections.List<TutorialAnimationCheckpoint> checkpoints;

		private TutorialAnimationCheckpoint lastCheckpointReached;
        private int index;

        private bool animationFinished;

		public TutorialAnimation(IEnumerable<TutorialAnimationCheckpoint> checkpoints)
		{
            this.checkpoints = new List<TutorialAnimationCheckpoint>(checkpoints);
            this.checkpoints.TrimExcess();

            restartAnimation();
		}
		
		/// <summary>
		/// Computes a new point for a DVSC tutorial animation based on the
		/// current timestamp. The method assumes a linear positive advancement
		/// of time across calls.
		/// If the timestamp exceeds the time period described by the animation
		/// then a point with negative values will be returned.
		/// </summary>
		public SkeletonPoint getLocationForTimestamp(long timestamp)
		{
            wrapIndex();

            TutorialAnimationCheckpoint nextPoint = checkpoints[index];

			// If a sufficiently long period of time has passed since the last
			// point was requested the nextPoint pulled from the queue may be
			// in the past, rather than the future, respective to the provided stamp.
			// As such we will find the
			// next checkpoint in the queue past the provided timestamp.
			while (nextPoint != null &&
				    nextPoint.getTimeForPoint() < timestamp && index < (checkpoints.Count - 1))
			{
                index += 1;
                nextPoint = checkpoints[index];
			}

            if ((index >= (checkpoints.Count - 1)) && (nextPoint.getTimeForPoint() < timestamp))
            {
                wrapIndex();

                nextPoint = checkpoints[index];

                animationFinished = true;
            }

			// If there are no further checkpoints passed the provided timestamp
			// then we have reached the end of the animation; we'll signal this
			// to the user by returning a Point with negative values.
			if (nextPoint == null)
			{
                SkeletonPoint output = new SkeletonPoint();
                output.X = -1;
                output.Y = -1;
                output.Z = -1;
				return output;
			}
			
			// Extract Point data from previous and next checkpoints and convert them to vector data
			SkeletonPoint lastCheckpointPoint, nextCheckpointPoint;
			lastCheckpointPoint = lastCheckpointReached.getPoint();
			nextCheckpointPoint = nextPoint.getPoint();

            Vector3 lastCheckpointVector3, nextCheckpointVector3;
            lastCheckpointVector3 = new Vector3(lastCheckpointPoint.X, lastCheckpointPoint.Y, lastCheckpointPoint.Z);
            nextCheckpointVector3 = new Vector3(nextCheckpointPoint.X, nextCheckpointPoint.Y, nextCheckpointPoint.Z);
			// Compute the vector between the preceeding Point and the next defined
			// Point in the animation
			Vector3 vectorBetweenPoints = Vector3.Subtract(
				nextCheckpointVector3, lastCheckpointVector3);
			
			// Compute the time difference between the previously generated point
			// and the next defined point in the animation
			long timeDifference = nextPoint.getTimeForPoint()
				- lastCheckpointReached.getTimeForPoint();
			
			float interpolationRatio = (timestamp - lastCheckpointReached.getTimeForPoint()) / (float)timeDifference;
			
			// Compute a vector that will define the translation from the last
			// generated checkpoint to the Point being requested
			Vector3 mutationVector = Vector3.Multiply(
				vectorBetweenPoints, interpolationRatio);
			
			// Generate the Point being requested
			SkeletonPoint newPoint = new SkeletonPoint();
            newPoint.X = lastCheckpointPoint.X + mutationVector.X;
            newPoint.Y = lastCheckpointPoint.Y + mutationVector.Y;
            newPoint.Z = lastCheckpointPoint.Z + mutationVector.Z;
			// The just generated Point is now the new previous checkpoint
			// which all future requested checkpoints for this animation will
			// be computed with respect to
			lastCheckpointReached = new TutorialAnimationCheckpoint(
				timestamp, newPoint.X, newPoint.Y, newPoint.Z);

			return newPoint;
		}

        public bool isAnimationFinished()
        {
            return animationFinished;
        }

        public void restartAnimation()
        {
            animationFinished = false;
            lastCheckpointReached = this.checkpoints[0];
            index = 1;
        }

        private void wrapIndex()
        {

            if (index >= checkpoints.Count)
            {
                index = 0;
            }

        }
	}
}
