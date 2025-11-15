using UnityEngine;

public class CollectibleHeal : MonoBehaviour
{
    [Header("Heal Settings")]
    [SerializeField] private float healAmount = 5f;

    [Header("Optional Visuals/SFX")]
    [SerializeField] private AudioClip healSound;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Heal(healAmount);

                // Optional: play sound
                if (healSound != null)
                    AudioSource.PlayClipAtPoint(healSound, transform.position);

                Debug.Log($"Player healed by {healAmount} HP via collectible.");

                Destroy(gameObject);
            }
        }
    }
}
