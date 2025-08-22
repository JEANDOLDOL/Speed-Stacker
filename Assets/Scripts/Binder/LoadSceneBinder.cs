using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoaderBinder : MonoBehaviour
{
    [SerializeField] private string targetScene = "Main";
    [SerializeField] Button button;

    private void Awake()
    {
        if(button != null)
        {
            button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(LoadTargetScene);
        }
    }
    public void LoadTargetScene()
    {
        if (string.IsNullOrWhiteSpace(targetScene)) return;
        SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
    }
}
