using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using DotNET = System.Windows;

namespace WindowsGame1
{
     ///<summary>
     ///Contains a collection of flockable agents and facilitates adding, removing, and updating them
     ///</summary>
    class Flock
    {
        private int MAX_BOIDS = 195;
        private List<Agent> boids;

         ///<summary>
         ///Create a Flock object
         ///</summary>
         ///<param name="startNum">The number of agents to start with in the Flock</param>
         ///<param name="width">The width of the flocking area</param>
         ///<param name="height">The height of the flocking area</param>
         ///<param name="maxSpeed">The maximum speed of an agent</param>
        public Flock(int startNum, int width, int height, double maxSpeed)
        {
            boids = new List<Agent>();
            Vector2 startVect;
            Random random = new Random();
            double startX, startY, startVel;

            for (int i = 0; i < startNum; i++)
            {
                startX = random.NextDouble() * width;
                startY = random.NextDouble() * height;
                //startVel = random.NextDouble() * maxSpeed;
                startVel = maxSpeed / 2;
                startVect = new Vector2((float)startX, (float)startY);

                boids.Add(new Agent(startX, startY, startVel, Vector2.Normalize(startVect)));
            }
        }

         ///<summary>
         ///Add an agent to the flock
         ///</summary>
         ///<param name="newAgent">The agent to add</param>
        public void AddAgent(Agent newAgent)
        {
            if (boids.Count <= MAX_BOIDS)
            {
                boids.Add(newAgent);
            }
        } // end AddAgent()

        
         ///<summary>
         ///Add an agent to the flock at a Point
         ///</summary>
         ///<param name="startPoint">The initial position of the Agent</param>
         ///<param name="startVel">The start velocity of the Agent</param>
         ///<param name="color">The color of the Agent</param>
        public void AddAgent(DotNET.Point startPoint, double startVel, Color color)
        {
            Vector2 startVect = new Vector2((float) startPoint.X, (float) startPoint.Y);
            startVect.Normalize();
            this.AddAgent(new Agent(startPoint, startVel, startVect, color));
        }


         ///<summary>
         ///Clear all agents in the flock
         ///</summary>
        public void ClearAgents()
        {
            boids.Clear();
        }

         ///<summary>
         ///Get a list of agents in the Flock
         ///</summary>
        public List<Agent> GetAgents()
        {
            return boids;
        }

         //<summary>
         ///Get the number of agents in the Flock
         ///</summary>         
        public int numAgents()
        {
            return boids.Count;
        }


         ///<summary>
         ///Instruct the agents to upated their positions
         ///</summary>
         ///<param name="delta">The time interval since last update</param>
         ///<param name="passiveAgents">A list of non boid agents (e.g. hands)</param>
        public void updateAgents(double delta, List<Flockable> passiveAgents)
        {
            Agent agent;
            for (int i = 0; i < boids.Count(); i++)
            {
                agent = boids[i];
                if (agent.Dead() == false)
                {
                    agent.update(delta * 2);
                }
                else
                {
                    boids.RemoveAt(i);
                }
            }

            FlockingEngine.Flock(boids, passiveAgents);
   
        }
    }
}
