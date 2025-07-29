//using UnityEngine;
//using UnityEngine.UI; // For UI buttons
//using TMPro; // For TextMeshPro UI text, remove if not using

//public class ProgressTester : MonoBehaviour
//{
//    [Header("Testing Controls")]
//    [Tooltip("Input field to enter a specific patient count")]
//    public TMP_InputField patientCountInput; // Remove if not using TextMeshPro

//    [Tooltip("Optional label to show current patient count")]
//    public TextMeshProUGUI currentCountText; // Remove if not using TextMeshPro

//    private void Start()
//    {
//        // Initialize display
//        UpdateCurrentCountText();
//    }

//    /// <summary>
//    /// Updates the display of current patient count (if text element exists)
//    /// </summary>
//    public void UpdateCurrentCountText()
//    {
//        if (currentCountText != null && StarProgressManager.Instance != null)
//        {
//            int count = PlayerPrefs.GetInt("PatientsCured", 0);
//            int stars = PlayerPrefs.GetInt("StarsUnlocked", 0);
//            currentCountText.text = $"Patients: {count} | Stars: {stars}";
//        }
//    }

//    /// <summary>
//    /// Add one patient to the count
//    /// </summary>
//    public void AddOnePatient()
//    {
//        if (StarProgressManager.Instance != null)
//        {
//            StarProgressManager.Instance.AddCuredPatient();
//            UpdateCurrentCountText();
//        }
//    }

//    /// <summary>
//    /// Remove one patient from the count
//    /// </summary>
//    public void RemoveOnePatient()
//    {
//        if (StarProgressManager.Instance != null)
//        {
//            // Get current count
//            int currentCount = PlayerPrefs.GetInt("PatientsCured", 0);

//            // Decrease by 1 (minimum 0)
//            currentCount = Mathf.Max(0, currentCount - 1);

//            // Set the new value
//            SetPatientCount(currentCount);
//        }
//    }

//    /// <summary>
//    /// Called from a button to set patient count to a specific value
//    /// </summary>
//    public void SetPatientCountFromInput()
//    {
//        if (patientCountInput != null && !string.IsNullOrEmpty(patientCountInput.text))
//        {
//            if (int.TryParse(patientCountInput.text, out int newCount))
//            {
//                SetPatientCount(newCount);
//            }
//        }
//    }

//    /// <summary>
//    /// Set the patient count to a specific value
//    /// </summary>
//    public void SetPatientCount(int count)
//    {
//        if (StarProgressManager.Instance != null)
//        {
//            // Ensure count is non-negative
//            count = Mathf.Max(0, count);

//            // Set the value directly in PlayerPrefs
//            PlayerPrefs.SetInt("PatientsCured", count);
//            PlayerPrefs.Save();

//            Debug.Log($"Patient count manually set to: {count}");

//            // Force the manager to reload progress and update visuals
//            StarProgressManager.Instance.ForceRefresh();

//            UpdateCurrentCountText();
//        }
//        else
//        {
//            Debug.LogError("StarProgressManager instance not found!");
//        }
//    }

//    /// <summary>
//    /// Reset all progress
//    /// </summary>
//    public void ResetAllProgress()
//    {
//        if (StarProgressManager.Instance != null)
//        {
//            StarProgressManager.Instance.ResetAllProgress();
//            UpdateCurrentCountText();
//        }
//    }

//    /// <summary>
//    /// Run an automated test that cycles through each patient count to verify stars unlock at the correct thresholds
//    /// </summary>
//    public void RunAutomatedStarTest()
//    {
//        StartCoroutine(AutomatedStarTestCoroutine());
//    }

//    private System.Collections.IEnumerator AutomatedStarTestCoroutine()
//    {
//        // First reset everything
//        if (StarProgressManager.Instance != null)
//        {
//            StarProgressManager.Instance.ResetAllProgress();
//            UpdateCurrentCountText();

//            Debug.Log("=== STARTING AUTOMATED STAR TEST ===");

//            // Test each patient count from 0 to 31
//            // We go to 31 to verify the behavior after all stars are unlocked
//            for (int testCount = 0; testCount <= 31; testCount++)
//            {
//                // Set the patient count directly
//                SetPatientCount(testCount);

//                // Give the UI a moment to update
//                yield return new WaitForSeconds(0.5f);

//                // Calculate how many stars should be unlocked at this count
//                int expectedStars = 0;
//                for (int i = 0; i < 15; i++)
//                {
//                    int threshold = (i + 1) * 2;  // 2, 4, 6, 8... pattern
//                    if (testCount >= threshold)
//                        expectedStars++;
//                    else
//                        break;
//                }

//                // Get the actual number of stars unlocked
//                int actualStars = PlayerPrefs.GetInt("StarsUnlocked", 0);

//                // Log the result
//                string result = (expectedStars == actualStars) ? "PASS" : "FAIL";
//                Debug.Log($"Test Count {testCount}: Expected {expectedStars} stars, Got {actualStars} stars - {result}");

//                yield return new WaitForSeconds(0.2f);
//            }

//            Debug.Log("=== AUTOMATED STAR TEST COMPLETE ===");
//        }
//    }
//}
