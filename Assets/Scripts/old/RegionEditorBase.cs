//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//namespace Game.World
//{
//    [ExecuteInEditMode]
//    public abstract class RegionEditorBase : MonoBehaviour
//    {
//        [SerializeField]
//        [HideInInspector]
//        protected Transform[] subSceneRoots = new Transform[Enum.GetValues(typeof(eSubSceneType)).Length];

//        [SerializeField]
//        bool drawBounds;

//        public abstract List<eSubSceneType> SubSceneTypes { get; }

//        void Awake()
//        {
//#if !UNITY_EDITOR
//            Destroy(this);
//#else
//            if (subSceneRoots.Length != Enum.GetValues(typeof(eSubSceneType)).Length)
//            {
//                var old = subSceneRoots;

//                subSceneRoots = new Transform[Enum.GetValues(typeof(eSubSceneType)).Length];

//                for (int i = 0; i < old.Length; i++)
//                {
//                    if (old[i] != null && i < subSceneRoots.Length)
//                    {
//                        subSceneRoots[i] = old[i];
//                    }
//                }
//            }
//#endif
//        }

//        /// <summary>
//        /// Loads a SubScene and transfers its contents to the appropriate Root object.
//        /// </summary>
//        /// <param name="sceneType"></param>
//        public void LoadSubScene(eSubSceneType sceneType)
//        {
//            //doing some checks before executing
//            if (!IsRegionIdUnique())
//            {
//                Debug.LogError("Several Regions have the same Id");
//                return;
//            }
//            else if (subSceneRoots[(int)sceneType] != null)
//            {
//                Debug.LogErrorFormat("The SubScene \"{0}\" is already loaded!", sceneType.ToString());
//                return;
//            }

//            //paths
//            string subScenePath = WorldUtility.GetSubScenePath(gameObject.scene.path, eSuperRegionType.Centre, GetComponent<RegionBase>().UniqueId, sceneType);
//            string subScenePathFull = WorldUtility.GetFullPath(subScenePath);

//            Scene subScene = new Scene();

//            if (System.IO.File.Exists(subScenePathFull))
//            {
//                subScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(subScenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
//            }

//            //subScene does not exist: create a new root
//            if (!subScene.IsValid())
//            {
//                var rootGO = new GameObject(WorldUtility.GetSubSceneRootName(sceneType));
//                var root = rootGO.transform;
//                root.SetParent(transform);
//                subSceneRoots[(int)sceneType] = root;
//            }
//            //else: move subScene content to open world scene
//            else
//            {
//                var rootGO = subScene.GetRootGameObjects()[0];
//                UnityEditor.SceneManagement.EditorSceneManager.MoveGameObjectToScene(rootGO, this.gameObject.scene);

//                var root = rootGO.transform;
//                root.SetParent(this.transform);

//                if (!root.gameObject.activeSelf)
//                {
//                    root.gameObject.SetActive(true);
//                }

//                subSceneRoots[(int)sceneType] = root;
//            }

//            //end: close subScene and mark open world scene as dirty
//            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(subScene, true);
//            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
//        }

//        /// <summary>
//        /// Transfers the content of a SubScene from the world to the scene object.
//        /// </summary>
//        /// <param name="sceneType"></param>
//        public void SaveSubScene(eSubSceneType sceneType)
//        {
//            //doing some checks before executing
//            if (!IsRegionIdUnique())
//            {
//                Debug.LogError("Several Regions have the same Id");
//                return;
//            }
//            else if (subSceneRoots[(int)sceneType] == null)
//            {
//                Debug.LogErrorFormat("The SubScene \"{0}\" is not loaded!", sceneType.ToString());
//                return;
//            }

//            //paths
//            string subScenePath = WorldUtility.GetSubScenePath(gameObject.scene.path, eSuperRegionType.Centre, GetComponent<RegionBase>().UniqueId, sceneType);
//            string subScenePathFull = WorldUtility.GetFullPath(subScenePath);

//            Scene subScene = new Scene();
//            Transform root = subSceneRoots[(int)sceneType];

//            //trying to open the scene
//            if (System.IO.File.Exists(subScenePathFull))
//            {
//                subScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(subScenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
//            }

//            //creating a new scene
//            if (!subScene.IsValid())
//            {
//                subScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Additive);
//            }

//            //cleaning the scene
//            var objects = subScene.GetRootGameObjects();
//            foreach (var obj in objects)
//            {
//                DestroyImmediate(obj);
//            }

//            //moving the root object
//            Vector3 rootPos = root.position;
//            root.SetParent(null, true);
//            UnityEditor.SceneManagement.EditorSceneManager.MoveGameObjectToScene(root.gameObject, subScene);

//            subSceneRoots[(int)sceneType] = null;

//            //create folder, in case it does not exist
//            string scenePath = gameObject.scene.path;
//            string folderPath = scenePath.Remove(scenePath.LastIndexOf('.'));
//            string parentFolderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
//            //Debug.LogFormat("Checking if path \"{0}\" is valid", folderPath);
//            if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
//            {
//                UnityEditor.AssetDatabase.CreateFolder(parentFolderPath, gameObject.scene.name);
//            }

//            //save the sub scene
//            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(subScene, subScenePath);

//            //close the sub scene
//            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(subScene, true);


//            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
//        }

//        /// <summary>
//        /// Helper methods that checks whether the id of the region is unique or not.
//        /// </summary>
//        /// <returns></returns>
//        protected bool IsRegionIdUnique()
//        {
//            Transform parentTransform = transform.parent;
//            string regionId = GetComponent<RegionBase>().UniqueId;
//            int occurences = 0;

//            for (int i = 0; i < parentTransform.childCount; i++)
//            {
//                var child = parentTransform.GetChild(i).GetComponent<RegionBase>();

//                if (child != null && child.UniqueId == regionId)
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

//        /// <summary>
//        /// Checks whether a specific SubScene is currently loaded.
//        /// </summary>
//        /// <param name="subSceneType"></param>
//        /// <returns></returns>
//        public bool IsSubSceneLoaded(eSubSceneType subSceneType)
//        {
//            return (subSceneRoots[(int)subSceneType] != null);
//        }

//        /// <summary>
//        /// Checks whether any SubScene is currently loaded.
//        /// </summary>
//        /// <returns></returns>
//        public bool IsAnySubSceneLoaded()
//        {
//            for (int i = 0; i < subSceneRoots.Length; i++)
//            {
//                if (subSceneRoots[i] != null)
//                {
//                    return true;
//                }
//            }

//            return false;
//        }

//        private void OnDrawGizmos()
//        {
//            if (drawBounds && Application.isEditor)
//            {
//                Gizmos.color = Color.green;
//                var bounds = GetComponent<RegionBase>().Bounds;
//                Gizmos.DrawWireCube(bounds.center, bounds.size);
//            }
//        }
//    }
//}