using UnityEngine;
using UnityEngine.UI;

public class RestartButtonBinder : MonoBehaviour
{
    [SerializeField] private Button btn;

    private void Awake()
    {
        if (btn == null) btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClickRestart);
    }

    private void OnClickRestart()
    {
        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
            gm.RestartGame();
        else
            Debug.LogWarning("GameManager�� ã�� ���߽��ϴ�");
    }
}
