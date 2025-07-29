
using DG.Tweening;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Patient : MonoBehaviour
{
    public string patientType;
    public bool isSitting = false;
    public string sittingAnim;
    public string walkingAnim;
    // References to any animations or visuals
    public Animator animator;
    public bool isEyes, isCold, isFoodAllergies, isChickenPox, isVaccination;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Setup(string type)
    {
        patientType = type;
    }

    public void SitDown()
    {
        isSitting = true;
        if (isVaccination)
        {
            this.gameObject.transform.GetChild(0).GetComponent<Animator>().Play("MotherSittingIdle");
        }

        Vector3 targetPosition = GetTargetSitPosition();
        transform.DOMove(targetPosition, 0.2f).SetEase(Ease.OutQuad);

        animator.Play(sittingAnim);
    }

    public Vector3 GetTargetSitPosition()
    {
        Vector3 target = transform.position;

       
        if (isVaccination)
        {
            //target.y += 0.8f;
        }

        return target;
    }
    public Vector3 GetTargetSitPosition2()
    {
        Vector3 target = Vector3.zero;

        if (isCold)
        {
            if (DeviceResForMainMenu.Instance.isIpad) { target.x -= 0.4f; print("ForColdPos;"); }
            else
            target.x -= (0.8f );
               

        }
        if (isFoodAllergies)
        {
            if (DeviceResForMainMenu.Instance.isIpad) { target.x += 0.6f; print("ForColdPos;"); }
           else
            target.x += 0.8f;
                
        }
        if (isEyes)
        {
            // target.x += 0.8f;
            if (DeviceResForMainMenu.Instance.isIpad) { target.x += 0.5f ; print("ForColdPos;"); }
            else
            target.x += 0.8f;
                
        }
        if (isChickenPox)
        {
            if (DeviceResForMainMenu.Instance.isIpad) { target.x -= 0.4f; print("ForColdPos;"); }
            else
            target.x -= 0.7f;
            //target.x += 0.8f;
               
        }
        if (isVaccination)
        {
            target.y -= 2.8f;
        }

        return target;
    }

    public void StandUp()
    {
        isSitting = false;
        
        //this.gameObject.transform.position = new Vector3(this.transform.position.x, WaitingRoomManager.Instance.walkingPos.position.y, this.transform.position.z);
            print("HellooVorasdsad");

        if (isVaccination)
        {
            this.gameObject.transform.GetChild(0).GetComponent<Animator>().Play("MotherWalking");
        }

        //// If you have an animator, trigger the stand animation
        //if (animator != null)
        //{
        //    animator.SetBool("IsSitting", false);
        //}
        animator.Play(walkingAnim);
    }

    private void OnMouseDown()
    {
        if (isSitting && !WaitingRoomManager.Instance.isPatientMoving)
        {
            // When clicked, notify the WaitingRoomManager
            WaitingRoomManager.Instance.OnPatientSelected(gameObject);
        }
    }
}
