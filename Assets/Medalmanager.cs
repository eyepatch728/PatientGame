using UnityEngine;

public class StarProgressManager : MonoBehaviour
{
    public static StarProgressManager Instance;
    private const string CURRENT_PATIENTS_KEY = "CurrentPatients";
    private const string STARS_UNLOCKED_KEY = "StarsUnlocked";

    public int currentPatients = 0; // Current patients treated for the next star
    public int starsUnlocked = 0;   // Total stars unlocked

    private const int MAX_STARS = 15;

    // Patients required for each star (resets after each star unlock)
    private int GetPatientsRequiredForNextStar()
    {
        return (starsUnlocked + 1) * 2; // 1st star = 2, 2nd star = 4, 3rd star = 6, etc.
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddCuredPatient()
    {
        if (starsUnlocked >= MAX_STARS)
        {
            Debug.Log("[AddCuredPatient] All stars already unlocked!");
            return;
        }

        currentPatients++;
        int requiredPatients = GetPatientsRequiredForNextStar();

        Debug.Log($"[AddCuredPatient] Current: {currentPatients}/{requiredPatients} for star {starsUnlocked + 1}");

        // Check if we've unlocked a new star
        if (currentPatients >= requiredPatients)
        {
            starsUnlocked++;
            currentPatients = 0; // Reset counter for next star
            Debug.Log($"[AddCuredPatient] ⭐ Star {starsUnlocked} unlocked! Counter reset to 0");
        }

        SaveProgress();

        // Update any UI that exists in the current scene
        StarProgressUI uiManager = FindObjectOfType<StarProgressUI>();
        if (uiManager != null)
        {
            uiManager.UpdateDisplay();
        }
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt(CURRENT_PATIENTS_KEY, currentPatients);
        PlayerPrefs.SetInt(STARS_UNLOCKED_KEY, starsUnlocked);
        PlayerPrefs.Save();
        Debug.Log($"[SaveProgress] Saved Current: {currentPatients}, Stars: {starsUnlocked}");
    }

    public void LoadProgress()
    {
        currentPatients = PlayerPrefs.GetInt(CURRENT_PATIENTS_KEY, 0);
        starsUnlocked = PlayerPrefs.GetInt(STARS_UNLOCKED_KEY, 0);
        Debug.Log($"[LoadProgress] Loaded Current: {currentPatients}, Stars: {starsUnlocked}");
    }

    public void ResetProgress()
    {
        currentPatients = 0;
        starsUnlocked = 0;
        PlayerPrefs.DeleteKey(CURRENT_PATIENTS_KEY);
        PlayerPrefs.DeleteKey(STARS_UNLOCKED_KEY);
        PlayerPrefs.Save();
        Debug.Log("[ResetProgress] Progress reset");

        // Update UI if it exists
        StarProgressUI uiManager = FindObjectOfType<StarProgressUI>();
        if (uiManager != null)
        {
            uiManager.UpdateDisplay();
        }
    }

    // Public method to get current progress info (for UI display)
    public string GetProgressText()
    {
        if (starsUnlocked >= MAX_STARS)
            return "All Stars Unlocked!";

        int required = GetPatientsRequiredForNextStar();
        return $"Star {starsUnlocked + 1}: {currentPatients}/{required} patients";
    }
}