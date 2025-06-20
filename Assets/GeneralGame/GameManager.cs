using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Animator lightAnimator; //Traffic light thing

    public LiftController liftController; //Animated car lift

    public TMP_Text timerText;
    public TMP_Text bestTimeText; //Text elements
    public TMP_Text finishText;

    public Image lightPanel;
    public Image light1;
    public Image light2; //Individual lights
    public Image light3;

    public Color lightGoColor = Color.green;
    public Color lightStopColor = Color.red; //Predefined colors
    public Color lightOffColor = Color.gray;

    private float currentTime = 0f;
    private float bestTime = Mathf.Infinity;
    private bool isRacing = false;
    private bool countdownFinished = false;

    //Runs just once
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;   
        }
        else Destroy(gameObject);
    }

    //Runs once per game
    void Start()
    {
        lightAnimator = lightPanel.GetComponent<Animator>();
        bestTime = PlayerPrefs.GetFloat("BestTime", Mathf.Infinity);

        UpdateBestTimeUI(); //Set previous best time
        StartCoroutine(Countdown()); //Traffic light thing
        liftController.StartLowering(); //Lower lift

        ShowStartUI();
        timerText.text = "0.00s";
        finishText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isRacing)
        {
            currentTime += Time.deltaTime;
            timerText.text = currentTime.ToString("F2") + "s"; //Set current time, float + 2 decimal places
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartRace(); //Restart at any time
        }
    }

    IEnumerator Countdown() //Runs independently in coroutine
    {
        light1.color = lightOffColor;
        light2.color = lightOffColor;
        light3.color = lightOffColor;

        yield return new WaitForSeconds(2f);

        light1.color = lightStopColor;
        yield return new WaitForSeconds(1f);

        light2.color = lightStopColor;
        yield return new WaitForSeconds(1f);

        light3.color = lightStopColor;
        yield return new WaitForSeconds(1f);

        light1.color = lightGoColor;
        light2.color = lightGoColor;
        light3.color = lightGoColor;

        yield return new WaitForSeconds(1.5f);

        light1.color = lightOffColor;
        light2.color = lightOffColor;
        light3.color = lightOffColor;

        countdownFinished = true;
    }

    public void StartRace()
    {
        if (!countdownFinished)
        {
            return; //Not allowed to start timer before lights go off
        }

        if (isRacing)
        {
            return;
        }

        currentTime = 0f;
        isRacing = true;
        ShowTimerUI();
    }

    public void FinishRace()
    {
        if (!isRacing) return;

        isRacing = false;

        if (currentTime < bestTime) //New best time?
        {
            bestTime = currentTime;
            PlayerPrefs.SetFloat("BestTime", bestTime); //Save to prefs
            PlayerPrefs.Save();
        }

        UpdateBestTimeUI();
        ShowFinishUI();
    }

    public void RestartRace()
    {
        SceneManager.LoadScene(0);
    }

    void UpdateBestTimeUI()
    {
        bestTimeText.text = "Best Time: " +
            (bestTime == Mathf.Infinity ? "—" : bestTime.ToString("F2") + "s");
    }

    void ShowStartUI()
    {
        finishText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
    }

    void ShowTimerUI()
    {
        lightAnimator.SetTrigger("HideLight"); //Light hide animation
        finishText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(true);
    }

    void ShowFinishUI()
    {
        finishText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(false);
    }
}