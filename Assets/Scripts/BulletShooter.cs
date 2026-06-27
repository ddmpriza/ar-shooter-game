using UnityEngine;

// Script υπεύθυνο για shooting bullets
public class BulletShooter : MonoBehaviour
{
    // Prefab του bullet - αντικείμενο που θα δημιουργηθεί όταν πατηθεί το κουμπί shoot (inisialized in Unity Inspector)
    [SerializeField] private GameObject bulletPrefab;

    // Δύναμη εκτόξευσης Bullet - αλλαγή από το Unity Inspector (500f default)
    [SerializeField] private float bulletForce = 500f;

    // Function που καλείται από το UI Shoot Button για να εκτοξεύσει ένα bullet
    public void Shoot()
    {   
        // Αν το game ΔΕΝ έχει ξεκινήσει - δεν επιτρέπεται shooting
        if (!GameStateManager.instance.gameStarted)
        {
            return;
        }
        
        // Δημιουργία ενός νέου bullet στη θέση και κατεύθυνση της κάμερας
        // bulletPrefab: prefab του bullet που θα δημιουργηθεί
        // Camera.main.transform.position: θέση εκτόξευσης - στη θέση της κάμερας
        // Camera.main.transform.rotation: κατεύθυνση εκτόξευσης - στην κατεύθυνση που κοιτάει η κάμερα
        GameObject bullet = Instantiate( bulletPrefab, Camera.main.transform.position, Camera.main.transform.rotation);

        
        // Rigidbody: Προσθήκη φυσικής διάστασης στο bullet 
        // Προστίθεται βαρύτητα για να πεφτεί σταδιακά προς το έδαφος (default mass = 1kg)
        // Προστίθεται δύναμη εκτόξευσης (AddForce)
        // Προστίθεται αντίδραση σε σύγκρουση με άλλο αντικείμενο (OnCollisionEnter)
        // Μετατροπή του bullet από οπτικό αντικείμενο σε φυσικό αντικείμενο
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        // Προσθήκη ώθησης στο bullet 
        rb.AddForce(Camera.main.transform.forward * bulletForce);
        }
}