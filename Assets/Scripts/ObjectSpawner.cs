using UnityEngine;

// Κλαση που επιτρέπει στον παίκτη να δημιουργεί κύβους κάνοντας κλικ με το ποντίκι στο έδαφος.
// Η class κληρονομεί από το MonoBehaviour, που είναι η βασική κλάση για όλα τα scripts στο Unity - επιτρέπει χρήσή callbacks όπως το Update() και το Start().
// Όταν ο παίκτης κάνει κλικ, η μέθοδος Update() ελέγχει αν το κλικ έγινε πάνω σε ένα αντικείμενο με τη χρήση ενός Raycast.
// Αν το Raycast χτυπήσει ένα αντικείμενο, δημιουργείται ένας νέος κύβος (cubePrefab) στη θέση που χτυπήθηκε.
public class ObjectSpawner : MonoBehaviour
{
    public GameObject cubePrefab;

    void Update()
    {
        // Η μέθοδος Input.GetMouseButtonDown(0) ελέγχει αν ο παίκτης έχει κάνει κλικ με το αριστερό κουμπί του ποντικιού (0 αντιστοιχεί στο αριστερό κουμπί).
        if (Input.GetMouseButtonDown(0))
        {
            // Η μέθοδος Camera.main.ScreenPointToRay μετατρέπει τη θέση του ποντικιού στην οθόνη σε ένα Ray που ξεκινάει από την κάμερα και κατευθύνεται προς τη θέση του ποντικιού στο κόσμο.
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Η δομή RaycastHit χρησιμοποιείται για να αποθηκεύσει πληροφορίες σχετικά με το αντικείμενο που χτυπήθηκε από το Raycast, όπως η θέση του χτυπήματος (hit.point) και το αντικείμενο που χτυπήθηκε (hit.collider).
            RaycastHit hit;

            // Η μέθοδος Physics.Raycast ελέγχει αν το Ray που δημιουργήθηκε από την κάμερα προς τη θέση του ποντικιού χτυπάει κάποιο αντικείμενο στο σκηνικό.
            if (Physics.Raycast(ray, out hit))
            {
                // Η μέθοδος Instantiate δημιουργεί ένα νέο αντικείμενο (cubePrefab) στη θέση που χτυπήθηκε από το Raycast (hit.point).
                // Ο κύβος δημιουργείται με την ταυτότητα περιστροφής (Quaternion.identity), που σημαίνει ότι δεν θα έχει καμία περιστροφή όταν δημιουργηθεί.
                Instantiate(cubePrefab, hit.point, Quaternion.identity);
            }
        }
    }
}