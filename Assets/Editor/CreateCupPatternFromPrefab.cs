using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CreateCupPatternFromPrefab : MonoBehaviour
{
    [MenuItem("Tools/Cup Pattern/Generate From Selected GameObject")]
    public static void GenerateCupPatternData()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("���� �ŵ��� ������ GameObject�� �����ϼ���.");
            return;
        }

        List<Vector2Int> cupPositions = new List<Vector2Int>();

        foreach (Transform child in selected.transform)
        {
            Vector3 pos = child.position;
            Vector2Int gridPos = new Vector2Int(
                Mathf.RoundToInt(pos.x),
                Mathf.RoundToInt(pos.y)
            );
            cupPositions.Add(gridPos);
        }

        // ScriptableObject ����
        CupPatternData pattern = ScriptableObject.CreateInstance<CupPatternData>();
        pattern.cupPositions = cupPositions;

        // ���� ��� ����
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Cup Pattern", "NewCupPattern", "asset", "������ ������ ��ġ�� �����ϼ���.");

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(pattern, path);
            AssetDatabase.SaveAssets();
            Debug.Log("CupPatternData ���� �Ϸ�!");
        }
    }
}
