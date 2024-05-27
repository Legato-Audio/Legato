using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Script adapted by rsodre on the Unity forum. Credit for the original script is given to Jeff Pizano.
// https://discussions.unity.com/t/how-to-make-public-variable-read-only-during-run-time/69124

[CustomPropertyDrawer(typeof(ReadOnlyWhenPlayingAttribute))]
public class ReadOnlyWhenPlayingAttributeDrawer : PropertyDrawer
{
    // Necessary since some properties tend to collapse smaller than their content
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    // Draw a disabled property field
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = !Application.isPlaying;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}