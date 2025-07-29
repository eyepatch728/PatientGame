using UnityEngine;
using UnityEngine.UI;

public class IrregularButton : MonoBehaviour
{
    public float alphaThreshold = 0.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
    }
}
