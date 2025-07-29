using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ClickableSprite : MonoBehaviour
{
    [Header("Click Event")]
    public UnityEvent OnSpriteClicked;

    private void OnMouseDown()
    {
        // Trigger the click event
        OnSpriteClicked?.Invoke();
    }
}