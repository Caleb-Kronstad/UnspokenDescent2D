using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyController : MonoBehaviour
{
    private int max_health = 100;
    private int health;
    private float respawn_time = 1.0f;
    private List<string> damaged_by_ids = new List<string>();

    void Start()
    {
        health = max_health;
    }

    public void TakeDamage(int damage, string damaged_by_id)
    {
        if (damaged_by_ids.Contains(damaged_by_id)) return;
        damaged_by_ids.Add(damaged_by_id);

        health -= damage;

        if (health <= 0)
        {
            health = 0;
            StartCoroutine(RespawnTimer(respawn_time));
        }

        Debug.Log(health);
    }

    private IEnumerator RespawnTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        health = max_health;
        damaged_by_ids.Clear();
    }
}
