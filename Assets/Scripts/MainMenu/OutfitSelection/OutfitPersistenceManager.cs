using UnityEngine;

public class OutfitPersistenceManager : MonoBehaviour
{
    // Singleton pattern
    public static OutfitPersistenceManager Instance;

    // Keys for PlayerPrefs
    private const string COAT_OUTFIT_KEY = "selectedCoatOutfit";
    private const string ACCESSORY_OUTFIT_KEY = "selectedAccessoryOutfit";
    private const string HAS_OUTFIT_SAVED_KEY = "hasOutfitSaved";

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Save the current outfit configuration
    public void SaveOutfitConfiguration(int coatOutfitId, int accessoryOutfitId)
    {
        PlayerPrefs.SetInt(COAT_OUTFIT_KEY, coatOutfitId);
        PlayerPrefs.SetInt(ACCESSORY_OUTFIT_KEY, accessoryOutfitId);
        PlayerPrefs.SetInt(HAS_OUTFIT_SAVED_KEY, 1);
        PlayerPrefs.Save();

        Debug.Log($"Saved outfit configuration: Coat={coatOutfitId}, Accessory={accessoryOutfitId}");
    }

    // Get the saved coat outfit ID
    public int GetSavedCoatOutfitId()
    {
        return PlayerPrefs.GetInt(COAT_OUTFIT_KEY, 0);
    }

    // Get the saved accessory outfit ID
    public int GetSavedAccessoryOutfitId()
    {
        return PlayerPrefs.GetInt(ACCESSORY_OUTFIT_KEY, 0);
    }

    // Check if an outfit has been saved
    public bool HasSavedOutfit()
    {
        return PlayerPrefs.GetInt(HAS_OUTFIT_SAVED_KEY, 0) == 1;
    }

    // Reset saved outfit data
    public void ResetSavedOutfit()
    {
        PlayerPrefs.DeleteKey(COAT_OUTFIT_KEY);
        PlayerPrefs.DeleteKey(ACCESSORY_OUTFIT_KEY);
        PlayerPrefs.SetInt(HAS_OUTFIT_SAVED_KEY, 0);
        PlayerPrefs.Save();
    }
}