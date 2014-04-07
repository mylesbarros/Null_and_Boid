using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using DotNET = System.Windows;

namespace WindowsGame1
{
     ///<summary>
     ///An entity that can use the flocking algorithm
     ///</summary>
    interface Flockable
    {
        Vector2 getHeading();
        DotNET.Point getLocation();
        int getFlockingWeight();
        int getAgentRadius();
        double getAgentRadiusSq();
        int getNeighborhoodRadius();
        double getNeighborhoodRadiusSq();
    }
}
