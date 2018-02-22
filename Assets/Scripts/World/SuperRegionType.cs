using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    public enum eSuperRegionType
    {
        Centre,
        North,      //y+
        NorthEast,
        East,       //z+
        SouthEast,
        South,      //y-
        SouthWest,
        West,       //z-
        NorthWest
    }
}