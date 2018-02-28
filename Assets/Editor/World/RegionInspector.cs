using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.World
{
    [CustomEditor(typeof(Region))]
    public class RegionInspector : RegionInspectorBase
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}