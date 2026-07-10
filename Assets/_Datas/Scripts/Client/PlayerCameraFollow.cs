using Unity.Netcode;
using UnityEngine;

namespace Client
{
    /// <summary>
    /// Component điều khiển Camera di chuyển mượt mà theo nhân vật sở hữu (Client-only).
    /// </summary>
    public class PlayerCameraFollow : NetworkBehaviour
    {
        [SerializeField] private Vector3 _cameraOffset = new Vector3(0f, 6f, -9f);
        [SerializeField] private float _smoothSpeed = 8f;

        private Transform _cameraTransform;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Chỉ Client sở hữu nhân vật này mới điều khiển Camera của chính họ
            if (IsOwner)
            {
                if (Camera.main != null)
                {
                    _cameraTransform = Camera.main.transform;
                }
                else
                {
                    Debug.LogWarning("[PlayerCameraFollow] No Main Camera found in the scene!");
                }
            }
        }

        private void LateUpdate()
        {
            // Chỉ thực hiện LateUpdate trên Client sở hữu nhân vật
            if (!IsOwner || _cameraTransform == null) return;

            // Vị trí Camera mong muốn
            Vector3 targetPos = transform.position + _cameraOffset;

            // Di chuyển Camera mượt mà tới vị trí mong muốn
            _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, targetPos, _smoothSpeed * Time.deltaTime);

            // Quay Camera luôn nhìn vào Player (nhích lên một chút ngang tầm ngực/đầu)
            _cameraTransform.LookAt(transform.position + Vector3.up * 1f);
        }
    }
}
