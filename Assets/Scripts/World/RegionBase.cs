﻿using Game.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.World
{
    [RequireComponent(typeof(UniqueId))]
    public abstract class RegionBase : MonoBehaviour, IRegionEventHandler
    {
        //========================================================================================

        #region member variables

        [SerializeField]
        [HideInInspector]
        private Vector3 boundsSize;

        [SerializeField]
        [HideInInspector]
        private bool overrideRenderDistanceFar;

        [SerializeField]
        [HideInInspector]
        private bool overrideRenderDistanceInactive;

        [SerializeField]
        [HideInInspector]
        private float localRenderDistanceFar;

        [SerializeField]
        [HideInInspector]
        private float localRenderDistanceInactive;

        private Transform myTransform;
        private SuperRegion superRegion;
        private UniqueId uniqueId;

        private bool isInitialized;
        protected eSubSceneState[] subSceneStates = new eSubSceneState[Enum.GetValues(typeof(eSubSceneType)).Length];
        private eRegionMode currentRegionMode = eRegionMode.Inactive;
        private eSubSceneMode currentSubSceneMode;
        private bool hasSubSceneModeChanged = false;

        private List<Vector3> boundsCorners;

#if UNITY_EDITOR
        [SerializeField]
        [HideInInspector]
        bool drawBounds;
#endif

        #endregion member variables

        //========================================================================================

        #region properties

        public Bounds Bounds { get { return new Bounds(transform.position, boundsSize); } }

        public string Id { get { if (!uniqueId) { uniqueId = GetComponent<UniqueId>(); } return uniqueId.Id; } }

        public SuperRegion SuperRegion { get { return superRegion; } }

        public float RenderDistanceFar { get { return overrideRenderDistanceFar ? localRenderDistanceFar : superRegion.World.RenderDistanceFar; } }

        public float RenderDistanceInactive { get { return overrideRenderDistanceInactive ? localRenderDistanceInactive : superRegion.World.RenderDistanceInactive; } }

        public eSubSceneMode CurrentSubSceneMode { get { return currentSubSceneMode; } }

        #endregion properties

        //========================================================================================

        #region abstract

        public abstract List<eSubSceneMode> AvailableSubSceneModes { get; }

        protected abstract eSubSceneMode InitialSubSceneMode { get; }

        #endregion abstract

        //========================================================================================

        #region monobehaviour methods

        // #if UNITY_EDITOR
        //         private void Awake()
        //         {
        //             if(instanceId == 0)
        //             {
        //                 instanceId = GetInstanceID();

        //                 if (string.IsNullOrEmpty(id))
        //                 {
        //                     id = Guid.NewGuid().ToString();
        //                 }

        //                 //"save" changes
        //                 UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        //             }
        //             else if(!Application.isPlaying && instanceId != GetInstanceID())
        //             {
        //                 instanceId = GetInstanceID();
        //                 id = Guid.NewGuid().ToString();

        //                 //"save" changes
        //                 UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        //             }
        //         }
        // #endif

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            float part = localRenderDistanceFar * 0.2f;
            if (part < 1)
            {
                part = 1;
            }
            else if (part - (int)part > 0)
            {
                part = (int)part + 1;
            }

            if (localRenderDistanceInactive < localRenderDistanceFar + part)
            {
                localRenderDistanceInactive = localRenderDistanceFar + part;
            }
        }
#endif

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (drawBounds && Application.isEditor)
            {
                Gizmos.color = Color.green;
                var bounds = Bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
#endif

        #endregion monobehaviour methods

        //========================================================================================

        #region public methods

        /// <summary>
        /// Destroys ALL children of the Region.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        public virtual void Initialize(SuperRegion superRegion)
        {
            if (isInitialized)
            {
                return;
            }

            myTransform = transform;
            this.superRegion = superRegion;

            currentSubSceneMode = InitialSubSceneMode;

            //initializing SubScene states array
            var subScenes = GetAllSubScenes();
            foreach (var subSceneType in Enum.GetValues(typeof(eSubSceneType)).Cast<eSubSceneType>())
            {
                int index = (int)subSceneType;
                subSceneStates[index] = eSubSceneState.Unloaded;

                //if (subScenes.FirstOrDefault(item => item.SubSceneType == subSceneType && item.SubSceneMode == currentSubSceneMode))
                //{
                //    subSceneStates[index] = eSubSceneState.Loaded;
                //}
            }

            //computing corner positions
            Bounds bounds = new Bounds(myTransform.position, boundsSize);
            boundsCorners = new List<Vector3>(){
                new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z),
                new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z),
                new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z),
                new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z),
                new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z),
                new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z),
                new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z),
                new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z)
            };

            isInitialized = true;
        }

        public List<SubSceneJob> UpdateRegion(Transform cameraTransform, List<Vector3> teleportPositions)
        {
            if (!isInitialized)
            {
                return null;
            }

            Bounds bounds = new Bounds(myTransform.position, boundsSize);
            Vector3 cameraPosition = cameraTransform.position;
            var SubScenes = GetAllSubScenes();

            bool isVisible = false;
            var result = new List<SubSceneJob>();

            //handling SubScene mode change
            if (hasSubSceneModeChanged)
            {
                Debug.Log("Region \"" + name + "\": mode has changed!");
                foreach (var subScene in SubScenes)
                {
                    if (subScene.SubSceneMode != currentSubSceneMode)
                    {
                        result.Add(CreateUnloadSubSceneJob(subScene.SubSceneMode, subScene.SubSceneType));
                    }
                }

                for (int i = 0; i < subSceneStates.Length; i++)
                {
                    if (subSceneStates[i] == eSubSceneState.Loading)
                    {
                        foreach (var value in Enum.GetValues(typeof(eSubSceneMode)).Cast<eSubSceneMode>())
                        {
                            result.Add(CreateUnloadSubSceneJob(value, (eSubSceneType)i));
                        }
                    }
                }

                for (int i = 0; i < subSceneStates.Length; i++)
                {
                    subSceneStates[i] = eSubSceneState.Unloaded;
                }

                hasSubSceneModeChanged = false;
            }

            //check if visible
            foreach (var corner in boundsCorners)
            {
                Vector3 vectorToCorner = corner - cameraPosition;

                if (Vector3.Angle(vectorToCorner, cameraTransform.forward) < 90)
                {
                    isVisible = true;
                    break;
                }
            }

            //compute distance and switch mode
            if (!isVisible)
            {
                //Debug.Log("Region \"" + name + "\": is not visible!");
                if (currentRegionMode != eRegionMode.Inactive)
                {
                    result = SwitchMode(eRegionMode.Inactive);
                }
            }
            else
            {
                float distance = float.MaxValue;

                if (bounds.Contains(cameraPosition))
                {
                    distance = 0;
                }
                else
                {
                    distance = (bounds.ClosestPoint(cameraPosition) - cameraPosition).magnitude;

                    foreach (var teleportPosition in teleportPositions)
                    {
                        if (bounds.Contains(teleportPosition))
                        {
                            distance = 0;
                            break;
                        }
                        else
                        {
                            float dist = (bounds.ClosestPoint(teleportPosition) - teleportPosition).magnitude;
                            dist *= superRegion.World.SecondaryPositionDistanceModifier;

                            if (dist < distance)
                            {
                                distance = dist;
                            }
                        }
                    }
                }

                switch (currentRegionMode)
                {
                    case eRegionMode.Near:
                        if (distance > RenderDistanceInactive * 1.1f)
                        {
                            result = SwitchMode(eRegionMode.Inactive);
                        }
                        else if (distance > RenderDistanceFar * 1.1f)
                        {
                            result = SwitchMode(eRegionMode.Far);
                        }
                        break;
                    case eRegionMode.Far:
                        if (distance > RenderDistanceInactive * 1.1f)
                        {
                            result = SwitchMode(eRegionMode.Inactive);
                        }
                        else if (distance < RenderDistanceFar)
                        {
                            result = SwitchMode(eRegionMode.Near);
                        }
                        break;
                    case eRegionMode.Inactive:
                        if (distance < RenderDistanceFar)
                        {
                            result = SwitchMode(eRegionMode.Near);
                        }
                        else if (distance < RenderDistanceInactive)
                        {
                            result = SwitchMode(eRegionMode.Far);
                        }
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a list with the Transforms of all the SubScenes.
        /// </summary>
        /// <returns></returns>
        public List<Transform> GetAllSubSceneRoots()
        {
            var result = new List<Transform>();

            foreach (Transform child in (isInitialized ? myTransform : transform))
            {
                if (child.GetComponent<SubScene>())
                {
                    result.Add(child);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a list of all the SubScenes.
        /// </summary>
        /// <returns></returns>
        public List<SubScene> GetAllSubScenes()
        {
            var result = new List<SubScene>();

            foreach (Transform child in (isInitialized ? myTransform : transform))
            {
                var subScene = child.GetComponent<SubScene>();
                if (subScene)
                {
                    result.Add(subScene);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the Transform of the SubScene of current mode and chosen type.
        /// </summary>
        /// <param name="subSceneType"></param>
        /// <param name="subSceneList"></param>
        /// <returns></returns>
        public Transform GetSubSceneRoot(eSubSceneType subSceneType, List<SubScene> subSceneList = null)
        {
            if (subSceneList == null)
            {
                subSceneList = GetAllSubScenes();
            }

            foreach (var subScene in subSceneList)
            {
                if (subScene.SubSceneMode == currentSubSceneMode && subScene.SubSceneType == subSceneType)
                {
                    return subScene.transform;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the Transform of the SubScene of chosen mode and type.
        /// </summary>
        /// <param name="subSceneType"></param>
        /// <param name="subSceneMode"></param>
        /// <param name="subSceneList"></param>
        /// <returns></returns>
        public Transform GetSubSceneRoot(eSubSceneType subSceneType, eSubSceneMode subSceneMode, List<SubScene> subSceneList = null)
        {
            if (subSceneList == null)
            {
                subSceneList = GetAllSubScenes();
            }

            foreach (var subScene in subSceneList)
            {
                if (subScene.SubSceneMode == subSceneMode && subScene.SubSceneType == subSceneType)
                {
                    return subScene.transform;
                }
            }

            return null;
        }

        #endregion public methods       

        //========================================================================================

        protected void ChangeSubSceneMode(eSubSceneMode newMode)
        {
            if (newMode != currentSubSceneMode && AvailableSubSceneModes.Contains(newMode))
            {
                currentSubSceneMode = newMode;
                hasSubSceneModeChanged = true;
            }
        }

        //========================================================================================

        #region private methods      

        private List<SubSceneJob> SwitchMode(eRegionMode newMode)
        {
            var result = new List<SubSceneJob>();

            switch (newMode)
            {
                case eRegionMode.Near:
                    //load
                    result.Add(CreateLoadSubSceneJob(currentSubSceneMode, eSubSceneType.Always));
                    result.Add(CreateLoadSubSceneJob(currentSubSceneMode, eSubSceneType.Near));

                    //unload
                    result.Add(CreateUnloadSubSceneJob(currentSubSceneMode, eSubSceneType.Far));

                    break;
                case eRegionMode.Far:
                    //load
                    result.Add(CreateLoadSubSceneJob(currentSubSceneMode, eSubSceneType.Always));
                    result.Add(CreateLoadSubSceneJob(currentSubSceneMode, eSubSceneType.Far));

                    //unload
                    result.Add(CreateUnloadSubSceneJob(currentSubSceneMode, eSubSceneType.Near));

                    break;
                case eRegionMode.Inactive:
                    //unload
                    result.Add(CreateUnloadSubSceneJob(currentSubSceneMode, eSubSceneType.Always));
                    result.Add(CreateUnloadSubSceneJob(currentSubSceneMode, eSubSceneType.Far));
                    result.Add(CreateUnloadSubSceneJob(currentSubSceneMode, eSubSceneType.Near));
                    break;
            }

            currentRegionMode = newMode;

            result.RemoveAll(item => item == null);
            return result;
        }

        private SubSceneJob CreateLoadSubSceneJob(eSubSceneMode subSceneMode, eSubSceneType subSceneType)
        {
            int index = (int)subSceneMode;
            eSubSceneState state = subSceneStates[index];

            //if (state == eSubSceneState.Loaded || state == eSubSceneState.Loading)
            //{
            //    return null;
            //}
            //else
            //{
                subSceneStates[index] = eSubSceneState.Loading;
                return new SubSceneJob(this, subSceneMode, subSceneType, eSubSceneJobType.Load, OnSubSceneJobDone);
            //}
        }

        private SubSceneJob CreateUnloadSubSceneJob(eSubSceneMode subSceneMode, eSubSceneType subSceneType)
        {
            int index = (int)subSceneMode;
            eSubSceneState state = subSceneStates[index];

            //if (state == eSubSceneState.Unloaded || state == eSubSceneState.Unloading)
            //{
            //    return null;
            //}
            //else
            //{
                subSceneStates[index] = eSubSceneState.Unloading;
                return new SubSceneJob(this, subSceneMode, subSceneType, eSubSceneJobType.Unload, OnSubSceneJobDone);
            //}
        }

        private void OnSubSceneJobDone(SubSceneJob subSceneJob)
        {
            if (subSceneJob.SubSceneMode != currentSubSceneMode)
            {
                return;
            }

            int index = (int)subSceneJob.SubSceneMode;

            if (subSceneJob.JobType == eSubSceneJobType.Load)
            {
                if (subSceneStates[index] == eSubSceneState.Loading)
                {
                    subSceneStates[index] = eSubSceneState.Loaded;
                }
            }
            else if (subSceneJob.JobType == eSubSceneJobType.Unload)
            {
                if (subSceneStates[index] == eSubSceneState.Unloading)
                {
                    subSceneStates[index] = eSubSceneState.Unloaded;
                }
            }
        }

#if UNITY_EDITOR
        void IRegionEventHandler.CreateSubScene(eSubSceneMode subSceneMode, eSubSceneType subSceneType)
        {
            string subScenePath = WorldUtility.GetSubScenePath(gameObject.scene.path, Id, subSceneMode, subSceneType);
            string subScenePathFull = WorldUtility.GetFullPath(subScenePath);

            if (GetSubSceneRoot(subSceneType) != null)
            {
                return;
            }
            else if (System.IO.File.Exists(subScenePathFull))
            {
                Debug.LogFormat("SubScene \"{0}\" already exists but is not loaded!", subScenePath);
                return;
            }
            else
            {
                var rootGO = new GameObject(WorldUtility.GetSubSceneRootName(subSceneMode, subSceneType), typeof(SubScene));
                rootGO.GetComponent<SubScene>().Initialize(subSceneMode, subSceneType);

                var root = rootGO.transform;
                root.SetParent(transform, false);
            }
        }
#endif

        //#if UNITY_EDITOR
        //        private void CreateSubScenes()
        //        {
        //            foreach (var subSceneType in SubSceneTypes)
        //            {
        //                string subScenePath = WorldUtility.GetSubScenePath(gameObject.scene.path, Id, subSceneType);
        //                string subScenePathFull = WorldUtility.GetFullPath(subScenePath);

        //                if (GetSubSceneRoot(subSceneType) != null)
        //                {
        //                    continue;
        //                }
        //                else if (System.IO.File.Exists(subScenePathFull))
        //                {
        //                    Debug.LogFormat("SubScene \"{0}\" already exists but is not loaded!", subScenePath);
        //                    continue;
        //                }
        //                else
        //                {
        //                    var rootGO = new GameObject(WorldUtility.GetSubSceneRootName(subSceneType), typeof(SubScene));
        //                    rootGO.GetComponent<SubScene>().SubSceneType = subSceneType;

        //                    var root = rootGO.transform;
        //                    root.SetParent(transform, false);
        //                }
        //            }
        //        }
        //#endif

#endregion private methods

        //========================================================================================

        //         #region editor methods

        // #if UNITY_EDITOR

        //         /// <summary>
        //         /// Helper methods that checks whether the id of the region is unique or not.
        //         /// </summary>
        //         /// <returns></returns>
        //         public bool IsRegionIdUnique()
        //         {
        //             Transform parentTransform = transform.parent;
        //             string regionId = GetComponent<RegionBase>().UniqueId;
        //             int occurences = 0;

        //             for (int i = 0; i < parentTransform.childCount; i++)
        //             {
        //                 var child = parentTransform.GetChild(i).GetComponent<RegionBase>();

        //                 if (child != null && child.UniqueId == regionId)
        //                 {
        //                     occurences++;
        //                 }
        //             }

        //             if (occurences > 1)
        //             {
        //                 return false;
        //             }
        //             else
        //             {
        //                 return true;
        //             }
        //         }

        // #endif

        //         #endregion editor methods

        //========================================================================================
    }
} //end of namespace