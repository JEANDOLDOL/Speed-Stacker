using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCupPattern", menuName = "CupGame/Cup Pattern")]
public class CupPatternData : ScriptableObject
{
    [Tooltip("���� ��ġ�ؾ� �� ��ǥ ����Ʈ")]
    public List<Vector2Int> cupPositions;
    [Tooltip("�� Ƚ�� ���� ������ �Ϸ��ؾ� ��")]
    public int MaxStack;
    public int CupCount => cupPositions != null ? cupPositions.Count : 0;
}
