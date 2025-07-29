using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{
        
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //}

    public void CompleteLevel()
    {
        PlayerPrefs.SetInt("LevelCompleted", 1);
        PlayerPrefs.SetString("CompletedPatientType", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }
    public void Back()
    {
        print("Helloweqweqweqwsdasvzxvsf");

        SceneManager.LoadSceneAsync("MainMenu");
        GlobalManager.Instance.hasComeFromTheMainMenu = true;
    }
}
