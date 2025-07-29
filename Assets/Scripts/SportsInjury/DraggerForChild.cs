using UnityEngine;

public class DraggerForChild : MonoBehaviour
{
    private Vector3 originalPosition;
    private bool isDragging = false;
    private SpriteRenderer[] spriteRenderers;
    private Color originalColor;
    private int originalSortOrder;

    public delegate void DragEndHandler(Transform draggedObject, Vector3 position);
    public event DragEndHandler OnDragEndEvent;

    void Awake()
    {
        spriteRenderers = gameObject.transform.GetComponentsInChildren<SpriteRenderer>();

        if (spriteRenderers.Length > 0)
        {
            originalColor = spriteRenderers[0].color;
            originalSortOrder = spriteRenderers[0].sortingOrder;
        }
    }

    void OnMouseDown()
    {
        originalPosition = transform.position;
        isDragging = true;

        // Make all sprites slightly transparent while dragging
        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                Color dragColor = originalColor;
                dragColor.a = 0.7f;
                spriteRenderer.color = dragColor;
            }
        }
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z; // Keep the original z position
            transform.position = mousePosition;

            // Adjust sorting order for all sprites
            foreach (var spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer != null)
                    spriteRenderer.sortingOrder = 200;
            }
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        // Restore original color and sorting order
        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
                spriteRenderer.sortingOrder = originalSortOrder;
            }
        }

        if (OnDragEndEvent != null)
        {
            OnDragEndEvent(transform, transform.position);
        }
    }
}
