using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DotNET = System.Windows;
using System.Collections;

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
        private List<Texture2D> textures;
        private DotNET.Point Location;
        private int representation;
        private int numTextures;

        private float radius;
        private float radiusSq;

        private Person[] people;

        private Stopwatch stopwatch;
        private long minNecessaryInteractionTime;
        private Boolean buttonHasBeenTriggered;
        private Boolean timingInteraction;

        public event EventHandler<EventArgs> ButtonTriggered;

        public Button(List<Texture2D> textures, DotNET.Point location, long minInteractionTime)
        {

            this.textures = textures;
            this.Location = location;
            representation = 0;
            this.numTextures = textures.Count;

            radius = ((textures[0].Height / 2) + (textures[0].Width / 2)) / 2;
            radiusSq = radius * radius;

            minNecessaryInteractionTime = minInteractionTime;
            stopwatch = new Stopwatch();

            people = new Person[6];

            buttonHasBeenTriggered = false;

            timingInteraction = false;
        }

        public void setVisualRepresentation1(List<Texture2D> sprites)
        {
            this.textures = sprites;
        }

        public Texture2D getSprite()
        {
            return textures[representation];
        }

        public void UpdateHands(Person[] newHands)
        {
            if (newHands != null)
            {
                this.people = newHands;
            }
        }

        public DotNET.Point getLocation()
        {
            return Location;
        }

        public void Update()
        {
                if (people != null)
                {
                    Hand hand1 = null;
                    Hand hand2 = null;
                    
                    Boolean handInteracting = false;
                    // Check each hand for a possible interaction with the button
                    for (int i = 0; i < people.Length; i++)
                    {
                        // If we have not already detected a hand in contact with the button...
                        if (handInteracting == false)
                        {
                            if (people[i] != null)
                            {
                                hand1 = people[i].leftHand;
                                hand2 = people[i].rightHand;
                            }
                            
                            // Then determine if either of the hands of the current person are overlapping the button
                            handInteracting = handInRange(hand1);
                            if (handInteracting == false)
                            {
                                handInteracting = handInRange(hand2);
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
                              if (ButtonTriggered != null && buttonHasBeenTriggered == false)
                                 {
                                  // Tell them the great news! A tutorial has been requested!
                                  // We'll send them bland information as they only care *that* it happened; details are
                                  // extraneous
                                  representation = (representation + 1) % numTextures;
                                  ButtonTriggered(null, new EventArgs());

                                  buttonHasBeenTriggered = true;
                                 }
                              }

                        } // end else (timingInteraction == false)
                     }
                     else // There are no hands interacting. =(
                        {
                            timingInteraction = false;
                            stopwatch.Stop();
                            stopwatch.Reset();

                            buttonHasBeenTriggered = false;
                        }

            }
        
        }

        private Boolean handInRange(Hand hand)
        {
            DotNET.Point handLoc;
            double dist, x, y;

            if (hand != null)
            {
                handLoc = hand.getLocation();

                x = handLoc.X * DaVinciExhibit.xScale;
                y = handLoc.Y * DaVinciExhibit.yScale;

                DotNET.Point convertedHandLoc = new DotNET.Point(x, y);

                // Compute the distance from the interaction button to a hand accessing it
                dist = Math.Pow(Location.X - convertedHandLoc.X, 2) + Math.Pow((Location.Y + radius) - radius - convertedHandLoc.Y, 2);
                //distances[i] = dist;
                // If a hand is touching the TutorialButton...
                if (dist < (radiusSq + hand.getAgentRadiusSq()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
