using UnityEngine;

public class PosForIpad : MonoBehaviour
{
    [SerializeField] Vector2 PosForIphone;
    [SerializeField] Vector2 PosForIPad;

    public static PosForIpad Instance;

    public bool isIpad;
    public bool isIphone;

    private Transform objTransform;
    private RectTransform rectTransform;
    private float aspectRatio;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        objTransform = GetComponent<Transform>();
        rectTransform = GetComponent<RectTransform>();
        CheckDeviceResolution();
    }

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

        ApplyPosition();
    }

    void ApplyPosition()
    {
        if (isIpad)
        {
            Debug.Log("Detected an iPad resolution. Applying iPad-specific settings...");
            objTransform.localPosition = new Vector3(PosForIPad.x, PosForIPad.y, transform.position.z);
        }
        else if (isIphone)
        {
            Debug.Log("Detected an iPhone resolution. Applying iPhone-specific settings...");
            objTransform.localPosition = new Vector3(PosForIphone.x, PosForIphone.y, transform.position.z);
        }
        else
        {
            Debug.LogWarning("Unknown device resolution.");
        }
    }
}
