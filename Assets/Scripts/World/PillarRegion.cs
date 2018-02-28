using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    public class PillarRegion : RegionBase
    {
        public override List<eSubSceneMode> AvailableSubSceneModes
        {
            get
            {
                return new List<eSubSceneMode>() {
                    eSubSceneMode.IntactPillar,
                    eSubSceneMode.DestroyedPillar
                };
            }
        }

        protected override eSubSceneMode InitialSubSceneMode
        {
            get
            {
                return eSubSceneMode.IntactPillar;
            }
        }
    }
} //end of namespace