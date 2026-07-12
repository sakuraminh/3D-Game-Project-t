---
trigger: always_on
---

# Quản lý Components
* Tự thêm các component bằng RequireComponent
* luôn kiểm tra null trước khi getcomponent
* mẫu cách getcomponent của tôi:

        protected override void LocalLoadComponents() 
        {
            this.LoadCamera();
        }

        protected void LoadCamera()
        {
            if (Camera.main == null && _cameraTransform != null)
            {
                Debug.LogWarning(Camera.main == null + " - " + _cameraTransform != null + " - " +, gameObject);
                return; 
            }
            this._cameraTransform = Camera.main.transform;
            Debug.Log("[PlayerCameraFollow] Main Camera found!" + this._cameraTransform.name, gameObject);
        }

luôn luôn gọi ở trong hàm reset của unity 
hàm LocalLoadComponents() sẽ được gọi ở start, awake, update ... tùy trường hợp
nếu là đối tượng mạng thì sẽ là NetworkLoadComponents