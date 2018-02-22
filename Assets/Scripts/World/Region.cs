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

        public override List<eSubSceneType> SubSceneTypes
        {
            get
            {
                return new List<eSubSceneType>() {
                    eSubSceneType.Always,
                    eSubSceneType.Far,
                    eSubSceneType.Near
                };
            }
        }

        protected override eSubSceneType GetSubSceneType(eSubSceneBaseType baseType)
        {
            switch (baseType)
            {
                case eSubSceneBaseType.Always:
                    return eSubSceneType.Always;
                case eSubSceneBaseType.Near:
                    return eSubSceneType.Near;
                case eSubSceneBaseType.Far:
                    return eSubSceneType.Far;
            }

            return eSubSceneType.None;
        }
    }
} //end of namespace