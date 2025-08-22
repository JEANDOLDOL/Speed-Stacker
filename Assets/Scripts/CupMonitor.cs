using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(Rigidbody))]
public class CupMonitor : MonoBehaviour
{
    [Tooltip("업 방향과 월드 Y축 사이 각도 유사도 (1=완전 수직, 0=완전 수평)")]
    [SerializeField] private float tiltDotThreshold = 0.9f;

    [Tooltip("이동/회전 속도가 이 값보다 작으면 정지로 판단")]
    [SerializeField] private float velocityThreshold = 0.05f;

    private Rigidbody _rigidbody;
    private bool _hasFall = false;
    private bool _hasStood = false;
    private bool _hasLanded = false;

    AudioSource _dropSound;
    [SerializeField] AudioClip _dropSoundClip;


    private Animator _animator;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _dropSound = GetComponent<AudioSource>();
    }

    private void Update()
    {
        CheckIfLanded();
    }

    private void CheckIfLanded()
    {
        if (!_hasLanded || _hasFall || _hasStood) return;

        if (_rigidbody.linearVelocity.sqrMagnitude < velocityThreshold * velocityThreshold &&
            _rigidbody.angularVelocity.sqrMagnitude < velocityThreshold * velocityThreshold)
        {
            float uprightDot = Vector3.Dot(transform.up.normalized, Vector3.up);

            if (uprightDot < tiltDotThreshold)
            {
                Debug.Log("Cup is not still");
                GameManager.Instance.DestroyCupAfterDelay(gameObject);
                CupStackManager.Instance.EvaluateFeasibility();
                _hasFall = true;
            }
            else
            {
                if (CupStackManager.Instance != null)
                {
                    CupStackManager.Instance.RegisterCup(transform.position, gameObject);
                    Debug.Log(transform.position);
                    _hasStood = true;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!_hasLanded)
            { 
                _dropSound.PlayOneShot(_dropSoundClip);
            }
            _hasLanded = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Vanishor"))
        {
            CupStackManager.Instance.EvaluateFeasibility();
            Destroy(gameObject);
        }
    }
}
