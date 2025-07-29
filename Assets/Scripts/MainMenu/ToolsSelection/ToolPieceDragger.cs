using UnityEngine;
using DG.Tweening;
public class ToolPieceDragger : MonoBehaviour
{
    private Vector3 originalPosition;
    private ToolBoxSlot targetSlot;
    private bool isDraggable = true;
    int sortingOrder;

    void Start()
    {
        originalPosition = transform.position;
        sortingOrder = this.gameObject.GetComponent<SpriteRenderer>().sortingOrder;

    }

    void OnMouseDown()
    {
        
            // Only allow dragging if the piece is still draggable
            if (!isDraggable)
                return;

            this.GetComponent<Collider2D>().enabled = false; // Disable collider to allow free movement
        

    }
    void OnMouseDrag()
    {
        
            // Only allow dragging if the piece is still draggable
            if (!isDraggable)
                return;
            this.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 30 ;

            // Move the piece with the mouse
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);
        

    }

    void OnMouseUp()
    {
        
            // Check if the piece is over the correct slot
            ToolBoxSlot hitSlot = GetSlotUnderMouse();
            if (hitSlot != null && hitSlot.CanPlacePiece(this))
            {
                // Place the piece correctly
                //SoundManager.instance.PlaySingleSound(0);
                transform.position = hitSlot.transform.position;
                targetSlot = hitSlot;

            //transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad);
            transform.DOScale(Vector3.one * 0.55f, 0.3f).SetEase(Ease.OutQuad);


            transform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);

                // Mark the piece as no longer draggable
                isDraggable = false;
                this.gameObject.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;

                // Disable further dragging interactions
                GetComponent<Collider2D>().enabled = false;

                // Inform the GameController to check if the puzzle is complete
                ToolsSelectionManager.Instance.CheckCompletion();
            }
            else
            {
                // If not correctly placed, return to the original position
                transform.position = originalPosition;

                // Re-enable dragging if returned to original position
                GetComponent<Collider2D>().enabled = true;
            }
        

    }

    private ToolBoxSlot GetSlotUnderMouse()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<ToolBoxSlot>();
        }
        return null;
    }

    public bool IsPlacedCorrectly()
    {
        return targetSlot != null && targetSlot.IsCorrectPiece(this);
    }
}
