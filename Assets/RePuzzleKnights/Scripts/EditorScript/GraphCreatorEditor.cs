using RePuzzleKnights.Scripts.InGame.PathFinder;
using UnityEditor;
using UnityEngine;

namespace RePuzzleKnights.Scripts.EditorScript
{
    [CustomEditor(typeof(GraphCreator))]
    public class GraphCreatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GraphCreator creator = (GraphCreator)target;
            GUILayout.Space(10);

            if (GUILayout.Button("シーン中で選択中のオブジェクトをリストに追加"))
            {
                AddSelectedObjects(creator);
            }
        }

        private void AddSelectedObjects(GraphCreator creator)
        {
            Undo.RecordObject(creator, "Add Selected Objects");

            SerializedProperty listProperty = serializedObject.FindProperty("blockGameObjects");
            foreach (GameObject obj in Selection.gameObjects)
            {
                bool alreadyExists = false;
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    if (listProperty.GetArrayElementAtIndex(i).objectReferenceValue == obj)
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    int index = listProperty.arraySize;
                    listProperty.InsertArrayElementAtIndex(index);
                    listProperty.GetArrayElementAtIndex(index).objectReferenceValue = obj;
                }
            }

            serializedObject.ApplyModifiedProperties();
            Debug.Log($"{Selection.gameObjects.Length}個のオブジェクトをリストに追加しました。");
        }
    }
}