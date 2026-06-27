using TMPro;
using UnityEngine;

// Script διαχείρισης score
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    // Current score
    int score = 0;

    // Εμφάνηση text στο UI text
    public TMP_Text scoreText;

    void Awake()
    {
        instance = this;
    }

    // Function που αυξάνει score
    public void AddScore()
    {
        // Αύξηση score
        score++;
        // Update UI
        scoreText.text = "Score: " + score;
    }

    public int GetScore()
    {
        return score;
    }
}