using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.World
{
    public class Region : RegionBase
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
                    eSubSceneMode.Normal
                };
            }
        }

        //protected override eSubSceneState GetSubSceneType(eSubSceneType baseType)
        //{
        //    switch (baseType)
        //    {
        //        case eSubSceneType.Always:
        //            return Game.World.eSubSceneMode.Always;
        //        case eSubSceneType.Near:
        //            return Game.World.eSubSceneMode.Near;
        //        case eSubSceneType.Far:
        //            return Game.World.eSubSceneMode.Far;
        //    }

        //    return Game.World.eSubSceneMode.None;
        //}
    }
} //end of namespace