using UnityEngine;

public class EnemyCollisionRelay : MonoBehaviour
{
    [SerializeField] private DummyController enemy;

    void OnCollisionEnter2D(Collision2D collision)
    {
        enemy.OnRelayCollisionEnter(collision, transform.tag);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        enemy.OnRelayCollisionStay(collision, transform.tag);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        enemy.OnRelayCollisionExit(collision, transform.tag);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        enemy.OnRelayTriggerEnter(collision, transform.tag);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        enemy.OnRelayTriggerStay(collision, transform.tag);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        enemy.OnRelayTriggerExit(collision, transform.tag);
    }
}
