using UnityEditor;
using UnityEngine;

namespace SceneReference
{
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty sceneAsset = property.FindPropertyRelative("sceneAsset");
            SerializedProperty sceneName = property.FindPropertyRelative("sceneName");

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            bool mixedValues = sceneAsset.hasMultipleDifferentValues || sceneName.hasMultipleDifferentValues;
            EditorGUI.showMixedValue = mixedValues;

            EditorGUI.BeginChangeCheck();
            Object selected = EditorGUI.ObjectField(position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);
            bool userChangedValue = EditorGUI.EndChangeCheck();

            EditorGUI.showMixedValue = false;

            if (userChangedValue)
            {
                sceneAsset.objectReferenceValue = selected;
                sceneName.stringValue = selected != null ? selected.name : string.Empty;
            }
            else if (!mixedValues)
            {
                // Passive resync (e.g. after a rename), but only when every
                // selected object already agrees — otherwise a blanket write
                // here would stamp one target's name onto a different target's
                // scene reference.
                string resolvedName = selected != null ? selected.name : string.Empty;
                if (sceneName.stringValue != resolvedName)
                {
                    sceneName.stringValue = resolvedName;
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
