using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class OutfitSelectionManager : MonoBehaviour
{
    private int outfitId;
    private WardrobeManager wardrobeManager;
    public Button[] outfitButtons;
    public Button confirmButton;
    private Vector3 defaultScale = Vector3.one;
    private Vector3 enlargedScale = new Vector3(1.1f, 1.1f, 1.1f);
    private float animationDuration = 0.2f;

    private void Start()
    {
        outfitId = PlayerPrefs.GetInt("outfitid", 0);
        wardrobeManager = FindObjectOfType<WardrobeManager>();

        // Update the buttons to show the current selection
        UpdateButtonScales(outfitId);

        // Link the confirm button
        if (confirmButton != null && wardrobeManager != null)
        {
            wardrobeManager.confirmButton = confirmButton;
            confirmButton.gameObject.SetActive(false);
        }

        // Update wardrobe with initial outfit
        if (wardrobeManager != null)
        {
            wardrobeManager.UpdateWardrobeOutfit(outfitId);
        }
    }

    public void SetOutfitIndex(int outfitNumber)
    {
        if (outfitId != outfitNumber) // Only update if different
        {
            outfitId = outfitNumber;
            PlayerPrefs.SetInt("outfitid", outfitId);
            PlayerPrefs.Save();
            SoundManager.instance.PlayClick();
            // Notify WardrobeManager to update outfit display
           
            if (wardrobeManager != null)
            {
                wardrobeManager.UpdateWardrobeOutfit(outfitId);

                // NOTE: We're not resetting clothing status when changing outfits
                // This allows mix-and-match capability
                wardrobeManager.CheckOutfitComplete();
            }

            // Update button animation
            UpdateButtonScales(outfitId);
        }
    }

    private void UpdateButtonScales(int selectedId)
    {
        for (int i = 0; i < outfitButtons.Length; i++)
        {
            if (i == selectedId)
            {
                outfitButtons[i].transform.DOScale(enlargedScale, animationDuration).SetEase(Ease.OutBack);
            }
            else
            {
                outfitButtons[i].transform.DOScale(defaultScale, animationDuration).SetEase(Ease.OutBack);
            }
        }
    }

    public void OnConfirmPressed()
    {
        Debug.Log("Outfit confirmed! Moving to next screen...");
        SoundManager.instance.PlayClick();
        MenuManager.Instance.OutfitToNextStep();
    }
}