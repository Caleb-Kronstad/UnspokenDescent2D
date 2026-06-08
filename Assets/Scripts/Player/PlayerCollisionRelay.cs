using UnityEngine;

public class PlayerCollisionRelay : MonoBehaviour
{
    private PlayerController player;

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        player.OnRelayCollisionEnter(collision, transform.tag);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        player.OnRelayCollisionStay(collision, transform.tag);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        player.OnRelayCollisionExit(collision, transform.tag);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        player.OnRelayTriggerEnter(collision, transform.tag);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        player.OnRelayTriggerStay(collision, transform.tag);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        player.OnRelayTriggerExit(collision, transform.tag);
    }
}
