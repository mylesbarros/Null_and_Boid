using Microsoft.Kinect;
using Microsoft.Xna.Framework;

namespace WindowsGame1
{
    /// <summary>
    /// A person with skeletal Kinect Data
    /// </summary>
    public class Person
    {
        private static int HASH_CODE = 0;
        private int hashCode;
        private bool isGhost;

        // add isGhost!
        public Person(SkeletonWrapper skeletonData, Color c, bool isGhost)
        {
            hashCode = HASH_CODE;
            HASH_CODE += 1;

            this.skeletonData = skeletonData;
            this.isGhost = isGhost;

            // Retrieve hand data.
            Joint rightHand = skeletonData.getRightHandJoint();
            Joint leftHand = skeletonData.getLeftHandJoint();

            // Retrieve torso data.
            if (isGhost == false)
            {
                Joint shoulderCenter = skeletonData.getCenterShoulderJoint();
                Joint spine = skeletonData.getSpineJoint();

                torsoTop = shoulderCenter.Position;
                torsoBottom = spine.Position;
            }

            leftHandPosition = leftHand.Position;
            rightHandPosition = rightHand.Position; 

            this.color = c;

            canSpawnBoids = true;

            this.rightHand = new Hand(rightHand);
            this.leftHand = new Hand(leftHand);
        }

        public bool isAGhost()
        {
            return isGhost;
        }

        public Person(SkeletonWrapper skeletonData,  bool isGhost) : this(skeletonData, new Color(), isGhost)
        {
        }

        public void ResetTimeBetweenSpawn()
        {
            timeBetweenSpawn = DEFAULT_TIME_BETWEEN_SPAWN;
        }

        public DepthImagePoint getLeftHandLocation()
        {
            DepthImagePoint output = new DepthImagePoint();

            output.X = (int)(leftHandLocation.X);
            output.Y = (int)(leftHandLocation.Y);
            output.Depth = leftHandLocation.Depth;

            return output;
        }

        public DepthImagePoint getRightHandLocation()
        {
            DepthImagePoint output = new DepthImagePoint();

            output.X = (int)(rightHandLocation.X);
            output.Y = (int)(rightHandLocation.Y);
            output.Depth = rightHandLocation.Depth;

            return output;
        }

        public SkeletonPoint getLeftHandPosition()
        {
            SkeletonPoint output = new SkeletonPoint();

            output.X = (int)(leftHandPosition.X);
            output.Y = (int)(leftHandPosition.Y);
            output.Z = leftHandPosition.Z;

            return output;
        }

        public SkeletonPoint getRightHandPosition()
        {
            SkeletonPoint output = new SkeletonPoint();

            output.X = (int)(rightHandPosition.X);
            output.Y = (int)(rightHandPosition.Y);
            output.Z = rightHandPosition.Z;

            return output;
        }

        public void setRightHandRadius(int radius)
        {
            rightHand.UpdateRadius(radius);
        }

        public void setLeftHandRadius(int radius)
        {
            leftHand.UpdateRadius(radius);
        }

        public void updateSkeletonData(SkeletonWrapper skel)
        {
            skeletonData = skel;

            Joint tempLeftHand, tempRightHand;
            tempLeftHand = skeletonData.getLeftHandJoint();
            tempRightHand = skeletonData.getRightHandJoint();

            this.leftHand.Update(tempLeftHand);
            this.rightHand.Update(tempRightHand);

            leftHandPosition = tempLeftHand.Position;
            rightHandPosition = tempRightHand.Position;
        }

        public int GetHashCode()
        {
            return hashCode;
        }

        public SkeletonWrapper skeletonData;

        public SkeletonPoint rightHandPosition;
        public SkeletonPoint leftHandPosition;

        public Hand leftHand;
        public Hand rightHand;

        public SkeletonPoint torsoTop;
        public SkeletonPoint torsoBottom;

        public Color color;

        private int leftHandRadius;
        private int rightHandRadius;

        public DepthImagePoint leftHandLocation;
        public DepthImagePoint rightHandLocation;

        public long timeBetweenSpawn;

        public long lastSpawnedBoidTimestampLeft;
        public long lastSpawnedBoidTimestampRight;

        public bool canSpawnBoids;

        private const long DEFAULT_TIME_BETWEEN_SPAWN = 80;
    }
}
