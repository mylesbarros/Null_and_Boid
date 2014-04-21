using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace WindowsGame1
{
    public class SkeletonWrapper
    {

        private Joint head;
        private Joint rightShoulder;
        private Joint leftShoulder;
        private Joint centerShoulder;
        private Joint rightFoot;
        private Joint leftFoot;
        private Joint rightHand;
        private Joint leftHand;
        private Joint spine;

        SkeletonTrackingState trackingState;

        public SkeletonWrapper()
        {
            setSkeleton(new Skeleton());
        }
        
        public SkeletonWrapper(Skeleton skel)
        {
            setSkeleton(skel);
        }

        public Joint getHeadJoint()
        {
            return head;
        }

        public void setHeadJoint(double x, double y, double z)
        {
            setJoint(x, y, z, ref head);
        }

        public Joint getRightShoulderJoint()
        {
            return rightShoulder;
        }

        public void setRightShoulderJoint(double x, double y, double z)
        {
            setJoint(x, y, z, ref rightShoulder);
        }

        public Joint getLeftShoulderJoint()
        {
            return leftShoulder;
        }

        public void setLeftShoulderJoint(double x, double y, double z)
        {
            setJoint(x, y, z, ref leftShoulder);
        }

        public Joint getCenterShoulderJoint()
        {
            return centerShoulder;
        }

        public void setCenterShoulderJoint(double x, double y, double z)
        {
            setJoint(x, y, z, ref centerShoulder);
        }

        public Joint getRightFootJoint()
        {
            return rightFoot;
        }

        public void setRightFootJoint(double x, double y, double z)
        {
            setJoint(x, y, z, ref rightFoot);
        }

        public Joint getLeftFootJoint()
        {
            return leftFoot;
        }

        public void setLeftFootJoint(double x, double y, double z)
        {
            setJoint(x, y, z, ref leftFoot);
        }

        public Joint getRightHandJoint()
        {
            return rightHand;
        }

        public void setRightHandJoint(double x, double y, double z)
        {
            setJoint(x, y, z, ref rightHand);
        }

        public Joint getLeftHandJoint()        
        {
            return leftHand;
        }

        public void setLeftHandJoint(double x, double y, double z)
        {
            setJoint(x, y, z, ref leftHand);
        }

        public Joint getSpineJoint()        
        {
            return spine;
        }

        public void setSpineJoint(double x, double y, double z)
        {
            setJoint(x, y, z, ref spine);
        }

        public SkeletonTrackingState getTrackingState()
        {
            return trackingState;
        }

        private void setJoint(double x, double y, double z, ref Joint joint)
        {
            SkeletonPoint newPoint = new SkeletonPoint();
            newPoint.X = (float)x;
            newPoint.Y = (float)y;
            newPoint.Z = (float)z;

            joint.Position = newPoint;
        }

        private void setSkeleton(Skeleton skel)
        {
            this.head = skel.Joints[JointType.Head];
            this.rightShoulder = skel.Joints[JointType.ShoulderRight];
            this.leftShoulder = skel.Joints[JointType.ShoulderLeft];
            this.centerShoulder = skel.Joints[JointType.ShoulderCenter];
            this.rightFoot = skel.Joints[JointType.FootRight];
            this.leftFoot = skel.Joints[JointType.FootLeft];
            this.leftHand = skel.Joints[JointType.HandLeft];
            this.rightHand = skel.Joints[JointType.HandRight];
            this.spine = skel.Joints[JointType.Spine];

            this.trackingState = skel.TrackingState;
        }
    }
}
