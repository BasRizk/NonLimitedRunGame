using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    public AudioClip invicibleModeMusic;
    public AudioClip startGameMusic;
    public AudioClip regularMusic;
    public AudioClip gameOverMusic;
    public AudioClip pauseMusic;

    public AudioClip startGameClip;
    public AudioClip pressBtnClip;
    public AudioClip pauseClip;
    public AudioClip unPauseClip;
    public Camera firstPersonViewCamera;
    public Camera thirdPersonViewCamera;
    public Canvas startCanvas;
    public Canvas runningCanvas;
    public Canvas pauseCanvas;
    public Canvas gameOverCanvas;
    public Canvas creditsCanvas;
    public Canvas howToCanvas;
    public TextMeshProUGUI muteText;
    public Text gameOverScoreText;
    public Text timeText;
    public Text scoreText;
    public Text boostTitleText;
    public Text boostText;
    public int timeLimit;
    public float maxBoostValue;
    public int invicibleModeTime;

    public Image boostMeterImage;
    public Image statusBarImage;
    private float scoreBoost;
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
    private bool isGameMuted;
    private bool windowsUsed;
    private AudioSource audioSource;
    public AudioSource audioSourceEffect;
    public AudioSource audioSourcePause;
    private bool notFirstInvincibleMode;
    void Awake()
    {
        initStatus();
        switchCameras();
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        notFirstInvincibleMode = false;
        isGameMuted = false;
        muteText.text = "Mute";
        if(SystemInfo.operatingSystem.Contains("Windows")) {
            Debug.Log("Windows is used, removing unneccessary buttons");
            windowsUsed = true;
        } else {
            windowsUsed = false;
        }
        onClickMainMenu();
    }

    private void initStatus()
    {
        gameStatus = new bool[6];
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
        audioSource.Stop();
        audioSource.clip = regularMusic;
        audioSource.loop = true;
        audioSource.Play();
        onClickResume();

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
                statusBarImage.color = new Color32(245, 27, 27, 100);
                boostText.text = "INVICIBLE";
            }
            else
            {
                boostText.text = scoreBoost.ToString() + "/" + maxBoostValue;
            }
        }
        else
        {
            statusBarImage.color = new Color32(37, 5, 31, 100);
            boostText.text = "0/" + maxBoostValue;
        }

        updateBoostMeterImage();

        if(windowsUsed) {
            jumpGameObject.SetActive(false);
            camSwitchGameObject.SetActive(false);
        }
    }

    private void updateBoostMeterImage() {
        boostMeterImage.enabled = true;
        boostMeterImage.transform.localScale = new Vector3(scoreBoost/maxBoostValue, 1.0f, 1.0f);
        // Debug.Log("boostMeterImage localscale.x should be " + scoreBoost/maxBoostValue);
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
            onClickPause();
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
        audioSource.Pause();
        audioSourceEffect.clip = invicibleModeMusic;
        audioSourceEffect.loop = true;
        if(notFirstInvincibleMode) {
            audioSourceEffect.UnPause();
        } else {
            audioSourceEffect.Play();
            notFirstInvincibleMode = true;
        }

        invincibleModeActivated = true;
        invicibleModeTimeLeft = invicibleModeTime;
        gamePlane.speed = gamePlane.speed*2;
        // gamePlane.updateSpeed(gamePlane.originalSpeed * 2);
    }

    private void deactivateInvinicibleMode()
    {
        // Debug.Log("deactivateInvincibleMode", this);
        audioSourceEffect.Pause();
        audioSource.UnPause();
        scoreBoost = 0;
        invincibleModeActivated = false;
        gamePlane.speed = gamePlane.originalSpeed;
        // gamePlane.updateSpeed(gamePlane.originalSpeed);

        playerController.deactivateInvinicibleMode();
    }
    private void disableTheRestUI(Canvas oneCanvas) {
        startCanvas.enabled = false;
        runningCanvas.enabled = false;
        gameOverCanvas.enabled = false;
        howToCanvas.enabled = false;
        creditsCanvas.enabled = false;
        pauseCanvas.enabled = false;
        oneCanvas.enabled = true;
    }
    public void gameOver()
    {
        // TODO Game Over screen should be displayed.
        // Display score
        // TODO kill player and show score
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = gameOverMusic;
        audioSource.Play();
        audioSource.loop = true;
        
        disableTheRestUI(gameOverCanvas);
        gameOverScoreText.text = currentScore.ToString();
        
        itemsSpawner.enabled = false;
        playerController.enabled = false;
        gamePlane.enabled = false;
        isGameOver = true;
        updateGameStatus("GameOverScreen");
    }
    public void onClickMainMenu()
    {
        // Debug.Log("gameStatus[1] = " + gameStatus[1]);
        if(!audioSource.isPlaying || !gameStatus[5] || !gameStatus[1]) {
            audioSource.clip = startGameMusic;
            audioSource.Play();
            audioSource.loop = true;
        }

        audioSource.PlayOneShot(pressBtnClip);


        gamePlane.pause();
        playerController.pause();
        itemsSpawner.pause();
        
        disableTheRestUI(startCanvas);
        gamePlane.enabled = false;
        playerController.enabled = false;
        itemsSpawner.enabled = false;
        updateGameStatus("StartScreen");
    }
    public void onClickStart()
    {
        audioSource.PlayOneShot(startGameClip);
        disableTheRestUI(runningCanvas);

        gamePlane.enabled = true;
        playerController.enabled = true;
        itemsSpawner.enabled = true;

        gamePlane.init();
        playerController.init();
        itemsSpawner.reinit();
        initRunningGameStats();
        onClickResume();
        updateGameStatus("RunningScreen");
    }
    public void onClickCredits()
    {
        audioSource.PlayOneShot(pressBtnClip);
        disableTheRestUI(creditsCanvas);
        updateGameStatus("CreditsScreen");
    }

    public void onClickRestart()
    {
        audioSource.PlayOneShot(startGameClip);
        disableTheRestUI(runningCanvas);

        playerController.enabled = true;
        gamePlane.enabled = true;
        itemsSpawner.enabled = true;

        gamePlane.init();
        playerController.init();
        itemsSpawner.reinit();
        initRunningGameStats();
        updateGameStatus("RunningScreen");
    }

    public void onClickPause()
    {
        audioSource.Pause();
        audioSourcePause.PlayOneShot(pauseClip);
        audioSourcePause.clip = pauseMusic;
        audioSourcePause.loop = true;
        audioSourcePause.Play();

        disableTheRestUI(pauseCanvas);

        gamePlane.pause();
        playerController.pause();
        itemsSpawner.pause();

        updateGameStatus("PauseScreen");
    }

    public void onClickResume()
    {
        audioSourcePause.Pause();
        audioSource.PlayOneShot(unPauseClip);
        audioSource.UnPause();

        disableTheRestUI(runningCanvas);

        gamePlane.unPause();
        playerController.unPause();
        itemsSpawner.unPause();
        updateGameStatus("RunningScreen");
    }

    public void onClickMute() {
        audioSource.PlayOneShot(pressBtnClip);
        if(isGameMuted) {
            audioSource.mute = false;
            playerController.mute(false);
            isGameMuted = false;
            muteText.text = "Mute";
        } else {
            audioSource.mute = true; 
            playerController.mute(true);    
            isGameMuted = true;
            muteText.text = "Unmute";
        }
    }

    public void onClickHowTo() {
        audioSource.PlayOneShot(pressBtnClip);
        disableTheRestUI(howToCanvas);
        updateGameStatus("HowToScreen");
    }

    public void onClickQuit() {
        audioSource.PlayOneShot(pressBtnClip);
        Application.Quit();
    }
    public void updateLeftTime(int updateValue)
    {
        // Debug.Log("Time value to be added = " + updateValue);
        leftTime += updateValue;
        // Debug.Log("TimeLeft after addition = " + leftTime);
    }

    public void updateScoreBoost(float value)
    {
        scoreBoost += value;
    }

    public float getScoreBoost()
    {
        return scoreBoost;
    }

    public int getLeftTime()
    {
        return leftTime;
    }

    public float getMaxBoostValue()
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
            case "HowToScreen":
                initStatus(); gameStatus[5] = true;
                break;
        }
    }

    private void debugGameStatus() {
        Debug.Log("GameStatus: ");
        for(int i = 0 ; i < gameStatus.Length; i++) {
            Debug.Log("GAMESTATUS[I] = " + gameStatus[i]);
        }
    }
}
