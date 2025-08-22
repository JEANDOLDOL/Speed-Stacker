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
            Debug.LogWarning("먼저 컵들을 포함한 GameObject를 선택하세요.");
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

        // ScriptableObject 생성
        CupPatternData pattern = ScriptableObject.CreateInstance<CupPatternData>();
        pattern.cupPositions = cupPositions;

        // 저장 경로 선택
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Cup Pattern", "NewCupPattern", "asset", "패턴을 저장할 위치를 선택하세요.");

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(pattern, path);
            AssetDatabase.SaveAssets();
            Debug.Log("CupPatternData 생성 완료!");
        }
    }
}
