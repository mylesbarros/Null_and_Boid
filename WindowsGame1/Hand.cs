using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNET = System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace WindowsGame1
{
    public class Hand : Flockable
    {
        private DotNET.Point currentLocation;
        private DotNET.Point previousLocation;

        private Joint prevHandData;

        private Vector2 heading;
        private Vector2 velocity;

        private Boolean active;

        private int radius;
        private double radiusSq;

        private const int FLOCKING_WEIGHT = 16;
        private const double HAND_DIST_THRESHOLD = 0.018f;

        private static KinectSensor sensor;

        public Hand(Joint hand)
        {
            heading = Vector2.One;

            prevHandData = hand;
            currentLocation = new DotNET.Point(-1, -1);
            UpdateLocation(ConvertSkeletonPointToLocation(hand.Position));

            active = false;

            radius = 0;
        }

        private void UpdateLocation(DotNET.Point newLocation)
        {
            previousLocation = currentLocation;
            currentLocation = newLocation;
            currentLocation.X = currentLocation.X;
            currentLocation.Y = currentLocation.Y;
        }

        public void Update(Joint hand)
        {
            //Sets the current and previous hand positions.
            SkeletonPoint currHandPosition = hand.Position;
            SkeletonPoint prevHandPosition = prevHandData.Position;

            //Gets the hand depth and updates its location
            UpdateLocation(ConvertSkeletonPointToLocation(currHandPosition));

            double metricDistanceSq = Math.Pow((currHandPosition.X - prevHandPosition.X), 2) + Math.Pow((currHandPosition.Y - prevHandPosition.Y), 2);

            DotNET.Vector tempHeading;
            //Calculates the heading if the hand direction changed
            if (Math.Sqrt(metricDistanceSq) > HAND_DIST_THRESHOLD) // ToDo: Remove square root
            {
                tempHeading = DotNET.Point.Subtract(currentLocation, previousLocation);

                if (tempHeading.X != 0 && tempHeading.Y != 0)
                {
                    velocity = new Vector2((float) tempHeading.X, (float) tempHeading.Y);
                    heading = new Vector2(velocity.X, velocity.Y);
                    heading.Normalize();
                }
            }

            if (currentLocation.X < 0 || currentLocation.Y < 0 || currentLocation.X > 640 || currentLocation.Y > 480)
            {
                if (previousLocation.X < 0 || previousLocation.Y < 0 || previousLocation.X > 640 || previousLocation.Y > 480)
                {
                    active = false;
                }
            }
            else
            {
                active = true;
            }
        }

        private DotNET.Point ConvertSkeletonPointToLocation(SkeletonPoint skelPoint)
        {
            DepthImagePoint tempNewLocation = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelPoint, DepthImageFormat.Resolution640x480Fps30);
            return new DotNET.Point(tempNewLocation.X, tempNewLocation.Y);
        }

        public Vector2 getHeading()
        {
            return heading;
        }

        public DotNET.Point getLocation()
        {
            return currentLocation;
        }

        public int getFlockingWeight()
        {
            return FLOCKING_WEIGHT;
        }

        public static void setSensor(KinectSensor newSensor)
        {
            sensor = newSensor;
        }

        public void UpdateRadius(int radius)
        {
            this.radius = radius;
            radiusSq = Math.Pow(radius, 2);
        }

        public int getAgentRadius()
        {
            return radius;
        }

        public double getAgentRadiusSq()
        {
            return radiusSq;
        }

        public int getNeighborhoodRadius()
        {
            return radius;
        }

        public double getNeighborhoodRadiusSq()
        {
            return radiusSq;
        }
    }
}
