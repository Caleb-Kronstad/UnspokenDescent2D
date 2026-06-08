using UnityEngine;

[System.Serializable]
public class Armor : Item
{
    private int defense;

    public int GetDefense() { return defense; }
    public void SetDefense(int new_defense) { defense = new_defense; }

    public Armor(string title, string description, Sprite sprite, int defense)
    {
        this.title = title;
        this.description = description;
        this.sprite = sprite;
        this.defense = defense;
    }
}
