using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PickRandomImage : MonoBehaviour
{
    [SerializeField] private Sprite[] impactSprites;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        Sprite selectedSprite = impactSprites[Random.Range(0, impactSprites.Length)];
        spriteRenderer.sprite = selectedSprite;
    }
}