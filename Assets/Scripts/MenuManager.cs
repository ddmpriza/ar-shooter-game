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
        // Φόρτωσε τη SampleScene και ξεκίνα το hosting
        PlayerPrefs.SetInt("StartHosting", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenGeospatial()
    {
        SceneManager.LoadScene("GeospatialScene");
    }

    
}