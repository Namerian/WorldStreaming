using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.World
{
    public class Region : RegionBase
    {
        public override List<eSubSceneMode> AvailableSubSceneModes
        {
            get
            {
                return new List<eSubSceneMode>() {
                    eSubSceneMode.Normal
                };
            }
        }

        protected override eSubSceneMode InitialSubSceneMode
        {
            get
            {
                return eSubSceneMode.Normal;
            }
        }
    }
} //end of namespace