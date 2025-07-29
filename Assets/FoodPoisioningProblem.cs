using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using static EyeProblemManager;

public class InfusionController : MonoBehaviour
{
    [Header("Doctor Objects")]
    public GameObject doctorObjects;

    [Header("Character Objects")]
    public GameObject characterObj;
    public Transform[] problemSprites;
    public Animator characterAnim;
    public GameObject story;
    public GameObject characterChair;
    public GameObject characterBottom;
    public Transform sittingPosForPatient;
    public Transform WalkingStartPosForPatient;


    // Step 2 - Infusion Variables
    [Header("Step 1 - Infusion")]
    [SerializeField] private Transform step1Container;
    [SerializeField] private Transform injection;
    [SerializeField] private Transform injectionTargetMarker;
    [SerializeField] private Transform infusionLiquid;
    //[SerializeField] private ParticleSystem infusionDripParticles;
    [SerializeField] private float injectionTargetDistance = 1.5f;
    [SerializeField] private float infusionDuration = 4f;
    private bool injectionPlaced = false;
    public GameObject DoctorCabin, Mouth, DocObjects;
    public Transform dirpBag, paitentPos;

    // Line Renderer Variables
    [Header("Step 1.2 - Line Renderer")]
    [SerializeField] private LineRenderer infusionLineRenderer;
    [SerializeField] private Transform lineStartPoint; // Point on the infusion bag where line starts
    [SerializeField] private Transform lineEndPoint;   // Point on the injection needle where line ends
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color lineColor = Color.blue;
    [SerializeField] private bool useCurvedLine = true;
    //[SerializeField] private int curveSegments = 50; // Smooth line    // Number of segments for curved line
    [SerializeField] private float curveDrop = 0.8f;   // How much the tube droops down

    [Header("Step2 - Stethoscope")]
    [SerializeField] private Transform step2Container;
    [SerializeField] private Transform stethoscope;
    [SerializeField] private Transform stethoscopeEndpoint;
    [SerializeField] private Transform[] targetMarkers; // Targets to be touched in sequence
    [SerializeField] private GameObject progressBar;
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

    [Header("Step 3 - Give the Pill")]
    public Transform step3Gameobject;
    public Transform pillTray;
    public Transform pill;
    public Transform waterGlassTray;
    public Transform waterGlass;
    public Transform emptyGlass;
    public Transform catMouth;
    public Transform catMouthOpen;
    public Transform catDrinkingWater;
    //public GameObject injectionPos;
    private bool forceFreezeInjection = false;
    private Vector3 injectionLockedPosition = new Vector3(-0.239999995f, -0.540000021f, 0);
    
    [Header("Step 4- Kill Bacteria")]
    // Device and Monster Setup
    [SerializeField] private GameObject step4Gameobject;            // The gun object that will slide in and be dragged
    [SerializeField] private Transform gunDevice;            // The gun object that will slide in and be dragged
    [SerializeField] private Transform greenArea;            // The shooting/green zone attached to the gun
    [SerializeField] private Transform greenAreaPoint;            // The shooting/green zone attached to the gun
    [SerializeField] private Transform[] monsterTargets;     // Array of monsters to "shoot" at
    [SerializeField] private float shootDistance = 1.6f;       // Max distance for a successful "hit" with the green area
    private bool isDragging1 = false;        // Whether the gun is currently being dragged
    [SerializeField] private float holdDurationPerTarget = 2f;

    // State Tracking
    [Header("Step 5 - Blanket")]
    [SerializeField] private GameObject step5Gameobject;            // The gun object that will slide in and be dragged
    [SerializeField] private Transform blanket;
    [SerializeField] private Transform blanketTargetPosition;
    [SerializeField] private float blanketSnapDistance = 8f;
    [SerializeField] private GameObject sadFace;
    [SerializeField] private GameObject happyFace;
    [SerializeField] private GameObject purpleHeadEffect;

    Vector3 characterInitialPos;
    Vector3 characterInitialScale;
    Vector3 chairInitialPos;
    Vector3 chairInitialScale;

    public Animator fadeInAnim;

    #region Step 0 - Start Story
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
    void Start()
    {
        CheckDeviceResolution();
        characterAnim.keepAnimatorStateOnDisable = true;
        // Initialize everything
        // if (step2Container) step2Container.gameObject.SetActive(false);
        // 
        // Start Step 2 immediately for testing
        //StartStep2();

        // PatChair.transform.position = new Vector3(-0.779999971f, -3.1099999f, -1.63185811f);
        //step3Container.gameObject.SetActive(true);
        // Start with the chest piece hidden
        CheckDeviceResolution();
        ResetProgressBar();
        StartCoroutine(HandleCatEntryThenShowProblem());

        //StartCoroutine(FadeIntoStep6Routine());
    }

    void Update()
    {
        if (forceFreezeInjection && injection != null)
        {
            injection.position = Vector3.MoveTowards(
                injection.position,
                injectionLockedPosition,
                5f * Time.deltaTime
            );
            // Check if it's close enough, then snap and stop moving
            if (Vector3.Distance(injection.position, injectionLockedPosition) < 0.001f)
            {
                injection.position = injectionLockedPosition;
                forceFreezeInjection = false; // stop updating after locking
            }
        }

        if (forceFreezeInjection && injection != null)
        {
            // Move the injection towards the locked position
            injection.position = Vector3.MoveTowards(injection.position, injectionLockedPosition, 5f * Time.deltaTime); // Adjust speed as needed

            // Lock position when reached
            if (injection.position == injectionLockedPosition)
            {
                forceFreezeInjection = false; // Stop freezing once locked
            }
        }

        // Update the line renderer to follow the injection and keep the end point at the 'endPoint'
        if (infusionLineRenderer != null && lineStartPoint != null && lineEndPoint != null)
        {
            if (useCurvedLine)
            {
                UpdateCurvedLinePositions();  // Use this for curved line update
            }
            else
            {
                UpdateLinePositions();  // Use this for straight line update
            }
        }

        if (stethoscopeLineRenderer != null && stethoscopeLineStart != null && stethoscopeLineEnd != null)
        {
            UpdateStethoscopeCurvedLinePositions();
        }
        // Handle the click and start dragging only when the device is clicked
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
            float[] holdTimers = new float[monsterTargets.Length];

            // Iterate over all monsters to check if the gun is close enough
            for (int i = 0; i < monsterTargets.Length; i++)
            {
                // Skip inactive monsters
                if (!monsterTargets[i].gameObject.activeSelf) continue;

                // Calculate the distance between the gun and the monster
                float distance = Vector3.Distance(greenAreaPoint.position, monsterTargets[i].position);
                //Debug.Log($"Distance to {monsterTargets[i].name}: {distance}");

                // If within shoot distance, increment the timer
                if (distance < shootDistance)
                {
                    holdTimers[i] += Time.deltaTime; // Increment timer only when in range
                    //Debug.Log($"Holding {monsterTargets[i].name} for {holdTimers[i]:F2} seconds");

                    // If the timer reaches the set duration (2 seconds), kill the monster
                    if (holdTimers[i] >= holdDurationPerTarget)
                    {
                        //Debug.Log($"Monster killed: {monsterTargets[i].name}");

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

    [SerializeField] private ScreenFade screenFade; // Assign in Inspector
    IEnumerator HandleCatEntryThenShowProblem()
    {
        yield return new WaitForSeconds(0.1f);

        // Move cat to the left off-screen start position
        characterInitialPos = characterObj.transform.position;
        characterInitialScale = characterObj.transform.localScale;
        chairInitialPos = characterChair.transform.position;
        chairInitialScale = characterChair.transform.localScale;

        Vector3 originalPos = characterAnim.transform.position;
        Vector3 walkStartPos =WalkingStartPosForPatient.position;  // Walking height
        Vector3 sittingPos = sittingPosForPatient.position;        // Sitting height

        characterAnim.transform.position = walkStartPos;
        characterAnim.gameObject.SetActive(true);
        characterAnim.Play("Walking");

        // Move to chair (X movement only)
        characterAnim.transform.DOMoveX(originalPos.x, 3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // Drop down to sitting position
                characterAnim.transform.DOMoveY(sittingPosForPatient.position.y, 0.3f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        // Play IdleSit animation
                        characterAnim.Play("IdleStandDown");
                        //sadFace.gameObject.SetActive(true);

                        // After 1 second, play storytelling
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            characterAnim.Play("Storytelling");
                        });
                    });
            });

        // Wait for walk and sit to finish
        yield return new WaitForSeconds(3.8f);

        StartStory();
    }


    void StartStory()
    {
        SoundManager.instance.PlayBlabla();
        story.gameObject.SetActive(true);
        ActivateTarget(currentTarget);
        AnimateIssue();
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
                sequence.AppendInterval(1.0f); // Wait before showing next sprite
            }
        }
        sequence.AppendInterval(1f); // Delay of 1 second before next tween
        sequence.OnComplete(() =>
        {
            //DocObjects.SetActive(false);

            // Smoothly zoom the camera before starting the animation
            float targetSize = 4f; // Adjust this to the desired zoom level
            float zoomDuration = 2f; // Adjust duration for smooth effect

            GetComponent<Camera>().DOOrthoSize(targetSize, zoomDuration).SetEase(Ease.InOutSine);
            // isStep1Complete = true;
            StartCoroutine(FadeIntoStep1Routine());
            //StartCoroutine(FadeIntoStep4Routine());
            //StartStep7();
            //DoctorCabin.SetActive(false);



            //  DocObjects.SetActive(false);
            //DocChair.SetActive(false);
            foreach (var sprite in problemSprites)
            {
                sprite.gameObject.SetActive(false);
            }
        });
    }

    private IEnumerator FadeIntoStep1Routine()
    {
        // 🔲 Fade to black first
        Mouth.gameObject.SetActive(false);
        characterAnim.Play("IdleStandDown");

        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic
        characterChair.SetActive(false);
        DocObjects.SetActive(false);

        StartStep1();

        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
        //DeviceResForResolution devres = DeviceResForResolution.Instance;
        //if (devres.isIpad)
        {
            //devres.gameObject.transform.localScale = Vector3.one*1.3f;
            //devres.sizeForIpad = Vector2.one * 1.3f;
            //add -2 in y axis
            characterObj.transform.position = new Vector3(characterObj.transform.position.x, characterObj.transform.position.y - 2, characterObj.transform.position.z);
            //make local scale 0.4f
            characterObj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        }
    }
    #endregion
    private void ResetProgressBar()
    {
        progressFillAmount = 0f;
        progressBar.transform.localScale = new Vector3(1f, 0f, 1f);
    }


    #region Step 1 - Infusion
    public Transform dripBagPosIphone;
    public void StartStep1()
    {
        
        infusionLineRenderer.startWidth = lineWidth;
        infusionLineRenderer.endWidth = lineWidth;
        dirpBag.transform.position = dripBagPosIphone.position;
        // Store the original position of the drip bag
        if (isIphone)
        {
            dirpBag.transform.position = dripBagPosIphone.position;
        }

        Vector3 originalPosition = dirpBag.transform.position;
        // Move the drip bag off-screen above before enabling it
        dirpBag.transform.position = originalPosition + Vector3.up * 10f;

        // Activate step 2 container and other scene adjustments
        step1Container.gameObject.SetActive(true);
        injectionTargetMarker.transform.parent.gameObject.SetActive(true);
        injectionTargetMarker.gameObject.SetActive(false);

        DocObjects.SetActive(false);
        DoctorCabin.SetActive(false);
        characterObj.transform.position = paitentPos.position;
        SoundManager.instance.PlayPutDripInPat();   
        // Animate the drip bag sliding down into place
        DOVirtual.DelayedCall(1f, () =>
        {
            SoundManager.instance.PlayObjectPlaced();
            dirpBag.transform.DOMoveY(originalPosition.y, 1f)
             .SetEase(Ease.OutBack)
             .OnComplete(() =>
             {
                 // Once drip bag comes into place, handle rest of the injection logic
                 if (injection)
                 {
                     if (useCurvedLine)
                         SetupCurvedLineRenderer();
                     else
                         SetupLineRenderer();

                     injectionPlaced = false;

                     // 👇 Instead of sliding injection in, just ensure it's ready
                     if (injection.TryGetComponent(out DraggableObject draggable))
                         draggable.OnDragEvent += OnInjectionDrag;
                     draggable.OnDragStartEvent += OnInjectionStartDrag;

                     draggable.OnDragEndEvent += OnInjectionDragEnd;

                     StartCoroutine(PulseTargetMarker());
                 }
                 else
                 {
                     Debug.LogError("Injection transform not assigned!");
                 }
             });
        });
    }
    void OnInjectionStartDrag(Transform draggedObject, Quaternion rotation)
    {
        SoundManager.instance.PlayClick();

    }
    private IEnumerator PulseTargetMarker()
    {
        // Make sure target marker is active
        if (injectionTargetMarker)
        {
            injectionTargetMarker.gameObject.SetActive(false);

            // Set the specific scale you provided
            Vector3 baseScale = new Vector3(0.241970628f, 0.241970628f, 0.241970628f);
            injectionTargetMarker.localScale = baseScale;

            while (!injectionPlaced)
            {
                // Pulse effect for the target marker
                float duration = 1.0f;
                float halfDuration = duration / 2.0f;

                // Pulse grow - increase by 20% from the specific scale
                injectionTargetMarker.DOScale(baseScale * 1.2f, halfDuration)
                    .SetEase(Ease.InOutSine);
                yield return new WaitForSeconds(halfDuration);

                // Pulse shrink - return to the specific scale
                injectionTargetMarker.DOScale(baseScale, halfDuration)
                    .SetEase(Ease.InOutSine);
                yield return new WaitForSeconds(halfDuration);
            }
        }
        else
        {
            Debug.LogError("Injection target marker not assigned!");
            yield break;
        }
    }

    void OnInjectionDragEnd(Transform injectionTransform, Vector3 position)
    {
        injectionTargetMarker.gameObject.SetActive(false);
    }

    void OnInjectionDrag(Transform injectionTransform, Vector3 position)
    {
        injectionTargetMarker.gameObject.SetActive(true);
        // Update the line renderer positions as the injection moves
        if (useCurvedLine)
            UpdateCurvedLinePositions();
        else
            UpdateLinePositions();

        if (injectionPlaced)
            return;

        // Check if injection is close enough to the target
        if (injectionTargetMarker && Vector3.Distance(position, injectionTargetMarker.position) < injectionTargetDistance)
        {
            injectionPlaced = true;
            PlaceInjection();
        }
    }
    private void SetupLineRenderer()
    {
        // If no LineRenderer component exists, add one
        if (infusionLineRenderer == null && injection != null)
        {
            infusionLineRenderer = injection.gameObject.AddComponent<LineRenderer>();
        }

        if (infusionLineRenderer != null)
        {
            // Configure the LineRenderer
            infusionLineRenderer.startWidth = lineWidth;
            infusionLineRenderer.endWidth = lineWidth;
            infusionLineRenderer.positionCount = 2;

            // Set material if provided
            if (lineMaterial != null)
            {
                infusionLineRenderer.material = lineMaterial;
            }

            infusionLineRenderer.startColor = lineColor;
            infusionLineRenderer.endColor = lineColor;

            // Initialize line positions
            UpdateLinePositions();
        }
    }

    private void UpdateLinePositions()
    {
        if (infusionLineRenderer != null && lineStartPoint != null && lineEndPoint != null)
        {
            // Ensure line always follows the injection
            infusionLineRenderer.SetPosition(0, lineStartPoint.position); // Line start stays fixed
            infusionLineRenderer.SetPosition(1, lineEndPoint.position); // Line end follows the end point (injection)
        }
    }
    [SerializeField] private int curveSegments = 50; // Ensure this is not overridden in the Inspector

    private void SetupCurvedLineRenderer()
    {
        if (infusionLineRenderer == null && injection != null)
        {
            infusionLineRenderer = injection.gameObject.AddComponent<LineRenderer>();
        }

        if (infusionLineRenderer != null)
        {
            infusionLineRenderer.useWorldSpace = true;
            infusionLineRenderer.numCapVertices = 10;
            infusionLineRenderer.numCornerVertices = 10;

            infusionLineRenderer.startWidth = lineWidth;
            infusionLineRenderer.endWidth = lineWidth;
            infusionLineRenderer.positionCount = curveSegments;

            if (lineMaterial == null)
            {
                lineMaterial = new Material(Shader.Find("Sprites/Default"));
            }

            infusionLineRenderer.material = lineMaterial;
            infusionLineRenderer.startColor = lineColor;
            infusionLineRenderer.endColor = lineColor;

            UpdateCurvedLinePositions();

            Debug.Log("Set line renderer segments to: " + curveSegments);
        }
    }

    private void UpdateCurvedLinePositions()
    {
        if (infusionLineRenderer != null && lineStartPoint != null && lineEndPoint != null)
        {
            Vector3 start = lineStartPoint.position;
            Vector3 end = lineEndPoint.position;

            // Midpoint between start and end
            Vector3 mid = (start + end) / 2f;

            // Amount of downward sag
            float sagAmount = curveDrop; // You can control this in Inspector

            for (int i = 0; i < curveSegments; i++)
            {
                float t = i / (float)(curveSegments - 1);

                // Basic bezier curve: start -> control (sag) -> end
                Vector3 pointOnCurve = Mathf.Pow(1 - t, 2) * start +
                                       2 * (1 - t) * t * (mid + Vector3.down * sagAmount) +
                                       Mathf.Pow(t, 2) * end;

                infusionLineRenderer.SetPosition(i, pointOnCurve);
            }
        }
    }
    void PlaceInjection()
    {
        // Set frozen position
        injectionTargetMarker.gameObject.SetActive(false);
        //injectionLockedPosition = injectionPos.transform.position;
        injectionLockedPosition = new Vector3(
        injectionTargetMarker.transform.position.x + 0.5f,
        injectionTargetMarker.transform.position.y - 0.45f,
        injectionTargetMarker.transform.position.z);

        //injectionLockedPosition = injectionTargetMarker.transform.position;
        forceFreezeInjection = true;

        var dragComp = injection.GetComponent<DraggableObject>();
        if (dragComp != null)
        {
            dragComp.OnDragEvent -= OnInjectionDrag;

            if (dragComp.isDragging)
                dragComp.OnMouseUp();

            dragComp.enabled = false;
        }

        var collider = injection.GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        DOTween.Kill(injection);

        if (injection.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 👉 Rotate to correct angle before animating
        injection.rotation = Quaternion.Euler(0f, 0f, -43.508f);

        // Animate into final position
        injection.DOMove(injectionLockedPosition, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                injection.position = injectionLockedPosition;
                forceFreezeInjection = false;
                StartInfusion();
            });

        if (injectionTargetMarker)
        {
            injectionTargetMarker.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack);
        }
    }

    void StartInfusion()
    {
        if (infusionLiquid)
        {
            infusionLiquid.gameObject.SetActive(true);

            Vector3 initialScale = infusionLiquid.localScale;
            Vector3 initialPosition = infusionLiquid.localPosition;

            // Set the scale to start from full height
            infusionLiquid.localScale = new Vector3(initialScale.x, 1f, initialScale.z);

            // Start the infusion process
            StartCoroutine(InfuseLiquid(initialScale, initialPosition));
        }
        else
        {
            Debug.LogError("Infusion liquid transform not assigned!");
            FinishStep1();
        }
    }

    IEnumerator InfuseLiquid(Vector3 initialScale, Vector3 initialPosition)
    {
        float elapsedTime = 0f;

        while (elapsedTime < infusionDuration)
        {
            float t = elapsedTime / infusionDuration; // Normalized time (0 to 1)

            // Update the liquid's scale (decreasing from top to bottom)
            float newScaleY = Mathf.Lerp(1f, 0f, t);
            infusionLiquid.localScale = new Vector3(initialScale.x, newScaleY, initialScale.z);

            // Adjust the position to keep the top fixed and move the bottom down
            infusionLiquid.localPosition = new Vector3(initialPosition.x, initialPosition.y - (initialScale.y - newScaleY) / 2f, initialPosition.z);

            elapsedTime += Time.deltaTime; // Increment the elapsed time
            yield return null; // Wait for the next frame
        }

        // Ensure it reaches the final state
        infusionLiquid.localScale = new Vector3(initialScale.x, 0f, initialScale.z);
        infusionLiquid.localPosition = new Vector3(initialPosition.x, initialPosition.y - initialScale.y / 2f, initialPosition.z);

        // After infusion, complete the step
        FinishStep1();
    }


    void FinishStep1()
    {
        // Stop particles if running
        //if (infusionDripParticles && infusionDripParticles.isPlaying)
        //{
        //    infusionDripParticles.Stop();
        //}

        // Wait a moment before moving to next step
        DOVirtual.DelayedCall(1f, (TweenCallback)(() =>
        {
            // Slide out the injection
            dirpBag.DOMoveY(injection.position.y + 10f, 1f)
                .SetEase(Ease.InBack)
                .OnComplete((TweenCallback)(() =>
                {
                    // Notify completion
                    Debug.Log("Step 2 completed. Ready for Step 3");
                    injectionTargetMarker.transform.parent.gameObject.SetActive(false);
                    // You can call StartStep3() here or trigger an event\ 
                    StartCoroutine(FadeIntoStep2Routine());
                    //StartCoroutine(FadeIntoStep5Routine());
                    ShortcutExtensions.DOMove(this.characterChair.transform, characterBottom.transform.position - new Vector3(0, -1f, 0), 0.3f);
                }));
        }));
    }
    #endregion

    #region Step 2 - Stethoscope
    private IEnumerator FadeIntoStep2Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade
        step1Container.gameObject.SetActive(false);
        // ✅ Then call StartStep2 logic
        StartStep2();

        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }

    public GameObject patChairPos;
    public Transform stehtoScopePosIphone;
    public Transform patientPosIphone;

    public void StartStep2()
    {
        print("Step3 Started");
        targetsInCat.SetActive(true);
        characterChair.SetActive(true);
        characterChair.transform.DOMove((characterBottom.transform.position - new Vector3(0, 1f, 0)), 0.2f);
        step2Container.gameObject.SetActive(true);
        if (isIphone)
        {
            stethoscope.position = stehtoScopePosIphone.position;
            //characterObj.transform.position = patientPosIphone.position;
        }
        SoundManager.instance.PlayListenToTheTummyWithStethoScope();
        stethoscope.position += Vector3.right * 10f;
        //PatChair.transform.position = patChairPos.transform.position;
        //PatChair.transform.position = (characterBottom.transform.position - new Vector3(0,-1f,0));

        DoctorCabin.SetActive(true);
        DocObjects.SetActive(false);
        // Setup the curved line before showing the stethoscope
        SetupStethoscopeCurvedLineRenderer();

        DOVirtual.DelayedCall(1f, () =>
        {
            SoundManager.instance.PlayObjectPlaced();
            stethoscope.DOMoveX(stethoscope.position.x - 10f, 1f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    isDragging = true;
                    StartCoroutine(TrackStethoscopeDrag());
                });
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

    private IEnumerator TrackStethoscopeDrag()
    {
        float holdDurationPerTarget = 1.5f;
        float currentHoldTime = 0f;

        // Starting point for Y scaling (start small)
        float initialScaleY = 0f;
        Vector3 initialPos = progressBar.transform.localPosition;

        while (currentTarget < targetMarkers.Length)
        {
            Transform target = targetMarkers[currentTarget];
            bool isTargetComplete = false;

            while (!isTargetComplete)
            {
                if (isDragging && Vector3.Distance(stethoscopeEndpoint.position, target.position) < placementDistance)
                {
                    currentHoldTime += Time.deltaTime;

                    if (currentHoldTime >= holdDurationPerTarget)
                    {
                        // Calculate progress based on the current target
                        float t = (currentTarget + 1) / (float)targetMarkers.Length;

                        // Only change Y scale
                        float targetScaleY = Mathf.Lerp(initialScaleY, maxProgressScaleY, t);

                        // Apply the Y scaling to the progress bar (X and Z remain the same)
                        progressBar.transform.DOScaleY(targetScaleY, 0.5f).SetEase(Ease.OutQuad);
                        progressBar.transform.DOLocalMoveY(maxProgressPosition.y, 0.5f).SetEase(Ease.OutQuad);

                        // Move the stethoscope to the target position and disable the target
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
                    currentHoldTime = 0f; // Reset hold time if player moves away
                }

                yield return null;
            }
        }
        stethoscopeEndpoint.gameObject.GetComponent<DraggableObject>().isDragable = false;
        stethoscopeEndpoint.gameObject.GetComponent<CircleCollider2D>().enabled = false;

        FinishStep2();
    }

    private void FinishStep2()
    {
        isDragging = false;

        stethoscope.DOMoveX(stethoscope.position.x + 10f, 1f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                Debug.Log("Step 3 completed.");
                step2Container.gameObject.SetActive(false);
                StartStep3();
            });
    }
     private float baseScale = 0.3f;
     private float pulseScale = 0.4f;
     private float pulseDuration = 0.5f;
    private void ActivateTarget(int index)
    {
        for (int i = 0; i < targetMarkers.Length; i++)
        {
            Transform marker = targetMarkers[i];
            bool isActive = i == index;

            marker.gameObject.SetActive(isActive);
            marker.DOKill();

            if (isActive)
            {
                marker.localScale = Vector3.one * baseScale;

                marker.DOScale(pulseScale, pulseDuration)
                      .SetLoops(-1, LoopType.Yoyo)
                      .SetEase(Ease.InOutSine);
            }
            else
            {
                // Reset inactive markers to base scale
                marker.localScale = Vector3.one * baseScale;
            }
        }
    }
    #endregion

    #region Step 3 - Give the Pill
    void StartStep3()
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

        step3Gameobject.gameObject.SetActive(true);
        if (DeviceResForResolution.Instance.isIpad)
        {
            Vector3 pos = step3Gameobject.transform.position;
            pos.y -= 1.5f;
            step3Gameobject.transform.position = pos;



        }
        // Show pill tray sliding up
        if (pillTray)
        {
            pillTray.gameObject.SetActive(true);
            Vector3 startPos = pillTray.position;
            pillTray.position = new Vector3(startPos.x, startPos.y - 10f, startPos.z);
            pillTray.DOMove(startPos, 1f).SetEase(Ease.OutCubic);
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
            draggedObject.DOMove(catMouthOpen.position, 0.5f).OnComplete(() =>
            {
                draggedObject.DOScale(0, 0.3f).SetEase(Ease.OutCubic);

                // Close cat's mouth
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    catMouthOpen.gameObject.SetActive(false);
                    catMouth.gameObject.SetActive(true);

                    // Hide pill tray
                    pillTray.DOMove(new Vector3(pillTray.position.x, pillTray.position.y - 10f, pillTray.position.z), 1f)
                        .SetEase(Ease.InBack).OnComplete(() =>
                        {
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
            draggedObject.GetComponent<BoxCollider2D>().enabled = false; // Disable collider to prevent further dragging
            // Show cat drinking animation
            catMouth.gameObject.SetActive(false);
            catMouthOpen.gameObject.SetActive(true);

            // Create a sequence for glass animation
            Sequence drinkingSequence = DOTween.Sequence();
            Vector3 drinkPosition = Vector3.one;
            if(isIphone)
                drinkPosition = catMouth.position + new Vector3(1.8f, 0f, 0);
            else
                drinkPosition = catMouth.position + new Vector3(1.3f, 0, 0);
            draggedObject.position = drinkPosition; // Snap to the right position immediately
            draggedObject.rotation = Quaternion.Euler(0, 0, 0); // Set rotation if needed

            // Append the movement to the drink position
            drinkingSequence.Append(draggedObject.DOMove(drinkPosition, 0.3f));
            drinkingSequence.AppendInterval(0.1f);
            //drinkingSequence.Append(catDrinkingWater.DOLocalMove(new Vector3(-5.1f, 3.6f, 0), 0.02f));
            //drinkingSequence.Append(catDrinkingWater.DOLocalMove(new Vector3(-5.1f, 3.6f, 0), 0.02f));
            //drinkingSequence.AppendInterval(0.1f);
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

            // Position the parent at the glass position
            drinkingSequence.AppendCallback(() =>
            {
                //catDrinkingWater.transform.position = draggedObject.position;
                //catDrinkingWater.transform.rotation = draggedObject.rotation;

                // Make sure all but the first frame are inactive
                for (int i = 1; i < waterFrames.Length; i++)
                {
                    if (waterFrames[i] != null)
                        waterFrames[i].SetActive(false);
                }

                // Activate first frame
                if (waterFrames.Length > 0 && waterFrames[0] != null)
                {
                    // Position and activate first frame
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
                    drinkingSequence.AppendCallback(() =>
                    {
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
            drinkingSequence.AppendCallback(() =>
            {
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
            drinkingSequence.OnComplete(() =>
            {
                // Reset cat mouth
                catDrinkingWater.DOLocalRotate(Vector3.zero, 0.5f);

                emptyGlass.DOMove(waterGlassTray.position + new Vector3(0, 1f, 0), 0.5f);
                catMouthOpen.gameObject.SetActive(false);
                catMouth.gameObject.SetActive(true);
                // Hide all elements after delay
                DOVirtual.DelayedCall(2f, () =>
                {
                    HideAllStep4Elements();
                    // Delay before deactivating Step4Gameobject
                    DOVirtual.DelayedCall(1f, () =>
                    {  // Adjust delay as needed
                        step3Gameobject.gameObject.SetActive(false);
                        StartCoroutine(FadeIntoStep4Routine());

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

    #region Step 4 - Kill Bacteria
    private IEnumerator FadeIntoStep4Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade
        step4Gameobject.SetActive(true);
        DocObjects.SetActive(false);
        characterObj.SetActive(false);
        DoctorCabin.SetActive(false);
        // ✅ Then call StartStep2 logic
        StartStep4();

        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }

    public void StartStep4()
    {
        Debug.Log("<color=green>Step 4 started</color>");

        gunDevice.position += Vector3.right * 10f;
        SoundManager.instance.PlayGetRidOfTheBacteria();
        DOVirtual.DelayedCall(1f, () =>
        {
            SoundManager.instance.PlayObjectPlaced();
            gunDevice.DOMoveX(gunDevice.position.x - 10f, 1f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    StartCoroutine(TrackGunDrag()); // 👈 This is what kicks off the detection loop
                });
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
    private IEnumerator TrackGunDrag()
    {
        float[] holdTimers = new float[monsterTargets.Length]; // Track time for each monster
        bool[] monsterKilled = new bool[monsterTargets.Length]; // Track if a monster is already killed
        //isDragging = true;
        while (true)
        {
          
            isDragging1 = gunDevice.GetComponent<DraggableObject>().isDragging;
            gunDevice.GetComponent<DraggableObject>().OnDragStartEvent += OnGunDragStart ;
            gunDevice.GetComponent<DraggableObject>().OnDragEndEvent += OnGunDragEnd ;

            if (isDragging1)
            {
                // Enable the greenArea when dragging starts
               // SoundManager.instance.PlayLaser();
                bool allMonstersKilled = true; // Assume all monsters are killed

                for (int i = 0; i < monsterTargets.Length; i++)
                {
                    if (!monsterTargets[i].gameObject.activeSelf || monsterKilled[i]) continue; // Skip inactive or already killed monsters

                    float distance = Vector3.Distance(greenAreaPoint.position, monsterTargets[i].position);

                    // If the gun is within the shoot distance of the monster
                    if (distance < shootDistance)
                    {
                        holdTimers[i] += Time.deltaTime; // Increment the timer for this monster
                        Debug.Log($"Holding {monsterTargets[i].name} for {holdTimers[i]:F2} seconds");
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
                        // Once the timer reaches 2 seconds, kill the monster
                        if (holdTimers[i] >= 2f)
                        {
                            Debug.Log($"Monster killed: {monsterTargets[i].name}");
                            DOTween.Kill("shake" + i);
                            Destroy(monsterTargets[i].GetComponent<ShakeTracker>());

                            monsterKilled[i] = true;

                            Transform monster = monsterTargets[i];
                            monster.transform.parent.gameObject.GetComponent<Animator>().enabled = false;
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
                        // Reset the timer when the monster is out of range
                        DOTween.Kill("shake" + i);
                        Destroy(monsterTargets[i].GetComponent<ShakeTracker>());
                        holdTimers[i] = 0f;
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
                    FinishStep4();  // Trigger Step 7
                    yield break;   // Exit the coroutine as Step 7 is starting
                }
            }
            else
            {
                // Disable the greenArea when dragging stops
                greenArea.gameObject.SetActive(false);
               // SoundManager.instance.StopLaser();
                for (int i = 0; i < monsterTargets.Length; i++)
                {
                    if (monsterTargets[i].gameObject.activeSelf)
                    {
                        // Reset the timer when the gun is not dragging
                        holdTimers[i] = 0f;
                        DOTween.Kill("shake" + i);
                        Destroy(monsterTargets[i].GetComponent<ShakeTracker>());
                        ////enable animators
                        //if (monsterTargets[i].gameObject.GetComponent<Animator>() != null)
                        //{
                        //    monsterTargets[i].gameObject.GetComponent<Animator>().enabled = true;
                        //}
                        //else
                        //{
                        //    Debug.LogWarning("Animator component not found on monster target.");
                        //}
                    }
                }
            }

            yield return null;
        }
    }

    private void FinishStep4()
    {
        Debug.Log("All monsters are killed. Finish Step 4!");

        // Animate the gun moving to the right smoothly
        Vector3 targetPosition = gunDevice.transform.position + Vector3.right * 5f; // Adjust the "5f" to control how far the gun moves

        // Use DOTween to smoothly move the gun
        gunDevice.transform.DOMove(targetPosition, 1f) // Duration is 1 second, adjust as needed
            .SetEase(Ease.InOutSine) // Smooth easing function
            .OnComplete(() =>
            {
                StartCoroutine(FadeIntoStep5Routine());
                // Add any logic to execute after the gun finishes moving
                Debug.Log("Gun has moved to the right!");
            });

        // Proceed to Step 4 after moving the gun
    }

    private IEnumerator FadeIntoStep5Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade
        step4Gameobject.SetActive(false);
        // ✅ Then call StartStep2 logic
        step5Gameobject.SetActive(false);
        StartStepfifth();
        step5Gameobject.gameObject.SetActive(true);
        step5Gameobject.SetActive(true);
        DoctorCabin.SetActive(true);
        characterObj.SetActive(true);
        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }
    #endregion

    #region Step 5 - Blanket
    public void StartStepfifth()
    {
        //print blue color debug
        Debug.Log("<color=blue>Step 5 started</color>");
        step5Gameobject.SetActive(true);
        DoctorCabin.SetActive(true);
        characterObj.SetActive(true);
        characterChair.SetActive(true);
        SoundManager.instance.PlayPutBlanket();
        if (isIphone)
        {

            //characterObj.transform.position = patientPosIphone.position;
        }

        //characterChair.transform.position = patChairPos.transform.position;
        //characterObj.transform.position = paitentPos.position;

        if (blanket != null && blanket.GetComponent<DraggableObject>() != null)
        {
            var drag = blanket.GetComponent<DraggableObject>();
            SoundManager.instance.PlayObjectPlaced();
            blanket.transform.localScale = Vector3.one * 1.3f;
            drag.OnDragStartEvent += OnGunBlanketStart;

            drag.OnDragEvent += OnBlanketDrag;
            drag.OnDragEndEvent += OnBlanketDrop;
        }
        else
        {
            Debug.LogError("Blanket or its DraggableObject component is missing!");
        }
        step5Gameobject.SetActive(true);

        // Set character to required position
        if (characterObj != null)
        {

        }
        else
        {
            Debug.LogWarning("Character transform not assigned for Step 4!");
        }

        // Enable blanket interaction or whatever is next

    }
    void OnGunBlanketStart(Transform draggedObject, Quaternion rotation)
    {

        SoundManager.instance.PlayClick();
    }
    private void OnBlanketDrag(Transform draggedObject, Vector3 position)
    {
        // Optional visual feedback could go here
    }
    private void OnBlanketDrop(Transform draggedObject, Vector3 position)
    {
        //float distance = Vector3.Distance(position, blanketTargetPosition.position);
        float distance = Vector3.Distance(position, characterBottom.transform.position);
        BoxCollider2D col = blanket.GetComponent<BoxCollider2D>();
        if (distance < 1)
        {
            // Snap it to position
            draggedObject.parent = characterObj.transform;
            draggedObject.GetComponent<BoxCollider2D>().enabled = false;
            // Move the dragged object to a new position
            draggedObject.DOLocalMove(new Vector3(-3f, 6f, 0), 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                //sadFace.SetActive(false);
                //happyFace.SetActive(true);
                //happyFace.GetComponent<SpriteRenderer>().enabled = true;

                // Wait for 2 seconds after the move animation completes
                DOVirtual.DelayedCall(2f, () =>
                {
                    // Disable drag
                    var drag = draggedObject.GetComponent<DraggableObject>();
                    if (drag != null)
                    {
                        drag.OnDragEndEvent -= OnBlanketDrop;
                        drag.OnDragEvent -= OnBlanketDrag;
                        drag.enabled = false;
                        col.enabled = false;
                    }
                    StartCoroutine(FadeIntoStep6Routine()); // Proceed to next step
                                                            // Finish the step
                                                            //sadFace.SetActive(false);
                                                            //happyFace.SetActive(true);
                                                            //happyFace.GetComponent<SpriteRenderer>().enabled = true;

                    // Move the character and blanket

                });
            });
        }
    }
    private IEnumerator FadeIntoStep6Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(1f); // Slightly longer than fade
        characterObj.transform.DOMove(characterInitialPos, 1f).SetEase(Ease.OutSine);
        blanket.transform.DOMove(blanket.transform.position + new Vector3(15f, 0, 0), 0.5f).SetEase(Ease.OutSine);

        characterChair.transform.DOMoveY(characterChair.transform.position.y - 4f, 0.5f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            doctorObjects.SetActive(true);
            blanket.gameObject.SetActive(false);
        });

        characterObj.transform.DOScale(characterInitialScale, 1f).SetEase(Ease.OutBack).OnComplete(() => {
            // Complete the level after a short delay
            SoundManager.instance.PlayWellDone();
            SoundManager.instance.PlayDone();

            blanket.gameObject.SetActive(false);
        });

        characterChair.transform.DOScale(chairInitialScale, 1f).SetEase(Ease.OutBack);
        characterAnim.Play("Happy");

        purpleHeadEffect.SetActive(false);
        Invoke(nameof(CompleteLevel), 6.5f);        // 🌓 Fade back to normal
        yield return new WaitForSeconds(1f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }

    public void CompleteLevel()
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
           

            // Show the progress bar
            SceneManager.LoadScene("MainMenu");
        });
        
    }
    #endregion

    private void FillProgressBar()
    {
        progressFillAmount += 0.22f;
        progressFillAmount = Mathf.Clamp01(progressFillAmount);

        float targetHeight = progressFillAmount * originalBarHeight;

        progressBar.transform.DOScaleY(targetHeight, progressFillDuration)
            .SetEase(Ease.Linear);
    }

    //private void FinishStep4()
    //{
    //    Debug.Log("Blanket placed! Step 4 completed.");
    //    // step4Container.SetActive(false);

    //    // You can trigger next steps or animations here
    //}

    private IEnumerator DisableAfter(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }
}
