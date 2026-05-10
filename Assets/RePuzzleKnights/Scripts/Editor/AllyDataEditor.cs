using UnityEngine;
using UnityEditor;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using System.Collections.Generic;

namespace RePuzzleKnights.Scripts.Editor
{
    /// <summary>
    /// AllyDataSO および SoulDataSO のためのカスタムエディタ。
    /// 攻撃範囲をグリッドUIで直感的に設定できるようにする。
    /// </summary>
    [CustomEditor(typeof(AllyDataSO), true)]
    public class AllyDataEditor : UnityEditor.Editor
    {
        private const int GridSize = 7; // 7x7 grid
        private const int HalfSize = GridSize / 2;

        public override void OnInspectorGUI()
        {
            // デフォルトのインスペクターを表示
            serializedObject.Update();
            
            // 全てのプロパティを順番に描画するが、AttackRangeGrids だけカスタム描画したい場合は
            // DrawPropertiesExcluding を使うのが一般的
            DrawPropertiesExcluding(serializedObject, "AttackRangeGrids");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("攻撃範囲設定 (グリッド)", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.HelpBox("中心(青)が味方の位置、赤色が攻撃対象マスです。\nクリックで切り替えます。上方向が正面(+Z方向)です。", MessageType.Info);

                AllyDataSO data = (AllyDataSO)target;
                if (data.AttackRangeGrids == null)
                {
                    data.AttackRangeGrids = new List<Vector2Int>();
                }

                float buttonSize = 35f;
                
                // グリッド描画
                for (int y = HalfSize; y >= -HalfSize; y--)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        for (int x = -HalfSize; x <= HalfSize; x++)
                        {
                            Vector2Int pos = new Vector2Int(x, y);
                            bool isActive = IsInGrid(data.AttackRangeGrids, pos);
                            bool isCenter = (x == 0 && y == 0);

                            GUI.backgroundColor = isCenter ? Color.cyan : (isActive ? Color.red : Color.white);
                            
                            string label = isCenter ? "C" : "";
                            if (GUILayout.Button(label, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize)))
                            {
                                ToggleGrid(data, pos);
                            }
                        }
                        GUILayout.FlexibleSpace();
                    }
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.Space();
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("全クリア", GUILayout.Width(100)))
                    {
                        ClearGrid(data);
                    }
                    if (GUILayout.Button("正面1マスのみ", GUILayout.Width(120)))
                    {
                        ResetToDefault(data);
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private bool IsInGrid(List<Vector2Int> grids, Vector2Int pos)
        {
            return grids.Exists(p => p.x == pos.x && p.y == pos.y);
        }

        private void ToggleGrid(AllyDataSO data, Vector2Int pos)
        {
            Undo.RecordObject(data, "Toggle Attack Range Grid");
            
            int index = data.AttackRangeGrids.FindIndex(p => p.x == pos.x && p.y == pos.y);
            if (index >= 0)
            {
                data.AttackRangeGrids.RemoveAt(index);
            }
            else
            {
                data.AttackRangeGrids.Add(pos);
            }
            
            EditorUtility.SetDirty(data);
        }

        private void ClearGrid(AllyDataSO data)
        {
            Undo.RecordObject(data, "Clear Attack Range Grid");
            data.AttackRangeGrids.Clear();
            EditorUtility.SetDirty(data);
        }

        private void ResetToDefault(AllyDataSO data)
        {
            Undo.RecordObject(data, "Reset Attack Range Grid");
            data.AttackRangeGrids.Clear();
            data.AttackRangeGrids.Add(new Vector2Int(0, 1));
            EditorUtility.SetDirty(data);
        }
    }
}
