using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

/// <summary>Keeps the orthographic camera fitted to the corridor width.</summary>
public static class CameraFitter
{
    // Walls sit at +/-4, half a unit of slack keeps their outer edge on screen.
    public const float DesignHalfWidth = 4.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        Apply();
        SceneManager.sceneLoaded += (_, _) => Apply();
    }

    private static void Apply()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic)
        {
            return;
        }

        PixelPerfectCamera ppc = cam.GetComponent<PixelPerfectCamera>();
        if (ppc != null)
        {
            ppc.enabled = false;
        }

        cam.orthographicSize = DesignHalfWidth / cam.aspect;
    }
}
