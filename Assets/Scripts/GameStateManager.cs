using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// Script υπεύθυνο για τη διαχείριση της κατάστασης του παιχνιδιού
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;

    // Αν το game έχει ξεκινήσει
    public bool gameStarted = false;

    // Reference στο Start button
    public GameObject startButton;

    // Reference στο GAME STARTED text
    public GameObject gameStartedText;

    void Awake()
    {
        instance = this;
    }

    // Start game - καλείται όταν ο παίκτης ξεκινάει το παιχνίδι
    public void StartGame()
    {   
        gameStarted = true;

        Debug.Log("Game Started!");

        // Εξαφάνιση Start button
        startButton.SetActive(false);

        // Εμφάνιση μηνύματος
        gameStartedText.SetActive(true);

        // Απόκρυψη μετά από 2 δευτερόλεπτα
        // Invoke("HideGameStartedText", 2f);
    }

    // Function που κρύβει το text
    void HideGameStartedText()
    {
        gameStartedText.SetActive(false);
    }

    // End game - καλείται όταν ο παίκτης τερματίζει το παιχνίδι
    public void EndGame()
    {
        gameStarted = false;

        Debug.Log("Game Ended!");
    }

    public void RestartGame()
    {
    // Ξαναφορτώνει τη σκηνή
    SceneManager.LoadScene(
        SceneManager.GetActiveScene().buildIndex
    );
}
}