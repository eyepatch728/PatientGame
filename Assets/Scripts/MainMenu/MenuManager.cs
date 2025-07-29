using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject outfitSelection;
    [SerializeField] private GameObject toolsSelection;
    [SerializeField] private GameObject waitingRoom;
    [SerializeField] private GameObject waitingRoomProgressBar;

    public static MenuManager Instance;

    private const string ToolsToWaitingRoomCompletedKey = "ToolsToWaitingRoomCompleted";

    private bool canClick = false;
    private void Awake()
    {
        Instance = this;
        //SoundManager.instance.PlayAmbulance();

        Invoke(nameof(PlayMainMenuSoundWithDelay), 0.1f);
    }
    public void PlayMainMenuSoundWithDelay ()
    {
        if (GlobalManager.Instance.hasComeFromTheMainMenu)
        {
            SkipToWaitingRoomOnly();
           

            SoundManager.instance.PlayWaitingRoomMusic();
            SoundManager.instance.PlayCrySniff();
            print("helloIAmBack");
            return;
        }
        else
        {
            SoundManager.instance.PlayAmbulance();
            SoundManager.instance.PlayMainMenuMusic();
            DOVirtual.DelayedCall(1f, () =>
            {
                SoundManager.instance.PlayWelcome();
               


            });

        }
    }

    
    void Start()
    {
        // Skip rest of Start if we came from MainMenu
        //if (GlobalManager.Instance.hasComeFromTheMainMenu) return;
        //SoundManager.instance.PlayAmbulance();
        //DOVirtual.DelayedCall(1f, () =>
        //{
        //    SoundManager.instance.PlayWelcome();

        //});
        // Simulate progress for debugging (remove in final build)

        Invoke(nameof(EnableClick), 1f);
        PlayerPrefs.SetInt("LevelCompleted", 5);
        PlayerPrefs.SetInt("TreatedPatients", 5);
        PlayerPrefs.SetInt("UnlockedMedals", 3);
        PlayerPrefs.Save();

        // Normal start: show main screen
        mainScreen.SetActive(true);
        outfitSelection.SetActive(false);
        toolsSelection.SetActive(false);
        waitingRoom.SetActive(false);
        if (GlobalManager.Instance.hasComeFromTheMainMenu)
        {
            SkipToWaitingRoomOnly();
            print("helloIAmBack");
            return;
        }
        print("here is the playerpref  " + PlayerPrefs.GetInt("LevelCompleted"));
    }
    void EnableClick()
    {
        canClick = true;
    }

    private void SkipToWaitingRoomOnly()
    {
        // Skip to waiting room without playing any sounds or animations
        mainScreen.SetActive(false);
        outfitSelection.SetActive(false);
        toolsSelection.SetActive(false);
        waitingRoom.SetActive(true);
        waitingRoomProgressBar.SetActive(true);
    }
    public void MainToOutfit()
    {
        if (!canClick) return;

        mainScreen.SetActive(false);
        outfitSelection.SetActive(true);
        SoundManager.instance.PlayClick();

        SoundManager.instance.PlayScenesMusic();
        Invoke(nameof(StopMainMenuSound), 0.5f);
    }

    public void StopMainMenuSound()
    {
        SoundManager.instance.StopWelcome();
        SoundManager.instance.PlayChooseOutFit();

    }
    public void OutfitToNextStep()
    {
        
        outfitSelection.SetActive(false);

        // If first time, go to tools screen
       // if (PlayerPrefs.GetInt(ToolsToWaitingRoomCompletedKey, 0) == 0)
        {
            toolsSelection.SetActive(true);
        }
        //else
        //{
        //    // Skip tools, go straight to waiting room
        //    waitingRoom.SetActive(true);
        //    waitingRoomProgressBar.SetActive(true);
        //    SoundManager.instance.PlayWaitingRoomMusic();
        //    SoundManager.instance.PlayCrySniff();
        //}
       
    }

    public void ToolsToWaitingRoom()
    {
        toolsSelection.SetActive(false);
        waitingRoom.SetActive(true);
        waitingRoomProgressBar.SetActive(true);
        SoundManager.instance.PlayWaitingRoomMusic();
        SoundManager.instance.PlayCrySniff();
        // Mark tools step as completed
        PlayerPrefs.SetInt(ToolsToWaitingRoomCompletedKey, 1);
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("LevelCompleted", 0);
    }
}
