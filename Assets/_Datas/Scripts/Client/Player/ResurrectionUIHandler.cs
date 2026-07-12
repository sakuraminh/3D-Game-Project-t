using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace Client
{
    [RequireComponent(typeof(UIDocument))]
    public class ResurrectionUIHandler : MonoBehaviour
    {
        public static ResurrectionUIHandler Instance { get; private set; }

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _deathPanel;
        private Button _resurrectButton;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _uiDocument = GetComponent<UIDocument>();
        }

        private void Start()
        {
            _root = _uiDocument.rootVisualElement;
            if (_root != null)
            {
                BuildUI();
            }
        }

        private void BuildUI()
        {
            _root.style.display = DisplayStyle.None;

            _deathPanel = new VisualElement();
            _deathPanel.style.position = Position.Absolute;
            _deathPanel.style.left = Length.Percent(0);
            _deathPanel.style.top = Length.Percent(0);
            _deathPanel.style.width = Length.Percent(100);
            _deathPanel.style.height = Length.Percent(100);
            _deathPanel.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.75f));
            _deathPanel.style.alignItems = Align.Center;
            _deathPanel.style.justifyContent = Justify.Center;

            Label title = new Label("BẠN ĐÃ CHẾT");
            title.style.fontSize = 40;
            title.style.color = new StyleColor(Color.red);
            title.style.marginBottom = 20;

            _resurrectButton = new Button(OnResurrectClicked);
            _resurrectButton.text = "Hồi Sinh";
            _resurrectButton.style.fontSize = 20;
            _resurrectButton.style.paddingLeft = 20;
            _resurrectButton.style.paddingRight = 20;
            _resurrectButton.style.paddingTop = 10;
            _resurrectButton.style.paddingBottom = 10;
            
            // Thiết kế nút bo tròn đẹp mắt
            _resurrectButton.style.borderTopLeftRadius = 5;
            _resurrectButton.style.borderTopRightRadius = 5;
            _resurrectButton.style.borderBottomLeftRadius = 5;
            _resurrectButton.style.borderBottomRightRadius = 5;
            _resurrectButton.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f));
            _resurrectButton.style.color = new StyleColor(Color.white);

            _deathPanel.Add(title);
            _deathPanel.Add(_resurrectButton);
            _root.Add(_deathPanel);
        }

        private void OnResurrectClicked()
        {
            if (Shared.GameplayManager.Instance != null)
            {
                Shared.GameplayManager.Instance.RequestRespawnServerRpc();
                Debug.Log("[ResurrectionUIHandler] Sent resurrect request to Server.");
            }
            else
            {
                Debug.LogError("[ResurrectionUIHandler] GameplayManager.Instance is null!");
            }
            Hide();
        }

        public void Show()
        {
            if (_root == null && _uiDocument != null)
            {
                _root = _uiDocument.rootVisualElement;
                if (_root != null && _deathPanel == null)
                {
                    BuildUI();
                }
            }

            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
                Debug.Log("[ResurrectionUIHandler] Death panel displayed.");
            }
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
#if !UNITY_SERVER
            if (FindAnyObjectByType<ResurrectionUIHandler>() == null)
            {
                GameObject go = new GameObject("[ResurrectionUI]");
                go.AddComponent<UIDocument>();
                go.AddComponent<ResurrectionUIHandler>();
                
                var panelSettings = Resources.FindObjectsOfTypeAll<PanelSettings>();
                if (panelSettings != null && panelSettings.Length > 0)
                {
                    go.GetComponent<UIDocument>().panelSettings = panelSettings[0];
                }
                
                DontDestroyOnLoad(go);
                Debug.Log("[ResurrectionUIHandler] Initialized dynamically after scene load.");
            }
#endif
        }
    }
}
