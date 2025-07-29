using System.Collections;
using DG.Tweening;
using UnityEngine;

public class BacteriaGun : MonoBehaviour
{
    [SerializeField] private Transform gun; // The gun object
    [SerializeField] private Transform greenArea; // The green area on the gun (laser)
    [SerializeField] private Transform[] monsters; // The monsters to be hit
    [SerializeField] private float touchDuration = 2f; // Duration for touch to be valid
    [SerializeField] private float maxScaleX = 1f; // Max scale for green area
    private bool isDragging1 = false; // Changed from isDragging to isDragging1
    private bool isTouchingMonster = false; // To track if the green area is touching a monster
    private float touchTime = 0f; // Track how long the green area is touching a monster
    private Transform currentMonster = null; // The current monster being touched
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartStep6()
    {
        gun.position += Vector3.right * 10f; // Start gun off-screen
        greenArea.localScale = new Vector3(0f, greenArea.localScale.y, greenArea.localScale.z); // Start with no scale
        greenArea.gameObject.SetActive(false); // Disable the green area (laser) at the beginning

        // Slide the gun in from the right
        gun.DOMoveX(gun.position.x - 10f, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                isDragging1 = true; // Renamed from isDragging to isDragging1
                StartCoroutine(TrackGunDrag());
            });
    }

    private IEnumerator TrackGunDrag()
    {
        while (isDragging1) // Renamed from isDragging to isDragging1
        {
            // Enable the green area (laser) while dragging the gun
            greenArea.gameObject.SetActive(true);

            // Increase the scale of the green area while dragging
            float scaleX = Mathf.Lerp(greenArea.localScale.x, maxScaleX, Time.deltaTime * 5f); // Adjust speed of scaling
            greenArea.localScale = new Vector3(scaleX, greenArea.localScale.y, greenArea.localScale.z);

            // Continuously check for monster collisions
            foreach (var monster in monsters)
            {
                // Ensure the monster has a trigger collider
                if (greenArea.GetComponent<Collider2D>().IsTouching(monster.GetComponent<Collider2D>()))
                {
                    if (currentMonster == null)
                    {
                        currentMonster = monster; // Set the first touched monster
                        touchTime = 0f; // Reset touch time
                    }

                    touchTime += Time.deltaTime;

                    // If the touch duration exceeds 2 seconds
                    if (touchTime >= touchDuration)
                    {
                        StartCoroutine(DestroyMonster(currentMonster)); // Destroy monster with animation
                        currentMonster = null; // Reset monster tracking
                    }

                    isTouchingMonster = true; // We are touching a monster
                    break;
                }
                else
                {
                    touchTime = 0f; // Reset touch time if not touching any monster
                    isTouchingMonster = false; // Not touching any monster
                }
            }

            yield return null;
        }

        // If we're not dragging anymore, disable the green area
        greenArea.gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            isTouchingMonster = true;
            currentMonster = other.transform;
            touchTime = 0f; // Reset the touch timer
        }
        if (other.CompareTag("Monster"))
        {
            Debug.Log("Touching monster: " + other.name);
            isTouchingMonster = true;
            currentMonster = other.transform;
            touchTime = 0f; // Reset the touch timer
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            touchTime += Time.deltaTime; // Increase the touch time

            // If the green area stays in contact for the required time, monster disappears
            if (touchTime >= touchDuration)
            {
                other.transform.localScale = Vector3.zero; // Disappear the monster
                touchTime = 0f; // Reset the timer after monster disappears
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            isTouchingMonster = false;
            touchTime = 0f; // Reset the timer when exiting trigger
        }
    }
    private IEnumerator DestroyMonster(Transform monster)
    {
        // Scale down the monster (animation)
        float elapsedTime = 0f;
        Vector3 initialScale = monster.localScale;

        while (elapsedTime < touchDuration)
        {
            float scale = Mathf.Lerp(1f, 0f, elapsedTime / touchDuration);
            monster.localScale = new Vector3(scale, scale, scale);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        monster.gameObject.SetActive(false); // Hide the monster after scaling down
    }



    private void OnMouseDrag()
    {
        if (isDragging1) // Renamed from isDragging to isDragging1
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = gun.position.z; // Keep the Z position fixed
            gun.position = worldPos;
        }
    }

    private void FinishStep6()
    {
        isDragging1 = false; // Renamed from isDragging to isDragging1
                             // Handle the end of the step (e.g., move to the next step)
        Debug.Log("Step 4 completed. Ready for next step.");
    }

}
