using Game.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.World
{
    public class World : MonoBehaviour
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
        private float renderDistanceFar = 5;

        [SerializeField]
        [HideInInspector]
        private float renderDistanceInactive = 10;

        List<SuperRegion> superRegions = new List<SuperRegion>();
        Queue<SubSceneJob> subSceneJobs = new Queue<SubSceneJob>();

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

        public Vector3 WorldSize { get { return worldSize; } }
        public float RenderDistanceFar { get { return renderDistanceFar; } }
        public float RenderDistanceInactive { get { return renderDistanceInactive; } }

#if UNITY_EDITOR

        public bool SubScenesLoaded { get { return editorSubScenesLoaded; } }

#endif

        //========================================================================================

        #region runtime methods

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            myTransform = transform;

            //find all the regions and clean them
            var initialRegions = GetComponentsInChildren<RegionBase>().ToList();

            foreach (var initialRegion in initialRegions)
            {
                initialRegion.Clear();
            }

            //if the subScenes are loaded they are used instead of the saved files and the world is not duplicated
            if (editorSubScenesLoaded)
            {
                var superRegion = new GameObject(string.Concat("SuperRegion_", eSuperRegionType.Centre.ToString()), typeof(SuperRegion)).GetComponent<SuperRegion>();
                superRegion.transform.SetParent(transform);
                superRegion.transform.Translate(Vector3.Scale(worldSize, SUPERREGION_OFFSETS[eSuperRegionType.Centre]));

                foreach (var region in initialRegions)
                {
                    region.transform.SetParent(superRegion.transform, true);
                }

                superRegion.Initialize(eSuperRegionType.Centre, this, initialRegions);
                superRegions.Add(superRegion);
            }
            //else if the subScenes are not loaded the world is duplicated and the initial (empty) regions are destroyed
            //create SuperRegions and clone the regions once for every SuperRegions
            else
            {
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
                    superRegions.Add(superRegion);
                }

                //destroy the initial regions
                foreach (var initialRegion in initialRegions)
                {
                    Destroy(initialRegion.gameObject);
                }
            }

            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized)
            {
                Debug.Log("d");
                return;
            }

            var cameraPos = Camera.main.transform.position;
            var cameraPositions = new List<Vector3>() { cameraPos };

            var halfSize = worldSize * 0.5f;
            int offset = 1;

            if (cameraPos.x > halfSize.x - offset)
            {
                cameraPositions.Add(cameraPos + new Vector3(-worldSize.x, 0, 0));
            }
            else if (cameraPos.x < -halfSize.x + offset)
            {
                cameraPositions.Add(cameraPos + new Vector3(worldSize.x, 0, 0));
            }

            if (cameraPos.y > halfSize.y - offset)
            {
                cameraPositions.Add(cameraPos + new Vector3(0, -worldSize.y, 0));
            }
            else if (cameraPos.y < -halfSize.y + offset)
            {
                cameraPositions.Add(cameraPos + new Vector3(0, worldSize.y, 0));
            }

            if (cameraPos.z > halfSize.z - offset)
            {
                cameraPositions.Add(cameraPos + new Vector3(0, 0, -worldSize.z));
            }
            else if (cameraPos.z < -halfSize.z + offset)
            {
                cameraPositions.Add(cameraPos + new Vector3(0, 0, worldSize.z));
            }

            string positions = "";
            foreach (var pos in cameraPositions)
            {
                positions += pos.ToString() + " ";
            }
            //Debug.LogFormat("World: cameraPositions = {0}", positions);

            var newJobs = superRegions[currentSuperRegionIndex].UpdateSuperRegion(cameraPositions);
            if (newJobs.Count != 0)
            {
                Debug.Log("SuperRegion " + superRegions[currentSuperRegionIndex] + ": " + newJobs.Count + " new jobs");
            }

            foreach (var job in newJobs)
            {
                subSceneJobs.Enqueue(job);
            }

            //Debug.Log("currentSuperRegionIndex=" + currentSuperRegionIndex);
            currentSuperRegionIndex++;
            if (currentSuperRegionIndex == superRegions.Count)
            {
                currentSuperRegionIndex = 0;
            }

            if (!isJobRunning && subSceneJobs.Count != 0)
            {
                //Debug.Log("jobCount = " + subSceneJobs.Count);
                var newJob = subSceneJobs.Dequeue();

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
        }

        private IEnumerator LoadSubSceneCR(SubSceneJob job)
        {
            Debug.LogFormat("Load Job started: {0} {1} {2}", job.Region.SuperRegion.Type, job.Region.name, job.SceneType);
            isJobRunning = true;

            string sceneName = WorldUtility.GetSubSceneName(job.Region.SuperRegion.Type, job.Region.UniqueId, job.SceneType);

            if (job.Region.GetSubSceneRoot(job.SceneType))
            {
                Debug.LogWarningFormat("Load Job for existing subScene started! region=\"{0}\", subScene=\"{1}\"", job.Region.name, job.SceneType.ToString());
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarningFormat("scene {0} cannot be streamed", sceneName);
                var root = new GameObject("empty").transform;
                root.SetParent(job.Region.transform);
                job.SubSceneRoot = root;
            }
            else
            {
                AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);

                while (!async.isDone)
                {
                    yield return null;
                }

                UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
                var root = scene.GetRootGameObjects()[0].transform;
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root.gameObject, this.gameObject.scene);
                root.SetParent(job.Region.transform);
                job.SubSceneRoot = root;

                async = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);

                while (!async.isDone)
                {
                    yield return null;
                }
            }

            job.IsJobSuccessful = true;
            job.Callback(job);
            isJobRunning = false;
            Debug.Log("Load Job done");
        }

        private IEnumerator UnloadSubSceneCR(SubSceneJob job)
        {
            Debug.LogFormat("Unload Job started: {0} {1} {2}", job.Region.SuperRegion.Type, job.Region.name, job.SceneType);
            isJobRunning = true;

            var subSceneRoot = job.Region.GetSubSceneRoot(job.SceneType);

            if (subSceneRoot)
            {
                Destroy(subSceneRoot.gameObject);
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
            Resources.UnloadUnusedAssets();

            job.IsJobSuccessful = true;
            job.Callback(job);
            isJobRunning = false;
            Debug.Log("Unload Job done");
        }

        #endregion runtime regions

        //========================================================================================

        #region editor methods
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
        }

        private void OnDrawGizmos()
        {
            if (drawBounds && Application.isEditor)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, worldSize);
            }
        }

        public void ImportSubScenes()
        {
            if (editorSubScenesLoaded)
            {
                return;
            }

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
                if (!region.IsRegionIdUnique())
                {
                    Debug.LogErrorFormat("Several Regions have the same Id as Region \"{0}\"", region.name);
                    continue;
                }

                foreach (var subSceneType in region.SubSceneTypes)
                {
                    if (region.GetSubSceneRoot(subSceneType) != null)
                    {
                        Debug.LogErrorFormat("The SubScene \"{0}\" of Region \"{1}\" is already loaded!", subSceneType.ToString(), region.name);
                        continue;
                    }

                    //paths
                    string subScenePath = WorldUtility.GetSubScenePath(gameObject.scene.path, eSuperRegionType.Centre, region.UniqueId, subSceneType);
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
                        root.SetParent(region.transform);

                        if (!root.gameObject.activeSelf)
                        {
                            root.gameObject.SetActive(true);
                        }
                    }

                    //end: close subScene
                    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(subScene, true);
                }
            }


            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            editorSubScenesLoaded = true;
        }

        public void ExportSubScenes()
        {
            if (!editorSubScenesLoaded)
            {
                return;
            }

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
                if (!region.IsRegionIdUnique())
                {
                    Debug.LogErrorFormat("Several Regions have the same Id as Region \"{0}\"", region.name);
                    continue;
                }

                foreach (var subSceneType in region.SubSceneTypes)
                {
                    if (region.GetSubSceneRoot(subSceneType) == null)
                    {
                        continue;
                    }

                    var root = region.GetSubSceneRoot(subSceneType);

                    if (root.childCount != 0)
                    {
                        foreach (var superRegionType in Enum.GetValues(typeof(eSuperRegionType)).Cast<eSuperRegionType>())
                        {
                            //paths
                            string subScenePath = WorldUtility.GetSubScenePath(gameObject.scene.path, superRegionType, region.UniqueId, subSceneType);
                            //string subScenePathFull = WorldUtility.GetFullPath(subScenePath);

                            //duplicating
                            var rootCopyGO = Instantiate(root.gameObject, root.position, Quaternion.identity);
                            rootCopyGO.name = root.name;
                            rootCopyGO.transform.Translate(Vector3.Scale(worldSize, SUPERREGION_OFFSETS[superRegionType]));

                            //moving copy to subScene
                            var subScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Additive);
                            UnityEditor.SceneManagement.EditorSceneManager.MoveGameObjectToScene(rootCopyGO, subScene);

                            //saving and closing the sub scene
                            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(subScene, subScenePath);
                            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(subScene, true);

                            var buildSettingsScenes = UnityEditor.EditorBuildSettings.scenes.ToList();
                            buildSettingsScenes.Add(new UnityEditor.EditorBuildSettingsScene(subScenePath, true));
                            UnityEditor.EditorBuildSettings.scenes = buildSettingsScenes.ToArray();
                        }
                    }

                    //destroying original root
                    DestroyImmediate(root.gameObject);
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            editorSubScenesLoaded = false;
        }

        /// <summary>
        /// Deletes the SubScene folder and its content and removes the subScenes from the build settings, but only if the subScenes have been loaded first.
        /// </summary>
        public void ClearSubSceneFolder()
        {
            if (!editorSubScenesLoaded)
            {
                return;
            }

            string worldScenePath = gameObject.scene.path;
            string worldSceneFolderPath = worldScenePath.Remove(worldScenePath.LastIndexOf('.'));

            var scenes = UnityEditor.EditorBuildSettings.scenes.ToList();
            var scenesToRemove = new List<UnityEditor.EditorBuildSettingsScene>();
            string pathPart = string.Concat(worldSceneFolderPath, "/SubScene_");

            foreach (var sceneEntry in scenes)
            {
                if (sceneEntry.path.Contains(pathPart))
                {
                    scenesToRemove.Add(sceneEntry);
                }
            }

            foreach (var sceneEntry in scenesToRemove)
            {
                scenes.Remove(sceneEntry);
            }

            UnityEditor.EditorBuildSettings.scenes = scenes.ToArray();

            UnityEditor.FileUtil.DeleteFileOrDirectory(worldSceneFolderPath);
        }

#endif
        #endregion editor methods

        //========================================================================================
    }
} //end of namespace