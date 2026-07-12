using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

namespace Shared
{
    /// <summary>
    /// Component dùng chung để đồng bộ hóa tên hiển thị của các nhóm cha trong Hierarchy từ Server xuống Client.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkGroupParent : NetworkBehaviour
    {
        // NetworkVariable chứa tên hiển thị của nhóm cha, chỉ Server mới có quyền ghi
        public NetworkVariable<FixedString32Bytes> ParentName { get; } = new NetworkVariable<FixedString32Bytes>(
            new FixedString32Bytes(""),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private void OnEnable()
        {
            // Đăng ký sự kiện thay đổi tên để cập nhật tên GameObject
            ParentName.OnValueChanged += OnParentNameChanged;
        }

        private void OnDisable()
        {
            // Hủy đăng ký sự kiện tránh rò rỉ bộ nhớ
            ParentName.OnValueChanged -= OnParentNameChanged;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Cập nhật tên cục bộ khi vừa kết nối
            UpdateGameObjectName(ParentName.Value);
        }

        private void OnParentNameChanged(FixedString32Bytes oldVal, FixedString32Bytes newVal)
        {
            UpdateGameObjectName(newVal);
        }

        private void UpdateGameObjectName(FixedString32Bytes newName)
        {
            string nameStr = newName.ToString();
            if (!string.IsNullOrEmpty(nameStr))
            {
                gameObject.name = nameStr;
            }
        }
    }
}
