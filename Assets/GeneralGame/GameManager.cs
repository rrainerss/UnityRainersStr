using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    private bool countdownFinished = false;
    public GameObject restartButton;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        bestTime = PlayerPrefs.GetFloat("BestTime", Mathf.Infinity);
        UpdateBestTimeUI();
        StartCoroutine(Countdown());
        liftController.StartLowering();

        ShowStartUI();
        timerText.text = "0.00s";
        finishText.gameObject.SetActive(false);

        if (restartButton != null)
        {
            restartButton.SetActive(false);

            Button btn = restartButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(RestartRace);
            }
            else
            {
                Debug.LogWarning("Restart button GameObject has no Button component!");
            }
        }
    }

    void Update()
    {
        if (isRacing)
        {
            currentTime += Time.deltaTime;
            timerText.text = currentTime.ToString("F2") + "s";
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartRace();
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

        countdownFinished = true;
        Debug.Log("Countdown finished! Ready to start race by crossing the start trigger.");
    }

    public void StartRace()
    {
        if (!countdownFinished)
        {
            Debug.Log("Cannot start race yet, countdown not finished.");
            return;
        }

        if (isRacing)
        {
            Debug.Log("Race already started.");
            return;
        }

        currentTime = 0f;
        isRacing = true;
        Debug.Log("Race started!");
        ShowTimerUI();

        if (restartButton != null)
            restartButton.SetActive(false);
    }

    public void FinishRace()
    {
        if (!isRacing) return;

        isRacing = false;

        if (currentTime < bestTime)
        {
            bestTime = currentTime;
            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.Save();
            Debug.Log("New best time: " + bestTime.ToString("F2") + "s");
        }

        UpdateBestTimeUI();
        ShowFinishUI();

        if (restartButton != null)
            restartButton.SetActive(true);
    }

    public void RestartRace()
    {
        Debug.Log("Restart button clicked - restarting scene.");
        SceneManager.LoadScene(0);
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
        lightPanel.gameObject.SetActive(false);
    }
}