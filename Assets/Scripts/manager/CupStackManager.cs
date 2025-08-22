using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Kamgam.UGUIWorldImage;

public class CupStackManager : MonoBehaviour
{
    private static CupStackManager instance;
    public static CupStackManager Instance => instance;
    private CupPatternData currentPattern;
    public CupPatternData CurrentPattern => currentPattern;

    [SerializeField] private float cupDiameter = 2f;
    [SerializeField] private float toleranceFactor = 0.75f;
    private float tolerance => cupDiameter * toleranceFactor;

    private Dictionary<Vector2Int, GameObject> placedCups = new Dictionary<Vector2Int, GameObject>();
    private List<Vector2Int> passedSubset = new List<Vector2Int>();

    private bool passInProgress = false;
    public bool PassInProgress => passInProgress;


    GenerateParticles generatrParticles;

    [Header("Fallen Monitor")]
    [Tooltip("이 각도보다 기울면 누운 의심")]
    [SerializeField] private float maxTiltDeg = 20f;
    [Tooltip("연속 의심 누적 시간 리밋")]
    [SerializeField] private float fallConfirmTime = 0.25f;
    [Tooltip("검사 주기")]
    [SerializeField] private float pollInterval = 0.05f;

    private int remainingStacks;
    public int RemainingStacks
    {
        get => remainingStacks;
        set => remainingStacks = value;
    }

    private class CupTrack
    {
        public GameObject go;
        public Rigidbody rb;
        public float notUprightAccum;
    }

    
    private Dictionary<GameObject, CupTrack> cupTracks = new Dictionary<GameObject, CupTrack>();
    private HashSet<GameObject> successLock = new HashSet<GameObject>();

    private Coroutine monitorRoutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        generatrParticles = FindFirstObjectByType<GenerateParticles>();
    }

    private void OnEnable()
    {
        if (monitorRoutine == null)
            monitorRoutine = StartCoroutine(MonitorCupsRoutine());
    }

    private void OnDisable()
    {
        if (monitorRoutine != null)
        {
            StopCoroutine(monitorRoutine);
            monitorRoutine = null;
        }
    }

    public void SetPattern(CupPatternData pattern)
    {
        passInProgress = false;
        currentPattern = pattern;
        ClearPlacedCups();
        remainingStacks = currentPattern.MaxStack;
        Debug.Log($"새로운 패턴이 설정되었습니다 {currentPattern.name}");
    }

    public void RegisterCup(Vector3 worldPos, GameObject cupObject)
    {
        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt(worldPos.x),
            Mathf.RoundToInt(worldPos.y)
        );

        if (!placedCups.ContainsKey(gridPos))
        {
            placedCups.Add(gridPos, cupObject);

            if (cupObject != null && !cupTracks.ContainsKey(cupObject))
            {
                var rb = cupObject.GetComponent<Rigidbody>();
                cupTracks[cupObject] = new CupTrack
                {
                    go = cupObject,
                    rb = rb,
                    notUprightAccum = 0f
                };
            }

            CheckForPass();
            if (passInProgress) return;

            // 여기
            EvaluateFeasibility();
        }
    }

    private void CheckForPass()
    {
        if (currentPattern == null)
        {
            Debug.LogWarning("currentPattern이 설정되지 않았습니다.");
            return;
        }

        int requiredCount = currentPattern.cupPositions.Count;

        // null 컵 제거
        Dictionary<Vector2Int, GameObject> filtered = new Dictionary<Vector2Int, GameObject>();
        foreach (var kvp in placedCups)
        {
            if (kvp.Value != null)
            {
                filtered.Add(kvp.Key, kvp.Value);
            }
        }
        placedCups = filtered;

        if (placedCups.Count < requiredCount) return;

        List<Vector2Int> placedPositions = placedCups.Keys.ToList();
        List<Vector2Int> normalizedPattern = NormalizePositions(currentPattern.cupPositions);
        var combinations = GetCombinations(placedPositions, requiredCount);

        foreach (var subset in combinations)
        {
            var normalizedSubset = NormalizePositions(subset);

            bool matched = true;
            foreach (Vector2Int target in normalizedPattern)
            {
                bool foundMatch = false;

                foreach (Vector2Int actual in normalizedSubset)
                {
                    if (Vector2.Distance(actual, target) < tolerance)
                    {
                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch)
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                passedSubset = new List<Vector2Int>(subset);
                Debug.Log("PASS");

                foreach (var pos in passedSubset)
                {
                    if (placedCups.TryGetValue(pos, out GameObject cupObj) && cupObj != null)
                    {
                        Rigidbody rb = cupObj.GetComponent<Rigidbody>();
                        MeshCollider collider = rb != null ? rb.GetComponent<MeshCollider>() : null;
                        if (collider != null) collider.isTrigger = true;
                        if (rb != null) rb.useGravity = false;

                        successLock.Add(cupObj);
                    }
                }

                passInProgress = true;
                StartCoroutine(SuccessEffectAndDestroyRoutine());
                generatrParticles.GenerateParticle();
                scoreManager.Instance.AddScore(1);
                return;
            }
        }
    }

    private IEnumerator SuccessEffectAndDestroyRoutine()
    {
        yield return null;

        ClearPlacedCups();
        GameManager.Instance.OnPatternPassed();
    }


    private List<Vector2Int> NormalizePositions(List<Vector2Int> positions)
    {
        int minX = positions.Min(p => p.x);
        int minY = positions.Min(p => p.y);

        return positions
            .Select(p => new Vector2Int(p.x - minX, p.y - minY))
            .ToList();
    }

    private List<List<Vector2Int>> GetCombinations(List<Vector2Int> list, int size)
    {
        List<List<Vector2Int>> result = new List<List<Vector2Int>>();
        GetCombinationsRecursive(list, size, 0, new List<Vector2Int>(), result);
        return result;
    }

    private void GetCombinationsRecursive(List<Vector2Int> list, int size, int index,
        List<Vector2Int> current, List<List<Vector2Int>> result)
    {
        if (current.Count == size)
        {
            result.Add(new List<Vector2Int>(current));
            return;
        }

        for (int i = index; i < list.Count; i++)
        {
            current.Add(list[i]);
            GetCombinationsRecursive(list, size, i + 1, current, result);
            current.RemoveAt(current.Count - 1);
        }
    }

    private void ClearPlacedCups()
    {
        foreach (var cup in placedCups.Values)
        {
            if (cup != null) Destroy(cup);
        }

        placedCups.Clear();
        passedSubset.Clear();
        cupTracks.Clear();
        successLock.Clear();
    }

    
    private IEnumerator MonitorCupsRoutine()
    {
        var wait = new WaitForSeconds(pollInterval);

        while (true)
        {
            var keys = cupTracks.Keys.ToList();
            foreach (var key in keys)
            {
                if (key == null)
                {
                    cupTracks.Remove(key);
                    continue;
                }

                // 이미 성공한 패턴들 락부여
                if (successLock.Contains(key))
                    continue;

                var track = cupTracks[key];
                var t = track.go.transform;

                // 여기서 기울기를 계산함
                float tilt = Vector3.Angle(t.up, Vector3.up);
                bool looksNotUpright = tilt > maxTiltDeg;

                if (looksNotUpright)
                {
                    track.notUprightAccum += pollInterval;

                    if (track.notUprightAccum >= fallConfirmTime)
                    {
                        OnCupFallen(track.go);
                        
                        continue;
                    }
                }
                else
                {
                    track.notUprightAccum = 0f;
                }
            }

            yield return wait;
        }
    }

    private void OnCupFallen(GameObject cup)
    {
        if (cup == null) return;

        var rb = cup.GetComponent<Rigidbody>();
        var col = cup.GetComponent<MeshCollider>();
        if (col != null) col.isTrigger = false;
        if (rb != null) rb.useGravity = true;

        Vector2Int? keyToRemove = null;
        foreach (var kv in placedCups)
        {
            if (kv.Value == cup)
            {
                keyToRemove = kv.Key;
                break;
            }
        }
        if (keyToRemove.HasValue)
        {
            placedCups.Remove(keyToRemove.Value);
        }
        
        cupTracks.Remove(cup);

        if (passedSubset.Count > 0)
        {
            Vector2Int approx = new Vector2Int(
                Mathf.RoundToInt(cup.transform.position.x),
                Mathf.RoundToInt(cup.transform.position.y)
            );
            passedSubset.Remove(approx);
        }

        CheckForPass();

        // 여기
        EvaluateFeasibility();
    }

    // gpt thanks
    private int FindBestOverlap(out HashSet<Vector2Int> bestMatchedKeys)
    {
        bestMatchedKeys = new HashSet<Vector2Int>();

        if (currentPattern == null || placedCups.Count == 0)
            return 0;

        // 패턴 좌표와 배치 좌표
        var pattern = currentPattern.cupPositions;
        var placed = placedCups.Keys.ToList();

        int best = 0;

        // a: 배치된 컵 하나, p: 패턴의 한 점
        // offset = a - p 를 기준으로 모든 배치 컵을 패턴 좌표계로 평행이동했을 때
        // 패턴 점들과 몇 개가 일치하는지 센다.
        for (int i = 0; i < placed.Count; i++)
        {
            var a = placed[i];
            for (int j = 0; j < pattern.Count; j++)
            {
                var p = pattern[j];
                Vector2Int offset = a - p;

                int count = 0;
                var matchedNow = new HashSet<Vector2Int>();

                // 평행이동된 각 배치 좌표가 패턴의 어느 좌표와 허용 오차 내로 일치하면 매치로 본다.
                foreach (var a2 in placed)
                {
                    Vector2Int shifted = a2 - offset; // 패턴 좌표계로 이동

                    bool found = false;
                    foreach (var pt in pattern)
                    {
                        // 기존 로직과 동일하게 tolerance 사용
                        if (Vector2.Distance((Vector2)shifted, (Vector2)pt) < tolerance)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        count++;
                        matchedNow.Add(a2);
                    }
                }

                if (count > best)
                {
                    best = count;
                    bestMatchedKeys = matchedNow;
                }
            }
        }

        return best;
    }

    // 지금 배치된 컵들 중에서 "가장 유효한" 컵만 추려 반환한다.
    // 컵 오브젝트까지 함께 돌려준다.
    private Dictionary<Vector2Int, GameObject> GetValidCupsForCurrentPattern()
    {
        var result = new Dictionary<Vector2Int, GameObject>();
        if (currentPattern == null || placedCups.Count == 0)
            return result;

        var matched = new HashSet<Vector2Int>();
        FindBestOverlap(out matched);

        foreach (var key in matched)
        {
            if (placedCups.TryGetValue(key, out var go) && go != null)
                result[key] = go;
        }
        return result;
    }

    // 완주 가능성 평가.
    // 유효 매치 수 + 남은 배치 가능 수 < 요구 개수 이면 게임오버.
    public void EvaluateFeasibility()
    {
        if (currentPattern == null) return;
        if (passInProgress) return;

        int required = currentPattern.cupPositions.Count;

        // 현재 배치 중에서 최대로 맞출 수 있는 수
        HashSet<Vector2Int> matchedKeys;
        int matched = FindBestOverlap(out matchedKeys);

        // 남은 배치 가능 수는 remainingStacks 를 사용한다.
        // 필요하면 RegisterCup 에서 배치 시 remainingStacks--; 로 관리해라.
        int potentialMax = matched + Mathf.Max(0, remainingStacks);

        Debug.Log($"잘하면 이만큼 쌓아{potentialMax}");
        Debug.Log($"지금까지 유효한 컵의 수야{matched}");

        // 더 해도 못 채우면 불가능 판정
        if (potentialMax < required)
        {
            Debug.Log("패턴 완성이 불가능합니다. 게임오버 처리.");
            GameManager.Instance.OnPatternFailed();
        }
    }

}
