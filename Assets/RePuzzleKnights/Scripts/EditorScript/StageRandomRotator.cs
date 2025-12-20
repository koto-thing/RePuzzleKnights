using UnityEditor;
using UnityEngine;

namespace RePuzzleKnights.Scripts.EditorScript
{
    public class StageRandomRotator
    {
        [MenuItem("Tools/Randomly Rotate Selected Stage Objects")]
        private static void RotateStageObjects()
        {
            // 何も選択されていなければ終了
            if (Selection.gameObjects.Length == 0)
            {
                Debug.LogWarning("回転させるオブジェクトを選択してください。");
                return;
            }

            // 記録グループを作成
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Random Rotate Y");
            var undoGroupIndex = Undo.GetCurrentGroup();

            int count = 0;

            foreach (GameObject obj in Selection.gameObjects)
            {
                // Undoシステムに登録
                Undo.RecordObject(obj.transform, "Random Rotate Object");

                // 現在の角度を取得
                Vector3 currentEuler = obj.transform.localEulerAngles;

                // 0, 90, 180, 270のいずれかをランダムに選択
                float randomY = Random.Range(0, 4) * 90f;

                // Y軸だけ変更し、XとZは維持する
                obj.transform.localEulerAngles = new Vector3(currentEuler.x, randomY, currentEuler.z);
                
                count++;
            }

            // Undoグループを閉じる
            Undo.CollapseUndoOperations(undoGroupIndex);

            Debug.Log($"{count} 個のオブジェクトをランダムに回転させました。");
        }
    }
}