using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Movement")]
    [SerializeField] private float move_speed = 2.0f;
    private int direction = 1;
    private float scale;

    [Header("Patrol")]
    [SerializeField] private float patrol_point_a = -5.0f;
    [SerializeField] private float patrol_point_b = 5.0f;
    private float patrol_world_a;
    private float patrol_world_b;

    [Header("AI")]
    [SerializeField] private float detection_range = 6.0f;
    [SerializeField] private float attack_range = 1.2f;
    [SerializeField] private float attack_cooldown = 1.5f;
    private bool can_attack = true;
    private Transform player;

    void Start()
    {
        health = max_health;
        health_bar_full_scale = health_bar.transform.localScale;
        health_bar_full_pos = health_bar.transform.localPosition;

        rigid_body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        scale = transform.localScale.x;

        patrol_world_a = transform.position.x + patrol_point_a;
        patrol_world_b = transform.position.x + patrol_point_b;

        Collider2D[] my_colliders = GetComponents<Collider2D>();

        GameObject player_obj = GameObject.FindGameObjectWithTag("Player");
        if (player_obj != null)
        {
            player = player_obj.transform;
            Collider2D[] player_colliders = player_obj.GetComponents<Collider2D>();
            foreach (Collider2D mc in my_colliders)
                foreach (Collider2D pc in player_colliders)
                    Physics2D.IgnoreCollision(mc, pc);
        }

        foreach (DummyController other in FindObjectsByType<DummyController>())
        {
            if (other == this) continue;
            foreach (Collider2D mc in my_colliders)
                foreach (Collider2D oc in other.GetComponents<Collider2D>())
                    Physics2D.IgnoreCollision(mc, oc);
        }
    }

    void Update()
    {
        if (dead) return;

        float dist_to_player = player != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;

        if (player != null && dist_to_player <= detection_range)
        {
            if (dist_to_player <= attack_range)
            {
                rigid_body.linearVelocity = new Vector2(0, rigid_body.linearVelocity.y);
                animator.SetBool("Walking", false);
                if (!swinging && can_attack)
                    Swing();
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            Patrol();
        }
    }

    private void ChasePlayer()
    {
        direction = player.position.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector3(scale * direction, scale, scale);
        rigid_body.linearVelocity = new Vector2(move_speed * direction, rigid_body.linearVelocity.y);
        animator.SetBool("Walking", true);
    }

    private void Patrol()
    {
        if ((direction == 1 && transform.position.x >= patrol_world_b) ||
            (direction == -1 && transform.position.x <= patrol_world_a))
        {
            direction *= -1;
            transform.localScale = new Vector3(scale * direction, scale, scale);
        }

        rigid_body.linearVelocity = new Vector2(move_speed * direction, rigid_body.linearVelocity.y);
        animator.SetBool("Walking", true);
    }

    private void Swing()
    {
        swing_id = Guid.NewGuid().ToString();
        animator.SetTrigger("Slash");
        swinging = true;
        can_attack = false;
    }

    public void EndSwing()
    {
        swinging = false;
        animator.SetTrigger("EndSlash");
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attack_cooldown);
        can_attack = true;
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
            rigid_body.linearVelocity = Vector2.zero;
            rigid_body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            animator.SetTrigger("Death");
            GameObject player_obj = GameObject.FindGameObjectWithTag("Player");
            if (player_obj != null)
                player_obj.GetComponent<PlayerController>().NotifyTargetDeath(this.transform);
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
        float fill = Mathf.Max((float)health / max_health, 0f);
        float full_width = health_bar.GetComponent<SpriteRenderer>().sprite.bounds.size.x * health_bar_full_scale.x;

        Vector3 scale_v = health_bar_full_scale;
        scale_v.x *= fill;
        health_bar.transform.localScale = scale_v;

        Vector3 pos = health_bar_full_pos;
        pos.x -= (full_width / 2f) * (1f - fill);
        health_bar.transform.localPosition = pos;
    }

    public void OnRelayCollisionEnter(Collision2D collision, string tag) { }
    public void OnRelayCollisionStay(Collision2D collision, string tag) { }
    public void OnRelayCollisionExit(Collision2D collision, string tag) { }

    public void OnRelayTriggerEnter(Collider2D collision, string tag) { OnRelayTrigger(collision, tag); }
    public void OnRelayTriggerStay(Collider2D collision, string tag) { OnRelayTrigger(collision, tag); }
    public void OnRelayTriggerExit(Collider2D collision, string tag) { OnRelayTrigger(collision, tag); }

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

    public void CanDealDamageTrue() { can_deal_damage = true; }
    public void CanDealDamageFalse() { can_deal_damage = false; }
    public void SwitchSwingId() { swing_id = Guid.NewGuid().ToString(); }

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
        can_attack = true;
        animator.SetTrigger("Revive");
    }
}