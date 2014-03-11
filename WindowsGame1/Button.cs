using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DotNET = System.Windows;

namespace WindowsGame1
{
    class ButtonEventArgs : EventArgs
    {
        private long timeSpentInteracting;

        public ButtonEventArgs(long timeSpentInteracting)
        {
            this.timeSpentInteracting = timeSpentInteracting;
        }

        public long getTimeSpentInteracting()
        {
            return timeSpentInteracting;
        }
    }


    public class Button
    {
        private Texture2D sprite;
        private DotNET.Point Location;

        private float radius;
        private float radiusSq;

        private Hand[] hands;

        public static double[] distances;

        private Stopwatch stopwatch;
        private long minNecessaryInteractionTime;

        public event EventHandler<EventArgs> ButtonTriggered;

        public Button(Texture2D visualRepresentation, DotNET.Point location, long minInteractionTime)
        {
            sprite = visualRepresentation;
            this.Location = location;

            radius = ((sprite.Height / 2) + (sprite.Width / 2)) / 2;
            radiusSq = radius * radius;

            minNecessaryInteractionTime = minInteractionTime;
            stopwatch = new Stopwatch();

            hands = new Hand[6];
        }

        public void setVisualRepresentation(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public Texture2D getSprite()
        {
            return sprite;
        }

        public void UpdateHands(Hand[] newHands)
        {
            if (newHands != null)
            {
                this.hands = newHands;
            }
        }

        public DotNET.Point getLocation()
        {
            return Location;
        }

        private static Boolean timingInteraction = false;
        public void Update()
        {
                if (hands != null)
                {
                    Hand hand;
                    DotNET.Point handLoc;
                    double dist, x, y;
                    Boolean handInteracting = false;
                    distances = new double[hands.Length];
                    for (int i = 0; i < hands.Length; i++) // for each hand...
                    {
                        if (handInteracting == false)
                        {

                            hand = hands[i];
                            if (hand != null)
                            {
                                handLoc = hand.getLocation();

                                x = handLoc.X * DaVinciExhibit.xScale;
                                y = handLoc.Y * DaVinciExhibit.yScale;

                                DotNET.Point convertedHandLoc = new DotNET.Point(x, y);

                                // Compute the distance from the interaction button to a hand accessing it
                                dist = Math.Pow(Location.X - convertedHandLoc.X, 2) + Math.Pow(Location.Y - convertedHandLoc.Y, 2);
                                distances[i] = dist;
                                // If a hand is touching the TutorialButton...
                                if (dist < (radiusSq + hand.getRadiusSq()))
                                {
                                    handInteracting = true;
                                }
                            }
                        }

                    }
                    
                    if (handInteracting == true)
                    {
                        // If we're not already timing an interaction...
                        if (timingInteraction == false)
                        {
                          // Start timing their interaction with the button
                          stopwatch.Start();
                          timingInteraction = true;
                        }
                        else // We're already tracking an interaction with the TutorialButton...
                        {
                          // If our timing of the interaction has met our minimum required amount of time...
                          if (stopwatch.ElapsedMilliseconds > minNecessaryInteractionTime)
                          {
                              // If there are observers/listners...
                              if (ButtonTriggered != null)
                                 {
                                  // Tell them the great news! A tutorial has been requested!
                                  // We'll send them bland information as they only care *that* it happened; details are
                                  // extraneous
                                  ButtonTriggered(null, new EventArgs());
                                 }
                              }

                        } // end else (timingInteraction == false)
                     }
                     else // There are no hands interacting. =(
                        {
                            timingInteraction = false;
                            stopwatch.Stop();
                            stopwatch.Reset();
                        }

            }
        
        }
    }
}
