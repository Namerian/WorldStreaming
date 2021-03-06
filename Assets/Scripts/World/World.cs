﻿using Game.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.World
{
    public class World : MonoBehaviour, IWorldEventHandler
    {
        public static readonly Dictionary<eSuperRegionType, Vector3> SUPERREGION_OFFSETS = new Dictionary<eSuperRegionType, Vector3>()
        {
            {eSuperRegionType.Centre, Vector3.zero },
            {eSuperRegionType.North,        new Vector3(0,1,0) },
            {eSuperRegionType.NorthEast,    new Vector3(0,1,1) },
            {eSuperRegionType.East,         new Vector3(0,0,1) },
            {eSuperRegionType.SouthEast,    new Vector3(0,-1,1) },
            {eSuperRegionType.South,        new Vector3(0,-1,0) },
            {eSuperRegionType.SouthWest,    new Vector3(0,-1,-1) },
            {eSuperRegionType.West,         new Vector3(0,0,-1) },
            {eSuperRegionType.NorthWest,    new Vector3(0,1,-1) }
        };

        //========================================================================================

        #region member variables

        [SerializeField]
        [HideInInspector]
        private Vector3 worldSize;

        [SerializeField]
        [HideInInspector]
        private float renderDistanceFar;

        [SerializeField]
        [HideInInspector]
        private float renderDistanceInactive;

        [SerializeField]
        [HideInInspector]
        private float preTeleportOffset;

        [SerializeField]
        [HideInInspector]
        private float secondaryPositionDistanceModifier;

        List<SuperRegion> superRegionsList = new List<SuperRegion>();
        List<SubSceneJob> subSceneJobsList = new List<SubSceneJob>();

        bool isInitialized = false;
        bool isJobRunning = false;

        int currentSuperRegionIndex;
        private Transform myTransform;

#if UNITY_EDITOR
        [SerializeField]
        [HideInInspector]
        private bool drawBounds;
#endif

        [SerializeField]
        [HideInInspector]
        private bool editorSubScenesLoaded;

        #endregion member variables 

        //========================================================================================

        #region properties

        public Vector3 WorldSize { get { return worldSize; } }
        public float RenderDistanceFar { get { return renderDistanceFar; } }
        public float RenderDistanceInactive { get { return renderDistanceInactive; } }
        public bool EditorSubScenesLoaded { get { return editorSubScenesLoaded; } }
        public float SecondaryPositionDistanceModifier { get { return secondaryPositionDistanceModifier; } }

        #endregion properties

        //========================================================================================

        #region monobehaviour methods

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!isInitialized)
            {
                Debug.Log("d");
                return;
            }

            var cameraTransform = Camera.main.transform;
            var cameraPos = cameraTransform.position;
            var teleportPositions = new List<Vector3>();
            var halfSize = worldSize * 0.5f;

            //***********************************************
            //identifying teleport positions
            if (cameraPos.y > halfSize.y - preTeleportOffset)
            {
                teleportPositions.Add(cameraPos + new Vector3(0, -worldSize.y, 0));
            }
            else if (cameraPos.y < -halfSize.y + preTeleportOffset)
            {
                teleportPositions.Add(cameraPos + new Vector3(0, worldSize.y, 0));
            }

            if (cameraPos.z > halfSize.z - preTeleportOffset)
            {
                teleportPositions.Add(cameraPos + new Vector3(0, 0, -worldSize.z));
            }
            else if (cameraPos.z < -halfSize.z + preTeleportOffset)
            {
                teleportPositions.Add(cameraPos + new Vector3(0, 0, worldSize.z));
            }

            //***********************************************
            //updating one super region, getting a list of new jobs
            var newJobs = superRegionsList[currentSuperRegionIndex].UpdateSuperRegion(cameraTransform, teleportPositions);

            currentSuperRegionIndex++;
            if (currentSuperRegionIndex == superRegionsList.Count)
            {
                currentSuperRegionIndex = 0;
            }

            //***********************************************
            //cleaning and updating the list of SubSceneJobs
            foreach (var job in newJobs)
            {
                eSubSceneJobType oppositeJobType = job.JobType == eSubSceneJobType.Load ? eSubSceneJobType.Unload : eSubSceneJobType.Load;

                int indexOfOppositeJob = subSceneJobsList.FindIndex(
                    item => item.Region.Id == job.Region.Id
                    && item.JobType == oppositeJobType
                    && item.SubSceneMode == job.SubSceneMode
                    && item.SubSceneType == job.SubSceneType
                );

                if (indexOfOppositeJob >= 0)
                {
                    subSceneJobsList.RemoveAt(indexOfOppositeJob);
                }
            }

            foreach (var job in newJobs) //this is temporarily in its own loop, will change when/if priorities are added
            {
                subSceneJobsList.Add(job);
            }

            //***********************************************
            //executing jobs
            if (!isJobRunning && subSceneJobsList.Count != 0)
            {
                var newJob = subSceneJobsList[0];
                subSceneJobsList.RemoveAt(0);

                switch (newJob.JobType)
                {
                    case eSubSceneJobType.Load:
                        StartCoroutine(LoadSubSceneCR(newJob));
                        break;
                    case eSubSceneJobType.Unload:
                        StartCoroutine(UnloadSubSceneCR(newJob));
                        break;
                }
            }
        } //end of Update()

#if UNITY_EDITOR
        private void OnValidate()
        {
            float part = renderDistanceFar * 0.2f;
            if (part < 1)
            {
                part = 1;
            }
            else if (part - (int)part > 0)
            {
                part = (int)part + 1;
            }

            if (renderDistanceInactive < renderDistanceFar + part)
            {
                renderDistanceInactive = renderDistanceFar + part;
            }

            if (preTeleportOffset < 1)
            {
                preTeleportOffset = 1;
            }

            if (secondaryPositionDistanceModifier < 0)
            {
                secondaryPositionDistanceModifier = 0;
            }
        }
#endif

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (drawBounds && Application.isEditor)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, worldSize);
            }
        }
#endif

        #endregion monobehaviour methods

        //========================================================================================

        #region public methods

        public void Initialize()
        {
            myTransform = transform;

            //find all the initial regions and clean them
            var initialRegions = GetComponentsInChildren<RegionBase>().ToList();

            //if the subScenes are loaded they are used instead of the saved files and the world is not duplicated
            if (editorSubScenesLoaded)
            {
                var superRegion = new GameObject(string.Concat("SuperRegion_", eSuperRegionType.Centre.ToString()), typeof(SuperRegion)).GetComponent<SuperRegion>();
                superRegion.transform.SetParent(transform);
                superRegion.transform.Translate(Vector3.Scale(worldSize, SUPERREGION_OFFSETS[eSuperRegionType.Centre]));

                foreach (var region in initialRegions)
                {
                    region.transform.SetParent(superRegion.transform, true);

                    //deactivating all subScenes
                    foreach (Transform child in region.transform)
                    {
                        if (child.GetComponent<SubScene>())
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }

                superRegion.Initialize(eSuperRegionType.Centre, this, initialRegions);
                superRegionsList.Add(superRegion);
            }
            //else if the subScenes are not loaded the world is duplicated and the initial (empty) regions are destroyed
            //create SuperRegions and clone the regions once for every SuperRegions
            else
            {
                //"cleaning" the initial regions, just in case
                foreach (var initialRegion in initialRegions)
                {
                    initialRegion.Clear();
                }

                //creating all the superRegions and duplicating the initial regions into them
                foreach (var superRegionType in Enum.GetValues(typeof(eSuperRegionType)).Cast<eSuperRegionType>())
                {
                    var superRegion = new GameObject(string.Concat("SuperRegion_", superRegionType.ToString()), typeof(SuperRegion)).GetComponent<SuperRegion>();
                    superRegion.transform.SetParent(transform);
                    superRegion.transform.Translate(Vector3.Scale(worldSize, SUPERREGION_OFFSETS[superRegionType]));


                    var clonedRegions = new List<RegionBase>();
                    foreach (var initialRegion in initialRegions)
                    {
                        var regionClone = Instantiate(initialRegion.gameObject, superRegion.transform, false);
                        regionClone.name = initialRegion.name;

                        clonedRegions.Add(regionClone.GetComponent<RegionBase>());
                    }

                    superRegion.Initialize(superRegionType, this, clonedRegions);
                    superRegionsList.Add(superRegion);
                }

                //destroy the initial regions
                foreach (var initialRegion in initialRegions)
                {
                    Destroy(initialRegion.gameObject);
                }
            }

            isInitialized = true;
        }

        #endregion public regions

        //========================================================================================

        #region private methods

        /// <summary>
        /// Runtime Coroutine that loads a subScene
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        private IEnumerator LoadSubSceneCR(SubSceneJob job)
        {
            isJobRunning = true;
            //Debug.LogFormat("Load Job started: {0} {1} {2}", job.Region.SuperRegion.Type, job.Region.name, job.SceneType);

            string sceneName = WorldUtility.GetSubSceneName(job.Region.Id, job.SubSceneMode, job.SubSceneType);
            var subSceneRoot = job.Region.GetSubSceneRoot(job.SubSceneType);

            //editor subScenes are loaded (no streaming)
            if (editorSubScenesLoaded)
            {
                if (subSceneRoot)
                {
                    subSceneRoot.gameObject.SetActive(true);
                }
            }
            //streaming
            else
            {
                if (subSceneRoot)
                {
                    Debug.LogWarningFormat("Load Job for existing subScene started! region=\"{0}\", subScene=\"{1}\"", job.Region.name, job.SubSceneMode.ToString());
                }
                else if (!Application.CanStreamedLevelBeLoaded(sceneName))
                {
                    Debug.LogWarningFormat("scene {0} cannot be streamed", sceneName);
                    var root = new GameObject("empty").transform;
                    root.SetParent(job.Region.transform);
                    job.SubSceneRoot = root;
                }
                else
                {
                    AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                    while (!async.isDone)
                    {
                        yield return null;
                    }

                    Scene scene = SceneManager.GetSceneByName(sceneName);
                    var root = scene.GetRootGameObjects()[0].transform;
                    SceneManager.MoveGameObjectToScene(root.gameObject, gameObject.scene);
                    root.SetParent(job.Region.transform, false);
                    job.SubSceneRoot = root;

                    async = SceneManager.UnloadSceneAsync(sceneName);

                    while (!async.isDone)
                    {
                        yield return null;
                    }
                }
            }

            job.IsJobSuccessful = true;
            job.Callback(job);

            //Debug.Log("Load Job done");
            isJobRunning = false;
        }

        /// <summary>
        /// Runtime Coroutine that unloads a subScene.
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        private IEnumerator UnloadSubSceneCR(SubSceneJob job)
        {
            isJobRunning = true;
            //Debug.LogFormat("Unload Job started: {0} {1} {2}", job.Region.SuperRegion.Type, job.Region.name, job.SceneType);

            var subSceneRoot = job.Region.GetSubSceneRoot(job.SubSceneType, job.SubSceneMode);

            //editor subScenes are loaded (no streaming)
            if (editorSubScenesLoaded)
            {
                if (subSceneRoot)
                {
                    subSceneRoot.gameObject.SetActive(false);
                }
            }
            //streaming
            else
            {
                if (subSceneRoot)
                {
                    Destroy(subSceneRoot.gameObject);
                }
            }

            //if (job.SubSceneRoot != null)
            //{
            //Destroy(job.SubSceneRoot.gameObject);

            //UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.CreateScene("destroyer");
            //var root = job.SubSceneRoot;
            //root.SetParent(null, true);
            //UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root.gameObject, scene);

            //AsyncOperation async = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);

            //while (!async.isDone)
            //{
            //    yield return null;
            //}                
            //}

            yield return null;
            //Resources.UnloadUnusedAssets();

            job.IsJobSuccessful = true;
            job.Callback(job);

            //Debug.Log("Unload Job done");
            isJobRunning = false;
        }

#if UNITY_EDITOR
        void IWorldEventHandler.ImportSubScenes()
        {
            if (editorSubScenesLoaded)
            {
                return;
            }

            var regions = new List<RegionBase>();

            for (int i = 0; i < transform.childCount; i++)
            {
                var region = transform.GetChild(i).GetComponent<RegionBase>();

                if (region)
                {
                    regions.Add(region);
                }
            }

            foreach (var region in regions)
            {
                foreach (var subSceneType in Enum.GetValues(typeof(eSubSceneType)).Cast<eSubSceneType>())
                {
                    foreach (var subSceneMode in region.AvailableSubSceneModes)
                    {
                        if (region.GetSubSceneRoot(subSceneType, subSceneMode) != null)
                        {
                            Debug.LogErrorFormat("The \"{0}\" of Region \"{1}\" is already loaded!", WorldUtility.GetSubSceneRootName(subSceneMode, subSceneType), region.name);
                            continue;
                        }

                        //paths
                        string subScenePath = WorldUtility.GetSubScenePath(gameObject.scene.path, region.Id, subSceneMode, subSceneType);
                        string subScenePathFull = WorldUtility.GetFullPath(subScenePath);

                        Scene subScene = new Scene();

                        if (System.IO.File.Exists(subScenePathFull))
                        {
                            subScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(subScenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                        }

                        //move subScene content to open world scene
                        if (subScene.IsValid())
                        {
                            var rootGO = subScene.GetRootGameObjects()[0];
                            UnityEditor.SceneManagement.EditorSceneManager.MoveGameObjectToScene(rootGO, this.gameObject.scene);

                            var root = rootGO.transform;
                            root.SetParent(region.transform, false);

                            if (!root.gameObject.activeSelf)
                            {
                                root.gameObject.SetActive(true);
                            }
                        }

                        //end: close subScene
                        UnityEditor.SceneManagement.EditorSceneManager.CloseScene(subScene, true);
                    }
                }
            }


            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            editorSubScenesLoaded = true;
        }
#endif

#if UNITY_EDITOR
        void IWorldEventHandler.ExportSubScenes()
        {
            if (!editorSubScenesLoaded)
            {
                return;
            }

            //clear subScene folder
            ((IWorldEventHandler)this).ClearSubSceneFolder();

            //create folder, in case it does not exist
            string scenePath = gameObject.scene.path;
            string folderPath = scenePath.Remove(scenePath.LastIndexOf('.'));
            string parentFolderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
            if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
            {
                UnityEditor.AssetDatabase.CreateFolder(parentFolderPath, gameObject.scene.name);
            }

            //finding all the regions
            var regions = new List<RegionBase>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var region = transform.GetChild(i).GetComponent<RegionBase>();

                if (region != null)
                {
                    regions.Add(region);
                }
            }

            foreach (var region in regions)
            {
                foreach (var subSceneType in Enum.GetValues(typeof(eSubSceneType)).Cast<eSubSceneType>())
                {
                    foreach (var subSceneMode in region.AvailableSubSceneModes)
                    {
                        var root = region.GetSubSceneRoot(subSceneType, subSceneMode);

                        if (!root || root.childCount == 0) //if root is null or empty there is no need to create a subScene
                        {
                            continue;
                        }

                        //paths
                        string subScenePath = WorldUtility.GetSubScenePath(gameObject.scene.path, region.Id, subSceneMode, subSceneType);
                        //string subScenePathFull = WorldUtility.GetFullPath(subScenePath);

                        //moving copy to subScene
                        root.SetParent(null, false);
                        var subScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Additive);
                        UnityEditor.SceneManagement.EditorSceneManager.MoveGameObjectToScene(root.gameObject, subScene);

                        //saving and closing the sub scene
                        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(subScene, subScenePath);
                        UnityEditor.SceneManagement.EditorSceneManager.CloseScene(subScene, true);

                        //add subScene to buildsettings
                        var buildSettingsScenes = UnityEditor.EditorBuildSettings.scenes.ToList();
                        buildSettingsScenes.Add(new UnityEditor.EditorBuildSettingsScene(subScenePath, true));
                        UnityEditor.EditorBuildSettings.scenes = buildSettingsScenes.ToArray();

                        // if (root.childCount != 0)
                        // {
                        //     foreach (var superRegionType in Enum.GetValues(typeof(eSuperRegionType)).Cast<eSuperRegionType>())
                        //     {
                        //         //duplicating
                        //         var rootCopyGO = Instantiate(root.gameObject, root.position, Quaternion.identity);
                        //         rootCopyGO.name = root.name;
                        //         rootCopyGO.transform.Translate(Vector3.Scale(worldSize, SUPERREGION_OFFSETS[superRegionType]));
                        //     }
                        // }
                    }
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            editorSubScenesLoaded = false;
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Deletes the SubScene folder and its content and removes the subScenes from the build settings, but only if the subScenes have been loaded first.
        /// </summary>
        void IWorldEventHandler.ClearSubSceneFolder()
        {
            if (!editorSubScenesLoaded)
            {
                return;
            }

            string worldScenePath = gameObject.scene.path;
            string worldSceneFolderPath = worldScenePath.Remove(worldScenePath.LastIndexOf('.'));

            //cleaning build settings
            var scenes = UnityEditor.EditorBuildSettings.scenes.ToList();
            var scenesToRemove = new List<UnityEditor.EditorBuildSettingsScene>();
            string pathPart = "/SubScene_"; //string.Concat(worldSceneFolderPath, "/SubScene_");

            foreach (var sceneEntry in scenes)
            {
                if (sceneEntry.path.Contains(pathPart) || string.IsNullOrEmpty(sceneEntry.path))
                {
                    scenesToRemove.Add(sceneEntry);
                }
            }

            foreach (var sceneEntry in scenesToRemove)
            {
                scenes.Remove(sceneEntry);
            }

            UnityEditor.EditorBuildSettings.scenes = scenes.ToArray();

            //deleting subScene folder (of the current scene only)
            UnityEditor.FileUtil.DeleteFileOrDirectory(worldSceneFolderPath);
            UnityEditor.AssetDatabase.Refresh();
        }
#endif
        #endregion editor methods

        //========================================================================================
    }
} //end of namespace