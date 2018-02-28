using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.World
{
    public abstract class RegionInspectorBase : Editor
    {
        protected RegionBase self;

        private SerializedProperty boundsSizeProperty;
        private SerializedProperty overrideRenderDistanceFarProperty;
        private SerializedProperty overrideRenderDistanceInactiveProperty;
        private SerializedProperty localRenderDistanceFarProperty;
        private SerializedProperty localRenderDistanceInactiveProperty;
        private SerializedProperty drawBoundsProperty;

        private List<SubScene> loadedSubScenes;
        private bool needSubSceneReloading;

        private void OnEnable()
        {
            self = target as RegionBase;

            boundsSizeProperty = serializedObject.FindProperty("boundsSize");
            overrideRenderDistanceFarProperty = serializedObject.FindProperty("overrideRenderDistanceFar");
            overrideRenderDistanceInactiveProperty = serializedObject.FindProperty("overrideRenderDistanceInactive");
            localRenderDistanceFarProperty = serializedObject.FindProperty("localRenderDistanceFar");
            localRenderDistanceInactiveProperty = serializedObject.FindProperty("localRenderDistanceInactive");
            drawBoundsProperty = serializedObject.FindProperty("drawBounds");

            loadedSubScenes = self.GetAllSubScenes();
        }

        public override void OnInspectorGUI()
        {
            //
            if (needSubSceneReloading)
            {
                loadedSubScenes = self.GetAllSubScenes();
                needSubSceneReloading = false;
            }

            //
            //base.OnInspectorGUI();

            EditorGUILayout.LabelField("----");

            boundsSizeProperty.vector3Value = EditorGUILayout.Vector3Field("Bounds Size", boundsSizeProperty.vector3Value);

            EditorGUILayout.LabelField("Render Distances");

            overrideRenderDistanceFarProperty.boolValue = EditorGUILayout.Toggle("  Override Far", overrideRenderDistanceFarProperty.boolValue);

            if (overrideRenderDistanceFarProperty.boolValue)
            {
                localRenderDistanceFarProperty.floatValue = EditorGUILayout.FloatField("  Far", localRenderDistanceFarProperty.floatValue);
            }

            overrideRenderDistanceInactiveProperty.boolValue = EditorGUILayout.Toggle("  Override Inactive", overrideRenderDistanceInactiveProperty.boolValue);

            if (overrideRenderDistanceInactiveProperty.boolValue)
            {
                localRenderDistanceInactiveProperty.floatValue = EditorGUILayout.FloatField("  Inactive", localRenderDistanceInactiveProperty.floatValue);
            }

            EditorGUILayout.LabelField("----");

            drawBoundsProperty.boolValue = EditorGUILayout.Toggle("Draw Bounds", drawBoundsProperty.boolValue);

            if (!Application.isPlaying && self.transform.parent.GetComponent<World>().EditorSubScenesLoaded)
            {
                GUILayout.Label("");

                foreach (var subSceneMode in self.AvailableSubSceneModes)
                {
                    foreach (var subSceneType in Enum.GetValues(typeof(eSubSceneType)).Cast<eSubSceneType>())
                    {
                        if (!self.GetSubSceneRoot(subSceneType, subSceneMode, loadedSubScenes) && GUILayout.Button("Create " + WorldUtility.GetSubSceneRootName(subSceneMode, subSceneType)))
                        {
                            UnityEngine.EventSystems.ExecuteEvents.Execute<IRegionEventHandler>(self.gameObject, null, (x, y) => x.CreateSubScene(subSceneMode, subSceneType));
                            needSubSceneReloading = true;
                        }
                    }
                }

                GUILayout.Label("");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }


    //public abstract class RegionInspectorBase : Editor
    //{
    //    protected RegionEditorBase self;

    //    private void OnEnable()
    //    {
    //        self = target as RegionEditorBase;
    //    }

    //    /// <summary>
    //    /// Shows the Load/Save button for a group of SubScenes.
    //    /// </summary>
    //    /// <param name="name"></param>
    //    /// <param name="subSceneTypes"></param>
    //    protected void ShowSubSceneMenuGroup(string name, List<eSubSceneType> subSceneTypes)
    //    {
    //        EditorGUILayout.LabelField(name);

    //        if (self.IsAnySubSceneLoaded())
    //        {
    //            if (GUILayout.Button(string.Concat("Save ", name)))
    //            {
    //                foreach (var subSceneType in subSceneTypes)
    //                {
    //                    self.SaveSubScene(subSceneType);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            if (GUILayout.Button(string.Concat("Load ", name)))
    //            {
    //                foreach (var subSceneType in subSceneTypes)
    //                {
    //                    self.LoadSubScene(subSceneType);
    //                }
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Shows the Load/Save button for a SubScene.
    //    /// </summary>
    //    /// <param name="subSceneType"></param>
    //    protected void ShowSubSceneMenu(eSubSceneType subSceneType)
    //    {
    //        EditorGUILayout.LabelField(subSceneType.ToString());

    //        if (self.IsSubSceneLoaded(subSceneType))
    //        {
    //            if (GUILayout.Button("Save SubScene "))
    //            {
    //                self.SaveSubScene(subSceneType);
    //            }
    //        }
    //        else
    //        {
    //            if (GUILayout.Button("Load SubScene "))
    //            {
    //                self.LoadSubScene(subSceneType);
    //            }
    //        }
    //    }
    //}
}