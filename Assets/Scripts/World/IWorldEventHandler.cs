using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.World
{
    public interface IWorldEventHandler : IEventSystemHandler
    {
#if UNITY_EDITOR
        void ImportSubScenes();
        void ExportSubScenes();
        void ClearSubSceneFolder();
#endif
    }
}