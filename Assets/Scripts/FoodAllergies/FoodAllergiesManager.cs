using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FoodAllergiesManager : MonoBehaviour
{
    [Header("Camera")]
    public Camera camera;

    public Animator characterAnim;
    public GameObject characterObj;
    public GameObject patChair;
    public Transform sittingPosForPatient;
    public Transform WalkingStartPosForPatient;

    [Header("Doctor Objects")]
    public GameObject doctorObjects;
    public GameObject patChairPos;
    [Header("Problem Visualization")]
    public Transform[] problemSprites;
    public GameObject story;

    [Header("Step 1 - Nose Wiping")]
    public Transform Step1Gameobject;
    public Transform tissueBox;
    public Transform tissue;
    public Transform usedTissue;
    public Transform trashCan;
    public Transform catNose;
    public Transform catNoseGerms;

    [Header("Step 2 - Allergy Test")]
    public Transform Step2Gameobject;

    public Transform remainingGameobjects;
    public Transform[] foodStickers;           // The food stickers to be placed
    public Transform[] stickerPlaceholders;    // Where stickers can be placed
    public Transform Tray;              // Tray containing stickers
    //public Transform pipetteTray;              // Tray containing test pipettes
    public Transform[] testPipettes;           // The test pipettes
    public Transform catArm;                   // Cat's arm where stickers are placed
    public Transform allergicReaction;         // The bump reaction
    private int foodAllergyIndex;              // Which food causes the allergy
    private int stickersPlaced = 0;            // Counter for placed stickers
    private int testsPerfomed = 0;             // Counter for tests performed
    public Transform resultsFolder;            // Folder that shows test results
    public Transform[] resultIcons;            // Icon results (check/X)
    public Transform stickersParent;
    public Transform pipetteParent;
    public GameObject[] reactionSprites;
    [Header("Step 3 - Give the Pill")]
    public Transform Step3Gameobject;
    public Transform pillTray;
    public Transform pill;
    public Transform waterGlassTray;
    public Transform waterGlass;
    public Transform emptyGlass;
    public Transform catMouth;
    public Transform catMouthOpen;
    public Transform catHappyMouth;
    public Transform catDrinkingWater;
    public Animator characterAnimator;
    public Transform catLowerBodyPart;

    private bool isStoryComplete = false;
    private bool isStep1Complete = false;
    private bool isStep2Complete = false;
    private bool isStep3Complete = false;
    private bool isDraggingTissue = false;
    private bool allergicReactionShown = false;
    private bool isNoseWiped = false;

    private List<Transform> occupiedPlaceholders = new List<Transform>();
    public bool isIpad;
    public bool isIphone;
    private float aspectRatio;

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CheckDeviceResolution();
        InitializeGameElements();
        //StartStory();
        characterAnimator.keepAnimatorStateOnDisable = true;
        StartCoroutine(HandleCatEntryThenShowProblem());
        //StartStep3();
    }

    // Update is called once per frame
    void Update()
    {

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


        if (Step3Gameobject) Step3Gameobject.gameObject.SetActive(false);
        if (pillTray) pillTray.gameObject.SetActive(false);
        if (pill) pill.gameObject.SetActive(false);
        if (waterGlassTray) waterGlassTray.gameObject.SetActive(false);
        //if (waterGlass) waterGlass.gameObject.SetActive(false);
        //if (emptyGlass) emptyGlass.gameObject.SetActive(false);
        if (catMouthOpen) catMouthOpen.gameObject.SetActive(false);
        //if (catDrinking) catDrinking.gameObject.SetActive(false);

    }
    IEnumerator HandleCatEntryThenShowProblem()
    {
        yield return new WaitForSeconds(0.1f);

        // Final sitting position
        Vector3 sittingPos = sittingPosForPatient.position;

        // Walking starts off-screen with walking Y pos
        //Vector3 walkingStartPos = new Vector3(sittingPos.x - 7f, -4f, 0);
        Vector3 walkingStartPos = WalkingStartPosForPatient.position;

        characterAnim.transform.position = walkingStartPos;
        characterAnim.gameObject.SetActive(true);
        // Play walking animation
        characterAnim.Play("Walking");

        // Move horizontally to chair (maintain Y = -4)
        characterAnim.transform.DOMoveX(sittingPos.x, 3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // Play idle standing first
                characterAnim.Play("Idle_StandDown");

                // Smoothly move up to sitting Y position
                characterAnim.transform.DOMoveY(sittingPos.y, 0.4f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        // Play storytelling after sitting
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            characterAnim.Play("Storytelling");
                        });
                    });
            });

        // Wait for entire walk and sit sequence
        yield return new WaitForSeconds(4.5f); // 3s walk + 0.4s sit + 1s delay

        // Continue the story
        StartStory();
    }


    #region Problem Visualization
    void StartStory()
    {
        AnimateIssue();
        catMouthOpen.gameObject.SetActive(true);
    }
    public float zoomVal;
    void AnimateIssue()
    {
        story.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        SoundManager.instance.PlayBlabla();

        for (int i = 0; i < problemSprites.Length; i++)
        {
            if (problemSprites[i] != null)
            {
                Transform sprite = problemSprites[i];

                if (i != 0)
                    sprite.gameObject.SetActive(false); // Hide all except first initially
                else
                    sprite.localScale = Vector3.zero;   // Hide first sprite (scale-wise)

                // Animate this sprite
                sequence.AppendCallback(() =>
                {
                    sprite.gameObject.SetActive(true); // Show before animating
                });
                if (isIpad) 
                {
                sequence.Append(sprite.DOScale(0.5f, 1.5f).SetEase(Ease.OutBack));

                }
                else
                sequence.Append(sprite.DOScale(0.6f, 1.5f).SetEase(Ease.OutBack));
                sequence.AppendInterval(1.0f); // Wait before next sprite
            }
        }

        sequence.OnComplete(() =>
        {
            doctorObjects.SetActive(false);

            // Smooth camera zoom
            if (DeviceResForResolution.Instance.isIpad)
            {
                zoomVal = 5f;

            }
            else
            {
                zoomVal = 4;
            }
            float zoomDuration = 0.5f;
            camera.DOOrthoSize(zoomVal, zoomDuration).SetEase(Ease.InOutSine);

            isStoryComplete = true;
            StartCoroutine(FadeIntoStep2Routine());
            //StartStep3();
        //    StartStep1();
        });
    }
    #endregion

    public Transform usedPaperPos;

    #region Step 1: Measure the temperature
    public Transform patChairPoss;
    private IEnumerator FadeIntoStep2Routine()
    {
        // 🔲 Fade to black first
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f); // Slightly longer than fade

        // ✅ Then call StartStep2 logic

        StartStep1();
        catMouthOpen.gameObject.SetActive(false);
        characterAnim.Play("Idle_StandDown");
        // Hide problem sprites
        foreach (var sprite in problemSprites)
        {
            if (sprite != null)
            {
                sprite.DOScale(0, 0.5f).SetEase(Ease.InBack);
            }
        }
        // Activate step 1 elements
        Step1Gameobject.gameObject.SetActive(true);
        Vector3 targetPosition = characterObj.transform.position + new Vector3(3f, -3f, 0);

        characterObj.transform.DOScale(0.4f, 0.2f).SetEase(Ease.OutBack);
        patChair.transform.DOScale(0.8f, 0.2f).SetEase(Ease.OutBack);

        // Move both character and chair, chair will be lower by 2 in Y-axis
        characterObj.transform.DOMove(targetPosition, 0.1f).SetEase(Ease.OutSine);
        if (DeviceResForResolution.Instance.isIpad) 
        {
            patChair.transform.position = patChairPoss.position;
            patChair.transform.DOScale(1.3f, 0.5f);

        }
        else
         patChair.transform.DOScale(0f, 0.5f);


        // 🌓 Fade back to normal
        yield return new WaitForSeconds(0.5f); // Optional small hold
        screenFade.FadeOut(0.5f);
    }
    void StartStep1()
    {
        camera.transform.position += new Vector3(0.5f, camera.transform.position.y, camera.transform.position.z);
       // patChair.transform.DOMove(targetPosition + new Vector3(+1f, 0f, 0), 1f).SetEase(Ease.OutSine);
        // Show tissue box sliding in from bottom
        if (tissueBox)
        {
            SoundManager.instance.PlayTissueWiping();
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
            Vector3 startPos = position;
            wipeSequence.Append(draggedObject.DOMove(new Vector3(startPos.x - 0.5f, startPos.y, startPos.z), 0.3f));
            wipeSequence.Append(draggedObject.DOMove(new Vector3(startPos.x + 0.5f, startPos.y, startPos.z), 0.3f));
            wipeSequence.Append(draggedObject.DOMove(startPos, 0.3f));
            wipeSequence.OnComplete(() =>
            {
                // Hide clean tissue
                draggedObject.gameObject.SetActive(false);
                catNoseGerms.gameObject.SetActive(false);
                // Show used tissue at UsedPaperPosition
                if (usedTissue)
                {
                    usedTissue.position = draggedObject.position; // start from where the clean tissue was
                    usedTissue.localScale = Vector3.zero;
                    usedTissue.gameObject.SetActive(true);

                    Sequence moveAndScale = DOTween.Sequence();
                    moveAndScale.Append(usedTissue.DOScale(new Vector3(0.6f , 0.6f , 0.6f), 0.2f).SetEase(Ease.OutBack));
                    moveAndScale.Join(usedTissue.DOMove(usedPaperPos.position, 0.5f).SetEase(Ease.OutBack));

                    usedTissue.GetComponent<DraggableObject>().OnDragEndEvent += OnUsedTissueDragEnd;
                }
                // Remove tissue box
                if (tissueBox)
                {
                    Vector3 exitPos = tissueBox.position;
                    tissueBox.DOMove(new Vector3(exitPos.x, exitPos.y - 10f, exitPos.z), 1f).SetEase(Ease.InBack);
                }
                // Show trash can
                if (trashCan)
                {
                    SoundManager.instance.PlayTissueInTrash();

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
            draggedObject.DOMove(trashCan.position - new Vector3(1.5f, 0, 0), 0.3f).OnComplete(() =>
            {
            SoundManager.instance.PlayWhoosh();

                draggedObject.DOScale(0, 0.3f).SetEase(Ease.InBack);
                // Hide trash can
                trashCan.DOMove(new Vector3(trashCan.position.x, trashCan.position.y - 10f, trashCan.position.z), 1f)
                    .SetEase(Ease.InBack).OnComplete(() =>
                    {
                        // Hide all elements after delay
                        DOVirtual.DelayedCall(0.1f, () =>
                        {
                            HideAllStep1Elements();
                            DOVirtual.DelayedCall(0.7f, () =>
                            {
                                isStep1Complete = true;
                                StartStep2();
                            });
                        });
                    });
            });
        }
    }

    void HideAllStep1Elements()
    {
        // Hide all remaining elements
        if (Step1Gameobject) Step1Gameobject.DOMove(new Vector3(Step1Gameobject.position.x, Step1Gameobject.position.y - 10f, Step1Gameobject.position.z), 1f).SetEase(Ease.InBack);
       
    }

    #endregion



    #region Step 2: Allergy Test 
    void StartStep2()
    {
        if (isIphone)
        {
         // Adjust duration for smooth effect
            
            camera.DOOrthoSize(1, 0.1f).SetEase(Ease.InOutSine);
        }
        allergicReactionShown = false;
        // Smoothly zoom the camera before starting the animation
        float targetSize = 5f; // Adjust this to the desired zoom level
        float zoomDuration = 2f; // Adjust duration for smooth effect
        if (isIphone)
            targetSize = 3.5f;
        camera.DOOrthoSize(targetSize, zoomDuration).SetEase(Ease.InOutSine);
        // Hide elements from previous step if still active
        if (Step1Gameobject.gameObject.activeInHierarchy)
        {
            Step1Gameobject.gameObject.SetActive(false);
        }
        remainingGameobjects.gameObject.SetActive(false);
        // Activate step 2 elements
        Step2Gameobject.gameObject.SetActive(true);

        // Randomly select which food will cause allergic reaction
        foodAllergyIndex = UnityEngine.Random.Range(0, foodStickers.Length);

        if (allergicReaction && reactionSprites != null)
        {
            foreach (var sprite in reactionSprites)
            {
                if (sprite) sprite.SetActive(false);
            }
        }

        // Show sticker tray sliding in from right
        if (Tray)
        {
            Tray.gameObject.SetActive(true);
            Vector3 startPos = Tray.position;
            Tray.position = new Vector3(startPos.x + 10f, startPos.y, startPos.z);
            Tray.DOMove(startPos, 1f).SetEase(Ease.OutBack);
        }
        SoundManager.instance.PlayPlaceTheStickersOnTheHand();
        // Activate stickers and add drag event handlers
        for (int i = 0; i < foodStickers.Length; i++)
        {
            if (foodStickers[i])
            {
                foodStickers[i].gameObject.SetActive(true);

                DraggableObject draggable = foodStickers[i].GetComponent<DraggableObject>();

                // Unsubscribe first to prevent duplicate subscriptions
                draggable.OnDragEndEvent -= OnStickerDragEnd;
                draggable.OnDragStartEvent -= OnStickerDragStart;

                draggable.OnDragEndEvent += OnStickerDragEnd;
                draggable.OnDragStartEvent += OnStickerDragStart;
            }
        }

        void OnStickerDragStart(Transform draggedObject, Quaternion rotation)
        {
            SoundManager.instance.PlayClick();
        }
        // Activate sticker placeholders
        for (int i = 0; i < stickerPlaceholders.Length; i++)
        {
            if (stickerPlaceholders[i])
            {
                stickerPlaceholders[i].gameObject.SetActive(true);
            }
        }

        // Hide pipette tray and pipettes initially
        //if (Tray) Tray.gameObject.SetActive(false);
        foreach (var pipette in testPipettes)
        {
            if (pipette) pipette.gameObject.SetActive(false);
        }

        // Hide results folder initially
        if (resultsFolder) resultsFolder.gameObject.SetActive(false);
        foreach (var icon in resultIcons)
        {
            if (icon) icon.gameObject.SetActive(false);
        }

        // Hide allergic reaction initially
        if (allergicReaction) allergicReaction.gameObject.SetActive(false);
    }


    void OnStickerDragEnd(Transform draggedObject, Vector3 position)
    {
        bool placed = false;
        Transform targetPlaceholder = null;

        foreach (Transform placeholder in stickerPlaceholders)
        {
            if (placeholder.gameObject.activeInHierarchy &&
                Vector3.Distance(position, placeholder.position) < 1.5f &&
                !occupiedPlaceholders.Contains(placeholder)) // Check if occupied
            {
                // Snap sticker to placeholder
                SoundManager.instance.PlayClick();
                draggedObject.DOMove(placeholder.position, 0.3f);
                draggedObject.GetComponent<DraggableObject>().enabled = false; // Disable further dragging
                draggedObject.GetComponent<CircleCollider2D>().enabled = false; // Disable further dragging
                draggedObject.GetComponent<SpriteRenderer>().sortingOrder = 3; // Disable further dragging
                draggedObject.DOScale(Vector3.one * 0.7f, 0.3f).SetEase(Ease.OutBack);
                targetPlaceholder = placeholder;
                occupiedPlaceholders.Add(placeholder); // Mark as occupied
                stickersPlaced++;
                placed = true;
                break;
            }
        }

        if (placed && targetPlaceholder != null)
        {
            draggedObject.SetParent(targetPlaceholder);
        }
        else
        {
            // Return to original position if not placed
            draggedObject.DOMove(draggedObject.GetComponent<DraggableObject>().originalPosition, 0.5f);
        }

        // Check if all stickers are placed
        if (stickersPlaced >= foodStickers.Length)
        {
            Tray.DOMove(new Vector3(Tray.position.x + 10f, Tray.position.y, Tray.position.z), 1f)
                .SetEase(Ease.OutBack).OnComplete(() => {
                    PrepareForTesting();
                });
        }
    }


    void PrepareForTesting()
    {
        stickersParent.gameObject.SetActive(false);
        pipetteParent.gameObject.SetActive(true);
        // Show pipette tray sliding in
        if (Tray)
        {
            Tray.gameObject.SetActive(true);
            Vector3 targetPos = new Vector3(Tray.position.x - 10f, Tray.position.y, Tray.position.z);
            Tray.DOMove(targetPos, 1f).SetEase(Ease.OutBack);

            // Activate pipettes and add drag event handlers
            foreach (var pipette in testPipettes)
            {
                if (pipette)
                {
                    pipette.gameObject.SetActive(true);
                    pipette.GetComponent<DraggableObject>().OnDragEndEvent += OnPipetteDragEnd;
                    pipette.GetComponent<DraggableObject>().OnDragStartEvent += OnPippeteDragStart;

                }
            }
        }
    }
    void OnPippeteDragStart(Transform draggedObject, Quaternion rotation)
    {
        SoundManager.instance.PlayClick();
        draggedObject.DORotate(Vector3.zero, 0.3f);
        //check in all the child objects of draggedobject & make their sorting order of sprite renderer as 201
        foreach (Transform child in draggedObject)
        {
            if (child.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.sortingOrder = 3000;
            }
        }
    }

    void OnPipetteDragEnd(Transform draggedObject, Vector3 position)
    {
        draggedObject.GetComponent<SpriteRenderer>().sortingOrder = 5;

        draggedObject.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 1000;
        draggedObject.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = 4;// Re-enable dragging
        draggedObject.GetChild(2).GetComponent<SpriteRenderer>().sortingOrder = 4;// Re-enable dragging
        draggedObject.GetChild(3).GetComponent<SpriteRenderer>().sortingOrder = 3;// Re-enable dragging
        // Re-enable dragging
        int pipetteIndex = Array.IndexOf(testPipettes, draggedObject);
        if (pipetteIndex < 0) return; // Safety check

        // Check if the pipette is near its corresponding sticker
        if (Vector3.Distance(position, foodStickers[pipetteIndex].position) < 1.5f)
        {
            SoundManager.instance.PlayDrop();
            // Animate pipette dropping liquid
            Sequence testSequence = DOTween.Sequence();
            Vector3 testPosition = foodStickers[pipetteIndex].position + new Vector3(0, 1f, 0);
            testSequence.Append(draggedObject.DOMove(testPosition, 0.3f));
            testSequence.Append(draggedObject.DOScale(draggedObject.localScale * 0.9f, 0.2f));
            testSequence.Append(draggedObject.DOScale(draggedObject.localScale, 0.2f));

            draggedObject.gameObject.GetComponent<Animator>().enabled = true;
            GameObject dropObject = new GameObject("Drop");
            dropObject.transform.position = draggedObject.position + new Vector3(0, -0.5f, 0);
            dropObject.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
            SpriteRenderer dropRenderer = dropObject.AddComponent<SpriteRenderer>();
            testSequence.Append(dropObject.transform.DOMove(foodStickers[pipetteIndex].position, 0.5f));

            testSequence.OnComplete(() => {
                GameObject.Destroy(dropObject);
                draggedObject.gameObject.GetComponent<Animator>().enabled = false;

                // Ensure only the correct pipette triggers an allergic reaction
                if (pipetteIndex == foodAllergyIndex)
                {
                    if (allergicReaction)
                    {
                        // Disable all reactions first
                        for (int i = 0; i < allergicReaction.childCount; i++)
                        {
                            allergicReaction.GetChild(i).gameObject.SetActive(false);
                        }

                        // Enable the correct allergic reaction
                        allergicReaction.gameObject.SetActive(true);
                        Transform reaction = allergicReaction.GetChild(foodAllergyIndex);
                        reaction.position = foodStickers[pipetteIndex].position-new Vector3(0f,1f,0f);
                        reaction.gameObject.SetActive(true);
                        reaction.DOScale(0, 0).OnComplete(() => {
                            reaction.DOScale(1, 0.5f).SetEase(Ease.OutElastic);
                        });
                    }
                }

                // Return pipette to tray and disable
                draggedObject.DOMove(draggedObject.GetComponent<DraggableObject>().originalPosition, 0.5f);
                draggedObject.DORotate(draggedObject.GetComponent<DraggableObject>().originalRotation.eulerAngles, 0.5f)
                    .OnComplete(() => {
                        draggedObject.GetComponent<DraggableObject>().enabled = false;
                        draggedObject.GetComponent<BoxCollider2D>().enabled = false;
                    });

                testsPerfomed++;
                if (testsPerfomed >= testPipettes.Length)
                {
                    DOVirtual.DelayedCall(2f, ShowResults);
                }
            });
        }
        else
        {
            // Return pipette if not placed correctly
            draggedObject.DOMove(draggedObject.GetComponent<DraggableObject>().originalPosition, 0.5f);
            draggedObject.DORotate(draggedObject.GetComponent<DraggableObject>().originalRotation.eulerAngles, 0.5f);
        }
    }



    void ShowResults()
    {
        // Hide pipette tray
        Tray.DOMove(new Vector3(Tray.position.x + 10f, Tray.position.y, Tray.position.z), 1f)
            .SetEase(Ease.InBack);
        SoundManager.instance.PlayOhNoYouHaveAllergy();
        // Show results folder sliding in
        if (resultsFolder)
        {
            resultsFolder.gameObject.SetActive(true);
            Vector3 startPos = resultsFolder.position;
            resultsFolder.position = new Vector3(startPos.x + 10f, startPos.y, startPos.z);
            resultsFolder.DOMove(startPos, 1f).SetEase(Ease.OutBack);
        }

        // Show results one by one
        Sequence resultsSequence = DOTween.Sequence();

        for (int i = 0; i < resultIcons.Length; i++)
        {
            int index = i; // Capture index for closure
            resultsSequence.AppendCallback(() => {
                if (resultIcons[index])
                {
                    bool isAllergic = (index == foodAllergyIndex);

                    // Find the corresponding check and cross objects
                    Transform checkMark = resultsFolder.Find($"CheckMark_{foodStickers[index].name}");
                    Transform crossMark = resultsFolder.Find($"CrossMark_{foodStickers[index].name}");

                    // Enable the correct one
                    if (checkMark) checkMark.gameObject.SetActive(!isAllergic);
                    if (crossMark) crossMark.gameObject.SetActive(isAllergic);

                    // Animate appearance
                    Transform activeMark = isAllergic ? crossMark : checkMark;
                    if (activeMark)
                    {
                        activeMark.localScale = Vector3.zero;
                        activeMark.DOScale(1, 0.5f).SetEase(Ease.OutBack);
                    }
                }
            });

            // Add delay between showing each result
            resultsSequence.AppendInterval(0.3f);
        }

        // After showing all results, wait then complete step
        //resultsSequence.AppendInterval(2f);
        resultsSequence.AppendCallback(() => {
            HideAllStep2Elements();
        });
    }



    void HideAllStep2Elements()
    {
        // Hide all step 2 elements with animation
        if (Step2Gameobject) Step2Gameobject.DOMove(new Vector3(Step2Gameobject.position.x, Step2Gameobject.position.y - 10f, Step2Gameobject.position.z), 1f)
            .SetEase(Ease.InBack).OnComplete(() => {
                Step2Gameobject.gameObject.SetActive(false);
                isStep2Complete = true;
                remainingGameobjects.gameObject.SetActive(true);
                // Start step 3
                StartStep3();
            });
    }

    #endregion

    #region Step 3 - Give the Pill
    void StartStep3()
    {
        if (DeviceResForResolution.Instance.isIpad)
        {
            zoomVal = 4f;

        }
        else
        {
            zoomVal = 4;
        }
        float zoomDuration1 = 0.5f;
        camera.DOOrthoSize(zoomVal, zoomDuration1).SetEase(Ease.InOutSine);
        if (isIpad)
        camera.transform.position = new Vector3(1, 0, -10);
        //characterObj.transform.position = new Vector3(characterObj.transform.position.x + 4f, characterObj.transform.position.y - 4, characterObj.transform.position.z);
        //make local scale 0.4f
        //characterObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.4f);
        if (DeviceResForResolution.Instance.isIpad)
        {
            patChair.transform.position = patChairPoss.position;
            patChair.transform.DOScale(1.3f, 0.5f);

        }
        // If we're coming from Step 2 (skipping Step 3), make sure to deactivate Step 2
        if (Step2Gameobject.gameObject.activeInHierarchy)
        {
            Step2Gameobject.gameObject.SetActive(false);
            
        }
        float targetSize = 5f; // Adjust this to the desired zoom level
        float zoomDuration = 2f; // Adjust duration for smooth effect
        if (isIphone)
            targetSize = 4f;
        camera.DOOrthoSize(targetSize, zoomDuration).SetEase(Ease.InOutSine);
        Step3Gameobject.gameObject.SetActive(true);

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
            catMouthOpen.GetComponent<SpriteRenderer>().enabled = true;

            // Animate pill going into cat's mouth
            draggedObject.DOMove(catMouthOpen.position, 0.5f).OnComplete(() => {
                draggedObject.DOScale(0, 0.3f).SetEase(Ease.InBack);

                // Close cat's mouth
                DOVirtual.DelayedCall(0.5f, () => {
                    catMouthOpen.gameObject.SetActive(false);
                    catMouth.gameObject.SetActive(true);
                    catMouthOpen.GetComponent<SpriteRenderer>().enabled = true;
                    catMouth.GetComponent<SpriteRenderer>().enabled = true;
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
        if (Vector3.Distance(position, catMouth.position) < 2f)
        {
            SoundManager.instance.PlayPouring();
            draggedObject.GetComponent<BoxCollider2D>().enabled = false;
            print("Amigo 0");
            Vector3 drinkPosition = Vector3.one;
            drinkPosition = catMouth.position + new Vector3(1.75f, 0.25f, 0);
            //if (isIphone)
            //    drinkPosition = catMouth.position + new Vector3(1.75f, 0.25f, 0);
            //else
            //    drinkPosition = catMouth.position + new Vector3(0.8f, 0.25f, 0);

            // ✅ Disable dragging component to prevent override
            if (draggedObject.TryGetComponent(out DraggableObject dragComponent))
            {
                dragComponent.enabled = false;
            }

            // ✅ Immediately snap position and rotation
            draggedObject.position = drinkPosition;
            //draggedObject.rotation = Quaternion.Euler(0, -0.2f, 0); // or keep original if needed
            print("Amigo 0: " + "drinkPosition: " + draggedObject.position);

            // Show cat drinking animation
            catMouth.gameObject.SetActive(false);
            catMouthOpen.gameObject.SetActive(true);

            // Create a sequence for glass animation
            Sequence drinkingSequence = DOTween.Sequence();
            print("Amigo 1");
            // Move to same position again for safety
            drinkingSequence.Append(draggedObject.DOMove(drinkPosition, 0.01f));
            drinkingSequence.AppendInterval(0.1f);
            //drinkingSequence.Append(catDrinkingWater.DOLocalMove(new Vector3(-9.03749847f, 4.94285583f, 0), 0.02f));
            //drinkingSequence.AppendInterval(0.1f);
            //drinkingSequence.Append(catDrinkingWater.DOLocalRotate(new Vector3(0, 0, 30), 0.1f));

            print("Amigo 2: " + "drinkPosition: " + drinkPosition);

            // Tilting animation - rotate the glass
            drinkingSequence.Append(draggedObject.DOLocalRotate(new Vector3(0, 0, 60f), 1f));

            GameObject[] waterFrames = new GameObject[9];
            for (int i = 1; i <= 9; i++)
            {
                Transform child = catDrinkingWater.transform.Find(i.ToString());
                if (child != null)
                {
                    waterFrames[i - 1] = child.gameObject;

                    // === FIX #2: Apply sorting order ===
                    SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.sortingOrder = 200;
                }
            }

            drinkingSequence.AppendCallback(() =>
            {
                //catDrinkingWater.transform.position = draggedObject.position;
                //catDrinkingWater.transform.rotation = draggedObject.rotation;
                print("Amigo 3: " + "drinkPosition: " + drinkPosition);

                for (int i = 1; i < waterFrames.Length; i++)
                {
                    if (waterFrames[i] != null)
                        waterFrames[i].SetActive(false);
                }

                if (waterFrames[0] != null)
                {
                    waterFrames[0].transform.position = draggedObject.position;
                    waterFrames[0].transform.rotation = draggedObject.rotation;
                    waterFrames[0].SetActive(true);
                }
            });

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

                draggedObject.gameObject.SetActive(false);
                waterGlass.gameObject.SetActive(false);

                emptyGlass.transform.position = draggedObject.position;
                emptyGlass.transform.rotation = draggedObject.rotation;
                emptyGlass.gameObject.SetActive(true);
            });

            drinkingSequence.Append(emptyGlass.DOLocalRotate(Vector3.zero, 1f));
            drinkingSequence.OnComplete(() =>
            {
                // Hide mouth and reset glass
                catDrinkingWater.DOLocalRotate(Vector3.zero, 0.5f);
                emptyGlass.DOMove(waterGlassTray.position + new Vector3(0, 1f, 0), 0.5f);
                catMouthOpen.gameObject.SetActive(false);
                catMouth.gameObject.SetActive(true);

                // 🔲 FADE TO BLACK
                screenFade.FadeIn(0.5f);

                DOVirtual.DelayedCall(0.5f, () =>
                {
                    HideAllStep3Elements();

                    DOVirtual.DelayedCall(1f, () =>
                    {
                        Step3Gameobject.gameObject.SetActive(false);
                        catHappyMouth.gameObject.SetActive(true);
                        catHappyMouth.GetComponent<SpriteRenderer>().enabled = true;

                        patChairPos.transform.DOMoveX(patChairPos.transform.position.x - 10f, 0.2f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            doctorObjects.SetActive(true);
                            Vector3 originalPos = doctorObjects.transform.position;
                            doctorObjects.transform.position = originalPos + new Vector3(5f, 0, 0);
                            doctorObjects.transform.DOMoveX(originalPos.x, 0.5f).SetEase(Ease.OutBack);

                            characterObj.transform.localScale = Vector3.zero;
                            characterObj.transform.position = new Vector3(
                                characterObj.transform.position.x - 4f,
                                characterObj.transform.position.y + 2f,
                                characterObj.transform.position.z
                            );

                            patChair.SetActive(true);

                            characterObj.transform.DOScale(new Vector3(0.25f, 0.25f, 0.4f), 0.5f).SetEase(Ease.OutBack);

                            characterAnimator.Play("Happy");
                            catMouth.gameObject.SetActive(false);

                            // ✅ FADE OUT TO REVEAL HAPPY SCENE
                            screenFade.FadeOut(0.5f);

                            SoundManager.instance.PlayDone();
                            SoundManager.instance.PlayWellDone();

                            DOVirtual.DelayedCall(2f, () =>
                            {
                                catLowerBodyPart.gameObject.SetActive(true);
                                CompleteLevel();
                            });
                        });
                    });
                });
            });
        }
        else
        {
            draggedObject.DOMove(waterGlassTray.position + new Vector3(0, 1f, 0), 0.5f);
        }
    }
    [SerializeField] private ScreenFade screenFade;


    void HideAllStep3Elements()
    {
        // Hide all remaining elements
        if (Step3Gameobject) Step3Gameobject.DOMove(new Vector3(Step3Gameobject.position.x, Step3Gameobject.position.y - 10f, Step3Gameobject.position.z), 1f).SetEase(Ease.InBack);
        //if (pillTray) pillTray.DOMove(new Vector3(pillTray.position.x, pillTray.position.y - 10f, pillTray.position.z), 1f).SetEase(Ease.InBack);
        //if (waterGlassTray) waterGlassTray.DOMove(new Vector3(waterGlassTray.position.x, waterGlassTray.position.y - 10f, waterGlassTray.position.z), 1f).SetEase(Ease.InBack);
    }
    #endregion
    public void CompleteLevel()
    {
        screenFade.FadeIn(0.5f); // Fade to black

        DOVirtual.DelayedCall(0.6f, () =>
        {
            SoundManager.instance.PlayDone();

            if (StarProgressManager.Instance != null)
            {
                StarProgressManager.Instance.AddCuredPatient();
                GlobalManager.Instance.hasComeFromTheMainMenu = true;
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
