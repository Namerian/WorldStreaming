using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{

    public class SuperRegion : MonoBehaviour
    {
        eSuperRegionType type;
        World world;

        List<RegionBase> regions = new List<RegionBase>();

        //========================================================================================

        public eSuperRegionType Type { get { return type; } }
        public World World { get { return world; } }

        //========================================================================================

        public void Initialize(eSuperRegionType type, World world, List<RegionBase> regions)
        {
            this.type = type;
            this.world = world;

            this.regions = new List<RegionBase>(regions);

            foreach (var region in this.regions)
            {
                region.Initialize(this);
            }
        }

        public List<SubSceneJob> UpdateSuperRegion(List<Vector3> cameraPositions)
        {
            var result = new List<SubSceneJob>();
            foreach (var region in regions)
            {
                result.AddRange(region.UpdateRegion(cameraPositions));
            }
            return result;
        }

        //========================================================================================

        //        #region editor methods

        //#if UNITY_EDITOR

        //        [ExecuteInEditMode]
        //        public bool IsRegionNameUnique(string regionName)
        //        {
        //            Transform myTransform = transform;
        //            int occurences = 0;

        //            for (int i = 0; i < myTransform.childCount; i++)
        //            {
        //                var child = myTransform.GetChild(i);

        //                if (child.GetComponent<RegionBase>() != null && child.name == regionName)
        //                {
        //                    occurences++;
        //                }
        //            }

        //            if (occurences > 1)
        //            {
        //                return false;
        //            }
        //            else
        //            {
        //                return true;
        //            }
        //        }

        //#endif

        //        #endregion editor methods

        //========================================================================================
    }
}