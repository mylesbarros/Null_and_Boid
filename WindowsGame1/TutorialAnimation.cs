using DotNET = System.Windows;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace WindowsGame1
{
	public class TutorialAnimation
	{
		private Queue<TutorialAnimationCheckpoint> checkpoints;
		private TutorialAnimationCheckpoint lastCheckpointReached;

		public TutorialAnimation(ICollection<TutorialAnimationCheckpoint> checkpoints,
		DotNET.Point spawnPoint)
		{
			this.checkpoints = new Queue(checkpoints);
			this.checkpoints.TrimToSize();			

			lastCheckpointReached = new TutorialAnimationCheckpoint(0f,
				spawnPoint.X, spawnPoint.Y);
		}
		
		/// <summary>
		/// Computes a new point for a DVSC tutorial animation based on the
		/// current timestamp. The method assumes a linear positive advancement
		/// of time across calls.
		/// If the timestamp exceeds the time period described by the animation
		/// then a point with negative values will be returned.
		/// </summary>
		public DotNET.Point getLocationForTimestamp(long timestamp)
		{
			TutorialAnimationCheckpoint nextPoint = checkpoints.peek();

			// If a sufficiently long period of time has passed since the last
			// point was requested the nextPoint pulled from the queue may be
			// in the past, rather than the future, respective to the provided stamp.
			// As such we will find the
			// next checkpoint in the queue past the provided timestamp.
			while (nextPoint != null &&
				nextPoint.getTimeForPoint() < timestamp)
			{
				nextPoint = checkpoints.dequeue();
			}

			// If there are no further checkpoints passed the provided timestamp
			// then we have reached the end of the animation; we'll signal this
			// to the user by returning a Point with negative values.
			if (nextPoint == null)
			{
				return new DotNET.Point(-1, -1);
			}
			
			// Extract Point data from previous and next checkpoints
			DotNET.Point lastCheckpointPoint, nextCheckpointPoint;
			lastCheckpointPoint = lastCheckpointReached.getPoint();
			nextCheckpointPoint = nextPoint.getPoint();
			// Compute the vector between the preceeding Point and the next defined
			// Point in the animation
			Vector vectorBetweenPoints = Vector.Subtract(
				nextCheckpointPoint, lastCheckpointPoint);
			
			// Compute the time difference between the previously generated point
			// and the next defined point in the animation
			long timeDifference = nextPoint.getTimeForPoint()
				- lastCheckpointReached.getTimeForPoint();
			
			long interpolationRatio = timestamp / timeDifference;
			
			// Compute a vector that will define the translation from the last
			// generated checkpoint to the Point being requested
			Vector mutationVector = Vector.Multiply(
				vectorBetweenPoints, interpolationRatio);
			
			// Generate the Point being requested
			DotNET.Point newPoint = lastCheckpointPoint.Add(mutationVector);
			// The just generated Point is now the new previous checkpoint
			// which all future requested checkpoints for this animation will
			// be computed with respect to
			lastCheckpointReached = new TutorialAnimationCheckpoint(
				timestamp, newPoint.X, newPoint.Y);

			return newPoint;
		}	
	}
}
