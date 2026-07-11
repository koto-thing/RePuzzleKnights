using System;
using R3;
using UnityEngine;
using RePuzzleKnights.Scripts.Domain.Enums;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Soul
{
    /// <summary>
    /// ステージ上にスポーンする属性付きSoulオブジェクト。
    /// </summary>
    public class SoulObjectView : MonoBehaviour
    {
        [Header("Soul Settings")]
        [SerializeField] private ElementType element = ElementType.Fire;
        [SerializeField] private int soulValue = 1;
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float floatAmplitude = 0.15f;
        [SerializeField] private float collectAnimationTime = 0.5f;

        private Vector3 _basePosition;
        private bool _isCollected = false;
        private float _collectTimer = 0f;
        private Vector3 _collectStartScale;
        private SpriteRenderer _spriteRenderer;

        public ElementType Element => element;
        public int SoulValue => soulValue;

        private readonly Subject<SoulObjectView> _onCollected = new();
        public Observable<SoulObjectView> OnCollected => _onCollected;

        private void Start()
        {
            _basePosition = transform.position;
            _collectStartScale = transform.localScale;

            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                var visualObj = new GameObject("Visual");
                visualObj.transform.SetParent(this.transform);
                visualObj.transform.localPosition = Vector3.zero;
                _spriteRenderer = visualObj.AddComponent<SpriteRenderer>();
            }

            SetupMaterial();

            if (GetComponent<Collider>() == null)
            {
                var col = gameObject.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 0.5f;
            }
        }

        /// <summary>
        /// 動的生成時に属性とマテリアルを初期化する
        /// </summary>
        public void Initialize(ElementType targetElement)
        {
            element = targetElement;
            
            // Startより先に呼ばれた場合に対応
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (_spriteRenderer == null)
                {
                    var visualObj = new GameObject("Visual");
                    visualObj.transform.SetParent(this.transform);
                    visualObj.transform.localPosition = Vector3.zero;
                    _spriteRenderer = visualObj.AddComponent<SpriteRenderer>();
                }
            }
            
            SetupMaterial();
        }

        private void SetupMaterial()
        {
            if (_spriteRenderer == null) return;

            Shader soulShader = Shader.Find("Custom/Soul");
            if (soulShader != null)
            {
                _spriteRenderer.material = new Material(soulShader);
                
                // 属性ごとに色を設定
                Color outer = Color.white;
                Color mid = Color.white;
                Color inner = Color.white;

                switch (element)
                {
                    case ElementType.Fire:
                        outer = new Color(1.0f, 0.1f, 0.0f, 1.0f);
                        mid = new Color(1.0f, 0.5f, 0.0f, 1.0f);
                        inner = new Color(1.0f, 0.8f, 0.2f, 1.0f);
                        break;
                    case ElementType.Water:
                        outer = new Color(0.0f, 0.2f, 1.0f, 1.0f);
                        mid = new Color(0.0f, 0.6f, 1.0f, 1.0f);
                        inner = new Color(0.5f, 0.9f, 1.0f, 1.0f);
                        break;
                    case ElementType.Grass:
                        outer = new Color(0.0f, 0.8f, 0.1f, 1.0f);
                        mid = new Color(0.2f, 1.0f, 0.3f, 1.0f);
                        inner = new Color(0.7f, 1.0f, 0.7f, 1.0f);
                        break;
                    case ElementType.Light:
                        outer = new Color(1.0f, 0.9f, 0.1f, 1.0f);
                        mid = new Color(1.0f, 1.0f, 0.6f, 1.0f);
                        inner = new Color(1.0f, 1.0f, 0.9f, 1.0f);
                        break;
                    case ElementType.Dark:
                        outer = new Color(0.3f, 0.0f, 0.6f, 1.0f);
                        mid = new Color(0.6f, 0.0f, 0.8f, 1.0f);
                        inner = new Color(0.9f, 0.6f, 1.0f, 1.0f);
                        break;
                    default:
                        outer = new Color(0.0f, 0.6f, 1.0f, 1.0f);
                        mid = new Color(0.0f, 1.0f, 0.8f, 1.0f);
                        inner = new Color(0.8f, 0.9f, 1.0f, 1.0f);
                        break;
                }

                _spriteRenderer.material.SetColor("_ColorOuter", outer);
                _spriteRenderer.material.SetColor("_ColorMid", mid);
                _spriteRenderer.material.SetColor("_ColorInner", inner);
                _spriteRenderer.material.SetColor("_ColorCore", Color.white);
                _spriteRenderer.material.SetFloat("_Speed", 2.0f);
                _spriteRenderer.material.SetFloat("_WobbleStrength", 1.2f);
            }
            else
            {
                Debug.LogWarning("SoulObjectView: Custom/Soul shader not found. Using default material.");
            }

            if (_spriteRenderer.sprite == null)
            {
                Texture2D tex = Texture2D.whiteTexture;
                _spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }

        private void Update()
        {
            if (_isCollected)
            {
                AnimateCollection();
                return;
            }

            Vector3 tempPos = _basePosition;
            tempPos.y += Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = tempPos;

            Billboard();
            CheckMouseOver();
        }

        private void Billboard()
        {
            if (Camera.main != null)
            {
                transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                    Camera.main.transform.rotation * Vector3.up);
            }
        }

        private void CheckMouseOver()
        {
            if (Camera.main == null) return;
 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    Collect();
                    break;
                }
            }
        }

        private void Collect()
        {
            if (_isCollected) return;
            _isCollected = true;
            _collectTimer = 0f;
            
            _onCollected.OnNext(this);
        }

        private void AnimateCollection()
        {
            _collectTimer += Time.deltaTime;
            float t = _collectTimer / collectAnimationTime;

            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += Vector3.up * (Time.deltaTime * 2f);
            transform.localScale = Vector3.Lerp(_collectStartScale, Vector3.zero, t);
        }

        private void OnDestroy()
        {
            _onCollected.OnCompleted();
            _onCollected.Dispose();
        }
    }
}
