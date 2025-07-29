using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class ChickenpoxManager : MonoBehaviour
{
    [Header("Camera")]
    public Camera camera;
    
    [Header("Doctor Objects")]
    public GameObject doctorObjects;
    public Animator characterAnim;
    public Transform patChair;
    public Transform character;
    public Transform sittingPosForPatient;
    public Transform WalkingStartPosForPatient;
    public GameObject happyOG1;
    public GameObject happyOG2;

    [Header("Problem Visualization")]
    public Transform[] problemSprites;
    public GameObject spritesGO;
    [Header("Step 1: Measure the temperature")]
    public Transform Step1Gameobject;
    public Transform thermometerTray;
    public Transform thermometer;
    public Transform thermometerPlacementIndicator;
    public Transform thermometerLight;
    public RectTransform thermBar;
    public Image temperatureDisplay;
    public Image progressBar;
    public Transform catFace;
    public Transform patientPos;
    public Transform patient;
    [Header("Step 2: Syrup")]
    public Transform Step2Gameobject;
    public Transform syrupBottle;
    public Transform syrupCap;
    public Transform spoon;
    public Transform tray;
    public Transform catMouthOpen;
    public Transform catMouthClose;
    public ParticleSystem syrupPourEffect;
    public Transform syrupInSpoon;
    public float trayEntranceDuration = 1.5f;
    public float capLiftDuration = 0.3f;
    public float capExitDuration = 0.7f;
    public float spoonEntranceDuration = 1f;
    public float bottleTiltDuration = 1f;
    public float pourDuration = 1.5f;
    public float tableExitDuration = 1.5f;
    public float mouthOpenDistance = 1.5f;
    private bool isCapRemoved = false;
    private bool isBottleTilted = false;
    private bool isSpoonFilled = false;
    private bool isBottleClickable = false;
    private Vector3 originalBottlePosition;
    private Quaternion originalBottleRotation;
    private Vector3 originalSpoonPosition;
    private DraggableObject spoonDraggable;


    [Header("Step 3: Ointment")]
    public Transform Step3Gameobject;
    public Transform ointmentParent;
    public Transform ointmentTube;
    public Transform ointmentCap;
    public Transform ointmentTray;
    public Transform ointmentContainer;
    public Transform[] redSpots;
    public GameObject[] ointmentEffects;
    private bool isOintmentTubeClicked = false;
    private bool isOintmentCapRemoved = true;
    private int currentSpotIndex = -1;
    private int treatedSpotsCount = 0;
    public Transform happyAnimation;

    private bool isStoryComplete = false;
    private bool isStep1Complete = false;
    private bool isStep2Complete = false;
    private bool isStep3Complete = false;

    public Transform characterMouth;

    public Transform docIniPos;

    public Animator fadeInAnim;

    void Start()
    {
        characterAnim.keepAnimatorStateOnDisable = true;
        // Initialize all elements
        InitializeGameElements();
        // Start with the story
        StartCoroutine(HandleCatEntryThenShowProblem());

        doctorObjects.transform.position = docIniPos.position;
        happyOG1.SetActive(false);
        happyOG2.SetActive(false);
    }
    private void Update()
    {
        if (Step2Gameobject.gameObject.activeSelf)
        {
        if (isSpoonFilled && spoon != null && spoon.gameObject.activeSelf)
        {
            // Check if spoon is near the cat's mouth
            float distanceToMouth = Vector3.Distance(spoon.position, catMouthOpen.position);

            if (distanceToMouth < mouthOpenDistance)
            {
                // Open mouth when spoon is near
                catMouthOpen.gameObject.SetActive(true);
                catMouthClose.gameObject.SetActive(false);
            }
            else
            {
                // Close mouth when spoon is far
                catMouthOpen.gameObject.SetActive(false);
                catMouthClose.gameObject.SetActive(true);
            }
        }
}
      
    }

    void InitializeGameElements()
    {
        // Hide elements from later steps
        if (Step1Gameobject) Step1Gameobject.gameObject.SetActive(false);
        if (Step2Gameobject) Step2Gameobject.gameObject.SetActive(false);
        if (Step3Gameobject) Step3Gameobject.gameObject.SetActive(false);

        // Initialize step 1 elements
        //if (targetPlacementCircle) targetPlacementCircle.gameObject.SetActive(false);
        //if (thermometerProgressBar) thermometerProgressBar.fillAmount = 0;
        //if (temperatureDisplay) temperatureDisplay.gameObject.SetActive(false);



        if (thermometerTray) thermometerTray.gameObject.SetActive(false);
        if (thermometer) thermometer.gameObject.SetActive(false);
        if (thermometerPlacementIndicator) thermometerPlacementIndicator.gameObject.SetActive(false);
        if (temperatureDisplay) temperatureDisplay.gameObject.SetActive(false);
        if (progressBar) progressBar.gameObject.SetActive(false);

        // Initialize step 2 elements
        if (spoon != null)
        {
            spoonDraggable = spoon.GetComponent<DraggableObject>();
            if (spoonDraggable != null)
            {
                spoonDraggable.OnDragEndEvent += OnSpoonDragEnd;
            }
        }

        // Only the spoon should have DraggableObject component
        if (spoonDraggable != null)
        {
            spoonDraggable.enabled = false; // Initially disabled until needed
        }

        // Initialize step 3 elements
        if (ointmentEffects != null)
        {
            foreach (var effect in ointmentEffects)
            {
                if (effect) effect.SetActive(false);
            }
        }
        ointmentCap.gameObject.GetComponent<ClickHandler>().enabled = false;
      
        if (happyAnimation) happyAnimation.gameObject.SetActive(false);
    }
    IEnumerator HandleCatEntryThenShowProblem()
    {
        yield return new WaitForSeconds(0.1f);

        // Final sitting position
        Vector3 sittingPos = characterAnim.transform.position;
        // Start walking position (X offset + walking Y)
        Vector3 walkingStartPos = WalkingStartPosForPatient.position;

        // Move character to walking start position
        characterAnim.transform.position = walkingStartPos;
        characterAnim.gameObject.SetActive(true);
        // Play walking animation
        characterAnim.Play("Walking");

        // Move horizontally to target X over 3.5 seconds
        characterAnim.transform.DOMoveX(sittingPosForPatient.position.x, 3.5f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // Change to Idle first
                characterAnim.Play("IdleStandDown");

                // Drop to sitting position on Y
                characterAnim.transform.DOMoveY(sittingPosForPatient.position.y, 0.4f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        // Wait 1 second then start storytelling
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            characterAnim.Play("Storytelling");
                        });
                    });
            });

        // Wait for walk and sit to finish
        yield return new WaitForSeconds(4.2f); // 3.5 walk + 0.4 sit + margin

        // Continue story
        StartStory();
    }

    #region Problem Visualization
    void StartStory()
    {
        AnimateIssue();
    }

    void AnimateIssue()
    {
         spritesGO.SetActive(true);
        SoundManager.instance.PlayBlabla();
        Sequence sequence = DOTween.Sequence();
        foreach (var sprite in problemSprites)
        {
            sprite.gameObject.SetActive(true); // Ensure the sprite is active   
            if (sprite != null)
            {
                sprite.localScale = Vector3.zero;
                sequence.Append(sprite.DOScale(0.3f, 1.5f).SetEase(Ease.OutBack));
                sequence.AppendInterval(1.0f); // Wait before showing next sprite
            }
        }
        sequence.OnComplete(() => {
            foreach (var sprite in problemSprites)
            {
                if (sprite != null)
                {
                    sprite.DOScale(0, 0.5f).SetEase(Ease.InBack);
                }
            }
            characterAnim.Play("IdleStandDown");
            characterMouth.gameObject.SetActive(false);
            DOVirtual.DelayedCall(1f, () =>
            {
              
                patChair.SetParent(character);
                Vector3 docSlideOffset = new Vector3(30f, 0f, 0f);
                float slideOutDuration = 1f;
               // patient.transform.DOMove(patientPos.transform.position, 1).SetEase(Ease.InBack);
                doctorObjects.transform.DOMove(doctorObjects.transform.position + docSlideOffset, slideOutDuration).SetEase(Ease.InBack).OnComplete(() =>
                {
                    float targetSize = 4f; // Adjust this to the desired zoom level
                    float zoomDuration = 2f; // Adjust duration for smooth effect

                    camera.DOOrthoSize(targetSize, zoomDuration).SetEase(Ease.InOutSine);
                    isStoryComplete = true;
                    StartStep1();
                });
            });
            // doctorObjects.SetActive(false);
            

            //// Smoothly zoom the camera before starting the animation
            //float targetSize = 4f; // Adjust this to the desired zoom level
            //float zoomDuration = 2f; // Adjust duration for smooth effect

            //camera.DOOrthoSize(targetSize, zoomDuration).SetEase(Ease.InOutSine);
            //isStoryComplete = true;
            //StartStep1();
        });
    }
    #endregion
    public Transform sittingPos;
    #region Step 1: Measure the temperature
    void StartStep1()
    {
        Sequence characterSequence = DOTween.Sequence();

        characterSequence.Append(character.transform.DOScale(new Vector3(0.8f, 0.8f, 0.4f), 1f).SetEase(Ease.OutBack));

        if (DeviceResForResolution.Instance.isIpad)
            characterSequence.Join(character.transform.DOMove(sittingPos.position , 1f).SetEase(Ease.OutQuad));

       else
            characterSequence.Join(character.transform.DOMove(new Vector3(-3.18f, -5.995f, 0f), 1f).SetEase(Ease.OutQuad));

        // Activate Step 1 elements
        Step1Gameobject.gameObject.SetActive(true);
        if (DeviceResForResolution.Instance.isIpad)
        {
            Vector3 pos = Step1Gameobject.transform.position;
            pos.y -= 1.3f;
            Step1Gameobject.transform.position = pos;
        }
        // Show thermometer tray sliding up
        if (thermometerTray)
        {
            thermometerTray.gameObject.SetActive(true);
            Vector3 startPos = thermometerTray.position;
            thermometerTray.position = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
            thermometerTray.DOMove(startPos, 1f).SetEase(Ease.OutBack);
        }
        SoundManager.instance.PlayThermometer();

        // Show thermometer and indicator
        if (thermometer)
        {
            thermometer.gameObject.SetActive(true);
            Vector3 startPos = thermometer.position;
            thermometer.position = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
            thermometer.DOMove(startPos, 1f).SetEase(Ease.OutBack);

            if (thermometerPlacementIndicator)
            {
                thermometerPlacementIndicator.gameObject.SetActive(true);
                thermometerPlacementIndicator.DOScale(0.7f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            }

            var drag = thermometer.GetComponent<DraggableObject>();
            drag.OnDragEndEvent += OnThermometerDragEnd;
            drag.OnDragStartEvent += OnThermometerDragStart;
            SoundManager.instance.PlayObjectPlaced();
        }
    }

    void OnThermometerDragStart(Transform draggedObject, Quaternion rotation)
    {
        draggedObject.DORotate(Vector3.zero, 0.3f);
        SoundManager.instance.PlayClick();
    }
    void OnThermometerDragEnd(Transform draggedObject, Vector3 position)
    {
        // Check if thermometer is near cat's head
        if (Vector3.Distance(position, catFace.position) < 5f)
        {
            SoundManager.instance.PlayCounter();
            thermometer.GetComponent<DraggableObject>().enabled = false; // Disable dragging
            thermometer.GetComponent<BoxCollider2D>().enabled = false; // Unsubscribe from event
            thermometerPlacementIndicator.gameObject.SetActive(false);
            thermBar.gameObject.SetActive(true);
            thermometer.DORotate(Vector3.zero, 0.3f);

            // Position thermometer on target
            draggedObject.DOMove(catFace.position + new Vector3(1.44000006f, - 1 , 0), 0) .OnComplete(() => {
                if (thermometerLight)
                {
                    thermometerLight.gameObject.SetActive(true);
                    //thermometerLight.transform.position = thermometerLight.transform.position + new Vector3(0f,0.72f,0f);
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
                        DOVirtual.DelayedCall(2f, () => {
                            HideAllStep1Elements();
                            DOVirtual.DelayedCall(2f, () =>
                            {
                                isStep1Complete = true;
                                StartStep2();
                            });
                                
                        });
                    });
                }
            });
        }
    }

    void HideAllStep1Elements()
    {
        // Hide all remaining elements
        if (thermometerTray) thermometerTray.DOMove(new Vector3(thermometerTray.position.x, thermometerTray.position.y - 10f, thermometerTray.position.z), 1f).SetEase(Ease.InBack);
        if (thermometer) thermometer.DOMove(new Vector3(thermometer.position.x, thermometer.position.y - 10f, thermometer.position.z), 1f).SetEase(Ease.InBack);
        if (thermometerPlacementIndicator) thermometerPlacementIndicator.DOMove(new Vector3(thermometerPlacementIndicator.position.x, thermometerPlacementIndicator.position.y - 10f, thermometerPlacementIndicator.position.z), 1f).SetEase(Ease.InBack);
        if (thermometerLight) thermometerLight.DOMove(new Vector3(thermometerLight.position.x, thermometerLight.position.y - 10f, thermometerLight.position.z), 1f).SetEase(Ease.InBack);
        //if (temperatureDisplay) temperatureDisplay.transform.DOScale(0, 0.5f).SetEase(Ease.InBack);
        //if (temperatureDisplay) temperatureDisplay.transform.DOMove(new Vector3(temperatureDisplay.transform.position.x, temperatureDisplay.transform.position.y - 10f, temperatureDisplay.transform.position.z), 1f).SetEase(Ease.InBack);
        //if (progressBar) progressBar.transform.DOMove(new Vector3(progressBar.transform.position.x, progressBar.transform.position.y - 10f, progressBar.transform.position.z), 1f).SetEase(Ease.InBack);
        if (thermBar)
            thermBar.DOAnchorPos(new Vector2(thermBar.anchoredPosition.x, thermBar.anchoredPosition.y - 1000f), 1f)
                .SetEase(Ease.InBack);
        thermometerPlacementIndicator.gameObject.SetActive(false);
    }

    #endregion

    #region Step 2: Syrup
    void StartStep2()
    {
        Step1Gameobject.gameObject.SetActive(false);
        Step2Gameobject.gameObject.SetActive(true);

        if (DeviceResForResolution.Instance.isIpad) 
        {
            Vector3 pos = Step2Gameobject.transform.position;
            pos.y -= 1.5f;
            Step2Gameobject.transform.position = pos;
        }
        syrupBottle.SetParent(tray);
        syrupCap.SetParent(syrupBottle);
        // Reset state for this step
        isCapRemoved = false;
        isBottleTilted = false;
        isSpoonFilled = false;
        isBottleClickable = false;


        // Store original positions for later
        if (syrupBottle != null)
        {
            originalBottlePosition = syrupBottle.position;
            originalBottleRotation = syrupBottle.rotation;
        }

        if (spoon != null)
            originalSpoonPosition = spoon.position;

        // Hide spoon initially
        if (spoon != null)
            spoon.gameObject.SetActive(false);

        // Hide syrup in spoon
        if (syrupInSpoon != null)
            syrupInSpoon.gameObject.SetActive(false);

        // Setup cat mouth
        if (catMouthOpen != null)
            catMouthOpen.gameObject.SetActive(false);
        if (catMouthClose != null)
            catMouthClose.gameObject.SetActive(true);

        // Start initial animation of tray coming from bottom
        AnimateTrayEntrance();
    }

     private void AnimateTrayEntrance()
    {
        // Setup initial positions
        Vector3 startPos = new Vector3(tray.position.x, tray.position.y - 5f, tray.position.z);
        tray.position = startPos;
        
        if (tray != null)
            tray.gameObject.SetActive(true);
        
        // Animate tray coming up with cat's hand using DOTween
        tray.DOMove(tray.position + Vector3.up * 5f, trayEntranceDuration)
            .SetEase(Ease.OutBack).OnComplete(() => { syrupBottle.parent = null; syrupCap.parent = null; });
        SoundManager.instance.PlayObjectPlaced();
        SoundManager.instance.PlayPourSyrp();

    }

    // Called when cap is clicked (use this with a Button or BoxCollider2D)
    public void OnCapClicked()
    {
        if (!isCapRemoved)
        {
            AnimateCapRemoval();
        }
    }

    private void AnimateCapRemoval()
    {
        // Slightly lift the cap first
        syrupCap.DOLocalMoveY(syrupCap.localPosition.y + 0.2f, capLiftDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                // Then move it to the right and out of view
                syrupCap.DOLocalMoveX(syrupCap.localPosition.x + 10f, capExitDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => {
                        isCapRemoved = true;
                        AnimateSpoonEntrance();
                    });
            });
    }
    private void AnimateSpoonEntrance()
    {
        // Setup spoon initial position (off-screen to the right)
        Vector3 startPos = originalSpoonPosition + new Vector3(10f, 0, 0);
        spoon.position = startPos;
        spoon.gameObject.SetActive(true);

        // Animate spoon sliding in from the right using DOTween
        spoon.DOMove(originalSpoonPosition, spoonEntranceDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                // Enable bottle interaction after spoon arrives
                isBottleClickable = true;
            });
    }
    // Called when bottle is clicked (use this with a Button or BoxCollider2D)
    public void OnBottleClicked()
    {
        if (isCapRemoved && !isBottleTilted && isBottleClickable)
        {
            AnimateBottleTilt();
            isBottleClickable = false; // Prevent multiple clicks
        }
    }
    private void AnimateBottleTilt()
    {
        SoundManager.instance.PlayPouring();
        // Tilt bottle 90 degrees using DOTween
        syrupBottle.DORotate(new Vector3(0, 0, -90), bottleTiltDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                isBottleTilted = true;

                // Start the pouring animation
                if (syrupPourEffect != null)
                {
                    syrupPourEffect.gameObject.SetActive(true);
                    syrupPourEffect.Play();

                    syrupInSpoon.gameObject.SetActive(true);     // Ensure it's visible
                    syrupInSpoon.localScale = Vector3.zero;      // Start from zero

                    // Start scaling IMMEDIATELY when pouring starts
                    syrupInSpoon.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutSine);
                }

                // Wait for pouring animation
                DOVirtual.DelayedCall(pourDuration, () => {
                    // Return bottle to original rotation
                    syrupBottle.DORotate(originalBottleRotation.eulerAngles, bottleTiltDuration)
                        .SetEase(Ease.InOutQuad)
                        .OnComplete(() => {
                            // Stop pouring effect
                            // Start pouring effect
                            if (syrupPourEffect != null)
                            {
                                // First ensure the particle is ready
                               // syrupPourEffect.gameObject.SetActive(true);
                               // syrupPourEffect.Play();
                            }

                            if (syrupInSpoon != null)
                            {
                            }


                            isSpoonFilled = true;
                           

                            // Animate tray sliding down
                            AnimateTrayExit();

                            // Make spoon draggable
                            if (spoonDraggable != null)
                            {
                                spoonDraggable.enabled = true;
                            }
                        });
                });

            });
    }
    private void AnimateTrayExit()
    {
        // Create a sequence for moving multiple objects down
        Sequence tableExitSequence = DOTween.Sequence();

        // Move table and all objects on it down
        Transform[] objectsToMove = { tray, syrupBottle, tray };

        foreach (Transform obj in objectsToMove)
        {
            if (obj != null)
            {
                tableExitSequence.Join(
                    obj.DOLocalMoveY(obj.localPosition.y - 10f, tableExitDuration)
                       .SetEase(Ease.InBack)
                );
            }
        }

        tableExitSequence.Play();
    }
    // Called by DraggableObject when spoon is released
    private void OnSpoonDragEnd(Transform draggedObject, Vector3 position)
    {
        if (isSpoonFilled && draggedObject == spoon)
        {
            // Check if spoon is released near mouth
            float distanceToMouth = Vector3.Distance(position, catMouthOpen.position);

            if (distanceToMouth < mouthOpenDistance)
            {
                // Complete feeding animation
                CompleteSpoonFeeding();
            }
        }
    }

    private void CompleteSpoonFeeding()
    {
        // Disable spoon dragging
        if (spoonDraggable != null)
        {
            spoonDraggable.enabled = false;
        }

        // Close mouth after feeding
        if (catMouthOpen != null)
            catMouthOpen.gameObject.SetActive(false);
        if (catMouthClose != null)
            catMouthClose.gameObject.SetActive(true);

        // Empty the spoon (hide syrup in spoon)
        if (syrupInSpoon != null)
            syrupInSpoon.gameObject.SetActive(false);

        // Wait a moment then slide spoon down
        DOVirtual.DelayedCall(0.5f, () => {
            // Animate spoon sliding down out of view
            spoon.DOLocalMoveY(spoon.localPosition.y - 5f, 1f)
                .SetEase(Ease.InQuad)
                .OnComplete(() => {
                    // Mark this step as complete
                    isStep2Complete = true;

                    // Proceed to next step if needed
                    if (isStep2Complete)
                        StartStep3();
                });
        });
    }

    #endregion

    #region Step 3: Ointment
    void StartStep3()
    {
        Step2Gameobject.gameObject.SetActive(false);
        Step3Gameobject.gameObject.SetActive(true);
        spotTreated = new bool[redSpots.Length];
        // Reset variables
        isOintmentTubeClicked = false;
        isOintmentCapRemoved = false;
        currentSpotIndex = -1;
        treatedSpotsCount = 0;
        // Animate ointment appearing from bottom
        ointmentParent.position -= new Vector3(0, 10f, 0);
        ointmentTray.position -= new Vector3(0, 10f, 0);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(ointmentTray.DOMoveY(ointmentTray.position.y + 10f, 1.5f).SetEase(Ease.OutBack));
        sequence.Join(ointmentParent.DOMoveY(ointmentParent.position.y + 10f, 1.5f).SetEase(Ease.OutBack));
        SoundManager.instance.PlayUseOintment();
    }



    public void OnOintmentTubeClicked()
    {
        if (isOintmentTubeClicked) return; // Prevent multiple clicks

        isOintmentTubeClicked = true; // Mark tube as clicked

        // Move ointment tray down and make cap clickable after
        ointmentTray.DOMoveY(ointmentTray.position.y - 10f, 1f)
            .SetEase(Ease.InBack);
           
    }

    public void OnOintmentCapClicked()
    {
        if (!isOintmentTubeClicked || isOintmentCapRemoved) return; // Ensure correct order

        AnimateOintmentCapRemoval();
    }
    private void AnimateOintmentCapRemoval()
    {
        Sequence capSequence = DOTween.Sequence();

        capSequence.Append(ointmentCap.DOLocalMoveX(ointmentCap.localPosition.x - 1f, 0.5f)
            .SetEase(Ease.OutQuad));

        capSequence.Append(ointmentCap.DOLocalMoveY(ointmentCap.localPosition.y - 10f, 0.7f)
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                isOintmentCapRemoved = true;

                // ✅ Add DraggableObject only after cap is removed
                DraggableObject draggable = ointmentTube.gameObject.AddComponent<DraggableObject>();
                //draggable.OnDragStartEvent += CheckOintmentApplication;
                draggable.OnDragEndEvent += CheckOintmentApplication;
                ointmentContainer.gameObject.SetActive(true);   

            }));
    }
    private bool[] spotTreated;
    private void CheckOintmentApplication(Transform draggedObject, Vector3 position)
    {
        if (!isOintmentCapRemoved) return; // ✅ Ensure cap is removed

        Transform ointmentTip = ointmentTube.Find("OintmentTip"); // 🔹 Find the tip transform
        if (ointmentTip == null) return; // ✅ Safety check

        float minDistance = Mathf.Infinity;
        Transform closestSpot = null;
        int closestIndex = -1;

        // 🔹 Find the nearest red spot based on the ointment tip position
        for (int i = 0; i < redSpots.Length; i++)
        {
            float distance = Vector3.Distance(ointmentTip.position, redSpots[i].position);
            if (distance < minDistance && distance < 1.5f) // ✅ Adjust threshold if needed
            {
                minDistance = distance;
                closestSpot = redSpots[i];
                closestIndex = i;
            SoundManager.instance.PlaySplash();
                
            }
        }

        if (closestSpot == null || spotTreated[closestIndex]) return;

        // Fade out previous effect (if any)
        if (currentSpotIndex >= 0 && ointmentEffects[currentSpotIndex].activeSelf)
        {
            FadeOutOintmentEffect(ointmentEffects[currentSpotIndex]);
        }

        ointmentEffects[closestIndex].transform.position = closestSpot.position;
        ointmentEffects[closestIndex].SetActive(true);
        currentSpotIndex = closestIndex;

        spotTreated[closestIndex] = true;
        treatedSpotsCount++;

        // ✅ Check if all spots are treated
        if (treatedSpotsCount >= redSpots.Length)
        {
            FadeOutOintmentEffect(ointmentEffects[currentSpotIndex], true); // 👈 Pass `true` for final fade
        }
    }


    private void FadeOutOintmentEffect(GameObject ointmentEffect, bool isLast = false)
    {
        if (ointmentEffect == null) return;

        SpriteRenderer ointmentRenderer = ointmentEffect.GetComponent<SpriteRenderer>();
        SpriteRenderer redSpotRenderer = redSpots[currentSpotIndex].GetComponent<SpriteRenderer>();

        int fadesToWait = 0;
        if (ointmentRenderer != null) fadesToWait++;
        if (redSpotRenderer != null) fadesToWait++;

        void CheckAllFaded()
        {
            fadesToWait--;
            if (fadesToWait <= 0 && isLast)
            {
                OnAllOintmentsApplied(); // ✅ Only call after full fade
            }
        }

        if (ointmentRenderer != null)
        {
            ointmentRenderer.DOFade(0, 2f)
                .OnComplete(() => {
                    ointmentEffect.SetActive(false);
                    CheckAllFaded();
                });
        }

        if (redSpotRenderer != null)
        {
            redSpotRenderer.DOFade(0, 2f)
                .OnComplete(() => {
                    redSpots[currentSpotIndex].gameObject.SetActive(false);
                    CheckAllFaded();
                });
        }
    }
    public Transform happyFace;
    public Transform sadFace;
    [SerializeField] private ScreenFade screenFade;

    private void OnAllOintmentsApplied()
    {
        SoundManager.instance.PlayDone();
        SoundManager.instance.PlayWellDone();

        Debug.Log("✅ All ointments applied! Proceeding to next step...");
        isStep3Complete = true;

        // Fade to black before starting animation
        screenFade.FadeIn(0.5f);

        DOVirtual.DelayedCall(0.5f, () =>
        {
            happyFace.gameObject.SetActive(true);

            Sequence exitSequence = DOTween.Sequence();

            // Animate tray and parent going down
            exitSequence.Append(ointmentTray.DOMoveY(ointmentTray.position.y - 10f, 1f).SetEase(Ease.InBack));
            exitSequence.Join(ointmentParent.DOMoveY(ointmentParent.position.y - 10f, 1f).SetEase(Ease.InBack));

            // Then character animation
            exitSequence.AppendInterval(0.2f);
            exitSequence.AppendCallback(() =>
            {
                Sequence characterSequence = DOTween.Sequence();

                characterSequence
                    .Append(character.transform.DOScale(new Vector3(0.45f, 0.45f, 0.4f), 0.5f).SetEase(Ease.OutBack))
                    .Join(character.transform.DOMove(new Vector3(-4.88f, -2.85f, 0), 0.5f).SetEase(Ease.OutQuad))
                    .AppendCallback(() =>
                    {
                        float targetSize = 5f;
                        float zoomDuration = 2f;

                        camera.DOOrthoSize(targetSize, zoomDuration).SetEase(Ease.InOutSine);

                        doctorObjects.transform.DOMove(
                            new Vector3(0f, doctorObjects.transform.position.y, doctorObjects.transform.position.z),
                            1f
                        ).OnComplete(() =>
                        {
                            patChair.gameObject.SetActive(false);

                            // Adjust characterAnim position
                            if (!DeviceResForResolution.Instance.isIpad)
                                characterAnim.transform.position -= new Vector3(0, 1f, 0);
                            happyOG1.SetActive(true);
                            happyOG2.SetActive(true);
                            characterAnim.Play("Happy");

                            // Fade out from black to show final scene
                            screenFade.FadeOut(0.5f);

                            // Wait, then complete level
                            DOVirtual.DelayedCall(3f, CompleteLevel);
                        });
                    });
            });
        });
    }

    #endregion

    public void CompleteLevel()
    {
        screenFade.FadeIn(0.5f); // Fade to black again

        DOVirtual.DelayedCall(0.6f, () =>
        {
            if (StarProgressManager.Instance != null)
            {
                StarProgressManager.Instance.AddCuredPatient();
                GlobalManager.Instance.hasComeFromTheMainMenu = true;
            }
            else
            {
                Debug.LogError("StarProgressManager instance not found!");
            }
            print("Ollaa ");
            SceneManager.LoadScene("MainMenu");
        });
    }

    public void Back()
    {
        GlobalManager.Instance.hasComeFromTheMainMenu = true;

        SceneManager.LoadSceneAsync("MainMenu");
    }
}
