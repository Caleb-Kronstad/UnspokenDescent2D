using UnityEngine;

[System.Serializable]
public class Weapon : Item
{
    private int damage;

    public int GetDamage() { return damage; }
    public void SetDamage(int new_damage) { damage = new_damage; }

    public Weapon(string title, string description, Sprite sprite, int damage)
    {
        this.title = title;
        this.description = description;
        this.sprite = sprite;
        this.damage = damage;
    }
}
