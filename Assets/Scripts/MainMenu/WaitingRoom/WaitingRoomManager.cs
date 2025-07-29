using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class WaitingRoomManager : MonoBehaviour
{
    public static WaitingRoomManager Instance { get; private set; }

    public List<string> patientQueue = new List<string>();
    public List<string> allPatients = new List<string>
    {
        "DentalCare", "Vaccination", "FoodPoisoning", "Cold", "ChickenpoxTreatment",
        "Eyes", "SportsInjuryCare", "Allergies", "HealthCheck", "Pneumonia"
    };
    public List<string> emergencies = new List<string>
    {
        "AsthmaAttack", "Drowning", "DuckTrappedInPlastic"
    };
    public Transform[] chairPositions;
    public GameObject[] patientPrefabs;
    public GameObject resGameObject;

    public float emergencyChance = 0.2f;
    public float walkSpeed = 2.0f;
    public float exitPoint = 10.0f;
    public float entryPoint = -10.0f;
    public float patientEntryDelay = 1.5f;
    public float walkDuration = 1.5f;
    public Ease walkEase = Ease.InOutQuad;

    private Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();
    private Patient[] currentPatients;
    private Patient selectedPatient = null;
    private int lastTreatedChairIndex = -1;
    public bool isPatientMoving = false;
    public Transform sittingYPosForPatient;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f); // Ensure everything is initialized before checking for completed level
        currentPatients = new Patient[chairPositions.Length];
        BuildPrefabLookup();
        InitializePatients();
        CheckForCompletedLevel();
    }

    void CheckForCompletedLevel()
    {
        if (PlayerPrefs.GetInt("LevelCompleted", 0) == 1)
        {
            PlayerPrefs.SetInt("LevelCompleted", 0);
            string completedPatientType = PlayerPrefs.GetString("CompletedPatientType", "");
            OnPatientTreated();
            Debug.Log("Patient treated: " + completedPatientType);
        }
    }

    void BuildPrefabLookup()
    {
        prefabLookup.Clear();
        foreach (GameObject prefab in patientPrefabs)
        {
            Patient patientScript = null;
            if (prefab.CompareTag("Twin"))
                patientScript = prefab.transform.GetChild(0).GetComponent<Patient>();
            else
                patientScript = prefab.GetComponent<Patient>();

            if (patientScript != null && !prefabLookup.ContainsKey(patientScript.patientType))
            {
                //if (prefab.name == "DaughterVaccinationCharacter 1")
                    prefabLookup.Add(patientScript.patientType, prefab);
            }
        }
    }

    void InitializePatients()
    {
        patientQueue.Clear(); // clear any existing queue
        while (patientQueue.Count < 2)
        {
            AddNewPatientOrEmergency();
        }

        StartCoroutine(SpawnTwoPatientsCoroutine());
    }
    IEnumerator SpawnTwoPatientsCoroutine()
    {
        if (patientQueue.Count > 0)
        {
            SpawnPatientAtIndex(0);
        }

        // Wait 3 to 4 seconds before spawning the second patient
        float delay = Random.Range(3f, 4f);
        yield return new WaitForSeconds(delay);

        if (patientQueue.Count > 0)
        {
            SpawnPatientAtIndex(1);
        }
    }



    void AddNewPatientOrEmergency()
    {
        string newCase;
        int attempts = 0;
        const int maxAttempts = 10;

        do
        {
            if (Random.value < emergencyChance)
            {
                newCase = emergencies[Random.Range(0, emergencies.Count)];
            }
            else
            {
                newCase = allPatients[Random.Range(0, allPatients.Count)];
            }

            bool isDuplicate = false;

            // Check against currently seated patients
            foreach (Patient patient in currentPatients)
            {
                if (patient != null && patient.patientType == newCase)
                {
                    isDuplicate = true;
                    break;
                }
            }

            // Check against patients already in the queue
            if (!isDuplicate && patientQueue.Contains(newCase))
            {
                isDuplicate = true;
            }

            if (!isDuplicate)
            {
                patientQueue.Add(newCase);
                break;
            }

            attempts++;
        } while (attempts < maxAttempts);
    }


    IEnumerator SpawnAllPatientsCoroutine()
    {
        for (int i = 0; i < chairPositions.Length; i++)
        {
            if (patientQueue.Count > 0)
            {
                SpawnPatientAtIndex(i);
                yield return new WaitForSeconds(patientEntryDelay);
            }
        }
    }

    IEnumerator SpawnSinglePatientCoroutine(int chairIndex)
    {
        yield return new WaitForSeconds(0.5f);
        if (patientQueue.Count > 0)
        {
            SpawnPatientAtIndex(chairIndex);
        }
    }
    public Transform walkingPos;
    public Transform walkingPosForVaccination;
    void SpawnPatientAtIndex(int index)
    {
        if (currentPatients[index] == null && patientQueue.Count > 0)
        {
            string patientName = patientQueue[0];
            patientQueue.RemoveAt(0);

            GameObject prefab = GetPatientPrefab(patientName);
            if (prefab == null)
            {
                Debug.LogWarning($"No prefab found for patient type {patientName}");
                return;
            }
            Debug.Log($" prefab found for patient type {patientName}");

            Transform selectedWalkingPos = prefab.CompareTag("Twin") ? walkingPosForVaccination : walkingPos;

            Vector3 entryPosition = new Vector3(entryPoint, selectedWalkingPos.position.y, chairPositions[index].position.z);

            GameObject patientObj = Instantiate(prefab, entryPosition, Quaternion.identity);
            patientObj.transform.SetParent(resGameObject.transform, true);
            patientObj.transform.localScale = prefab.transform.localScale;

            if (!patientObj.CompareTag("Twin"))
            {
                Vector3 posOffset = patientObj.GetComponent<Patient>().GetTargetSitPosition2();
                Patient patient = patientObj.GetComponent<Patient>();
                patient.Setup(patientName);

                currentPatients[index] = patient;

                MovePatientToChair(patient, chairPositions[index].position + posOffset);
            }
            else
            {
                Vector3 posOffset = patientObj.transform.GetChild(0).GetComponent<Patient>().GetTargetSitPosition2();
                Patient patient = patientObj.transform.GetChild(0).GetComponent<Patient>();
                patient.Setup(patientName);

                currentPatients[index] = patient;

                MovePatientToChair(patient, chairPositions[index].position + posOffset);
            }
        }
    }


    void MovePatientToChair(Patient patient, Vector3 chairPosition)
    {
        // Don't adjust Y here — patient was instantiated at walkingPos.y
        Vector3 walkTarget = chairPosition;
        walkTarget.y = walkingPos.position.y; // Use walkingPos.y consistently

        patient.StandUp();

        float distance = Vector3.Distance(patient.transform.position, walkTarget);
        float duration = distance / walkSpeed;

        patient.transform.DOMove(walkTarget, duration)
            .SetEase(walkEase)
            .OnComplete(() =>
            {
                Vector3 sittingPos = patient.transform.position;
                sittingPos.y = sittingYPosForPatient.position.y;

                patient.transform.DOMoveY(sittingPos.y, 0.3f)
                    .SetEase(Ease.OutSine)
                    .OnComplete(() =>
                    {
                        patient.SitDown();
                    });
            });
    }









    public void OnPatientSelected(GameObject patientObject)
    {
        if (isPatientMoving) return;

        Patient patient = patientObject.GetComponent<Patient>();
        string sceneName = patient.patientType;

        int chairIndex = -1;
        for (int i = 0; i < currentPatients.Length; i++)
        {
            if (currentPatients[i] == patient)
            {
                chairIndex = i;
                break;
            }
        }

        if (chairIndex >= 0)
        {
            isPatientMoving = true;
            selectedPatient = patient;
            lastTreatedChairIndex = chairIndex;
            currentPatients[chairIndex] = null;
            MovePatientOutAndLoadScene(patient, sceneName);
        }
    }

    void MovePatientOutAndLoadScene(Patient patient, string sceneName)
    {
        patient.StandUp();

        // Step 1: Drop from sitting height to walking height
        float walkY = -3.96f; // Walking ground level
        Vector3 dropPos = new Vector3(patient.transform.position.x, walkingPos.transform.position.y, patient.transform.position.z);

        patient.transform.DOMoveY(dropPos.y, 0.3f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                // Step 2: Walk to the exit slowly
                Vector3 exitPosition = new Vector3(exitPoint, walkingPos.transform.position.y, patient.transform.position.z);

                patient.transform.DOMove(exitPosition, walkDuration + 1.5f) // Slower movement
                    .SetEase(Ease.InOutQuad)
                    .SetDelay(0.2f)     
                    .OnComplete(() =>
                    {
                        SceneManager.LoadScene(sceneName);
                        SoundManager.instance.PlayScenesMusic();

                        isPatientMoving = false;
                    });
            });
    }


    GameObject GetPatientPrefab(string patientType)
    {
        if (prefabLookup.TryGetValue(patientType, out GameObject prefab))
        {
            return prefab;
        }
        return null;
    }

    public void OnPatientTreated()
    {
        if (selectedPatient != null)
        {
            Destroy(selectedPatient.gameObject);
            selectedPatient = null;
        }

        // Make sure queue has at least 1 new patient
        while (patientQueue.Count < 1)
        {
            AddNewPatientOrEmergency();
        }

        StartCoroutine(SpawnRemainingPatientCoroutine());
    }
    IEnumerator SpawnRemainingPatientCoroutine()
    {
        for (int i = 0; i < 2; i++)
        {
            if (currentPatients[i] == null && patientQueue.Count > 0)
            {
                SpawnPatientAtIndex(i);
                yield return new WaitForSeconds(patientEntryDelay);
                break;
            }
        }
    }


    IEnumerator SpawnLimitedPatientsCoroutine()
    {
        int count = Mathf.Min(patientQueue.Count, chairPositions.Length);
        for (int i = 0; i < count; i++)
        {
            if (currentPatients[i] == null)
            {
                SpawnPatientAtIndex(i);
                yield return new WaitForSeconds(patientEntryDelay);
            }
        }
    }
}
