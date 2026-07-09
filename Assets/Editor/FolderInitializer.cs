using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FolderInitializer
{
    [MenuItem("Architecture/Initialize Project Folders")]
    public static void InitializeFolders()
    {
        string rootPath = Path.Combine(Application.dataPath, "_Datas");

        // Danh sách các thư mục cần khởi tạo theo quy chuẩn kiến trúc
        List<string> foldersToCreate = new List<string>
        {
            "Scripts/Server",
            "Scripts/Client",
            "Scripts/Shared",
            "Prefabs/Network",
            "Prefabs/Local",
            "Scenes",
            "Settings",
            "ScriptableObjects/Data-Driven",
            "ScriptableObjects/Strategies"
        };

        int createdCount = 0;

        foreach (string folder in foldersToCreate)
        {
            string fullPath = Path.Combine(rootPath, folder);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                createdCount++;
                Debug.Log($"[Architecture] Created folder: Assets/_Datas/{folder}");
            }
        }

        if (createdCount > 0)
        {
            AssetDatabase.Refresh();
            Debug.Log($"[Architecture] Successfully initialized {createdCount} folders within Assets/_Datas/!");
        }
        else
        {
            Debug.Log("[Architecture] All folders already exist. No action needed.");
        }
    }
}