using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    public class SubScene : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        eSubSceneType subSceneType;

        public eSubSceneType SubSceneType { get { return subSceneType; } set { subSceneType = value; } }

        [ExecuteInEditMode]
        private void Update()
        {
            if(name != WorldUtility.GetSubSceneRootName(subSceneType))
            {
                name = WorldUtility.GetSubSceneRootName(subSceneType);
            }
        }
    }
}