using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;

public class DentalCareProblemManager : MonoBehaviour
{
    [Header("Camera")]
    public Camera camera;

    [Header("Problem Visualization")]
    public Transform[] problemSprites;
    public GameObject sprites;

    public GameObject doctorObjects;
    public Animator characterAnim;
    public Transform[] cavityTeeth;
    public Transform[] foodStains;
    public Transform[] yellowStains;
    public Transform saliva;
    public GameObject DoctorCabin, Mouth, Paitent, DocChair, patientPos, dentalMouthIssues, patChair;
    public float zoomValue;

    public Transform sittingPosForPatient;
    public Transform WalkingStartPosForPatient;
    [Header("Step 1 - Cleaning Tools")]
    public Transform step1Container;
    public Transform smallBrush;
    public Transform smallBrushChild;
    public Transform toothbrush;
    public Transform toothbrushChild;
    public ParticleSystem foamParticles;
    public GameObject[] stainOpacityControls;

    public Transform polisher;
    [Header("Step 2 - Cavity Repair")]
    public Transform step2Container;
    public Transform drillTool;
    public Transform drillToolChild;

    public Transform salivaSucker;
    public Transform salivaSuckerChild;

    public Transform fillingMaterial;
    public Transform fillingMaterialChild;

    public Transform polishingTool;
    public Transform polishingToolChild;

    public Image[] cavityProgressIndicators;

    [Header("Step 3 - Final Polish")]
    public Transform uvLight;
    public Transform uvLightChild;
    private bool[] cavityCured;
    public Transform step3Container;
    public Transform rotatingPolisher;
    public Transform rotatingPolisherChild;

    public ParticleSystem sparkleParticles;
    public Transform happyMouth;

    private int currentStain = 0;
    private int currentCavity = 0;
    private bool isBrushing = false;
    private bool isDrilling = false;
    public Transform happyPatient;
    public Transform happyPatientPos;
    public GameObject toothBrush;
    private bool isStep1Complete = false;

    private Vector3 originalCameraPosition;
    private float originalCameraZoom;
    private Vector3 originalDocChairPosition;
    private Vector3 originalDocChairScale;
    private Vector3 originalPatientPosition;

    public Animator fadeInAnim;

    void Start()
    {
        cavityCured = new bool[cavityTeeth.Length];
        CheckDeviceResolution();
        InitializeScene();
        //StartCoroutine(ShowProblemAnimation());
        StartCoroutine(HandleCatEntryThenShowProblem());

        if (isIpad)
        {
            zoomValue = 4f;
        }
        else if (isIphone)
        {
            zoomValue = 5f;
        }
        else
        {
            zoomValue = 3.5f; // Default value for other devices, if any
        }

        originalCameraPosition = Camera.main.transform.position;
        originalCameraZoom = Camera.main.orthographicSize;
        originalDocChairPosition = DocChair.transform.position;
        originalDocChairScale = DocChair.transform.localScale;
        originalPatientPosition = Paitent.transform.position;

        cavityBeingCured = new bool[cavityTeeth.Length];
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
    private void Update()
    {
        if (isIpad)
        {
            zoomValue = 4f;
        }
        else if (isIphone)
        {
            zoomValue = 5f;
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
        characterAnim.gameObject.SetActive(true);
        // Play walking animation
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
                        DOVirtual.DelayedCall(1f, () =>

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
    void InitializeScene()
    {
        step1Container.gameObject.SetActive(false);
        step2Container.gameObject.SetActive(false);
        step3Container.gameObject.SetActive(false);

        foreach (var stain in foodStains)
            stain.gameObject.SetActive(true);

        foreach (var stain in yellowStains)
            stain.gameObject.SetActive(true);


    }

    #region Problem Visualization
    void StartStory()
    {
        AnimateIssue();
    }

    void AnimateIssue()
    {
        sprites.SetActive(true);
        patChair.transform.SetParent(characterAnim.gameObject.transform);
        Sequence problemSequence = DOTween.Sequence();
        DOVirtual.DelayedCall(1f, () =>
        {
            // Move patient to sitting position
        SoundManager.instance.PlayBlabla();

        });
        // Animate problem sprites: scale up, hold, scale down
        foreach (var sprite in problemSprites)
        {
            if (sprite != null)
            {
                sprite.localScale = Vector3.zero;

                // Scale up
                problemSequence.Append(sprite.DOScale(0.6f, 1.5f).SetEase(Ease.OutBack));

                // Hold
                problemSequence.AppendInterval(1f);

                // Scale down
                problemSequence.Append(sprite.DOScale(0f, 0.5f).SetEase(Ease.InBack));
            }
        }

        // Slide out DoctorCabin, DocChair to the right and Paitent to the left
        float slideOutDuration = 1f;
        Vector3 doctorCabinOffset = new Vector3(30f, 0f, 0f);  // Move right
        Vector3 patientOffset = new Vector3(-30f, 0f, 0f);     // Move left

        problemSequence.Append(DoctorCabin.transform.DOMove(DoctorCabin.transform.position + doctorCabinOffset, slideOutDuration).SetEase(Ease.InBack));
        problemSequence.Join(DocChair.transform.DOMove(DocChair.transform.position + doctorCabinOffset, slideOutDuration).SetEase(Ease.InBack));
        problemSequence.Join(Paitent.transform.DOMove(Paitent.transform.position + patientOffset, slideOutDuration).SetEase(Ease.InBack));

        // Then move and zoom the camera
        problemSequence.AppendCallback(() =>
        {
            float zoomDuration = 2f;
            Vector3 targetPosition = new Vector3(0f, 0f, -10f);
            float targetZoom = 4f;

            Sequence camSequence = DOTween.Sequence();
            camSequence.Append(Camera.main.transform.DOMove(targetPosition, zoomDuration).SetEase(Ease.InOutSine));
            camSequence.Join(Camera.main.DOOrthoSize(zoomValue, zoomDuration).SetEase(Ease.InOutSine));
            camSequence.OnComplete(() =>
            {
                // Show the DocChair and move it to center with scale 2
                DocChair.SetActive(true);
                Sequence chairSequence = DOTween.Sequence();
                chairSequence.Append(DocChair.transform.DOMove(Vector3.zero, 0.8f).SetEase(Ease.OutBack));
                chairSequence.Join(DocChair.transform.DOScale(Vector3.one * 2f, 0.8f).SetEase(Ease.OutBack));
                chairSequence.OnComplete(() =>
                {
                    // Bring patient back to screen
                    dentalMouthIssues.SetActive(true);
                    dentalMouthIssues.transform.position = new Vector3(-30f, patientPos.transform.position.y, patientPos.transform.position.z); // Start far left
                    dentalMouthIssues.transform.DOMove(patientPos.transform.position, 0.8f).SetEase(Ease.OutBack).OnComplete(() =>
                    {
                        isStep1Complete = true;
                        StartStep1();

                        // Show/hide objects
                        Mouth.SetActive(true);

                        foreach (var sprite in problemSprites)
                        {
                            sprite.gameObject.SetActive(false);
                        }
                    });
                });
            });
        });
    }




    #endregion


    //IEnumerator ShowProblemAnimation()
    //{
    //    // Show dental problem animation
    //    foreach (var sprite in problemSprites)
    //    {
    //        sprite.localScale = Vector3.zero;
    //        sprite.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
    //        yield return new WaitForSeconds(0.3f);
    //    }

    //    yield return new WaitForSeconds(1f);

    //    // Transition to treatment
    //    foreach (var sprite in problemSprites)
    //    {
    //        sprite.DOScale(0, 0.5f).SetEase(Ease.InBack);
    //    }

    //    StartStep1();
    //}

    #region Step 1 - Cleaning Food Residues
    void StartStep1()
    {
        step1Container.gameObject.SetActive(true);

        // Slide in small brush from right
        smallBrush.position += Vector3.right * 10f;
        SoundManager.instance.PlayObjectPlaced();
        smallBrush.DOMoveX(smallBrush.position.x - 10f, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                smallBrush.GetComponent<DraggableObject>().OnDragEvent += OnSmallBrushDrag;
                smallBrush.GetComponent<DraggableObject>().OnDragStartEvent += OnSmallBrushDragStart;
                smallBrush.GetComponent<DraggableObject>().OnDragEndEvent += OnSmallBrushDragEnd;

            });
        saliva.gameObject.SetActive(true);
        StartCoroutine(ScaleUpSaliva());
    }
    void OnSmallBrushDragStart(Transform draggedObject, Quaternion rotation)
    {
        SoundManager.instance.sfxSource.loop = true; // Set loop to true for continuous sound
        SoundManager.instance.PlayDentalCleaning();
    }
    void OnSmallBrushDrag(Transform brush, Vector3 position)
    {
        // Check if brush child is touching any stains
        foreach (var stain in foodStains)
        {
            if (stain.gameObject.activeSelf && Vector3.Distance(smallBrushChild.position, stain.position) < 1f)
            {
                if (!isBrushing)
                {
                    isBrushing = true;
                    StartCoroutine(RemoveStain(stain));
                }
            }
        }
    }
    void OnSmallBrushDragEnd(Transform brush, Vector3 position)
    {
        SoundManager.instance.sfxSource.loop = false; // Set loop to true for continuous sound
        SoundManager.instance.StopDentalCleaning();

    }

    IEnumerator RemoveStain(Transform stain)
    {
        if (stain == null)
        {
            Debug.LogError("Stain is null!");
            yield break;
        }

        SpriteRenderer stainRenderer = stain.GetComponent<SpriteRenderer>();
        if (stainRenderer == null)
        {
            Debug.LogError($"Missing SpriteRenderer component on {stain.name}");
            yield break;
        }

        float opacity = 1f;
        while (opacity > 0f)
        {
            opacity -= Time.deltaTime * 0.5f;
            stainRenderer.color = new Color(1, 1, 1, opacity);

            // Smoothly move the brush toward the stain center
            Vector3 offset = new Vector3(0f, -1f, 0); // change values as needed
            smallBrush.position = Vector3.MoveTowards(smallBrush.position, stain.position + offset, Time.deltaTime * 2f);


            yield return null;
        }

        stainRenderer.color = new Color(1, 1, 1, 0); // Fully transparent
        stain.gameObject.SetActive(false);
        currentStain++;
        isBrushing = false;

        if (currentStain >= foodStains.Length)
        {
            FinishSmallBrushCleaning();
        }
    }



    void FinishSmallBrushCleaning()
    {
        smallBrush.GetComponent<DraggableObject>().enabled = false; // Unsubscribe from event

        smallBrush.GetComponent<BoxCollider2D>().enabled = false; // Unsubscribe from event

        smallBrush.DOMoveX(smallBrush.position.x + 10f, 1f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                smallBrush.gameObject.SetActive(false);
                StartToothbrushCleaning();
                //StartUVLightStep();
            });

    }

    void StartToothbrushCleaning()
    {
        toothbrush.gameObject.SetActive(true);

        float startX = toothbrush.position.x;

        toothbrush.position = new Vector3(startX + 10f, toothbrush.position.y, toothbrush.position.z);
        SoundManager.instance.PlayWhoosh();

        toothbrush.DOMoveX(startX, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                var draggable = toothbrush.GetComponent<DraggableObject>();
                draggable.OnDragStartEvent += OnToothbrushDragStart;
                draggable.OnDragEvent += OnToothbrushDrag;
                draggable.OnDragEndEvent += OnToothbrushDragEnd;
            });
    }
    void OnToothbrushDragStart(Transform draggedObject, Quaternion rotation)
    {
        toothbrush.DORotate(new Vector3(0, 0, 90), 0.2f).SetEase(Ease.OutBack);
        SoundManager.instance.sfxSource.loop = true; // Set loop to true for continuous sound
        SoundManager.instance.PlayDentalCleaning();
    }
    void OnToothbrushDragEnd(Transform draggedObject, Vector3 position)
    {
        toothbrush.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
        foamParticles.Stop(); // stop bubbles when drag ends
        SoundManager.instance.sfxSource.loop = false ; // Set loop to true for continuous sound
        SoundManager.instance.StopDentalCleaning();
    }
    

    void OnToothbrushDrag(Transform brush, Vector3 position)
    {
        foreach (var stain in yellowStains)
        {
            if (stain.gameObject.activeSelf && Vector3.Distance(toothbrushChild.position, stain.position) < 1f)
            {
                if (!isBrushing)
                {
                    isBrushing = true;
                    if (!foamParticles.isPlaying)
                        foamParticles.Play();
                    StartCoroutine(RemoveYellowStain(stain));
                }
            }
        }
    }



    IEnumerator RemoveYellowStain(Transform stain)
    {
        if (stain == null)
        {
            Debug.LogError("Stain is null!");
            yield break;
        }

        SpriteRenderer stainRenderer = stain.GetComponent<SpriteRenderer>();
        if (stainRenderer == null)
        {
            Debug.LogError($"Missing SpriteRenderer component on {stain.name}");
            yield break;
        }

        float opacity = 1f;
        while (opacity > 0f)
        {
            opacity -= Time.deltaTime * 0.7f;
            stainRenderer.color = new Color(1, 1, 1, opacity);

            Vector3 offset = new Vector3(0f, -1f, 0); // change values as needed

            toothbrush.position = Vector3.MoveTowards(toothbrush.position, stain.position + offset, Time.deltaTime * 2f);

            yield return null;
        }

        stainRenderer.color = new Color(1, 1, 1, 0);
        stain.gameObject.SetActive(false);
        currentStain++;
        isBrushing = false;

        if (currentStain >= foodStains.Length + yellowStains.Length)
        {
            foamParticles.Stop();
            print("yellow stain removed");
            FinishStep1();
        }
    }






    void FinishStep1()
    {
        toothbrush.DOMoveX(toothbrush.position.x + 10f, 1f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                //toothBrush.SetActive(false);
                print("starting step 2");
                StartStep2();
                print("Start Step 2");
                step1Container.gameObject.SetActive(false);
            });
    }
    #endregion

    #region Step 2 - Cavity Repair
    void StartStep2()
    {
        step2Container.gameObject.SetActive(true);

        drillTool.gameObject.SetActive(true); // ⬅️ make sure it's active
        Vector3 startPos = drillTool.position + Vector3.right * 18f;
        drillTool.position = startPos;
        SoundManager.instance.PlayWhoosh();
        drillTool.DOMoveX(startPos.x - 18f, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                drillTool.GetComponent<DraggableObject>().OnDragEvent += OnDrillToolDrag;
                drillTool.GetComponent<DraggableObject>().OnDragStartEvent += OnDrillToolDragStart;
                drillTool.GetComponent<DraggableObject>().OnDragEndEvent += OnDrillToolDragEnd;

            });
    }

    void OnDrillToolDragStart(Transform draggedObject, Quaternion rotation)
    {
        
        SoundManager.instance.sfxSource.loop = true; // Enable loop for continuous drill sound
        SoundManager.instance.PlayDentalCleaning(); // Replace with actual drill sound method
    }

    void OnDrillToolDragEnd(Transform draggedObject, Vector3 position)
    {
        
        SoundManager.instance.sfxSource.loop = false; // Stop looping the sound
        SoundManager.instance.PlayDentalCleaning(); // Replace with actual stop method
    }


    void OnDrillToolDrag(Transform tool, Vector3 position)
    {
        // Check if drillToolChild is touching any cavities
        for (int i = 0; i < cavityTeeth.Length; i++)
        {
            if (cavityTeeth[i].gameObject.activeSelf &&
                Vector3.Distance(drillToolChild.position, cavityTeeth[i].position) < 0.2f)
            {
                if (!isDrilling)
                {
                    isDrilling = true;
                    StartCoroutine(RepairCavity(i));
                }
            }
        }
    }


    IEnumerator RepairCavity(int cavityIndex)
    {
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * 1.5f;

            // Move drillTool directly toward cavity (no offset)
            Vector3 offset = new Vector3(0f, -1.5f, 0); // change values as needed

            drillTool.position = Vector3.MoveTowards(drillTool.position, cavityTeeth[cavityIndex].position + offset, Time.deltaTime * 3f);

            yield return null;
        }

        // Change cavity color to yellow
        SpriteRenderer cavityRenderer = cavityTeeth[cavityIndex].GetComponent<SpriteRenderer>();
        if (cavityRenderer != null)
        {
            cavityRenderer.color = new Color(0.95f, 0.85f, 0.55f, 1f); // Soft yellow
        }
        else
        {
            Debug.LogWarning($"Missing SpriteRenderer on cavityTeeth[{cavityIndex}]");
        }

        currentCavity++;
        isDrilling = false;

        if (AreAllCavitiesRepaired())
        {
            FinishCavityRepair();
        }
    }



    bool AreAllCavitiesRepaired()
    {
        Color targetColor = new Color(0.95f, 0.85f, 0.55f, 1f);
        float tolerance = 0.01f;

        foreach (var tooth in cavityTeeth)
        {
            SpriteRenderer sr = tooth.GetComponent<SpriteRenderer>();
            if (sr == null || !ColorsApproximatelyEqual(sr.color, targetColor, tolerance))
            {
                return false;
            }
        }
        return true;
    }

    bool ColorsApproximatelyEqual(Color a, Color b, float tolerance)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance &&
               Mathf.Abs(a.a - b.a) < tolerance;
    }




    void FinishCavityRepair()
    {
        // Get the current position first
        drillTool.GetComponent<BoxCollider2D>().enabled = false;
        float targetX = drillTool.position.x + 18f;
        
        drillTool.DOMoveX(targetX, 1f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                drillTool.gameObject.SetActive(false);
                //drillTool.gameObject.SetActive(false); // Disable after animation
                StartSalivaRemoval(); // Move to next step
            });
    }
    private bool hasStartedSalivaRemoval = false;
    void StartSalivaRemoval()
    {
        if (hasStartedSalivaRemoval) return; // ✅ Prevent duplicate call
        hasStartedSalivaRemoval = true;

        salivaSucker.gameObject.SetActive(true);

        Vector3 startPos = salivaSucker.position + Vector3.right * 10f;
        salivaSucker.position = startPos;

        salivaSucker.DOMoveX(startPos.x - 10f, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                SoundManager.instance.PlayWhoosh();
                salivaSucker.GetComponent<DraggableObject>().OnDragEvent += OnSalivaSuckerDrag;
            });
    }

    bool hasRemovedSaliva = false; // Add this as a class-level variable
    IEnumerator ScaleUpSaliva()
    {
        Vector3 targetScale = new Vector3(1.5f, 1.5f, 1f);
        float duration = 40f; // Duration in seconds, adjust as needed

        saliva.localScale = Vector3.zero; // Start from 0 scale

        saliva.DOScale(targetScale, duration).SetEase(Ease.Linear);

        yield return new WaitForSeconds(duration);

        // Now it's fully scaled to 1.5
    }
    public ParticleSystem salivaSuckerBubbles;
    bool isSucking = false;
    void OnSalivaSuckerDrag(Transform tool, Vector3 position)
    {
        if (hasRemovedSaliva) return;

        float distance = Vector3.Distance(salivaSuckerChild.position, saliva.position);

        if (saliva.gameObject.activeSelf && distance < 1.5f)
        {
            // Start sound and bubbles only once
            if (!isSucking)
            {
                SoundManager.instance.PlayPouring(); // Play once
                salivaSuckerBubbles.Play();          // Start once
                print("BubblesWorking");
                isSucking = true;
            }

            SpriteRenderer sr = saliva.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color currentColor = sr.color;
                currentColor.a -= Time.deltaTime * 0.2f; // Fade saliva
                sr.color = currentColor;

                if (currentColor.a <= 0f)
                {
                    saliva.gameObject.SetActive(false);
                    hasRemovedSaliva = true;

                    // Stop effects
                    salivaSuckerBubbles.Stop();
                    SoundManager.instance.StopPouring();
                    isSucking = false;

                    // Slide tool out
                    float targetX = salivaSucker.position.x + 18f;
                    salivaSucker.DOMoveX(targetX, 1f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            salivaSucker.gameObject.SetActive(false);
                            StartCavityFill(); // Next step
                        });
                }
            }
            else
            {
                Debug.LogWarning("Saliva object is missing a SpriteRenderer!");
            }
        }
        else
        {
            // If tool moves out of range before saliva is gone
            if (isSucking)
            {
                salivaSuckerBubbles.Stop();
                SoundManager.instance.StopPouring();
                isSucking = false;
            }
        }
    }


    void StartCavityFill()
    {
        fillingMaterial.gameObject.SetActive(true); // Make sure it's visible

        // Move it off-screen to the right
        Vector3 startPos = fillingMaterial.position + Vector3.right * 18f;
        fillingMaterial.position = startPos;

        // Animate it sliding in
        fillingMaterial.DOMoveX(startPos.x - 18f, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                fillingMaterial.GetComponent<DraggableObject>().OnDragEvent += OnCavityDrillToolDrag;
                fillingMaterial.GetComponent<DraggableObject>().OnDragStartEvent += OnCavityFillingDragStart;
                fillingMaterial.GetComponent<DraggableObject>().OnDragEndEvent += OnCavityFillingDragEnd;

            });
    }
    void OnCavityDrillToolDrag(Transform tool, Vector3 position)
    {
        // Check if drill is touching any cavities
        for (int i = 0; i < cavityTeeth.Length; i++)
        {
            if (cavityTeeth[i].gameObject.activeSelf &&
                Vector3.Distance(fillingMaterialChild.position, cavityTeeth[i].position) < 0.2f)
            {
                if (!isDrilling)
                {
                    isDrilling = true;
                    StartCoroutine(FillCavity(i));
                }
            }
        }
    }

    void OnCavityFillingDragStart(Transform draggedObject, Quaternion rotation)
    {
        // Optional rotation or visual effect
       

        // Enable looping sound for filling process
        SoundManager.instance.sfxSource.loop = true;
        SoundManager.instance.PlayDentalCleaning(); // Replace with your actual filling sound method
    }

    void OnCavityFillingDragEnd(Transform draggedObject, Vector3 position)
    {
        // Reset rotation
        fillingMaterial.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutQuad);

        // Stop looping sound
        SoundManager.instance.sfxSource.loop = false;
        SoundManager.instance.StopDentalCleaning(); // Replace with your actual stop method
    }

    IEnumerator FillCavity(int cavityIndex)
    {
        float progress = 0f;

        // No need for offset now, just wait to simulate the filling time
        while (progress < 1f)
        {
            progress += Time.deltaTime * 0.8f; // Adjust fill speed
            yield return null;
        }

        // ✅ Change color to yellow (filling completed)
        SpriteRenderer cavityRenderer = cavityTeeth[cavityIndex].GetComponent<SpriteRenderer>();
        if (cavityRenderer != null)
        {
            cavityRenderer.color = Color.yellow;
        }
        else
        {
            Debug.LogWarning($"Missing SpriteRenderer on cavityTeeth[{cavityIndex}]");
        }

        currentCavity++;
        isDrilling = false;

        // ✅ Check if all cavities are filled
        if (AreAllCavitiesFilled())
        {
            FinishCavityFill();
        }
    }

    bool AreAllCavitiesFilled()
    {
        foreach (var tooth in cavityTeeth)
        {
            SpriteRenderer sr = tooth.GetComponent<SpriteRenderer>();
            if (sr == null || sr.color != Color.yellow)
            {
                return false;
            }
        }
        return true;
    }
    void FinishCavityFill()
    {
        float targetX = fillingMaterial.position.x + 18f;
        fillingMaterial.GetComponent<BoxCollider2D>().enabled = false; // Unsubscribe from event
        fillingMaterial.DOMoveX(targetX, 1f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                fillingMaterial.gameObject.SetActive(false); // Disable after animation
                // fillingMaterial.gameObject.SetActive(false); // Disable after exit
                StartUVLightStep(); // Proceed to next step
            });
    }
    void StartPolishingTool()
    {
        polishingTool.gameObject.SetActive(true); // Make sure it's visible
        Vector3 startPos = polishingTool.position + Vector3.right * 18f;
        polishingTool.position = startPos;

        polishingTool.DOMoveX(startPos.x - 18f, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                polishingTool.GetComponent<DraggableObject>().OnDragEvent += OnUVLightDrag;
            });
    }
    bool AreAllCavitiesCured()
    {
        Color curedColor = new Color(1f, 0.82f, 0.61f); // Light skin color
        foreach (var tooth in cavityTeeth)
        {
            SpriteRenderer sr = tooth.GetComponent<SpriteRenderer>();
            if (sr == null || sr.color != curedColor)
            {
                return false;
            }
        }
        return true;
    }

    private bool hasStartedUVLightStep = false;

    void StartUVLightStep()
    {
        if (hasStartedUVLightStep) return; // ✅ Prevent multiple triggers
        hasStartedUVLightStep = true;

        uvLight.gameObject.SetActive(true);

        Vector3 startPos = uvLight.position + Vector3.right * 18f;
        uvLight.position = startPos;

        uvLight.DOMoveX(startPos.x - 18f, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                uvLight.GetComponent<DraggableObject>().OnDragEvent += OnUVLightDrag;
            });
    }





    void OnUVLightDrag(Transform tool, Vector3 position)
    {
        Vector3 actualUVPosition = uvLightChild.position;

        for (int i = 0; i < cavityTeeth.Length; i++)
        {
            if (!cavityCured[i] &&
                !cavityBeingCured[i] &&
                cavityTeeth[i].gameObject.activeSelf &&
                Vector3.Distance(actualUVPosition, cavityTeeth[i].position) < 0.5f)
            {
                cavityBeingCured[i] = true;
                StartCoroutine(CureWithUVLightAll());
            }
        }
    }



    bool[] cavityBeingCured;
    public GameObject UVlight;
    IEnumerator CureWithUVLightAll()
    {
        float[] progress = new float[cavityTeeth.Length];
        Debug.Log("[UV Light] Simultaneous cavity curing started");

        while (true)
        {
            bool allCured = true;
            bool isCloseToAnyCavity = false;

            for (int i = 0; i < cavityTeeth.Length; i++)
            {
                if (!cavityCured[i] && cavityTeeth[i].gameObject.activeSelf)
                {
                    float distance = Vector3.Distance(uvLightChild.position, cavityTeeth[i].position);

                    if (distance < 0.5f)
                    {
                        isCloseToAnyCavity = true;
                    }

                    if (distance < 0.8f)
                    {
                        progress[i] += Time.deltaTime * 0.5f;

                        if (progress[i] >= 1f)
                        {
                            SpriteRenderer sr = cavityTeeth[i].GetComponent<SpriteRenderer>();
                            if (sr != null)
                            {
                                sr.color = new Color(1f, 0.82f, 0.61f); // cured color
                            }

                            cavityCured[i] = true;
                            Debug.Log($"[UV Light] Cavity {i} cured!");
                        }
                    }

                    if (!cavityCured[i])
                    {
                        allCured = false;
                    }
                }
            }

            // Turn UV light ON if near any cavity, OFF otherwise
            UVlight.SetActive(isCloseToAnyCavity);

            if (allCured)
            {
                Debug.Log("[UV Light] All cavities cured. Finishing UV step...");
                UVlight.SetActive(false);
                FinishUVLightStep();
                yield break;
            }

            yield return null;
        }
    }



    void OnDrawGizmos()
    {
        if (uvLightChild != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(uvLightChild.position, 0.5f); // Show detection radius
        }
    }
    bool AreAllCavitiesCuredForUV()
    {
        foreach (bool cured in cavityCured)
        {
            if (!cured) return false;
        }
        return true;
    }


    void FinishUVLightStep()
    {
        uvLight.DOMoveX(uvLight.position.x + 18f, 1f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                print("Heelooo");
                uvLight.gameObject.SetActive(false);
                StartRotatingPolisherStep(); // Move to next step
            });
    }

    void FinishStep2()
    {
        salivaSucker.DOMoveX(salivaSucker.position.x + 18f, 1f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                salivaSucker.gameObject.SetActive(false); // ⬅️ deactivate AFTER moving
                StartStep3();
            });
    }

    public Animator brushAnimator;
    Tween rotationTween;
    void StartRotatingPolisherStep()
    {
        polisher.gameObject.SetActive(true);

        Vector3 targetPos;

        if (DeviceResForResolution.Instance.isIpad)
            targetPos = new Vector3(3f, polisher.position.y, polisher.position.z);
        else
            targetPos = new Vector3(6f, polisher.position.y, polisher.position.z);

        polisher.position = targetPos + Vector3.right * 18f;

        polisher.DOMoveX(targetPos.x, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                var draggable = polisher.GetComponent<DraggableObject>();
                draggable.OnDragStartEvent += OnPolisherDragStart;
                draggable.OnDragEndEvent += OnPolisherDragEnd;
                draggable.OnDragEvent += OnPolisherDrag1;

                CheckPolisherAndCavity();
            });
    }

    void OnPolisherDragStart(Transform tool, Quaternion rotation)
    {
        SoundManager.instance.PlayZzz(); // Play once and loop

        if (rotationTween == null || !rotationTween.IsActive())
        {
            rotationTween = polisher.GetChild(1).DORotate(
                new Vector3(0, 0, 360f), 2f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear);
        }
    }

    void OnPolisherDrag1(Transform tool, Vector3 position)
    {
        // No need to play sound here repeatedly
    }

    void OnPolisherDragEnd(Transform tool, Vector3 position)
    {
        SoundManager.instance.StopZzz();

        if (rotationTween != null && rotationTween.IsActive())
        {
            rotationTween.Kill();
            polisher.GetChild(1).rotation = Quaternion.identity;
        }
    }

    void StartRotatingPolisher()
    {
        // Start rotating the polisher
        polisher.GetChild(1).DORotate(new Vector3(0, 0, 360f), 2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)  // Infinite loop with incremental rotation
            .SetEase(Ease.Linear);

        // Detect when the polisher touches a cavity
        CheckPolisherAndCavity();
    }

    void CheckPolisherAndCavity()
    {
        // Check if the rotating polisher is near a cavity
        StartCoroutine(PolisherCheck());
    }

    IEnumerator PolisherCheck()
    {
       // SoundManager.instance.PlayZzz();
        float[] polishProgress = new float[cavityTeeth.Length];
        float requiredTime = 1.5f; // Time in seconds to polish one cavity

        while (true)
        {
            for (int i = 0; i < cavityTeeth.Length; i++)
            {
                if (cavityTeeth[i].gameObject.activeSelf)
                {
                    SpriteRenderer cavityRenderer = cavityTeeth[i].GetComponent<SpriteRenderer>();

                    if (cavityRenderer != null && cavityRenderer.color != Color.white)
                    {
                        float distance = Vector3.Distance(polishingToolChild.position, cavityTeeth[i].position);

                        if (distance < 0.5f)
                        {
                            polishProgress[i] += Time.deltaTime;
                            Debug.Log($"[Polisher] Cavity {i} progress: {polishProgress[i]:F2}");
                           // SoundManager.instance.PlayZzz(); // Play polishing sound
                            if (polishProgress[i] >= requiredTime)
                            {
                                cavityRenderer.color = Color.white;
                                Debug.Log($"[Polisher] Cavity {i} polished!");
                            }
                        }
                        else
                        {
                            // Optional: Slowly decay progress if tool moves away
                            polishProgress[i] = Mathf.Max(0, polishProgress[i] - Time.deltaTime * 0.5f);
                        }
                    }
                }
            }

            if (AreAllCavitiesWhite())
            {
                Debug.Log("[Polisher] All cavities polished!");
                FinishPolisherStep();
                break;
            }

            yield return null;
        }
    }




    bool AreAllCavitiesWhite()
    {
        foreach (var tooth in cavityTeeth)
        {
            SpriteRenderer sr = tooth.GetComponent<SpriteRenderer>();
            if (sr == null || sr.color != Color.white)  // Check if any cavity is not white
            {
                return false;  // If any cavity is not white, return false
            }
        }
        return true;  // Return true if all cavities are white
    }
    void FinishPolisherStep()
    {
        polisher.DOMoveX(polisher.position.x + 18f, 1f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                polisher.gameObject.SetActive(false);  // Deactivate polisher after moving
                FinishStep2();
                sparkleParticles.gameObject.SetActive(true);
                StartCoroutine(WaitForParticlesToFinish(sparkleParticles));
                print("gola");// Proceed to next step (or the next appropriate step)
            });
    }
    IEnumerator WaitForParticlesToFinish(ParticleSystem sparkleParticles)
    {
        sparkleParticles.gameObject.SetActive(true);
        sparkleParticles.Play();

        // Wait until the particles are finished
        yield return new WaitUntil(() => sparkleParticles.isStopped);
        //MovePatientOutOfScreen();
        AnimateGameEnd();
        // Do something after particles finish
        Debug.Log("Particles finished!");
    }
    void MovePatientOutOfScreen()
    {
        // Slide out the dentalMouthIssues (patient) to the left
        float slideOutDuration = 1f;
        Vector3 offScreenPosition = new Vector3(-30f, dentalMouthIssues.transform.position.y, dentalMouthIssues.transform.position.z);

        dentalMouthIssues.transform.DOMove(offScreenPosition, slideOutDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                // After the slide out completes, bring the patient back to their original position
                BringThePatientBack();
            });
    }
    public Transform happyPatientForIphone;
    public void BringThePatientBack()
    {
        if (isIpad)
        {
            // Move the dentalMouthIssues (patient) back to the patient position
            happyPatient.transform.DOMove(happyPatientPos.transform.position, 1f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    Debug.Log("Patient is back on screen!");
                    // Optionally, trigger any further actions here after bringing the patient back
                });
        }
        else if (isIphone)
        {
            // Move the dentalMouthIssues (patient) back to the patient position
            happyPatient.transform.DOMove(happyPatientForIphone.transform.position, 1f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    Debug.Log("Patient is back on screen!");
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        AnimateGameEnd();
                    });
                    // Optionally, trigger any further actions here after bringing the patient back
                });
        }

    }

    #endregion

    #region Step 3 - Final Polish
    void StartStep3()
    {
        step3Container.gameObject.SetActive(true);

        // Show rotating polisher
        rotatingPolisher.position += Vector3.right * 10f;
        rotatingPolisher.DOMoveX(rotatingPolisher.position.x - 10f, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                rotatingPolisher.GetComponent<DraggableObject>().OnDragEvent += OnPolisherDrag;
            });
    }

    void OnPolisherDrag(Transform tool, Vector3 position)
    {
        SoundManager.instance.PlayZzz();
        // Check if polisher is near teeth
        if (Vector3.Distance(position, happyMouth.position) < 2f)
        {
            rotatingPolisher.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);

            sparkleParticles.Play();

            // Transition to happy state
            happyMouth.gameObject.SetActive(true);
            happyMouth.localScale = Vector3.zero;
            happyMouth.DOScale(1f, 0.5f).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        if (StarProgressManager.Instance != null)
                        {
                            StarProgressManager.Instance.AddCuredPatient();

                            // The AddCuredPatient method already includes SaveProgress(),
                            // but if you need to call save explicitly elsewhere:
                            // StarProgressManager.Instance.SaveProgress();
                        }
                        else
                        {
                            Debug.LogError("StarProgressManager instance not found!");
                        }
                        fadeInAnim.Play("FadeIn");
                        SceneManager.LoadScene(0);
                    });
                });
        }
    }
    [SerializeField] private ScreenFade screenFade;
    //[SerializeField] private Animator fadeInAnim;

    private IEnumerator FadeIntoStep5Routine()
    {
        screenFade.FadeIn(0.5f);
        yield return new WaitForSeconds(0.6f);

        Sequence endSequence = DOTween.Sequence();

        Vector3 mouthTargetPos = new Vector3(-30f, Mouth.transform.position.y, Mouth.transform.position.z);
        endSequence.Append(Mouth.transform.DOMove(mouthTargetPos, 0.1f).SetEase(Ease.InBack));
        endSequence.Join(dentalMouthIssues.transform.DOMove(new Vector3(-30f, dentalMouthIssues.transform.position.y, dentalMouthIssues.transform.position.z), 0.1f).SetEase(Ease.InBack));
        endSequence.Join(Paitent.transform.DOMove(new Vector3(-30f, Paitent.transform.position.y, Paitent.transform.position.z), 0.1f).SetEase(Ease.InBack));
        endSequence.Join(DocChair.transform.DOMove(DocChair.transform.position + new Vector3(-30f, 0f, 0f), 0.1f).SetEase(Ease.InBack));

        DocChair.SetActive(false);
        patChair.transform.SetParent(doctorObjects.transform);

        endSequence.AppendCallback(() =>
        {
            Mouth.SetActive(false);
            dentalMouthIssues.SetActive(false);
        });

        endSequence.Append(Camera.main.transform.DOMove(originalCameraPosition, 0.1f).SetEase(Ease.InOutSine));
        endSequence.Join(Camera.main.DOOrthoSize(originalCameraZoom, 0.1f).SetEase(Ease.InOutSine));

        Vector3 doctorCabinReturnPos = DoctorCabin.transform.position - new Vector3(30f, 0f, 0f);
        endSequence.Append(DoctorCabin.transform.DOMove(doctorCabinReturnPos, 0.1f).SetEase(Ease.OutBack));
        endSequence.Join(DocChair.transform.DOMove(originalDocChairPosition, 0.1f).SetEase(Ease.OutBack));
        endSequence.Join(Paitent.transform.DOMove(originalPatientPosition, 0.1f).SetEase(Ease.OutBack));

        endSequence.OnComplete(() =>
        {
            characterAnim.Play("Happy");
            SoundManager.instance.PlayWellDone();
            SoundManager.instance.PlayDone();

            screenFade.FadeOut(0.5f);

            DOVirtual.DelayedCall(1.5f, () =>
            {
                fadeInAnim.Play("FadeIn");

                DOVirtual.DelayedCall(3f, () =>
                {
                    SceneManager.LoadScene(0);
                });
            });
        });
    }


    void AnimateGameEnd()
    {
        StartCoroutine(FadeIntoStep5Routine());

        if (StarProgressManager.Instance != null)
        {
            StarProgressManager.Instance.AddCuredPatient();
            GlobalManager.Instance.hasComeFromTheMainMenu = true;
        }
        else
        {
            Debug.LogError("StarProgressManager instance not found!");
        }
    }





    #endregion
}