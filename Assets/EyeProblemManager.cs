using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Threading;

public class EyeProblemManager : MonoBehaviour
{
    [Header("Camera")]
    public Camera amera;

    [Header("Problem Visualization")]
    public Transform[] computerOveruseSprites;
    public Transform redEyesLeft;
    public Transform redEyesRight;
    public GameObject paitentHead;
    private bool isStoryComplete = false;
    public GameObject story;
    [Header("Step 1 - Kill Germs")]
    public Transform step1Container;
    public Transform germKillerDevice;
    [SerializeField] private Transform gunDevice;            // The gun object that will slide in and be dragged
    public Transform monsterGO;
    public Transform eyeMask;
    public Transform[] monsterTargets;
    public GameObject doctorObjects;
    public Transform eyeMaskPos;
    [SerializeField] private Transform greenArea;
    [SerializeField] private GameObject shootEffect;
    [SerializeField] private float shootDistance =10f;
    private bool isDragging1 = false;
    private int currentMonster = 0;
    [SerializeField] private float holdDurationPerTarget = 2f;
    public Animator characterAnim;
    public Transform sittingPosForPatient;
    public Transform WalkingStartPosForPatient;



    [Header("Eye Test Components")]
    public Transform step2Container;
    [SerializeField] private Transform eyeTestDevice;
    [SerializeField] private Transform leftEyePosition;
    [SerializeField] private Transform rightEyePosition;
    [SerializeField] private Transform lensBoard;
    [SerializeField] private Transform eyeChart;
    [SerializeField] private Transform[] lenses;
    [SerializeField] private int correctLeftLensIndex = 2;
    [SerializeField] private int correctRightLensIndex = 2;
    [SerializeField] private Transform leftEyePos;
    [SerializeField] private Transform rightEyePos;
    Transform currentLeftLens = null; // Track left eye's current lens
    Transform currentRightLens = null;
    public Material spriteBlurMaterial;

    [Header("UI Components")]
    [SerializeField] private GameObject letterGrid; // Parent object containing all the letter sprites
    [SerializeField] private Transform glassesMenuContainer;
    [SerializeField] private ScrollRect glassesScrollView;
    [SerializeField] private Transform[] glassesFrames;
    [SerializeField] private Button greenButton;
    [SerializeField] private SpriteRenderer blurOverlaySpriteRenderer;


    [Header("Blur Settings")]
    [SerializeField][Range(0, 5)] private float maxBlurAmount = 3f;
    [SerializeField] private int blurCopies = 3; // How many copies to create for blur effect

    [Header("Cat Components")]
    [SerializeField] private Transform catFace;
    [SerializeField] private SpriteRenderer catMouthRenderer;
    [SerializeField] private Sprite neutralMouthSprite;
    [SerializeField] private Sprite happyMouthSprite;

    private bool isTestingLeftEye = true;
    private Transform currentLens;
    private Transform currentGlasses;
    private bool isLensPlaced = false;
    private bool isLeftEyeComplete = false;
    private bool isRightEyeComplete = false;

    // Store original letter objects and blur copies
    private GameObject[] letterCopies;
    private Transform[] originalLetters;
    private Vector3[] originalLetterScales;
    private Vector3[] originalLetterPositions;
    public Transform framesPos;
    public Button doneButton;

    [Header("Step 3 - Create Glasses")]
    public Transform step3Container;
    public Transform glassCutter;
    public Transform glassPiece;
    public Transform cuttingPath;
    public Image cuttingProgress;
    public Transform finishedGlasses;
    public Button doneBtn;
    private int currentGermCount = 0;
    private int lensesCorrect = 0;
    private int glassesSelected = 0;
    private float cuttingProgressValue = 0f;
    private bool isCutting = false;
    public int glassesIndex;
    public Transform cutterPos;
    public Animator glassCutterAnimator;
    public GameObject[] glassesInPatient;
    public GameObject happyFace;
    public Animator patientAnimator;
    void Start()
    {

        CheckDeviceResolution(); // Check device resolution at the start
        if (isIphone)
        {
            zoomValue = 1;
            cameraPos = new Vector3(-4.05999994f, 0.610000014f, -10);

        }
        else if (isIpad)
        {

            zoomValue = 0.8f;
            cameraPos = new Vector3(-2.8499999f, 0.519989967f, -10);
        }

        foreach (Transform frames in glassesFrames)
        {
            frames.position = framesPos.position;
        }

        //InitializeScene();
        StartCoroutine(HandleCatEntryThenShowProblem());

        //StartStep3(); // Start with Step 1
        //StartStep2();
        // InitializeEyeTest();
        //StartCoroutine(DelayBeforeGlassesSelection());
        correctLeftLensIndex = Random.Range(0, lenses.Length);
        correctRightLensIndex = Random.Range(0, lenses.Length);
        if (spriteBlurMaterial != null)
        {
            // Set the blur amount using the property name ("BlurAmount") from the shader
            spriteBlurMaterial.SetFloat("_BlurAmount", 0.0034f);
        }
        doneBtn.onClick.AddListener(() =>
        {
            frameDonebtn();
        });
        doneButton.onClick.AddListener(() =>
        {
            glassesDonebtn();
        });
        greenButton.onClick.AddListener(CompleteEyeTest);

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
        Vector3 sittingPos = characterAnim.transform.position;

        // Start from offscreen left and walking Y position
        Vector3 walkingStartPos = WalkingStartPosForPatient.position;
        characterAnim.transform.position = walkingStartPos;
        // Play walking animation
        characterAnim.gameObject.SetActive(true);
        characterAnim.Play("Walking");

        // Move to final position over 3 seconds
        characterAnim.transform.DOMoveX(sittingPosForPatient.position.x, 3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                Vector3 nextPos = sittingPosForPatient.position;
                characterAnim.transform.DOMove(nextPos, 0.3f)
                    .SetEase(Ease.OutQuad);

                // Play IdleStandDown animation first
                characterAnim.Play("IdleStandDown");

                // After 1 second, switch to Storytelling animation
                // Move up to sitting Y smoothly
                characterAnim.transform.DOMoveY(sittingPosForPatient.position.y, 0.4f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        // After 1s, play storytelling
                        DOVirtual.DelayedCall(0.1f, () =>

                        {
                            characterAnim.Play("Storytelling");
                        });
                    });
            });

        // Wait for movement to finish
        yield return new WaitForSeconds(3.5f);

        // Continue normal story
        //StartCoroutine(ShowProblemAnimation());
        AnimateIssue();
    }

    private void Update()
    {
        if (greenArea == null) return;

        // Animate greenArea scale based on drag state
        if (isDragging1)
        {
            // Scale up smoothly
            greenArea.localScale = Vector3.Lerp(greenArea.localScale, Vector3.one, Time.deltaTime * 5f);
        }
        else
        {
            // Scale down smoothly
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
            float[] holdTimers = new float[monsterTargets.Length];

            // Iterate over all monsters to check if the gun is close enough
            for (int i = 0; i < monsterTargets.Length; i++)
            {
                // Skip inactive monsters
                if (!monsterTargets[i].gameObject.activeSelf) continue;

                // Calculate the distance between the gun and the monster
                float distance = Vector3.Distance(greenArea.position, monsterTargets[i].position);
                Debug.Log($"Distance to {monsterTargets[i].name}: {distance}");

                // If within shoot distance, increment the timer
                if (distance < shootDistance)
                {
                    holdTimers[i] += Time.deltaTime; // Increment timer only when in range
                    Debug.Log($"Holding {monsterTargets[i].name} for {holdTimers[i]:F2} seconds");

                    // If the timer reaches the set duration (2 seconds), kill the monster
                    if (holdTimers[i] >= holdDurationPerTarget)
                    {
                        Debug.Log($"Monster killed: {monsterTargets[i].name}");

                        // Apply scaling effect to make the monster shrink and disappear
                        monsterTargets[i].DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);

                        // Disable the monster after it is "killed"
                        monsterTargets[i].gameObject.SetActive(false);

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
    void InitializeScene()
    {
        step1Container.gameObject.SetActive(false);
        step2Container.gameObject.SetActive(false);
        step3Container.gameObject.SetActive(false);
        //confirmGlassesButton.gameObject.SetActive(false);
    }

    void AnimateIssue()
    {
        story.SetActive(true);
        

        Sequence sequence = DOTween.Sequence();
        SoundManager.instance.PlayBlabla();

        for (int i = 0; i < computerOveruseSprites.Length; i++)
        {
            Transform sprite = computerOveruseSprites[i];
            if (sprite != null)
            {
                sprite.localScale = Vector3.zero;
                sprite.gameObject.SetActive(true);

                // Scale in
                sequence.Append(sprite.DOScale(0.6f, 0.5f).SetEase(Ease.OutBack));
                sequence.AppendInterval(1.0f); // Stay visible

                // Scale out and deactivate
                sequence.Append(sprite.DOScale(0f, 0.5f).SetEase(Ease.InBack)
                    .OnComplete(() => sprite.gameObject.SetActive(false)));

                sequence.AppendInterval(0.3f); // Short delay before next
            }
        }

        // After all sprites shown and hidden
        sequence.OnComplete(() =>
        {
            characterAnim.Play("IdleStandDown");

            doctorObjects.transform.DOMoveX(doctorObjects.transform.position.x + 15f, 1f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    doctorObjects.SetActive(false);

                    // Camera move + zoom
                    Vector3 targetFocusPosition = new Vector3(-3f, -0.5f, Camera.main.transform.position.z);
                    float targetZoom = 3f;
                    float zoomDuration = 2f;

                    Sequence camSequence = DOTween.Sequence();
                    camSequence.Append(Camera.main.transform.DOMove(targetFocusPosition, zoomDuration).SetEase(Ease.InOutSine));
                    camSequence.Join(Camera.main.DOOrthoSize(targetZoom, zoomDuration).SetEase(Ease.InOutSine));
                    camSequence.OnComplete(() =>
                    {
                        Debug.Log("Zoom and move complete!");
                        isStoryComplete = true;
                        StartStep1();
                    });
                });
        });
    }


    void Awake()
    {
        Invoke(nameof(ZoomAndCameraForDevices), 0.01f);
    }

    public void ZoomAndCameraForDevices()
    {


        if (isIphone)
        {
            zoomValue = 1f;
            cameraPos = new Vector3(-4.05999994f, 0.610000014f, -10);
            cameraPosForLens = new Vector3(-3.92000008f, 0.50999999f, -10);
            cameraPosForCutting = new Vector3(-0.0199999996f, 0.219999999f, -10);
            zoomValueForLens = 2.4f;
            zoomValueForCutting = 5f;
        }
        else if (isIpad)
        {
            zoomValue = 0.8f;
            zoomValueForLens = 2.5f;
            cameraPosForLens = new Vector3(-2.86f, 0.519990027f, -10);
            cameraPos = new Vector3(-2.8499999f, 0.519989967f, -10);
            cameraPosForCutting = new Vector3(-0.0199999996f, 0.219999999f, -10);

            zoomValueForCutting = 5f;
        }
    }

    float zoomValue;
    Vector3 cameraPos;
    float zoomValueForLens;
    float zoomValueForCutting;
    Vector3 cameraPosForLens;
    Vector3 cameraPosForCutting;



    #region Step 1 - Kill Germs
    void StartStep1()
    {
        print("Started");
        characterAnim.Play("IdleStandDown");
        step1Container.gameObject.SetActive(true);
        redEyesLeft.gameObject.SetActive(true);
        monsterGO.gameObject.SetActive(true);
        // Show germ killer device sliding down
        germKillerDevice.position += Vector3.up * 10f;
        SoundManager.instance.PlayObjectPlaced();

        SoundManager.instance.PlayEyeInfection();

        germKillerDevice.DOMoveY(germKillerDevice.position.y - 10f, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete((TweenCallback)(() => {
                print("YUSSSS");
                DOVirtual.DelayedCall(2.8f, () =>
                {
                    //Vector3 targetFocusPosition = new Vector3(-.85f, 0.51999f, Camera.main.transform.position.z);
                    Vector3 targetFocusPosition = cameraPos;

                    float targetZoom = 0.8f;
                    float zoomDuration = 2f;

                    eyeMask.transform.position = eyeMaskPos.position;
                    eyeMask.transform.localScale = new Vector3(0.16824691f, 0.178325117f, 0.699999988f);

                    Sequence camSequence = DOTween.Sequence();
                    camSequence.Append(Camera.main.transform.DOMove(targetFocusPosition, zoomDuration).SetEase(Ease.InOutSine));
                    camSequence.Join(Camera.main.DOOrthoSize(zoomValue, zoomDuration).SetEase(Ease.InOutSine));
                    camSequence.OnComplete((TweenCallback)(() => {
                        Debug.Log("Step 1 Zoom complete!");

                        germKillerDevice.gameObject.SetActive(false);
                        var sr = eyeMask.GetComponent<SpriteRenderer>();
                        Color color = sr.color;
                        color.a = 0f;
                        sr.color = color; // Set initial alpha to 0
                        eyeMask.gameObject.SetActive(true); // Keep it active in case it was disabled
                        sr.DOFade(1f, 0.3f); // Smooth fade to full opacity
                        foreach (var germ in this.monsterTargets)
                        {
                            germ.gameObject.SetActive(true);
                            germ.localScale = Vector3.zero;
                            ShortcutExtensions.DOScale(germ, 1f, 0.5f).SetEase(Ease.OutBack);
                            print("Germ scale set to 1");
                        }
                        EnableGun();
                    }));
                });

                
            }));

        // ❌ REMOVE this part:
        // foreach (var germ in eyeGerms)
        // {
        //     germ.gameObject.SetActive(true);
        //     germ.localScale = Vector3.zero;
        //     germ.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        // }

    }

    void EnableGun()
    {
        gunDevice.gameObject.SetActive(true);

        gunDevice.position += Vector3.right * 10f;
        SoundManager.instance.PlayGetRidOfTheBacteria();
        gunDevice.DOMoveX(gunDevice.position.x - 10f, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                StartCoroutine(MonitorGermKilling()); // 👈 This is what kicks off the detection loop
            });

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
    public class ShakeTracker : MonoBehaviour { }

    IEnumerator MonitorGermKilling()
    {
        float[] holdTimers = new float[monsterTargets.Length]; // Track time for each monster
        bool[] monsterKilled = new bool[monsterTargets.Length]; // Track if a monster is already killed
                                                                //isDragging = true;
        while (true)
        {
            isDragging1 = gunDevice.GetComponent<DraggableObject>().isDragging;
            gunDevice.GetComponent<DraggableObject>().OnDragStartEvent += OnGunDragStart;
            gunDevice.GetComponent<DraggableObject>().OnDragEndEvent += OnGunDragEnd;
            //gunDevice.GetComponent<DraggableObject>().OnDragEvent += OnGlassCutting;

            if (isDragging1)
            {
                // Enable the greenArea when dragging starts

                bool allMonstersKilled = true; // Assume all monsters are killed

                for (int i = 0; i < monsterTargets.Length; i++)
                {
                    if (!monsterTargets[i].gameObject.activeSelf || monsterKilled[i]) continue; // Skip inactive or already killed monsters

                    float distance = Vector3.Distance(greenArea.position, monsterTargets[i].position);

                    // If the gun is within the shoot distance of the monster
                    if (distance < shootDistance)
                    {
                        holdTimers[i] += Time.deltaTime;

                        // Start shaking if not already
                        if (!monsterTargets[i].GetComponent<ShakeTracker>())
                        {
                            monsterTargets[i].gameObject.AddComponent<ShakeTracker>();
                            monsterTargets[i]
      .DOShakeScale(
          duration: 0.5f,
          strength: 0.25f,    // ← Higher = stronger shake (default is 1)
          vibrato: 20,        // ← More shake "jumps" per duration
          randomness: 90,     // ← Angle variation
          fadeOut: true       // ← Smoothly taper the shake
      )
      .SetLoops(-1, LoopType.Restart)
      .SetId("shake" + i);
                        }

                        if (holdTimers[i] >= 2f)
                        {
                            monsterKilled[i] = true;

                            // Stop shake
                            DOTween.Kill("shake" + i);
                            Destroy(monsterTargets[i].GetComponent<ShakeTracker>());

                            Transform monster = monsterTargets[i];
                            monster
                                .DOScale(Vector3.zero, 0.5f)
                                .SetEase(Ease.InBack)
                                .OnComplete(() =>
                                {
                                    monster.gameObject.SetActive(false);
                                });
                        }
                    }
                    else
                    {
                        holdTimers[i] = 0f;

                        DOTween.Kill("shake" + i);
                        Destroy(monsterTargets[i].GetComponent<ShakeTracker>());
                    }


                    // If the monster is still active, we aren't done yet
                    if (monsterTargets[i].gameObject.activeSelf)
                    {
                        allMonstersKilled = false;
                    }
                }

                // Check if all monsters are killed (all are inactive or flagged as killed)
                if (allMonstersKilled)
                {
                    // All monsters are killed, proceed to Step 7
                    FinishStep1();  // Trigger Step 7
                    yield break;   // Exit the coroutine as Step 7 is starting
                }
            }
            else
            {
                // Disable the greenArea when dragging stops
                greenArea.gameObject.SetActive(false);
                for (int i = 0; i < monsterTargets.Length; i++)
                {
                    if (monsterTargets[i].gameObject.activeSelf)
                    {
                        // Reset the timer when the gun is not dragging
                        holdTimers[i] = 0f;
                        DOTween.Kill("shake" + i);
                        Destroy(monsterTargets[i].GetComponent<ShakeTracker>());
                    }
                }
            }

            yield return null;
        }
    }


    void FinishStep1()
    {
        var sr = eyeMask.GetComponent<SpriteRenderer>();

        // Step 1: Set initial alpha and enable eye mask
        Color color = sr.color;
        color.a = 1f; // It's already visible before
        sr.color = color;
        eyeMask.gameObject.SetActive(true);

        // Step 2: Move gun upward
        gunDevice.DOMoveY(germKillerDevice.position.y + 10f, 1f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                // Step 3: Fade out eyes + mask together
                float fadeDuration = 0.3f;

                redEyesLeft.GetComponent<SpriteRenderer>().DOFade(0f, fadeDuration);
                redEyesRight.GetComponent<SpriteRenderer>().DOFade(0f, fadeDuration);
                sr.DOFade(0f, fadeDuration);

                print("YUSSSS");

                // Step 4: Set mask transform immediately
                eyeMask.transform.position = eyeMaskPos.position;
                eyeMask.transform.localScale = new Vector3(0.16824691f, 0.178325117f, 0.699999988f);

                // Step 5: Wait for fade, then zoom
                DOVirtual.DelayedCall(fadeDuration, () =>
                {
                    Vector3 targetFocusPosition = cameraPosForLens;
                    float targetZoom = 4f;
                    float zoomDuration = 2f;

                    Sequence camSequence = DOTween.Sequence();
                    camSequence.Append(Camera.main.transform.DOMove(targetFocusPosition, zoomDuration).SetEase(Ease.InOutSine));
                    camSequence.Join(Camera.main.DOOrthoSize(zoomValueForLens, zoomDuration).SetEase(Ease.InOutSine));

                    camSequence.OnComplete(() =>
                    {
                        Debug.Log("Camera zoom complete!");
                        step1Container.gameObject.SetActive(false); 
                        InitializeEyeTest(); // Uncomment when ready
                    });
                });
            });
    }


    #endregion
    #region Step 2 - Eye Examination
    void InitializeEyeTest()
    {
        // Set initial states
        SetupBlurEffect();
        //
        //
        //
        //
        AnimateEyeTestEntrance();
        step2Container.gameObject.SetActive(true);
        ApplyBlurEffect(maxBlurAmount); // Start with maximum blur
        greenButton.gameObject.SetActive(false);
        glassesMenuContainer.gameObject.SetActive(false);
        SoundManager.instance.PlayCheckPatSight();

        // Setup lens draggable behavior
        foreach (Transform lens in lenses)
        {
            DraggableObject draggable = lens.GetComponent<DraggableObject>();
            if (draggable == null)
            {
                draggable = lens.gameObject.AddComponent<DraggableObject>();
            }

            // Store original position for returning lenses
            draggable.originalPosition = lens.position;

            // Subscribe to events
            draggable.OnDragEvent += OnLensDrag;
            draggable.OnDragEndEvent += OnLensDragEnd;
            draggable.OnDragStartEvent += OnLensDragStart;
        }
    }

    void OnLensDragStart(Transform draggedObject, Quaternion rotation)
    {
       SoundManager.instance.PlayClick();

    }
    private void AnimateEyeTestEntrance()
    {

        {
            // Cache target positions
            Vector3 deviceTargetPos = eyeTestDevice.position;
            Vector3 boardTargetPos = lensBoard.position;
            Vector3 chartTargetPos = eyeChart.position;
            Vector3[] lensTargetPositions = lenses.Select(l => l.position).ToArray();

            // Move off-screen
            eyeTestDevice.position = deviceTargetPos + Vector3.left * 15f;
            lensBoard.position = boardTargetPos + Vector3.right * 15f;
            eyeChart.position = chartTargetPos + Vector3.left * 15f;

            for (int i = 0; i < lenses.Length; i++)
                lenses[i].position = lensTargetPositions[i] + Vector3.right * 15f;

            // Animate to screen
            float duration = 1.2f;
            Ease easing = Ease.InOutQuad;

            eyeTestDevice.DOMove(deviceTargetPos, duration).SetEase(easing);
            lensBoard.DOMove(boardTargetPos, duration).SetEase(easing);
            eyeChart.DOMove(chartTargetPos, duration).SetEase(easing);
            SoundManager.instance.PlayObjectPlaced();
            for (int i = 0; i < lenses.Length; i++)
                lenses[i].DOMove(lensTargetPositions[i], duration).SetEase(easing);
        }


    }

    void SetupBlurEffect()
    {
        originalLetters = new Transform[letterGrid.transform.childCount];
        originalLetterScales = new Vector3[letterGrid.transform.childCount];
        originalLetterPositions = new Vector3[letterGrid.transform.childCount];

        for (int i = 0; i < letterGrid.transform.childCount; i++)
        {
            Transform letter = letterGrid.transform.GetChild(i);
            originalLetters[i] = letter;
            originalLetterScales[i] = letter.localScale;
            originalLetterPositions[i] = letter.localPosition;
        }

        letterCopies = new GameObject[originalLetters.Length * blurCopies];

        for (int i = 0; i < originalLetters.Length; i++)
        {
            Transform original = originalLetters[i];

            for (int j = 0; j < blurCopies; j++)
            {
                GameObject copy = Instantiate(original.gameObject, letterGrid.transform);
                copy.name = original.name + "_blur_" + j;

                SpriteRenderer renderer = copy.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    Color color = renderer.color;
                    color.a = 0.3f / blurCopies;
                    renderer.color = color;
                    renderer.sortingOrder = renderer.sortingOrder - 1;
                }

                letterCopies[i * blurCopies + j] = copy;
                copy.SetActive(false);
            }
        }
    }

    void ApplyBlurEffect(float amount)
    {
        if (blurOverlaySpriteRenderer == null) return;
        float normalized = Mathf.Clamp01(amount / maxBlurAmount);
        Color color = blurOverlaySpriteRenderer.color;
        color.a = normalized;
        blurOverlaySpriteRenderer.color = color;
    }

    void OnLensDrag(Transform lens, Vector3 position)
    {
        currentLens = lens;
        Transform targetEyePosition = isTestingLeftEye ? leftEyePosition : rightEyePosition;

        if (Vector3.Distance(position, targetEyePosition.position) < 5f)
        {
            // close to eye, do nothing yet
        }
        else
        {
            if (isLensPlaced)
            {
                ApplyBlurEffect(maxBlurAmount);
                isLensPlaced = false;
            }
        }
    }

    void OnLensDragEnd(Transform lens, Vector3 position)
    {
        Transform targetEyePosition = isTestingLeftEye ? leftEyePosition : rightEyePosition;
        DraggableObject draggable = lens.GetComponent<DraggableObject>();
        float distanceToEye = Vector3.Distance(position, targetEyePosition.position);
        float distanceToBoard = Vector3.Distance(position, draggable.originalPosition);

        // Add more detailed logging to investigate
        Debug.Log($"[{lens.name}] Drag end at: {position}");
        Debug.Log($"Eye position: {targetEyePosition.position}, distance: {distanceToEye}");
        Debug.Log($"Board position: {draggable.originalPosition}, distance: {distanceToBoard}");

        // Check if this lens is currently placed
        bool isThisLensPlaced = (currentLeftLens == lens || currentRightLens == lens);
        Debug.Log($"Is this lens currently placed? {isThisLensPlaced}");

        // Priority: If lens is already placed, allow returning to board
        if (isThisLensPlaced && distanceToBoard < 5f)
        {
            Debug.Log("Currently placed lens being returned to board.");
            // Reset state
            ApplyBlurEffect(maxBlurAmount);
            isLensPlaced = false;
            currentLens = null;
            if (isTestingLeftEye)
                isLeftEyeComplete = false;
            else
                isRightEyeComplete = false;

            ReturnLensToBoardSimple(lens, true);
            isThisLensPlaced = false;
            return;
        }

        // If not already placed lens, handle normal dropping logic
        // 1. Check for eye placement
        if (distanceToEye < distanceToBoard && distanceToEye < 5f)
        {
            Debug.Log("Lens dropped near eye. Snapping to eye.");
            lens.DOKill();
            lens.DOMove(targetEyePosition.position, 0.2f).SetEase(Ease.OutQuad);
            lens.DOScale(new Vector3(0.7665959f, 0.7665959f, 0.402704358f), 0.5f).SetEase(Ease.OutBack);
            CheckLensCorrectness(lens);
            return;
        }

        // 2. Check for board placement
        if (distanceToBoard < 5f)
        {
            Debug.Log("Lens dropped near original board position. Returning to start.");
            ReturnLensToBoardSimple(lens);
            isLensPlaced = false;
            return;
        }

        // 3. Invalid drop: return to board
        Debug.Log("Lens dropped in invalid area. Returning to board.");
        // Reset state first if this lens was previously placed
        if (isThisLensPlaced)
        {
            ApplyBlurEffect(maxBlurAmount);
            isLensPlaced = false;
            currentLens = null;
            if (isTestingLeftEye)
                isLeftEyeComplete = false;
            else
                isRightEyeComplete = false;
        }
        ReturnLensToBoardSimple(lens);
    }

    void ClearPlacementStateIfNeeded(Transform lens)
    {
        if (currentLeftLens == lens)
        {
            currentLeftLens = null;
            isLeftEyeComplete = false;
        }
        if (currentRightLens == lens)
        {
            currentRightLens = null;
            isRightEyeComplete = false;
        }

        if (currentLens == lens)
        {
            currentLens = null;
            isLensPlaced = false;
            ApplyBlurEffect(maxBlurAmount);
        }
    }


    void ReturnLensToBoardSimple(Transform lens, bool resetState = false)
    {
        if (resetState)
            ClearPlacementStateIfNeeded(lens);

        DraggableObject draggable = lens.GetComponent<DraggableObject>();
        if (draggable != null)
        {
            lens.DOMove(draggable.delayPosition, 0.5f).SetEase(Ease.OutBack);
            lens.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
    }
    void StartRightEyeTest()
    {
        isTestingLeftEye = false;  // Set flag for right eye test
                                   // Any additional setup for right eye test (e.g., lens selection)
    }

    void StartLeftEyeTest()
    {
        isTestingLeftEye = true;  // Set flag for left eye test
                                  // Any additional setup for left eye test (e.g., lens selection)
    }

    void CheckLensCorrectness(Transform lens)
    {
        int lensIndex = System.Array.IndexOf(lenses, lens);
        int correctLensIndex = isTestingLeftEye ? correctLeftLensIndex : correctRightLensIndex;

        if (lensIndex == correctLensIndex)
        {
            DOTween.To(
                () => spriteBlurMaterial.GetFloat("_BlurAmount"),
                x => spriteBlurMaterial.SetFloat("_BlurAmount", x),
                0f,
                0.5f
            ).SetEase(Ease.OutQuad);

            isLensPlaced = true;
            currentLens = lens;
            SoundManager.instance.PlayDone();
            if (isTestingLeftEye)
            {
                if (currentLeftLens != null && currentLeftLens != lens)
                    ReturnLensToBoardSimple(currentLeftLens);

                currentLeftLens = lens;
                isLeftEyeComplete = true;

                StartCoroutine(ReturnLensAfterDelay(lens, 2f));
                StartCoroutine(DelayBeforeNextEye());
            }
            else
            {
                if (currentRightLens != null && currentRightLens != lens)
                    ReturnLensToBoardSimple(currentRightLens);

                currentRightLens = lens;
                isRightEyeComplete = true;

                StartCoroutine(ReturnLensAfterDelay(lens, 2f));
                StartCoroutine(DelayBeforeGlassesSelection());
            }
        }

        else
        {
            // ❌ Wrong lens — blurAmount = 0.0034
            DOTween.To(
                () => spriteBlurMaterial.GetFloat("_BlurAmount"),
                x => spriteBlurMaterial.SetFloat("_BlurAmount", x),
                0.0034f,
                0.3f
            ).SetEase(Ease.OutQuad);

            // Play wrong answer feedback here if needed

            // Return the incorrect lens to the board after a short delay
            StartCoroutine(ReturnLensAfterDelay(lens, 1.5f));
        }
    }

    // This coroutine was likely already defined in your code
    // but I'm including it here to ensure it's available



    IEnumerator ReturnLensAfterDelay(Transform lens, float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearPlacementStateIfNeeded(lens);
        ReturnLensToBoardSimple(lens);
    }



    IEnumerator DelayBeforeNextEye()
    {
        yield return new WaitForSeconds(1.5f);

        isTestingLeftEye = false;

        Vector3 offset = rightEyePosition.position - leftEyePosition.position;
        eyeTestDevice.DOMove(eyeTestDevice.position + offset, 1f).SetEase(Ease.InOutQuad);
        DOTween.To(
               () => spriteBlurMaterial.GetFloat("_BlurAmount"),
               x => spriteBlurMaterial.SetFloat("_BlurAmount", x),
               0.0034f,
               0.3f
           ).SetEase(Ease.OutQuad);
        if (currentLens != null)
        {
            ReturnLensToBoardSimple(currentLens);
        }

        DOTween.To(() => 0f, x => ApplyBlurEffect(x), maxBlurAmount, 0.5f).SetEase(Ease.InQuad);
        isLensPlaced = false;
    }

    IEnumerator DelayBeforeGlassesSelection()
    {
        yield return new WaitForSeconds(2.5f);

        // Check if we should clear the currentRightLens
        if (currentRightLens != null)
        {
            ReturnLensToBoardSimple(currentRightLens);
            currentRightLens = null;
        }

        // Also clear the global currentLens
        if (currentLens != null)
        {
            currentLens = null;
        }

        isLensPlaced = false;

        eyeTestDevice.DOMove(eyeTestDevice.position + Vector3.left * 15f, 1f).SetEase(Ease.InOutQuad);
        lensBoard.DOMove(lensBoard.position + Vector3.right * 15f, 1f).SetEase(Ease.InOutQuad);
        foreach (Transform lens in lenses)
        {
            lens.DOMove(lens.position + Vector3.right * 15f, 1f).SetEase(Ease.InOutQuad);
        }
        eyeChart.DOMove(eyeChart.position + Vector3.left * 15f, 1f).SetEase(Ease.InOutQuad)
        .OnComplete(() => {
            ShowGlassesSelection();
        });
    }


    void ShowGlassesSelection()
    {
        // Animate the glasses menu container
        RectTransform rt = glassesMenuContainer.GetComponent<RectTransform>();
        glassesMenuContainer.gameObject.SetActive(true);
        // doneButton.gameObject.SetActive(true);

        // Start far off to the right
        Vector2 offScreenRight = new Vector2(1000f, rt.anchoredPosition.y);
        rt.anchoredPosition = offScreenRight;

        // End slightly more to the left
        Vector2 onScreenTarget = new Vector2(-400f, rt.anchoredPosition.y);

        // Same duration for both animations
        float animationDuration = 1f;
        SoundManager.instance.PlayChooseFrame();

        rt.DOAnchorPos(onScreenTarget, animationDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                SetupGlassesFrames();
            });

        // Animate the done button
        RectTransform donebtn = doneButton.GetComponent<RectTransform>();
        Vector3 targetPos = donebtn.position;
        Vector3 offScreenStartPos = targetPos + Vector3.left * 500f; // Start far to the left

        donebtn.position = offScreenStartPos; // Set it off-screen to the left

        donebtn.DOMove(targetPos, animationDuration).SetEase(Ease.OutBack); // Same easing and duration

    }

    void SetupGlassesFrames()
    {
        foreach (Transform glasses in glassesFrames)
        {
            DraggableObject draggable = glasses.GetComponent<DraggableObject>();
            if (draggable == null)
            {
                draggable = glasses.gameObject.AddComponent<DraggableObject>();
            }

            // Store original position for returning glasses
            draggable.originalPosition = glasses.position;

            // Subscribe to events
            draggable.OnDragEvent += OnGlassesDrag;
            draggable.OnDragEndEvent += OnGlassesDragEnd;
        }
    }

    void OnGlassesDrag(Transform glasses, Vector3 position)
    {
        currentGlasses = glasses;
    }

    public void SelectGlasses(int index) // called on the glasses
    {
        for (int i = 0; i < glassesFrames.Length; i++)
        {
            glassesFrames[i].gameObject.SetActive(i == index);
            glassesFrames[i].SetParent(paitentHead.transform);
        }
        SoundManager.instance.PlayClick();
        
        glassesIndex = index;
        print("Selected Glasses  : " + glassesIndex);
        doneButton.gameObject.SetActive(true);
    }
    public void frameDonebtn()
    {


        float animationDuration = 1f;
        SoundManager.instance.PlayDone();

        // Animate glasses menu container to the right and hide
        RectTransform rt = glassesMenuContainer.GetComponent<RectTransform>();
        Vector2 offScreenRight = new Vector2(1000f, rt.anchoredPosition.y); // Exit to the right

        rt.DOAnchorPos(offScreenRight, animationDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                glassesMenuContainer.gameObject.SetActive(false); // Hide after anim
            });

        // Animate done button to the left and hide
        RectTransform donebtn = doneButton.GetComponent<RectTransform>();
        Vector3 offScreenLeft = donebtn.position + Vector3.left * 500f; // Move left off-screen

        donebtn.DOMove(offScreenLeft, animationDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                doneButton.gameObject.SetActive(false); // Hide after anim
            });


    }

    void OnGlassesDragEnd(Transform glasses, Vector3 position)
    {
        // If glasses are close to cat's face, snap to position, otherwise return to menu
        if (Vector3.Distance(position, catFace.position) < 2f)
        {
            // Snap glasses to cat's face
            glasses.DOMove(catFace.position, 0.2f).SetEase(Ease.OutQuad);

            // Make cat happy
            catMouthRenderer.sprite = happyMouthSprite;

            // Show green button
            if (!greenButton.gameObject.activeSelf)
            {
                greenButton.gameObject.SetActive(true);

                // Animate button appearing
                CanvasGroup buttonGroup = greenButton.GetComponent<CanvasGroup>();
                if (buttonGroup == null)
                {
                    buttonGroup = greenButton.gameObject.AddComponent<CanvasGroup>();
                }

                buttonGroup.alpha = 0f;
                buttonGroup.DOFade(1f, 0.5f);

                // Setup button click
                greenButton.onClick.RemoveAllListeners();
                greenButton.onClick.AddListener(CompleteEyeTest);
            }
        }
        else
        {
            // Return glasses to menu
            ReturnGlassesToMenu(glasses);
        }
    }

    void ReturnGlassesToMenu(Transform glasses)
    {
        DraggableObject draggable = glasses.GetComponent<DraggableObject>();
        glasses.DOMove(draggable.originalPosition, 0.5f).SetEase(Ease.OutBack);
    }


        void CompleteEyeTest()
    {
        // Animate button up
        greenButton.transform.DOMove(greenButton.transform.position + Vector3.up * 5f, 0.5f)
            .SetEase(Ease.InBack);
        print("Hola");
        // Slide glasses menu to the right
        glassesMenuContainer.DOMove(glassesMenuContainer.position + Vector3.right * 15f, 1f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {

               
                // Complete the game or move to next stage
                StartStep3();
                Debug.Log("Eye Test Complete!");
            });
        print("Hola22");

    }
    public void glassesDonebtn()
    {

        DOVirtual.DelayedCall(2f, () =>
        {
            print("OhYeaeYUSSSS");


            //Vector3 targetFocusPosition = new Vector3(-.85f, 0.51999f, Camera.main.transform.position.z);
            Vector3 targetFocusPosition = cameraPosForCutting;

            float targetZoom = 0.8f;
            float zoomDuration = 2f;

            eyeMask.transform.position = eyeMaskPos.position;
            eyeMask.transform.localScale = new Vector3(0.16824691f, 0.178325117f, 0.699999988f);

            Sequence camSequence = DOTween.Sequence();
            camSequence.Append(Camera.main.transform.DOMove(targetFocusPosition, zoomDuration).SetEase(Ease.InOutSine));
            camSequence.Join(Camera.main.DOOrthoSize(zoomValueForCutting, zoomDuration).SetEase(Ease.InOutSine));
            camSequence.OnComplete((TweenCallback)(() =>
            {
                Debug.Log("Step 1 Zoom complete!");

                germKillerDevice.gameObject.SetActive(false);
                var sr = eyeMask.GetComponent<SpriteRenderer>();
                Color color = sr.color;
                color.a = 0f;
                sr.color = color; // Set initial alpha to 0
                eyeMask.gameObject.SetActive(true); // Keep it active in case it was disabled
                sr.DOFade(1f, 0.3f); // Smooth fade to full opacity
                foreach (var germ in this.monsterTargets)
                {
                    germ.gameObject.SetActive(true);
                    germ.localScale = Vector3.zero;
                    ShortcutExtensions.DOScale(germ, 1f, 0.5f).SetEase(Ease.OutBack);
                    print("Germ scale set to 1");
                }
                //EnableGun();
            }));
            DOVirtual.DelayedCall(2f, () => { 
             StartStep3();
            });
        });
    }

    void OnDestroy()
    {
        //// Clean up the copies
        //foreach (GameObject copy in letterCopies)
        //{
        //    if (copy != null)
        //    {
        //        Destroy(copy);
        //    }
        //}
    }


    #endregion

    #region Step 3 - Create Glasses

    public GameObject[] glassGO;
    void StartStep3()
    {
        SoundManager.instance.StopGetRidOfTheBacteria();
        step3Container.gameObject.SetActive(true);

        // Setup glass cutting
        glassPiece.gameObject.SetActive(true);

        // Store original positions
        Vector3 originalCutterPosition = glassCutter.position;
        Vector3 originalPathPosition = cuttingPath.position;

        // Move objects off-screen to the left
        glassCutter.position = new Vector3(originalCutterPosition.x - 15f, originalCutterPosition.y, originalCutterPosition.z);
        cuttingPath.position = new Vector3(originalPathPosition.x - 15f, originalPathPosition.y, originalPathPosition.z);

        // Activate the objects
        cuttingPath.gameObject.SetActive(true);
        glassCutter.gameObject.SetActive(true);

        // Animate them coming in from the left
        cuttingPath.DOMove(originalPathPosition, 1f).SetEase(Ease.OutBack);
        glassCutter.DOMove(originalCutterPosition, 1f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            glassGO[glassesIndex].SetActive(true);

        });
        SoundManager.instance.PlayCuttLens();
       

        // Setup drag events
        //   glassCutter.GetComponent<DraggableObject>().OnDragEvent += OnGlassCutting;
        glassCutter.GetComponent<DraggableObject>().OnDragEndEvent += OnGlassCutComplete;
        glassCutter.GetComponent<SpriteRenderer>().sortingOrder = 1200;
    }

    //void OnGlassCutting(Transform cutter, Vector3 position)
    //{
    //    // Check if cutter is near the glass
    //    float distanceToGlass = Vector3.Distance(cutter.position, glassPiece.position);

    //    // Define a threshold distance to trigger the animation
    //    float activationDistance = 2.0f; // Adjust this value as needed

    //    if (distanceToGlass < activationDistance && !isCutting)
    //    {
    //        // Start cutting and play animation
    //        isCutting = true;
    //        PlayGlassCuttingAnimation(glassesSelected);
    //    }
    //}

    void OnGlassCutComplete(Transform cutter, Vector3 position)
    {
        glassCutter.GetComponent<SpriteRenderer>().sortingOrder = 1200;
        if (Vector3.Distance(position, cutterPos.position) < 5f)
        {
            glassCutterAnimator.enabled = true;
            SoundManager.instance.PlayCutting();
            DOVirtual.DelayedCall(1.5f, () =>
            {
            SoundManager.instance.PlayCutting();

            });
            DOVirtual.DelayedCall(3.5f, () =>
            {
                SoundManager.instance.PlayCutting();
                
            });
            switch (glassesIndex)
            {
                case 0:
                    glassCutterAnimator.Play("GlassShape8");

                    break;
                case 1:
                    // Play animation for the second type of glasses
                    glassCutterAnimator.Play("GlassShape2");
                    break;
                case 2:
                    glassCutterAnimator.Play("GlassShape3");

                    // Play animation for the third type of glasses
                    break;
                case 3:
                    glassCutterAnimator.Play("GlassShape4");

                    // Play animation for the fourth type of glasses
                    break;
                case 4:
                    glassCutterAnimator.Play("GlassShape5");

                    // Play animation for the fifth type of glasses
                    break;
                case 5:
                    glassCutterAnimator.Play("GlassShape7");

                    print("Playeed");
                    // Play animation for the sixth type of glasses
                    break;
                case 6:
                    glassCutterAnimator.Play("GlassShape8");

                    // Play animation for the seventh type of glasses
                    break;
                case 7:
                    glassCutterAnimator.Play("GlassShape6");

                    // Play animation for the eighth type of glasses
                    break;
                case 8:
                    glassCutterAnimator.Play("GlassShape8");

                    // Play animation for the eighth type of glasses
                    break;


            }


        }
    }

    void SetupNextGlassPiece()
    {
        // Reset for next glass piece
        glassPiece.DOScale(1f, 0.5f).From(0f);
        glassesSelected++;

        // Move cutter back to start position
        glassCutter.DOMove(cuttingPath.GetChild(0).position, 0.5f);
    }

    //void PlayGlassCuttingAnimation(int index)
    //{
    //    // Get animator and play animation based on index
    //    Animator glassAnimator = glassPiece.GetComponent<Animator>();

    //    switch (index)
    //    {
    //        case 0:
    //            glassAnimator.Play("GlassCutting_Type1");
    //            break;
    //        case 1:
    //            glassAnimator.Play("GlassCutting_Type2");
    //            break;
    //        case 2:
    //            glassAnimator.Play("GlassCutting_Type3");
    //            break;
    //        default:
    //            glassAnimator.Play("GlassCutting_Default");
    //            break;
    //    }
    //}
    public void FinishStep3()
    {
        glassesFrames[glassesIndex].gameObject.SetActive(false);
        glassesInPatient[glassesIndex].SetActive(true);
        glassCutterAnimator.enabled = false;

        Vector3 exitOffset = Vector3.left * 15f;
        Vector3 targetPosition = step3Container.position + exitOffset;

        // Move the whole container off-screen
        step3Container.DOMove(targetPosition, 1f).SetEase(Ease.InBack).OnComplete(() =>
        {
            // Deactivate after animation
            step3Container.gameObject.SetActive(false);
            glassGO[glassesIndex].SetActive(false);
        });
        happyFace.SetActive(true);
        patientAnimator.transform.position = new Vector3(patientAnimator.transform.position.x, patientAnimator.transform.position.y - 1, patientAnimator.transform.position.z);
        doctorObjects.SetActive(true);
        doctorObjects.transform.DOMoveX(doctorObjects.transform.position.x - 15f, 1f)
      .SetEase(Ease.InBack);
        SoundManager.instance.PlayDone();
        SoundManager.instance.PlayPatCanSeeBetter();


        patientAnimator.Play("Happy");
        CompleteLevel();
    }

    public void CompleteLevel() 
    
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

            SceneManager.LoadScene(0);
        });

    }
#endregion
}

