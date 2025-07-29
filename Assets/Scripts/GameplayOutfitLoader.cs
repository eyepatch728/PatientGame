using UnityEngine;

public class GameplayOutfitLoader : MonoBehaviour
{
    public Transform doctorCharacter;

    private void Start()
    {
        if (doctorCharacter == null)
        {
            doctorCharacter = GameObject.Find("DoctorCharacter")?.transform;
        }

        int coatId = PlayerPrefs.GetInt("selectedCoat", -1);
        int accessoryId = PlayerPrefs.GetInt("selectedAccessory", -1);

        //Debug.Log($"🔍 Loaded Outfit - Coat: {coatId}, Accessory: {accessoryId}");

        if (coatId == -1 || accessoryId == -1)
        {
            //Debug.LogError("❌ Outfit not found in PlayerPrefs! Make sure it was saved in WardrobeManager.");
        }
        else
        {
            ApplyCoatOutfit(coatId);
            ApplyAccessoryOutfit(accessoryId);
        }
    }

    private void ApplyCoatOutfit(int outfitId)
    {
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

    private void ApplyAccessoryOutfit(int outfitId)
    {
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
            Debug.LogWarning($"⚠️ Could not find {objectName} in doctor character");
        }
    }
}