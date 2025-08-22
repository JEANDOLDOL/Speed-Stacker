using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class cupSpawner : MonoBehaviour
{
    Rigidbody rb;

    public float moveSpeed = 3f;
    public float moveRange = 20f;
    private int direction = 1;
    private float startX;

    public int cupSpawned = 0;

    [SerializeField] GameObject cupPrefab;

    private bool _touchBeganOverUI = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startX = transform.position.x; // 필드에 기록
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void OnTap(InputAction.CallbackContext context)
    {
        // 설정창 열려있으면 아예 무시
        if (GameManager.Instance.IsSetting) return;

        if (context.started)
        {
            var touch = context.control as TouchControl;
            if (touch != null)
            {
                Vector2 pos = touch.position.ReadValue();
                _touchBeganOverUI = RaycastUI(pos);
            }
            else
            {
                _touchBeganOverUI = false;
            }
            return;
        }

        if (!context.performed) return;
        if (_touchBeganOverUI) return;

        if (cupSpawned < CupStackManager.Instance.CurrentPattern.MaxStack
            && !GameManager.Instance.IsGameOver)
        {
            Vector3 spawnPoint = rb.position;
            var cup = Instantiate(cupPrefab, spawnPoint, cupPrefab.transform.rotation);

            var mr = cup.GetComponent<MeshRenderer>();
            var newMats = mr.materials;
            newMats[0].color = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.8f, 1f);
            mr.materials = newMats;

            CupStackManager.Instance.RemainingStacks -= 1;
            UIManager.Instance.SetCupsLeft();
            cupSpawned++;
        }
    }

    private static bool RaycastUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        var ped = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);
        return results.Count > 0;
    }

    private void Move()
    {
        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;

        float offsetX = transform.position.x - startX;
        if (Mathf.Abs(offsetX) >= moveRange / 2f)
            direction *= -1;
    }
}
