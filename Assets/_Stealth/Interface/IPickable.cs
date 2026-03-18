public interface IPickable
{
    float Weight { get; }
    void OnPickedUp();
    void OnReleased();
}