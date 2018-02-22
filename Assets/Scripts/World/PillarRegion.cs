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

        public override List<eSubSceneType> SubSceneTypes
        {
            get
            {
                return new List<eSubSceneType>() {
                    eSubSceneType.IntactAlways,
                    eSubSceneType.IntactFar,
                    eSubSceneType.IntactNear,
                    eSubSceneType.DestroyedAlways,
                    eSubSceneType.DestroyedFar,
                    eSubSceneType.DestroyedNear
                };
            }
        }

        protected override eSubSceneType GetSubSceneType(eSubSceneBaseType baseType)
        {
            switch (baseType)
            {
                case eSubSceneBaseType.Always:
                    return eSubSceneType.IntactAlways;
                case eSubSceneBaseType.Near:
                    return eSubSceneType.IntactNear;
                case eSubSceneBaseType.Far:
                    return eSubSceneType.IntactFar;
            }

            return eSubSceneType.None;
        }
    }
} //end of namespace