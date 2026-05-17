using UnityEngine;

// Script υπεύθυνο για shooting bullets
public class BulletShooter : MonoBehaviour
{
    // Prefab του bullet
    public GameObject bulletPrefab;

    // Δύναμη εκτόξευσης Bullet
    public float bulletForce = 500f;

    // Function που καλείται από το UI Shoot Button για να εκτοξεύσει ένα bullet
    public void Shoot()
    {   
        // Αν το game ΔΕΝ έχει ξεκινήσει - δεν επιτρέπεται shooting
        if (!GameStateManager.instance.gameStarted)
        {
            return;
        }
        
        // Δημιουργία ενός νέου bullet στη θέση και κατεύθυνση της κάμερας
        GameObject bullet = Instantiate(
            // prefab του bullet που θα δημιουργηθεί
            bulletPrefab,
            // θέση εκτόξευσης - στη θέση της κάμερας
            Camera.main.transform.position,
            // κατεύθυνση εκτόξευσης - στην κατεύθυνση που κοιτάει η κάμερα
            Camera.main.transform.rotation
        );

        // Rigidbody του bullet - εφαρμογή physics
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        // Προσθήκη δύναμης στο bullet 
        rb.AddForce(
            Camera.main.transform.forward * bulletForce);
        }
}