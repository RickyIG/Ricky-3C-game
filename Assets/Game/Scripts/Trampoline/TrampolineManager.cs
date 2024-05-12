using UnityEngine;

public class TrampolineManager : MonoBehaviour
{

    [SerializeField]
    private float jumpForce;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Rigidbody>() is Rigidbody rb)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }
    }
}
