using System.Collections;
using UnityEngine;
using TMPro;

namespace Client
{
    /// <summary>
    /// Component quản lý vòng đời và hoạt ảnh (Tween) của một chữ số nhảy sát thương trong không gian 3D.
    /// Tự động xoay Billboard và trả bản thân về Object Pool sau khi hoạt ảnh kết thúc.
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class FloatingDamageText : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float _duration = 1.0f;
        [SerializeField] private float _moveSpeed = 1.5f;
        [SerializeField] private Vector3 _randomOffsetRange = new Vector3(0.5f, 0f, 0.5f);

        private TextMeshPro _textMesh;
        private Transform _mainCameraTransform;
        private Coroutine _animationCoroutine;

        private void Awake()
        {
            _textMesh = GetComponent<TextMeshPro>();
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                _mainCameraTransform = Camera.main.transform;
            }
        }

        /// <summary>
        /// Khởi tạo và bắt đầu chạy hoạt ảnh số sát thương.
        /// </summary>
        public void Initialize(int damageAmount, Vector3 startPosition)
        {
            if (_textMesh == null) _textMesh = GetComponent<TextMeshPro>();

            // Áp dụng random offset nhỏ để các chữ số không bị xếp chồng hoàn toàn lên nhau
            Vector3 randomOffset = new Vector3(
                Random.Range(-_randomOffsetRange.x, _randomOffsetRange.x),
                Random.Range(-_randomOffsetRange.y, _randomOffsetRange.y),
                Random.Range(-_randomOffsetRange.z, _randomOffsetRange.z)
            );

            transform.position = startPosition + Vector3.up * 1.5f + randomOffset;
            _textMesh.text = damageAmount.ToString();
            _textMesh.alpha = 1f;

            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            _animationCoroutine = StartCoroutine(AnimateTextFlow());
        }

        private IEnumerator AnimateTextFlow()
        {
            float elapsed = 0f;
            Vector3 targetPosition = transform.position + Vector3.up * _moveSpeed;

            // Cache camera transform nếu chưa có
            if (_mainCameraTransform == null && Camera.main != null)
            {
                _mainCameraTransform = Camera.main.transform;
            }

            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _duration;

                // Di chuyển đi lên mượt mà (Lerp)
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 3f);

                // Mờ dần theo thời gian (Alpha Lerp)
                _textMesh.alpha = Mathf.Lerp(1f, 0f, t);

                // Billboard: Xoay hướng mặt chữ về phía camera
                if (_mainCameraTransform != null)
                {
                    transform.rotation = _mainCameraTransform.rotation;
                }

                yield return null;
            }

            // Hoàn thành hoạt ảnh, tắt đối tượng và trả về Pool để tái sử dụng
            if (DamageTextPoolManager.Instance != null)
            {
                DamageTextPoolManager.Instance.ReturnToPool(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
