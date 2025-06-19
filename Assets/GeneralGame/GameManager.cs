using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TMP_Text timerText;
    public TMP_Text bestTimeText;
    public TMP_Text startUI;
    public TMP_Text finishUI;

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
        ShowStartUI();
    }

    void Update()
    {
        if (isRacing)
        {
            currentTime += Time.deltaTime;
            timerText.text = currentTime.ToString("F2") + "s";
        }
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
        startUI.enabled = true;
        finishUI.enabled = false;
    }

    void ShowTimerUI()
    {
        startUI.enabled = false;
        finishUI.enabled = false;
    }

    void ShowFinishUI()
    {
        finishUI.enabled = true;
    }

    public void RestartRace()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
