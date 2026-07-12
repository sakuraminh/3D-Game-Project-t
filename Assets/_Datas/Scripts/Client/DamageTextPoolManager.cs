using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    /// <summary>
    /// Singleton quản lý Object Pool của các chữ số nhảy sát thương (Floating Damage Text).
    /// Đảm bảo triệt tiêu hành vi Instantiate/Destroy khi chạy gameplay.
    /// </summary>
    public class DamageTextPoolManager : MonoBehaviour
    {
        public static DamageTextPoolManager Instance { get; private set; }

        [Header("Pool Configurations")]
        [SerializeField] private GameObject _damageTextPrefab;
        [SerializeField] private int _initialPoolSize = 25;

        private readonly Queue<GameObject> _poolQueue = new Queue<GameObject>();

        private void Awake()
        {
            // Thiết lập Singleton Pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private Transform _damageTextsRoot;

        private void Start()
        {
            _damageTextsRoot = GetOrCreateLocalEffectsParent("[DamageTexts]");
            InitializePool();
        }

        /// <summary>
        /// Tìm kiếm hoặc tự động sinh GameObject cha cho các hiệu ứng cục bộ.
        /// </summary>
        private Transform GetOrCreateLocalEffectsParent(string parentName)
        {
            // 1. Tìm hoặc tạo root cha [LocalEffects] tại (0,0,0)
            GameObject rootObj = GameObject.Find("[LocalEffects]");
            if (rootObj == null)
            {
                rootObj = new GameObject("[LocalEffects]");
                rootObj.transform.position = Vector3.zero;
                rootObj.transform.rotation = Quaternion.identity;
            }

            // 2. Tìm hoặc tạo sub-root (ví dụ: [DamageTexts]) làm con của [LocalEffects]
            Transform subRoot = rootObj.transform.Find(parentName);
            if (subRoot == null)
            {
                GameObject subRootObj = new GameObject(parentName);
                subRoot = subRootObj.transform;
                subRoot.SetParent(rootObj.transform);
                subRoot.localPosition = Vector3.zero;
                subRoot.localRotation = Quaternion.identity;
            }

            return subRoot;
        }

        /// <summary>
        /// Khởi tạo trước danh sách các đối tượng văn bản đưa vào Pool.
        /// </summary>
        private void InitializePool()
        {
            if (_damageTextPrefab == null)
            {
                Debug.LogError("[DamageTextPoolManager] Damage Text Prefab is not assigned in the Inspector!");
                return;
            }

            for (int i = 0; i < _initialPoolSize; i++)
            {
                GameObject obj = Instantiate(_damageTextPrefab, _damageTextsRoot);
                obj.SetActive(false);
                _poolQueue.Enqueue(obj);
            }
        }

        /// <summary>
        /// Lấy một đối tượng văn bản từ Pool. Nếu Pool rỗng sẽ tự sinh thêm.
        /// </summary>
        public GameObject GetFromPool()
        {
            if (_poolQueue.Count > 0)
            {
                GameObject obj = _poolQueue.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                // Pool bị cạn kiệt do lượng sát thương sinh ra quá dày đặc, tự động tạo thêm
                GameObject obj = Instantiate(_damageTextPrefab, _damageTextsRoot);
                obj.SetActive(true);
                return obj;
            }
        }

        /// <summary>
        /// Trả đối tượng văn bản lại Pool để tái sử dụng.
        /// </summary>
        public void ReturnToPool(GameObject textObj)
        {
            if (textObj != null)
            {
                textObj.SetActive(false);
                _poolQueue.Enqueue(textObj);
            }
        }

        /// <summary>
        /// Kích hoạt và hiển thị số sát thương nhảy tại vị trí chỉ định.
        /// </summary>
        public void SpawnDamageText(int amount, Vector3 position)
        {
            GameObject textObj = GetFromPool();
            if (textObj != null)
            {
                FloatingDamageText damageText = textObj.GetComponent<FloatingDamageText>();
                
                if (damageText != null)
                {
                    damageText.Initialize(amount, position);
                }
                else
                {
                    Debug.LogWarning("[DamageTextPoolManager] Retrieved object does not have FloatingDamageText component attached!");
                    ReturnToPool(textObj);
                }
            }
        }
    }
}
