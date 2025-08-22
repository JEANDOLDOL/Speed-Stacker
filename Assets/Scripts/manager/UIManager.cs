using TMPro;
using UnityEngine;
using Ricimi;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;

    [SerializeField] GameObject IngameUI;
    [SerializeField] TextMeshProUGUI CupsLeftUI;

    [Header("Popup Prefab")]
    [SerializeField] Popup gameOverPopupPrefab;     // 프리팹 참조
    [SerializeField] Transform popupParent;          // 비우면 IngameUI 밑으로

    private Popup currentPopup;                      // 현재 떠있는 팝업 참조

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void EnableIngameUI() => IngameUI.SetActive(true);
    public void DisableIngameUI() => IngameUI.SetActive(false);

    public void SetCupsLeft()
    {
        CupsLeftUI.SetText("{0}", CupStackManager.Instance.RemainingStacks);
    }

    public void EnableGameOverUI()
    {
        // 이전에 떠있던 팝업이 파괴되었으면 참조 정리
        if (currentPopup == null)
        {
            var parent = popupParent != null ? popupParent : IngameUI.transform;
            currentPopup = Instantiate(gameOverPopupPrefab, parent);
        }

        currentPopup.gameObject.SetActive(true);
        currentPopup.transform.SetAsLastSibling(); // 최상단으로
        currentPopup.Open();

        var anim = currentPopup.GetComponent<Animator>();
        if (anim) anim.Play("Open", 0, 0f);
    }

    public void DisableGameOverUI()
    {
        //if (gameOverPopupPrefab == null)
        //    gameOverPopupPrefab = IngameUI.GetComponentInChildren<Popup>(true);

        //gameOverPopupPrefab.gameObject.SetActive(true);

        currentPopup.Close();
    }
}

