using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    private int dust = 100; // amount of dust the player currently holds (currency for buying levels and items)
    private int vigor = 10; // number of vigor levels the player currently has
    private int endurance = 10; // number of endurance levels the player currently has
    private int might = 10; // number of might levels the player currently has

    private List<Item> inventory = new List<Item>();
    private Weapon equipped_weapon;
    private Armor equipped_armor;

    void Awake()
    {
        equipped_weapon = new Weapon("Iron Sword", "Basic sword made of Iron", null, 10);
        equipped_armor = new Armor("Chainmail", "Basic set of chainmail", null, 100);
    }

    public int GetDust() { return dust; }
    public int GetVigor() { return vigor; }
    public int GetEndurance() { return endurance; }
    public int GetMight() { return might; }
    public Weapon GetEquippedWeapon() { return equipped_weapon; }
    public Armor GetEquippedArmor() { return equipped_armor; }

    public void GiveDust(int amount) { dust += amount; }
    public void GiveVigor(int amount) { vigor += amount; }
    public void GiveEndurance(int amount) { endurance += amount; }
    public void GiveMight(int amount) { might += amount; }
    public void SetEquippedWeapon(Weapon weapon) { equipped_weapon = weapon; }
    public void SetEquippedArmor(Armor armor) { equipped_armor = armor; }

    public List<Item> GetInventory() { return inventory; }
    public void AddItemToInventory(Item item) { inventory.Add(item); }
    public void RemoveItemFromInventory(Item item) { inventory.Remove(item); }
}
