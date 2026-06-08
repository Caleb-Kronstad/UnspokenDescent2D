using UnityEngine;

[System.Serializable]
public class Item
{
    protected string title;
    protected string description;
    protected Sprite sprite;

    public string GetTitle() { return title; }
    public string GetDescription() { return description; }
    public Sprite GetSprite() { return sprite; }

    public void SetTitle(string new_title) { title = new_title; }
    public void SetDescription(string new_description) { description = new_description; }
    public void SetSprite(Sprite new_sprite) { sprite = new_sprite; }
}
