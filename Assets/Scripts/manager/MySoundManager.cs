using UnityEngine;

public class MySoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource winAudio;
    [SerializeField] private AudioSource gameOverAudio;

    private static MySoundManager instance;
    public static MySoundManager Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        if (winAudio == null)
        {
            Transform winTransform = transform.Find("Win");
            if (winTransform != null)
                winAudio = winTransform.GetComponent<AudioSource>();
        }
    }

    public void PlayWin()
    {
        if (winAudio != null && !winAudio.isPlaying)
        {
            winAudio.Play();
        }
    }
    public void PlayGameover()
    {
        if (gameOverAudio != null && !gameOverAudio.isPlaying)
        {
            gameOverAudio.Play();
        }
    }
}
