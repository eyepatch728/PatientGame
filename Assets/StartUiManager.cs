using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// This script handles ONLY the UI display and goes on each scene that needs to show stars
public class StarProgressUI : MonoBehaviour
{
    public List<GameObject> starObjects = new List<GameObject>();
    public Transform progressBarFill;
    private float progressBarOriginalYScale;

    private int[] starUnlockThresholds = new int[] { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 };

    private void Start()
    {
        // Store initial Y scale of the progress bar
        if (progressBarFill != null)
        {
            progressBarOriginalYScale = progressBarFill.localScale.y;
        }

        // Update display when this UI component starts
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (StarProgressManager.Instance == null) return;

        int currentPatients = StarProgressManager.Instance.currentPatients;
        int starsUnlocked = StarProgressManager.Instance.starsUnlocked;

        // Update stars
        for (int i = 0; i < starObjects.Count; i++)
        {
            if (starObjects[i] != null)
            {
                starObjects[i].SetActive(i < starsUnlocked);
            }
        }

        // Update progress bar
        if (progressBarFill != null)
        {
            float progress = 0f;
            const int MAX_STARS = 15;

            if (starsUnlocked < MAX_STARS)
            {
                int requiredPatients = (starsUnlocked + 1) * 2; // Same formula as in StarProgressManager
                progress = Mathf.Clamp01((float)currentPatients / requiredPatients);
            }
            else
            {
                progress = 1f; // All stars unlocked
            }

            Vector3 scale = progressBarFill.localScale;
            scale.y = progressBarOriginalYScale * progress;
            progressBarFill.localScale = scale;
        }
    }
}
