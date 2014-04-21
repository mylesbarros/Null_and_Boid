using Microsoft.Kinect;

namespace WindowsGame1
{
	/// <summary>
	/// A checkpoint defining a point on the path of a tutorial animation.
	/// </summary>
	public class TutorialAnimationCheckpoint
	{
		private long timestamp;
		private SkeletonPoint intendedLocation;

		public TutorialAnimationCheckpoint(long timestamp, float x, float y, float z)
		{
			this.timestamp = timestamp;
			intendedLocation.X = x;
            intendedLocation.Y = y;
            intendedLocation.Z = z;
		}

		public long getTimeForPoint()
		{
			return timestamp;
		}

		public SkeletonPoint getPoint()
		{
			return intendedLocation;
		}
	}
}       
