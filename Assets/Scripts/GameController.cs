using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public AudioClip highSpeedMusic;
    public AudioClip regularMusic;
    public AudioClip gameOverClip;
    public AudioClip startGameClip;
    public AudioClip pressBtnClip;
    public Camera firstPersonViewCamera;
    public Camera thirdPersonViewCamera;
    public Canvas startCanvas;
    public Canvas runningCanvas;
    public Canvas pauseCanvas;
    public Canvas gameOverCanvas;
    public Canvas creditsCanvas;
    public Text gameOverScoreText;
    public Text timeText;
    public Text scoreText;
    public Text boostTitleText;
    public Text boostText;
    public int timeLimit;
    public int maxBoostValue;
    public int invicibleModeTime;

    private int scoreBoost;
    private int timePastLastScoreBoost;
    private int leftTime;
    private int currentScore;
    private float lastSecondObserved;
    private bool invincibleModeActivated;
    private int invicibleModeTimeLeft;
    public EndlessPlane gamePlane;
    public PlayerController playerController;
    public ItemsSpawner itemsSpawner;

    public GameObject camSwitchGameObject;
    public GameObject jumpGameObject;
    private bool[] gameStatus;
    // GameStatus [StartScreen, CreditsScreen, RunningScreen, PauseScreen, GameOverScreen];

    private bool isGameOver;
    private bool windowsUsed;
    private AudioSource audioSource;
    void Awake()
    {
        initStatus();
        switchCameras();
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if(SystemInfo.operatingSystem.Contains("Windows")) {
            Debug.Log("Windows is used, removing unneccessary buttons");
            windowsUsed = true;
        } else {
            windowsUsed = false;
        }
        getMainMenu();
    }

    private void initStatus()
    {
        gameStatus = new bool[5];
        for (int i = 0; i < gameStatus.Length; i++)
        {
            gameStatus[i] = false;
        }
    }

    private void initRunningGameStats()
    {
        leftTime = timeLimit;
        scoreBoost = 0;
        timePastLastScoreBoost = 0;
        currentScore = 0;
        lastSecondObserved = Time.realtimeSinceStartup;
        invincibleModeActivated = false;
        isGameOver = false;
        updateStatusUI();
        audioSource.clip = regularMusic;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void updateStatusUI()
    {
        scoreText.text = currentScore.ToString();
        timeText.text = leftTime.ToString();
        if (scoreBoost > 0)
        {
            boostText.enabled = true;
            if (invincibleModeActivated)
            {
                boostText.text = "INVICIBLE";
            }
            else
            {
                boostText.text = scoreBoost.ToString() + "/" + maxBoostValue;
            }
        }
        else
        {
            boostText.text = "0/" + maxBoostValue;
        }

        if(windowsUsed) {
            jumpGameObject.SetActive(false);
            camSwitchGameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStatus[2])
        {
            updateRunningScreen();
        }
    }

    private void updateRunningScreen()
    {
        if (leftTime >= 0)
        {
            updateStatusUI();
            currentScore += ((int)gamePlane.getUnAccessedTravelledDistance());
            // Debug.Log("CurrentScore = " + currentScore);
        }
        else
        {
            gameOver();
        }

    }

    private void switchCameras()
    {
        if (firstPersonViewCamera.enabled)
        {
            thirdPersonViewCamera.enabled = true;
            thirdPersonViewCamera.GetComponent<AudioListener>().enabled = true;
            firstPersonViewCamera.enabled = false;
            firstPersonViewCamera.GetComponent<AudioListener>().enabled = false;
        }
        else
        {
            firstPersonViewCamera.enabled = true;
            firstPersonViewCamera.GetComponent<AudioListener>().enabled = true;
            thirdPersonViewCamera.enabled = false;
            thirdPersonViewCamera.GetComponent<AudioListener>().enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (gameStatus[2])
        {
            fixUpdateRunningScreen();
        }
    }

    public void switchCameraBtn()
    {
        switchCameras();
    }

    private void fixUpdateRunningScreen()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseGame();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            switchCameras();
        }

        if (Time.realtimeSinceStartup - lastSecondObserved > 1)
        {
            leftTime -= 1;
            lastSecondObserved = Time.realtimeSinceStartup;

            if (invincibleModeActivated)
            {
                // Debug.Log("invicibleModeTimeLeft = " + invicibleModeTimeLeft, this);
                invicibleModeTimeLeft -= 1;
                if (invicibleModeTimeLeft == 0)
                {
                    deactivateInvinicibleMode();
                }
            }
        }
    }




    public void turnInvincibleMode()
    {
        // Debug.Log("turnInvincibleMode", this);
        invincibleModeActivated = true;
        invicibleModeTimeLeft = invicibleModeTime;
        gamePlane.speed = gamePlane.speed*2;
        // gamePlane.updateSpeed(gamePlane.originalSpeed * 2);
    }

    private void deactivateInvinicibleMode()
    {
        // Debug.Log("deactivateInvincibleMode", this);
        scoreBoost = 0;
        invincibleModeActivated = false;
        gamePlane.speed = gamePlane.originalSpeed;
        // gamePlane.updateSpeed(gamePlane.originalSpeed);

        playerController.deactivateInvinicibleMode();
    }

    public void gameOver()
    {
        // TODO Game Over screen should be displayed.
        // Display score
        // TODO kill player and show score
        audioSource.Stop();
        audioSource.loop = false;
        gameOverCanvas.enabled = true;
        gameOverScoreText.text = currentScore.ToString();
        runningCanvas.enabled = false;

        itemsSpawner.enabled = false;
        playerController.enabled = false;
        gamePlane.enabled = false;

        isGameOver = true;
        updateGameStatus("GameOverScreen");
    }

    public void getMainMenu()
    {
        startCanvas.enabled = true;
        runningCanvas.enabled = false;
        gameOverCanvas.enabled = false;
        creditsCanvas.enabled = false;
        pauseCanvas.enabled = false;

        gamePlane.enabled = false;
        playerController.enabled = false;
        itemsSpawner.enabled = false;

        updateGameStatus("StartScreen");
    }
    public void startGame()
    {
        runningCanvas.enabled = true;
        startCanvas.enabled = false;

        gamePlane.enabled = true;
        playerController.enabled = true;
        itemsSpawner.enabled = true;

        gamePlane.init();
        playerController.init();
        itemsSpawner.reinit();
        initRunningGameStats();
        updateGameStatus("RunningScreen");
    }

    public void openCredits()
    {
        creditsCanvas.enabled = true;
        startCanvas.enabled = false;
        updateGameStatus("CreditsScreen");
    }

    public void restartGame()
    {
        // TODO
        runningCanvas.enabled = true;
        pauseCanvas.enabled = false;
        gameOverCanvas.enabled = false;

        playerController.enabled = true;
        gamePlane.enabled = true;
        itemsSpawner.enabled = true;

        gamePlane.init();
        playerController.init();
        itemsSpawner.reinit();
        initRunningGameStats();
        updateGameStatus("RunningScreen");
    }

    public void pauseGame()
    {
        gamePlane.pause();
        playerController.pause();
        itemsSpawner.pause();
        pauseCanvas.enabled = true;
        runningCanvas.enabled = false;
        updateGameStatus("PauseScreen");
    }

    public void resumeGame()
    {
        gamePlane.unPause();
        playerController.unPause();
        itemsSpawner.unPause();
        runningCanvas.enabled = true;
        pauseCanvas.enabled = false;
        updateGameStatus("RunningScreen");
    }

    public void updateLeftTime(int updateValue)
    {
        // Debug.Log("Time value to be added = " + updateValue);
        leftTime += updateValue;
        // Debug.Log("TimeLeft after addition = " + leftTime);
    }

    public void updateScoreBoost(int value)
    {
        scoreBoost += value;
    }

    public int getScoreBoost()
    {
        return scoreBoost;
    }

    public int getLeftTime()
    {
        return leftTime;
    }

    public int getMaxBoostValue()
    {
        return maxBoostValue;
    }


    private void updateGameStatus(string status)
    {
        switch (status)
        {
            case "StartScreen":
                initStatus(); gameStatus[0] = true;
                break;
            case "CreditsScreen":
                initStatus(); gameStatus[1] = true;
                break;
            case "RunningScreen":
                initStatus(); gameStatus[2] = true;
                break;
            case "PauseScreen":
                initStatus(); gameStatus[3] = true;
                break;
            case "GameOverScreen":
                initStatus(); gameStatus[4] = true;
                break;
        }
    }
}
