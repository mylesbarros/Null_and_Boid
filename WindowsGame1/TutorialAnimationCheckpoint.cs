using Microsoft.Kinect;
using DotNET = System.Windows;

namespace WindowsGame1
{
	/// <summary>
	/// A checkpoint defining a point on the path of a tutorial animation.
	/// </summary>
	public class TutorialAnimationCheckpoint
	{
		private long timestamp;
		private DotNET.Point intendedLocation;

		public TutorialAnimationCheckpoint(long timestamp, float x, float y)
		{
			this.timestamp = timestamp;
			intendedLocation = new DotNET.Point(x, y);
		}

		public long getTimeForPoint()
		{
			return timestamp;
		}

		public DotNET.Point getPoint()
		{
			return intendedLocation;
		}
	}
}       
