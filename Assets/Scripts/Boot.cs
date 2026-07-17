using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneReference;

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
        SceneManager.LoadScene(sceneToLoad);
    }
}
