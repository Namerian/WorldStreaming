using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    public static class WorldUtility
    {
        public static string GetSubSceneRootName(eSubSceneType type)
        {
            return string.Concat("SubScene_", type.ToString());
        }

        public static string GetSubSceneName(eSuperRegionType superRegionTag, string regionId, eSubSceneType subSceneTag)
        {
            return string.Concat("SubScene_", superRegionTag.ToString(), "_", regionId, "_", subSceneTag.ToString());
        }

        public static string GetSubScenePath(string worldScenePath, eSuperRegionType superRegionTag, string regionId, eSubSceneType subSceneTag)
        {
            string worldScenePathCleaned = worldScenePath.Remove(worldScenePath.LastIndexOf('.'));
            return string.Concat(worldScenePathCleaned, "/", GetSubSceneName(superRegionTag, regionId, subSceneTag), ".unity");
        }

        public static string GetFullPath(string path)
        {
            string appPath = Application.dataPath.Remove(Application.dataPath.LastIndexOf("Assets"));
            return string.Concat(appPath, path);
        }
    }
}