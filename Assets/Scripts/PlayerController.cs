using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public AudioClip coinClip;
    public AudioClip activateInvincibleModeClip;
    public AudioClip deactivateInvinicibleModeClip;

    public AudioClip hitBombClip;
    public AudioClip hitIronBallClip;
    public AudioClip boostSphereClip;

    public AudioClip jumpInTheAirClip;
    public AudioClip jumpFallingClip;
    private AudioSource audioSource;
    public float swipingMinMagnitude;
    public float laneSpacing;
    public float speed;
    public float jumpHeight;
    public float jumpSpeed;

    private float tmpSpeed;
    private float tmpJumpSpeed;
    private float currentLane;
    private float unMovedAmount;
    private float unJumpedAmount;
    private float movingDirection;
    private Rigidbody rb;

    private GameObject hitGameObject;
    private float passedTimeJump;
    private float passedTimeMoved;
    private bool isPaused;
    public GameController gameController;
    public Material originalMaterial;
    public Material invicibleMaterial;

    private bool tap, swipeLeft, swipeRight, swipeUp;
    private bool isDragging;
    private Vector2 startTouch, swipeDelta;

    private Vector3 defPos;
    private Quaternion defRot;
    private Vector3 defScale;
    private bool invincibleModeActivated;
 

    // Start is called before the first frame update
    void Start()
    {
        defPos = transform.position;
        // Debug.Log("Player default postion = " + defPos.ToString());
        defRot = transform.localRotation;
        defScale = transform.localScale;
        init();
    }

    public void init()
    {
        transform.localPosition = defPos;
        // transform.localRotation = defRot;
        // transform.localScale = defScale;
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        currentLane = 0;
        unMovedAmount = 0;
        unJumpedAmount = 0;
        passedTimeJump = 0;
        passedTimeMoved = 0;
        isPaused = false;
        invincibleModeActivated = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused)
        {
            return;
        }
        updateSwipes();
        moveHorizontal();
        jump();

    }

    public void jumpBtn()
    {
        initOneJump();
    }

    private void jump()
    {
        if (unJumpedAmount != 0)
        {
            float toBeJumpedAmount = Time.deltaTime * tmpJumpSpeed * jumpHeight;
            tmpJumpSpeed += (tmpJumpSpeed < jumpSpeed) ? 2.0f : 0.0f;
            unJumpedAmount -= toBeJumpedAmount;
            // Debug.Log("UnJumped Amount currently = " + unJumpedAmount);

            if (unJumpedAmount < 0)
            {
                toBeJumpedAmount += unJumpedAmount;
                unJumpedAmount = 0;
                audioSource.PlayOneShot(jumpFallingClip);
            }
            // Debug.Log("UnJumped Amount currently = " + unJumpedAmount);

            Vector3 movement = new Vector3(0.0f, toBeJumpedAmount, 0.0f);
            transform.Translate(movement);
            // Debug.Log("Player y pos = " + transform.position.y);

        }
        else if (swipeUp || Input.GetKeyUp(KeyCode.Space))
        {
            swipeUp = false;
            initOneJump();
        }
        passedTimeJump += Time.deltaTime;
    }

    private void initOneJump()
    {
        if (unJumpedAmount == 0 && passedTimeJump > 0.6)
        {
            passedTimeJump = 0;
            // Debug.Log("Unjumped amount now = " + unJumpedAmount);
            // rb.AddForce(transform.up * Time.deltaTime * 300,ForceMode.Impulse);    
            unJumpedAmount = jumpHeight;
            tmpJumpSpeed = 1;
            audioSource.PlayOneShot(jumpInTheAirClip);

            // Debug.Log("Space pressed, and unjumped amount now = " + unJumpedAmount);
        }
    }

    private void moveHorizontal()
    {   
        float horizontal = Input.GetAxis("Horizontal");
        if(swipeLeft) {
            movingDirection = -1.0f;
            initOneMovement();
            swipeLeft = false;
        } else if(swipeRight) {
            movingDirection = 1.0f;
            initOneMovement();
            swipeRight = false;
        } else if(horizontal != 0) {
            movingDirection = horizontal > 0 ? 1.0f : -1.0f;
            initOneMovement();
        }

        if (unMovedAmount > 0)
        {

            float toBeMovedAmount = laneSpacing * Time.deltaTime * tmpSpeed;
            tmpSpeed -= (tmpSpeed > 1) ? 1.0f : 0.0f;
            unMovedAmount -= toBeMovedAmount;
            if (unMovedAmount < 0)
            {
                toBeMovedAmount += unMovedAmount;
                unMovedAmount = 0;
                
            }
            // Debug.Log("toBeMoved, UnMoved = " + toBeMovedAmount + " "  + unMovedAmount);
            Vector3 movement = new Vector3(movingDirection * toBeMovedAmount, 0.0f, 0.0f);
            transform.Translate(movement);

            // Debug.Log("transform.position " +
            //         transform.position.x + ", "+
            //         transform.position.y + ", " +
            //         transform.position.z);
        }
        passedTimeMoved += Time.deltaTime;
    }

    private void initOneMovement() {
        if(unMovedAmount == 0 && passedTimeMoved > 0.3)
        {   
            // Debug.Log("movingDirection = " + " " + movingDirection);
            if (currentLane != movingDirection)
            {
                currentLane += movingDirection;
                // Debug.Log("Current Lane = " + " " + currentLane);
                unMovedAmount = laneSpacing;
                tmpSpeed = speed;
                passedTimeMoved = 0;
            }
        }
    }

    private void updateSwipes()
    {
        tap = swipeLeft = swipeRight = false;
        # region Mobile Inputs
        if (Input.touches.Length > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                isDragging = true;
                tap = true;
                startTouch = Input.touches[0].position;

            }
            else if (Input.touches[0].phase == TouchPhase.Ended ||
                      Input.touches[0].phase == TouchPhase.Canceled)
            {
                isDragging = false;
                resetSwipes();
            }
        }
        
        // Calculate Distance
        swipeDelta = Vector2.zero;
        if(isDragging) {
            if(Input.touches.Length > 0) {
                swipeDelta = Input.touches[0].position = startTouch;
            }
        }
        // Did cross allowed zone
        if(swipeDelta.magnitude > swipingMinMagnitude) {
            float x = swipeDelta.x;
            float y = swipeDelta.y;
            if(Mathf.Abs(x) > Mathf.Abs(y)) {
                if(x < 0) {
                    // Debug.Log("Swiped left");
                    swipeLeft = true;
                    swipeRight = swipeUp = false;
                } else {
                    // Debug.Log("Swiped Right");
                    swipeRight = true;
                    swipeLeft = swipeUp = false;
                }
            } else {
                if(y > 0) {
                    // Debug.Log("Swiped Up");
                    swipeUp = true;
                    swipeLeft = swipeRight = false;
                }
            }
            resetSwipes();
        }
        # endregion
    }

    private void resetSwipes()
    {
        isDragging = false;
        startTouch = swipeDelta = Vector2.zero;
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if(invincibleModeActivated) {
            return;
        }
        
        hitGameObject = other.gameObject;
        if (hitGameObject.CompareTag("Coin"))
        {
            //TODO audio
            disableItem(hitGameObject);
            gameController.updateLeftTime(2);
            audioSource.PlayOneShot(coinClip);
            // Debug.Log("Time should be updated +2");
        }
        else if (hitGameObject.CompareTag("BoostSphere"))
        {
            disableItem(hitGameObject);
            gameController.updateScoreBoost(1);
            audioSource.PlayOneShot(boostSphereClip);
            // Debug.Log("Score boost should be updated +1");
            if (gameController.getScoreBoost() == gameController.getMaxBoostValue())
            {
                turnInvincibleMode();
                // Debug.Log("Player should be in invincible mode");
            }
        }
        else if (hitGameObject.CompareTag("IronBall"))
        {
            disableItem(hitGameObject);
            gameController.updateLeftTime(-10);
            audioSource.PlayOneShot(hitIronBallClip);
            // Debug.Log("Ironball hit, Time should be updated -10");
        }
        else if (hitGameObject.CompareTag("Bomb"))
        {
            disableItem(hitGameObject);
            endGame();
            audioSource.PlayOneShot(hitBombClip);
            Debug.Log("Game should be over.");
        }
        else
        {
            // Debug.Log("Unlabeled object collision.");
        }
    }

    private void disableItem(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void turnInvincibleMode()
    {
        invincibleModeActivated = true;
        this.GetComponent<Renderer>().material = invicibleMaterial;
        audioSource.PlayOneShot(activateInvincibleModeClip);
        gameController.turnInvincibleMode();

    }
    public void deactivateInvinicibleMode()
    {
        invincibleModeActivated = false;
        this.GetComponent<Renderer>().material = originalMaterial;
        audioSource.PlayOneShot(deactivateInvinicibleModeClip);
    }
    private void endGame()
    {
        gameController.gameOver();
    }

    public void unPause()
    {
        isPaused = false;
    }
    public void pause()
    {
        isPaused = true;
    }

}