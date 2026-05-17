using UnityEngine;

// Script υπεύθυνο για shooting bullets
public class BulletShooter : MonoBehaviour
{
    // Prefab του bullet
    public GameObject bulletPrefab;

    // Δύναμη εκτόξευσης
    public float bulletForce = 500f;

    void Update()
    {
        // Αν πατηθεί SPACE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Δημιουργία bullet
            GameObject bullet = Instantiate(
                bulletPrefab,

                // Θέση camera
                Camera.main.transform.position,

                // Rotation camera
                Camera.main.transform.rotation
            );

            // Παίρνουμε το Rigidbody
            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            // Προσθέτουμε δύναμη
            // προς την κατεύθυνση της camera
            rb.AddForce(
                Camera.main.transform.forward * bulletForce
            );
        }
    }
}