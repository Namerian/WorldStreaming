using Game.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.World
{
    [RequireComponent(typeof(UniqueId))]
    public abstract class RegionBase : MonoBehaviour
    {
        public enum eRegionMode
        {
            Near,
            Far,
            Inactive
        }

        public enum eSubSceneState
        {
            Unloaded,
            Loading,
            Loaded,
            Unloading
        }

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
        protected eSubSceneState[] subSceneStates = new eSubSceneState[Enum.GetValues(typeof(eSubSceneType)).Length];
        protected Transform[] subSceneRoots = new Transform[Enum.GetValues(typeof(eSubSceneType)).Length];
        private eRegionMode currentMode = eRegionMode.Inactive;
        private bool isInitialized;
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

        public string Id {
            get {
                if(!uniqueId){
                    uniqueId = GetComponent<UniqueId>();
                }
                return uniqueId.Id; 
            } 
        }

        public SuperRegion SuperRegion { get { return superRegion; } }

        public float RenderDistanceFar { get { if (overrideRenderDistanceFar) { return localRenderDistanceFar; } else { return superRegion.World.RenderDistanceFar; } } }

        public float RenderDistanceInactive { get { if (overrideRenderDistanceInactive) { return localRenderDistanceInactive; } else { return superRegion.World.RenderDistanceInactive; } } }

        #endregion properties

        //========================================================================================

        #region abstract

        public abstract List<eSubSceneType> SubSceneTypes { get; }

        protected abstract eSubSceneType GetSubSceneType(eSubSceneBaseType baseType);

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
        private void OnValidate()
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
            if(isInitialized){
                return;
            }

            myTransform = transform;
            this.superRegion = superRegion;

            foreach (var value in Enum.GetValues(typeof(eSubSceneType)).Cast<eSubSceneType>())
            {
                int index = (int)value;
                subSceneStates[index] = subSceneRoots[index] ? eSubSceneState.Loaded : eSubSceneState.Unloaded;
            }

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
            if(!isInitialized){
                return null;
            }

            Bounds bounds = new Bounds(myTransform.position, boundsSize);
            bool isVisible = false;
            Vector3 cameraPosition = cameraTransform.position;
            var result = new List<SubSceneJob>();

            //check if visible
            foreach(var corner in boundsCorners){
                Vector3 vectorToCorner = corner - cameraPosition;

                if(Vector3.Angle(vectorToCorner, cameraTransform.forward) < 100){
                    isVisible = true;
                    break;
                }
            }

            if(!isVisible){
                if(currentMode != eRegionMode.Inactive){
                    result = SwitchMode(eRegionMode.Inactive);
                }
            }
            else{
                float distance = float.MaxValue;

                if (bounds.Contains(cameraPosition))
                {
                    distance = 0;
                }
                else{
                    distance = (bounds.ClosestPoint(cameraPosition) - cameraPosition).magnitude;

                    foreach(var teleportPosition in teleportPositions){
                        if (bounds.Contains(teleportPosition))
                        {
                            distance = 0;
                            break;
                        }
                        else{
                            float dist = (bounds.ClosestPoint(teleportPosition) - teleportPosition).magnitude;
                            dist *= superRegion.World.SecondaryPositionDistanceModifier;

                            if (dist < distance)
                            {
                                distance = dist;
                            }
                        }
                    }
                }

                switch (currentMode)
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

        public Transform GetSubSceneRoot(eSubSceneType subSceneType)
        {
            foreach(Transform child in transform)
            {
                var tag = child.GetComponent<SubScene>();

                if(tag && tag.SubSceneType == subSceneType)
                {
                    return child;
                }
            }

            //if (Application.isPlaying)
            //{
            //    return subSceneRoots[(int)subSceneType];
            //}
            //else
            //{
            //for (int i = 0; i < transform.childCount; i++)
            //{
            //    var child = transform.GetChild(i);
            //    var tag = child.GetComponent<SubScene>();

            //    if (tag != null && tag.SubSceneType == subSceneType)
            //    {
            //        return child;
            //    }
            //}
            //}

            return null;
        }

        #endregion public methods       

        //========================================================================================

        #region private methods      

        private List<SubSceneJob> SwitchMode(eRegionMode newMode)
        {
            var result = new List<SubSceneJob>();

            switch (newMode)
            {
                case eRegionMode.Near:
                    //load
                    result.Add(CreateLoadSubSceneJob(GetSubSceneType(eSubSceneBaseType.Always)));
                    result.Add(CreateLoadSubSceneJob(GetSubSceneType(eSubSceneBaseType.Near)));

                    //unload
                    result.Add(CreateUnloadSubSceneJob(GetSubSceneType(eSubSceneBaseType.Far)));

                    break;
                case eRegionMode.Far:
                    //load
                    result.Add(CreateLoadSubSceneJob(GetSubSceneType(eSubSceneBaseType.Always)));
                    result.Add(CreateLoadSubSceneJob(GetSubSceneType(eSubSceneBaseType.Far)));

                    //unload
                    result.Add(CreateUnloadSubSceneJob(GetSubSceneType(eSubSceneBaseType.Near)));

                    break;
                case eRegionMode.Inactive:
                    //unload
                    result.Add(CreateUnloadSubSceneJob(GetSubSceneType(eSubSceneBaseType.Always)));
                    result.Add(CreateUnloadSubSceneJob(GetSubSceneType(eSubSceneBaseType.Far)));
                    result.Add(CreateUnloadSubSceneJob(GetSubSceneType(eSubSceneBaseType.Near)));
                    break;
            }

            currentMode = newMode;

            result.RemoveAll(item => item == null);
            return result;
        }

        private SubSceneJob CreateLoadSubSceneJob(eSubSceneType subSceneType)
        {
            int index = (int)subSceneType;
            eSubSceneState state = subSceneStates[index];

            if (state == eSubSceneState.Loaded || state == eSubSceneState.Loading)
            {
                return null;
            }
            else
            {
                subSceneStates[index] = eSubSceneState.Loading;
                return new SubSceneJob(this, subSceneType, eSubSceneJobType.Load, OnSubSceneJobDone);
            }
        }

        private SubSceneJob CreateUnloadSubSceneJob(eSubSceneType subSceneType)
        {
            int index = (int)subSceneType;
            eSubSceneState state = subSceneStates[index];

            if (state == eSubSceneState.Unloaded || state == eSubSceneState.Unloading)
            {
                return null;
            }
            else
            {
                subSceneStates[index] = eSubSceneState.Unloading;
                return new SubSceneJob(this, subSceneType, eSubSceneJobType.Unload, OnSubSceneJobDone);
            }
        }

        private void OnSubSceneJobDone(SubSceneJob subSceneJob)
        {
            int index = (int)subSceneJob.SceneType;

            if (subSceneJob.JobType == eSubSceneJobType.Load && subSceneJob.IsJobSuccessful)
            {
                if (subSceneStates[index] == eSubSceneState.Loading)
                {
                    subSceneStates[index] = eSubSceneState.Loaded;
                }

                subSceneRoots[index] = subSceneJob.SubSceneRoot;
            }
            else if (subSceneJob.JobType == eSubSceneJobType.Unload && subSceneJob.IsJobSuccessful)
            {
                if (subSceneStates[index] == eSubSceneState.Unloading)
                {
                    subSceneStates[index] = eSubSceneState.Unloaded;
                }

                subSceneRoots[index] = null;
            }
        }

#if UNITY_EDITOR
        private void CreateSubScenes()
        {
            foreach (var subSceneType in SubSceneTypes)
            {
                string subScenePath = WorldUtility.GetSubScenePath(gameObject.scene.path, Id, subSceneType);
                string subScenePathFull = WorldUtility.GetFullPath(subScenePath);

                if (GetSubSceneRoot(subSceneType) != null)
                {
                    continue;
                }
                else if (System.IO.File.Exists(subScenePathFull))
                {
                    Debug.LogFormat("SubScene \"{0}\" already exists but is not loaded!", subScenePath);
                    continue;
                }
                else
                {
                    var rootGO = new GameObject(WorldUtility.GetSubSceneRootName(subSceneType), typeof(SubScene));
                    rootGO.GetComponent<SubScene>().SubSceneType = subSceneType;

                    var root = rootGO.transform;
                    root.SetParent(transform, false);
                }
            }
        }
#endif

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