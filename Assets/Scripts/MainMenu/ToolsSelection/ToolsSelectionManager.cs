using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsSelectionManager : MonoBehaviour
{
    public List<ToolBoxSlot> slots;
    public GameObject suitcase;
    private bool isToolsCompleted = false;
    public static ToolsSelectionManager Instance;
    [SerializeField] private GameObject suitcaseLid;

    private void Awake()
    {
        Instance = this;
        SoundManager.instance.StopChooseOutFit();    
        SoundManager.instance.PlayMatchTools();
    }

    public void CheckCompletion()
    {
        bool allCorrect = true;

        foreach (ToolBoxSlot slot in slots)
        {
            if (slot.IsPieceCorrectlyPlaced())
            {
                SoundManager.instance.PlayCorrectMatch(); // ✅ Play for every correct one
            }
            else
            {
                allCorrect = false;
            }
        }

        if (allCorrect)
        {
            CompleteToolBox();
        }
    }


    private void CompleteToolBox()
    {
        if (isToolsCompleted) return;
        isToolsCompleted = true;
        suitcase.SetActive(true);

        AnimateSuitcase();
    }

    private void AnimateSuitcase()
    {
        print("Tools Placed Successfully");
        Vector3 targetScale = new Vector3(0.3f, 0.3f, 0.3f);
        //Vector3 targetPosition = GetScreenCenterInWorld();
        Vector3 targetPosition = new Vector3(0f,-4f,0f) ;
        float animationDuration = 1f; // Adjust timing as needed
        float closingDuration = 1f;
        // Create a sequence to manage both animations
        Sequence sequence = DOTween.Sequence();

        // Add scale and position animations to the sequence
        sequence.Join(suitcase.transform.DOScale(targetScale, animationDuration).SetEase(Ease.OutQuad));
        sequence.Join(suitcase.transform.DOLocalMove(targetPosition, animationDuration).SetEase(Ease.OutQuad));

        // Add delay and completion callback
        sequence.AppendInterval(0.3f); // Same as WaitForSeconds(0.3f)

        sequence.Append(CloseSuitcase(closingDuration));

        sequence.AppendInterval(0.5f);
        sequence.OnComplete(() => {
            //SceneManager.LoadSceneAsync("WaitingRoom");
            MenuManager.Instance.ToolsToWaitingRoom();
        });
    }
    private Tween CloseSuitcase(float duration)
    {
        if (suitcaseLid != null)
        {
            // Create a sequence to handle both the rotation and position change
            Sequence lidSequence = DOTween.Sequence();

            // Add rotation animation
            lidSequence.Join(suitcaseLid.transform.DOLocalRotate(new Vector3(180f, 0f, 0f), duration).SetEase(Ease.InOutQuad));

            // Add position animation - adjust the Y value as needed
            Vector3 currentPosition = suitcaseLid.transform.localPosition;
            Vector3 targetPosition = new Vector3(currentPosition.x, currentPosition.y - 1f, currentPosition.z); // Move down by 1 units
            lidSequence.Join(suitcaseLid.transform.DOLocalMove(targetPosition, duration).SetEase(Ease.InOutQuad));
            SoundManager.instance.PlayWellDone();
            return lidSequence;

        }
        else
        {
            // Fallback if suitcaseLid is null
            return DOTween.Sequence().AppendInterval(duration);
        }
    }
    private Vector3 GetScreenCenterInWorld()
    {
        Camera mainCamera = Camera.main;
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, mainCamera.nearClipPlane);
        return mainCamera.ScreenToWorldPoint(screenCenter);
    }

}
