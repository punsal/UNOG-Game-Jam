using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SceneReference
{
    public class SceneFieldBuildPreprocessor : IPreprocessBuildWithReport
    {
        private static readonly string[] SearchFolders = { "Assets" };

        private static readonly string SceneFieldManagedReferenceTypeName =
            $"{typeof(SceneField).Assembly.GetName().Name} {typeof(SceneField).FullName}";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            ResyncPrefabs();
            ResyncScriptableObjects();
            ResyncScenes();
        }

        private static void ResyncPrefabs()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", SearchFolders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    bool changed = false;
                    foreach (Component component in root.GetComponentsInChildren<Component>(true))
                    {
                        if (component != null)
                        {
                            changed |= ResyncObject(component);
                        }
                    }

                    if (changed)
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void ResyncScriptableObjects()
        {
            bool anyChanged = false;

            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", SearchFolders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is ScriptableObject && ResyncObject(asset))
                    {
                        EditorUtility.SetDirty(asset);
                        anyChanged = true;
                    }
                }
            }

            if (anyChanged)
            {
                AssetDatabase.SaveAssets();
            }
        }

        private static void ResyncScenes()
        {
            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
                {
                    if (buildScene.enabled)
                    {
                        ResyncScene(buildScene.path);
                    }
                }
            }
            finally
            {
                if (previousSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                }
            }
        }

        private static void ResyncScene(string path)
        {
            Scene existingScene = SceneManager.GetSceneByPath(path);
            bool wasAlreadyOpen = existingScene.IsValid() && existingScene.isLoaded;
            bool wasAlreadyDirty = wasAlreadyOpen && existingScene.isDirty;

            Scene scene = wasAlreadyOpen ? existingScene : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                var components = new List<Component>();
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    components.AddRange(root.GetComponentsInChildren<Component>(true));
                }

                // Scan before mutating anything: if this scene turns out to
                // need a fix and was already dirty for unrelated reasons, we
                // must be able to fail without having touched it at all.
                bool needsFix = false;
                foreach (Component component in components)
                {
                    if (component != null && HasStaleSceneField(component))
                    {
                        needsFix = true;
                        break;
                    }
                }

                if (!needsFix)
                {
                    return;
                }

                if (wasAlreadyDirty)
                {
                    // Applying the fix now would mutate the user's already-dirty
                    // open scene in memory, and saving would persist their
                    // unrelated pending edits as a side effect of building.
                    // Fail loudly instead, before touching anything.
                    throw new BuildFailedException(
                        $"'{path}' is already open with unsaved changes and contains a stale SceneField. " +
                        "Save the scene manually and build again.");
                }

                bool changed = false;
                foreach (Component component in components)
                {
                    if (component != null)
                    {
                        changed |= ResyncObject(component);
                    }
                }

                if (changed)
                {
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                if (!wasAlreadyOpen)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static bool HasStaleSceneField(Object target) => ScanObject(target, apply: false);

        private static bool ResyncObject(Object target) => ScanObject(target, apply: true);

        private static bool ScanObject(Object target, bool apply)
        {
            var serializedObject = new SerializedObject(target);
            bool changed = false;
            var visitedManagedReferenceIds = new HashSet<long>();

            // Next(true) (unlike NextVisible) also walks [HideInInspector] and
            // managed-reference fields, so hidden SceneFields aren't skipped.
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            while (property.Next(enterChildren))
            {
                enterChildren = true;

                // Managed-reference graphs can be cyclic; stop descending into
                // a reference we've already visited so a cycle can't hang the build.
                if (property.propertyType == SerializedPropertyType.ManagedReference
                    && !visitedManagedReferenceIds.Add(property.managedReferenceId))
                {
                    enterChildren = false;
                    continue;
                }

                if (IsSceneField(property))
                {
                    if (IsSceneFieldStale(property, out string resolvedName))
                    {
                        changed = true;
                        if (apply)
                        {
                            property.FindPropertyRelative("sceneName").stringValue = resolvedName;
                        }
                    }

                    enterChildren = false;
                }
            }

            if (apply && changed)
            {
                serializedObject.ApplyModifiedProperties();
            }

            return changed;
        }

        private static bool IsSceneField(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                return property.managedReferenceFullTypename == SceneFieldManagedReferenceTypeName;
            }

            // property.type is just the short class name, so a same-named
            // SceneField in another namespace would also match it. boxedValue
            // resolves the field's actual declared C# type, so this is exact.
            return property.type == nameof(SceneField) && property.boxedValue is SceneField;
        }

        private static bool IsSceneFieldStale(SerializedProperty sceneFieldProperty, out string resolvedName)
        {
            SerializedProperty sceneAsset = sceneFieldProperty.FindPropertyRelative("sceneAsset");
            SerializedProperty sceneName = sceneFieldProperty.FindPropertyRelative("sceneName");
            if (sceneAsset == null || sceneName == null)
            {
                resolvedName = null;
                return false;
            }

            resolvedName = sceneAsset.objectReferenceValue != null ? sceneAsset.objectReferenceValue.name : string.Empty;
            return sceneName.stringValue != resolvedName;
        }
    }
}
