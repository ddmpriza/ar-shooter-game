using UnityEngine;
using UnityEngine.SceneManagement;

// Διαχείριση του UI menu και των κουμπιών
public class MenuManager : MonoBehaviour
{
    // Έναρξη του παιχνιδιού με φόρτωση της SampleScene
    public void OpenGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Φόρτωση της ίδιας σκηνής σε hosting mode 
    public void InitializeCloudAnchors()
    {
        PlayerPrefs.SetInt("StartHosting", 1);      // Διαχωρισμός του GameMode από το CloudAnchorInitialization
        PlayerPrefs.Save();
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenGeospatial()
    {
        SceneManager.LoadScene("GeospatialScene");
    }

    
}