using UnityEngine;
using StarterAssets;

public class Enemy : MonoBehaviour
{
    [Header("踏みつけ時のプレイヤーの跳ね返り力")]
    public float bounceForce = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ThirdPersonController playerController = collision.gameObject.GetComponent<ThirdPersonController>();

            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    if (playerController != null)
                    {
                        playerController.Bounce(bounceForce);
                    }

                    Die();
                    return;
                }
            }

        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}