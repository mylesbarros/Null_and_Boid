using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;

namespace WindowsGame1
{
    static class ColorUtility
    {

        private const float MILLIMETER = 0.001f;
        /* Computes a unique color for a user based on the average color of their torso.
         * Takes a TorsoData object that defines the dimensions of the user's torso, a width that defines the width
         * of the Kinect's ColorImageFrame, and a colorMap that defines the location of our colorData.
         * 
         * Returns a color that defines a user based on their torso color.
         */
        public static Color ComputeUserColor(TorsoData user, int width, Color[] colorMap, KinectSensor kSensor)
        {
            /*  ComputeUserColor computes a Color for a user based on the color of their torso. This
             * color is computed by taking the metric length of their torso and taking a vertical slice
             * from which an average is computed from a selection of color samples. These samples are
             * taken every millimeter from the base of the user's torso to the top of it.
             * For those of us who are not drug users, deals, or Europhiles, this equates to between
             * 300 and 400 color samples for an average adult and approximately 200 samples for a child.
             */

            // Retrieve the skeletal data from the TorsoData.
            SkeletonPoint torsoTop = user.torsoTop;
            SkeletonPoint torsoBottom = user.torsoBottom;
            // Compute the metric length of the user's torso.
            double metricTorsoLength = torsoTop.Y - torsoBottom.Y;

            // The avergae color computed from our user sampling. Defaults to Hot Pink but will be overriden,
            // rather than modified, in the event that we successfully compute a user color.
            Color colorAverage = Color.HotPink;

            // We initialize the RGB values as integers, rather than bytes, as each of their values prior to being
            // averaged will almost certainly be many times larger than a byte.
            int r = 0, g = 0, b = 0;
            /* The skeletalSamplePosition defines the location of the skeletal position for which we desire a color sample
            // from the RGB colorMap. As we are taking a vertical color sample we initialize the skeletalSamplePosition
            // as being equivalent to the base of the user's torso and merely modify the Y value for each new color sample. */
            SkeletonPoint skeletalSamplePosition = new SkeletonPoint(); // New as opposed to pointing torsoBottom directly to avoid reference issues.
            skeletalSamplePosition.X = torsoBottom.X;
            skeletalSamplePosition.Y = torsoBottom.Y;
            skeletalSamplePosition.Z = torsoBottom.Z;

            if ((skeletalSamplePosition.X != 0 || skeletalSamplePosition.Y != 0 || skeletalSamplePosition.Z != 0) == false)
            {
                return new Color();
            }
            // Defines the location of the color sample within the RGB space.
            ColorImagePoint colorLocation;
            // The actual color sample defined by colorLocation.
            Color sampleColor;
            // The number of color samples taken from the user. Used to average our RGB values into an average color.
            int numColorSamples = 0;
            // The distance from the bottom of the user's torso of the current color sample. Measured in millimeters.
            float i;
            for (i = 0; i < metricTorsoLength; i += MILLIMETER)
            {
                // Set the color value to a default, value-less color.
                sampleColor = new Color();
                // Shift the source of the sample upwards by the value of i.
                skeletalSamplePosition.Y = torsoBottom.Y + i;
                // Convert our skeletal position (colorPosition) to color coordinates.
                colorLocation = kSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skeletalSamplePosition, ColorImageFormat.RgbResolution640x480Fps30);

                // Ensure that the location of our color sample is a valid index. If the person moves
                // offscreen while we're taking the sample then the colorLocation will contain invalid
                // negative values.
                if (colorLocation.X > 0 && colorLocation.Y > 0)
                {
                    // Extract the color sample from the RGB image.
                    sampleColor = colorMap[colorLocation.X + colorLocation.Y * width];
                }

                // Sum the color sample data for later averaging.
                r += sampleColor.R;
                g += sampleColor.G;
                b += sampleColor.B;

                // We have now taken a color sample. Let's maintain our invariant.
                numColorSamples++;
            } // end for

            /* Average the running sum of RGB values taken from our color sampling.
            // If we were passed an invalid TorsoData then we will not have taken any color samples,
            // so we need to account by division by zero. Unless you want to be a dick, then by all means,
               delete this if-condition. */
            if (numColorSamples != 0)
            {
                colorAverage.R = (byte)(r / numColorSamples);
                colorAverage.G = (byte)(g / numColorSamples);
                colorAverage.B = (byte)(b / numColorSamples);
            } // end if

            return colorAverage;
        } // end method
    }
}
