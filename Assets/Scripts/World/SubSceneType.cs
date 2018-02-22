using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    public enum eSubSceneBaseType
    {
        Always,
        Near,
        Far
    }

    public enum eSubSceneType
    {
        Always,
        Near,
        Far,
        IntactAlways,
        IntactNear,
        IntactFar,
        DestroyedAlways,
        DestroyedNear,
        DestroyedFar,
        None
    }
}