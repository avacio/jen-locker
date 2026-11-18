using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ClickableSprite : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        PhotoViewer.Instance.Open(_spriteRenderer.sprite);
    }
}
