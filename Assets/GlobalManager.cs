using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public bool hasComeFromTheMainMenu;
    public static GlobalManager Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       // hasComeFromTheMainMenu = false;
    }
    private void Awake()
    {
        // Ensure that this GameObject persists across scene loads
        DontDestroyOnLoad(gameObject);

        // Singleton pattern setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
