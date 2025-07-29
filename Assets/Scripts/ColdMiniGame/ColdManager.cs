using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using static EyeProblemManager;

public class ColdManager : MonoBehaviour
{
    [Header("Camera")]
    public Camera camera;

    [Header("Doctor Objects")]
    public GameObject doctorObjects;
    [Header("Character Objects")]
    public Animator characterAnim;
    public GameObject story;
    public GameObject characterObj;
    public GameObject characterChair;
    public GameObject characterBottom;
    public Transform sittingPosForPatient;
    public Transform WalkingStartPosForPatient;
    [Header("Game Settings")]
    [Tooltip("Set to true to skip the 'Kill the Virus' step")]
    public bool skipVirusStep = false;

    [Header("Problem Visualization")]
    public Transform[] problemSprites;

    [Header("Step 1 - Nose Wiping")]
    public Transform Step1Gameobject;
    public Transform tissueBox;
    public Transform tissue;
    public Transform usedTissue;
    public Transform trashCan;
    public Transform catNose;
    public Transform catNoseGerms;

    [Header("Step 2 - Temperature")]
    public Transform Step2Gameobject;
    public Transform thermometerTray;
    public Transform thermometer;
    public Transform redSpot;
    public Transform thermometerLight;
    public Transform thermBar;
    public Image temperatureDisplay;
    public Image progressBar;

    [Header("Step 3 - Kill the Virus")]
    public Transform Step3Gameobject;
    public Transform xRayMachine;
    public Transform exclamationMark;
    public Transform[] viruses;
    public Transform gunDevice;
    public Transform greenArea;
    public Transform greenAreaPoint;
    private bool isDragging1 = false;        // Whether the gun is currently being dragged
    [SerializeField] private float shootDistance = 0.6f;       // Max distance for a successful "hit" with the green area
    [SerializeField] private float holdDurationPerTarget = 2f;

    [Header("Step 4 - Give the Pill")]
    public Transform Step4Gameobject;
    public Transform pillTray;
    public Transform pill;
    public Transform waterGlassTray;
    public Transform waterGlass;
    public Transform emptyGlass;
    public Transform catMouth;
    public Transform catMouthOpen;
    public Transform catDrinkingWater;

    [Header("Step 5 - Recovery")]
    public Transform Step5Gameobject;
    public Transform happyMouth;
    public Transform[] recoveryAnimationSprites;
    public Transform happyCat;

    private bool isStep1Complete = false;
    private bool isStep2Complete = false;
    private bool isStep3Complete = false;
    private bool isStep4Complete = false;
    private bool isStep5Complete = false;
    private bool isDraggingTissue = false;
    private bool isDraggingThermometer = false;
    private bool isNoseWiped = false;
    private int virusesDestroyed = 0;

    void Start()
    {
        // Initialize all elements
        characterAnim.keepAnimatorStateOnDisable = true;
        CheckDeviceResolution();

        InitializeGameElements();
        // Start with Step 1
        //StartStep1();
        //StartStory();
        StartCoroutine(HandleCatEntryThenShowProblem());
    }

    public bool isIpad;
    public bool isIphone;
    private float aspectRatio;
    void CheckDeviceResolution()
    {
        aspectRatio = (float)Screen.width / Screen.height;

        // Detect iPad (Common aspect ratios: 4:3, 3:2)
        if (aspectRatio < 1.6f) // Covers iPads (4:3, 3:2)
        {
            isIpad = true;
            isIphone = false;
        }
        else // Detect iPhone (Common aspect ratios: 16:9, 19.5:9)
        {
            isIpad = false;
            isIphone = true;
        }
    }

    IEnumerator HandleCatEntryThenShowProblem()
    {
        yield return new WaitForSeconds(0.1f);

        // Define positions
        Vector3 finalPos = characterAnim.transform.position;
        //Vector3 walkingStartPos = new Vector3(finalPos.x - 7f, -4.15f, finalPos.z); // walking y
        Vector3 walkingStartPos = sittingPosForPatient.position; // walking y

        characterAnim.transform.position = WalkingStartPosForPatient.position;
        characterAnim.gameObject.SetActive(true);
        // Play walking animation
        characterAnim.Play("Walking");

        // Walk to target x over 3 seconds
        characterAnim.transform.DOMoveX(finalPos.x, 3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // Play standing animation
                characterAnim.Play("IdleStandDown");

                // Sit down (Y move from -4.15 to -3.08)
                characterAnim.transform.DOMoveY(sittingPosForPatient.position.y, 0.4f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        characterAnim.Play("Storytelling");
                    });
            });

        // Wait for walk + sit
        yield return new WaitForSeconds(4f);

        // Start the next step
        StartStory();
    }


    void InitializeGameElements()
    {
        // Hide elements from later steps
        if (Step1Gameobject) Step1Gameobject.gameObject.SetActive(false);
        if (tissueBox) tissueBox.gameObject.SetActive(false);
        if (tissue) tissue.gameObject.SetActive(false);
        if (usedTissue) usedTissue.gameObject.SetActive(false);
        if (trashCan) trashCan.gameObject.SetActive(false);

        if (Step2Gameobject) Step2Gameobject.gameObject.SetActive(false);
        if (thermometerTray) thermometerTray.gameObject.SetActive(false);
        if (thermometer) thermometer.gameObject.SetActive(false);
        if (redSpot) redSpot.gameObject.SetActive(false);
        if (temperatureDisplay) temperatureDisplay.gameObject.SetActive(false);
        if (progressBar) progressBar.gameObject.SetActive(false);

        if (Step3Gameobject) Step3Gameobject.gameObject.SetActive(false);
        if (xRayMachine) xRayMachine.gameObject.SetActive(false);
        if (exclamationMark) exclamationMark.gameObject.SetActive(false);
        if (gunDevice) gunDevice.gameObject.SetActive(false);
        if (greenArea) greenArea.gameObject.SetActive(false);
        foreach (var virus in viruses)
        {
            if (virus) virus.gameObject.SetActive(false);
        }

        if (Step4Gameobject) Step4Gameobject.gameObject.SetActive(false);
        if (pillTray) pillTray.gameObject.SetActive(false);
        if (pill) pill.gameObject.SetActive(false);
        if (waterGlassTray) waterGlassTray.gameObject.SetActive(false);
        //if (waterGlass) waterGlass.gameObject.SetActive(false);
        //if (emptyGlass) emptyGlass.gameObject.SetActive(false);
        if (catMouthOpen) catMouthOpen.gameObject.SetActive(false);
        //if (catDrinking) catDrinking.gameObject.SetActive(false);

        if (Step5Gameobject) Step5Gameobject.gameObject.SetActive(false);
        if (happyCat) happyCat.gameObject.SetActive(false);
        foreach (var sprite in recoveryAnimationSprites)
        {
            if (sprite) sprite.gameObject.SetActive(false);
        }
    }

    #region Problem Visualization
    void StartStory()
    {
        story.gameObject.SetActive(true);
        AnimateIssue();
        SoundManager.instance.PlayBlabla();
        SoundManager.instance.sfxSource.loop = true;
    }

    void AnimateIssue()
    {
        Sequence sequence = DOTween.Sequence();

        foreach (var sprite in problemSprites)
        {
            if (sprite != null)
            {
                sprite.localScale = Vector3.zero;
                sequence.Append(sprite.DOScale(0.6f, 1.5f).SetEase(Ease.OutBack));
                sequence.AppendInterval(1.5f); // Wait before showing next sprite
            }
        }

        sequence.OnComplete(() => {
            doctorObjects.SetActive(false);

            // Smoothly zoom the camera before starting the animation
            float targetSize = 4f; // Adjust this to the desired zoom level
            float zoomDuration = 2f; // Adjust duration for smooth effect

            camera.DOOrthoSize(targetSize, zoomDuration).SetEase(Ease.InOutSine);
            isStep1Complete = true;
           
            SoundManager.instance.sfxSource.loop = false;
            StartStep1();
            //StartStep3();
            //StartStep4();
        });
    }
    #endregion

    #region Step 1 - Nose Wiping
    void StartStep1()
    {
        characterAnim.Play("IdleStandDown");
        // Hide problem sprites
        foreach (var sprite in problemSprites)
        {
            if (sprite != null)
            {
                sprite.DOScale(0, 0.5f).SetEase(Ease.InBack);
            }
        }
        // Scale to 0.4 for character and 0.8 for chair
        // Define the target position for both objects
        Vector3 targetPosition = characterObj.transform.position + new Vector3(1.2f, -2f, 0);

        // Scale to 0.4 for character and 0.8 for chair
        characterObj.transform.DOScale(0.4f, 1f).SetEase(Ease.OutBack);
        characterChair.transform.DOScale(0.8f, 1f).SetEase(Ease.OutBack);

        // Move both character and chair, chair will be lower by 2 in Y-axis
        characterObj.transform.DOMove(targetPosition, 1f).SetEase(Ease.OutSine);
        characterChair.transform.DOMove(targetPosition + new Vector3(+1f, 0f, 0), 1f).SetEase(Ease.OutSine);



        Step1Gameobject.gameObject.SetActive(true);

        // Show tissue box sliding in from bottom
        SoundManager.instance.PlayTissueWiping();
        if (tissueBox)
        {
            tissueBox.gameObject.SetActive(true);
            Vector3 startPos = tissueBox.position;
            tissueBox.position = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
            tissueBox.DOMove(startPos, 1f).SetEase(Ease.OutBack);
        }

        // Show tissue
        if (tissue)
        {
            tissue.gameObject.SetActive(true);
            tissue.GetComponent<DraggableObject>().OnDragEndEvent += OnTissueDragEnd;
        }
    }

    void OnTissueDragEnd(Transform draggedObject, Vector3 position)
    {
        // Check if tissue is near cat's nose
        if (Vector3.Distance(position, catNose.position) < 1.5f)
        {
            // Animate tissue wiping nose (left-right motion)
            Sequence wipeSequence = DOTween.Sequence();
            Vector3 startPos = catNose.position;

            wipeSequence.Append(draggedObject.DOMove(new Vector3(startPos.x - 0.5f, startPos.y, startPos.z), 0.3f));
            wipeSequence.Append(draggedObject.DOMove(new Vector3(startPos.x + 0.5f, startPos.y, startPos.z), 0.3f));
            wipeSequence.Append(draggedObject.DOMove(startPos, 0.3f));

            wipeSequence.OnComplete(() => {
                // Hide clean tissue
                draggedObject.gameObject.SetActive(false);
                catNoseGerms.gameObject.SetActive(false);
                // Show used tissue
                if (usedTissue)
                {
                    usedTissue.position = startPos;
                    usedTissue.gameObject.SetActive(true);
                    usedTissue.DOMove(new Vector3(startPos.x+2, startPos.y, startPos.z), 0.3f).OnComplete(() =>
                    {
                        // Hide used tissue after a short delay
                        //DOVirtual.DelayedCall(0.5f, () =>
                        //{
                            //usedTissue.gameObject.SetActive(false);
                        //});
                        //usedTissue.gameObject.SetActive(true);
                        usedTissue.GetComponent<DraggableObject>().OnDragEndEvent += OnUsedTissueDragEnd;
                    });

                }

                // Remove tissue box
                if (tissueBox)
                {
                    Vector3 exitPos = tissueBox.position;
                    tissueBox.DOMove(new Vector3(exitPos.x, exitPos.y - 10f, exitPos.z), 1f).SetEase(Ease.InBack);
                }
                SoundManager.instance.PlayTissueInTrash();
                // Show trash can
                if (trashCan)
                {
                    trashCan.gameObject.SetActive(true);
                    Vector3 trashStartPos = trashCan.position;
                    trashCan.position = new Vector3(trashStartPos.x, trashStartPos.y - 10f, trashStartPos.z);
                    trashCan.DOMove(trashStartPos, 1f).SetEase(Ease.OutBack);
                }

                isNoseWiped = true;
            });
        }
    }

    void OnUsedTissueDragEnd(Transform draggedObject, Vector3 position)
    {
        // Check if used tissue is over trash can
        if (trashCan && Vector3.Distance(position, trashCan.position) < 4f)
        {
            // Animate tissue going into trash
            draggedObject.DOMove(trashCan.position-new Vector3(1.6f,0,0), 0.3f).OnComplete(() => {
                draggedObject.DOScale(0, 0.3f).SetEase(Ease.InBack);
                SoundManager.instance.PlayWhoosh();
                // Hide trash can
                trashCan.DOMove(new Vector3(trashCan.position.x, trashCan.position.y - 10f, trashCan.position.z), 1f)
                    .SetEase(Ease.InBack).OnComplete(() => {
                        isStep2Complete = true;
                        StartStep2();
                    });
            });
        }
    }
    #endregion

    #region Step 2 - Temperature Measurement
    void StartStep2()
    {
        Step1Gameobject.gameObject.SetActive(false);
        Step2Gameobject.gameObject.SetActive(true);
        redSpot.gameObject.SetActive(true);
        redSpot.DOMove(catNose.position + new Vector3(0,2f,0), 0.4f);

        // Show thermometer tray sliding up
        if (thermometerTray)
        {
            thermometerTray.gameObject.SetActive(true);
            Vector3 startPos = thermometerTray.position;
            thermometerTray.position = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
            thermometerTray.DOMove(startPos, 1f).SetEase(Ease.OutBack);
        }
        SoundManager.instance.PlayThermometer();
        // Show thermometer
        if (thermometer)
        {
            thermometer.gameObject.SetActive(true);
            // Show placement indicator
            if (redSpot)
            {
                redSpot.gameObject.SetActive(false);
                //thermometerPlacementIndicator.position = catFace.position;
                redSpot.position = redSpot.position-new Vector3(2.2f,-1f,0f);
                redSpot.DOScale(0.7f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            }
            thermometer.GetComponent<DraggableObject>().OnDragEndEvent += OnThermometerDragEnd;
            thermometer.GetComponent<DraggableObject>().OnDragStartEvent += OnThermometerDragStart;

        }
    }
    void OnThermometerDragStart(Transform draggedObject, Quaternion rotation)
    {
        draggedObject.DORotate(thermometer.transform.position, 0.3f);
        redSpot.gameObject.SetActive(true);
        SoundManager.instance.PlayClick();
    }
    void OnThermometerDragEnd(Transform draggedObject, Vector3 position)
    {
        redSpot.gameObject.SetActive(false);
        // Check if thermometer is near cat's head
        if (Vector3.Distance(position, catNose.position) < 5f)
        {
            SoundManager.instance.PlayCounter();
            draggedObject.GetComponent<BoxCollider2D>().enabled = false;
            
            thermBar.gameObject.SetActive(true);
            // Position thermometer on target
            draggedObject.DOMove(redSpot.transform.position - new Vector3(-2.6f, 0.9f, 0f), 0.3f).OnComplete(() => {
                if (thermometerLight)
                {
                    thermometerLight.gameObject.SetActive(true);
                }
                // Show progress bar
                if (progressBar)
                {
                    progressBar.gameObject.SetActive(true);
                    progressBar.fillAmount = 0f;
                    progressBar.DOFillAmount(1f, 3f).SetEase(Ease.Linear).OnComplete(() => {
                        // Show temperature display
                        if (temperatureDisplay)
                        {
                            temperatureDisplay.gameObject.SetActive(true);
                            temperatureDisplay.transform.localScale = Vector3.zero;
                            temperatureDisplay.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
                        }
                        SoundManager.instance.StopCounter();
                        SoundManager.instance.PlayTemperatureHigh();
                        // Hide all elements after delay
                        DOVirtual.DelayedCall(4f, () => {
                            HideAllStep2Elements();

                            // Check if we should skip Step 3
                            if (skipVirusStep)
                            {
                                isStep3Complete = true;
                                isStep4Complete = true;
                                DOVirtual.DelayedCall(1.5f, () =>
                                {
                                FinishStep3();
                                   
                                });
                            }
                            else
                            {
                                isStep3Complete = true;
                                DOVirtual.DelayedCall(1.5f, () =>
                                {
                                    StartStep3();
                                });
                            }
                        });
                    });
                }
            });
        }
    }

    void HideAllStep2Elements()
    {
        if (thermometerTray)
        {
            thermometerTray.DOMove(new Vector3(thermometerTray.position.x, thermometerTray.position.y - 10f, thermometerTray.position.z), 1f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    thermometerTray.gameObject.SetActive(false); // ✅ Hide after animation
                });
        }

        if (thermometer)
        {
            thermometer.transform.DOMoveY(thermometer.transform.position.y - 10f, 1f).SetEase(Ease.InBack)
                .OnComplete(() => thermometer.gameObject.SetActive(false));
        }

        if (redSpot)
        {
            redSpot.transform.DOMoveY(redSpot.transform.position.y - 10f, 1f).SetEase(Ease.InBack)
                .OnComplete(() => redSpot.gameObject.SetActive(false));
        }

        if (temperatureDisplay)
        {
            temperatureDisplay.transform.DOScale(0, 0.5f).SetEase(Ease.InBack)
                .OnComplete(() => temperatureDisplay.gameObject.SetActive(false));
        }

        if (progressBar) progressBar.gameObject.SetActive(false); // Optional: this can stay if you're hiding UI
        if (thermBar) thermBar.gameObject.SetActive(false);       // Optional
    }

    #endregion

    #region Step 3 - Kill the Virus
    void StartStep3()
    {
        // If we're skipping this step, go directly to Step 4
        if (skipVirusStep)
        {
            isStep4Complete = true;
            StartStep4();
            return;
        }

        Step2Gameobject.gameObject.SetActive(false);
        Step3Gameobject.gameObject.SetActive(true);

        // Show X-ray machine sliding in from right
        SoundManager.instance.PlayXrayDrag();
        if (xRayMachine)
        {
            xRayMachine.gameObject.SetActive(true);
            Vector3 startPos = xRayMachine.position;
            xRayMachine.position = new Vector3(startPos.x + 10f, startPos.y, startPos.z);
            xRayMachine.DOMove(startPos, 1f).SetEase(Ease.OutBack);
            xRayMachine.GetComponent<DraggableObject>().enabled = true;
            xRayMachine.GetComponent<DraggableObject>().changeChildColorAlpha = false;
            DOVirtual.DelayedCall(4f, () =>
            {

                xRayMachine.GetComponent<DraggableObject>().OnDragEndEvent += OnXRayMachineDragEnd;
               // xRayMachine.GetComponent<DraggableObject>().OnDragStartEvent += OnXRayMachineDragEnd;

            });
        }
    }

    void OnXRayMachineDragEnd(Transform draggedObject, Vector3 position)
    {
        // Check if X-ray machine is near cat's neck
        if (Vector3.Distance(position, catNose.position) < 2f)
        {
            draggedObject.GetComponent<BoxCollider2D>().enabled = false;
            // Disable dragging
            draggedObject.GetComponent<BoxCollider2D>().enabled = false;
            draggedObject.GetComponent<DraggableObject>().isDragable = false;
            draggedObject.GetComponent<DraggableObject>().enabled = false;
            //draggedObject.DOLocalMove(new Vector3(-4.32f, -0.3f, 0),0.5f).OnComplete(() => {
            draggedObject.DOMove(
            new Vector3(
                catNose.position.x,
                catNose.transform.position.y - 0.7f,
                catNose.position.z
            ),
            0.5f
            ).OnComplete(() => {
                print("XrayMachine Drag Ended & Positioned");
            });

            // Show exclamation mark
                        SoundManager.instance.PlayVirus();
            if (exclamationMark)
            {
                exclamationMark.gameObject.SetActive(true);
                //exclamationMark.position = new Vector3(position.x, position.y + 2f, position.z);
                exclamationMark.localScale = Vector3.zero;
                exclamationMark.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
                exclamationMark.DOScale(1.2f, 0.5f).SetLoops(4, LoopType.Yoyo).OnComplete(() => {
                    exclamationMark.DOScale(0, 0.5f).SetEase(Ease.InBack).OnComplete(() => {
                        DOVirtual.DelayedCall(2f, () =>
                        {
                            ShowViruses();
                        });
                        //ShowViruses();
                    });
                });
            }
            else
            {
                ShowViruses();
            }
        }
    }

    void ShowViruses()
    {
        // Show viruses with animation
        foreach (var virus in viruses)
        {
            if (virus)
            {
                virus.gameObject.SetActive(true);
                virus.localScale = Vector3.zero;
                virus.DOScale(0.29f, 0.31f).SetEase(Ease.OutBack);

                // Add small movement animation
                float randomX = Random.Range(-0.15f, 0.15f);
                float randomY = Random.Range(-0.15f, 0.15f);
                virus.DOMove(virus.position + new Vector3(randomX, randomY, 0), 1f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutQuad);
            }
        }
        //return;
        // Zoom in
        //Camera.main.DOOrthoSize(Camera.main.orthographicSize * 1f, 1f).OnComplete(() => {
        Camera.main.transform.DOMove(new Vector3(0, 0f, -10f), 1f);
        Camera.main.DOOrthoSize(3f, 1f).OnComplete(() => {
            // Show anti-virus device
            SoundManager.instance.PlayGetRidOfTheBacteria();
            if (gunDevice)
            {
                gunDevice.gameObject.SetActive(true);
                gunDevice.localScale = Vector3.one* 0.6f;
                Vector3 startPos = gunDevice.position;
                gunDevice.position = new Vector3(startPos.x + 10f, startPos.y, startPos.z);
                gunDevice.DOLocalMove(startPos, 1f).SetEase(Ease.OutBack).OnComplete(() => {
                    gunDevice.GetComponent<DraggableObject>().enabled = true;
                    gunDevice.GetComponent<BoxCollider2D>().enabled = true;
                    //greenArea.gameObject.SetActive(true);

                    // Monitor green area collision with viruses
                    StartCoroutine(MonitorGreenAreaOverlap());
                });
                gunDevice.GetComponent<DraggableObject>().changeColorAlpha = false;
                gunDevice.GetComponent<DraggableObject>().changeChildColorAlpha = false;
                gunDevice.GetComponent<DraggableObject>().OnDragEvent += OnAntiVirusDeviceDrag;
                gunDevice.GetComponent<DraggableObject>().OnDragStartEvent += OnGunDragStart;
                gunDevice.GetComponent<DraggableObject>().OnDragEndEvent += OnGunDragEnd;
            }
        });
    }
    void OnGunDragEnd(Transform draggedObject, Vector3 position)
    {
        SoundManager.instance.StopLaser();
        SoundManager.instance.sfxSource.loop = false; // Set loop to true for continuous sound

    }
    void OnGunDragStart(Transform draggedObject, Quaternion rotation)
    {
        SoundManager.instance.sfxSource.loop = true; // Set loop to true for continuous sound
        SoundManager.instance.PlayLaser();

    }
    public void OnAntiVirusDeviceDrag(Transform draggedObject, Vector3 position)
    {
        //greenArea.gameObject.SetActive(true);
        greenArea.GetComponent<SpriteRenderer>().enabled = true;
        // Check if anti-virus device is near cat's neck
        if (Vector3.Distance(position, catNose.position) < 2f)
        {
            {
                // Disable dragging
                //draggedObject.GetComponent<BoxCollider2D>().enabled = false;
                //draggedObject.GetComponent<DraggableObject>().isDragable = false;
                //draggedObject.GetComponent<DraggableObject>().enabled = false;
                //draggedObject.DOLocalMove(new Vector3(-4.32f, -0.3f, 0), 0.5f).OnComplete(() =>
                //{
                //    print("AntiVirusDevice Drag Ended & Positioned");
                //});
            }
        }
    }

    private IEnumerator MonitorGreenAreaOverlap()
    {
        float[] holdTimers = new float[viruses.Length];
        bool[] virusKilled = new bool[viruses.Length];
        ShakeTracker[] trackers = new ShakeTracker[viruses.Length];

        while (true)
        {
            isDragging1 = gunDevice.GetComponent<DraggableObject>().isDragging;

            if (isDragging1)
            {
                bool allVirusesKilled = true;

                for (int i = 0; i < viruses.Length; i++)
                {
                    if (!viruses[i].gameObject.activeSelf || virusKilled[i]) continue;

                    float distance = Vector3.Distance(greenAreaPoint.position, viruses[i].position);

                    if (distance < shootDistance)
                    {
                        holdTimers[i] += Time.deltaTime;

                        // Apply shake effect if not already applied
                        if (trackers[i] == null)
                        {
                            trackers[i] = viruses[i].gameObject.AddComponent<ShakeTracker>();
                            viruses[i].DOShakeScale(
                                duration: 0.4f,
                                strength: 0.18f,
                                vibrato: 25,
                                randomness: 90,
                                fadeOut: true
                            )
                            .SetEase(Ease.InOutSine)
                            .SetLoops(-1, LoopType.Restart)
                            .SetId("shakeVirus" + i);
                        }

                        if (holdTimers[i] >= holdDurationPerTarget)
                        {
                            DOTween.Kill("shakeVirus" + i);
                            if (trackers[i]) Destroy(trackers[i]);

                            virusKilled[i] = true;
                            virusesDestroyed++;

                            Transform virus = viruses[i];
                            virus.DOScale(Vector3.zero, 0.5f)
                                .SetEase(Ease.InBack)
                                .OnComplete(() =>
                                {
                                    virus.gameObject.SetActive(false);
                                });

                            // Optional: add a brief delay per kill to pace the effects
                            yield return new WaitForSeconds(0.2f);
                        }
                    }
                    else
                    {
                        // Reset shake and timer when out of range
                        DOTween.Kill("shakeVirus" + i);
                        if (trackers[i]) Destroy(trackers[i]);
                        trackers[i] = null;
                        holdTimers[i] = 0f;
                    }

                    if (!virusKilled[i])
                    {
                        allVirusesKilled = false;
                    }
                }

                if (allVirusesKilled)
                {
                    gunDevice.GetComponent<DraggableObject>().isDragable = false;
                    gunDevice.GetComponent<BoxCollider2D>().enabled = false;

                    FinishStep3();
                    yield break;
                }
            }
            else
            {
                greenArea.gameObject.SetActive(false);

                for (int i = 0; i < viruses.Length; i++)
                {
                    if (viruses[i].gameObject.activeSelf)
                    {
                        holdTimers[i] = 0f;
                        DOTween.Kill("shakeVirus" + i);
                        if (trackers[i]) Destroy(trackers[i]);
                        trackers[i] = null;
                    }
                }
            }

            yield return null;
        }
    }


    void FinishStep3()
    {
        SoundManager.instance.PlayWellDone();

        float maxAnimTime = 1f;
        int completedTweens = 0;
        int totalTweens = 2; // gunDevice + xRayMachine

        void CheckAndDisableStep3()
        {
            completedTweens++;
            if (completedTweens >= totalTweens)
            {
                if (Step3Gameobject) Step3Gameobject.gameObject.SetActive(false);
            }
        }

        // Animate and disable X-ray
        if (xRayMachine)
        {
            xRayMachine.DOMove(new Vector3(xRayMachine.position.x + 15f, xRayMachine.position.y, xRayMachine.position.z), maxAnimTime)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    xRayMachine.gameObject.SetActive(false);
                    CheckAndDisableStep3();
                });
        }
        else
        {
            totalTweens--; // Skip if null
        }

        // Animate and disable gun
        if (gunDevice)
        {
            gunDevice.DOMove(new Vector3(gunDevice.position.x + 10f, gunDevice.position.y, gunDevice.position.z), maxAnimTime)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    gunDevice.gameObject.SetActive(false);
                    CheckAndDisableStep3();
                });
        }
        else
        {
            totalTweens--; // Skip if null
        }

        // Hide green area immediately
        if (greenArea) greenArea.gameObject.SetActive(false);

        // Reset camera position & zoom
        Camera.main.transform.DOMove(new Vector3(0f, 0f, -10f), maxAnimTime);
        Camera.main.DOOrthoSize(4f, maxAnimTime).OnComplete(() =>
        {
            isStep4Complete = true;
            DOVirtual.DelayedCall(2.5f, () =>
            {
                StartStep4();
            });
        });
    }

    #endregion
    void Update()
    {
        // Handle the click and start dragging only when the device is clicked
        if (greenArea == null) return;

        // Animate greenArea scale based on drag state
        if (isDragging1)
        {
            // Smoothly scale up to 0.8 max
            greenArea.localScale = Vector3.Lerp(greenArea.localScale, Vector3.one * 0.8f, Time.deltaTime * 5f);
        }
        else
        {
            // Smoothly scale down to 0
            greenArea.localScale = Vector3.Lerp(greenArea.localScale, Vector3.zero, Time.deltaTime * 5f);
        }


        // Check if we are dragging the gun


        // Check for drag start (this is where isDragging1 should be set to true)
        if (isDragging1)
        {
            // Enable the greenArea only if we're dragging
            greenArea.gameObject.SetActive(true);
        }
        else
        {
            // Disable the greenArea when not dragging
            greenArea.gameObject.SetActive(false);
        }

        // Your existing gun drag logic
        if (isDragging1)
        {
            // Track time for each monster (only when dragging the gun)
            float[] holdTimers = new float[viruses.Length];

            // Iterate over all monsters to check if the gun is close enough
            for (int i = 0; i < viruses.Length; i++)
            {
                // Skip inactive monsters
                if (!viruses[i].gameObject.activeSelf) continue;

                // Calculate the distance between the gun and the monster
                float distance = Vector3.Distance(greenAreaPoint.position, viruses[i].position);
                Debug.Log($"Distance to {viruses[i].name}: {distance}");

                // If within shoot distance, increment the timer
                if (distance < shootDistance)
                {
                    holdTimers[i] += Time.deltaTime; // Increment timer only when in range
                    Debug.Log($"Holding {viruses[i].name} for {holdTimers[i]:F2} seconds");

                    // If the timer reaches the set duration (2 seconds), kill the monster
                    if (holdTimers[i] >= holdDurationPerTarget)
                    {
                        Debug.Log($"Monster killed: {viruses[i].name}");

                        // Apply scaling effect to make the monster shrink and disappear
                        viruses[i].DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);

                        // Disable the monster after it is "killed"
                        viruses[i].gameObject.SetActive(false);

                        // Reset the timer for this monster
                        holdTimers[i] = 0f;
                    }
                }
                else
                {
                    // Reset timer when the monster is out of range
                    holdTimers[i] = 0f;
                }
            }
        }
    }

    #region Step 4 - Give the Pill
    void StartStep4()
    {
        // If we're coming from Step 2 (skipping Step 3), make sure to deactivate Step 2
        if (Step2Gameobject.gameObject.activeInHierarchy)
        {
            Step2Gameobject.gameObject.SetActive(false);
        }

        // If we're coming from Step 3, make sure to deactivate it
        if (Step3Gameobject.gameObject.activeInHierarchy)
        {
            //Step3Gameobject.gameObject.SetActive(false);
        }

        Step4Gameobject.gameObject.SetActive(true);
        if (DeviceResForResolution.Instance.isIpad) 
        {
            Vector3 pos = Step4Gameobject.transform.position;
            pos.y -= 1f;
            Step4Gameobject.transform.position = pos;

        }
        SoundManager.instance.PlayGivePill();

        // Show pill tray sliding up
        if (pillTray)
        {
            pillTray.gameObject.SetActive(true);
            Vector3 startPos = pillTray.position;
            pillTray.position = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
            pillTray.DOMove(startPos + new Vector3(0,1,0), 1f).SetEase(Ease.OutBack);
        }

        // Show pill
        if (pill)
        {
            pill.gameObject.SetActive(true);
            pill.GetComponent<DraggableObject>().OnDragEndEvent += OnPillDragEnd;
            pill.GetComponent<DraggableObject>().OnDragStartEvent += OnPillDragStart;
        }
    }
    void OnPillDragStart(Transform draggedObject, Quaternion rotation)
    {
        SoundManager.instance.PlayClick();

    }

    void OnPillDragEnd(Transform draggedObject, Vector3 position)
    {
        // Check if pill is near cat's mouth
        if (Vector3.Distance(position, catMouth.position) < 2f)
        {
            // Open cat's mouth
            catMouth.gameObject.SetActive(false);
            catMouth.GetComponent<SpriteRenderer>().enabled = false;

            catMouthOpen.gameObject.SetActive(true);
            catMouthOpen.GetComponent<SpriteRenderer>().enabled = true;


            // Animate pill going into cat's mouth
            draggedObject.DOMove(catMouthOpen.position, 0.5f).OnComplete(() => {
                draggedObject.DOScale(0, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
                    // AFTER scale is zero, deactivate or destroy the pill
                    draggedObject.gameObject.SetActive(false);

                    // Close cat's mouth
                    DOVirtual.DelayedCall(0.5f, () => {
                        catMouthOpen.gameObject.SetActive(false);
                        catMouthOpen.GetComponent<SpriteRenderer>().enabled = false;
                        catMouth.gameObject.SetActive(true);
                        catMouth.GetComponent<SpriteRenderer>().enabled = true;

                        // Hide pill tray
                        pillTray.DOMove(new Vector3(pillTray.position.x, pillTray.position.y - 10f, pillTray.position.z), 1f)
                            .SetEase(Ease.InBack).OnComplete(() => {
                                GiveWater();
                            });
                    });
                });
            });
        }
        else
        {
            // Return pill to tray
            draggedObject.DOMove(pillTray.position + new Vector3(0, 0.3f, 0), 0.5f);
        }
    }
    bool givingWater=false;
    void GiveWater()
    {
        if (givingWater) return;
        // Show water glass tray sliding up
        if (waterGlassTray)
        {
            givingWater = true;
            waterGlassTray.gameObject.SetActive(true);
            Vector3 startPos = waterGlassTray.localPosition;
            waterGlassTray.localPosition = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
            waterGlassTray.DOLocalMove(startPos, 1f).SetEase(Ease.OutBack);
            print("Water Start Pos: " + startPos);
        }
        SoundManager.instance.PlayGiveWater();
        // Show water glass
        if (waterGlass)
        {
            waterGlass.gameObject.SetActive(true);
            waterGlass.GetComponent<DraggableObject>().OnDragEndEvent += OnWaterGlassDragEnd;
        }
    }

    void OnWaterGlassDragEnd(Transform draggedObject, Vector3 position)
    {
        // Check if water glass is near cat's mouth
        if (Vector3.Distance(position, catMouth.position) < 2f)
        {
            SoundManager.instance.PlayPouring();
            // Show cat drinking animation
            catMouth.gameObject.SetActive(false);
            catMouth.GetComponent<SpriteRenderer>().enabled = false;
            catMouthOpen.gameObject.SetActive(true);
            catMouthOpen.GetComponent<SpriteRenderer>().enabled = true;

            // Create a sequence for glass animation
            Sequence drinkingSequence = DOTween.Sequence();
            Vector3 drinkPosition = Vector3.one;
            // First move the glass to the correct position if needed
            if (!isIpad)
                drinkPosition = catMouth.position + new Vector3(1.1f, 0.25f, 0);
            else
                drinkPosition = catMouth.position + new Vector3(0.9f, 0.1f, 0);

            // Ensure the water glass is exactly at the right position before animating
            //draggedObject.position = drinkPosition; // Snap to the right position immediately
            draggedObject.rotation = Quaternion.Euler(0, 0, 0); // Set rotation if needed

            // Append the movement to the drink position
            drinkingSequence.Append(draggedObject.DOMove(drinkPosition, 0.3f));
            //drinkingSequence.AppendInterval(0.1f);
            //drinkingSequence.Append(catDrinkingWater.DOLocalMove(new Vector3(-7, 5.5f, 0), 0.02f));
            drinkingSequence.AppendInterval(0.1f);
            //drinkingSequence.Append(catDrinkingWater.DOLocalRotate(new Vector3(0, 0, 30), 0.1f));


            // Tilting animation - rotate the glass
            drinkingSequence.Append(draggedObject.DOLocalRotate(new Vector3(0, 0, 60), 1f));

            // Create an array to store references to all water frames
            GameObject[] waterFrames = new GameObject[9];
            for (int i = 1; i <= 9; i++)
            {
                Transform child = catDrinkingWater.transform.Find(i.ToString());
                if (child != null)
                {
                    waterFrames[i - 1] = child.gameObject;
                    // Just store reference, don't disable yet
                }
            }

            // Position the parent at the glass position and add frame activations
            drinkingSequence.AppendCallback(() => {
                //catDrinkingWater.transform.position = draggedObject.position;
                //catDrinkingWater.transform.rotation = draggedObject.rotation;

                // Make sure all but the first frame are inactive
                for (int i = 1; i < waterFrames.Length; i++)
                {
                    if (waterFrames[i] != null)
                        waterFrames[i].SetActive(false);
                }

                // Activate the first frame
                if (waterFrames.Length > 0 && waterFrames[0] != null)
                {
                    waterFrames[0].transform.position = draggedObject.position;
                    waterFrames[0].transform.rotation = draggedObject.rotation;
                    waterFrames[0].SetActive(true);
                }
            });

            // Show each subsequent frame in sequence
            for (int i = 1; i < waterFrames.Length; i++)
            {
                int frameIndex = i;
                int previousIndex = i - 1;

                if (waterFrames[frameIndex] != null)
                {
                    drinkingSequence.AppendInterval(0.2f);
                    drinkingSequence.AppendCallback(() => {
                        // Deactivate previous frame
                        if (waterFrames[previousIndex] != null)
                            waterFrames[previousIndex].SetActive(false);

                        // Position and activate current frame
                        waterFrames[frameIndex].transform.position = draggedObject.position;
                        waterFrames[frameIndex].transform.rotation = draggedObject.rotation;
                        waterFrames[frameIndex].SetActive(true);
                    });
                }
            }

            // Add a small interval after the last frame
            drinkingSequence.AppendInterval(0.2f);

            // Switch to empty glass immediately after last water frame
            drinkingSequence.AppendCallback(() => {
                // Deactivate the last frame
                if (waterFrames[waterFrames.Length - 1] != null)
                    waterFrames[waterFrames.Length - 1].SetActive(false);

                // Hide water glass immediately
                draggedObject.gameObject.SetActive(false);
                waterGlass.gameObject.SetActive(false);

                // Position and activate the empty glass
                emptyGlass.transform.position = draggedObject.position;
                emptyGlass.transform.rotation = draggedObject.rotation;
                emptyGlass.gameObject.SetActive(true);
            });

            // Have the empty glass rotate back to normal position
            drinkingSequence.Append(emptyGlass.DOLocalRotate(Vector3.zero, 1f));

            // Complete the animation
            drinkingSequence.OnComplete(() => {
                // Reset cat mouth
                //catDrinkingWater.DOLocalRotate(Vector3.zero, 0.5f);

                emptyGlass.DOMove(waterGlassTray.position + new Vector3(0, 1f, 0), 0.5f);
                catMouth.gameObject.SetActive(true);
                catMouth.GetComponent<SpriteRenderer>().enabled = true;
                catMouthOpen.gameObject.SetActive(false);
                catMouthOpen.GetComponent<SpriteRenderer>().enabled = false;
                // Hide all elements after delay
                DOVirtual.DelayedCall(2f, () => {
                    HideAllStep4Elements();
                    // Delay before deactivating Step4Gameobject
                    DOVirtual.DelayedCall(1f, () => {  // Adjust delay as needed
                        Step4Gameobject.gameObject.SetActive(false);
                        StartStep5();
                    });
                });
            });
        }
        else
        {
            // Return water glass to tray
            draggedObject.DOMove(waterGlassTray.position + new Vector3(0, 1f, 0), 0.5f);
        }
    }

    void HideAllStep4Elements()
    {
        // Hide all remaining elements
        if (pillTray) pillTray.DOMove(new Vector3(pillTray.position.x, pillTray.position.y - 10f, pillTray.position.z), 1f).SetEase(Ease.InBack);
        if (waterGlassTray) waterGlassTray.DOMove(new Vector3(waterGlassTray.position.x, waterGlassTray.position.y - 10f, waterGlassTray.position.z), 1f).SetEase(Ease.InBack);
    }
    #endregion

    #region Step 5 - Recovery
    [SerializeField] private ScreenFade screenFade;

    void StartStep5()
    {
        // Fade in to black first
        screenFade.FadeIn(0.5f);

        // Sequence that begins AFTER fade-in is done
        DOVirtual.DelayedCall(0.5f, () =>
        {
            Step4Gameobject.gameObject.SetActive(false);
            Step5Gameobject.gameObject.SetActive(true);
            happyMouth.gameObject.SetActive(true);
            catMouth.gameObject.SetActive(false);

            Sequence step5Sequence = DOTween.Sequence();

            // Recovery animation sequence
            foreach (var sprite in recoveryAnimationSprites)
            {
                if (sprite != null)
                {
                    sprite.gameObject.SetActive(true);
                    sprite.localScale = Vector3.zero;
                    step5Sequence.Append(sprite.DOScale(1f, 0.1f).SetEase(Ease.OutBack));
                    step5Sequence.AppendInterval(0.1f);
                    step5Sequence.Append(sprite.DOScale(0, 0.1f).SetEase(Ease.InBack));
                }
            }

            step5Sequence.OnComplete(() =>
            {
                if (happyCat)
                {
                    catMouth.gameObject.SetActive(true);
                    catMouth.GetComponent<SpriteRenderer>().enabled = false;
                    catMouthOpen.gameObject.SetActive(true);
                    catMouthOpen.GetComponent<SpriteRenderer>().enabled = false;

                    happyCat.gameObject.SetActive(true);
                    happyCat.localScale = Vector3.zero;
                    characterChair.transform.SetParent(doctorObjects.transform);

                    SoundManager.instance.PlayDone();
                    SoundManager.instance.PlayWellDone();
                    characterAnim.Play("Happy");

                    characterObj.transform.DOScale(0.22f, 0.1f).SetEase(Ease.OutBack).OnComplete(() =>
                    {
                        doctorObjects.SetActive(true);
                    });

                    characterObj.transform.DOMove(characterObj.transform.position + new Vector3(-2f, 2f, 9), 0.1f).SetEase(Ease.OutSine);
                    characterChair.transform.DOMoveY(characterChair.transform.position.y - 4f, 0.1f).SetEase(Ease.OutSine);

                    happyCat.DOScale(1f, 1f).SetEase(Ease.OutBack).OnComplete(() =>
                    {
                        // Delay, then call CompleteLevel (which includes fade out)
                        DOVirtual.DelayedCall(3.5f, CompleteLevel);
                    });
                }
                else
                {
                    CompleteLevel();
                }
            });

            // Fade out from black after visuals begin
            DOVirtual.DelayedCall(0.1f, () => screenFade.FadeOut(0.5f));
        });
    }



    #endregion
    public Animator fadeInAnim;

    public void CompleteLevel()
    {
        fadeInAnim.Play("FadeIn"); // Animator fade

        DOVirtual.DelayedCall(1.0f, () => // Match this to your fade animation's length
        {
            //PlayerPrefs.SetInt("LevelCompleted", 1);
            //PlayerPrefs.SetString("CompletedPatientType", SceneManager.GetActiveScene().name);
            //PlayerPrefs.Save();
            print("ASdjasjkfadskgfahskdgfasfasdf");

            GlobalManager.Instance.hasComeFromTheMainMenu = true;

            if (StarProgressManager.Instance != null)
            {
                StarProgressManager.Instance.AddCuredPatient();
            }
            else
            {
                Debug.LogError("StarProgressManager instance not found!");
            }
            SceneManager.LoadScene("MainMenu");
        });
    }



    public void Back()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }
}