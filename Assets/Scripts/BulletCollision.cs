using UnityEngine;

// Script για collision detection
public class BulletCollision : MonoBehaviour
{
    // Η function καλείται αυτόματα
    // όταν γίνει collision
    void OnCollisionEnter(Collision collision)
    {
        // Ελέγχουμε αν το αντικείμενο
        // που χτυπήσαμε έχει tag "Target"
        if (collision.gameObject.CompareTag("Target"))
        {
            // Καταστρέφουμε το target
            Destroy(collision.gameObject);

            // Καταστρέφουμε και το bullet
            Destroy(gameObject);
        }
    }
}