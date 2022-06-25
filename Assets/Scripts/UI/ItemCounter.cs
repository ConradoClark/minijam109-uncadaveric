using System.Collections;
using System.Collections.Generic;
using Licht.Unity.Objects;
using UnityEngine;

public class ItemCounter : BaseUIObject
{
    public SpriteRenderer SpriteRenderer;
    public float ItemSize;
    public float InitialSize;
    public int ItemCount;
    public int ItemMaximum;

    public void GiveItem(int amount)
    {
        ItemCount += amount;
        if (ItemCount > ItemMaximum) ItemCount = ItemMaximum;
        Render();
    }

    public void RemoveItem(int amount)
    {
        ItemCount -= amount;
        if (ItemCount < 0) ItemCount = 0;
        Render();
    }

    public bool UseItem()
    {
        if (ItemCount < 1) return false;
        ItemCount--;
        Render();
        return true;
    }

    private void OnEnable()
    {
        Render();
    }

    private void Render()
    {
        SpriteRenderer.size = new Vector2(InitialSize + ItemCount * ItemSize, SpriteRenderer.size.y);
    }
}
