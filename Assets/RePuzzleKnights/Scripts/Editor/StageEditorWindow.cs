using System.Collections.Generic;
using System.IO;
using RePuzzleKnights.Scripts.Infrastructure.InGame;
using RePuzzleKnights.Scripts.Infrastructure.InGame.PathFinder;
using RePuzzleKnights.Scripts.Infrastructure.InGame.StageData;
using UnityEditor;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Editor
{
    public class StageEditorWindow : EditorWindow
    {
        // ─── 状態 ───────────────────────────────────────────────────────────

        private StageMapData _mapData;
        private BlockType[,] _grid;
        private BlockType _selectedBlock = BlockType.Ground;
        private Vector2 _scrollPos;
        private int _paintBtn = -1;

        // ヘッダーのサイズ入力フィールドに表示する変更前の値
        private int _pendingW = 10;
        private int _pendingH = 10;

        // GUIStyle キャッシュ（OnGUI で遅延初期化）
        private GUIStyle _titleStyle;
        private GUIStyle _cellLabelStyle;

        // ─── 定数 ────────────────────────────────────────────────────────────

        private const int CellPx = 28;
        private const string DataDir = "Assets/RePuzzleKnights/StageData";
        private const string MatDir  = "Assets/RePuzzleKnights/StageData/Materials";

        private static readonly Color[] BlockColors =
        {
            new Color(0.22f, 0.22f, 0.22f), // 0 Empty
            new Color(0.25f, 0.72f, 0.25f), // 1 Ground
            new Color(1.00f, 0.55f, 0.10f), // 2 HighGround
            new Color(0.90f, 0.18f, 0.18f), // 3 Start
            new Color(0.18f, 0.38f, 0.92f), // 4 Goal
            new Color(0.90f, 0.85f, 0.10f), // 5 NonPlaceable
            new Color(0.35f, 0.05f, 0.55f), // 6 Pitfall
        };

        private static readonly (BlockType type, string label)[] BlockDefs =
        {
            (BlockType.Ground,       "地上"),
            (BlockType.HighGround,   "高台"),
            (BlockType.Start,        "スタート"),
            (BlockType.Goal,         "ゴール"),
            (BlockType.NonPlaceable, "配置不可"),
            (BlockType.Pitfall,      "落とし穴"),
            (BlockType.Empty,        "消しゴム"),
        };

        private static readonly string[] RequiredTags =
            { "START_BLOCK", "GOAL_BLOCK", "GROUND_BLOCK", "HIGHGROUND_BLOCK", "NOPLACE_BLOCK", "PITFALL_BLOCK" };

        // ─── ウィンドウライフサイクル ───────────────────────────────────────

        [MenuItem("RePuzzleKnights/Stage Editor")]
        public static void Open()
        {
            var w = GetWindow<StageEditorWindow>("ステージエディタ");
            w.minSize = new Vector2(600, 540);
            w.Show();
        }

        private void OnEnable()
        {
            if (_mapData == null) _mapData = new StageMapData();
            if (_grid   == null) RebuildGrid(_mapData.width, _mapData.height);
            _pendingW = _mapData.width;
            _pendingH = _mapData.height;
        }

        // ─── メイン GUI ──────────────────────────────────────────────────

        private void OnGUI()
        {
            InitStyles();

            if (_mapData == null) { _mapData = new StageMapData(); }
            if (_grid    == null) RebuildGrid(_mapData.width, _mapData.height);

            DrawHeader();
            EditorGUILayout.Space(4);
            DrawBlockSelector();
            EditorGUILayout.Space(4);
            DrawGrid();
            EditorGUILayout.Space(6);
            DrawBottomBar();
        }

        private void InitStyles()
        {
            if (_titleStyle == null)
                _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                    { fontSize = 13, alignment = TextAnchor.MiddleCenter };

            if (_cellLabelStyle == null)
                _cellLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize  = 9,
                    normal    = { textColor = new Color(1f, 1f, 1f, 0.85f) }
                };
        }

        // ─── ヘッダー: タイトル / ステージ名 / グリッドサイズ ────────────────

        private void DrawHeader()
        {
            GUILayout.Label("RePuzzleKnights  Stage Editor", _titleStyle, GUILayout.Height(24));

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("ステージ名:", GUILayout.Width(72));
            _mapData.stageName = EditorGUILayout.TextField(_mapData.stageName, GUILayout.Width(160));
            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("幅:", GUILayout.Width(18));
            _pendingW = EditorGUILayout.IntField(_pendingW, GUILayout.Width(40));
            EditorGUILayout.LabelField("高さ:", GUILayout.Width(28));
            _pendingH = EditorGUILayout.IntField(_pendingH, GUILayout.Width(40));

            if (GUILayout.Button("リサイズ", EditorStyles.miniButton, GUILayout.Width(62)))
                RebuildGrid(_pendingW, _pendingH);

            EditorGUILayout.EndHorizontal();
        }

        // ─── ブロック種別セレクター ───────────────────────────────────────

        private void DrawBlockSelector()
        {
            EditorGUILayout.BeginHorizontal();
            foreach (var (type, label) in BlockDefs)
            {
                bool sel  = _selectedBlock == type;
                Color col = BlockColors[(int)type];
                GUI.backgroundColor = sel ? Color.Lerp(col, Color.white, 0.45f) : col * 0.82f;

                var btnStyle = new GUIStyle(GUI.skin.button)
                {
                    fontStyle = sel ? FontStyle.Bold : FontStyle.Normal,
                    fontSize  = 11,
                    normal    = { textColor = Color.white }
                };
                if (GUILayout.Button(label, btnStyle, GUILayout.Height(30)))
                    _selectedBlock = type;
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        // ─── グリッド描画 & マウス入力 ────────────────────────────────────

        private void DrawGrid()
        {
            EditorGUILayout.LabelField(
                "左クリック: 配置    右クリック: 消去    ドラッグ: 連続配置",
                EditorStyles.centeredGreyMiniLabel);

            const float labelW = 26f;
            const float labelH = 20f;
            float contentW = labelW + _mapData.width  * CellPx;
            float contentH = labelH + _mapData.height * CellPx;
            float viewH    = Mathf.Min(contentH + 6f, 380f);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(viewH));

            Rect area  = GUILayoutUtility.GetRect(contentW, contentH);
            Event evt  = Event.current;
            bool dirty = false;

            // 列番号（グリッドが広い場合は5列おきに表示）
            for (int x = 0; x < _mapData.width; x++)
            {
                if (x % 5 == 0 || _mapData.width <= 15)
                {
                    EditorGUI.LabelField(
                        new Rect(area.x + labelW + x * CellPx, area.y, CellPx, labelH),
                        x.ToString(), EditorStyles.centeredGreyMiniLabel);
                }
            }

            for (int y = 0; y < _mapData.height; y++)
            {
                // 行番号
                if (y % 5 == 0 || _mapData.height <= 15)
                {
                    EditorGUI.LabelField(
                        new Rect(area.x, area.y + labelH + y * CellPx, labelW, CellPx),
                        y.ToString(), EditorStyles.centeredGreyMiniLabel);
                }

                for (int x = 0; x < _mapData.width; x++)
                {
                    var cellRect = new Rect(
                        area.x + labelW + x * CellPx + 1,
                        area.y + labelH + y * CellPx + 1,
                        CellPx - 2, CellPx - 2);

                    BlockType bt = _grid[x, y];

                    if (evt.type == EventType.Repaint)
                    {
                        EditorGUI.DrawRect(cellRect, BlockColors[(int)bt]);
                        string shortName = ShortName(bt);
                        if (shortName.Length > 0)
                            GUI.Label(cellRect, shortName, _cellLabelStyle);
                    }

                    if (cellRect.Contains(evt.mousePosition))
                    {
                        if (evt.type == EventType.MouseDown)
                        {
                            _paintBtn = evt.button;
                            PaintAt(x, y);
                            dirty = true;
                            evt.Use();
                        }
                        else if (evt.type == EventType.MouseDrag && _paintBtn >= 0)
                        {
                            PaintAt(x, y);
                            dirty = true;
                            evt.Use();
                        }
                    }
                }
            }

            if (evt.type == EventType.MouseUp)
                _paintBtn = -1;

            EditorGUILayout.EndScrollView();

            if (dirty) Repaint();
        }

        private void PaintAt(int x, int y)
        {
            if (_paintBtn == 0)      _grid[x, y] = _selectedBlock;
            else if (_paintBtn == 1) _grid[x, y] = BlockType.Empty;
        }

        private static string ShortName(BlockType t) => t switch
        {
            BlockType.Ground       => "地",
            BlockType.HighGround   => "高",
            BlockType.Start        => "S",
            BlockType.Goal         => "G",
            BlockType.NonPlaceable => "×",
            BlockType.Pitfall      => "穴",
            _                      => ""
        };

        // ─── 下部バー: 統計情報 + アクションボタン ────────────────────────

        private void DrawBottomBar()
        {
            // ブロック数のカウント
            int startCnt = 0, goalCnt = 0, groundCnt = 0, highCnt = 0;
            for (int x = 0; x < _mapData.width; x++)
                for (int y = 0; y < _mapData.height; y++)
                    switch (_grid[x, y])
                    {
                        case BlockType.Start:     startCnt++;  break;
                        case BlockType.Goal:      goalCnt++;   break;
                        case BlockType.Ground:    groundCnt++; break;
                        case BlockType.HighGround: highCnt++;  break;
                    }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            DrawCountLabel($"S:{startCnt}",  BlockType.Start);
            DrawCountLabel($"G:{goalCnt}",   BlockType.Goal);
            DrawCountLabel($"地:{groundCnt}", BlockType.Ground);
            DrawCountLabel($"高:{highCnt}",   BlockType.HighGround);
            GUILayout.FlexibleSpace();

            if (startCnt == 0) DrawWarning("⚠ スタート未配置");
            if (goalCnt  == 0) DrawWarning("⚠ ゴール未配置");

            EditorGUILayout.EndHorizontal();

            // 1行目: 配置 / クリア
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.45f, 0.75f, 1.0f);
            if (GUILayout.Button("シーンに配置", GUILayout.Height(34)))
                PlaceInScene();

            GUI.backgroundColor = new Color(1.0f, 0.45f, 0.45f);
            if (GUILayout.Button("グリッドをクリア", GUILayout.Height(34)))
            {
                if (EditorUtility.DisplayDialog("確認", "グリッドの内容をすべてクリアしますか？", "クリア", "キャンセル"))
                    ClearGrid();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            // 2行目: 保存 / 読み込み
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.40f, 0.85f, 0.40f);
            if (GUILayout.Button("JSON を保存", GUILayout.Height(34)))
                SaveJson();

            GUI.backgroundColor = new Color(1.0f, 0.75f, 0.30f);
            if (GUILayout.Button("JSON を読み込み", GUILayout.Height(34)))
                LoadJson();

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawCountLabel(string text, BlockType type)
        {
            GUI.color = BlockColors[(int)type];
            GUILayout.Label(text, EditorStyles.miniLabel, GUILayout.Width(36));
            GUI.color = Color.white;
        }

        private static void DrawWarning(string text)
        {
            GUI.color = new Color(1f, 0.4f, 0.4f);
            GUILayout.Label(text, EditorStyles.miniLabel);
            GUI.color = Color.white;
        }

        // ─── グリッドユーティリティ ───────────────────────────────────────

        private void RebuildGrid(int w, int h)
        {
            w = Mathf.Clamp(w, 1, 50);
            h = Mathf.Clamp(h, 1, 50);

            var next = new BlockType[w, h];
            if (_grid != null)
            {
                int cw = Mathf.Min(_mapData.width,  w);
                int ch = Mathf.Min(_mapData.height, h);
                for (int x = 0; x < cw; x++)
                    for (int y = 0; y < ch; y++)
                        next[x, y] = _grid[x, y];
            }
            _grid = next;
            _mapData.width  = w;
            _mapData.height = h;
            _pendingW = w;
            _pendingH = h;
        }

        private void ClearGrid()
        {
            for (int x = 0; x < _mapData.width;  x++)
                for (int y = 0; y < _mapData.height; y++)
                    _grid[x, y] = BlockType.Empty;
            Repaint();
        }

        // ─── シーン配置 ───────────────────────────────────────────────────

        private void PlaceInScene()
        {
            // スタート / ゴールの検証
            bool hasStart = false, hasGoal = false;
            for (int x = 0; x < _mapData.width;  x++)
            for (int y = 0; y < _mapData.height; y++)
            {
                if (_grid[x, y] == BlockType.Start) hasStart = true;
                if (_grid[x, y] == BlockType.Goal)  hasGoal  = true;
            }
            if (!hasStart || !hasGoal)
            {
                string msg = (!hasStart ? "スタート " : "") + (!hasGoal ? "ゴール " : "") + "が未配置です。続けますか？";
                if (!EditorUtility.DisplayDialog("警告", msg, "続ける", "キャンセル")) return;
            }

            foreach (var tag in RequiredTags) EnsureTag(tag);

            // 既存の StageRoot を置き換える
            const string rootName = "StageRoot";
            var existing = GameObject.Find(rootName);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("確認", "'StageRoot' を置き換えますか？", "置き換える", "キャンセル")) return;
                Undo.DestroyObjectImmediate(existing);
            }

            var root = new GameObject(rootName);
            root.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(root, "Place Stage");

            if (!Directory.Exists(MatDir)) Directory.CreateDirectory(MatDir);

            var blockObjs = new List<GameObject>();

            for (int y = 0; y < _mapData.height; y++)
            for (int x = 0; x < _mapData.width;  x++)
            {
                BlockType bt = _grid[x, y];
                if (bt == BlockType.Empty) continue;

                string tag = TagFor(bt);
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"{tag}_{x}_{y}";
                go.transform.SetParent(root.transform);
                go.transform.localPosition = new Vector3(x, 0f, y);
                go.transform.localScale    = ScaleFor(bt);

                try   { go.tag = tag; }
                catch { Debug.LogWarning($"[StageEditor] タグ '{tag}' を設定できませんでした。"); }

                int layer = EnsureLayer(LayerNameFor(bt));
                if (layer >= 0) go.layer = layer;


                go.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMat(bt);

                Undo.RegisterCreatedObjectUndo(go, "Place Block");
                blockObjs.Add(go);
            }

            // GraphCreator のセットアップ
            var gc = root.AddComponent<GraphCreator>();
            var so = new SerializedObject(gc);
            var list = so.FindProperty("blockGameObjects");
            list.ClearArray();
            for (int i = 0; i < blockObjs.Count; i++)
            {
                list.InsertArrayElementAtIndex(i);
                list.GetArrayElementAtIndex(i).objectReferenceValue = blockObjs[i];
            }
            so.ApplyModifiedProperties();

            Selection.activeGameObject = root;
            SceneView.lastActiveSceneView?.FrameSelected();

            Debug.Log($"[StageEditor] '{_mapData.stageName}' 配置完了。ブロック数: {blockObjs.Count}");
        }

        private static string TagFor(BlockType t) => t switch
        {
            BlockType.Ground       => "GROUND_BLOCK",
            BlockType.HighGround   => "HIGHGROUND_BLOCK",
            BlockType.Start        => "START_BLOCK",
            BlockType.Goal         => "GOAL_BLOCK",
            BlockType.NonPlaceable => "NOPLACE_BLOCK",
            BlockType.Pitfall      => "PITFALL_BLOCK",
            _                      => "Untagged"
        };

        // 高さ調整はここで
        private static Vector3 ScaleFor(BlockType t) => t switch
        {
            BlockType.HighGround   => new Vector3(1.0f, 1.3f, 1.0f), // 高台
            BlockType.Pitfall      => new Vector3(1.0f, 0.05f, 1.0f), // 落とし穴
            BlockType.NonPlaceable => new Vector3(1.0f, 1.6f, 1.0f), // 配置不可
            _                      => new Vector3(1.0f, 1.0f, 1.0f) // その他
        };

        // BlockType ごとにマテリアルを生成・永続化し、以降の呼び出しで再利用する
        private static Material GetOrCreateMat(BlockType t)
        {
            string path = $"{MatDir}/{t}Mat.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            // HDRP → URP → Standard の順でフォールバック
            var shader = Shader.Find("HDRP/Lit")
                      ?? Shader.Find("Universal Render Pipeline/Lit")
                      ?? Shader.Find("Standard");

            mat = new Material(shader);
            Color col = BlockColors[(int)t];

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     col);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.1f);
            if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic",   0.0f);

            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void EnsureTag(string tagName)
        {
            var tm   = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            var tags = tm.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; i++)
                if (tags.GetArrayElementAtIndex(i).stringValue == tagName) return;

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tagName;
            tm.ApplyModifiedProperties();
            Debug.Log($"[StageEditor] タグ '{tagName}' を追加しました。");
        }

        private static int EnsureLayer(string layerName)
        {
            if (layerName == "Default") return 0;

            int existing = LayerMask.NameToLayer(layerName);
            if (existing != -1) return existing;

            var tm     = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            var layers = tm.FindProperty("layers");
            for (int i = 8; i < layers.arraySize; i++) // 0〜7 は Unity 組み込みレイヤー
            {
                var prop = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(prop.stringValue)) continue;
                prop.stringValue = layerName;
                tm.ApplyModifiedProperties();
                Debug.Log($"[StageEditor] レイヤー '{layerName}' を追加しました (index={i})。");
                return i;
            }
            Debug.LogWarning($"[StageEditor] レイヤー '{layerName}' を追加できませんでした。空きレイヤーがありません。");
            return 0;
        }

        private static string LayerNameFor(BlockType t) => t switch
        {
            BlockType.Ground       => "Ground",
            BlockType.Start        => "Ground",
            BlockType.Goal         => "Ground",
            BlockType.HighGround   => "HighGround",
            _                      => "Default"
        };

        // ─── JSON 入出力 ──────────────────────────────────────────────────

        private void SaveJson()
        {
            _mapData.cells.Clear();
            for (int x = 0; x < _mapData.width;  x++)
            for (int y = 0; y < _mapData.height; y++)
                if (_grid[x, y] != BlockType.Empty)
                    _mapData.cells.Add(new CellData { x = x, y = y, blockType = _grid[x, y] });

            if (!Directory.Exists(DataDir)) Directory.CreateDirectory(DataDir);

            string absDir = Path.GetFullPath(DataDir);
            string path   = EditorUtility.SaveFilePanel("ステージデータを保存", absDir, _mapData.stageName, "json");
            if (string.IsNullOrEmpty(path)) return;

            File.WriteAllText(path, JsonUtility.ToJson(_mapData, prettyPrint: true));
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("保存完了", $"保存しました:\n{path}", "OK");
        }

        private void LoadJson()
        {
            string absDir = Directory.Exists(DataDir) ? Path.GetFullPath(DataDir) : string.Empty;
            string path   = EditorUtility.OpenFilePanel("ステージデータを読み込み", absDir, "json");
            if (string.IsNullOrEmpty(path)) return;

            var loaded = JsonUtility.FromJson<StageMapData>(File.ReadAllText(path));
            if (loaded == null)
            {
                EditorUtility.DisplayDialog("エラー", "読み込みに失敗しました。", "OK");
                return;
            }

            _mapData = loaded;
            RebuildGrid(_mapData.width, _mapData.height);
            foreach (var cell in _mapData.cells)
                if (cell.x >= 0 && cell.x < _mapData.width && cell.y >= 0 && cell.y < _mapData.height)
                    _grid[cell.x, cell.y] = cell.blockType;

            Repaint();
            Debug.Log($"[StageEditor] 読み込み完了: {path}");
        }
    }
}