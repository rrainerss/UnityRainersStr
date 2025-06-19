using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public LiftController liftController;

    public TMP_Text timerText;
    public TMP_Text bestTimeText;
    public TMP_Text finishText;

    public Image lightPanel;
    public Image light1;
    public Image light2;
    public Image light3;

    public Color lightGoColor = Color.green;
    public Color lightStopColor = Color.red;
    public Color lightOffColor = Color.gray;

    private float currentTime = 0f;
    private float bestTime = Mathf.Infinity;
    private bool isRacing = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        bestTime = PlayerPrefs.GetFloat("BestTime", Mathf.Infinity);
        UpdateBestTimeUI();
        StartCoroutine(Countdown());
        liftController.StartLowering();
    }

    void Update()
    {
        if (isRacing)
        {
            currentTime += Time.deltaTime;
            timerText.text = currentTime.ToString("F2") + "s";
        }
    }

    IEnumerator Countdown()
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
    }

    public void StartRace()
    {
        currentTime = 0f;
        isRacing = true;
        ShowTimerUI();
    }

    public void FinishRace()
    {
        isRacing = false;

        if (currentTime < bestTime)
        {
            bestTime = currentTime;
            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.Save();
        }

        UpdateBestTimeUI();
        ShowFinishUI();
    }

    void UpdateBestTimeUI()
    {
        bestTimeText.text = "Best Time: " +
            (bestTime == Mathf.Infinity ? "—" : bestTime.ToString("F2") + "s");
    }

    void ShowStartUI()
    {
        lightPanel.gameObject.SetActive(true);
        finishText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
    }

    void ShowTimerUI()
    {
        lightPanel.gameObject.SetActive(false);
        finishText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(true);
    }

    void ShowFinishUI()
    {
        finishText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(false);
    }

    public void RestartRace()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
