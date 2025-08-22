using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCupPattern", menuName = "CupGame/Cup Pattern")]
public class CupPatternData : ScriptableObject
{
    [Tooltip("컵이 위치해야 할 좌표 리스트")]
    public List<Vector2Int> cupPositions;
    [Tooltip("이 횟수 내에 패턴을 완료해야 함")]
    public int MaxStack;
    public int CupCount => cupPositions != null ? cupPositions.Count : 0;
}
