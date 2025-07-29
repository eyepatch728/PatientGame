using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class PneumoniaPatientManager : MonoBehaviour
{
    [SerializeField] private Transform[] problemSprites;
    [SerializeField] private Transform breathingEffect;

    [Header("Camera")]
    public Camera camera;

    [Header("Doctor Objects")]
    public GameObject DocObjects;
    public GameObject DoctorCabin;

    [Header("Doctor Objects")]
    public Animator characterAnim;
    
    public Transform sittingPosForPatient;
    public Transform WalkingStartPosForPatient;
    [Header("Screen Fade Anims")]
    [SerializeField] private ScreenFade screenFade; // Assign in Inspector
    public GameObject storyObj;

    [Header("Step 1 - Temperature")]
    public Transform step1Container;
    public Transform thermometerTray;
    public Transform thermometer;
    public Transform thermometerPlacementIndicator;
    public Transform thermometerLight;
    public RectTransform thermBar;
    public Image temperatureDisplay;
    public Image progressBar;
    public Transform catNose;

    [Header("Step 2 - Stethoscope Check")]
  
    [SerializeField] private Transform step2Container;
    [SerializeField] private Transform stethoscope;
    [SerializeField] private Transform stethoscopeEndpoint;
    [SerializeField] private Transform[] targetMarkers; // Targets to be touched in sequence
    [SerializeField] private GameObject progressBarForStethoScope;
    [SerializeField] private GameObject stethoScopeProgressBarParent;
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
    public GameObject Mouth, Paitent, PatChair, paitentPos, patChairPos;
    [SerializeField] Transform stethoscopeStartPos; // Off-screen right
    [SerializeField] Transform stethoscopeTargetPos; // Where it should appear
    

    [Header("Step 3 - Virus Elimination")]
    public Transform step3Container;
    public Transform xRayMachine;
    public Transform pinkLungs;
    public Transform[] lungGerms;
    public Transform germKillerDevice;
    public Transform greenArea;
    public Transform greenAreaPoint;
    public Transform warningSign;
    private int germsDestroyed = 0;
    private bool isDragging1 = false;        // Whether the gun is currently being dragged
    [SerializeField] private float shootDistance = 1.6f;       // Max distance for a successful "hit" with the green area
    [SerializeField] private float holdDurationPerTarget = 2f;

    [Header("Step 4 - Medication")]
    public Transform step4Container;
    public Transform pillTray;
    public Transform pill;
    public Transform waterGlassTray;
    public Transform waterGlass;
    public Transform emptyGlass;
    public Transform catMouth;
    public Transform catMouthOpen;
    public Transform catDrinkingWater;

    [Header("Completion")]
    public Transform happyPatient;
    public Transform waitingRoomProgressBar;

    private int currentStep = 0;
    public bool isIpad;
    public bool isIphone;
    private float aspectRatio;
    private Vector3 docObjectsStartPos;
    public Animator fadeInAnim;
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
    void Start()
    {
        CheckDeviceResolution();
        characterAnim.keepAnimatorStateOnDisable = true;

        docObjectsStartPos = DocObjects.transform.localPosition;
        InitializeScene();
        //StartCoroutine(ShowProblemAnimation());
        StartCoroutine(HandleCatEntryThenShowProblem());
        //ResetProgressBar();
        //ActivateTarget(currentTarget);
        //StartStep2();
        //SetupStethoscopeCurvedLineRenderer();
    }

    void InitializeScene()
    {
        step1Container.gameObject.SetActive(false);
        step2Container.gameObject.SetActive(false);
        step3Container.gameObject.SetActive(false);
        step4Container.gameObject.SetActive(false);

        thermometerPlacementIndicator.gameObject.SetActive(false);
        thermometerLight.gameObject.SetActive(false);
        if (temperatureDisplay) temperatureDisplay.gameObject.SetActive(false);
        if(happyPatient) happyPatient.gameObject.SetActive(false);
    }

    IEnumerator HandleCatEntryThenShowProblem()
    {
        yield return new WaitForSeconds(0.1f);

        // 1. Set walking animation and start position
        Paitent.transform.position = WalkingStartPosForPatient.position;
        characterAnim.gameObject.SetActive(true);
        characterAnim.Play("Walking");

        // 2. Move patient to sitting X position (maintaining walk Y)
        Vector3 walkTarget = new Vector3(sittingPosForPatient.position.x, WalkingStartPosForPatient.position.y, Paitent.transform.position.z);
        Paitent.transform.DOMoveX(walkTarget.x, 3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 3. After walk, move down to sitting Y
                Vector3 sitTarget = sittingPosForPatient.position;
                Paitent.transform.DOMoveY(sitTarget.y, 0.4f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        // 4. Play storytelling animation after sitting
                        characterAnim.Play("Storytelling");
                    });
            });

        // 5. Wait for full sequence to finish (walk + sit + buffer)
        yield return new WaitForSeconds(4f); // 3s walk + 0.4s sit + small buffer

        // 6. Continue the story
        StartCoroutine(ShowProblemAnimation());
    }



    private Vector3 initialCatPos = new Vector3( 0 , 0 , 0);
    IEnumerator ShowProblemAnimation()
    {
        SoundManager.instance.PlayBlabla();
        storyObj.SetActive(true);
        // Show breathing problem animation
        breathingEffect.localScale = Vector3.zero;
        breathingEffect.DOScale(0.6f, 1f).SetEase(Ease.OutBack);

        foreach (var sprite in problemSprites)
        {
            sprite.localScale = Vector3.zero;
            sprite.DOScale(0.6f, 1f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(2f);
        }

        yield return new WaitForSeconds(2f);

        // Transition to treatment
        foreach (var sprite in problemSprites)
        {
            sprite.DOScale(0, 0.3f).SetEase(Ease.InBack);
        }
        initialCatPos = Paitent.transform.position;

        float targetSize = 4f;
        float zoomDuration = 2f;
        camera.DOOrthoSize(targetSize, zoomDuration).SetEase(Ease.InOutSine);
        DoctorCabin.SetActive(true);
        DocObjects.transform.DOMove(DocObjects.transform.position + new Vector3(30f,0f,0f),1f);
        //DocObjects.SetActive(false);
        StartStep1();
        //StartStep4();
        
    }

    #region Step 1 - Temperature Measurement
    void StartStep1()
    {
       // Mouth.gameObject.SetActive(false);
        characterAnim.Play("IdleSittingAnimation");
        step1Container.gameObject.SetActive(true);
        if (DeviceResForResolution.Instance.isIpad)
        {
            Vector3 pos = step1Container.transform.position;
            pos.y -= 1f;
            step1Container.transform.position = pos;

            Vector3 pos1 = thermometerPlacementIndicator.position;
            pos.y += 2f;
            thermometerPlacementIndicator.position = pos;

        }
        PatChair.transform.parent = characterAnim.transform;
        if(isIpad)
        camera.transform.position = new Vector3(0, -1, -10f);
        if(isIphone)
        camera.transform.position = new Vector3(0, 0, -10f);

        // Scale down the patient to 0.5
        Paitent.transform.DOScale(0.5f, 0.5f).SetEase(Ease.OutQuad);

        // Move patient to the specified fixed position
        Paitent.transform.DOMove(new Vector3(-0.319999993f, -5.53f, 0), 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                // Show thermometer tray sliding up
                if (thermometerTray != null)
                {
                    thermometerTray.gameObject.SetActive(true);

                    // Slide it from 10 units below into place
                    Vector3 startPos = thermometerTray.position;
                    thermometerTray.position = startPos + Vector3.down * 10f;
                    thermometerTray
                        .DOMove(startPos, 1f)
                        .SetEase(Ease.OutBack);
                }
                SoundManager.instance.PlayThermometer();

                // Show thermometer
                if (thermometer != null)
                {
                    thermometer.gameObject.SetActive(true);

                    // Show placement indicator
                    if (thermometerPlacementIndicator != null)
                    {
                        if (isIpad)
                            thermometerPlacementIndicator.position = catNose.position + new Vector3(0, 1.1f, 0f);
                        if (isIphone)
                            thermometerPlacementIndicator.position = catNose.position + new Vector3(0, 1.5f, 0f);

                        thermometerPlacementIndicator
                            .DOScale(0.7f, 0.5f)
                            .SetLoops(-1, LoopType.Yoyo);
                    }

                    var draggable = thermometer.GetComponent<DraggableObject>();
                    draggable.OnDragEndEvent += OnThermometerDragEnd;
                    draggable.OnDragStartEvent += OnThermometerDragStart;
                }
            });


        //// Show thermometer tray sliding up
        //if (thermometerTray)
        //{
        //    thermometerTray.gameObject.SetActive(true);
        //    Vector3 startPos = thermometerTray.position;
        //    thermometerTray.position = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
        //    thermometerTray.DOMove(startPos, 1f).SetEase(Ease.OutBack);
        //}

        //// Show thermometer
        //if (thermometer)
        //{
        //    thermometer.gameObject.SetActive(true);
        //    // Show placement indicator
        //    if (thermometerPlacementIndicator)
        //    {
        //        thermometerPlacementIndicator.gameObject.SetActive(true);
        //        //thermometerPlacementIndicator.position = catFace.position;
        //        thermometerPlacementIndicator.position = catNose.position + new Vector3(0, 1.25f, 0f);
        //        thermometerPlacementIndicator.DOScale(0.7f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        //    }
        //    thermometer.GetComponent<DraggableObject>().OnDragEndEvent += OnThermometerDragEnd;
        //    thermometer.GetComponent<DraggableObject>().OnDragStartEvent += OnThermometerDragStart;
        //}
    }
    void OnThermometerDragStart(Transform draggedObject, Quaternion rotation)
    {
        draggedObject.DORotate(thermometer.transform.position, 0.3f);
        thermometerPlacementIndicator.gameObject.SetActive(true);
        SoundManager.instance.PlayClick();

    }
    void OnThermometerDragEnd(Transform draggedObject, Vector3 position)
    {
        thermometerPlacementIndicator.gameObject.SetActive(false);
        // Check if thermometer is near cat's head
        if (Vector3.Distance(position, catNose.position) < 3f)
        {
            SoundManager.instance.PlayCounter();
            thermometer.gameObject.GetComponent<DraggableObject>().isDragable = false;
            thermometer.gameObject.GetComponent<BoxCollider2D>().enabled = false;

            thermBar.gameObject.SetActive(true);
            // Position thermometer on target
            draggedObject.DOMove(thermometerPlacementIndicator.transform.position - new Vector3(-2.6f, 0.9f, 0f), 0.3f).OnComplete(() => {
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
                        // Wait 1 second before hiding elements
                        DOVirtual.DelayedCall(3f, () =>
                        {
                            HideAllStep1Elements();
                        });

                        // Hide all elements after delay

                    });
                }
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
                .SetEase(Ease.InBack).OnComplete(() => {
                   StartStep2();
                    step1Container.gameObject.SetActive(false);
                    ActivateTarget(currentTarget);
                    SetupStethoscopeCurvedLineRenderer();
                    //StartStep2();
                });

    }
    #region Step 2 - Stethoscope Functions

    public Transform step2PosForIpad;
    public void StartStep2()
    {
        SoundManager.instance.PlayWhoosh();

        SoundManager.instance.PlayListenLungWithStethoScope();
        print("Step2 Started");
        targetsInCat.SetActive(true);
        step2Container.gameObject.SetActive(true);
        if (isIpad) 
        {
        step2Container.transform.position = step2PosForIpad.position;
        }
        stethoscope.position += Vector3.right * 15f;
        //PatChair.transform.position = patChairPos.transform.position;
        //Paitent.transform.position = paitentPos.transform.position;

        DoctorCabin.SetActive(true);
        DocObjects.SetActive(false);
        // Setup the curved line before showing the stethoscope
        SetupStethoscopeCurvedLineRenderer();
        // Store the final target position first
        stethoscope.DOMoveX(stethoscope.position.x - 10f, 1f)
           .SetEase(Ease.OutBack)
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
        Vector3 initialPos = progressBar.transform.localPosition;

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
                    //stethoscope.gameObject.GetComponent<DraggableObject>().isDragable = false;
                    //stethoscope.gameObject.GetComponent<BoxCollider2D>().enabled = false;
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
                    //stethoscope.gameObject.GetComponent<DraggableObject>().isDragable = true;
                    //stethoscope.gameObject.GetComponent<BoxCollider2D>().enabled = true;

                }
               
                yield return null;
            }
        }
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
    private IEnumerator FadeIntoStep2Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic
        PatChair.SetActive(false);
        DocObjects.SetActive(false);

        StartStep3();
        DoctorCabin.SetActive(false);
        characterAnim.gameObject.SetActive(false);
        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }

    private void FinishStep2()
    {
        isDragging = false;

        stethoScopeProgressBarParent.transform.DOMoveX(stethoScopeProgressBarParent.transform.position.x - 10f, 1f);
        stethoscope.DOMoveX(stethoscope.position.x + 15f, 1f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                Debug.Log("Step 3 completed.");
                step3Container.gameObject.SetActive(false);
                //StartCoroutine(FadeIntoStep2Routine());
                StartStep3();
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


    #endregion
    #region Step 3 - Virus Elimination

    public Transform step3PosForIpad;
    public Transform step3PosForIphone;

    public Transform step3PosForIpadWhileGunActive;

    void StartStep3()
    {
        SoundManager.instance.PlayXrayDrag();
        step3Container.gameObject.SetActive(true);

        // Slide in X-ray machine
        xRayMachine.position += Vector3.right * 10f;
        xRayMachine.DOMoveX(xRayMachine.position.x - 10f, 1f)
            .SetEase(Ease.OutBack);
        if(isIpad)
        step3Container.transform.position = step3PosForIpad.position;
        else
            step3Container.transform.position = step3PosForIphone.position;
        DOVirtual.DelayedCall(4f, () =>
        {
            
        xRayMachine.GetComponent<DraggableObject>().OnDragEndEvent += OnXRayMachineDragEnd;
            xRayMachine.GetComponent<DraggableObject>().OnDragStartEvent += OnXRayMachineDragEnd;

        });
    }
    void OnXRayMachineDragEnd(Transform draggedObject, Quaternion rotation)
    {
        SoundManager.instance.PlayClick();

    }
    void OnXRayMachineDragEnd(Transform draggedObject, Vector3 position)
    {
        Sequence characterSequence = DOTween.Sequence();



        // Check distance to germs parent
        Vector3 germsParentPos = lungGerms[0].parent.position;
        if (Vector3.Distance(position, germsParentPos) < 1f)
        {
            SoundManager.instance.PlayVirus();

            // Disable dragging
            BoxCollider2D col = draggedObject.GetComponent<BoxCollider2D>();
            DraggableObject drag = draggedObject.GetComponent<DraggableObject>();
            if (col) col.enabled = false;
            if (drag)
            {
                drag.isDragable = false;
                drag.enabled = false;
            }
            // Snap into position
            draggedObject.DOMove(germsParentPos, 0.5f).OnComplete(() => {
                Debug.Log("XrayMachine correctly positioned");

                if (warningSign)
                {
                    //warningSign.position = germsParentPos + new Vector3(0, 0f, 0);
                    warningSign.localScale = Vector3.zero;
                    warningSign.gameObject.SetActive(true);

                    // Show warning animation (blinking)
                    warningSign.DOScale(0.17f, 0.3f).SetEase(Ease.OutBack)
                        .OnComplete(() => {
                            warningSign.DOScale(0.2f, 0.4f)
                                .SetLoops(4, LoopType.Yoyo)
                                .OnComplete(() => {
                                    warningSign.DOScale(0, 0.3f).SetEase(Ease.InBack)
                                        .OnComplete(() => {
                                            warningSign.gameObject.SetActive(false);
                                            pinkLungs.gameObject.SetActive(true);
                                            step3Container.transform.DOScale(new Vector3(2f, 2f, 0.4f), 1f)
                                     .SetEase(Ease.OutSine);
                                            if (isIpad)
                                            {
                                                step3Container.transform.DOMove(step3PosForIpadWhileGunActive.position, 1f)
                                                    .SetEase(Ease.OutQuad);
                                            }
                                            else
                                            {
                                                step3Container.transform.DOMove(new Vector3(5.42999983f, 9.57999992f, 0), 1f)
                                                    .SetEase(Ease.OutQuad);
                                            }
                                            Invoke(nameof(RevealGermsAndGun),2f);
                                        });
                                });
                        });
                }
                else
                {
                    RevealGermsAndGun();
                }
            });
        }
    }
    void OnGunDragStart(Transform draggedObject, Quaternion rotation)
    {
        SoundManager.instance.sfxSource.loop = true; // Set loop to true for continuous sound
        SoundManager.instance.PlayLaser();

    }
    void OnGunDragEnd(Transform draggedObject, Vector3 position)
    {
        SoundManager.instance.StopLaser();
        SoundManager.instance.sfxSource.loop = false; // Set loop to true for continuous sound

    }
    void RevealGermsAndGun()
    {
        SoundManager.instance.PlayGetRidOfTheBacteria();

        // Show germs
        foreach (var germ in lungGerms)
        {
            germ.gameObject.SetActive(true);
            germ.localScale = Vector3.zero;

            // Scale up initially, then start breathing loop
            germ.DOScale(0.3f, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                germ.DOScale(0.35f, 0.6f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo); // Breathing effect
            });
        }

        germKillerDevice.gameObject.SetActive(true);
        // Slide in germ killer gun
        germKillerDevice.position += Vector3.right * 10f;
        germKillerDevice.DOMoveX(germKillerDevice.position.x - 10f, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                var drag = germKillerDevice.GetComponent<DraggableObject>();
                if (drag) drag.enabled = true;
                germKillerDevice.GetComponent<DraggableObject>().OnDragStartEvent += OnGunDragStart;
                germKillerDevice.GetComponent<DraggableObject>().OnDragEndEvent += OnGunDragEnd;
                greenArea.gameObject.SetActive(true);
                germsDestroyed = 0;
                StartCoroutine(MonitorGermKilling());
            });
    }

    IEnumerator MonitorGermKilling()
    {
        while (germsDestroyed < lungGerms.Length)
        {
            isDragging1 = germKillerDevice.GetComponent<DraggableObject>().isDragging;

            for (int i = 0; i < lungGerms.Length; i++)
            {
                // Skip if not active or already out of range
                if (!lungGerms[i].gameObject.activeSelf ||
                    Vector3.Distance(greenArea.position, lungGerms[i].position) >= 1.5f)
                    continue;

                float timer = 0f;

                // Track time while gun is close
                while (timer < 2f &&
                       Vector3.Distance(greenArea.position, lungGerms[i].position) < 1.5f)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                if (timer >= 2f && lungGerms[i].gameObject.activeSelf)
                {
                    int index = i; // ✅ capture copy for lambda

                    lungGerms[index].DOKill(); // Stop any previous tweens like breathing

                    // First shake
                    lungGerms[index].DOShakeScale(
                        duration: 0.5f,
                        strength: 0.25f,
                        vibrato: 20,
                        randomness: 90,
                        fadeOut: true
                    ).OnComplete(() =>
                    {
                        // Then scale down and disable
                        lungGerms[index].DOScale(Vector3.zero, 0.5f)
                            .SetEase(Ease.InBack)
                            .OnComplete(() =>
                            {
                                lungGerms[index].gameObject.SetActive(false);
                                germsDestroyed++;

                                if (germsDestroyed >= lungGerms.Length)
                                {
                                    FinishStep3();
                                }
                            });
                    });

                    yield return new WaitForSeconds(1f); // Wait for shake + shrink to complete
                }
            }

            yield return null;
        }
    }

    #endregion
    void Update()
    {
        UpdateStethoscopeCurvedLinePositions();

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
            // Make sure holdTimers is declared outside this block if reused across frames
            float[] holdTimers = new float[lungGerms.Length];

            for (int i = 0; i < lungGerms.Length; i++)
            {
                if (!lungGerms[i].gameObject.activeSelf) continue;

                float distance = Vector3.Distance(greenAreaPoint.position, lungGerms[i].position);
                Debug.Log($"Distance to {lungGerms[i].name}: {distance}");

                if (distance < shootDistance)
                {
                    holdTimers[i] += Time.deltaTime;
                    Debug.Log($"Holding {lungGerms[i].name} for {holdTimers[i]:F2} seconds");

                    if (holdTimers[i] >= holdDurationPerTarget)
                    {
                        Debug.Log($"Monster killed: {lungGerms[i].name}");

                        // First shake the monster
                        lungGerms[i].DOShakeScale(
                            duration: 0.5f,
                            strength: 0.25f,
                            vibrato: 20,
                            randomness: 90,
                            fadeOut: true
                        ).OnComplete(() =>
                        {
                            // Then scale down and disable after shake
                            lungGerms[i].DOScale(Vector3.zero, 0.5f)
                                .SetEase(Ease.InBack)
                                .OnComplete(() =>
                                {
                                    lungGerms[i].gameObject.SetActive(false);
                                });
                        });

                        // Reset the timer for this monster
                        holdTimers[i] = 0f;
                    }
                }
                else
                {
                    holdTimers[i] = 0f;
                }
            }
        }

    }
    void FinishStep3()
    {
        xRayMachine.DOMoveX(xRayMachine.position.x + 10f, 1f)
            .SetEase(Ease.InBack);
        germKillerDevice.DOMoveX(germKillerDevice.position.x + 10f, 1f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                step3Container.gameObject.SetActive(false);
                StartStep4();
            });
    }

    #region Step 4 - Medication
    void StartStep4()
    {
        // If we're coming from Step 2 (skipping Step 3), make sure to deactivate Step 2
        //if (Step2Gameobject.gameObject.activeInHierarchy)
        //{
        //    Step2Gameobject.gameObject.SetActive(false);
        //}

        //// If we're coming from Step 3, make sure to deactivate it
        //if (Step3Gameobject.gameObject.activeInHierarchy)
        //{
        //    Step3Gameobject.gameObject.SetActive(false);
        //}

        step4Container.gameObject.SetActive(true);
        if (DeviceResForResolution.Instance.isIpad)
        {
            Vector3 pos = step4Container.transform.position;
            pos.y -= 1.5f;
            step4Container.transform.position = pos;

           

        }
        // Show pill tray sliding up
        if (pillTray)
        {
            pillTray.gameObject.SetActive(true);
            Vector3 startPos = pillTray.position;
            pillTray.position = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
            pillTray.DOMove(startPos, 1f).SetEase(Ease.OutBack);
        }
        SoundManager.instance.PlayGivePill();

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
            catMouthOpen.gameObject.SetActive(true);

            // Animate pill going into cat's mouth
            draggedObject.DOMove(catMouthOpen.position, 0.5f).OnComplete(() => {
                draggedObject.DOScale(0, 0.3f).SetEase(Ease.InBack);

                // Close cat's mouth
                DOVirtual.DelayedCall(0.5f, () => {
                    catMouthOpen.gameObject.SetActive(false);
                    catMouth.gameObject.SetActive(true);

                    // Hide pill tray
                    pillTray.DOMove(new Vector3(pillTray.position.x, pillTray.position.y - 10f, pillTray.position.z), 1f)
                        .SetEase(Ease.InBack).OnComplete(() => {
                            GiveWater();
                        });
                });
            });
        }
        else
        {
            // Return pill to tray
            draggedObject.DOMove(pillTray.position + new Vector3(0, 1f, 0), 0.5f);
        }
    }

    void GiveWater()
    {
        // Show water glass tray sliding up
        if (waterGlassTray)
        {
            waterGlassTray.gameObject.SetActive(true);
            Vector3 startPos = waterGlassTray.position;
            waterGlassTray.position = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
            waterGlassTray.DOMove(startPos, 1f).SetEase(Ease.OutBack);
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
            catMouthOpen.gameObject.SetActive(true);

            // Create a sequence for glass animation
            Sequence drinkingSequence = DOTween.Sequence();

            // Snap water glass to drink position (same as working one)
            //Vector3 drinkPosition = catMouth.position + new Vector3(1f, 0.5f, 0);
            //Vector3 drinkPosition = new Vector3(-6.22f, 3.9f, 0);
            //Vector3 drinkPosition = new Vector3(-8.6f, 5.25f, 0);
            //Vector3 drinkPosition = catMouth.position + new Vector3(2.19f, 2.5f, 0);
            Vector3 drinkPosition = Vector3.one;
            if (isIphone)
                drinkPosition = catMouth.position + new Vector3(1.25f, 0.1f, 0);
            else
                drinkPosition = catMouth.position + new Vector3(0.75f, 0.1f, 0);

            //draggedObject.position = drinkPosition;
            //draggedObject.rotation = Quaternion.Euler(0, 0, 0);
            drinkingSequence.AppendInterval(0.1f);
            // Move to correct position
            drinkingSequence.Append(draggedObject.DOMove(drinkPosition, 0.4f));
            //drinkingSequence.AppendInterval(0.1f);

            // Move and tilt water splash animation to match glass
            //drinkingSequence.Append(catDrinkingWater.DOLocalMove(new Vector3(-5.1f, 3.6f, 0), 0.02f));
            //drinkingSequence.Append(catDrinkingWater.DOLocalMove(new Vector3(3.5f, 3.5f, 0), 0.02f));
            drinkingSequence.AppendInterval(0.1f);
            //drinkingSequence.Append(catDrinkingWater.DOLocalRotate(new Vector3(0, 0, 30), 0.1f));

            // Tilt the water glass itself
            drinkingSequence.Append(draggedObject.DOLocalRotate(new Vector3(0, 0, 60), 1f));

            // Prepare water frames
            GameObject[] waterFrames = new GameObject[9];
            for (int i = 1; i <= 9; i++)
            {
                Transform child = catDrinkingWater.transform.Find(i.ToString());
                if (child != null)
                    waterFrames[i - 1] = child.gameObject;
            }

            drinkingSequence.AppendCallback(() =>
            {
                // Deactivate all except first
                for (int i = 1; i < waterFrames.Length; i++)
                    if (waterFrames[i] != null)
                        waterFrames[i].SetActive(false);

                if (waterFrames[0] != null)
                {
                    waterFrames[0].transform.position = draggedObject.position;
                    waterFrames[0].transform.rotation = draggedObject.rotation;
                    waterFrames[0].SetActive(true);
                }
            });

            // Loop through remaining frames
            for (int i = 1; i < waterFrames.Length; i++)
            {
                int frameIndex = i;
                int previousIndex = i - 1;

                if (waterFrames[frameIndex] != null)
                {
                    drinkingSequence.AppendInterval(0.2f);
                    drinkingSequence.AppendCallback(() =>
                    {
                        if (waterFrames[previousIndex] != null)
                            waterFrames[previousIndex].SetActive(false);

                        waterFrames[frameIndex].transform.position = draggedObject.position;
                        waterFrames[frameIndex].transform.rotation = draggedObject.rotation;
                        waterFrames[frameIndex].SetActive(true);
                    });
                }
            }

            drinkingSequence.AppendInterval(0.2f);
            drinkingSequence.AppendCallback(() =>
            {
                if (waterFrames[waterFrames.Length - 1] != null)
                    waterFrames[waterFrames.Length - 1].SetActive(false);

                // Replace with empty glass
                draggedObject.gameObject.SetActive(false);
                waterGlass.gameObject.SetActive(false);

                emptyGlass.transform.position = draggedObject.position;
                emptyGlass.transform.rotation = draggedObject.rotation;
                emptyGlass.gameObject.SetActive(true);
            });

            drinkingSequence.Append(emptyGlass.DOLocalRotate(Vector3.zero, 1f));

            drinkingSequence.OnComplete(() =>
            {
                // Reset mouth
                catDrinkingWater.DOLocalRotate(Vector3.zero, 0.5f);
                emptyGlass.DOMove(waterGlassTray.position + new Vector3(0, 1f, 0), 0.5f);

                catMouthOpen.gameObject.SetActive(false);
                catMouth.gameObject.SetActive(true);

                // Final transitions
                DOVirtual.DelayedCall(2f, () =>
                {
                    HideAllStep4Elements();
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        step4Container.gameObject.SetActive(false);
                        //StartCoroutine(FadeIntoStep6Routine());
                       // StartStep4();
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
        CompleteTreatment();
    }
    #endregion
    public void ResetMinimal()
    {
        // Reset player's scale
        Paitent.transform.localScale = new Vector3(0.25f , 0.25f, 0.25f) ;
        Paitent.transform.position =sittingPosForPatient.position;
        // Reset Doctor objects position
        DocObjects.SetActive(true);
        DocObjects.transform.DOMove(DocObjects.transform.position + new Vector3(-30f, 0f, 0f), 1f);
        PatChair.transform.parent = null;
        Paitent.transform.position = new Vector3(Paitent.transform.position.x, Paitent.transform.position.y-1, Paitent.transform.position.z);
        PatChair.GetComponent<SpriteRenderer>().sortingOrder = 0;
    }
    void CompleteTreatment()
    {
        // Show happy patient
        ResetMinimal();
        characterAnim.Play("FinalHappy");
        SoundManager.instance.PlayDone();
        SoundManager.instance.PlayWellDone();
        happyPatient.gameObject.SetActive(true);
        happyPatient.localScale = Vector3.zero;
        happyPatient.DOScale(1f, 0.5f)
            .SetEase(Ease.OutBack);

        // Update waiting room progress
        waitingRoomProgressBar.DOScaleX(1f, 1f);
        float targetSize = 5f;
        float zoomDuration = 2f;
        camera.DOOrthoSize(targetSize, zoomDuration).SetEase(Ease.InOutSine);
        DoctorCabin.SetActive(true);
        DocObjects.SetActive(true);
        DocObjects.transform.DOMove(DocObjects.transform.position + new Vector3(-30, 0f, 0f), 1f);
        // Move PatChair to x - 5
        //PatChair.transform.DOMoveX(PatChair.transform.position.x - 5f, 1f)
        //    .SetEase(Ease.OutSine);

        //// Move characterAnim to x - 5 and y - 0.8
        //Vector3 targetPos = characterAnim.transform.position + new Vector3(-5f, -0.8f, 0f);
        //characterAnim.transform.DOMove(targetPos, 1f)
        //    .SetEase(Ease.OutSine);


        // Return to waiting room after delay
        DOVirtual.DelayedCall(2f, () => {
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
                // Reset progress bar
                //waitingRoomProgressBar.DOScaleX(0f, 0.5f);
                // Go back to main menu
                SceneManager.LoadScene("MainMenu");
            });
        });
    }
}