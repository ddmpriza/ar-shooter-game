using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OpenGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void InitializeCloudAnchors()
    {
        Debug.Log("Initialize Cloud Anchors pressed!");
    }
}