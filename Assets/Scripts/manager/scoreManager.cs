using UnityEngine;
using TMPro;

public class scoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreUi;

    private TextMeshProUGUI currentScoreUI;
    private TextMeshProUGUI highScoreUI;

    private int highScore = 0;
    private int currentScore = 0;

    private const string HighScoreKey = "HighScore";

    private static scoreManager instance;
    public static scoreManager Instance => instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadHighScore();
        ForceRefreshUI();
    }

    public void AttachUI(TextMeshProUGUI current, TextMeshProUGUI high)
    {
        currentScoreUI = current;
        highScoreUI = high;
    }
    public void DetachUI(TextMeshProUGUI current, TextMeshProUGUI high)
    {
        if (currentScoreUI == current) currentScoreUI = null;
        if (highScoreUI == high) highScoreUI = null;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
        }
        ForceRefreshUI();
    }

    public void ForceRefreshUI()
    {
        if (scoreUi != null) scoreUi.text = currentScore.ToString();
        if (currentScoreUI != null) currentScoreUI.text = currentScore.ToString();
        if (highScoreUI != null) highScoreUI.text = highScore.ToString();
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HighScoreKey, highScore);
        PlayerPrefs.Save();
    }

    public void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey(HighScoreKey);
        highScore = 0;
        ForceRefreshUI();
    }
}
