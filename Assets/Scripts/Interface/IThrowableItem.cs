
using UnityEngine;

public interface IThrowableItem
{
    void OnThrown(Vector3 velocity);
    public bool CanThrowItem { get; }
    public ItemType ItemType { get; }
}