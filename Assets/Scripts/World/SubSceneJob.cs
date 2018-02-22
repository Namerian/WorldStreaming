using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    public enum eSubSceneJobType
    {
        Load,
        Unload
    }

    public class SubSceneJob
    {
        public RegionBase Region { get; private set; }
        public eSubSceneType SceneType { get; private set; }
        public eSubSceneJobType JobType { get; private set; }
        public Action<SubSceneJob> Callback { get; private set; }

        public Transform SubSceneRoot { get; set; }
        public bool IsJobSuccessful { get; set; }

        public SubSceneJob(RegionBase region, eSubSceneType sceneType, eSubSceneJobType jobType, Action<SubSceneJob> callback)
        {
            Region = region;
            SceneType = sceneType;
            JobType = jobType;
            Callback = callback;
        }
    }
}