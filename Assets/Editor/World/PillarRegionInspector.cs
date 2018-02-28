using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.World
{
    [CustomEditor(typeof(PillarRegion))]
    public class PillarRegionInspector : RegionInspectorBase
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}