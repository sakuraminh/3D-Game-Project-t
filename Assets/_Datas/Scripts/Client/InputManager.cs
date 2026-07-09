using UnityEngine;

namespace Client
{
    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance;
        public static InputManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("[InputManager]");
                    _instance = go.AddComponent<InputManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private MInputSystem _inputSystem;
        public Vector2 MoveInput { get; private set; }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _inputSystem = new @MInputSystem();
            _inputSystem.Player.Enable();
        }

        private void OnDestroy()
        {
            if (_inputSystem != null)
            {
                _inputSystem.Player.Disable();
                _inputSystem.Dispose();
            }
        }

        private void Update()
        {
            if (_inputSystem != null)
            {
                MoveInput = _inputSystem.Player.Move.ReadValue<Vector2>();
            }
        }

        // Tự động khởi tạo InputManager trước khi Scene đầu tiên được tải
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var unused = Instance;
        }
    }
}
