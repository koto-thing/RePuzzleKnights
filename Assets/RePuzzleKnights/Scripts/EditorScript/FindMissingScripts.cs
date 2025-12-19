using UnityEditor;
using UnityEngine;

namespace RePuzzleKnights.Scripts.EditorScript
{
    public class FindMissingScripts : EditorWindow
    {
        [MenuItem("Tools/Find Missing Scripts")]
        public static void ShowWindow()
        {
            GetWindow<FindMissingScripts>("Find Missing Scripts");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Find Missing Scripts in Scene"))
            {
                FindInScene();
            }
        }

        private static void FindInScene()
        {
            var allObjects = GameObject.FindObjectsOfType<GameObject>(true);
            int missingCount = 0;

            foreach (var go in allObjects)
            {
                var components = go.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        Debug.LogError($"Missing script found on: {GetGameObjectPath(go)}", go);
                        missingCount++;
                    }
                }
            }

            if (missingCount == 0)
            {
                Debug.Log("No missing scripts found!");
            }
            else
            {
                Debug.LogWarning($"Found {missingCount} missing scripts!");
            }
        }

        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }
            return path;
        }
    }
}