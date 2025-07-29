using UnityEngine;
using DG.Tweening;

public class ScreenFade : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public GameObject panel;

    void Awake()
    {
        canvasGroup.alpha = 0; // Start fully transparent
        panel.gameObject.SetActive(false);
    }

    public void FadeIn(float duration = 0.5f)
    {
        canvasGroup.DOFade(1f, duration).SetEase(Ease.InOutQuad);
        panel.gameObject.SetActive(true);
    }

    public void FadeOut(float duration = 0.5f)
    {
        canvasGroup.DOFade(0f, duration).SetEase(Ease.InOutQuad);
        panel.gameObject.SetActive(false);
    }

    public void FadeInOut(float duration = 0.5f, float holdTime = 0.5f)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1f, duration));
        seq.AppendInterval(holdTime);
        seq.Append(canvasGroup.DOFade(0f, duration));
    }
}
