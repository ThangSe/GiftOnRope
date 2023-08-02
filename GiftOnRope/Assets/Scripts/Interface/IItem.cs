using UnityEngine;

public interface IItem
{
    public void Act();

    public void Active(Vector3 pos, Vector3 dir);

    public void Active(Vector3 pos, Vector3 dir, string text);

    public void Deactive();

    public bool GetActiveState();

    public void Setting(Sprite newSprite, int index);

    public bool CompareTagGO (string tag);
}