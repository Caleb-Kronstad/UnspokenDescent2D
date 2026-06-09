using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class DummyController : MonoBehaviour
{
    private int max_health = 100;
    private int health;
    private float respawn_time = 1.0f;
    private List<string> damaged_by_ids = new List<string>();

    [SerializeField] private GameObject health_bar;
    private Vector3 health_bar_full_scale;
    private Vector3 health_bar_full_pos;

    void Start()
    {
        health = max_health;
        health_bar_full_scale = health_bar.transform.localScale;
        health_bar_full_pos = health_bar.transform.localPosition;
    }

    public void ReceiveDamage(int amount, string damaged_by_id)
    {
        if (damaged_by_ids.Contains(damaged_by_id)) return;
        damaged_by_ids.Add(damaged_by_id);

        health -= amount;

        if (health <= 0)
        {
            health = 0;
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

    private IEnumerator RespawnTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        health = max_health;
        damaged_by_ids.Clear();
    }
}
