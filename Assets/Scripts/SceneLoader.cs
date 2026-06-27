using UnityEngine;
using UnityEngine.SceneManagement;

// Φόρτωση του βασικού menu με το κουμπί Back
public class SceneLoader : MonoBehaviour
{
    public void LoadMenu()
    {
        SceneManager.LoadScene("MyARGame");
    }
}