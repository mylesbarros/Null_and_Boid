using Microsoft.Kinect;
using Microsoft.Xna.Framework;

namespace WindowsGame1
{
    /// <summary>
    /// A person with skeletal Kinect Data
    /// </summary>
    public class Person
    {

        public Person(Skeleton skeletonData)
        {
            this.skeletonData = skeletonData;

            // Retrieve hand data.
            Joint rightHand = skeletonData.Joints[JointType.HandRight];
            Joint leftHand = skeletonData.Joints[JointType.HandLeft];

            // Retrieve torso data.
            Joint shoulderCenter = skeletonData.Joints[JointType.ShoulderCenter];
            Joint spine = skeletonData.Joints[JointType.Spine];

            leftHandPosition = leftHand.Position;
            rightHandPosition = rightHand.Position;

            torsoTop = shoulderCenter.Position;
            torsoBottom = spine.Position;

            this.color = new Color();
        }

        public Person(SkeletonPoint leftHand, SkeletonPoint rightHand, SkeletonPoint torsoTop, SkeletonPoint torsoBottom, Color c)
            {
                leftHandPosition = leftHand;
                rightHandPosition = rightHand;

                this.torsoTop = torsoTop;
                this.torsoBottom = torsoBottom;

                color = c;

                leftHandRadius = 0;
                rightHandRadius = 0;

                leftHandLocation = new DepthImagePoint();
                rightHandLocation = new DepthImagePoint();

                timeBetweenSpawn = DEFAULT_TIME_BETWEEN_SPAWN;
                lastSpawnedBoidTimestampLeft = 0;
                lastSpawnedBoidTimestampRight = 0;
            }

            public Person(SkeletonPoint leftHand, SkeletonPoint rightHand, SkeletonPoint torsoTop, SkeletonPoint torsoBottom)
            {
                leftHandPosition = leftHand;
                rightHandPosition = rightHand;

                this.torsoTop = torsoTop;
                this.torsoBottom = torsoBottom;

                color = new Color();

                leftHandRadius = 0;
                rightHandRadius = 0;

                leftHandLocation = new DepthImagePoint();
                rightHandLocation = new DepthImagePoint();

                timeBetweenSpawn = DEFAULT_TIME_BETWEEN_SPAWN;
                lastSpawnedBoidTimestampLeft = 0;
                lastSpawnedBoidTimestampRight = 0;
            }

            public Person(SkeletonPoint leftHand, SkeletonPoint rightHand)
            {
                torsoTop = new SkeletonPoint();
                torsoBottom = new SkeletonPoint();

                leftHandPosition = leftHand;
                rightHandPosition = rightHand;

                color = new Color();

                leftHandRadius = 0;
                rightHandRadius = 0;

                leftHandLocation = new DepthImagePoint();
                rightHandLocation = new DepthImagePoint();

                timeBetweenSpawn = DEFAULT_TIME_BETWEEN_SPAWN;
                lastSpawnedBoidTimestampLeft = 0;
                lastSpawnedBoidTimestampRight = 0;
            }

            public void ResetTimeBetweenSpawn()
            {
                timeBetweenSpawn = DEFAULT_TIME_BETWEEN_SPAWN;
            }

            public DepthImagePoint getLeftHandLocation()
            {
                DepthImagePoint output = new DepthImagePoint();

                output.X = (int) (leftHandLocation.X);
                output.Y = (int) (leftHandLocation.Y);
                output.Depth = leftHandLocation.Depth;

                return output;
            }

            public DepthImagePoint getRightHandLocation()
            {
                DepthImagePoint output = new DepthImagePoint();

                output.X = (int) (rightHandLocation.X);
                output.Y = (int) (rightHandLocation.Y);
                output.Depth = rightHandLocation.Depth;

                return output;
            }

            public SkeletonPoint getLeftHandPosition()
            {
                SkeletonPoint output = new SkeletonPoint();

                output.X = (int) (leftHandPosition.X);
                output.Y = (int) (leftHandPosition.Y);
                output.Z = leftHandPosition.Z;

                return output;
            }

            public SkeletonPoint getRightHandPosition()
            {
                SkeletonPoint output = new SkeletonPoint();

                output.X = (int) (rightHandPosition.X);
                output.Y = (int) (rightHandPosition.Y);
                output.Z = rightHandPosition.Z;

                return output;
            }

            public Skeleton skeletonData;

            public SkeletonPoint rightHandPosition;
            public SkeletonPoint leftHandPosition;

            public SkeletonPoint torsoTop;
            public SkeletonPoint torsoBottom;

            public Color color;

            public int leftHandRadius;
            public int rightHandRadius;

            public DepthImagePoint leftHandLocation;
            public DepthImagePoint rightHandLocation;

            public long timeBetweenSpawn;

            public long lastSpawnedBoidTimestampLeft;
            public long lastSpawnedBoidTimestampRight;

            private const long DEFAULT_TIME_BETWEEN_SPAWN = 80;
        }
}
