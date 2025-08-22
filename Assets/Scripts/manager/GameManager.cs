using System;
using System.Collections;
using System.Collections.Generic;
using Kamgam.UGUIWorldImage;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] List<CupPatternData> patternList;

    public Transform parentTransform;

    private int currentPatternIndex = 0;
    private int previousPatternIndex = 0;
    private bool isGameOver = false;
    public bool IsGameOver => isGameOver;
    public bool IsSetting = false;

    private static GameManager instance = null;
    public static GameManager Instance => instance;

    [SerializeField] private PrefabInstantiatorForWorldImage worldImageInstantiator;

    cupSpawner spawner;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(gameObject);

        spawner = FindFirstObjectByType<cupSpawner>();
        Application.targetFrameRate = 120;
    }

    private void Start()
    {
        StartGame(); // Temp code. Gotta refactor after building intros.
        SetCurrentPattern();
    }

    public void StartGame()
    {
        // initiate start game logic
        UIManager.Instance.EnableIngameUI();
    }

    public void RestartGame()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        SceneManager.LoadScene(currentSceneIndex);
    }

    public void PlaySuccessEffect(GameObject cup)
    {
        Animator animator = cup.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("successPattern");
        }
    }

    public int GetRandomPattern()
    {
        int randomIndex = UnityEngine.Random.Range(0, patternList.Count);
        return randomIndex;
    }

    public void DestroyCupAfterDelay(GameObject cup, float delay = 1f)
    {
        StartCoroutine(DelayedDestroy(cup, delay));
    }

    private IEnumerator DelayedDestroy(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }

    public void OnPatternPassed()
    {
        previousPatternIndex = currentPatternIndex;
        currentPatternIndex = GetRandomPattern();

        while (currentPatternIndex == previousPatternIndex)
        {
            currentPatternIndex = GetRandomPattern();
        }

        MySoundManager.Instance.PlayWin();

        spawner.cupSpawned = 0;
        SetCurrentPattern();
        
    }

    private void SetCurrentPattern()
    {
        CupStackManager.Instance.SetPattern(patternList[currentPatternIndex]);
        Debug.Log(currentPatternIndex);
        ToggleUI(currentPatternIndex);
        UIManager.Instance.SetCupsLeft();
    }

    private void ToggleUI(int index)
    {
        worldImageInstantiator.ToogleOrCreate(index, false, true);
    }

    public void OnPatternFailed()
    {
        GameOver();
    }

    private void GameOver()
    {
        if (!isGameOver)
        {
            isGameOver = true;

            scoreManager.Instance.LoadHighScore();
            StartCoroutine(DelayedGameOver(1f));
        }
    }

    private IEnumerator DelayedGameOver(float delay)
    {
        yield return new WaitForSeconds(delay);
        UIManager.Instance.EnableGameOverUI();
        MySoundManager.Instance.PlayGameover();
        Debug.Log("Game Over!");
    }

    public void SetSettingConditions()
    {
        IsSetting = !IsSetting;
    }
}

