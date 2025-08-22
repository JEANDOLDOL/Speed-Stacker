using UnityEngine;
using UnityEngine.UI;

public class QuitGameBinder : MonoBehaviour
{
    [SerializeField] Button button;

    private void Awake()
    {
        if (button != null)
        {
            button = GetComponent<Button>();
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnclickQuitGame);
    }

    private void OnclickQuitGame()
    {
        Application.Quit();
    }
}
