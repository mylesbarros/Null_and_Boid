using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNET = System.Windows;
using Microsoft.Xna.Framework;
using System.Windows;

namespace WindowsGame1
{
    static class FlockingEngine
    {
        public static float HIGH_COHESION = 2f;
        public static float LOW_COHESION = 1f;
        public static float HIGH_SEPARATION = 2f;
        public static float LOW_SEPARATION = 1f;
        public static float HIGH_ALIGNMENT = 2f;
        public static float LOW_ALIGNMENT = 1f;

        private static float ALIGN_WEIGHT = 1f;
        private static float COHESE_WEIGHT = 1f;
        private static float SEPARATE_WEIGHT = 1f;

        private static double MAX_TURN = Math.PI / 32;

        private static double OVERLAP_THRESHOLD = Math.Pow(Agent.DEFAULT_AGENT_RADIUS, 2);

        public static float getCohesion()
        {
            return COHESE_WEIGHT;
        }

        public static void setCohesion(float newCohesion)
        {
            COHESE_WEIGHT = newCohesion;
        }

        public static float getSeparation()
        {
            return SEPARATE_WEIGHT;
        }

        public static void setSeparatoin(float newSeparation)
        {
            SEPARATE_WEIGHT = newSeparation;
        }

        public static float getAlignment()
        {
            return ALIGN_WEIGHT;
        }

        public static void setAlignment(float newAlignment)
        {
            ALIGN_WEIGHT = newAlignment;
        }

        public static void Flock(List<Agent> activeAgents, List<Flockable> passiveAgents)
        {
            List<Flockable> neighbors;
            Vector2 pull;
            Vector2 newHead;
            double theta;

            List<Flockable> neighborCandidates = new List<Flockable>(activeAgents);

            List<Hand> hands = getHandsFromList(passiveAgents);

            for (int i = 0; i < passiveAgents.Count(); i++)
            {
                neighborCandidates.Add(passiveAgents[i]);
            }

            foreach (Agent agent in activeAgents)
            {
                neighbors = getNeigbors(agent, neighborCandidates);
                Vector2 align = ComputeAlignment(agent, neighbors);
                Vector2 cohese = ComputeCohesion(agent, neighbors);
                Vector2 separate = ComputeSeperation(agent, neighbors);

                pull = new Vector2();
                pull = Vector2.Add(pull, Vector2.Multiply(align, ALIGN_WEIGHT));
                pull = Vector2.Add(pull, Vector2.Multiply(cohese, COHESE_WEIGHT));
                pull = Vector2.Add(pull, Vector2.Multiply(separate, SEPARATE_WEIGHT));

                Hand closestHand = findClosestHand(agent, hands);

                Vector2 targetHand;
                Vector2 targetVector;
                if (closestHand != null)
                {
                    targetHand = new Vector2((float) closestHand.getLocation().X, (float) closestHand.getLocation().Y);
                    targetVector = Seek(agent, targetHand);

                    pull = Vector2.Add(pull, Vector2.Multiply(targetVector, closestHand.getFlockingWeight()));
                }

                newHead = Vector2.Add(agent.getHeading(), pull);

                theta = Vector.AngleBetween(new Vector(agent.getHeading().X, agent.getHeading().Y), new Vector(newHead.X, newHead.Y)) * (Math.PI / 180);

                if (Math.Abs(theta) > MAX_TURN)
                {
                    if (theta < 0)
                    {
                        theta = MAX_TURN * -1;
                    }
                    else
                    {
                        theta = MAX_TURN;
                    }

                    pull = new Vector2((float)((agent.getHeading().X * Math.Cos(theta)) - (agent.getHeading().Y * Math.Sin(theta))),
                                       (float)((agent.getHeading().Y * Math.Cos(theta)) + (agent.getHeading().X * Math.Sin(theta))));

                    agent.setHeading(Vector2.Normalize(pull));
                }
                else
                {
                    agent.setHeading(Vector2.Normalize(newHead));
                }

                //correctOverlap(agent, neighbors);       
            }
        }

        public static void correctOverlap(Agent agent, List<Flockable> neighbors)
        {
                double distSq;
                double overlap;
                double newX, newY;

                foreach (Flockable other in neighbors)
                {
                    if (other is Agent == true)
                    {
                        Vector2 toOther = new Vector2((float)(other.getLocation().X - agent.getLocation().X), (float)(other.getLocation().Y - agent.getLocation().Y));
                        distSq = toOther.LengthSquared();
                        //distSq = (Math.Pow((agent.getLocation().X - other.getLocation().X), 2) + Math.Pow((agent.getLocation().Y - other.getLocation().Y), 2));
                        overlap = agent.getAgentRadiusSq() + other.getAgentRadiusSq() - distSq;
                        if (agent.Equals(other) == false && overlap >= 0)
                        {
                            double scale = overlap / Math.Sqrt(distSq);
                            Vector2 move = new Vector2((float)(toOther.X * scale), (float)(toOther.Y * scale));
                            //newX = agent.getLocation().X + (((other.getLocation().X - agent.getLocation().X) / Math.Sqrt(distSq)) * overlap);
                            //newY = agent.getLocation().Y + (((other.getLocation().Y - agent.getLocation().Y) / Math.Sqrt(distSq)) * overlap);
                            newX = agent.getLocation().X + move.X;
                            newY = agent.getLocation().Y + move.Y;
                            agent.setLocation(new DotNET.Point(newX, newY));
                        }
                    }
                } // end foreach
        }

        public static List<Flockable> getNeigbors(Agent agent, List<Flockable> candidates)
        {
            List<Flockable> neighbors = new List<Flockable>();
            double distSq;

            foreach (Flockable other in candidates)
            {

                distSq = (Math.Pow((agent.getLocation().X - other.getLocation().X), 2) + Math.Pow((agent.getLocation().Y - other.getLocation().Y), 2));
                if (other is Hand)
                {
                    // nevermind
                }
                else if (distSq < other.getNeighborhoodRadiusSq() && agent.Equals(other) == false)
                {
                    neighbors.Add(other);
                }
            }

            return neighbors;
        }       

        /* Alignment
         * Align the heading of an agent based on those of its neighbors. The
         * force is calculated by generating an average heading vector. We then
         * subtract the agent's heading to get the steering force for the agent.
         * 
         * We assume that neighbors represents all of the visible neighbors of the
         * provided agent.
         */
        private static Vector2 ComputeAlignment(Agent agent, List<Flockable> neighbors)
        {
            // A running sum of the given agent's neighbors headings.
            Vector2 headingSum = new Vector2();
            int divisor = 0;

            Vector2 weightedHeading;
            // For each of the given agents neighbors...
            foreach (Flockable otherAgent in neighbors)
            {
                // Ensure that our agent was not counted among its neighbors.
                if (agent.Equals(otherAgent) == false)
                {
                    // Add our neighbors heading to our running sum
                    weightedHeading = Vector2.Multiply(otherAgent.getHeading(), otherAgent.getFlockingWeight());
                    headingSum = Vector2.Add(headingSum, weightedHeading);

                    // Necessary as we allow our given boid to be included in the neighbors.
                    divisor += otherAgent.getFlockingWeight();
                }
            }

            // The average of the neighbor headings.
            // Zero vector in the event that provided agent has no neighbors.
            Vector2 averageHeading = new Vector2();

            if (divisor > 0) // Avoid divide by zero erros.
            {
                // Basic math.
                averageHeading = Vector2.Divide(headingSum, (float) divisor);

                // Subtract the provided agent's heading to produce the steering force.
                // If this is puzzling the most effective way to think about this is to draw a
                // picture of the current heading and the average heading.
                averageHeading = Vector2.Subtract(averageHeading, agent.getHeading());
            }

            // Return steering force necessary to achieve alignment with peers.
            return averageHeading;
        }

        /* Seperation
         * Steer away from our neighbors to distance given agent from peers.
         * 
         * We assume that neighbors represents all of the visible neighbors of the
         * provided agent.
         */
        private static Vector2 ComputeSeperation(Agent agent, List<Flockable> neighbors)
        {
            Vector2 steeringForce = new Vector2();
            int numNeighbors = 0;

            foreach (Flockable otherAgent in neighbors)
            {
                if (otherAgent.Equals(agent) == false)
                {
                    Vector stupidTemp = DotNET.Point.Subtract(agent.getLocation(), otherAgent.getLocation());
                    Vector2 diffBetweenAgents = new Vector2((float) stupidTemp.X, (float) stupidTemp.Y);

                    diffBetweenAgents.Normalize();
                    diffBetweenAgents = Vector2.Divide(diffBetweenAgents, diffBetweenAgents.Length());

                    steeringForce = Vector2.Add(steeringForce, diffBetweenAgents);

                    numNeighbors += 1;
                }
            }

            return steeringForce;
        }

        /* Cohesion
         * Calculate center of mass of neighbors.
         */
        private static Vector2 ComputeCohesion(Agent agent, List<Flockable> neighbors)
        {
            Vector2 centerMassSum = new Vector2();
            float numNeighbors = 0;

            foreach (Flockable otherAgent in neighbors)
            {
                if (agent.Equals(otherAgent) == false)
                {
                    Vector2 stupidTemp = new Vector2((float) otherAgent.getLocation().X, (float) otherAgent.getLocation().Y);
                    if (otherAgent.getFlockingWeight() > 1)
                    {
                        stupidTemp = Vector2.Multiply(stupidTemp, otherAgent.getFlockingWeight());
                        numNeighbors += otherAgent.getFlockingWeight();
                    }
                    else
                    {
                        numNeighbors += 1;
                    }
                    centerMassSum = Vector2.Add(centerMassSum, stupidTemp);
                }
            }

            if (numNeighbors > 0)
            {
                centerMassSum = Vector2.Divide(centerMassSum, numNeighbors);

                return Seek(agent, centerMassSum);
            }

            return new Vector2();
        }

        public static List<Hand> getHandsFromList(List<Flockable> theList)
        {
            List<Hand> toReturn = new List<Hand>();
            foreach (Flockable agent in theList)
            {
                if (agent is Hand)
                {
                    toReturn.Add((Hand)agent);
                }
            }
            return toReturn;
        }

        public static Hand findClosestHand(Flockable agent, List<Hand> candidatesForHanddom)
        {
            double distSq = Double.MaxValue;
            Hand closest = null;
            foreach (Hand hand in candidatesForHanddom)
            {
                double tmp = Vector2.DistanceSquared(new Vector2((float)agent.getLocation().X, (float)agent.getLocation().Y), new Vector2((float)hand.getLocation().X, (float)hand.getLocation().Y));
                if (tmp < distSq)
                {
                    closest = hand;
                    distSq = tmp;
                }
            }

            return closest;
        }

        public static Vector2 Seek(Agent agent, Vector2 goal)
        {
            Vector2 agentLocation = new Vector2((float) agent.getLocation().X, (float) agent.getLocation().Y);
            Vector2 desired = Vector2.Subtract(goal, agentLocation);

            desired.Normalize();

            return desired;
        }
    }
}
