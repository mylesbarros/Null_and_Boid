﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNET = System.Windows;
using Microsoft.Xna.Framework;

namespace WindowsGame1
{
    /// <summary>
    /// A single boid entity
    /// </summary>
    class Agent : Flockable
    {
        private DotNET.Point location;
        public double velocity { get; set; }
        private Vector2 heading;
        public Color color;
        private const int FLOCKING_WEIGHT = 1;

        public const int DEFAULT_AGENT_RADIUS = 10;
        public const int DEFAULT_NEIGHBORHOOD_RADIUS = 25;
        private int agentRadius;
        private double agentRadiusSq;

        private int neighborhoodRadius;
        private double neighborhoonRadiusSq;

        private bool isDead;

        private double heightVariable;
        private static double HEIGHT_START = 2;


        /// <summary>
        /// Construct an Agent from specified values
        /// </summary>
        /// <param name="x">starting x location of the agent</param>
        /// <param name="y">starting y location of the agent</param>
        /// <param name="velocity">starting velocity of the agent</param>
        /// <param name="initialHeading">starting heading of the agent</param>
        /// <param name="radius">radius of the agents collision circle</param>
        public Agent(double x, double y, double velocity, Vector2 initialHeading, int radius)
        {
            // As we are using an auto-implemented property we cannot modify location's variables directly;
            // doing so would merely modify the values of the duplicate made available to us. Setting
            // it to a new struct, however, overwrites the original reference. "Wakawaka" - C#.
            location = new DotNET.Point(x, y);

            this.velocity = velocity;

            heading = new Vector2(initialHeading.X, initialHeading.Y);

            color = Color.Black;

            isDead = false;
            heightVariable = HEIGHT_START;

            this.agentRadius = radius;
            this.agentRadiusSq = Math.Pow(radius, 2);

            this.neighborhoodRadius = DEFAULT_NEIGHBORHOOD_RADIUS;
            this.neighborhoonRadiusSq = Math.Pow(this.neighborhoodRadius, 2);
        }

        /// <summary>
        /// Construct an Agent from specified values
        /// </summary>
        /// <param name="x">starting x location of the agent</param>
        /// <param name="y">starting y location of the agent</param>
        /// <param name="velocity">starting velocity of the agent</param>
        /// <param name="initialHeading">starting heading of the agent</param>
        public Agent(double x, double y, double velocity, Vector2 initialHeading)
        {
            // As we are using an auto-implemented property we cannot modify location's variables directly;
            // doing so would merely modify the values of the duplicate made available to us. Setting
            // it to a new struct, however, overwrites the original reference. "Wakawaka" - C#.
            location = new DotNET.Point(x, y);

            this.velocity = velocity;

            heading = new Vector2(initialHeading.X, initialHeading.Y);

            color = Color.Black;
            heightVariable = HEIGHT_START;

            isDead = false;

            this.agentRadius = DEFAULT_AGENT_RADIUS;
            this.agentRadiusSq = Math.Pow(this.agentRadius, 2);

            this.neighborhoodRadius = DEFAULT_NEIGHBORHOOD_RADIUS;
            this.neighborhoonRadiusSq = Math.Pow(this.neighborhoodRadius, 2);
        }

        /// <summary>
        /// Construct an agent from specified values
        /// </summary>
        /// <param name="initialLocation">Starting point of the agent</param>
        /// <param name="velocity">starting velocity of the agent</param>
        /// <param name="initialHeading">starting heading of the agent</param>
        public Agent(DotNET.Point initialLocation, double velocity, Vector2 initialHeading)
        {
            location = initialLocation;

            this.velocity = velocity;

            heading = initialHeading;

            color = Color.Black;

            isDead = false;
            heightVariable = HEIGHT_START;

            this.agentRadius = DEFAULT_AGENT_RADIUS;
            this.agentRadiusSq = Math.Pow(this.agentRadius, 2);

            this.neighborhoodRadius = DEFAULT_NEIGHBORHOOD_RADIUS;
            this.neighborhoonRadiusSq = Math.Pow(this.neighborhoodRadius, 2);
        }

        /// <summary>
        /// Construct an agent from specified values
        /// </summary>
        /// <param name="initialLocation">Starting point of the agent</param>
        /// <param name="velocity">starting velocity of the agent</param>
        /// <param name="initialHeading">starting heading of the agent</param>
        /// <param name="c">color of the Agent</param>
        public Agent(DotNET.Point initialLocation, double velocity, Vector2 initialHeading, Color c)
        {
            location = initialLocation;

            this.velocity = velocity;

            heading = initialHeading;

            color = c;

            isDead = false;

            this.agentRadius = DEFAULT_AGENT_RADIUS;
            this.agentRadiusSq = Math.Pow(this.agentRadius, 2);

            this.neighborhoodRadius = DEFAULT_NEIGHBORHOOD_RADIUS;
            this.neighborhoonRadiusSq = Math.Pow(this.neighborhoodRadius, 2);
        }

        /// <summary>
        /// Instruct an agent to update it's position
        /// </summary>
        /// <param name="delta">The time since the last update</param>
        public void update(double delta)
        {
            Vector2 moveVect = Vector2.Multiply(heading, (float)(velocity * delta));
            location = DotNET.Point.Add(location, new DotNET.Vector(moveVect.X, moveVect.Y));
            wraparound();

            if (heightVariable > 0)
            {
                heightVariable -= (delta / 50);
            }
        }

        /// <summary>
        /// Get an agents heading
        /// </summary>
        /// <returns>the heading</returns>
        public Vector2 getHeading()
        {
            return heading;
        }

        public double getHeightVar()
        {
            return heightVariable;
        }

        /// <summary>
        /// Set an agents heading
        /// </summary>
        /// <param name="newHeading">the new heading</param>
        public void setHeading(Vector2 newHeading)
        {
            heading = newHeading;
        }

        /// <summary>
        /// Set an agents location
        /// </summary>
        /// <param name="newLocation">the new location</param>
        public void setLocation(DotNET.Point newLocation)
        {
            location = newLocation;
        }

        /// <summary>
        /// Get an agents location
        /// </summary>
        /// <returns>the location</returns>
        public DotNET.Point getLocation()
        {
            return location;
        }

        /// <summary>
        /// Get the flocking weight of the agents
        /// </summary>
        /// <returns>the flocking weight</returns>
        public int getFlockingWeight()
        {
            return FLOCKING_WEIGHT;
        }

        /// <summary>
        /// Wraps the agent back to the screen if an update causes it to leave
        /// </summary>
        private void wraparound()
        {
            double newX = location.X;
            double newY = location.Y;
            if (location.X < 0)
            {
                newX = DaVinciExhibit.dataWidth - 1;
            }
            else if (location.X > DaVinciExhibit.dataWidth)
            {
                newX = 0;
            }
            if (location.Y < 0)
            {
                newY = DaVinciExhibit.dataHeight - 1;
            }
            if (location.Y > DaVinciExhibit.dataHeight)
            {
                newY = 0;
            }

            location = new DotNET.Point(newX, newY);
        }

        public override bool Equals(Object other)
        {
            bool isAnAgent = this.GetType().IsAssignableFrom(other.GetType());

            if (isAnAgent == false)
            {
                return false;
            }
            else
            {
                Agent otherAgent = (Agent) other;

                // Todo: Comparing doubles.
                if (otherAgent.location == this.location && otherAgent.heading.Equals(this.heading) && otherAgent.velocity == this.velocity)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns if the agent is dead
        /// </summary>
        /// <returns>true if agent is dead. false otherwise</returns>
        public bool Dead()
        {
            return isDead;
        }

        /// <summary>
        /// Set an agent to be dead.
        /// </summary>
        public void KillAgent()
        {
            isDead = true;
        }

        /// <summary>
        /// Get an agents collision radius
        /// </summary>
        /// <returns>the radius</returns>
        public int getAgentRadius()
        {
            return agentRadius;
        }

        /// <summary>
        /// Get an agents collision radius squared
        /// </summary>
        /// <returns>radius * radius</returns>
        public double getAgentRadiusSq()
        {
            return agentRadiusSq;
        }

        public int getNeighborhoodRadius()
        {
            return neighborhoodRadius;
        }

        public double getNeighborhoodRadiusSq()
        {
            return neighborhoonRadiusSq;
        }
    }
}
