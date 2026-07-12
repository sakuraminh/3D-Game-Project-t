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

        [Header("Rotation Settings")]
        [SerializeField] private float _rotationSpeed = 5f;
        [SerializeField] private float _minPitch = 10f;
        [SerializeField] private float _maxPitch = 85f;

        private float _yaw = 0f;
        private float _pitch = 33.7f;
        private float _distance;

        private void Start()
        {
            this.LoadComponents();
            _distance = _cameraOffset.magnitude;
            _yaw = transform.eulerAngles.y;
        }

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

            // Quay camera khi nhấn giữ chuột phải
            if (UnityEngine.InputSystem.Mouse.current != null && 
                UnityEngine.InputSystem.Mouse.current.rightButton.isPressed)
            {
                Vector2 mouseDelta = UnityEngine.InputSystem.Mouse.current.delta.ReadValue();
                _yaw += mouseDelta.x * _rotationSpeed * 0.1f;
                _pitch -= mouseDelta.y * _rotationSpeed * 0.1f;
                _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

                // Cập nhật camera offset mới
                Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
                _cameraOffset = rotation * new Vector3(0f, 0f, -_distance);
            }

            // Vị trí Camera mong muốn
            Vector3 targetPos = transform.position + _cameraOffset;

            // Di chuyển Camera mượt mà tới vị trí mong muốn
            _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, targetPos, _smoothSpeed * Time.deltaTime);

            // Quay Camera luôn nhìn vào Player (nhích lên một chút ngang tầm ngực/đầu)
            _cameraTransform.LookAt(transform.position + Vector3.up * 1f);
        }
 
        protected void LoadComponents() 
        {
            this.LoadCamera();
        }

        protected void LoadCamera()
        {
            if (Camera.main == null && _cameraTransform != null)
            {
                Debug.LogWarning((Camera.main == null) + " - " + (_cameraTransform != null) + " - ", gameObject);
                return; 
            }
            this._cameraTransform = Camera.main.transform;
            Debug.Log("[PlayerCameraFollow] Main Camera found! " + this._cameraTransform.name, gameObject);
        }
    }
}
 