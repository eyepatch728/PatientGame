using UnityEngine;

public class DeviceResForResolution : MonoBehaviour
{
    [SerializeField]public Vector2 sizeForIpad;
    [SerializeField]public Vector2 sizeForIphone;
   
    public bool isIpad;
    public bool isIphone;
    public static DeviceResForResolution Instance;
    public GameObject tutorialBG;
    public float aspectRatio;

    Transform Transform;
    void Start()
    {
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
        //Debug.Log("Screen Resolution: " + Screen.width + "x" + Screen.height);
        //Debug.Log("Aspect Ratio: " + aspectRatio);

        // Reset flags
        isIpad = false;
        isIphone = false;

        // Typical iPad aspect ratio is around 4:3
        //if (Mathf.Approximately(aspectRatio, 4f / 3f) || (Screen.width >= 768 && Screen.height >= 1024))
        //{
        //    isIpad = true;
        //    isIphone = false;
        //}
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
            //Debug.Log("Detected an iPad resolution.");
            HandleIpadUI();
        }
        else if (isIphone)
        {
            //Debug.Log("Detected an iPhone resolution.");
            HandleIphoneUI();
        }
        else
        {
            //Debug.Log("Unknown device resolution.");
        }
    }

    void HandleIpadUI()
    {
        //Debug.Log("Applying iPad-specific settings...");
        Transform = this.GetComponent<Transform>();
        Transform.localScale = new Vector2(sizeForIpad.x, sizeForIpad.y);
      
        
    }

    void HandleIphoneUI()
    {
        //Debug.Log("Applying iPhone-specific settings...");
        Transform = this.GetComponent<Transform>();
        Transform.localScale = new Vector2(sizeForIphone.x,sizeForIphone.y);
       

    }
}
