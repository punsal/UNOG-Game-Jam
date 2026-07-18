using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneReference;

/// <summary>Loads the configured game scene after bootstrap initialization.</summary>
public class Boot : MonoBehaviour
{
    [SerializeField] private SceneField sceneToLoad;

    private void Awake()
    {
        StartCoroutine(Bootstrap());
    }

    private IEnumerator Bootstrap()
    {
        yield return new WaitForEndOfFrame();
        LoadScene();
    }

    private void LoadScene()
    {
        Debug.Log($"Loading scene '{sceneToLoad}'.", this);
        SceneManager.LoadScene(sceneToLoad);
    }
}
