using Unity.Netcode;
using UnityEngine;

namespace Client
{
    public class ConnectionHUD : MonoBehaviour
    {
        private void OnGUI()
        {
            // Thiết lập vùng hiển thị GUI ở góc trên bên trái màn hình
            GUILayout.BeginArea(new Rect(20, 20, 250, 200));
            
            if (NetworkManager.Singleton == null)
            {
                GUILayout.Label("No NetworkManager found!");
                GUILayout.EndArea();
                return;
            }

            // Nếu chưa chạy ở bất kỳ chế độ mạng nào, hiển thị nút khởi tạo kết nối
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
            }

            GUILayout.EndArea();
        }

        private void StartButtons()
        {
            if (GUILayout.Button("Start Host (Server + Client)", GUILayout.Height(35)))
            {
                NetworkManager.Singleton.StartHost();
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Start Server Only", GUILayout.Height(35)))
            {
                NetworkManager.Singleton.StartServer();
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Start Client", GUILayout.Height(35)))
            {
                NetworkManager.Singleton.StartClient();
            }
        }

        private void StatusLabels()
        {
            string mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Box($"Status: Connected as {mode}");
            GUILayout.Space(5);
            
            if (GUILayout.Button("Disconnect", GUILayout.Height(30)))
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
    }
}
