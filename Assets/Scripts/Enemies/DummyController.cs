using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class DummyController : MonoBehaviour
{
    private int max_health = 100;
    private int health;
    private float respawn_time = 3.0f;
    private List<string> damaged_by_ids = new List<string>();
    private bool dead = false;

    private Rigidbody2D rigid_body;
    private Animator animator;

    [SerializeField] private GameObject health_bar;
    private Vector3 health_bar_full_scale;
    private Vector3 health_bar_full_pos;

    private bool can_deal_damage = false;
    [SerializeField] private float damage = 10.0f;

    private bool swinging = false;
    private string swing_id = "none";

    void Start()
    {
        health = max_health;
        health_bar_full_scale = health_bar.transform.localScale;
        health_bar_full_pos = health_bar.transform.localPosition;

        rigid_body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!swinging) Swing();
    }

    private void Swing()
    {
        swing_id = Guid.NewGuid().ToString();
        animator.SetTrigger("Slash");
        swinging = true;
    }

    public void EndSwing()
    {
        swinging = false;
        animator.SetTrigger("EndSlash");
    }

    public void ReceiveDamage(int amount, string damaged_by_id)
    {
        if (damaged_by_ids.Contains(damaged_by_id)) return;
        damaged_by_ids.Add(damaged_by_id);

        health -= amount;

        if (health <= 0 && !dead)
        {
            health = 0;
            dead = true;
            animator.SetTrigger("Death");
            StartCoroutine(RespawnTimer(respawn_time));
        }
        UpdateHealthBar();
    }

    public void ReceiveHealing(int amount)
    {
        health += amount;
        if (health > max_health)
        {
            health = max_health;
        }
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        float fill = (float)health / max_health;
        float full_width = health_bar.GetComponent<SpriteRenderer>().sprite.bounds.size.x * health_bar_full_scale.x;

        Vector3 scale = health_bar_full_scale;
        scale.x *= fill;
        health_bar.transform.localScale = scale;

        Vector3 pos = health_bar_full_pos;
        pos.x -= (full_width / 2f) * (1f - fill);
        health_bar.transform.localPosition = pos;
    }

    public void OnRelayCollisionEnter(Collision2D collision, string tag)
    {
    }
    public void OnRelayCollisionStay(Collision2D collision, string tag)
    {
    }
    public void OnRelayCollisionExit(Collision2D collision, string tag)
    {
    }

    public void OnRelayTriggerEnter(Collider2D collision, string tag)
    {
        OnRelayTrigger(collision, tag);
    }
    public void OnRelayTriggerStay(Collider2D collision, string tag)
    {
        OnRelayTrigger(collision, tag);
    }
    public void OnRelayTriggerExit(Collider2D collision, string tag)
    {
        OnRelayTrigger(collision, tag);
    }

    public void OnRelayTrigger(Collider2D collision, string tag)
    {
        if (can_deal_damage && tag == "EnemyHitbox" && swinging)
        {
            if (collision.transform.tag == "Player")
            {
                collision.transform.GetComponent<PlayerController>().ReceiveDamage(damage, swing_id);
            }
        }
    }

    public void CanDealDamageTrue()
    {
        can_deal_damage = true;
    }
    public void CanDealDamageFalse()
    {
        can_deal_damage = false;
    }
    public void SwitchSwingId()
    {
        swing_id = Guid.NewGuid().ToString();
    }

    public void DeathAnimationChange()
    {
        animator.SetTrigger("Dead");
    }

    private IEnumerator RespawnTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        dead = false;
        health = max_health;
        damaged_by_ids.Clear();
        UpdateHealthBar();
        animator.SetTrigger("Revive");
    }
}
