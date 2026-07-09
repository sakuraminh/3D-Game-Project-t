using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode.Components;

using Client; // Cho ConnectionHUD
using Shared; // Cho PlayerNetworkMovement

public static class NetworkSceneCreator
{
    [MenuItem("Architecture/Create Multiplayer Test Scene")]
    public static void CreateScene()
    {
        // 1. Tạo hoặc tải Player Prefab mạng trước
        GameObject playerPrefab = CreatePlayerPrefab();
        if (playerPrefab == null)
        {
            Debug.LogError("[Architecture] Failed to create Player Network Prefab.");
            return;
        }

        // 2. Tạo một Scene mới với các GameObject mặc định (Main Camera, Directional Light)
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // 3. Tạo GameObject NetworkManager
        GameObject networkManagerGo = new GameObject("NetworkManager");
        NetworkManager networkManager = networkManagerGo.AddComponent<NetworkManager>();
        
        // 4. Thêm Component UnityTransport (Bộ truyền tải mạng mặc định của NGO)
        NetworkTransport transport = networkManagerGo.AddComponent<UnityTransport>();
        
        // 5. Thêm ConnectionHUD hiển thị giao diện UI kết nối
        networkManagerGo.AddComponent<ConnectionHUD>();
        
        // 6. Gán transport và Player Prefab vào cấu hình của NetworkManager
        if (networkManager.NetworkConfig == null)
        {
            networkManager.NetworkConfig = new NetworkConfig();
        }
        networkManager.NetworkConfig.NetworkTransport = transport;
        networkManager.NetworkConfig.PlayerPrefab = playerPrefab;
        
        // Ghi chú: PlayerPrefab sẽ tự động được NetworkManager đăng ký vào danh sách NetworkPrefabs khi runtime.
        
        // 7. Lưu Scene vào đường dẫn quy hoạch Assets/_Datas/Scenes/
        string folderPath = "Assets/_Datas/Scenes";
        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }
        
        string scenePath = $"{folderPath}/MultiplayerTestScene.unity";
        bool saveSuccess = EditorSceneManager.SaveScene(newScene, scenePath);
        
        if (saveSuccess)
        {
            Debug.Log($"[Architecture] Successfully created Multiplayer Test Scene at: {scenePath}");
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("[Architecture] Failed to save Multiplayer Test Scene.");
        }
    }

    private static GameObject CreatePlayerPrefab()
    {
        string prefabFolder = "Assets/_Datas/Prefabs/Network";
        if (!System.IO.Directory.Exists(prefabFolder))
        {
            System.IO.Directory.CreateDirectory(prefabFolder);
        }
        string prefabPath = $"{prefabFolder}/PlayerNetwork.prefab";

        // Thử tải prefab hiện có nếu đã được tạo trước đó
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            return existingPrefab;
        }

        // Nếu chưa có, tạo mới một GameObject tạm
        GameObject tempPlayer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        tempPlayer.name = "PlayerNetwork";

        // Thêm các Component mạng
        tempPlayer.AddComponent<NetworkObject>();
        tempPlayer.AddComponent<NetworkTransform>();
        tempPlayer.AddComponent<PlayerNetworkMovement>();

        // Lưu thành Asset Prefab
        GameObject playerPrefab = PrefabUtility.SaveAsPrefabAsset(tempPlayer, prefabPath);
        
        // Hủy GameObject tạm thời trong Editor
        GameObject.DestroyImmediate(tempPlayer);

        Debug.Log($"[Architecture] Successfully created Player Network Prefab at: {prefabPath}");
        return playerPrefab;
    }
}
