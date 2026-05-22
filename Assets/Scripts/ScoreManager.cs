using TMPro;
using UnityEngine;

// Script διαχείρισης score
public class ScoreManager : MonoBehaviour
{
    // Singleton-style access
    public static ScoreManager instance;

    // Current score
    int score = 0;

    // Reference στο UI text
    public TMP_Text scoreText;

    void Awake()
    {
        // Αποθηκεύουμε reference
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