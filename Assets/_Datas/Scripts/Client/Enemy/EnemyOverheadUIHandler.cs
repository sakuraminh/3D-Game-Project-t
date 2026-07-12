using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Shared;

namespace Client
{
    /// <summary>
    /// Component Client quản lý Canvas hiển thị máu nổi trên đầu quái vật và tải mô hình 3D động từ Addressables.
    /// </summary>
    [RequireComponent(typeof(EnemyNetworkData))]
    public class EnemyOverheadUIHandler : NetworkBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas _overheadCanvas;
        [SerializeField] private Slider _hpSlider;

        private EnemyNetworkData _networkData;
        private Transform _mainCameraTransform;
        private GameObject _loadedPrefab;
        private GameObject _instantiatedModel;

        private void Awake()
        {
            // Caching components một lần duy nhất tại Awake (Memory Optimization)
            _networkData = GetComponent<EnemyNetworkData>();

            if (_overheadCanvas == null)
            {
                _overheadCanvas = GetComponentInChildren<Canvas>(true);
            }

            if (_overheadCanvas != null && _hpSlider == null)
            {
                _hpSlider = _overheadCanvas.GetComponentInChildren<Slider>(true);
            }
        }

        private void Start()
        {
            // Cache Main Camera transform
            if (Camera.main != null)
            {
                _mainCameraTransform = Camera.main.transform;
            }

            // Tải mô hình 3D của quái vật bất đồng bộ qua hệ thống Addressables sử dụng ResourceManager
            if (_networkData != null && _networkData.Config != null)
            {
                string key = _networkData.Config.ModelAddressableKey;
                if (!string.IsNullOrEmpty(key))
                {
                    ResourceManager.LoadAssetAsync<GameObject>(key, OnModelLoaded);
                }
            }
        }

        /// <summary>
        /// Callback nhận kết quả khi mô hình 3D được tải thành công từ Addressables.
        /// </summary>
        private void OnModelLoaded(GameObject modelPrefab)
        {
            if (modelPrefab == null || this == null) return;

            // Đề phòng vòng lặp vô hạn (Recursive Spawn Loop)
            if (modelPrefab.GetComponent<EnemyOverheadUIHandler>() != null || 
                modelPrefab.GetComponent<EnemyNetworkData>() != null)
            {
                Debug.LogError($"[EnemyOverheadUIHandler] Cảnh báo cấu hình lỗi: ModelAddressableKey '{_networkData.Config.ModelAddressableKey}' trên Config '{_networkData.Config.name}' đang trỏ tới chính Prefab của Enemy mạng (gây vòng lặp vô hạn)! Đã dừng việc Instantiate.");
                return;
            }

            _loadedPrefab = modelPrefab;
            
            // Tạo bản sao mô hình và gán làm con của Enemy GameObject
            _instantiatedModel = Instantiate(modelPrefab, transform);
            _instantiatedModel.transform.localPosition = Vector3.zero;
            _instantiatedModel.transform.localRotation = Quaternion.identity;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (_networkData != null)
            {
                // Đăng ký lắng nghe sự kiện thay đổi máu từ NetworkVariable của EnemyNetworkData (Observer Pattern)
                _networkData.CurrentHP.OnValueChanged += HandleHealthChanged;
                _networkData.MaxHP.OnValueChanged += HandleMaxHealthChanged;

                // Đồng bộ chỉ số máu ban đầu
                UpdateHPDisplay(_networkData.CurrentHP.Value, _networkData.MaxHP.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (_networkData != null)
            {
                // Hủy đăng ký sự kiện tránh rò rỉ bộ nhớ (Memory Leak)
                _networkData.CurrentHP.OnValueChanged -= HandleHealthChanged;
                _networkData.MaxHP.OnValueChanged -= HandleMaxHealthChanged;
            }
            base.OnNetworkDespawn();
        }

        private void LateUpdate()
        {
            // Cập nhật Camera nếu bị mất tham chiếu
            if (_mainCameraTransform == null && Camera.main != null)
            {
                _mainCameraTransform = Camera.main.transform;
            }

            // Billboard: Xoay World-Space Canvas luôn hướng thẳng về phía Camera
            if (_overheadCanvas != null && _mainCameraTransform != null)
            {
                _overheadCanvas.transform.rotation = _mainCameraTransform.rotation;
            }
        }

        private void HandleHealthChanged(float oldValue, float newValue)
        {
            UpdateHPDisplay(newValue, _networkData.MaxHP.Value);
        }

        private void HandleMaxHealthChanged(float oldValue, float newValue)
        {
            UpdateHPDisplay(_networkData.CurrentHP.Value, newValue);
        }

        /// <summary>
        /// Cập nhật giá trị hiển thị trên thanh máu (Slider).
        /// </summary>
        private void UpdateHPDisplay(float current, float max)
        {
            if (_hpSlider != null)
            {
                _hpSlider.maxValue = max;
                _hpSlider.value = current;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            // Giải phóng tài nguyên Addressable khỏi bộ nhớ RAM để chống tràn (Memory Leak)
            if (_loadedPrefab != null)
            {
                ResourceManager.ReleaseAsset(_loadedPrefab);
            }
        }
    }
}
