using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.UI
{
    /// <summary>
    /// 自作のオリジナルプロシージャルシェーダー（Burn, Slow, Stun, Regen）を適用した
    /// Quadメッシュオブジェクトを動的にビルドし、状態異常エフェクトとして描画するファクトリクラス。
    /// 完全に同期的に生成され、Z軸の手前オフセットを適用して2Dスプライトの手前に確実に描画します。
    /// </summary>
    public static class EffectVisualFactory
    {
        private static Mesh _quadMesh;

        private static Mesh GetQuadMesh()
        {
            if (_quadMesh == null)
            {
                _quadMesh = new Mesh();
                _quadMesh.vertices = new Vector3[] {
                    new Vector3(-0.5f, -0.5f, 0f),
                    new Vector3(0.5f, -0.5f, 0f),
                    new Vector3(-0.5f, 0.5f, 0f),
                    new Vector3(0.5f, 0.5f, 0f)
                };
                _quadMesh.uv = new Vector2[] {
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0f),
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f)
                };
                _quadMesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
                _quadMesh.RecalculateNormals();
            }
            return _quadMesh;
        }

        private static GameObject CreateEffectObject(Transform parent, string shaderName, string objectName, Vector3 localPos, Vector3 localScale)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;

            var filter = go.AddComponent<MeshFilter>();
            filter.mesh = GetQuadMesh();

            var renderer = go.AddComponent<MeshRenderer>();
            Shader shader = Shader.Find(shaderName);
            if (shader != null)
            {
                renderer.material = new Material(shader);
            }
            else
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = Color.magenta;
                Debug.LogWarning($"[EffectVisualFactory] Custom shader not found: {shaderName}. Fallback applied.");
            }

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            return go;
        }

        public static UniTask<GameObject> CreateBurnEffectAsync(Transform parent)
        {
            // 炎上エフェクト: メラメラ燃える縦長の炎 (1.2 x 1.6)
            var go = CreateEffectObject(parent, "Custom/Effects/Burn", "BurnEffect_Procedural", new Vector3(0f, 0.45f, -0.2f), new Vector3(1.2f, 1.6f, 1f));
            return UniTask.FromResult(go);
        }

        public static UniTask<GameObject> CreateSlowEffectAsync(Transform parent)
        {
            // 減速エフェクト: 足元で回転・脈動する氷の魔方陣結晶 (1.4 x 1.4)
            var go = CreateEffectObject(parent, "Custom/Effects/Slow", "SlowEffect_Procedural", new Vector3(0f, -0.2f, -0.2f), new Vector3(1.4f, 1.4f, 1f));
            return UniTask.FromResult(go);
        }

        public static UniTask<GameObject> CreateStunEffectAsync(Transform parent)
        {
            // 気絶エフェクト: 頭上を回る光輪と星のハロー (1.2 x 0.5 の扁平率で立体回転に見せる)
            var go = CreateEffectObject(parent, "Custom/Effects/Stun", "StunEffect_Procedural", new Vector3(0f, 1.6f, -0.2f), new Vector3(1.2f, 0.5f, 1f));
            return UniTask.FromResult(go);
        }

        public static UniTask<GameObject> CreateRegenEffectAsync(Transform parent)
        {
            // 回復エフェクト: 上昇し消えていくプラス記号と治癒オーラ (1.2 x 1.5)
            var go = CreateEffectObject(parent, "Custom/Effects/Regen", "RegenEffect_Procedural", new Vector3(0f, 0.5f, -0.2f), new Vector3(1.2f, 1.5f, 1f));
            return UniTask.FromResult(go);
        }
    }
}
