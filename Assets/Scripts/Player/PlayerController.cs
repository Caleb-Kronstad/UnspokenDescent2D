using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    private float max_health = 100;
    private float health;
    private float max_stamina = 100.0f;
    private float stamina;

    [SerializeField] private GameObject health_bar;
    [SerializeField] private GameObject stamina_bar;

    private bool can_regen_stamina = true;
    [SerializeField] private float regen_stamina_cooldown = 1.0f;
    [SerializeField] private float regen_stamina_rate = 5.0f;
    private Coroutine regen_stamina_coroutine;

    [Header("Camera")]
    [SerializeField] private Camera player_camera;
    private PlayerData player_data;
    [SerializeField] private float camera_speed = 2.0f;
    [SerializeField] private float camera_distance = -15.0f;

    private PlayerControls controls;
    private Rigidbody2D rigid_body;
    private Animator animator;

    private float scale;

    [Header("Movement")]
    [SerializeField] private float move_multiplier = 250.0f;
    [SerializeField] private float jump_multiplier = 750.0f;
    private Vector2 move_vector;
    private int direction = 1;
    private bool jumping = false;

    private string swing_id = "none";
    private bool swinging = false;
    private bool swinging_twice = false;
    private bool queue_swing = false;
    private float swing_stamina_cost = 10.0f;
    private float queue_timer = 0.5f;
    private Coroutine queue_coroutine;
    private bool can_deal_damage;

    private bool dashing = false;
    [SerializeField] private float dash_multiplier = 20.0f;
    [SerializeField] private float dash_stamina_cost = 20;

    [SerializeField] private LayerMask ground_layer;
    [SerializeField] private float ray_distance = 1.0f;

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

        health = max_health;
        stamina = max_stamina;
        can_regen_stamina = true;
    }

    void Update()
    {
        move_vector = controls.Land.Move.ReadValue<Vector2>();
        bool touching_solid_ground = TouchingSolidGround();
        if (jumping && touching_solid_ground)
        {
            jumping = false;
            animator.SetTrigger("EndJump");
        }

        if (controls.Land.Jump.triggered)
        {
            Jump(touching_solid_ground);
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

        if (move_vector.x != 0f)
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
    public void UpdateHealthBar()
    {
        health_bar.GetComponent<RectTransform>().sizeDelta = new Vector2(10 * health, health_bar.GetComponent<RectTransform>().sizeDelta.y);
    }

    public void UseStamina(float amount)
    {
        stamina -= amount;
        // if (stamina <= 0)
        // {
        //     regen_stamina_coroutine = StartCoroutine(RegenStaminaTimer(regen_stamina_cooldown));
        // }
        regen_stamina_coroutine = StartCoroutine(RegenStaminaTimer(regen_stamina_cooldown));
        UpdateStaminaBar();
    }
    public void RegenStamina()
    {
        if (!can_regen_stamina) return;
        stamina += regen_stamina_rate * Time.deltaTime;
        if (stamina >= max_stamina)
            stamina = max_stamina;
        UpdateStaminaBar();
    }
    public void UpdateStaminaBar()
    {
        float display = Mathf.Floor(stamina);
        stamina_bar.GetComponent<RectTransform>().sizeDelta = new Vector2(10 * display, stamina_bar.GetComponent<RectTransform>().sizeDelta.y);
    }

    public void Jump(bool touching_solid_ground)
    {
        if (!touching_solid_ground) return;
        animator.SetTrigger("Jump");
        rigid_body.AddForceY(jump_multiplier, ForceMode2D.Force);
    }

    public void JumpingTrue()
    {
        jumping = true;
    }

    public void SwordSwing()
    {
        if (swinging && !queue_swing && !swinging_twice)
        {
            queue_swing = true;
            queue_coroutine = StartCoroutine(QueueTimer(queue_timer));
        }
        else if (!swinging && stamina > 0)
        {
            swinging = true;
            swing_id = Guid.NewGuid().ToString();
            animator.SetTrigger("Swing1");
            UseStamina(swing_stamina_cost);
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
            if (stamina > 0)
            {
                swinging_twice = true;
                swing_id = Guid.NewGuid().ToString();
                animator.SetTrigger("Swing2");
                UseStamina(swing_stamina_cost);
            }
            else
            {
                swinging = false;
                swinging_twice = false;
                animator.SetTrigger("EndSwing");
            }
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
        if (dashing || stamina <= 0) return;
        dashing = true;
        UseStamina(dash_stamina_cost);
        animator.SetTrigger("Dash");
        rigid_body.AddForceX(dash_multiplier * direction, ForceMode2D.Impulse);
    }

    public void EndDash()
    {
        dashing = false;
        animator.SetTrigger("EndDash");
    }

    public bool TouchingSolidGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, Vector2.down, ray_distance, ground_layer);
        Debug.DrawRay(transform.position, Vector2.down * ray_distance, Color.red);
        if (hit.collider != null && hit.collider.CompareTag("SolidGround"))
            return true;
        else
            return false;
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
        if (can_deal_damage && tag == "PlayerHitbox" && swinging)
        {
            if (collision.transform.tag == "DummyEnemy")
            {
                collision.transform.GetComponent<DummyController>().ReceiveDamage(player_data.GetEquippedWeapon().GetDamage(), swing_id);
            }
        }
    }
    public void OnRelayTriggerExit(Collider2D collision, string tag)
    {
    }

    public void CanDealDamageTrue()
    {
        can_deal_damage = true;
    }
    public void CanDealDamageFalse()
    {
        can_deal_damage = false;
    }

    private IEnumerator QueueTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        queue_swing = false;
    }

    private IEnumerator RegenStaminaTimer(float seconds)
    {
        can_regen_stamina = false;
        yield return new WaitForSeconds(seconds);
        can_regen_stamina = true;
    }
}
