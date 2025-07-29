using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class HealthCheckManager : MonoBehaviour
{
    public Camera camera;
    [Header("Step 0 - Introduction")]
    public GameObject storySprite;
    public Transform[] introSprites;
    public Transform patientCharacter;
    public Transform doctorCharacter;
    public Animator animator;
    public Animator characterAnim;
    public Transform sittingPosForPatient;
    public Transform WalkingStartPosForPatient;

    [Header("Step 1 - Robe Dressing")]
    public Transform step1Container;
    public Transform robeHanger;
    public Transform robeOnPatient;
    public Button doneButton1;
    public Transform robePatient;
    public Transform robTop, robBottom;
    public Transform normaldress;
    public Transform chrPos;
    public Transform patlegs;
    public Transform patRobLegs;


    [Header("Step 2 - Height/Weight Measurement")]
    public Transform step2Container;
    public Transform measurementDevice;
    public Transform measurementBar;
    public Button measureButton;
    public GameObject measureButtonPressed;
    public GameObject measureButtonNotPressed;
    public TMP_Text weightText;
    public TMP_Text heightText;
    private float currentWeight = 40f;
    private float currentHeight = 120f;
    public Transform robePatientReal;
    public Transform robTopReal, robBottomReal;
    public Transform normaldressReal;
    public Transform docCabin;
    public Transform chrPosStep2;
    public Transform measureBarPos;
    public Transform chrHeadPos;
    public Button doneButtonOrg;

    [Header("Step 3 - Blood Pressure")]
    public Transform step3Container;
    public Transform bloodPressureCuffRight;
    public Transform bloodPressureCuffLeft;
    public Transform bloodPressureCuffClosed;
    public Transform pressureBulb;
    public Image pulseIndicator;
    public TMP_Text topPressureText;
    public TMP_Text bottomPressureText;
    private int pulseCount = 0;
    private bool leftCuffPressed = false;
    private bool rightCuffPressed = false;
    public Button doneButtonForBloodPressure;

    [Header("Step 4 - Topographic Scan")]
    public Transform step4Container;
    public Transform scannerDevice;
    public Transform scanBed;
    public Button scanButton;
    public Image scanProgressBar;
    public GameObject scanButtonPressed;
    public GameObject scanButtonNotPressed;
    public Button scanTaskDone;

    [Header("Step 5 - Vision Check")]
    public Transform step5Container;
    public Transform magnifyingGlass;
    public Transform[] visionOptions;
    public Transform[] eyeCharts;
    private int visionAttempts = 0;
    private int maxVisionAttempts = 3;
    private int currentHighlightedIndex = -1;
    private int correctAnswerIndex = -1;
    private char[] eyeChartSymbols = new char[] { 'A', 'B', 'C', 'D', 'a', 'b', 'c', 'd', '1', '2', '3', '4', '5' };

    // Define which positions are available in your eyeCharts array
    private Dictionary<int, int> symbolPositionMap = new Dictionary<int, int>();
    [Header("Step 6 - Hearing Check")]
    public Transform step6Container;
    public Transform headphones;
    public Button hearingTestButtonRight;
    public Button hearingTestButtonLeft;

    public ParticleSystem musicNotesRight;
    public ParticleSystem musicNotesLeft;
    public Transform patPosforHearCheck;


    [Header("Step 5 - Signature Elements")]
    public Transform step7Container;
    public Transform cardBoard;
    public Transform pen;
    public Transform signatureSpace;
    public Transform signature;
    public Animator signatureAnimator;
    public Transform penTriggerer;
    public Transform motherPosForSign;
    public Animator motherAmimator;
    public Transform paitentChair;
    private int currentStep = 0;
    private DraggableObject draggable;


    public Animator fadeInAnim;
    [SerializeField] private ScreenFade screenFade;
    void Start()
    {
        animator.keepAnimatorStateOnDisable = true;
        CheckDeviceResolution();
        animator.Play("IdleStandingPosition");
        //draggable = GetComponent<DraggableObject>();
        //if (draggable != null)
        //{
        //    draggable.OnDragClickEvent += OnBulbPressed;
        //}
        //StartStep5();
        InitializeScene();
        StartCoroutine(HandleCatEntryThenShowProblem());
        //StartStep6();
        //StartStep7();
        // StartStep3();
        //InitializeSymbolPositionMap();

        // Setup the UI for the first test
        //SetupNewVisionTest();
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

        // Final sitting position
        Vector3 sittingPos = sittingPosForPatient.position;

        // Walking starts off-screen with walking Y pos
        Vector3 walkingStartPos = WalkingStartPosForPatient.position;
        characterAnim.transform.position = walkingStartPos;
        characterAnim.gameObject.SetActive(true);
        // Play walking animation
        characterAnim.Play("Walking");

        // Move horizontally to chair (maintain Y = -4)
        characterAnim.transform.DOMoveX(sittingPosForPatient.position.x, 3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // Play idle standing first
                characterAnim.Play("IdleSittingPosition");

                // Smoothly move up to sitting Y position
                characterAnim.transform.DOMoveY(sittingPosForPatient.position.y, 0.4f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        // Play storytelling after sitting
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            characterAnim.Play("StoryTelling");
                        });
                    });
            });

        // Wait for entire walk and sit sequence
        yield return new WaitForSeconds(4.5f); // 3s walk + 0.4s sit + 1s delay

        // Continue the story
        StartCoroutine(StartHealthCheck());
    }
    void InitializeScene()
    {
        step1Container.gameObject.SetActive(false);
        step2Container.gameObject.SetActive(false);
        step3Container.gameObject.SetActive(false);
        step4Container.gameObject.SetActive(false);
        step5Container.gameObject.SetActive(false);
        step6Container.gameObject.SetActive(false);
        step7Container.gameObject.SetActive(false);

        //robeOnPatient.gameObject.SetActive(false);
        measurementBar.gameObject.SetActive(false);
        //signature.gameObject.SetActive(false);
        //happyPatient.gameObject.SetActive(false);
    }

    IEnumerator StartHealthCheck()
    {
        storySprite.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        SoundManager.instance.PlayBlabla();

        for (int i = 0; i < introSprites.Length; i++)
        {
            Transform sprite = introSprites[i];
            if (sprite != null)
            {
                sprite.localScale = Vector3.zero;
                sprite.gameObject.SetActive(true);

                // Scale in the current sprite
                sequence.Append(sprite.DOScale(0.3f, 0.5f).SetEase(Ease.OutBack));

                // Wait before scaling out
                sequence.AppendInterval(1f);

                // Scale out and deactivate
                int capturedIndex = i;
                sequence.Append(sprite.DOScale(0f, 0.5f).SetEase(Ease.InBack));
            }
        }

        // Wait a bit then proceed to the next step
        sequence.AppendInterval(0.5f);
        sequence.OnComplete(() =>
        {
            StartCoroutine(FadeIntoStep2Routine());
        });

        yield break; // exit the coroutine; the rest is handled by the sequence
    }

    private IEnumerator FadeIntoStep2Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic
        patlegs.gameObject.SetActive(false);
        patRobLegs.gameObject.SetActive(true);

        StartStep1();
        

        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }
    #region Step 1 - Robe Dressing
    void StartStep1()
    {
        characterAnim.Play("IdleStandingPosition");

        step1Container.gameObject.SetActive(true);
        patientCharacter.transform.position = chrPos.position;
        // Show robe sliding in from side
        robeHanger.position += Vector3.right * 10f;
        SoundManager.instance.PlayPutOnMedicalGown();
        DOVirtual.DelayedCall(0.5f, () =>
        {
            robeHanger.gameObject.SetActive(true);
            robeHanger.DOMoveX(robeHanger.position.x - 10f, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                robePatient.gameObject.SetActive(true);
                robeHanger.GetComponent<DraggableObject>().OnDragEndEvent += OnRobeDropped;
            });

        });
    }
    public Sprite legRobSprite;
    public Sprite legSprite;

    public GameObject legsRobleft;
    public GameObject legsRobRight;

    void OnRobeDropped(Transform robe, Vector3 position)
    {
        // Check if robe is near patient
        if (Vector3.Distance(position, patientCharacter.position) < 3f)
        {
            // Put robe on patient
            robeHanger.gameObject.SetActive(false);
            robeOnPatient.gameObject.SetActive(true);
            normaldress.gameObject.SetActive(false);
            robTop.gameObject.SetActive(true);
            robBottom.gameObject.SetActive(true);
            SoundManager.instance.PlayClick();

            legsRobleft.GetComponent<SpriteRenderer>().sprite = legRobSprite;
            legsRobRight.GetComponent<SpriteRenderer>().sprite = legRobSprite;

            // Show done button
            doneButton1.gameObject.SetActive(true);
            doneButton1.onClick.AddListener(FinishStep1);
            print("grabbed");
        }
        else
        {
            // Return robe to original position
            robe.DOMove(robeHanger.position, 0.5f);
        }
    }

    void FinishStep1()
    {

        StartCoroutine(FadeIntoStep3Routine());
    }

    private IEnumerator FadeIntoStep3Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic
        step1Container.gameObject.SetActive(false);
        StartStep2();

        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }
    #endregion

    #region Step 2 - Height/Weight Measurement
    void StartStep2()
    {
        print("step2Started");
        step2Container.gameObject.SetActive(true);
        patientCharacter.gameObject.SetActive(true);
        normaldressReal.gameObject.SetActive(false);
        robTopReal.gameObject.SetActive(true);
        measurementBar.gameObject.SetActive(true);
        robBottomReal.gameObject.SetActive(true);
        doctorCharacter.gameObject.SetActive(false);
        docCabin.gameObject.SetActive(false);
        SoundManager.instance.PlayMeasuringPatHeightAndWeight();

        if (DeviceResForResolution.Instance.isIpad)
        {
        chrPosStep2.position = new Vector3(chrPosStep2.position.x, chrPosStep2.position.y-0.35f, chrPosStep2.position.z);
            //camera.transform.position = new Vector3(0, -1.72f, -10);
        }
        patientCharacter.transform.position = chrPosStep2.position;

        // Position patient on scale
        patientCharacter.DOMove(chrPosStep2.position + new Vector3(0, 0.8f, 0), 1f)
            .OnComplete(() => {
                // measureButton.gameObject.SetActive(true);
                //measureButton.onClick.AddListener(StartMeasurement);

            });
    }

    public void StartMeasurement()
    {
        measureButton.gameObject.SetActive(false);
        measureButtonPressed.gameObject.SetActive(true);
        measureButtonNotPressed.gameObject.SetActive(false);

        // Set bar to its start position
        measurementBar.position = measureBarPos.position;

        // Calculate target local position based on chrHeadPos in world space
        Vector3 worldTargetPos = chrHeadPos.position;
        Vector3 localTargetPos = measurementBar.parent.InverseTransformPoint(worldTargetPos);
        SoundManager.instance.PlayWhoosh();

        measurementBar.DOLocalMove(localTargetPos, 1f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                measurementBar.localPosition = localTargetPos;

                currentWeight += Random.Range(0.1f, 0.5f);
                currentHeight += Random.Range(0.5f, 1.5f);

                weightText.text = currentWeight.ToString("F1") + " kg";
                heightText.text = currentHeight.ToString("F1") + " cm";

                //if (isIpad)
                //{
                //    // Adjust UI positions for iPad
                //    weightText.rectTransform.anchoredPosition = new Vector3(464, 356, 0);
                //    heightText.rectTransform.anchoredPosition = new Vector3(462, 135, 0);
                //    doneButtonOrg.GetComponent<RectTransform>().anchoredPosition = new Vector3(-1100, -449, 0);
                //}

                RectTransform btnRect = doneButtonOrg.GetComponent<RectTransform>();
                btnRect.anchoredPosition = new Vector2(-1800f, btnRect.anchoredPosition.y);
                doneButtonOrg.gameObject.SetActive(true);
                btnRect.DOAnchorPosX(191, 1f).SetEase(Ease.OutBack);

                doneButtonOrg.onClick.AddListener(FinishStep2);
            });
    }


    
    void FinishStep2()
    {

        StartCoroutine(FadeIntoStep4Routine());
    }
    #endregion

    private IEnumerator FadeIntoStep4Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic

        step2Container.gameObject.SetActive(false);
        patientCharacter.gameObject.SetActive(false);
        StartStep3();

        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }

    #region Step 3 - Blood Pressure
    void StartStep3()
    {
        step3Container.gameObject.SetActive(true);
        SoundManager.instance.PlayMeasureBloodPressure();

        // Enable right cuff interaction
        // var dragRight = bloodPressureCuffRight.GetComponent<DraggableObject>();
        //// dragRight.isClickable = true;
        // dragRight.OnDragClickEvent += OnRightCuffClicked;

        // // Enable left cuff interaction
        // var dragLeft = bloodPressureCuffLeft.GetComponent<DraggableObject>();
        // //dragLeft.isClickable = true;
        // dragLeft.OnDragClickEvent += OnLeftCuffClicked;
    }

    public void OnLeftCuffClicked(Transform obj, Vector3 pos)
    {
        Debug.Log("Left cuff clicked.");
        leftCuffPressed = true;
        SoundManager.instance.PlayClick();

        // Visual feedback (optional)
        obj.DOScale(1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);
        bloodPressureCuffLeft.GetComponent<ClickableSprite>().enabled = false;
        CheckBothCuffsPressed();
    }
    public void OnLeftCuffClicked(Transform obj)
    {
        Debug.Log("Left cuff clicked.");
        leftCuffPressed = true;
        SoundManager.instance.PlayClick();
        // Visual feedback: pop + fade
        obj.DOScale(1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);

        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr) sr.DOFade(0.5f, 0.3f); // Fades to 50% opacity

        CheckBothCuffsPressed();
    }
    void OnRightCuffClicked(Transform obj, Vector3 pos)
    {
        Debug.Log("Right cuff clicked.");
        rightCuffPressed = true;

        // Visual feedback (optional)
        obj.DOScale(1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);

        CheckBothCuffsPressed();
    }
    public void OnRightCuffClicked(Transform obj)
    {
        Debug.Log("Right cuff clicked.");
        rightCuffPressed = true;
        SoundManager.instance.PlayClick();

        // Visual feedback: pop + fade
        obj.DOScale(1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);
        bloodPressureCuffRight.GetComponent<ClickableSprite>().enabled = false;

        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr) sr.DOFade(0.5f, 0.3f); // Fades to 50% opacity

        CheckBothCuffsPressed();
    }
    void CheckBothCuffsPressed()
    {
        if (leftCuffPressed && rightCuffPressed)
        {
            Debug.Log("Both cuffs clicked. Showing closed cuff.");

            // Smoothly scale down and hide left cuff
            if (bloodPressureCuffLeft)
            {
                bloodPressureCuffLeft.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    bloodPressureCuffLeft.gameObject.SetActive(false);
                });
            }

            // Smoothly scale down and hide right cuff
            if (bloodPressureCuffRight)
            {
                bloodPressureCuffRight.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    bloodPressureCuffRight.gameObject.SetActive(false);
                });
            }

            // Delay, then show closed cuff with scale animation
            DOVirtual.DelayedCall(0.5f, () =>
            {
                if (bloodPressureCuffClosed)
                {
                    bloodPressureCuffClosed.localScale = Vector3.zero;
                    bloodPressureCuffClosed.gameObject.SetActive(true);
                    bloodPressureCuffClosed.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f).SetEase(Ease.OutBack);
                    pressureBulb.GetComponent<ClickableSprite>().enabled = true;
                    pressureBulb.GetComponent<CircleCollider2D>().enabled = true;
                    print("Hola");
                }
            });
        }
    }



    void OnCuffPlaced(Transform cuff, Vector3 position)
    {
        // Check if cuff is on arm
        if (Vector3.Distance(position, patientCharacter.position) < 2f)
        {
            // Show bulb
            pressureBulb.gameObject.SetActive(true);
            pressureBulb.GetComponent<DraggableObject>().OnDragClickEvent += OnBulbPressed;
        }
        else
        {
            // Return cuff to original position
            //cuff.DOMove(bloodPressureCuff.position, 0.5f);
        }
    }

    void OnBulbPressed(Transform obj, Vector3 pos)
    {
        Debug.Log($"Bulb clicked at position: {pos}, current pulse count: {pulseCount}");

        // Squash animation using DOScale (not DOMove — scale, not position!)
        obj.DOScale(new Vector3(0.8f, 0.4660401f, 0.8f), 0.15f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            // After 2 seconds, return to original scale
            DOVirtual.DelayedCall(2f, () =>
            {
                obj.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.15f).SetEase(Ease.OutBack);
            });
        });
        SoundManager.instance.PlayClick();

        // Pulse indicator animation
        pulseIndicator.DOFade(1f, 0.3f)
            .SetLoops(3, LoopType.Yoyo)
            .OnComplete(() => {
                pulseCount++;
                Debug.Log($"Pulse animation complete. Pulse count is now: {pulseCount}");

                if (pulseCount >= 3)
                {
                    int topPressure = Random.Range(110, 130);
                    int bottomPressure = Random.Range(70, 85);

                    Debug.Log($"Blood pressure result: {topPressure}/{bottomPressure}");

                    topPressureText.text = topPressure.ToString();
                    bottomPressureText.text = bottomPressure.ToString();

                    doneButton1.gameObject.SetActive(true);
                    doneButton1.onClick.RemoveAllListeners();
                    doneButton1.onClick.AddListener(FinishStep3);
                }
            });
    }

    public void OnBulbPressed(Transform obj)
    {
        // Disable interactivity
        var clickable = obj.GetComponent<ClickableSprite>();
        var collider = obj.GetComponent<CircleCollider2D>();
        if (clickable) clickable.enabled = false;
        if (collider) collider.enabled = false;

        // Animate bulb squash (Y-axis)
        obj.DOScale(new Vector3(0.8f, 0.4660401f, 0.8f), 0.1f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            // Restore to original scale after 2 seconds
            DOVirtual.DelayedCall(0.5f, () =>
            {
                obj.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.15f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    // Re-enable click/collider once scale is back
                    if (clickable) clickable.enabled = true;
                    if (collider) collider.enabled = true;

                    // If 3rd press, show final pressure and slide in button
                    if (pulseCount >= 2)
                    {
                        int topPressure = Random.Range(110, 130);
                        int bottomPressure = Random.Range(70, 85);

                        topPressureText.text = topPressure.ToString();
                        bottomPressureText.text = bottomPressure.ToString();

                        Debug.Log($"Blood pressure result: {topPressure}/{bottomPressure}");

                        // Slide in done button
                        RectTransform doneRect = doneButtonForBloodPressure.GetComponent<RectTransform>();
                        doneRect.anchoredPosition = new Vector2(-1989f, doneRect.anchoredPosition.y);
                        doneButtonForBloodPressure.gameObject.SetActive(true);
                        doneRect.DOAnchorPosX(-1115f, 0.5f).SetEase(Ease.OutBack);
                        if (clickable) clickable.enabled = false;
                        if (collider) collider.enabled = false;
                        doneButtonForBloodPressure.onClick.RemoveAllListeners();
                        doneButtonForBloodPressure.onClick.AddListener(FinishStep3);
                    }
                });
            });
        });

        // Pulse indicator animation
        pulseIndicator.DOFade(1f, 0.3f)
            .SetLoops(3, LoopType.Yoyo)
            .OnComplete(() =>
            {
                pulseCount++;
                Debug.Log($"Pulse animation complete. Pulse count is now: {pulseCount}");

                if (pulseCount == 1)
                {
                    topPressureText.text = "95";
                    bottomPressureText.text = "60";
                }
                else if (pulseCount == 2)
                {
                    topPressureText.text = "105";
                    bottomPressureText.text = "68";
                }
            });
    }












    void FinishStep3()
    {
        StartCoroutine(FadeIntoStepScanRoutine());
    }
    #endregion

    private IEnumerator FadeIntoStepScanRoutine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic
        step3Container.gameObject.SetActive(false);
        StartStep4();

        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }

    #region Step 4 - Topographic Scan
    void StartStep4()
    {
        step4Container.gameObject.SetActive(true);
        patientCharacter.gameObject.SetActive(true);
        SoundManager.instance.PlayComputerScan();

        // Position patient on scan bed
        //patientCharacter.DOMove(scanBed.position, 1f)
        //    .OnComplete(() => {
        //        //scanButton.gameObject.SetActive(true);
        //        //scanButton.onClick.AddListener(StartScan);
        //    });
    }

    public void OnScanButtonPressed()
    {
        scanButtonNotPressed.gameObject.SetActive(false);
        scanButtonPressed.gameObject.SetActive(true);
        SoundManager.instance.PlayXray();
        scannerDevice.DOMoveY(scannerDevice.position.y - 7f, 5f)
       .SetEase(Ease.InOutSine).OnComplete(() =>
       {
           RectTransform btnRect = scanTaskDone.GetComponent<RectTransform>();
           btnRect.anchoredPosition = new Vector2(-1800f, btnRect.anchoredPosition.y); // off-screen left
           SoundManager.instance.StopXray();

           scanTaskDone.gameObject.SetActive(true);
          // SoundManager.instance.PlayXray();

           // Animate to its intended on-screen position
           btnRect.DOAnchorPosX(-1100, 1f).SetEase(Ease.OutBack);
           scanTaskDone.onClick.AddListener(() => FinishStep4());
       });
    }

    void FinishStep4()
    {

        StartCoroutine(FadeIntoStep5Routine());
    }
    #endregion

    public Transform patPosForVisionCheck;
    private IEnumerator FadeIntoStep5Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic
        step4Container.gameObject.SetActive(false);
        patientCharacter.transform.position = patPosForVisionCheck.position;
        StartStep5();


        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }


    #region Step 5 - Vision Check
    void StartStep5()
    {
        step5Container.gameObject.SetActive(true);
        SoundManager.instance.PlayCheckPatSight();

        // Slide magnifying glass in from right
        Vector3 startPos = magnifyingGlass.position + Vector3.right * 10f;
        magnifyingGlass.position = startPos;
        foreach (Transform options in visionOptions) 
        {

            {
                if (isIpad)
                {
                    //camera.transform.position = new Vector3(0, -1.72f, -10);
                }
                foreach (var option in visionOptions)
                {
                    option.transform.position  = new Vector3(option.transform.position.x, -4.0F, option.transform.position.z);
                }
            }
        }
       

        magnifyingGlass.DOMoveX(startPos.x - 10f, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    MoveMagnifyingGlassTo(eyeCharts[2].position);

                    // Animate first vision options coming up
                    foreach (int i in new[] { 7, 3, 1 })
                    {

                        Transform option = visionOptions[i];
                        Vector3 targetPos = option.position;
                        Vector3 startHiddenPos = targetPos + Vector3.down * 5f;
                        option.position = startHiddenPos;
                        option.gameObject.SetActive(true);
                        option.DOMoveY(targetPos.y, 0.5f).SetEase(Ease.OutBack);
                    }
                });
            });
    }

    void AnimateVisionOptions(int[] toHide, int[] toShow, System.Action onComplete = null)
    {
        int completedCount = 0;

        if (toHide.Length == 0)
        {
            // If nothing to hide, just show and call onComplete
            foreach (int showIndex in toShow)
            {
                Transform showOption = visionOptions[showIndex];
                showOption.gameObject.SetActive(true);
                Vector3 originalPos = showOption.position;
                showOption.position = new Vector3(originalPos.x, originalPos.y - 5f, originalPos.z);
                showOption.DOMoveY(originalPos.y, 0.5f)
                    .SetEase(Ease.OutBack);
            }

            onComplete?.Invoke();
            return;
        }

        foreach (int index in toHide)
        {
            Transform option = visionOptions[index];
            Vector3 originalPos = option.position;

            option.DOMoveY(originalPos.y - 5f, 0.5f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    // Reset to original position so it comes from the same spot next time
                    option.position = originalPos;
                    option.gameObject.SetActive(false);
                    completedCount++;

                    if (completedCount == toHide.Length)
                    {
                        foreach (int showIndex in toShow)
                        {
                            Transform showOption = visionOptions[showIndex];
                            showOption.gameObject.SetActive(true);
                            Vector3 showOriginalPos = showOption.position;
                            showOption.position = new Vector3(showOriginalPos.x, showOriginalPos.y - 5f, showOriginalPos.z);
                            showOption.DOMoveY(showOriginalPos.y, 0.5f)
                                .SetEase(Ease.OutBack);
                        }

                        onComplete?.Invoke();
                    }
                });
        }
    }




    public void OnFirstAttempt()
    {
        MoveMagnifyingGlassTo(eyeCharts[5].position);
        AnimateVisionOptions(new[] { 7, 3, 1 }, new[] { 8, 0, 4 });
        SoundManager.instance.PlayClick();
        SoundManager.instance.PlayWhoosh();
        //visionOptions[0].GetComponent<ClickableSprite>().enabled = false;
    }

    public void OnSecondAttempt()
    {
        MoveMagnifyingGlassTo(eyeCharts[8].position);
        AnimateVisionOptions(new[] { 8, 0, 4 }, new[] { 0, 8, 5 });
        visionOptions[0].GetComponent<ClickableSprite>().enabled = false;
        visionOptions[0].GetComponent<BoxCollider2D>().enabled = false;

        SoundManager.instance.PlayClick();
        SoundManager.instance.PlayWhoosh();
    }

    public void OnThirdAttempt()
    {
        AnimateVisionOptions(new[] { 0, 4, 5, 8 }, new int[] { }, () => {
            //SoundManager.instance.PlayClick();

            FinishStep5();
        });
    }


    private void MoveMagnifyingGlassTo(Vector3 targetPosition)
    {
        magnifyingGlass.DOMove(targetPosition, 0.5f).SetEase(Ease.OutQuad);
        SoundManager.instance.PlayWhoosh();

    }

    private void SetVisionOptionsActive(int[] indices, bool isActive)
    {
        foreach (int index in indices)
        {
            if (index >= 0 && index < visionOptions.Length && visionOptions[index] != null)
            {
                visionOptions[index].gameObject.SetActive(isActive);
            }
        }
    }

    void OnVisionOptionSelected(Button selectedOption)
    {
        visionAttempts++;

        if (selectedOption == visionOptions[Random.Range(0, visionOptions.Length)]) // Random correct answer for demo
        {
            // Correct answer
            selectedOption.image.color = Color.green;

            // Show done button
            doneButton1.gameObject.SetActive(true);
            doneButton1.onClick.AddListener(FinishStep5);
        }
        else
        {
            // Wrong answer
            selectedOption.image.color = Color.red;

            if (visionAttempts >= 3)
            {
                // Force move to next step after 3 attempts
                FinishStep5();
            }
        }
    }
    private IEnumerator FadeIntoStep6Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic
        step5Container.gameObject.SetActive(false);
        camera.orthographicSize = 2.5f;
        camera.transform.position = new Vector3(0.21f, 0.46f, -10);
        StartStep6();


        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }
    void FinishStep5()
    {

        StartCoroutine(FadeIntoStep6Routine());
    }
    #endregion

    #region Step 6 - Hearing Check
    bool rightPressed = false;
    bool leftPressed = false;

    void StartStep6()
    {
        step6Container.gameObject.SetActive(true);
        patientCharacter.position = patPosforHearCheck.position;
        SoundManager.instance.PlayTestHearing();

        // Slide in headphones
        DOVirtual.DelayedCall(0.5f, () =>
        {

            headphones.position += Vector3.right * 10f;
            headphones.DOMoveX(headphones.position.x - 10f, 1f)
               .SetEase(Ease.OutBack)
               .OnComplete(() =>
               {
        DOVirtual.DelayedCall(1.5f, () => {

            hearingTestButtonRight.gameObject.SetActive(true);
            hearingTestButtonLeft.gameObject.SetActive(true);

            // Play right note initially
            musicNotesRight.Play();
            SoundManager.instance.PlayEarSound1();

            // Add listeners
            hearingTestButtonRight.onClick.AddListener(() =>
            {
                SoundManager.instance.PlayEarSound1();
                //SoundManager.instance.PlayClick();
                print("Helloroasdasfa22222222222222222222");

                if (!rightPressed)
                {
                    rightPressed = true;
                    musicNotesRight.Stop();
                    musicNotesLeft.Play();
                    print("Helloroasdasfa");
                    SoundManager.instance.PlayEarSound2();
                    // SoundManager.instance.PlayClick();


                }
            });

            hearingTestButtonLeft.onClick.AddListener(() =>
            {
                if (rightPressed && !leftPressed)
                {
                    leftPressed = true;
                    musicNotesLeft.Stop();

                    DOVirtual.DelayedCall(1f, () =>
                    {
                        FinishStep6();
                    });
                }
                SoundManager.instance.PlayClick();
            });


        });


               });
        });

    }


    void PlayHearingTest()
    {
        musicNotesLeft.Play();
        musicNotesRight.Play();

        // Show done button after test
        DOVirtual.DelayedCall(2f, () => {
            doneButton1.gameObject.SetActive(true);
            doneButton1.onClick.AddListener(FinishStep6);
        });
    }
    private IEnumerator FadeIntoStep7Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic
        camera.orthographicSize = 5f;
        camera.transform.position = new Vector3(0f, 0, -10);
        step6Container.gameObject.SetActive(false);
        docCabin.gameObject.SetActive(true);
        patientCharacter.gameObject.SetActive(false);
        paitentChair.gameObject.SetActive(false);
        StartStep7();

        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }
    void FinishStep6()
    {
        StartCoroutine(FadeIntoStep7Routine());
    }
    #endregion

    #region Step 7 - Final Form
    void StartStep7()
    {
        SoundManager.instance.PlaySignToFinishCheckUp();

        step7Container.gameObject.SetActive(true);
        DOVirtual.DelayedCall(0.5f, () =>
        {
            // Slide in cardboard and pen
            if (cardBoard)
            {
                cardBoard.gameObject.SetActive(true);
                Vector3 startPos = cardBoard.position;
                cardBoard.position = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
                cardBoard.DOMove(startPos, 1f).SetEase(Ease.OutBack);
            }

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
            SoundManager.instance.PlayWriting();

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
                        //StartStep4(); // Replace with your actual next step method
                        FinishStep7();
                    });
                });
            }
        }
    }
    public Transform PatPosAfterSig;
    public Transform paitent;
    public Transform patChair;
    public GameObject patLegs;
    private IEnumerator FadeIntoStep8Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic
        docCabin.gameObject.SetActive(true);
        doctorCharacter.gameObject.SetActive(true);
        paitent.gameObject.SetActive(true);
        patChair.gameObject.SetActive(true);
        print("helloooooooooooooooooooooooo");
        patientCharacter.position = PatPosAfterSig.position;
        normaldress.gameObject.SetActive(true);
        //reOnPatient.gameObject.SetActive(false);    
        robTop.gameObject.SetActive(false);
        robBottom.gameObject.SetActive(false);
        patlegs.gameObject.SetActive(true);
        patRobLegs.gameObject.SetActive(false);
       patLegs.gameObject.SetActive(true);
        patChair.gameObject.SetActive(false);
        legsRobleft.GetComponent<SpriteRenderer>().sprite = legSprite;
        legsRobRight.GetComponent<SpriteRenderer>().sprite = legSprite;
        characterAnim.Play("HappyEnd");
        SoundManager.instance.PlayPatHealthyAndStrong();
        SoundManager.instance.PlayDone();

        characterAnim.transform.position = new Vector3(characterAnim.transform.position.x, characterAnim.transform.position.y -1 , characterAnim.transform.position.z);

        EndGame();

        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }
    void FinishStep7()
    {
        StartCoroutine(FadeIntoStep8Routine());
    }

    public void EndGame() 
    { 
        //animator.Play("HappyEnd");
     DOVirtual.DelayedCall(4f, () =>
        {
            // Load the next scene or perform any other action
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
            DOVirtual.DelayedCall(1f, () =>
            {
                // Load the next scene or perform any other action
                SceneManager.LoadScene("MainMenu");
            });
        });

    }

    #endregion
}
