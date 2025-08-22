using UnityEngine;
using TMPro;

public class ScoreUIBinder : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentScoreUI;
    [SerializeField] private TextMeshProUGUI highScoreUI;

    private void OnEnable()
    {
        if (scoreManager.Instance != null)
        {
            scoreManager.Instance.AttachUI(currentScoreUI, highScoreUI);
            scoreManager.Instance.ForceRefreshUI();
        }
    }

    private void OnDisable()
    {
        if (scoreManager.Instance != null)
            scoreManager.Instance.DetachUI(currentScoreUI, highScoreUI);
    }
}
