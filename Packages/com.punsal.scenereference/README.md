# Scene Reference (`com.punsal.scenereference`)

An Inspector-friendly way to point a `MonoBehaviour` field at a scene. Drag a
scene asset onto the field like you would a prefab or texture, and get a
build-safe scene name back at runtime.

## Why

Unity's `UnityEditor.SceneAsset` type is what lets you drag-and-drop a scene
in the Inspector, but it lives in the `UnityEditor` namespace and can't be
referenced from code that ships in a player build. `SceneField` wraps a
`SceneAsset` reference for editor-time picking, and exposes only a plain
`string` scene name for runtime use.

## Installation

This package is embedded directly in this project under `Packages/`, so Unity
picks it up automatically — no `manifest.json` entry needed. To reuse it in
another project, copy the `com.punsal.scenereference` folder into that
project's `Packages/` directory.

## Usage

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneReference;

public class Boot : MonoBehaviour
{
    [SerializeField] private SceneField sceneToLoad;

    private void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
```

1. Add a `[SerializeField] private SceneField someField;` to your script.
2. In the Inspector, drag a `.unity` scene asset onto the field.
3. Use `someField.SceneName` (or rely on the implicit `string` conversion, as
   shown above) anywhere a scene name is expected, e.g.
   `SceneManager.LoadScene`.

**Important:** the target scene must be added to
**File > Build Profiles > Scene List** (or Build Settings), same as any scene
loaded via `SceneManager.LoadScene(string)`. This package only solves picking
the scene in the Inspector — it doesn't affect build inclusion.

## How it works

- `Runtime/SceneField.cs` — a serializable class holding a `UnityEngine.Object`
  reference (the scene asset) and a `string sceneName`. Only the string is
  meaningful at runtime; the object reference is editor-only bookkeeping.
- `Editor/SceneFieldPropertyDrawer.cs` — a `PropertyDrawer` that renders the
  field as a `SceneAsset` object picker and re-syncs `sceneName` from the
  asset reference on every repaint (so a renamed scene self-heals the next
  time its field is drawn in the Inspector, for a single selected object). If
  you multi-select several objects whose `SceneField` values disagree, the
  drawer shows Unity's normal mixed-value dash instead of guessing, and leaves
  the underlying data alone rather than risking overwriting one target's data
  with another's.
- `Editor/SceneFieldBuildPreprocessor.cs` — an `IPreprocessBuildWithReport`
  step that walks every prefab, `ScriptableObject` (including sub-assets), and
  build scene under `Assets/` (packages are left untouched, since they may be
  read-only) right before a build and force-resyncs any `SceneField` it finds
  — including ones hidden with `[HideInInspector]`, nested in arrays, or
  behind `[SerializeReference]` — as a safety net for fields that were never
  reopened in the Inspector after a rename (including the mixed-selection
  case above). A `SceneField` is matched by its actual declared C# type (via
  `boxedValue`/exact managed-reference type name), not just by class name, so
  an unrelated same-named type elsewhere isn't mistaken for it. Scenes already
  open in the editor are resynced in place rather than reopened, and the
  editor's previous scene setup is restored afterward. Each scene is scanned
  read-only first; if an already-open scene has unrelated unsaved changes and
  is found to need a fix, the build fails with a clear message before the
  scene is touched at all, rather than mutating it in memory (or saving those
  unrelated changes) as a side effect. Cyclic
  `[SerializeReference]` graphs are traversed with a visited-set guard so a
  cycle can't hang the build.
- The `Editor` assembly definition is restricted to the `Editor` platform, so
  none of this `UnityEditor`-dependent code is ever compiled into a player
  build.

## Limitations

- `sceneName` is a plain string copy, kept in sync by the drawer (on view) and
  the build preprocessor (on build). If you inspect the raw serialized value
  of a field that hasn't been viewed or built since a rename, it may be
  briefly stale — but both normal editing and building always self-correct it.
- Scene lookup at runtime is by name via `SceneManager.LoadScene(string)`, so
  duplicate scene names anywhere in the build will resolve ambiguously (a
  general Unity limitation, not specific to this package).
