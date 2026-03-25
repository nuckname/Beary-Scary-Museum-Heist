public interface IPickable
{
    void OnPickedUp();
    
    //This is different from the IThrowableItem's OnThrown() because
    //it is called by the PlayerGrabController when the item is released,
    //not by the PlayerThrowController when the item is thrown.
    //This allows for more flexibility in how items can be
    //released (dropped without throwing).
    void OnReleased();
    
    bool IsOnGround();
}