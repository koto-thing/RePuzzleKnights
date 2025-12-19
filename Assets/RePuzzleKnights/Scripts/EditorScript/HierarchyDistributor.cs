using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RePuzzleKnights.Scripts.EditorScript
{
    public class FixedDistanceDistributor : EditorWindow
    {
        // デフォルトの設定値
        private float spacing = 1.0f;
        private Vector3 direction = Vector3.right;
    
        /// <summary>
        /// メニューからウィンドウを開く
        /// </summary>
        [MenuItem("Tools/Distribute Objects")]
        public static void ShowWindow()
        {
            GetWindow<FixedDistanceDistributor>("Fixed Distribute");
        }

        private void OnGUI()
        {
            // ウィンドウのタイトルと説明
            GUILayout.Label("等間隔配置ツール", EditorStyles.boldLabel);
            GUILayout.Space(5);
        
            // 設定フィールド
            spacing = EditorGUILayout.FloatField("間隔 (m)", spacing);
            direction = EditorGUILayout.Vector3Field("方向ベクトル", direction);

            // 方向プリセットボタン
            GUILayout.Space(5);
            GUILayout.Label("方向プリセット:", EditorStyles.miniLabel);
        
            // 方向選択ボタン
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("X軸 (横)")) direction = Vector3.right;
            if (GUILayout.Button("Z軸 (奥)")) direction = Vector3.forward;
            if (GUILayout.Button("Y軸 (縦)")) direction = Vector3.up;
            EditorGUILayout.EndHorizontal();
        
            // 自動計算ボタン
            if (GUILayout.Button("選択した2つの向きに合わせる (Auto)"))
            {
                CalculateDirectionFromSelection();
            }

            // 操作セクション
            GUILayout.Space(10);
            GUILayout.Label("操作:", EditorStyles.boldLabel);
        
            // 実行ボタン
            if (GUILayout.Button("配置を実行 (ヒエラルキー順)", GUILayout.Height(30)))
            {
                Distribute();
            }
        }

        /// <summary>
        /// 選択したオブジェクトの位置から方向ベクトルと間隔を計算する
        /// </summary>
        private void CalculateDirectionFromSelection()
        {
            // 選択オブジェクトの取得
            Transform[] selection = Selection.transforms;
            if (selection.Length < 2)
            {
                Debug.LogWarning("方向を計算するには2つ以上のオブジェクトを選択してください。");
                return;
            }
        
            // ヒエラルキー順にソートして方向と間隔を計算
            var sorted = selection.OrderBy(t => t.GetSiblingIndex()).ToList();
            direction = (sorted[1].position - sorted[0].position).normalized;
            spacing = Vector3.Distance(sorted[0].position, sorted[1].position);
        }

        /// <summary>
        /// 選択したオブジェクトを指定した間隔と方向で等間隔に配置する
        /// </summary>
        private void Distribute()
        {
            // 選択オブジェクトの取得
            Transform[] selection = Selection.transforms;
            if (selection.Length < 1) 
                return;
        
            // ヒエラルキー順にソートしてUndo登録
            var sortedList = selection.OrderBy(t => t.GetSiblingIndex()).ToList();
            Undo.RecordObjects(sortedList.ToArray(), "Distribute Fixed Distance");
        
            // 開始位置と正規化方向ベクトルの計算
            Vector3 startPos = sortedList[0].position;
            Vector3 dirNormalized = direction.normalized;

            // オブジェクトの配置
            for (int i = 0; i < sortedList.Count; i++)
            {
                sortedList[i].position = startPos + (dirNormalized * spacing * i);
            }
        
            Debug.Log($"{sortedList.Count}個のオブジェクトを {spacing}m 間隔で配置しました。");
        }
    }
}