using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class VaccinationManager : MonoBehaviour
{
    [Header("Camera")]
    public Camera mainCamera;
    public float zoomValue;
    

    [Header("Problem Visualization")]
    public Transform[] problemSprites;
    public Transform patientMother;
    public Transform catHappyFace;
    public Transform catSadFace;
    public GameObject sprites;
    public Transform patChair;
    public Transform charPosForIpad;

    public Transform sittingPosForPatient;
    public Transform WalkingStartPosForPatient;
    public Transform sittingPosForPatient2;
    public Transform WalkingStartPosForPatient2;

    [Header("Step 1 - Stethoscope Check")]

    [SerializeField] private Transform step2Container;
    [SerializeField] private Transform stethoscope;
    [SerializeField] private Transform stethoscopeEndpoint;
    [SerializeField] private Transform[] targetMarkers; // Targets to be touched in sequence
    [SerializeField] private GameObject progressBarForStethoScope;
    [SerializeField] private GameObject progressBarForStethoScopeGO;

    [SerializeField] private float placementDistance = 1.0f;
    [SerializeField] private float progressFillDuration = 0.5f;
    [SerializeField] private float originalBarHeight = 1f;
    [SerializeField] private LineRenderer stethoscopeLineRenderer;
    [SerializeField] private Transform stethoscopeLineStart;
    [SerializeField] private Transform stethoscopeLineEnd;
    [SerializeField] private float stethoscopeCurveDrop = 1.5f;
    [SerializeField] private Material stethoscopeLineMaterial;
    [SerializeField] private Color stethoscopeLineColor = Color.magenta;
    [SerializeField] private float stethoscopeLineWidth = 0.12f;
    private const int stethoscopeCurveSegments = 50;
    [SerializeField] private float maxProgressScaleY = 0.645149946f; // Max Y scale
    [SerializeField] private Vector3 maxProgressPosition = new Vector3(-9.37014961f, -1.38090003f, 0f); // Max Position for the progress bar
    private int currentTarget = 0;
    private float progressFillAmount = 0f;
    private bool isDragging = false;
    [SerializeField] private GameObject targetsInCat;
    public GameObject DoctorCabin, Mouth, Paitent, PatChair, DocObjects, paitentPos, patChairPos;
    [SerializeField] Transform stethoscopeStartPos; // Off-screen right
    [SerializeField] Transform stethoscopeTargetPos; // Where it should appear
    public Transform stehtoscopePos;

    [Header("Step 2 - Temperature")]
    public Transform step1Container;
    public Transform thermometerTray;
    public Transform thermometer;
    public Transform thermometerPlacementIndicator;
    public Transform thermometerLight;
    public RectTransform thermBar;
    public Image temperatureDisplay;
    public Image progressBar;
    public Transform catNose;
    public Transform greenLight;

    [Header("Step 4 - Injection Elements")]
    public Transform step4Container;
    public Transform injectionTray;
    public Transform plasterTray;
    public Transform injection;
    public Transform plaster;
    public Transform injectionPlunger; // Child object of injection that moves down
    public Transform injectionLiquid; // Visual representation of liquid in the injection
    public Transform injectionTargetMarker; // Red area on patient's arm
    public Transform injectionPos; // Final position for the injection
    public Transform greenCheckmark;
    public Transform catArm;
    private bool injectionPlaced = false;
    private bool forceFreezeInjection = false;
    private Vector3 injectionLockedPosition;
    private float injectionTargetDistance = 1f; // Adjust this based on your needs
    private bool useCurvedLine = false; // Set based on your preference
    public Transform injectionMark;
    public Transform injectionPosOffset;
    // Add these to your initialization method

    [Header("Step 5 - Signature Elements")]
    public Transform step5Container;
    public Transform cardBoard;
    public Transform pen;
    public Transform signatureSpace;
    public Transform signature;
    public Animator signatureAnimator;
    public Transform penTriggerer;
    public Transform motherPosForSign;
    public Animator motherAmimator;
    public Vector3 iniMotherPos;
    public Vector3 doctorIniPos;
    public Animator characterAnim;
    public Animator secondCharacterAnim;

    public Animator fadeInAnim;
    void InitializeGameElements()
    {
        // ... existing initialization code

        // Reset injection system
        if (injection)
        {
            injection.gameObject.SetActive(false);

            // Set up the injection structure
            if (injectionPlunger)
            {
                // Make sure plunger is at its starting position
                injectionPlunger.localPosition = Vector3.zero; // Adjust this based on your model
            }

            if (injectionLiquid)
            {
                // Make sure liquid is at full scale
                injectionLiquid.localScale = Vector3.one;
            }
        }

        if (injectionTargetMarker)
        {
            injectionTargetMarker.gameObject.SetActive(false);
        }

        if (greenCheckmark)
        {
            greenCheckmark.gameObject.SetActive(false);
        }
    }
    void Start()
    {
        iniMotherPos = patientMother.position;
        doctorIniPos = DocObjects.transform.position;
        characterAnim.keepAnimatorStateOnDisable = true;
        secondCharacterAnim.keepAnimatorStateOnDisable = true;
        CheckDeviceResolution();

        //InitializeScene();
        // ShowProblemAnimation();
        ResetProgressBar();
        ActivateTarget(currentTarget);
        StartCoroutine(HandleCatEntryThenShowProblem());

        //StartStep5();
        SetupStethoscopeCurvedLineRenderer();
        if (isIpad)
        {
            zoomValue = 3f;
            stehtoscopePos.position = new Vector3(stehtoscopePos.position.x + 3, stehtoscopePos.position.y, stehtoscopePos.position.z);
        }
        else if (isIphone)
        {
            zoomValue = 4f;
        }
        else
        {
            zoomValue = 3.5f; // Default value for other devices, if any
        }

        
        
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

        // Walking and sitting positions from assigned Transforms
        Vector3 walkStart1 = WalkingStartPosForPatient.position;
        Vector3 sitPos1 = sittingPosForPatient.position;

        Vector3 walkStart2 = WalkingStartPosForPatient2.position;
        Vector3 sitPos2 = sittingPosForPatient2.position;

        // Character 1 setup
        characterAnim.gameObject.SetActive(true);
        characterAnim.transform.position = walkStart1;
        characterAnim.Play("DaughterWalking");

        // Character 2 setup
        secondCharacterAnim.gameObject.SetActive(true);
        secondCharacterAnim.transform.position = walkStart2;
        secondCharacterAnim.Play("MotherWalking");
        // Character 1: Walk → Sit
        characterAnim.transform.DOMoveX(sitPos1.x, 3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                characterAnim.transform.DOMoveY(sitPos1.y, 0.3f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        characterAnim.Play("DaughterSittingIdle");

                        DOVirtual.DelayedCall(1f, () =>
                        {
                            // characterAnim.Play("Storytelling");
                        });
                    });
            });

        // Character 2: Walk → Sit
        secondCharacterAnim.transform.DOMoveX(sitPos2.x, 3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                secondCharacterAnim.transform.DOMoveY(sitPos2.y, 0.3f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        secondCharacterAnim.Play("MotherSittingIdle");

                        DOVirtual.DelayedCall(1f, () =>
                        {
                            secondCharacterAnim.Play("MotherStorytelling");
                        });
                    });
            });

        // Wait until both walk and sit animations are done
        yield return new WaitForSeconds(3.8f);

        ShowProblemAnimation();
    }



    void InitializeScene()
    {
        //    step1Container.gameObject.SetActive(false);
        //    step2Container.gameObject.SetActive(false);
        //    step3Container.gameObject.SetActive(false);
        //    step4Container.gameObject.SetActive(false);

        //    thermometerPlacementIndicator.gameObject.SetActive(false);
        //    thermometerLight.gameObject.SetActive(false);
        //    temperatureDisplay.gameObject.SetActive(false);
        //    happyPatient.gameObject.SetActive(false);
        StartStep2();
    }

    void ShowProblemAnimation()
    {
        SoundManager.instance.PlayBlabla();
        sprites.gameObject.SetActive(true);
        Sequence problemSequence = DOTween.Sequence();

        // Show each sprite one-by-one with delay between them
        foreach (var sprite in problemSprites)
        {
            if (sprite != null)
            {
                sprite.localScale = Vector3.zero;

                // Scale up
                problemSequence.Append(sprite.DOScale(0.28f, 0.5f).SetEase(Ease.OutBack));

                // Hold for 1.5 seconds
                problemSequence.AppendInterval(1.5f);

                // Scale down before showing the next
                problemSequence.Append(sprite.DOScale(0f, 0.5f).SetEase(Ease.InBack));
            }
        }

        // Slide DocObjects and patientMother out in opposite directions
        float slideOutDuration = 1f;
        Vector3 docSlideOffset = new Vector3(30f, 0f, 0f);   // Slide right
        Vector3 momSlideOffset = new Vector3(-30f, 0f, 0f);  // Slide left

        problemSequence.Append(DocObjects.transform.DOMove(DocObjects.transform.position + docSlideOffset, slideOutDuration).SetEase(Ease.InBack));
        problemSequence.Join(patientMother.transform.DOMove(patientMother.transform.position + momSlideOffset, slideOutDuration).SetEase(Ease.InBack));

        // After slides, move PatChair and Patient in, then zoom in camera
        problemSequence.AppendCallback(() =>
        {
            float slideInDuration = 1f;

            Sequence slideSequence = DOTween.Sequence();
            slideSequence.Append(PatChair.transform.DOMove(patChairPos.transform.position, slideInDuration).SetEase(Ease.OutCubic));
            slideSequence.Join(Paitent.transform.DOMove(paitentPos.transform.position, slideInDuration).SetEase(Ease.OutCubic));

            // Camera zoom and position
            Vector3 targetFocusPosition = new Vector3(2.01f, -0.5f, -10f);
            float targetZoom = 3f;
            float zoomDuration = 2f;

            Sequence camSequence = DOTween.Sequence();
            camSequence.Append(Camera.main.transform.DOMove(targetFocusPosition, zoomDuration).SetEase(Ease.InOutSine));
            camSequence.Join(Camera.main.DOOrthoSize(zoomValue, zoomDuration).SetEase(Ease.InOutSine));
            camSequence.OnComplete(() =>
            {
                Debug.Log("Breathing problem camera zoom complete!");
                StartStep2();
            });
        });
    }




    #region Step 2 - Temperature Measurement
    void StartStep1()
    {
        // Enable step UI and position elements
        step1Container.gameObject.SetActive(true);
        if (DeviceResForResolution.Instance.isIpad)
        {
            Vector3 pos = step1Container.transform.position;
            pos.x -= 1f;
            step1Container.transform.position = pos;
            Vector3 indPos = thermometerPlacementIndicator.transform.position;
            indPos.x += 1f;
            thermometerPlacementIndicator.transform.position = indPos;
        }
        // Optional: position chair/patient
        // PatChair.transform.position = patChairPos.transform.position;
        // Paitent.transform.position = paitentPos.transform.position;

        // Show thermometer tray sliding up
        if (thermometerTray)
        {
            thermometerTray.gameObject.SetActive(true);
            Vector3 startPos = thermometerTray.position;
            thermometerTray.position = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
            thermometerTray.DOMove(startPos, 1f).SetEase(Ease.OutBack);
        }

        SoundManager.instance.PlayThermometer();

        // Show thermometer and placement indicator
        if (thermometer)
        {
            // thermometer.gameObject.SetActive(true);
            if (thermometerPlacementIndicator)
            {
                // thermometerPlacementIndicator.gameObject.SetActive(true);
                // thermometerPlacementIndicator.DOMove(new Vector3(-4.98999977f, 5.75f, 0f), 1f).SetEase(Ease.OutBack);
                thermometerPlacementIndicator.DOScale(0.7f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            }

            DraggableObject drag = thermometer.GetComponent<DraggableObject>();
            drag.OnDragEndEvent += OnThermometerDragEnd;
            drag.OnDragStartEvent += OnThermometerDragStart;
        }
    }

    void OnThermometerDragStart(Transform draggedObject, Quaternion rotation)
    {
        draggedObject.DORotate(thermometer.transform.position, 0.3f);
        SoundManager.instance.PlayClick();
    }
    void OnThermometerDragEnd(Transform draggedObject, Vector3 position)
    {
        // Check if thermometer is near cat's head
        if (Vector3.Distance(position, catNose.position) < 3f)
        {
            // thermBar.gameObject.SetActive(true);
            thermometer.GetComponent<DraggableObject>().enabled = false;
            thermometer.GetComponent<BoxCollider2D>().enabled = false;// Disable further dragging
            thermometerPlacementIndicator.gameObject.SetActive(false);
            float Xvalue;

            if (DeviceResForResolution.Instance.isIpad)
            {
                Xvalue = 1.5f;
            }
            else
            {
                Xvalue = -0.5f; // ← Use a smaller value instead of 100
            }




            // Move thermometer to correct position
            draggedObject.DOMove(catNose.position - new Vector3(1.5f, 0, 0), 0.3f)
  .OnComplete(() =>
            {
                if (thermometerLight)
                {
                    thermometerLight.gameObject.SetActive(true);
                    
                }

                // Wait 2 seconds before showing green light
                DOVirtual.DelayedCall(2f, () =>
                {
                    if (greenLight) greenLight.gameObject.SetActive(true);
                    SoundManager.instance.PlayTransition();
                    SoundManager.instance.PlayTempIsGood();
                    // Wait 1 more second, then hide all
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        HideAllStep1Elements();
                    });
                });
            });
        }
    }


    #endregion

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
                .SetEase(Ease.InBack).OnComplete(() =>
                {
                    StartStep5();
                    step1Container.gameObject.SetActive(false);
                });

    }
    #region Step 2 - Stethoscope Functions

    public void StartStep2()
    {
        print("Step2 Started");

        PatChair.transform.SetParent(patient.transform);

        Vector3 targetPosition = characterAnim.transform.position + new Vector3(0.5f, -5f, 0);

        // Animate scale and position together using a Sequence
        Sequence moveAndScale = DOTween.Sequence();

        if (DeviceResForResolution.Instance.isIpad)
        {

            // Use charPosForIpad only for position movement
            moveAndScale.Join(patient.transform.DOMove(charPosForIpad.position, 1f).SetEase(Ease.OutSine));
        }
        else
        {
            // Default movement for patient
            moveAndScale.Join(patient.transform.DOMove(targetPosition, 1f).SetEase(Ease.OutSine));
        }

        // Scale patient in both cases
        moveAndScale.Join(patient.transform.DOScale(1.5f, 1f).SetEase(Ease.OutBack));

        // Set initial positions and states
        SoundManager.instance.PlayListenLungWithStethoScope();
        targetsInCat.SetActive(true);
        step2Container.gameObject.SetActive(true);
        stethoscope.position += Vector3.right * 15f;
         
            PatChair.transform.position = patChairPos.transform.position;
        Paitent.transform.position = paitentPos.transform.position;

        DoctorCabin.SetActive(true);
        DocObjects.SetActive(false);

        // Setup the curved line before showing the stethoscope
        SetupStethoscopeCurvedLineRenderer();

        // Set the progress bar to initial position (invisible at the start)
        progressBarForStethoScopeGO.transform.position = new Vector3(progressBarForStethoScopeGO.transform.position.x - 10f, progressBarForStethoScopeGO.transform.position.y, progressBarForStethoScopeGO.transform.position.z);
       if(isIpad)
        stehtoscopePos.position = new Vector3(stehtoscopePos.position.x - 7f, stehtoscopePos.position.y, stehtoscopePos.position.z);
       else if (isIphone)
            stehtoscopePos.position = new Vector3(stehtoscopePos.position.x - 7f, stehtoscopePos.position.y, stehtoscopePos.position.z);

        Debug.Log("Stethoscope position: " + stethoscope.position);
        // Animate the stethoscope and progress bar
        stethoscope.DOMoveX(stehtoscopePos.position.x , 1f)
            .SetEase(Ease.OutBack)
            .OnStart(() =>
            {
                SoundManager.instance.PlayObjectPlaced();
                // Start the progress bar animation when the stethoscope starts moving
                progressBarForStethoScopeGO.transform.DOMoveX(progressBarForStethoScopeGO.transform.position.x + 10f, 1f).SetEase(Ease.OutBack);
            })
            .OnComplete(() =>
            {
                isDragging = true;
                StartCoroutine(TrackStethoscopeDrag());
            });
    }






    private IEnumerator TrackStethoscopeDrag()
    {
        float holdDurationPerTarget = 1.5f;
        float currentHoldTime = 0f;
        float screenPlacementThreshold = 50f; // Pixels on screen

        // Starting point for Y scaling (start small)
        float initialScaleY = 0f;
        Vector3 initialPos = progressBarForStethoScope.transform.localPosition;

        while (currentTarget < targetMarkers.Length)
        {
            Transform target = targetMarkers[currentTarget];
            bool isTargetComplete = false;

            while (!isTargetComplete)
            {
                Vector2 screenStetho = RectTransformUtility.WorldToScreenPoint(Camera.main, stethoscopeEndpoint.position);
                Vector2 screenTarget = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);
                float distance = Vector2.Distance(screenStetho, screenTarget);

                if (isDragging && distance < screenPlacementThreshold)
                {
                    currentHoldTime += Time.deltaTime;

                    if (currentHoldTime >= holdDurationPerTarget)
                    {
                        // Calculate progress based on the current target
                        float t = (currentTarget + 1) / (float)targetMarkers.Length;

                        // Only change Y scale
                        float targetScaleY = Mathf.Lerp(initialScaleY, maxProgressScaleY, t);

                        // Apply the Y scaling to the progress bar (X and Z remain the same)
                        progressBarForStethoScope.transform.DOScaleY(targetScaleY, 0.5f).SetEase(Ease.OutQuad);
                        progressBarForStethoScope.transform.DOLocalMoveY(maxProgressPosition.y, 0.5f).SetEase(Ease.OutQuad);

                        // Snap the stethoscope to the target and deactivate it
                        stethoscopeEndpoint.position = target.position;
                        target.gameObject.SetActive(false);
                        SoundManager.instance.PlayClick();
                        currentTarget++;
                        currentHoldTime = 0f;
                        isTargetComplete = true;

                        if (currentTarget < targetMarkers.Length)
                            ActivateTarget(currentTarget);

                        yield return new WaitForSeconds(0.5f);
                    }
                }
                else
                {
                    currentHoldTime = 0f;
                }

                yield return null;
            }
        }
        yield return new WaitForSeconds(1f);
        stethoscopeEndpoint.gameObject.GetComponent<DraggableObject>().isDragable = false;
        stethoscopeEndpoint.gameObject.GetComponent<CircleCollider2D>().enabled = false;

        FinishStep2(); // <-- Make sure this calls Step3, not Step2
    }



    private void ActivateTarget(int index)
    {
        for (int i = 0; i < targetMarkers.Length; i++)
        {
            Transform marker = targetMarkers[i];
            bool isActive = i == index;

            marker.gameObject.SetActive(isActive);

            if (isActive)
            {
                // Reset scale and kill any previous tweens on this marker
                marker.DOKill();
                marker.localScale = Vector3.one;

                // Apply the looping Yoyo scale animation
                marker.DOScale(1.5f, 0.5f)
                      .SetLoops(-1, LoopType.Yoyo)
                      .SetEase(Ease.InOutSine);
            }
            else
            {
                // Stop any ongoing animations for inactive markers
                marker.DOKill();
                marker.localScale = Vector3.one;
            }
        }
    }


    private void ResetProgressBar()
    {
        progressFillAmount = 0f;
        progressBarForStethoScope.transform.localScale = new Vector3(1f, 0f, 1f);
    }

    private void FillProgressBar()
    {
        progressFillAmount += 0.22f;
        progressFillAmount = Mathf.Clamp01(progressFillAmount);

        float targetHeight = progressFillAmount * originalBarHeight;

        progressBar.transform.DOScaleY(targetHeight, progressFillDuration)
            .SetEase(Ease.Linear);
    }

    private void FinishStep2()


    {
        // Create a sequence for hiding both stethoscope and progress bar smoothly
        Sequence hideSequence = DOTween.Sequence();

        // Animate the stethoscope out of the screen (move it away)
        hideSequence.Append(stethoscope.DOMoveX(stethoscope.position.x + 10f, 1f)
            .SetEase(Ease.InBack)); // You can adjust the movement direction as needed

        // Animate the progress bar out of the screen (move it away)
        hideSequence.Join(progressBarForStethoScopeGO.transform.DOMoveX(progressBarForStethoScopeGO.transform.position.x + 10f, 1f)
            .SetEase(Ease.InBack));

        // You can also make them fade out (if necessary)
        hideSequence.Join(stethoscope.GetComponent<SpriteRenderer>().DOFade(0f, 1f)); // Fades out stethoscope
        hideSequence.Join(progressBarForStethoScopeGO.GetComponent<SpriteRenderer>().DOFade(0f, 1f)); // Fades out progress bar

        // Optionally, deactivate them after the sequence is complete
        hideSequence.OnComplete(() =>
        {
            stethoscope.gameObject.SetActive(false);
            progressBarForStethoScopeGO.SetActive(false); // Deactivate progress bar
            Debug.Log("Stethoscope and Progress Bar hidden!");
            StartStep1();
        });
    }


    private void SetupStethoscopeCurvedLineRenderer()
    {
        if (stethoscopeLineRenderer == null && stethoscope != null)
        {
            stethoscopeLineRenderer = stethoscope.gameObject.AddComponent<LineRenderer>();
        }

        if (stethoscopeLineRenderer != null)
        {
            stethoscopeLineRenderer.useWorldSpace = true;
            stethoscopeLineRenderer.numCapVertices = 10;
            stethoscopeLineRenderer.numCornerVertices = 10;

            stethoscopeLineRenderer.startWidth = stethoscopeLineWidth;
            stethoscopeLineRenderer.endWidth = stethoscopeLineWidth;
            stethoscopeLineRenderer.positionCount = stethoscopeCurveSegments;

            if (stethoscopeLineMaterial == null)
            {
                stethoscopeLineMaterial = new Material(Shader.Find("Sprites/Default"));
            }

            stethoscopeLineRenderer.material = stethoscopeLineMaterial;
            stethoscopeLineRenderer.startColor = stethoscopeLineColor;
            stethoscopeLineRenderer.endColor = stethoscopeLineColor;

            UpdateStethoscopeCurvedLinePositions();
        }
    }

    private void UpdateStethoscopeCurvedLinePositions()
    {
        if (stethoscopeLineRenderer == null || stethoscopeLineStart == null || stethoscopeLineEnd == null)
            return;

        Vector3 start = stethoscopeLineStart.position;
        Vector3 end = stethoscopeLineEnd.position;

        for (int i = 0; i < stethoscopeCurveSegments; i++)
        {
            float t = i / (float)(stethoscopeCurveSegments - 1);
            Vector3 straightPoint = Vector3.Lerp(start, end, t);
            float curveOffset = Mathf.Sin(t * Mathf.PI) * stethoscopeCurveDrop;
            Vector3 curvedPoint = straightPoint + new Vector3(0, -curveOffset, 0);
            stethoscopeLineRenderer.SetPosition(i, curvedPoint);
        }
    }
    // 👇 Call this from Update to keep line following
    private void Update()
    {
        if (isIpad)
        {
            zoomValue = 3f;
        }
        else if (isIphone) 
        {
            zoomValue = 4f;
        }

        {
            UpdateStethoscopeCurvedLinePositions();
        }

        // Keep injection at locked position if needed
        if (forceFreezeInjection && injection != null)
        {
            injection.position = injectionLockedPosition;
            injection.rotation = Quaternion.Euler(0f, 0f, -43.508f);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    #endregion

    #region Step 4 - Vaccine Injection
    void StartStep4()
    {
       
        Vector3 targetFocusPosition = new Vector3(2.01f, -0.5f, -10f);
        float targetZoom = 3f;
        float zoomDuration = 2f;

        Sequence camSequence = DOTween.Sequence();
        camSequence.Append(Camera.main.transform.DOMove(targetFocusPosition, zoomDuration).SetEase(Ease.InOutSine));
        camSequence.Join(Camera.main.DOOrthoSize(zoomValue, zoomDuration).SetEase(Ease.InOutSine));
        camSequence.OnComplete(() =>
        {
            Debug.Log("Breathing problem camera zoom complete!");
            step4Container.gameObject.SetActive(true);
            //PatChair.transform.position = patChairPos.transform.position;
            if (injectionTray)
            {
                injectionTray.gameObject.SetActive(true);
                Vector3 startPos = injectionTray.position;
                injectionTray.position = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
                injectionTray.DOMove(startPos, 1f).SetEase(Ease.OutBack);
                if (injection)
                {
                    injection.gameObject.SetActive(true);
                    SoundManager.instance.PlayGiveVacciene();
                    // Show placement indicator on the patient's arm
                    if (injectionTargetMarker)
                    {
                        injectionTargetMarker.gameObject.SetActive(true);
                        injectionTargetMarker.DOScale(0.7f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                    }

                    injection.GetComponent<DraggableObject>().OnDragEndEvent += OnInjectionDragEnd;
                    injection.GetComponent<DraggableObject>().OnDragStartEvent += OnInjectionDragStart;
                }
            }
        });

        // Show injection tray sliding up from bottom
       

        // Show injection
      
    }

    void OnInjectionDragStart(Transform draggedObject, Quaternion rotation)
    {
        // Orient the injection correctly during drag
        injectionLiquid.GetComponent<SpriteRenderer>().sortingOrder = 2500;
        injectionPlunger.GetComponent<SpriteRenderer>().sortingOrder = 199;
        draggedObject.DORotate(new Vector3(0, 0, -45), 0.3f); // Angle it for injection
        SoundManager.instance.PlayClick();
    }
    public Transform injectionPosition;
    void OnInjectionDragEnd(Transform draggedObject, Vector3 position)
    {
        if (injectionTargetMarker && Vector3.Distance(position, injectionTargetMarker.position) < 1.5f)
        {
            // Move the injection to the target marker position directly
            draggedObject.DOMove(injectionPosition.position, 0.3f);
            draggedObject.DORotate(new Vector3(0, 0, -45), 0.3f).OnComplete(() =>
            {
                // Hide target marker
                injectionTargetMarker.gameObject.SetActive(false);

                // Move it slightly up and to the left
                //Vector3 slightOffset = new Vector3(-0.4f, 0.5f, 0f);
                //draggedObject.DOMove(draggedObject.position + slightOffset, 0.3f).SetEase(Ease.OutQuad);

                // Also move the injectionPos GameObject off-screen
                if (injectionPos)
                {
                    injectionPos.DOMoveX(injectionPos.position.x + 50f, 0.6f).SetEase(Ease.InBack);
                }

                // Start the injection animation after a short delay
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    StartInjectionAnimation();
                });
            });
        }
    }


    void StartInjectionAnimation()
    {
        float injectionDuration = 5f; // Duration in seconds for a slow injection effect

        if (injectionPlunger)
        {
            Vector3 startPos = new Vector3(1.68560028f, 0.242599964f, 0f);
            Vector3 endPos = new Vector3(0.939999998f, 0.239999995f, 0f);

            injectionPlunger.localPosition = startPos;
            injectionPlunger.DOLocalMove(endPos, injectionDuration).SetEase(Ease.InOutQuad);

            if (injectionLiquid)
            {
                Vector3 startScale = new Vector3(0.526239991f, 0.526239991f, 0.526239991f);
                Vector3 endScale = new Vector3(-5.26239965e-05f, 0.526239991f, 0.526239991f);

                injectionLiquid.localScale = startScale;
                injectionLiquid.DOScale(endScale, injectionDuration).SetEase(Ease.InOutQuad);
            }

            // After animation completes
            DOVirtual.DelayedCall(injectionDuration, () =>
            {
                // ✅ Show injection mark
                if (injectionMark)
                {
                    injectionMark.gameObject.SetActive(true);
                    injectionMark.localScale = Vector3.zero;
                    injectionMark.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f).SetEase(Ease.OutBack);
                }

                // ✅ Then finish the step
                CompleteInjectionStep();
            });
        }
    }


    void CompleteInjectionStep()
    {
        // Show injection mark with correct scale
        if (injectionMark)
        {
            injectionMark.gameObject.SetActive(true);
            injectionMark.localScale = Vector3.zero;
            injectionMark.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f).SetEase(Ease.OutBack);
        }

        // Move injection tray down
        if (injectionTray)
        {
            injectionTray.DOMove(injectionTray.position - new Vector3(0, 10f, 0), 0.7f).SetEase(Ease.InBack);
        }

        // Wait for the tray to go down, then bring up the plaster tray and enable plaster
        DOVirtual.DelayedCall(1.5f, () =>
        {
            // Bring up plaster tray
            if (plasterTray)
            {
                plasterTray.gameObject.SetActive(true);
                Vector3 targetPos = plasterTray.position;
                plasterTray.position = targetPos - new Vector3(0, 10f, 0);
                plasterTray.DOMove(targetPos, 1f).SetEase(Ease.OutBack);
            }

            // Enable plaster object
            if (plaster)
            {
                plaster.gameObject.SetActive(true);

                // Attach drag events
                var dragScript = plaster.GetComponent<DraggableObject>();
                if (dragScript)
                {
                    dragScript.OnDragStartEvent += OnPlasterDragStart;
                    dragScript.OnDragEndEvent += OnPlasterDragEnd;
                }
            }

            // Hide the injection itself (if needed)
            if (injection)
            {
                injection.gameObject.SetActive(false);
            }
        });
    }
    void OnPlasterDragStart(Transform draggedObj, Quaternion rotation)
    {
        // draggedObj.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        SoundManager.instance.PlayClick();
    }

    void OnPlasterDragEnd(Transform draggedObj, Vector3 pos)
    {
        if (injectionMark && Vector3.Distance(pos, injectionMark.position) < 1f)
        {
            draggedObj.DOMove(injectionMark.position, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                // Lock plaster in place
                draggedObj.GetComponent<DraggableObject>().enabled = false;

                // Keep plaster scale at 0.1
                draggedObj.DOScale(new Vector3(0.1f, 0.1f, 0.1f), 0.25f).SetEase(Ease.OutBack);


                // Hide the needle mark after plaster is placed
                injectionMark.gameObject.SetActive(false);
                plaster.SetParent(catArm);

                // Proceed to next step after plaster is placed
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    HideAllStep4Elements();

                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        // Move plaster tray down
                        if (plasterTray)
                        {
                            plasterTray.DOMove(plasterTray.position - new Vector3(0, 10f, 0), 0.7f).SetEase(Ease.InBack);
                        }
                        catHappyFace.gameObject.SetActive(true);
                        catSadFace.gameObject.SetActive(false);
                        catSadFace.GetComponent<SpriteRenderer>().enabled = true;
                        // Optional: Start next step
                        StartCoroutine(FadeIntoStep7Routine());
                    });
                });
            });
        }
    }



    void HideAllStep4Elements()
    {
        if (injectionTray)
        {
            injectionTray.DOMove(injectionTray.position - new Vector3(0, 10f, 0), 0.7f).SetEase(Ease.InBack);
        }

        if (injection)
        {
            injection.DOScale(0f, 0.3f).OnComplete(() => injection.gameObject.SetActive(false));
        }

        if (greenCheckmark)
        {
            greenCheckmark.DOScale(0f, 0.3f).OnComplete(() => greenCheckmark.gameObject.SetActive(false));
        }
    }
    #endregion

    void StartStep5()
    {

        // Slide in the cardboard from below

        // Camera zoom and position
        Vector3 targetFocusPosition = new Vector3(0, 0, -10f);
        float targetZoom = 5f;
        float zoomDuration = 2f;

        Sequence camSequence = DOTween.Sequence();
        camSequence.Append(Camera.main.transform.DOMove(targetFocusPosition, zoomDuration).SetEase(Ease.InOutSine));
        camSequence.Join(Camera.main.DOOrthoSize(targetZoom, zoomDuration).SetEase(Ease.InOutSine));
        camSequence.OnComplete(() =>
        {
            step5Container.gameObject.SetActive(true);
            //patientMother.transform.DOMove(motherPosForSign.transform.position, 1).SetEase(Ease.InBack);
            ////    motherAmimator.Play("MotherStandingIdle");
            //paitentPos.gameObject.SetActive(false);
            //    PatChair.gameObject.SetActive(false);

            if (cardBoard)
            {
                cardBoard.gameObject.SetActive(true);
                Vector3 startPos = cardBoard.position;
                cardBoard.position = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
                cardBoard.DOMove(startPos, 1f).SetEase(Ease.OutBack);
            }
            SoundManager.instance.PlayMumSignForm();
            if (pen)
            {
                pen.gameObject.SetActive(true);

                Vector3 penStartPos = pen.position;
                pen.position = new Vector3(penStartPos.x, penStartPos.y - 10f, penStartPos.z);
                pen.DOMove(penStartPos, 1f).SetEase(Ease.OutBack);

                // Attach drag events
                var dragScript = pen.GetComponent<DraggableObject>();
                if (dragScript)
                {
                    dragScript.OnDragEndEvent += OnPenDragEnd;
                    dragScript.OnDragStartEvent += OnPenDragStart;
                }

                pen.GetComponent<SpriteRenderer>().sortingOrder = 700;
            }
        });
        // Slide in the pen from below
       
    }

    void OnPenDragStart(Transform draggedObj, Quaternion rotation)
    {

        draggedObj.GetComponent<SpriteRenderer>().sortingOrder = 705;
        SoundManager.instance.PlayClick();
    }

    void OnPenDragEnd(Transform draggedObj, Vector3 pos)
    {
        // Return pen to default sorting layer
        draggedObj.GetComponent<SpriteRenderer>().sortingOrder = 700;

        // Check distance between pen nib and signature space
        if (Vector3.Distance(penTriggerer.position, signatureSpace.position) < 1.5f)
        {
            // Calculate offset between pen and its nib
            Vector3 nibOffset = pen.position - penTriggerer.position;

            // Calculate final position so nib aligns with signature space
            Vector3 targetPos = signatureSpace.position + nibOffset;

            draggedObj.DOMove(targetPos, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                // Disable drag
                draggedObj.GetComponent<DraggableObject>().enabled = false;

                // Check and trigger signature animation and hiding logic
                CheckPenToSignatureProximity();
               
                Debug.Log("Pen nib aligned and signature animation triggered.");
            });
        }
    }

    void CheckPenToSignatureProximity()
    {
        if (penTriggerer && signatureSpace && Vector3.Distance(penTriggerer.position, signatureSpace.position) < 1f)
        {
            if (!signature.gameObject.activeSelf)
            {
                signature.gameObject.SetActive(true);

                if (signatureAnimator)
                {
                    signatureAnimator.enabled = true;
                    SoundManager.instance.PlayWriting();
                    signatureAnimator.Play("SignatureAnimation");
                }

                // Hide pen and cardboard after 3.5 seconds
                DOVirtual.DelayedCall(3.5f, () =>
                {
                    if (signatureAnimator)
                        signatureAnimator.enabled = false;
                    signature.SetParent(signatureSpace);

                    Sequence exitSequence = DOTween.Sequence();

                    if (pen)
                        exitSequence.Join(pen.DOMove(pen.position - new Vector3(0, 10f, 0), 1f).SetEase(Ease.InBack));
                    if (cardBoard)
                        exitSequence.Join(cardBoard.DOMove(cardBoard.position - new Vector3(0, 10f, 0), 1f).SetEase(Ease.InBack));
                    // After both animations finish, call next step
                    exitSequence.OnComplete(() =>
                    {
                        Debug.Log("Step 5 complete. Starting next step...");
                        StartStep4();
                        step5Container.gameObject.SetActive(false);// Replace with your actual next step method
                    });
                });
            }
        }
    }
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    public Transform mother;
    public Animator motherAnimator;
    public Transform patient;
    public Animator patientAnimator;
    [SerializeField] private ScreenFade screenFade;
    public Transform motherFoot;
    public Transform paitentFoot;
    public Transform motherPos;
    public Transform patientPos;
    public Transform chairPos;
    public Transform charPosHappy;
    public Transform motherPosHappy;

    private IEnumerator FadeIntoStep7Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic
        mother.position = iniMotherPos;
        mainCamera.orthographicSize = 6;
        mainCamera.transform.position = new Vector3(0, 0, -10);
        DoctorCabin.SetActive(true);
        DoctorCabin.transform.position = doctorIniPos;
        mother.position = motherPos.position;
        patient.position = patientPos.position;
        PatChair.transform.position = chairPos.position;
        motherFoot.gameObject.SetActive(true);
        paitentFoot.gameObject.SetActive(true);
        PatChair.gameObject.SetActive(false);
        Vector3 targetPosition = charPosHappy.position ;
        motherAmimator.transform.position = motherPosHappy.position;
        // Animate scale and position together using a Sequence
        Sequence moveAndScale = DOTween.Sequence();
        moveAndScale.Join(patient.transform.DOScale(0.6f, 1f).SetEase(Ease.OutBack));
        moveAndScale.Join(patient.transform.DOMove(targetPosition, 1f).SetEase(Ease.OutSine));
        plaster.gameObject.SetActive(false);    
        motherAnimator.Play("MotherStandingIdle");
        patientAnimator.Play("HappyFinal");
        BringEveryThingTogether();
        SoundManager.instance.PlayWellDone(); 
        SoundManager.instance.PlayDone();
        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }
    void BringEveryThingTogether() 
    {
    DOVirtual.DelayedCall(4f, () =>
        {

            if (StarProgressManager.Instance != null)
            {
                StarProgressManager.Instance.AddCuredPatient();
                GlobalManager.Instance.hasComeFromTheMainMenu = true;

                // The AddCuredPatient method already includes SaveProgress(),
                // but if you need to call save explicitly elsewhere:
                // StarProgressManager.Instance.SaveProgress();
            }
            else
            {
                Debug.LogError("StarProgressManager instance not found!");
            }
            fadeInAnim.Play("FadeIn");
            DOVirtual.DelayedCall(1.5f, () =>
            {
                SceneManager.LoadScene(0);

            });


        });

    }
}




