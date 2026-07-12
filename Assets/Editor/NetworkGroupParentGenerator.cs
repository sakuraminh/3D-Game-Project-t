using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using Shared;

public static class NetworkGroupParentGenerator
{
    [MenuItem("Architecture/Generate Network Group Parent Prefab")]
    public static void GeneratePrefab()
    {
        // 1. Tạo thư mục nếu chưa tồn tại
        string folderPath = "Assets/_Datas/Prefabs/Network";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.dataPath, "_Datas/Prefabs/Network"));
            AssetDatabase.Refresh();
        }

        string localPath = folderPath + "/NetworkGroupParent.prefab";

        // 2. Tạo đối tượng tạm thời
        GameObject tempGo = new GameObject("NetworkGroupParent");
        tempGo.AddComponent<NetworkObject>();
        tempGo.AddComponent<NetworkGroupParent>();

        // 3. Lưu thành Prefab
        bool success;
        PrefabUtility.SaveAsPrefabAsset(tempGo, localPath, out success);

        // 4. Giải phóng đối tượng tạm thời
        Object.DestroyImmediate(tempGo);

        if (success)
        {
            AssetDatabase.Refresh();
            Debug.Log($"[Architecture] Đã tạo thành công Network Group Parent Prefab tại: {localPath}");
            
            // Focus vào file vừa tạo trong Project window
            Object prefabAsset = AssetDatabase.LoadAssetAtPath<Object>(localPath);
            if (prefabAsset != null)
            {
                Selection.activeObject = prefabAsset;
                EditorGUIUtility.PingObject(prefabAsset);
            }
        }
        else
        {
            Debug.LogError("[Architecture] Không thể tạo Network Group Parent Prefab.");
        }
    }
}
