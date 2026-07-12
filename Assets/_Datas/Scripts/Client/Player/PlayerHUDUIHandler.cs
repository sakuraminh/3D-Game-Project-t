using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using Shared;

namespace Client
{
    /// <summary>
    /// Cấu trúc lưu trữ dữ liệu của một vật phẩm trong hòm đồ.
    /// </summary>
    public struct InventoryItem
    {
        public string itemName;
        public int quantity;
    }

    /// <summary>
    /// Component Client quản lý hiển thị HUD chính (HP/MP, Phím tắt, Hòm đồ ảo hóa) sử dụng UI Toolkit.
    /// Chỉ hiển thị và đăng ký sự kiện mạng đối với nhân vật thuộc sở hữu của người chơi hiện tại (IsOwner == true).
    /// </summary>
    [RequireComponent(typeof(PanelRenderer))]
    [RequireComponent(typeof(PlayerNetworkData))]
    public class PlayerHUDUIHandler : NetworkBehaviour
    {
        private PanelRenderer _panelRenderer;
        private PlayerNetworkData _networkData;

        // Các thành phần giao diện đã truy vấn được
        private ProgressBar _hpBar;
        private ProgressBar _mpBar;
        private VisualElement _skillContainer;
        private VisualElement _inventoryPanel;
        private ListView _inventoryListView;

        // Danh sách dữ liệu hòm đồ giả lập
        private readonly List<InventoryItem> _inventoryData = new List<InventoryItem>();

        private void Awake()
        {
            _panelRenderer = GetComponent<PanelRenderer>();
            _networkData = GetComponent<PlayerNetworkData>();

            // Giả lập dữ liệu hòm đồ với 1000 vật phẩm để kiểm nghiệm tính năng ảo hóa (Virtualization)
            for (int i = 1; i <= 1000; i++)
            {
                _inventoryData.Add(new InventoryItem 
                { 
                    itemName = $"Vật phẩm số {i}", 
                    quantity = Random.Range(1, 99) 
                });
            }
        }

        private void OnEnable()
        {
            // Đăng ký callback để xử lý khởi tạo hoặc reload UI của PanelRenderer
            if (_panelRenderer != null)
            {
                _panelRenderer.RegisterUIReloadCallback(OnUIReload);
            }
        }

        private void OnDisable()
        {
            // Hủy đăng ký callback để tránh rò rỉ bộ nhớ
            if (_panelRenderer != null)
            {
                _panelRenderer.UnregisterUIReloadCallback(OnUIReload);
            }
        }

        /// <summary>
        /// Được gọi khi UI được tải lần đầu hoặc nạp lại (Asset reload).
        /// </summary>
        private void OnUIReload(PanelRenderer renderer, VisualElement root)
        {
            if (root != null)
            {
                _hpBar = root.Q<ProgressBar>("hp-bar");
                _mpBar = root.Q<ProgressBar>("mp-bar");
                _skillContainer = root.Q<VisualElement>("skill-container");
                _inventoryPanel = root.Q<VisualElement>("inventory-panel");
                _inventoryListView = root.Q<ListView>("inventory-list");

                // Thiết lập cơ chế ảo hóa danh sách (UI Virtualization) cho ListView
                SetupInventoryVirtualization();

                // Cập nhật giao diện theo dữ liệu mạng hiện tại (nếu nhân vật đã spawn)
                if (IsSpawned && IsOwner && _networkData != null)
                {
                    UpdateHPDisplay(_networkData.CurrentHP.Value, _networkData.MaxHP.Value);
                    UpdateMPDisplay(_networkData.CurrentMP.Value, _networkData.MaxMP.Value);
                }
            }
        }

        /// <summary>
        /// Cấu hình makeItem và bindItem cho ListView để ảo hóa hiển thị.
        /// </summary>
        private void SetupInventoryVirtualization()
        {
            if (_inventoryListView == null) return;

            // Thiết lập chiều cao cố định cho dòng hiển thị (Bắt buộc để kích hoạt ảo hóa)
            _inventoryListView.fixedItemHeight = 46f; // row height 40px + margin-bottom 6px
            _inventoryListView.itemsSource = _inventoryData;

            // Định nghĩa cách tạo một dòng hiển thị trống mẫu (Chỉ được gọi khoảng 15-20 lần)
            _inventoryListView.makeItem = () =>
            {
                var row = new VisualElement();
                row.AddToClassList("inventory-item-row");

                var nameLabel = new Label();
                nameLabel.name = "item-name";
                nameLabel.AddToClassList("item-name");

                var qtyLabel = new Label();
                qtyLabel.name = "item-quantity";
                qtyLabel.AddToClassList("item-quantity");

                row.Add(nameLabel);
                row.Add(qtyLabel);

                return row;
            };

            // Định nghĩa cách nạp đè dữ liệu lên dòng hiển thị mẫu khi cuộn danh sách (Gắn dữ liệu liên tục)
            _inventoryListView.bindItem = (element, index) =>
            {
                if (index >= 0 && index < _inventoryData.Count)
                {
                    InventoryItem item = _inventoryData[index];
                    var nameLabel = element.Q<Label>("item-name");
                    var qtyLabel = element.Q<Label>("item-quantity");

                    if (nameLabel != null) nameLabel.text = item.itemName;
                    if (qtyLabel != null) qtyLabel.text = $"x{item.quantity}";
                }
            };
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Nếu không phải là Owner của nhân vật này, tắt giao diện HUD đi để tránh hiển thị trùng lặp
            if (!IsOwner)
            {
                if (_panelRenderer != null)
                {
                    _panelRenderer.enabled = false;
                }
                enabled = false; // Tắt component xử lý UI này
                return;
            }

            // Đối với Owner Client: Đăng ký lắng nghe sự kiện đồng bộ mạng để cập nhật UI tức thời
            if (_networkData != null)
            {
                _networkData.CurrentHP.OnValueChanged += OnHPChanged;
                _networkData.MaxHP.OnValueChanged += OnMaxHPChanged;
                _networkData.CurrentMP.OnValueChanged += OnMPChanged;
                _networkData.MaxMP.OnValueChanged += OnMaxMPChanged;

                // Cập nhật giao diện lần đầu tiên theo dữ liệu hiện tại
                UpdateHPDisplay(_networkData.CurrentHP.Value, _networkData.MaxHP.Value);
                UpdateMPDisplay(_networkData.CurrentMP.Value, _networkData.MaxMP.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            // Hủy đăng ký lắng nghe sự kiện khi nhân vật biến mất khỏi mạng để tránh rò rỉ bộ nhớ (GC leak)
            if (IsOwner && _networkData != null)
            {
                _networkData.CurrentHP.OnValueChanged -= OnHPChanged;
                _networkData.MaxHP.OnValueChanged -= OnMaxHPChanged;
                _networkData.CurrentMP.OnValueChanged -= OnMPChanged;
                _networkData.MaxMP.OnValueChanged -= OnMaxMPChanged;
            }
        }

        /// <summary>
        /// Bật/Tắt hiển thị bảng hòm đồ.
        /// </summary>
        public void ToggleInventory()
        {
            if (_inventoryPanel != null)
            {
                bool isVisible = _inventoryPanel.resolvedStyle.display != DisplayStyle.None;
                _inventoryPanel.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;

                // Làm mới danh sách khi mở bảng
                if (!isVisible && _inventoryListView != null)
                {
                    _inventoryListView.Rebuild();
                }
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

        private void OnMPChanged(float oldVal, float newVal)
        {
            UpdateMPDisplay(newVal, _networkData.MaxMP.Value);
        }

        private void OnMaxMPChanged(float oldVal, float newVal)
        {
            UpdateMPDisplay(_networkData.CurrentMP.Value, newVal);
        }

        /// <summary>
        /// Cập nhật hiển thị thanh Máu.
        /// </summary>
        private void UpdateHPDisplay(float current, float max)
        {
            if (_hpBar != null)
            {
                _hpBar.value = current;
                _hpBar.highValue = max;
                _hpBar.title = $"HP: {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
            }
        }

        /// <summary>
        /// Cập nhật hiển thị thanh Năng lượng.
        /// </summary>
        private void UpdateMPDisplay(float current, float max)
        {
            if (_mpBar != null)
            {
                _mpBar.value = current;
                _mpBar.highValue = max;
                _mpBar.title = $"MP: {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
            }
        }
    }
}
