using UnityEngine;

public class OutfitDragger : MonoBehaviour
{
    private Vector3 offset;
    public Vector3 initialPosition;
    public Transform coatDropArea;
    public Transform accessoryDropArea;
    public float coatDropRadius = 1.5f;   // Separate drop radius for coats
    public float accessoryDropRadius = 3.5f;
    private Camera mainCamera;
    private bool isDragging = false;
    private SpriteRenderer spriteRenderer;

    // Reference to the doctor character
    public Transform doctorCharacter;

    // Identify what type of item this is
    public enum OutfitItemType { Coat, Accessory }
    public OutfitItemType itemType;

    // Outfit ID this item belongs to (0, 1, 2 for your three outfits)
    public int outfitId;

    private const string CoatOutfitKey = "SelectedCoatOutfit";
    private const string AccessoryOutfitKey = "SelectedAccessoryOutfit";


    // Reference to the WardrobeManager
    private WardrobeManager wardrobeManager;

    void Start()
    {
        mainCamera = Camera.main;
        initialPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        wardrobeManager = FindObjectOfType<WardrobeManager>();

        if (doctorCharacter == null)
        {
            doctorCharacter = GameObject.Find("DoctorCharacter")?.transform;
        }
    }

    void OnMouseDown()
    {
        offset = transform.position - GetMouseWorldPosition();
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
            spriteRenderer.sortingOrder = 100; // Bring to front while dragging
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        // Check if dropped on doctor
        if (IsDroppedInCorrectArea())
        {
            // Apply the outfit item to the doctor
            ApplyOutfitToDoctor();
        }
        else
        {
            // Return to initial position if not dropped on doctor
            transform.position = initialPosition;
        }

        // Reset sorting order
        spriteRenderer.sortingOrder = 0;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10f;
        return mainCamera.ScreenToWorldPoint(mousePosition);
    }
    private bool IsDroppedInCorrectArea()
    {
        if (itemType == OutfitItemType.Coat)
        {
            return IsWithinDropZone(transform.position, coatDropArea, coatDropRadius);
        }
        else if (itemType == OutfitItemType.Accessory)
        {
            return IsWithinDropZone(transform.position, accessoryDropArea, accessoryDropRadius);
        }
        return false;
    }

    private bool IsWithinDropZone(Vector3 itemPosition, Transform dropZone, float dropRadius)
    {
        if (dropZone == null) return false;
        return Vector3.Distance(itemPosition, dropZone.position) < dropRadius;
    }

    private bool IsDroppedOnDoctor()
    {
        if (doctorCharacter == null) return false;

        // You might want to use colliders for more accurate detection
        // This is a simple distance-based check
        float dropThreshold = 2.0f;
        return Vector3.Distance(transform.position, doctorCharacter.position) < dropThreshold;
    }

    private void ApplyOutfitToDoctor()
    {
        if (wardrobeManager == null || doctorCharacter == null) return;

        // Always return to hanger position after drop
        transform.position = initialPosition;

        if (itemType == OutfitItemType.Coat)
        {
            if (wardrobeManager.currentCoatOutfitId == outfitId)
            {
                Debug.Log("Same coat already equipped, skipping outfit change.");
                return;
            }

            ApplyCoatOutfit(outfitId);
            wardrobeManager.hasClothes = 1;
            wardrobeManager.currentCoatOutfitId = outfitId;

            PlayerPrefs.SetInt(CoatOutfitKey, outfitId);
            PlayerPrefs.Save();

            PlayOutfitVoiceLine(outfitId);
        }
        else if (itemType == OutfitItemType.Accessory)
        {
            if (wardrobeManager.currentAccessoryOutfitId == outfitId)
            {
                Debug.Log("Same accessory already equipped, skipping outfit change.");
                return;
            }

            ApplyAccessoryOutfit(outfitId);
            wardrobeManager.hasAccessory = 1;
            wardrobeManager.currentAccessoryOutfitId = outfitId;

            PlayerPrefs.SetInt(AccessoryOutfitKey, outfitId);
            PlayerPrefs.Save();

            PlayOutfitVoiceLine(outfitId);
        }

        wardrobeManager.CheckOutfitComplete();
    }

    private void PlayOutfitVoiceLine(int id)
    {
        switch (id)
        {
            case 0:
                SoundManager.instance.PlayWowAfterOutfit();
                break;
            case 1:
                SoundManager.instance.PlayGreatChoiceAfterOutfit();
                break;
            case 2:
                SoundManager.instance.PlayLooksPerfectAfterOutfit();
                break;
        }
    }

    private void ApplyCoatOutfit(int outfitId)
    {
        // Reset all coat elements
        SetGameObjectActive("SleevesForCoat", false);
        SetGameObjectActive("Coat", false);
        SetGameObjectActive("BaseDress", false);
        SetGameObjectActive("SleevesForBaseDress", false);
        SetGameObjectActive("Pants+Tshirt", false);
        SetGameObjectActive("PinkDress", false);
        SetGameObjectActive("GreenTie", false);
        SetGameObjectActive("BlueTie", false);


        // Apply specific outfit elements
        switch (outfitId)
        {
            case 0: // Outfit 1
                SetGameObjectActive("SleevesForCoat", true);
                SetGameObjectActive("Coat", true);
                SetGameObjectActive("BaseDress", true);
                SetGameObjectActive("GreenTie", true);
                break;

            case 1: // Outfit 2
                SetGameObjectActive("SleevesForCoat", true);
                SetGameObjectActive("Coat", true);
                SetGameObjectActive("PinkDress", true);
                SetGameObjectActive("BlueTie", true);
                break;

            case 2: // Outfit 3
                SetGameObjectActive("SleevesForCoat", true);
                SetGameObjectActive("Coat", true);
                SetGameObjectActive("Pants+Tshirt", true);
                break;
        }
    }

    private void ApplyAccessoryOutfit(int outfitId)
    {
        // Reset accessory elements
        SetGameObjectActive("PinkAccessory", false);
        SetGameObjectActive("BlueAccessory", false);
        SetGameObjectActive("Stethoscope", false);

        // Apply specific accessory
        switch (outfitId)
        {
            case 0: // Outfit 1
                SetGameObjectActive("PinkAccessory", true);
                break;

            case 1: // Outfit 2
                SetGameObjectActive("BlueAccessory", true);
                break;

            case 2: // Outfit 3
                SetGameObjectActive("Stethoscope", true);
                break;
        }
    }

    private void SetGameObjectActive(string objectName, bool active)
    {
        Transform obj = doctorCharacter.Find(objectName);
        if (obj != null)
        {
            obj.gameObject.SetActive(active);
            Debug.Log($"Set {objectName} to {active}");
        }
        else
        {
            // Check in HeadAccessories for accessories
            Transform headAccessories = doctorCharacter.Find("HeadAccessories");
            if (headAccessories != null)
            {
                obj = headAccessories.Find(objectName);
                if (obj != null)
                {
                    obj.gameObject.SetActive(active);
                    Debug.Log($"Set HeadAccessories/{objectName} to {active}");
                    return;
                }
            }
            Debug.LogWarning($"Could not find {objectName} in doctor character");
        }
    }
}