using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Shared
{
    /// <summary>
    /// Quản lý nạp và giải phóng tài nguyên bất đồng bộ qua hệ thống Unity Addressables.
    /// Giúp tối ưu hóa bộ nhớ RAM, tự động dọn dẹp các Asset không còn sử dụng.
    /// </summary>
    public static class ResourceManager
    {
        // Cache quản lý AsyncOperationHandle theo kết quả tải về
        private static readonly Dictionary<object, AsyncOperationHandle> _operationHandles = new Dictionary<object, AsyncOperationHandle>();

        /// <summary>
        /// Nạp bất đồng bộ một tài nguyên dựa trên khóa định danh (Addressable Key / Address).
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu của Asset</typeparam>
        /// <param name="key">Khóa định danh Addressable</param>
        /// <param name="onComplete">Callback nhận kết quả tải về</param>
        public static void LoadAssetAsync<T>(string key, System.Action<T> onComplete) where T : Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    // Cache handle dùng chính Asset tải về làm khóa để giải phóng sau này
                    _operationHandles[op.Result] = handle;
                    onComplete?.Invoke(op.Result);
                }
                else
                {
                    Debug.LogError($"[ResourceManager] Failed to load asset with key: {key}");
                    onComplete?.Invoke(null);
                }
            };
        }

        /// <summary>
        /// Giải phóng tài nguyên khỏi RAM khi không còn sử dụng.
        /// </summary>
        /// <param name="assetInstance">Đối tượng asset cần giải phóng</param>
        public static void ReleaseAsset(object assetInstance)
        {
            if (assetInstance == null) return;

            if (_operationHandles.TryGetValue(assetInstance, out AsyncOperationHandle handle))
            {
                Addressables.Release(handle);
                _operationHandles.Remove(assetInstance);
            }
            else
            {
                // Fallback nếu không có handle trong Cache
                Addressables.Release(assetInstance);
            }
        }
    }
}
