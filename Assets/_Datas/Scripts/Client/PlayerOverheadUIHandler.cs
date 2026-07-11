using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Shared;

namespace Client
{
    /// <summary>
    /// Component Client chịu trách nhiệm hiển thị thanh máu nổi (Overhead Healthbar) và Tên nhân vật trong không gian 3D.
    /// Toàn bộ các Client đều xử lý và hiển thị thông tin này cho tất cả nhân vật trong phòng.
    /// </summary>
    [RequireComponent(typeof(PlayerNetworkData))]
    public class PlayerOverheadUIHandler : NetworkBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas _overheadCanvas;
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private TMP_Text _nameText;

        private PlayerNetworkData _networkData;
        private Transform _mainCameraTransform;

        private void Awake()
        {
            _networkData = GetComponent<PlayerNetworkData>();
            if (_overheadCanvas == null)
            {
                // includeInactive = true giúp tìm thấy Canvas kể cả khi nó đang bị deactive trong prefab
                _overheadCanvas = GetComponentInChildren<Canvas>(true);
            }
            
            if (_overheadCanvas != null)
            {
                if (_hpSlider == null)
                {
                    _hpSlider = _overheadCanvas.GetComponentInChildren<Slider>(true);
                }
                if (_nameText == null)
                {
                    _nameText = _overheadCanvas.GetComponentInChildren<TMP_Text>(true);
                }
            }
            else
            {
                Debug.LogWarning($"[PlayerOverheadUIHandler] Canvas not found in children of {gameObject.name}!");
            }
        }

        private void Reset()
        {
            _overheadCanvas = GetComponentInChildren<Canvas>(true);
            if (_overheadCanvas != null)
            {
                _hpSlider = _overheadCanvas.GetComponentInChildren<Slider>(true);
                _nameText = _overheadCanvas.GetComponentInChildren<TMP_Text>(true);
            }
        }

        private void Start()
        {
            // Cache Transform của Main Camera tại Start để tránh truy vấn liên tục
            if (Camera.main != null)
            {
                _mainCameraTransform = Camera.main.transform;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Đăng ký lắng nghe sự kiện thay đổi máu từ Server để cập nhật UI nổi
            if (_networkData != null)
            {
                _networkData.CurrentHP.OnValueChanged += OnHPChanged;
                _networkData.MaxHP.OnValueChanged += OnMaxHPChanged;

                // Đồng bộ chỉ số máu ban đầu
                UpdateHPDisplay(_networkData.CurrentHP.Value, _networkData.MaxHP.Value);
            }

            // Gán hiển thị tên Player dựa trên Network Client ID
            if (_nameText != null)
            {
                _nameText.text = IsOwner ? $"Player {OwnerClientId} (You)" : $"Player {OwnerClientId}";
            }
            else
            {
                Debug.LogWarning($"[PlayerOverheadUIHandler] _nameText component is null on Client {OwnerClientId}. Cannot assign player name.");
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            // Hủy đăng ký sự kiện tránh rò rỉ bộ nhớ
            if (_networkData != null)
            {
                _networkData.CurrentHP.OnValueChanged -= OnHPChanged;
                _networkData.MaxHP.OnValueChanged -= OnMaxHPChanged;
            }
        }

        private void LateUpdate()
        {
            // Kiểm tra và lấy lại Camera chính nếu bị thay đổi hoặc chưa cache
            if (_mainCameraTransform == null && Camera.main != null)
            {
                _mainCameraTransform = Camera.main.transform;
            }

            // Cơ chế Billboard: Xoay Canvas World-Space luôn hướng thẳng mặt về Camera
            if (_overheadCanvas != null && _mainCameraTransform != null)
            {
                _overheadCanvas.transform.rotation = _mainCameraTransform.rotation;
            }
        }

        private void OnHPChanged(float oldVal, float newVal)
        {
            UpdateHPDisplay(newVal, _networkData.MaxHP.Value);
        }

        private void OnMaxHPChanged(float oldVal, float newVal)
        {
            UpdateHPDisplay(_networkData.CurrentHP.Value, newVal);
        }

        private void UpdateHPDisplay(float current, float max)
        {
            if (_hpSlider != null)
            {
                _hpSlider.maxValue = max;
                _hpSlider.value = current;
            }
        }
    }
}
