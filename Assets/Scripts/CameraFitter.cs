using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

// Width-locks the ortho camera so the full corridor (walls at +/-4) is always
// visible regardless of device aspect. The serialized ortho size (5) and the
// PixelPerfectCamera ref resolution (288x512) both disagree with the level's
// authored width, which let the player and altars leave the screen.
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
