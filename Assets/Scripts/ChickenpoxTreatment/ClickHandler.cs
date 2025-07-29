using UnityEngine;
using UnityEngine.Events;

public class ClickHandler : MonoBehaviour
{
    public UnityEvent OnClick = new UnityEvent();

    void OnMouseDown()
    {
        OnClick.Invoke();
    }
}
