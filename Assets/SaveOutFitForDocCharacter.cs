using UnityEngine;

public class DoctorOutfitLoader : MonoBehaviour
{
    public Transform doctorCharacter;

    void Start()
    {
        LoadSavedOutfit();
    }

    void LoadSavedOutfit()
    {
        int coatId = PlayerPrefs.GetInt("SelectedCoatOutfit", 0);
        int accessoryId = PlayerPrefs.GetInt("SelectedAccessoryOutfit", 0);

        EnableCoatById(coatId);
        EnableAccessoryById(accessoryId);
    }

    void EnableCoatById(int id)
    {
        DisableAllCoats();

        switch (id)
        {
            case 0:
                EnableChild("Coat");
                EnableChild("BaseDress");
                EnableChild("GreenTie");
                EnableChild("SleevesForCoat");
                break;
            case 1:
                EnableChild("Coat");
                EnableChild("PinkDress");
                EnableChild("BlueTie");
                EnableChild("SleevesForCoat");
                break;
            case 2:
                EnableChild("Coat");
                EnableChild("Pants+Tshirt");
                EnableChild("SleevesForCoat");
                break;
        }
    }

    void EnableAccessoryById(int id)
    {
        DisableAllAccessories();

        switch (id)
        {
            case 0:
                EnableChild("PinkAccessory", true);
                break;
            case 1:
                EnableChild("BlueAccessory", true);
                break;
            case 2:
                EnableChild("Stethoscope", true);
                break;
        }
    }

    Transform EnableChild(string name, bool isAccessory = false)
    {
        Transform parent = isAccessory ? doctorCharacter.Find("HeadAccessories") : doctorCharacter;
        if (parent != null)
        {
            Transform child = parent.Find(name);
            if (child != null)
            {
                child.gameObject.SetActive(true);
                return child;
            }
        }
        return null;
    }


    void DisableAllCoats()
    {
        string[] coatParts = {
        "SleevesForCoat", "Coat", "BaseDress", "SleevesForBaseDress",
        "Pants+Tshirt", "PinkDress", "GreenTie", "BlueTie"
    };
        foreach (string part in coatParts)
        {
            Transform item = EnableChild(part);
            if (item != null) item.gameObject.SetActive(false);
        }
    }
    void DisableAllAccessories()
    {
        string[] accessories = { "PinkAccessory", "BlueAccessory", "Stethoscope" };
        foreach (string part in accessories)
        {
            Transform item = EnableChild(part, true);
            if (item != null) item.gameObject.SetActive(false);
        }
    }
}
