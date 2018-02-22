using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Utilities
{
    [CustomPropertyDrawer(typeof(UniqueId))]
    public class UniqueIdInspector : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            Rect contentPosition = EditorGUI.PrefixLabel(position, label);

            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = 1f;

            EditorGUI.LabelField(contentPosition, label, new GUIContent(property.FindPropertyRelative("uniqueId").stringValue));

            EditorGUI.EndProperty();
        }
    }
}