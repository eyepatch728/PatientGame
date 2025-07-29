using DG.Tweening;
using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

public class SportsInjuryManager : MonoBehaviour
{
    [Header("Camera")]
    public Camera camera;

    public Transform patient;
    public Transform patientInjury;
    public Transform medicalBoard;
    public Transform[] injuryIssue;
    public Transform doctorObjects;
    public Transform patMouth;
    public Animator characterAnim;
    public GameObject sprites;

    public Transform sittingPosForPatient;
    public Transform WalkingStartPosForPatient;

    public Transform patientArm;

    public Transform happyMouths;
    public Transform sadMouth1;
    public Transform sadMouth2;

    [Header("General Settings")]
    [SerializeField] private float rightMoveAmount = 5f; // How far items move to the right on completion
    [SerializeField] private float transitionDelay = 0.5f; // Delay before transitioning to next step
    [SerializeField] private float boardMoveUpDuration = 1.0f; // Duration for board animation up
    [SerializeField] private float boardMoveDownDuration = 1.0f; // Duration for board animation down
    [SerializeField] private float objectReturnDuration = 0.7f; // Duration for object to return to board

    [Header("Step 1 - Dirt Removal")]
    [SerializeField] private Transform tweezers;
    [SerializeField] private Transform tweezersChild;
    [SerializeField] private Transform[] dirtSpots;
    [SerializeField] private Transform trashBin;
    public Transform medicalBoardForTweezers;
    private Vector3 tweezersInitialPosition;
    public Transform bin;

    private bool isHoldingDirt = false;
    private Transform heldDirt = null;
    private bool isDropping = false;
    private int removedDirtCount = 0;
    private int totalDirtCount => dirtSpots.Length;

    [Header("Step 2 - Applying Ointment")]
    [SerializeField] private Transform ointment;
    [SerializeField] private Transform ointmentTarget;
    [SerializeField] private Animator ointmentAnimator;
    [SerializeField] private SpriteRenderer ointmentRenderer;
    [SerializeField] private GameObject ointmentContainer;
    public Transform ointmentCap;// Parent object for ointment step
    private bool isOintmentApplied = false;
    public Transform medicalBoardForOintment;
    private Vector3 ointmentInitialPosition;

    [Header("Step 3 - Applying Plasters")]
    [SerializeField] private Transform[] plasters;
    [SerializeField] private Transform[] plasterTargets;
    [SerializeField] private Transform medicineBoard;
    [SerializeField] private GameObject plastersContainer; // Parent object for plaster step
    private int appliedBandAidsCount = 0;
    private int totalBandAidsCount => plasters.Length;
    private bool[] plasterPlaced;
    public Transform medicalBoardForPlasters;
    private Vector3[] plastersInitialPositions;

    [Header("Step 4 - Bandage Wrap and Cut")]
    [SerializeField] private Transform bandageDraggable;
    [SerializeField] private Transform[] bandageTargets;
    [SerializeField] private GameObject[] bandageSprites; // 3 Sprites representing wraps
    [SerializeField] private Transform scissors;
    [SerializeField] private Transform scissorsTarget;
    [SerializeField] private Transform bandagePosAfterApplied;
    [SerializeField] private GameObject bandageContainer; // Parent object for bandage step
    public Transform medicalBoardForBandage;
    public Transform medicalBoardForScissors;
    private Vector3 bandageInitialPosition;
    private Vector3 scissorsInitialPosition;

    private int currentWrapStep = 0;
    private bool isCuttingStep = false;
    [SerializeField] private ScreenFade screenFade;
    // Step tracking
    private enum TreatmentStep
    {
        None,
        DirtCleaning,
        OintmentApplication,
        PlasterPlacement,
        BandageWrap
    }

    private TreatmentStep currentStep = TreatmentStep.None;
    public Animator fadeInAnim;
    void Start()
    {
        // Initialize arrays
        characterAnim.keepAnimatorStateOnDisable = true;
        CheckDeviceResolution();

        plasterPlaced = new bool[plasters.Length];
        plastersInitialPositions = new Vector3[plasters.Length];

        // Store initial positions
        if (tweezers != null) tweezersInitialPosition = tweezers.position;
        if (ointment != null) ointmentInitialPosition = ointment.position;
        if (plasters != null)
        {
            for (int i = 0; i < plasters.Length; i++)
            {
                if (plasters[i] != null) plastersInitialPositions[i] = plasters[i].position;
            }
        }
        if (bandageDraggable != null) bandageInitialPosition = bandageDraggable.position;
        if (scissors != null) scissorsInitialPosition = scissors.position;

        // Ensure all step containers are inactive initially
        if (ointmentContainer != null) ointmentContainer.SetActive(false);
        if (plastersContainer != null) plastersContainer.SetActive(false);
        if (bandageContainer != null) bandageContainer.SetActive(false);

        // Position medical board offscreen initially
        if (medicalBoard != null)
        {
            medicalBoard.gameObject.SetActive(false);
        }
        StartCoroutine(HandleCatEntryThenShowProblem());
        // Start the first step
       // AnimateIssue();
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

        // Set final sitting position
        Vector3 finalSitPos =sittingPosForPatient.position;

        // Walking starts off-screen at lower Y (walking height)
        Vector3 walkingStartPos = WalkingStartPosForPatient.position;
        characterAnim.transform.position = walkingStartPos;
        characterAnim.gameObject.SetActive(true);
        // Play walking animation
        characterAnim.Play("Walking");

        // Move to chair horizontally (maintain walking Y)
        characterAnim.transform.DOMoveX(finalSitPos.x, 3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // Switch to idle before sitting
                characterAnim.Play("IdleSittingAnimation");

                // Smoothly move up to sitting Y
                characterAnim.transform.DOMoveY(sittingPosForPatient.position.y, 0.4f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        // After 1s, play storytelling animation
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            characterAnim.Play("Storytelling");
                        });
                    });
            });

        // Wait until walk + sit is done
        yield return new WaitForSeconds(4.5f); // 3s walk + 0.4s sit + buffer

        // Continue with story
        AnimateIssue();
        sadMouth1.gameObject.SetActive(true);
    }

    void AnimateIssue()
    {
        //// Show computer overuse animation
        //foreach (var sprite in computerOveruseSprites)
        //{
        //    sprite.localScale = Vector3.zero;
        //    sprite.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        //    yield return new WaitForSeconds(0.3f);
        //}

        //yield return new WaitForSeconds(1f);

        //// Transition to treatment
        //foreach (var sprite in computerOveruseSprites)
        //{
        //    sprite.DOScale(0, 0.5f).SetEase(Ease.InBack);
        //}
        sprites.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        SoundManager.instance.PlayBlabla();
        for (int i = 0; i < injuryIssue.Length; i++)
        {
            Transform sprite = injuryIssue[i];
            if (sprite != null)
            {
                sprite.localScale = Vector3.zero;
                sprite.gameObject.SetActive(true);

                // Scale in the current sprite
                sequence.Append(sprite.DOScale(0.5f, 0.5f).SetEase(Ease.OutBack));

                // Wait before deactivating current and showing next
                sequence.AppendInterval(1f);

                // Deactivate current sprite (after it's been visible)
                int capturedIndex = i; // capture index for closure
                sequence.AppendCallback(() =>
                {
                    if (injuryIssue[capturedIndex] != null)
                        injuryIssue[capturedIndex].gameObject.SetActive(false);
                });
               DOVirtual.DelayedCall(5f, () =>
                {
                    // Move the camera to the right
                    StartCoroutine(FadeIntoStep2Routine());
                });
            }
        }

        sadMouth1.gameObject.SetActive(false);


    }
    private IEnumerator FadeIntoStep2Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic
        
        patientInjury.gameObject.SetActive(false);
        patientArm.gameObject.SetActive(true);
        StartStep1_DirtCleaning();

        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }
    #region Step 1 - Dirt Cleaning
    void StartStep1_DirtCleaning()
    {
        currentStep = TreatmentStep.DirtCleaning;
        Debug.Log("Starting Step 1: Dirt Cleaning");

        // Hide other step elements
        if (ointmentContainer != null) ointmentContainer.SetActive(false);
        if (plastersContainer != null) plastersContainer.SetActive(false);
        if (bandageContainer != null) bandageContainer.SetActive(false);

        // Activate medical board for tweezers
        if (medicalBoardForTweezers != null)
        {
            medicalBoardForTweezers.gameObject.SetActive(true);
            // Animate the board coming up from bottom of screen
            medicalBoardForTweezers.position = new Vector3(medicalBoardForTweezers.position.x, medicalBoardForTweezers.position.y - 10f, medicalBoardForTweezers.position.z);
            if (isIpad) 
            {
                medicalBoardForTweezers.position = new Vector3(medicalBoardForTweezers.position.x, medicalBoardForTweezers.position.y + 1.5f, medicalBoardForTweezers.position.z);

                medicalBoardForTweezers.DOMoveY(medicalBoardForTweezers.position.y +10, boardMoveUpDuration).SetEase(Ease.OutBack)
                    .OnComplete(() => {
                        // After board is up, show the tweezers
                        SetupTweezers();
                    });
            }
            medicalBoardForTweezers.DOMoveY(medicalBoardForTweezers.position.y + 10f, boardMoveUpDuration).SetEase(Ease.OutBack)
                .OnComplete(() => {
                    // After board is up, show the tweezers
                    SetupTweezers();
                });
            SoundManager.instance.PlayObjectPlaced();
            SoundManager.instance.PlayToolToRemoveBacteria();

        }
        else
        {
            // No specific board for tweezers, proceed directly
            SetupTweezers();
        }
    }

    void SetupTweezers()
    {
        // Reset tweezers position
        //tweezers.position = tweezersInitialPosition;

        // Setup tweezers with entrance animation

        tweezers.GetComponent<DraggableObject>().OnDragEvent += OnTweezersDrag;
        tweezers.GetComponent<DraggableObject>().OnDragStartEvent += OnTweezersStartDrag;



        // Setup dirt spots
        foreach (var dirt in dirtSpots)
            dirt.gameObject.SetActive(true);

        // Hide trash bin initially
        trashBin.gameObject.SetActive(false);
    }
    void OnTweezersStartDrag(Transform draggedObject, Quaternion rotation)
    {
        SoundManager.instance.PlayClick();
    }
    void OnTweezersDrag(Transform dragged, Vector3 position)
    {
        if (!isHoldingDirt)
        {
            foreach (var dirt in dirtSpots)
            {
                if (dirt.gameObject.activeSelf && Vector3.Distance(tweezersChild.position, dirt.position) < 0.3f)
                {
                    heldDirt = dirt;
                    isHoldingDirt = true;
                    trashBin.gameObject.SetActive(true);
                    break;
                }
            }
        }

        if (isHoldingDirt && heldDirt != null)
        {
            heldDirt.position = tweezersChild.position;

            if (!isDropping && Vector3.Distance(heldDirt.position, trashBin.position) < 1f)
            {
                isDropping = true;
                StartCoroutine(DropDirtIntoBin());
            }
        }
    }

    IEnumerator DropDirtIntoBin()
    {
        if (heldDirt == null) yield break;

        float dropTime = 0.5f;
        Vector3 start = heldDirt.position;
        Vector3 end = trashBin.position;

        float t = 0f;
        while (t < 1f)
        {
            heldDirt.position = Vector3.Lerp(start, end, t);
            t += Time.deltaTime / dropTime;
            yield return null;
        }

        heldDirt.gameObject.SetActive(false);
        heldDirt = null;
        isHoldingDirt = false;
        isDropping = false;

        removedDirtCount++;
        SoundManager.instance.PlayWhoosh();
        if (removedDirtCount >= totalDirtCount)
        {
            FinishStep1_DirtCleaning();
        }
    }
    public Transform twizersIniPos;
    public Transform binPos;
    void FinishStep1_DirtCleaning()
    {
        Debug.Log("Finishing Step 1: Dirt Cleaning");

        // Disable tweezers drag functionality
        if (tweezers != null)
        {
            DraggableObject draggable = tweezers.GetComponent<DraggableObject>();
            if (draggable != null)
            {
                draggable.OnDragEvent -= OnTweezersDrag;
                draggable.enabled = false;
            }
        }
        bin.DOMoveY(binPos.position.y - 10f, boardMoveDownDuration).SetEase(Ease.InBack);
        // Return tweezers to the medical board
        tweezers.DOMove(twizersIniPos.transform.position, objectReturnDuration).SetEase(Ease.OutQuad)
            .OnComplete(() => {
                // Move medical board down after tweezers return
                if (medicalBoardForTweezers != null)
                {
                    medicalBoardForTweezers.DOMoveY(medicalBoardForTweezers.position.y - 10f, boardMoveDownDuration).SetEase(Ease.InBack)
                        .OnComplete(() => {
                            medicalBoardForTweezers.gameObject.SetActive(false);
                            // Move to next step after board animation completes
                            Invoke("StartStep2_Ointment", transitionDelay);
                        });
                }
                else
                {
                    // No specific board, proceed to next step after delay
                    Invoke("StartStep2_Ointment", transitionDelay);
                }
            });

        // Move trash bin down
        trashBin.DOMoveY(trashBin.position.y - 10f, 1f).SetEase(Ease.InBack);
    }
    #endregion

    #region Step 2 - Ointment Application
    void StartStep2_Ointment()
    {
        currentStep = TreatmentStep.OintmentApplication;
        Debug.Log("Starting Step 2: Ointment Application");

        // Reset everything related to ointment
        isOintmentApplied = false;
        if (ointmentAnimator != null)
        {
            ointmentAnimator.enabled = false;
            ointmentAnimator.Rebind();
        }

        if (ointmentRenderer != null)
        {
            Color color = ointmentRenderer.color;
            color.a = 1f;
            ointmentRenderer.color = color;
        }
        SoundManager.instance.PlayOintmentOnTheCutt();
        // Activate medical board for ointment
        if (medicalBoardForOintment != null)
        {
            medicalBoardForOintment.gameObject.SetActive(true);
            ointmentContainer.SetActive(true);
            ointmentTarget.gameObject.SetActive(true);
            ointment.gameObject.SetActive(true);
            // Animate the board coming up from bottom of screen
            medicalBoardForOintment.position = new Vector3(medicalBoardForOintment.position.x, medicalBoardForOintment.position.y - 10f, medicalBoardForOintment.position.z);
            medicalBoardForOintment.DOMoveY(medicalBoardForOintment.position.y + 10f, boardMoveUpDuration).SetEase(Ease.OutBack)
                .OnComplete(() => {
                    // After board is up, show the ointment
                    SetupOintment();
                });
        }
        else
        {
            // No specific board for ointment, proceed directly
            SetupOintment();
        }
    }
    public Transform ointmentIniPos;
    void SetupOintment()
    {
        // Activate ointment container
        if (ointmentContainer != null)
            ointmentContainer.SetActive(true);

        // Reset ointment position
        ointment.position = ointmentIniPos.transform.position;

        // Animate the ointment cap: move left and down
        if (ointmentCap != null)
        {
            Vector3 targetPos = ointmentCap.position + new Vector3(-0.5f, -0.3f, 0f);
            ointmentCap.DOMoveY(ointment.position.y - 10 , 1.5f).SetEase(Ease.OutQuad);
        }
        ointment.GetComponent<DraggableObject>().OnDragStartEvent += OnOintmentDragStart;
        // Set up draggable component
        SetupDraggable(ointment, OnOintmentDropped);

        // Ensure the ointment target is visible if needed
    }
    public void OnOintmentDragStart(Transform draggedObject, Quaternion rotation) 
    {
    SoundManager.instance.PlayClick();
        // Play sound when ointment is picked up
       
    }
    void OnOintmentDropped(Transform ointmentObj, Vector3 position)
    {
        if (isOintmentApplied) return;

        float distance = Vector3.Distance(position, ointmentTarget.position);
        Debug.Log($"Ointment dropped: Distance to target = {distance}");

        // Use consistent threshold as other interactions (same as plaster)
        if (distance < 1f)
        {
            Debug.Log("Ointment applied successfully");

            // Disable dragging while animating
            DraggableObject draggable = ointmentObj.GetComponent<DraggableObject>();
            if (draggable != null)
                draggable.enabled = false;
            // Move ointment to target position
            ointmentObj.DOMove(ointmentTarget.position, 0.25f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    // Enable animator and play animation
                    if (ointmentAnimator != null)
                    {
            SoundManager.instance.PlaySplash();
                        ointmentAnimator.enabled = true;
                        ointmentAnimator.SetTrigger("Ointment");
                        Debug.Log("Ointment animation triggered");

                        // Backup in case animation event doesn't work
                        StartCoroutine(CheckOintmentCompleted());
                    }
                    else
                    {
                        // If no animator, simulate completion
                        FadeOutOintment();
                    }
                });
            DOVirtual.DelayedCall(5f, () =>
            {
            FadeOutOintment();
            
            });
            isOintmentApplied = true;
        }
        else
        {
            // Provide feedback on unsuccessful placement
            ointmentObj.DOMoveY(ointmentObj.position.y - 0.2f, 0.2f)
                .SetLoops(2, LoopType.Yoyo);
            Debug.Log("Ointment not close enough to target");
        }
    }

    // Backup in case animation event doesn't fire
    IEnumerator CheckOintmentCompleted()
    {
        yield return new WaitForSeconds(2f);
        if (ointment.gameObject.activeSelf && ointmentRenderer != null && ointmentRenderer.color.a > 0.9f)
        {
            Debug.Log("Backup ointment completion triggered");
            FadeOutOintment();
        }
    }

    // Called from animation event or as backup
    public void FadeOutOintment()
    {
        if (ointmentRenderer != null)
        {
            Debug.Log("Fading out ointment");

            // Return ointment to the medical board
            ointment.DOMove(ointmentIniPos.transform.position, objectReturnDuration).SetEase(Ease.OutQuad);

            // Fade out the ointment
            ointmentRenderer.DOFade(0f, 0.5f)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => {
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        // After fading out, disable the ointment and move the board down
                        FinishStep2_Ointment();
                    });
                });
        }
        else
        {
            Debug.LogError("Ointment renderer is null");
            FinishStep2_Ointment();
        }
    }

    void FinishStep2_Ointment()
    {
        Debug.Log("Finishing Step 2: Ointment Application");

        // Move medical board down
        if (medicalBoardForOintment != null)
        {
            medicalBoardForOintment.DOMoveY(medicalBoardForOintment.position.y - 10f, boardMoveDownDuration).SetEase(Ease.InBack)
                .OnComplete(() => {
                    medicalBoardForOintment.gameObject.SetActive(false);
                    // Deactivate ointment container after board moves down
                    if (ointmentContainer != null) ointmentContainer.SetActive(false);
                    // Move to next step after board animation completes
                    Invoke("StartStep3_Plasters", transitionDelay);
                });
        }
        else
        {
            // No specific board, deactivate ointment container directly
            if (ointmentContainer != null) ointmentContainer.SetActive(false);
            // Move to next step
            Invoke("StartStep3_Plasters", transitionDelay);
        }
    }
    #endregion

    #region Step 3 - Plaster Application
    void StartStep3_Plasters()
    {
        currentStep = TreatmentStep.PlasterPlacement;
        Debug.Log("Starting Step 3: Plaster Application");

        // Reset plaster placement state
        appliedBandAidsCount = 0;
        for (int i = 0; i < plasterPlaced.Length; i++)
            plasterPlaced[i] = false;
        SoundManager.instance.PlayPlaceBandage();

        // Activate medical board for plasters
        if (medicalBoardForPlasters != null)
        {
            medicalBoardForPlasters.gameObject.SetActive(true);
            plastersContainer.SetActive(true);
            // Animate the board coming up from bottom of screen
            medicalBoardForPlasters.position = new Vector3(medicalBoardForPlasters.position.x, medicalBoardForPlasters.position.y - 10f, medicalBoardForPlasters.position.z);
            medicalBoardForPlasters.DOMoveY(medicalBoardForPlasters.position.y + 10f, boardMoveUpDuration).SetEase(Ease.OutBack)
                .OnComplete(() => {
                    // After board is up, show the plasters
                    SetupPlasters();
                });
        }
        else
        {
            // No specific board for plasters, proceed directly
            SetupPlasters();
        }
    }

    void SetupPlasters()
    {
        // Activate plasters container
       
        // Reset plaster positions
        //for (int i = 0; i < plasters.Length; i++)
        //{
        //    plasters[i].position = plastersInitialPositions[i];
        //}

        // Setup plasters as draggable
        for (int i = 0; i < plasters.Length; i++)
        {
            plasterTargets[i].gameObject.SetActive(true); // Hide targets initially
            plasters[i].gameObject.SetActive(true);
            int index = i;
            SetupDraggable(plasters[i], (plasterObj, pos) => OnPlasterDropped(plasterObj, pos, index));
            plasters[i].GetComponent<DraggableObject>().OnDragStartEvent += OnGunDragStart;
        }

        // Show plaster targets
        foreach (var target in plasterTargets)
            target.gameObject.SetActive(true);

       
        // Show medicine board with animation
        //medicineBoard.gameObject.SetActive(true);
        //medicineBoard.position = new Vector3(medicineBoard.position.x, -10f, medicineBoard.position.z);
        //medicineBoard.DOMove(new Vector3(medicineBoard.position.x, -4f, medicineBoard.position.z), 1f).SetEase(Ease.OutBack);
    }
    void OnGunDragStart(Transform draggedObject, Quaternion rotation)
    {
        SoundManager.instance.PlayClick();
    }
    void OnPlasterDropped(Transform plasterObj, Vector3 position, int index)
    {
        if (plasterPlaced[index]) return;

        Transform target = plasterTargets[index];

        if (Vector3.Distance(position, target.position) < 3f)
        {
            plasterObj.DOMove(target.position, 0.25f).SetEase(Ease.OutQuad);
            plasterObj.DORotate(new Vector3(0f, 0f, 16.6241512f), 0.25f).SetEase(Ease.OutQuad);
            plasterPlaced[index] = true;
            appliedBandAidsCount++;
            SoundManager.instance.PlayClick();
            plasterObj.GetComponent<DraggableObject>().enabled = false;
            plasterObj.GetComponent<BoxCollider2D>().enabled = false;
            plasterObj.SetParent(patient);
            if (appliedBandAidsCount >= totalBandAidsCount)
            {
                FinishStep3_Plasters();
            }
        }
        else
        {
            plasterObj.DOMoveY(plasterObj.position.y - 0.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
    }

    void FinishStep3_Plasters()
    {
        Debug.Log("Finishing Step 3: Plaster Application");

        // Return plasters to their initial positions on the board
        for (int i = 0; i < plasters.Length; i++)
        {
            if (plasters[i] != null && !plasterPlaced[i])
            {
                plasters[i].DOMove(plastersInitialPositions[i], objectReturnDuration).SetEase(Ease.OutQuad);
            }
        }

        // Move medical board down
        if (medicalBoardForPlasters != null)
        {
            medicalBoardForPlasters.DOMoveY(medicalBoardForPlasters.position.y - 10f, boardMoveDownDuration).SetEase(Ease.InBack)
                .OnComplete(() => {
                    medicalBoardForPlasters.gameObject.SetActive(false);
                });
        }

        // Move medicine board to the right before exiting
        medicineBoard.DOLocalMoveX(medicineBoard.localPosition.x + rightMoveAmount, 0.7f).SetEase(Ease.OutQuad);

        // Move medicine board down and out of view
        medicineBoard.DOMove(new Vector3(medicineBoard.position.x, -10f, medicineBoard.position.z), 1f)
            .SetDelay(transitionDelay)
            .OnComplete(() => {
                // Deactivate plasters container
                if (plastersContainer != null) plastersContainer.SetActive(false);
                else medicineBoard.gameObject.SetActive(false);

                // Start the next step
                StartStep4_BandageWrap();
            });
    }
    #endregion

    #region Step 4 - Bandage Wrap

    public Transform sissorsIniPos;
    void StartStep4_BandageWrap()
    {
        currentStep = TreatmentStep.BandageWrap;
        Debug.Log("Starting Step 4: Bandage Wrap");
        
        // Activate medical board for bandage
        if (medicalBoardForBandage != null)
        {
            medicalBoardForBandage.gameObject.SetActive(true);
            bandageContainer.SetActive(true);

            // Animate the board coming up from bottom of screen
            medicalBoardForBandage.position = new Vector3(medicalBoardForBandage.position.x, medicalBoardForBandage.position.y - 10f, medicalBoardForBandage.position.z);
            medicalBoardForBandage.DOMoveY(medicalBoardForBandage.position.y + 10f, boardMoveUpDuration).SetEase(Ease.OutBack)
                .OnComplete(() => {
                    // After board is up, show the bandage
                    SetupBandage();
                });
        }
        else
        {
            // No specific board for bandage, proceed directly
            SetupBandage();
        }
            SoundManager.instance.PlayRollTheBandage();

    }

    public Transform bandageIniPos;

    void SetupBandage()
    {
        // Activate bandage container
        if (bandageContainer != null)
            bandageContainer.SetActive(true);

        // Reset wrap step
        currentWrapStep = 0;
        isCuttingStep = false;

        // Hide all bandage sprites initially
        for (int i = 0; i < bandageSprites.Length; i++)
            bandageSprites[i].SetActive(false);

        // Hide scissors initially
        scissors.gameObject.SetActive(false);

        // Reset bandage position
        bandageDraggable.position = bandageIniPos.transform.position;

        // Setup bandage as draggable
        SetupDraggable(bandageDraggable, OnBandageDropped);
        bandageDraggable.GetComponent<DraggableObject>().OnDragStartEvent += OnBandageDragStart;

    }

    void OnBandageDragStart(Transform draggedObject, Quaternion rotation)
    {
        SoundManager.instance.PlayClick();
    }
    public Transform bandagePosAfterCutting;

    void OnBandageDropped(Transform bandageObj, Vector3 position)
    {
        if (currentWrapStep >= bandageTargets.Length || isCuttingStep)
            return;

        Transform target = bandageTargets[currentWrapStep];

        // Check if drop is close enough to the target
        if (Vector3.Distance(position, target.position) < 2f)
        {
            GameObject bandageSprite = bandageSprites[currentWrapStep];
            bandageSprite.SetActive(true);
            SoundManager.instance.PlayWhoosh();
            // Reset localScale to prepare for animation
            bandageSprite.transform.localScale = new Vector3(0f, bandageSprite.transform.localScale.y, bandageSprite.transform.localScale.z);

            if (currentWrapStep == 2)
            {
                // Special case for third bandage: scale both X and Y
                Vector3 scale = new Vector3(0.65f, 0.43f, 0.65f);
                bandageSprite.transform.DOScale(scale, 1f).SetEase(Ease.OutBack);
            }
            else
            {
                // Only scale in X, keep Y/Z the same
                Vector3 targetScale = bandageSprite.transform.localScale;
                targetScale.x = 0.439873f;
                bandageSprite.transform.DOScaleX(targetScale.x, 1f).SetEase(Ease.OutBack);
            }

            currentWrapStep++;

            if (currentWrapStep >= bandageTargets.Length)
            {
                // Done with wrapping, prepare for cutting
                bandageObj.GetComponent<BoxCollider2D>().enabled = false;
                isCuttingStep = true;

                bandageObj.DOMove(bandagePosAfterCutting.position, 0.5f).SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        TransitionToScissors();
                    });
            }
        }
        else
        {
            // Invalid drop: bounce back for feedback
            bandageObj.DOMoveY(bandageObj.position.y - 0.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
    }


    void TransitionToScissors()
    {
        // Animate medicine board down
        medicineBoard.DOMove(new Vector3(medicineBoard.position.x, -10f, medicineBoard.position.z), 1f)
            .SetDelay(transitionDelay)
            .OnComplete(() => {
                // Deactivate plasters container
                if (plastersContainer != null) plastersContainer.SetActive(false);
                else medicineBoard.gameObject.SetActive(false);
                if (isIpad) 
                {
                    if (medicalBoardForScissors != null)
                    {
                        medicalBoardForScissors.gameObject.SetActive(true);
                        // Animate the board coming up from bottom of screen
                        medicalBoardForScissors.position = new Vector3(medicalBoardForScissors.position.x + 5, medicalBoardForScissors.position.y - 10f, medicalBoardForScissors.position.z);
                        medicalBoardForScissors.DOMoveY(medicalBoardForScissors.position.y + 5f, boardMoveUpDuration).SetEase(Ease.OutBack)
                            .OnComplete(() => {
                                // After board is up, show the scissors
                                SetupScissors();
                            });
                    }
                }
                // Activate scissors board
                if (medicalBoardForScissors != null)
                {
                    medicalBoardForScissors.gameObject.SetActive(true);
                    // Animate the board coming up from bottom of screen
                    medicalBoardForScissors.position = new Vector3(medicalBoardForScissors.position.x, medicalBoardForScissors.position.y - 10f, medicalBoardForScissors.position.z);
                    medicalBoardForScissors.DOMoveY(medicalBoardForScissors.position.y + 10f, boardMoveUpDuration).SetEase(Ease.OutBack)
                        .OnComplete(() => {
                            // After board is up, show the scissors
                            SetupScissors();
                        });
                }
                
                else
                {
                    // No specific board for scissors, proceed directly
                    SetupScissors();
                }
            });
    }

    void SetupScissors()
    {
        // Reset scissors position
        scissors.position = scissorsInitialPosition;

        // Show scissors with entrance animation
        scissors.gameObject.SetActive(true);
        //scissors.position = new Vector3(scissors.position.x, -10f, scissors.position.z);
        //scissors.DOMove(new Vector3(scissors.position.x, -4f, scissors.position.z), 0.8f)
        //    .SetEase(Ease.OutBack)
        //    .OnComplete(() => {
                SetupDraggable(scissors, OnScissorsDropped);
                DraggableObject dragScript = scissors.GetComponent<DraggableObject>();
                if (dragScript != null)
                    dragScript.OnDragStartEvent += OnScissorsDragStart;
            //});
    }

    void OnScissorsDragStart(Transform dragged, Quaternion _)
    {
        dragged.DORotate(new Vector3(0, 0, -151.467f), 0.3f).SetEase(Ease.OutSine);
        SoundManager.instance.PlayClick();
    }

    // Modified OnScissorsDropped function
    void OnScissorsDropped(Transform scissorsObj, Vector3 position)
    {
        // Add debug logs to see what's happening
        Debug.Log($"Scissors dropped at: {position}, Target is: {scissorsTarget.position}, Distance: {Vector3.Distance(position, scissorsTarget.position)}");

        // Increase the detection radius to make it easier to hit the target
        if (Vector3.Distance(position, scissorsTarget.position) < 2f) // Changed from 1f to 2f
        {
            Debug.Log("Scissors target hit successfully!");

            // Rotate blades
            Transform blade1 = scissors.GetChild(0);
            Transform blade2 = scissors.GetChild(1);

            blade1.DOLocalRotate(new Vector3(0, 0, -16f), 0.3f).SetEase(Ease.OutSine);
            blade2.DOLocalRotate(new Vector3(0, 0, 16f), 0.3f).SetEase(Ease.OutSine);
            SoundManager.instance.PlayCutting();
            // Move and animate scissors
            scissorsObj.DOMove(scissorsTarget.position, 0.3f).SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    Debug.Log("Scissors animation completed");

                    // After scissors cut, adjust final bandage sprite if needed
                    if (currentWrapStep == 3)
                    {
                        GameObject finalBandageSprite = bandageSprites[2];
                        if (finalBandageSprite != null)
                            finalBandageSprite.transform.DOScale(new Vector3(0.45f, 0.4f, 0.4f), 0.3f).SetEase(Ease.OutBack);
                    }

                    // First, move the bandage to its initial position
                    Transform bandageObj = bandageDraggable;
                    bandageObj.DOMove(bandageIniPos.position, 0.5f).SetEase(Ease.OutQuad)
                        .OnComplete(() => {
                            Debug.Log("Bandage returned to initial position");

                            // After bandage is in position, return scissors to the board
                            scissorsObj.DOMove(scissorsInitialPosition, objectReturnDuration).SetEase(Ease.OutQuad)
                                .OnComplete(() => {
                                    Debug.Log("Scissors returned to initial position");

                                    // When scissors return to board, move the scissors board down
                                    if (medicalBoardForScissors != null)
                                    {
                                        medicalBoardForScissors.DOMoveY(medicalBoardForScissors.position.y - 10f, boardMoveDownDuration).SetEase(Ease.InBack)
                                            .OnComplete(() => {
                                                medicalBoardForScissors.gameObject.SetActive(false);

                                                // Now lower the bandage medical board if it's still active
                                                if (medicalBoardForBandage != null && medicalBoardForBandage.gameObject.activeSelf)
                                                {
                                                    medicalBoardForBandage.DOMoveY(medicalBoardForBandage.position.y - 10f, boardMoveDownDuration).SetEase(Ease.InBack)
                                                        .OnComplete(() => {
                                                            medicalBoardForBandage.gameObject.SetActive(false);

                                                            // Complete the wrapping step
                                                            FinishStep4_Wrapping();
                                                        });
                                                }
                                                else
                                                {
                                                    // Complete the wrapping step
                                                    FinishStep4_Wrapping();
                                                }
                                            });
                                    }
                                    else
                                    {
                                        // No scissors board to animate, so lower bandage board if needed
                                        if (medicalBoardForBandage != null && medicalBoardForBandage.gameObject.activeSelf)
                                        {
                                            medicalBoardForBandage.DOMoveY(medicalBoardForBandage.position.y - 10f, boardMoveDownDuration).SetEase(Ease.InBack)
                                                .OnComplete(() => {
                                                    medicalBoardForBandage.gameObject.SetActive(false);

                                                    // Complete the wrapping step
                                                    FinishStep4_Wrapping();
                                                });
                                        }
                                        else
                                        {
                                            // No boards to animate, proceed directly
                                            FinishStep4_Wrapping();
                                        }
                                    }
                                });
                        });
                });
        }
        else
        {
            Debug.Log("Scissors missed target - distance too far");
            // Visual feedback that the scissors are in the wrong position
            scissorsObj.DOMoveY(scissorsObj.position.y - 0.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
    }

    void FinishStep4_Wrapping()
    {
        SoundManager.instance.StopCutting();
        Debug.Log("Finishing Step 4: Bandage Wrap");

        // Deactivate bandage container after a delay
        if (bandageContainer != null)
        {
            Invoke("DeactivateBandageContainer", transitionDelay);
        }
        else
        {
            // Directly complete treatment
            CompleteTreatment();
            StartCoroutine(FadeIntoStep3Routine());
        }
    }
    public Transform realPatientBandage;
    private IEnumerator FadeIntoStep3Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic

        patientInjury.gameObject.SetActive(true);
        patientArm.gameObject.SetActive(false);
        realPatientBandage.gameObject.SetActive(true);
        characterAnim.transform.position = new Vector3(characterAnim.transform.position.x, characterAnim.transform.position.y - 1,characterAnim.transform.position.z);
        patientInjury.gameObject.GetComponent<Animator>().Play("FinalHappy");
        happyMouths.gameObject.SetActive(true);
        sadMouth2.gameObject.SetActive(false);
        sadMouth1.gameObject.SetActive(false);  
        patMouth.gameObject.SetActive(false);
       // StartStep1_DirtCleaning();
        SoundManager.instance.PlayDone();
        SoundManager.instance.PlayWellDone();

        DOVirtual.DelayedCall(5f, () =>
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
            DOVirtual.DelayedCall(1.4f, () =>
            {
                // Optionally, you can show a success message or UI here
                Debug.Log("Patient cured successfully!");
                SceneManager.LoadScene(0);

            });
        });
        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }
    void DeactivateBandageContainer()
    {
        if (bandageContainer != null) bandageContainer.SetActive(false);
        CompleteTreatment();
    }
    #endregion

    #region Common Helper Methods
    void SetupDraggable(Transform obj, DraggableObject.DragEndHandler handler)
    {
        if (obj != null)
        {
            DraggableObject draggable = obj.GetComponent<DraggableObject>();
            if (draggable == null)
                draggable = obj.gameObject.AddComponent<DraggableObject>();

            // Remove previous bindings to avoid duplicates
            draggable.OnDragEndEvent -= handler;
            draggable.OnDragEndEvent += handler;

            // Ensure collider exists
            if (obj.GetComponent<BoxCollider2D>() == null)
                obj.gameObject.AddComponent<BoxCollider2D>();

            // Enable draggable
            draggable.enabled = true;
        }
    }

    void CompleteTreatment()
    {
        Debug.Log("Treatment completed successfully!");
        StartCoroutine(FadeIntoStep3Routine());
        // Here you would add game completion logic
        // Such as showing success UI, playing celebratory animation, etc.
    }
    #endregion
}