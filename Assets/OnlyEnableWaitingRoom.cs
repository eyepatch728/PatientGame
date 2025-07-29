using UnityEngine;

public class OnlyEnableWaitingRoom : MonoBehaviour
{
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject outfitSelection;
    [SerializeField] private GameObject toolsSelection;
    [SerializeField] private GameObject waitingRoom;
    [SerializeField] private GameObject waitingRoomProgressBar;

    void Start()
    {
        if(GlobalManager.Instance.hasComeFromTheMainMenu)
        {
            if (mainScreen != null) mainScreen.SetActive(false);
            if (outfitSelection != null) outfitSelection.SetActive(false);
            if (toolsSelection != null) toolsSelection.SetActive(false);
            if (waitingRoom != null) waitingRoom.SetActive(true);
            //if (waitingRoomProgressBar != null) waitingRoomProgressBar.SetActive(true);
            print("Skipping to waiting room only as per GlobalManager flag.");
            return;
        }
        
    }
}
