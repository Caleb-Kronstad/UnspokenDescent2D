using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private int max_health = 100;
    private int health;
    private float max_stamina = 100;
    private float stamina;

    [SerializeField] private GameObject health_bar_bg;
    [SerializeField] private GameObject health_bar;

    private bool can_regen_stamina = true;
    private float regen_stamina_cooldown = 1.0f;
    private float regen_stamina_rate = 1.0f;
    private Coroutine regen_stamina_coroutine;

    [SerializeField] private Camera player_camera;
    private PlayerData player_data;
    private float camera_speed = 2f;
    private float camera_distance = -15f;

    private PlayerControls controls;
    private Rigidbody2D rigid_body;
    private Animator animator;

    private float scale;

    private Vector2 move_vector;
    private float move_multiplier = 150;
    private float jump_multiplier = 750f;
    private bool jumping = false;
    private int direction = 1;

    private string swing_id = "none";
    private bool swinging = false;
    private bool swinging_twice = false;
    private bool queue_swing = false;
    private float queue_timer = 0.5f;
    private Coroutine queue_coroutine;

    private bool dashing = false;
    private float dash_multiplier = 1.0f;
    private float dash_mass = 0.1f;
    private float original_mass;
    private int dash_cost = 20;

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable() {
        controls.Disable();
    }

    void Awake()
    {
        controls = new PlayerControls();
        scale = this.transform.localScale.x;
    }

    void Start()
    {
        player_data = GetComponent<PlayerData>();
        rigid_body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        original_mass = rigid_body.mass;
    }

    void Update()
    {
        move_vector = controls.Land.Move.ReadValue<Vector2>();
        if (controls.Land.Jump.triggered && !jumping)
        {
            //jumping = true;
            ReceiveDamage(10);
        }

        if (controls.Land.Attack.triggered)
        {
            SwordSwing();
        }
        if (controls.Land.Dash.triggered)
        {
            Dash();
        }

        RegenStamina();
    }

    void FixedUpdate()
    {
        move_vector *= move_multiplier * Time.fixedDeltaTime;
        if (move_vector.x > 0)
            direction = 1;
        else if (move_vector.x < 0)
            direction = -1;

        if (!dashing)
        {
            this.transform.localScale = new Vector2(scale * direction, scale);
            rigid_body.linearVelocity = new Vector2(move_vector.x, rigid_body.linearVelocity.y);
        }

        if (move_vector != Vector2.zero)
        {
            animator.SetBool("Walking", true);
        }
        else
        {
            animator.SetBool("Walking", false);
        }

        player_camera.transform.position = Vector3.Lerp(player_camera.transform.position, new Vector3(this.transform.position.x, 0, camera_distance), Time.fixedDeltaTime * camera_speed);
    }

    public void ReceiveDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            health = 0;
        }
        health_bar.GetComponent<RectTransform>().sizeDelta = new Vector2(10 * health, health_bar.GetComponent<RectTransform>().sizeDelta.y);
    }
    public void ReceiveHealing(int amount)
    {
        health += amount;
        if (health > max_health)
        {
            health = max_health;
        }
        health_bar.GetComponent<RectTransform>().sizeDelta = new Vector2(10 * health, health_bar.GetComponent<RectTransform>().sizeDelta.y);
    }

    public void UseStamina(int amount)
    {
        stamina -= amount;
        if (stamina <= 0)
        {
            can_regen_stamina = false;
            regen_stamina_coroutine = StartCoroutine(RegenStaminaTimer(regen_stamina_cooldown));
        }
    }
    public void RegenStamina()
    {
        if (!can_regen_stamina) return;
        stamina += regen_stamina_rate * Time.deltaTime;
        stamina = Mathf.Floor(stamina);
        if (stamina >= max_stamina)
            stamina = max_stamina;
    }

    public void Jump()
    {
        if (jumping)
        {
            jumping = false;
            rigid_body.AddForceY(jump_multiplier, ForceMode2D.Force);
        }
    }

    public void SwordSwing()
    {
        if (swinging && !queue_swing && !swinging_twice)
        {
            queue_swing = true;
            queue_coroutine = StartCoroutine(QueueTimer(queue_timer));
        }
        else if (!swinging)
        {
            swinging = true;
            swing_id = Guid.NewGuid().ToString();
            animator.SetTrigger("Swing1");
        }
    }

    public void EndSwordSwing()
    {
        if (queue_coroutine != null)
        {
            StopCoroutine(queue_coroutine);
            queue_coroutine = null;
        }

        if (queue_swing)
        {
            queue_swing = false;
            swinging_twice = true;
            swing_id = Guid.NewGuid().ToString();
            animator.SetTrigger("Swing2");
        }
        else
        {
            swinging = false;
            swinging_twice = false;
            animator.SetTrigger("EndSwing");
        }
    }

    public void Dash()
    {
        if (dashing || !(stamina > 0)) return;
        dashing = true;
        UseStamina(dash_cost);
        animator.SetTrigger("Dash");
        rigid_body.mass = dash_mass;
        rigid_body.AddForceX(dash_multiplier * direction, ForceMode2D.Impulse);
    }

    public void EndDash()
    {
        dashing = false;
        animator.SetTrigger("EndDash");
        rigid_body.mass = original_mass;
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
    }
    public void OnRelayTriggerStay(Collider2D collision, string tag)
    {
        if (tag == "PlayerHitbox" && swinging)
        {
            if (collision.transform.tag == "DummyEnemy")
            {
                collision.transform.GetComponent<DummyController>().TakeDamage(player_data.GetEquippedWeapon().GetDamage(), swing_id);
            }
        }
    }
    public void OnRelayTriggerExit(Collider2D collision, string tag)
    {
    }

    private IEnumerator QueueTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        queue_swing = false;
    }

    private IEnumerator RegenStaminaTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        can_regen_stamina = true;
    }
}
