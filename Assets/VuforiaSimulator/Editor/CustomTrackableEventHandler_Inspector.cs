/*==============================================================================
* Custom Inspector for CustomTrackableEventHandler
* Shows variables for Editor simulation only if it is enabled.
* Modified By: Foo Jing Ting
* Date: 27 February 2019
==============================================================================*/

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomTrackableEventHandler))]
public class CustomTrackableEventHandler_Inspector : Editor {
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CustomTrackableEventHandler script = (CustomTrackableEventHandler)target;
        if (script.enableTrackingWithoutMarker)
        {
            script.toggleTrackingKey = (KeyCode)EditorGUILayout.EnumPopup("Tracking Key", script.toggleTrackingKey);
            script.imageTargetTexture = (Texture)EditorGUILayout.ObjectField("Texture", script.imageTargetTexture, typeof(Texture), false);
        }
    }

}
