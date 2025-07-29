using UnityEngine;

public class SpriteDeviceRes : MonoBehaviour
{
    public bool isIpad;
    public bool isIphone;
    public static SpriteDeviceRes Instance;

    private SpriteRenderer spriteRenderer;
    public float aspectRatio;

    public Vector2 sizeForIphone = new Vector2();
    public Vector2 sizeForIpad = new Vector2();

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer component found on this GameObject!");
        }

        CheckDeviceResolution();
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        CheckDeviceResolution();
    }

    void CheckDeviceResolution()
    {
        aspectRatio = (float)Screen.width / Screen.height;
        Debug.Log("Screen Resolution: " + Screen.width + "x" + Screen.height);
        Debug.Log("Aspect Ratio: " + aspectRatio);

        // Reset flags
        isIpad = false;
        isIphone = false;

        // Typical iPhone aspect ratios are around 16:9 or 19.5:9 (newer models)
        if (aspectRatio >= 16f / 9f || Screen.width < 768)
        {
            isIpad = false;
            isIphone = true;
        }
        else
        {
            isIpad = true;
            isIphone = false;
        }

        if (isIpad)
        {
            Debug.Log("Detected an iPad resolution.");
            HandleIpadSprite();
        }
        else if (isIphone)
        {
            Debug.Log("Detected an iPhone resolution.");
            HandleIphoneSprite();
        }
        else
        {
            Debug.Log("Unknown device resolution.");
        }
    }

    void HandleIpadSprite()
    {
        Debug.Log("Applying iPad-specific settings to sprite...");
        if (spriteRenderer != null)
        {
            transform.localScale = new Vector3(sizeForIpad.x, sizeForIpad.y, 1);
        }
    }

    void HandleIphoneSprite()
    {
        Debug.Log("Applying iPhone-specific settings to sprite...");
        if (spriteRenderer != null)
        {
            transform.localScale = new Vector3(sizeForIphone.x, sizeForIphone.y, 1);
        }
    }
}
