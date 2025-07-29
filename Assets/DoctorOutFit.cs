 using UnityEngine;

public class OutfitLoader : MonoBehaviour
{
    [Header("Outfit References")]
    public SpriteRenderer coat;
    public SpriteRenderer shirt; // Use this for both pink and brown shirts
    public SpriteRenderer pants;
    public SpriteRenderer tie;

    [Header("Accessory References")]
    public SpriteRenderer hat;
    public SpriteRenderer light;
    public SpriteRenderer stethoscope;

    [Header("Outfit Sprites")]
    public Sprite coatSprite;
    public Sprite brownShirtSprite;
    public Sprite pinkShirtSprite;
    public Sprite pantsSprite;
    public Sprite greenTieSprite;
    public Sprite blueTieSprite;

    [Header("Accessory Sprites")]
    public Sprite hatSprite;
    public Sprite lightSprite;
    public Sprite stethoscopeSprite;

    private void Start()
    {
        int selectedCoat = PlayerPrefs.GetInt("SelectedCoatOutfit", 0);
        int selectedAccessory = PlayerPrefs.GetInt("SelectedAccessoryOutfit", 0);

        Debug.Log($"🧥 Loaded Coat ID: {selectedCoat}, Accessory ID: {selectedAccessory}");

        ApplyOutfit(selectedCoat);
        ApplyAccessory(selectedAccessory);
    }

    private void ApplyOutfit(int id)
    {
        // Reset all outfit parts
        SetSpriteAndVisibility(coat, null);
        SetSpriteAndVisibility(shirt, null);
        SetSpriteAndVisibility(pants, null);
        SetSpriteAndVisibility(tie, null);

        switch (id)
        {
            case 0: // Coat + Green Tie + Brown Shirt
                SetSpriteAndVisibility(coat, coatSprite);
                SetSpriteAndVisibility(tie, greenTieSprite);
                SetSpriteAndVisibility(shirt, brownShirtSprite);
                break;
            case 1: // Coat + Blue Tie + Pink Shirt
                SetSpriteAndVisibility(coat, coatSprite);
                SetSpriteAndVisibility(tie, blueTieSprite);
                SetSpriteAndVisibility(shirt, pinkShirtSprite);
                break;
            case 2: // Coat + Pink Shirt + Pants
                SetSpriteAndVisibility(coat, coatSprite);
                SetSpriteAndVisibility(shirt, pinkShirtSprite);
                SetSpriteAndVisibility(pants, pantsSprite);
                break;
            default:
                Debug.LogWarning("⚠️ Invalid outfit ID.");
                break;
        }
    }

    private void ApplyAccessory(int id)
    {
        // Reset all accessories
        SetSpriteAndVisibility(hat, null);
        SetSpriteAndVisibility(light, null);
        SetSpriteAndVisibility(stethoscope, null);

        switch (id)
        {
            case 0:
                SetSpriteAndVisibility(hat, hatSprite);
                break;
            case 1:
                SetSpriteAndVisibility(light, lightSprite);
                break;
            case 2:
                SetSpriteAndVisibility(stethoscope, stethoscopeSprite);
                break;
            default:
                Debug.LogWarning("⚠️ Invalid accessory ID.");
                break;
        }
    }

    private void SetSpriteAndVisibility(SpriteRenderer renderer, Sprite sprite)
    {
        if (renderer == null) return;

        if (sprite == null)
        {
            renderer.gameObject.SetActive(false);
        }
        else
        {
            renderer.sprite = sprite;
            renderer.gameObject.SetActive(true);
        }
    }
}