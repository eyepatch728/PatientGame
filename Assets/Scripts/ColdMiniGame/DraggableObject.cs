using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    public Vector3 originalPosition;
    public Quaternion originalRotation;
    public Vector3 initPosition;
    public Vector3 delayPosition;
    public bool isDragging = false;
    public bool isClicked = false;
    public bool isDragable = true;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public delegate void DragEndHandler(Transform draggedObject, Vector3 position);
    public delegate void DragRotationHandler(Transform draggedObject, Quaternion rotation);
    public delegate void DragClickHandler(Transform draggedObject, Vector3 position);
    public event DragEndHandler OnDragEndEvent;
    public event DragEndHandler OnDragEvent;
    public event DragRotationHandler OnDragStartEvent;
    public event DragClickHandler OnDragClickEvent;
    private int originalSortOrder;
    public bool changeSortingLayer = true;
    public bool changeChildSortingLayer = true;
    public bool changeColorAlpha = true;
    public bool changeChildColorAlpha = true;
    public int initialParentSortingOrder;
    public List<int> initialChildSortingOrders = new List<int>();


    // Add a flag to handle parent objects without SpriteRenderer
    private bool hasRenderer = false;

    // Reference to all child SpriteRenderers (optional)
    public SpriteRenderer[] childRenderers;

    void Awake()
    {
        isDragable = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
        hasRenderer = (spriteRenderer != null);

        if (hasRenderer)
        {
            originalColor = spriteRenderer.color;
            originalSortOrder = spriteRenderer.sortingOrder;
            childRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
        else
        {
            // Cache child renderers if the parent doesn't have one
            childRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

    }
    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f); // Wait for the end of the frame to ensure all components are initialized
        initPosition = transform.position;


        // Store the initial sorting order of the parent
        if (hasRenderer)
        {
            initialParentSortingOrder = spriteRenderer.sortingOrder;
        }

        // Store the initial sorting order for each child renderer
        if (childRenderers != null)
        {
            //print("childRenderers != null");
            initialChildSortingOrders.Clear(); // Clear any previous data
            foreach (var renderer in childRenderers)
            {
              //  print("var renderer != null");
                if (renderer != null)
                {
                    initialChildSortingOrders.Add(renderer.sortingOrder); // Add each child's initial sorting order
                }
            }
        }
        else
        {
            print("childRenderers == null");
        }
        Invoke(nameof(setPosAfter2Seconds), 1.5f);
    }

    public void setPosAfter2Seconds()
    {
        delayPosition = transform.position;

    }
    void OnMouseDown()
    {
        if (!isDragable)
            return;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        isDragging = true;
        isClicked = true;

        // Make it slightly transparent while dragging
        if (hasRenderer && changeColorAlpha)
        {
            Color dragColor = originalColor;
            // dragColor.a = 0.7f;
            spriteRenderer.color = dragColor;
        }
        else if (childRenderers != null && changeChildColorAlpha)
        {
            // Optional: You can modify child renderers during drag if needed
            foreach (var renderer in childRenderers)
            {
                if (renderer.gameObject.activeSelf)
                {
                    Color dragColor = renderer.color;
                    //dragColor.a = 0.7f;
                    renderer.color = dragColor;
                }
            }
        }
        if (!isDragable) return;

        isDragging = true;

        if (!didDrag && OnDragStartEvent != null)
        {
            OnDragStartEvent(transform, transform.rotation);
            didDrag = true;
        }
    }
    public bool didDrag;
    void OnMouseDrag()
    {
        if (!isDragable)
            return;

        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z; // Keep the original z position
            transform.position = mousePosition;

            if (OnDragEvent != null)
            {
                OnDragEvent(transform, transform.position);
            }

            // Add 200 to the initial sorting order of the parent renderer
            if (hasRenderer)
            {
                spriteRenderer.sortingOrder = initialParentSortingOrder + 2000;
            }

            // Add 200 to the initial sorting order of each child renderer
            if (childRenderers != null)
            {
                for (int i = 0; i < childRenderers.Length; i++)
                {
                    if (childRenderers[i] != null && childRenderers[i].gameObject.activeSelf)
                    {
                        // Use the corresponding initial sorting order from the list
                        childRenderers[i].sortingOrder = initialChildSortingOrders[i] + 2000;
                    }
                }
            }

           
        }
    }

    public void OnMouseUp()
    {
        isDragging = false;
        didDrag = false;
        // Restore original color
        if (hasRenderer)
        {
            spriteRenderer.color = originalColor;
        }
        else if (childRenderers != null)
        {
            // Optional: Restore child renderers' original state
            foreach (var renderer in childRenderers)
            {
                if (renderer.gameObject.activeSelf)
                {
                    // You might want to store the original colors somewhere if needed
                    Color originalChildColor = renderer.color;
                    originalChildColor.a = 1.0f;
                    renderer.color = originalChildColor;
                }
            }
        }

        if (OnDragEndEvent != null)
        {
            OnDragEndEvent(transform, transform.position);
        }
        //if (OnDragStartEvent != null)
        //{
        //    OnDragStartEvent(transform, transform.rotation);
        //}
    }

    public void finishOintment() 
    {
    
    
    }
}
