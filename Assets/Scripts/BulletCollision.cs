using UnityEngine;

// Script για collision detection
public class BulletCollision : MonoBehaviour
{
    // Η function καλείται αυτόματα όταν γίνει collision (Collider και Rigidbody)
    // Καλείται λόγω του ονοματός της function (OnCollisionEnter) και του τύπου παραμέτρου (Collision)
    void OnCollisionEnter(Collision collision)
    {
        // Έλεγχος αν το αντικείμενο που χτυπήθηκε είναι "Target"/κυβάκι
        if (collision.gameObject.CompareTag("Target"))
        {
            // Αυξάνουμε το score κατά 1
            ScoreManager.instance.AddScore();

            // Καταστρέφουμε το target
            Destroy(collision.gameObject);

            // Καταστρέφουμε και το bullet
            Destroy(gameObject);
        }
    }
}