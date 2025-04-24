using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Lego"))
        {
            // Çarpışma olduğunda harekete direnç ekle
            GetComponent<Rigidbody>().linearVelocity *= 0.1f;
        }
    }
}
