using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WardrobeManager : MonoBehaviour
{
    public List<GameObject> outfitsOnHangers = new List<GameObject>();
    public List<GameObject> coatItems = new List<GameObject>();
    public List<GameObject> accessoryItems = new List<GameObject>();

    public Button confirmButton;
    public Transform doctorCharacter;

    [HideInInspector] public int hasClothes;
    [HideInInspector] public int hasAccessory;

    [HideInInspector] public int currentCoatOutfitId = -1;
    [HideInInspector] public int currentAccessoryOutfitId = -1;

    private void Start()
    {
        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);

        if (doctorCharacter == null)
            doctorCharacter = GameObject.Find("DoctorCharacter")?.transform;

        int outfitId = PlayerPrefs.GetInt("outfitid", 0);
        UpdateWardrobeOutfit(outfitId);

        hasClothes = 0;
        hasAccessory = 0;
        ResetCharacterOutfit();
        ApplyAccessoryOutfit(outfitId);
        ApplyCoatOutfit(outfitId);
    }

    private void ResetCharacterOutfit()
    {
        SetObjectActive("SleevesForCoat", false);
        SetObjectActive("Coat", false);
        SetObjectActive("Pants+Tshirt", false);
        SetObjectActive("PinkDress", false);
        SetObjectActive("GreenTie", false);
        SetObjectActive("BlueTie", false);
        SetObjectActive("PinkAccessory", false);
        SetObjectActive("BlueAccessory", false);
        SetObjectActive("Stethoscope", false);
        SetObjectActive("BaseDress", false);
        SetObjectActive("SleevesForBaseDress", false);
    }

    private void SetObjectActive(string objectName, bool active)
    {
        if (doctorCharacter == null) return;

        Transform obj = doctorCharacter.Find(objectName);
        if (obj != null)
        {
            obj.gameObject.SetActive(active);
        }
        else
        {
            Transform headAccessories = doctorCharacter.Find("HeadAccessories");
            if (headAccessories != null)
            {
                obj = headAccessories.Find(objectName);
                if (obj != null)
                {
                    obj.gameObject.SetActive(active);
                    return;
                }
            }
            Debug.LogWarning($"Could not find {objectName} in doctor character");
        }
    }

    public void UpdateWardrobeOutfit(int outfitId)
    {
        if (outfitId < 0 || outfitId >= outfitsOnHangers.Count)
        {
            Debug.LogWarning($"Invalid hanger outfit ID: {outfitId}");
            return;
        }

        foreach (var hanger in outfitsOnHangers)
            if (hanger != null) hanger.SetActive(false);

        if (outfitId < outfitsOnHangers.Count && outfitsOnHangers[outfitId] != null)
            outfitsOnHangers[outfitId].SetActive(true);

        EnableAllDraggableItems();
    }

    private void EnableAllDraggableItems()
    {
        foreach (var coat in coatItems)
            if (coat != null) coat.SetActive(true);

        foreach (var accessory in accessoryItems)
            if (accessory != null) accessory.SetActive(true);
    }

    public void CheckOutfitComplete()
    {
        if (hasClothes == 1 && hasAccessory == 1)
        {
            if (confirmButton != null)
                confirmButton.gameObject.SetActive(true);
            Debug.Log("Doctor is fully dressed! Confirm button activated.");
        }
        else
        {
            if (confirmButton != null)
                confirmButton.gameObject.SetActive(false);
        }
    }

    public void ConfirmOutfit()
    {
        Debug.Log($"🔹 Attempting to save outfit - Coat: {currentCoatOutfitId}, Accessory: {currentAccessoryOutfitId}");

        if (currentCoatOutfitId >= 0 && currentAccessoryOutfitId >= 0)
        {
            PlayerPrefs.SetInt("selectedCoat", currentCoatOutfitId);
            PlayerPrefs.SetInt("selectedAccessory", currentAccessoryOutfitId);
            PlayerPrefs.Save();

            Debug.Log($"✅ Outfit confirmed and saved! Coat: {currentCoatOutfitId}, Accessory: {currentAccessoryOutfitId}");

            MenuManager.Instance.OutfitToNextStep();
        }
        else
        {
            Debug.LogError("❌ Cannot confirm outfit: Invalid outfit IDs!");
        }
    }

    public void ApplyCoatOutfit(int outfitId)
    {
        if (currentCoatOutfitId == outfitId)
        {
            Debug.Log("Same coat outfit already applied. Returning item to hanger.");
            ReturnItemToInitialPosition(coatItems, outfitId);
            return;
        }

        currentCoatOutfitId = outfitId;
        hasClothes = 1;
        CheckOutfitComplete();

        SetObjectActive("SleevesForCoat", false);
        SetObjectActive("Coat", false);
        SetObjectActive("BaseDress", false);
        SetObjectActive("SleevesForBaseDress", false);
        SetObjectActive("Pants+Tshirt", false);
        SetObjectActive("PinkDress", false);
        SetObjectActive("GreenTie", false);
        SetObjectActive("BlueTie", false);

        switch (outfitId)
        {
            case 0:
                SetObjectActive("SleevesForCoat", true);
                SetObjectActive("Coat", true);
                SetObjectActive("BaseDress", true);
                SetObjectActive("GreenTie", true);
                break;
            case 1:
                SetObjectActive("SleevesForCoat", true);
                SetObjectActive("Coat", true);
                SetObjectActive("PinkDress", true);
                SetObjectActive("BlueTie", true);
                break;
            case 2:
                SetObjectActive("SleevesForCoat", true);
                SetObjectActive("Coat", true);
                SetObjectActive("Pants+Tshirt", true);
                break;
        }
    }


    public void ApplyAccessoryOutfit(int outfitId)
    {
        if (currentAccessoryOutfitId == outfitId)
        {
            Debug.Log("Same accessory already applied. Returning item to hanger.");
            ReturnItemToInitialPosition(accessoryItems, outfitId);
            return;
        }

        currentAccessoryOutfitId = outfitId;
        hasAccessory = 1;
        CheckOutfitComplete();

        SetObjectActive("PinkAccessory", false);
        SetObjectActive("BlueAccessory", false);
        SetObjectActive("Stethoscope", false);

        switch (outfitId)
        {
            case 0:
                SetObjectActive("PinkAccessory", true);
                break;
            case 1:
                SetObjectActive("BlueAccessory", true);
                break;
            case 2:
                SetObjectActive("Stethoscope", true);
                break;
        }
    }
    private void ReturnItemToInitialPosition(List<GameObject> items, int index)
    {
        if (index < 0 || index >= items.Count) return;

        GameObject item = items[index];
        if (item != null)
        {
            // Disable dragging or play return animation here if you want
            item.SetActive(true); // Or reset its position manually if needed
            item.transform.localPosition = item.GetComponent<OutfitDragger>().initialPosition;
        }
    }

}
