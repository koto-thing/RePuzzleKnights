using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RePuzzleKnights.Scripts.EditorScript
{
    public class NameReplacer : EditorWindow
    {
        [SerializeField]
        private List<GameObject> targetObjects = new List<GameObject>();
        private string newName = "GameObject_{sequence}";

        private SerializedObject serializedObject;
        private SerializedProperty targetObjectsProperty;

        [MenuItem("Tools/Name Replacer")]
        public static void ShowWindow()
        {
            GetWindow<NameReplacer>("Name Replacer");
        }

        /// <summary>
        /// エディタウィンドウが有効になったときに呼び出される
        /// </summary>
        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            // フィールド名でプロパティを検索する
            targetObjectsProperty = serializedObject.FindProperty("targetObjects");
        }

        /// <summary>
        /// エディタウィンドウのGUIを描画する
        /// </summary>
        private void OnGUI()
        {
            serializedObject.Update();

            GUILayout.Label("GameObject Name Replacer", EditorStyles.boldLabel);

            // リスト形式でGameObjectを設定するフィールドを描画
            EditorGUILayout.PropertyField(targetObjectsProperty, new GUIContent("Target GameObjects"), true);

            newName = EditorGUILayout.TextField("New Name Pattern", newName);
            EditorGUILayout.HelpBox("名前に {sequence} を含めると、01から始まる2桁の連番に置き換えられます。", MessageType.Info);

            // ボタンを横並びに配置
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Replace Names"))
            {
                ReplaceObjectNames();
            }

            // リストをリセットするボタン
            if (GUILayout.Button("Reset List"))
            {
                ResetList();
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// GameObjectの名前を変更する処理
        /// </summary>
        private void ReplaceObjectNames()
        {
            if (targetObjects == null || targetObjects.Count == 0)
            {
                Debug.LogError("対象のGameObjectが指定されていません。");
                return;
            }

            if (string.IsNullOrEmpty(newName))
            {
                Debug.LogError("新しい名前のパターンが入力されていません。");
                return;
            }

            Undo.RecordObjects(targetObjects.ToArray(), "Rename Multiple GameObjects");

            int sequence = 1;
            foreach (var targetObject in targetObjects)
            {
                if (targetObject == null)
                {
                    continue;
                }

                // {sequence} を0埋め2桁の連番に置き換える
                string finalName = newName.Replace("{sequence}", sequence.ToString("D2"));
                targetObject.name = finalName;
                sequence++;
            }

            Debug.Log($"{targetObjects.Count}個のGameObjectの名前を変更しました。");
        }

        /// <summary>
        /// GameObjectのリストをクリアする
        /// </summary>
        private void ResetList()
        {
            if (targetObjects.Count > 0)
            {
                // SerializedProperty を直接操作して配列をクリアする
                targetObjectsProperty.ClearArray();
                Debug.Log("Target GameObjects list has been cleared.");
            }
        }
    }
}