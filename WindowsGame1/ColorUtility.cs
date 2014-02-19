using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;

using SysDraw = System.Drawing;

namespace WindowsGame1
{
    static class ColorUtility
    {

        enum colours { Red, Green, Blue };

        private const float MILLIMETER = 0.001f;
        private const float SLIGHTLY_BRIGHTER = 1.08f;
        private const float SIGNIFICANTLY_BRIGHTER = 1.36f;

        private const int SATURATE_CONSTANT = 50;
        /* Computes a unique color for a user based on the average color of their torso.
         * Takes a TorsoData object that defines the dimensions of the user's torso, a width that defines the width
         * of the Kinect's ColorImageFrame, and a colorMap that defines the location of our colorData.
         * 
         * Returns a color that defines a user based on their torso color.
         */
        public static Color ComputeUserColor(Person user, int width, Color[] colorMap, KinectSensor kSensor)
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
            Color userColorAverage = Color.HotPink;

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
            Color userSampleColor;
            // The number of color samples taken from the user. Used to average our RGB values into an average color.
            int numColorSamples = 0;
            // The distance from the bottom of the user's torso of the current color sample. Measured in millimeters.
            float i;
            for (i = 0; i < metricTorsoLength; i += MILLIMETER)
            {
                // Set the color value to a default, value-less color.
                userSampleColor = new Color();
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
                    userSampleColor = colorMap[colorLocation.X + colorLocation.Y * width];
                }

                // Sum the color sample data for later averaging.
                r += userSampleColor.R;
                g += userSampleColor.G;
                b += userSampleColor.B;

                // We have now taken a color sample. Let's maintain our invariant.
                numColorSamples++;
            } // end for


            /* Average the running sum of RGB values taken from our color sampling.
            // If we were passed an invalid TorsoData then we will not have taken any color samples,
            // so we need to account by division by zero. Unless you want to be a dick, then by all means,
               delete this if-condition. */
            if (numColorSamples != 0)
            {
                userColorAverage.R = (byte)(r / numColorSamples);
                userColorAverage.G = (byte)(g / numColorSamples);
                userColorAverage.B = (byte)(b / numColorSamples);
            } // end if


            // Determine color of area surrounding user

            // Retrieve the position of the user's head as we can use its Y coordinate to define
            // the outer edge of the person
            Joint personTopJoint = user.skeletonData.Joints[JointType.Head];
            SkeletonPoint personTopPos = personTopJoint.Position;

            // Retrieve the position of the user's left shoulder as we can use its X coordinate
            // to define the outer edge of the person
            Joint leftShoulderJoint = user.skeletonData.Joints[JointType.ShoulderLeft];
            SkeletonPoint leftShoulderPos = leftShoulderJoint.Position;

            // We define the top bound of the person to be excluded as being the height of the person
            // shifted over to their far lefthand side, so we create a faux SkeletonPoint that amalgamates
            // the head and the shoulder
            SkeletonPoint personTopBoundPos = new SkeletonPoint();
            personTopBoundPos.X = leftShoulderPos.X;
            personTopBoundPos.Y = personTopPos.Y;
            personTopBoundPos.Z = personTopPos.Z;

            // We define the bottom bound of the person to be excluded as being their bottom right
            // foot, as this is caddy-corner to our upper bound, implying a rectangle that surrounds
            // the person
            Joint personBottomBoundJoint = user.skeletonData.Joints[JointType.FootRight];
            SkeletonPoint personBottomBoundPos = personBottomBoundJoint.Position;

            // Convert the Skeletal location data to color location data so that our locations correspond
            // to the colormap that we are working with
            ColorImagePoint topExclusionBound = kSensor.CoordinateMapper.MapSkeletonPointToColorPoint(personTopBoundPos, ColorImageFormat.RgbResolution640x480Fps30);
            ColorImagePoint bottomExclusionBound = kSensor.CoordinateMapper.MapSkeletonPointToColorPoint(personBottomBoundPos, ColorImageFormat.RgbResolution640x480Fps30);

            Color outerSampleColor;
            int numOuterColorSamples = 0;

            int currY;
            int currX;
            Boolean isOuterColor = true;
            r = 0; g = 0; b = 0;

            for (int j = 0; j < colorMap.Length; j++)
            {
                outerSampleColor = new Color();

                currY = j % width;

                // If j currently identifies a point within the exclusion bounds on the Y-axis...
                if (currY >= topExclusionBound.Y && currY <= bottomExclusionBound.Y)
                {
                    // Then it is possible that the color data corresponding to j is within our
                    // exclusion zone.

                    // We'll determine j's X coordinate and compare it to the exclusion bounds.
                    currX = j - (currY * width);
                    if (currX >= topExclusionBound.X && currX <= topExclusionBound.X)
                    {
                        isOuterColor = false;
                    }
                    else
                    {
                        isOuterColor = true;
                    }
                }
                else
                {
                    isOuterColor = true;
                }

                if (isOuterColor == true)
                {
                    outerSampleColor = colorMap[j];

                    // Sum the color sample data for later averaging.
                    r += outerSampleColor.R;
                    g += outerSampleColor.G;
                    b += outerSampleColor.B;
                }

                // We have now taken a color sample. Let's maintain our invariant.
                numOuterColorSamples++;
            } // end for-loop

            Color outerColorAverage = new Color();
            /* Average the running sum of RGB values taken from our color sampling.
            // If we were passed an invalid TorsoData then we will not have taken any color samples,
            // so we need to account by division by zero. Unless you want to be a dick, then by all means,
               delete this if-condition. */
            if (numOuterColorSamples != 0)
            {
                outerColorAverage.R = (byte)(r / numOuterColorSamples);
                outerColorAverage.G = (byte)(g / numOuterColorSamples);
                outerColorAverage.B = (byte)(b / numOuterColorSamples);
            } // end if

            int colorContrast = computerContrast(outerColorAverage, userColorAverage);

            
                // If the contrast between the user and the surrounding area is high...
                if (colorContrast > 125)
                {
                    // Then we only need to brighten the color of our resulting color by a small amount
                    userColorAverage = brightenColor(userColorAverage, SLIGHTLY_BRIGHTER);
                }
                // the contrast between the user and the surrounding area is low...
                else
                {
                    // We need to brighten our resulting color by a non-trivial amount
                    userColorAverage = brightenColor(userColorAverage, SIGNIFICANTLY_BRIGHTER);
                }

                userColorAverage = saturateColor(userColorAverage);

            // HSLColor hslColor = new HSLColor(colorAverage.R, colorAverage.G, colorAverage.B);
            // hslColor.Luminosity *= 0.1;
            // hslColor.Saturation *= 0.8;


            return userColorAverage;
        } // end method

        private static int capColorValue(int colorVal)
        {
            if (colorVal > 255)
            {
                colorVal = 255;
            }

            return colorVal;
        }

        private static Color brightenColor(Color color, float scale)
        {
            // Note on types - the multiplication produces a double. We cast this to an int, rather than
            // a byte, because if a byte were to increase past 255 then we would produce unexpected, seemingly
            // random color values
            int tempR = (int)(color.R * scale);
            int tempG = (int)(color.G * scale);
            int tempB = (int)(color.B * scale);

            capColorValue(tempR);
            capColorValue(tempG);
            capColorValue(tempB);

            return new Color(tempR, tempG, tempB);
        }

        private static Color saturateColor(Color color)
        {
            int tempR = (int)(color.R);
            int tempG = (int)(color.G);
            int tempB = (int)(color.B);

            int max = Math.Max(tempR, tempG);
            max = Math.Max(max, tempB);
            colours maxColor = colours.Red;

            int min = Math.Min(tempR, tempG);
            min = Math.Min(min, tempB);

            int constant = (int)(min * 1);

            if (max == tempR)
            {
                maxColor = colours.Red;
            }
            else if (max == tempG)
            {
                maxColor = colours.Green;
            }
            else
            {
                maxColor = colours.Blue;
            }

            tempR -= constant;
            tempG -= constant;
            tempB -= constant;

            double ratio = 0.0;
            switch (maxColor)
            {
                case colours.Red:
                    ratio = max / (double)tempR;
                    break;
                case colours.Green:
                    ratio = max / (double)tempG;
                    break;
                default:
                    ratio = max / (double)tempB;
                    break;
            }

            tempR = (int)(tempR * ratio);
            tempG = (int)(tempG * ratio);
            tempB = (int)(tempB * ratio);

            return new Color(tempR, tempG, tempB);
        }

        private static int computerContrast(Color firstColor, Color secondColor)
        {
            int firstColorBrightnessIndex, secondColorBrightnessIndex;

            // These constants are predetermined values computed by color specialists like Bob Ross
            firstColorBrightnessIndex = (firstColor.R * 299 + firstColor.G * 587 + firstColor.B * 114) / 1000;
            secondColorBrightnessIndex = (secondColor.R * 299 + secondColor.G * 587 + secondColor.B * 114) / 1000;

            int colorContrast = Math.Abs(firstColorBrightnessIndex - secondColorBrightnessIndex);

            return colorContrast;
        }

        private static Color saturate(Color color)
        {
            colours cCase = colours.Red;

            if (color.R > color.G)
            {
                // Red Saturate Case
                if (color.R > color.B)
                {
                    cCase = colours.Red;
                }

                // Blue Saturate Case
                else if (color.B > color.G)
                {
                    cCase = colours.Blue;
                }
            }

            // Green Saturate Case
            else if (color.G > color.B)
            {
                cCase = colours.Green;
            }

            // Blue Saturate Case #2
            else
            {
                cCase = colours.Blue;
            }

            switch (cCase)
            {
                case (colours.Red):
                    //red
                    break;
                case (colours.Green):
                    //green
                    break;
                default:
                    //blue
                    break;
            }

            return color;
        }
    }
}
