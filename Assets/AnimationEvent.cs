using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public EyeProblemManager problemManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AnimationEvvent() 
    {
    problemManager.FinishStep3();
    }
}
