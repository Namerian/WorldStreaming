using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    public class PillarRegion : RegionBase
    {
        public override void Initialize(SuperRegion superRegion)
        {
            base.Initialize(superRegion);
        }

        public override List<eSubSceneMode> SubSceneTypes
        {
            get
            {
                return new List<eSubSceneMode>() {
                    eSubSceneMode.IntactPillar,
                    eSubSceneMode.DestroyedPillar
                };
            }
        }

        //protected override eSubSceneState GetSubSceneType(eSubSceneType baseType)
        //{
        //    switch (baseType)
        //    {
        //        case eSubSceneType.Always:
        //            return Game.World.eSubSceneMode.IntactAlways;
        //        case eSubSceneType.Near:
        //            return Game.World.eSubSceneMode.IntactNear;
        //        case eSubSceneType.Far:
        //            return Game.World.eSubSceneMode.IntactFar;
        //    }

        //    return Game.World.eSubSceneMode.None;
        //}
    }
} //end of namespace