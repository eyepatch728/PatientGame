using UnityEngine;

public class ToolBoxSlot : MonoBehaviour
{
    public ToolPieceDragger correctPiece;

    // Check if the piece is correctly placed
    public bool IsPieceCorrectlyPlaced()
    {
        return correctPiece != null && correctPiece.transform.position == transform.position;
    }

    // Check if a piece can be placed in this slot
    public bool CanPlacePiece(ToolPieceDragger piece)
    {
        return correctPiece == piece;
    }

    // Check if the given piece is the correct one
    public bool IsCorrectPiece(ToolPieceDragger piece)
    {
        return piece == correctPiece;
    }
}
