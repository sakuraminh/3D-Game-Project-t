using UnityEditor;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Antigravity.EditorTools
{
    public class PortFreeerEditor : EditorWindow
    {
        private int _targetPort = 7777;
        private string _statusMessage = "";
        private MessageType _messageType = MessageType.Info;

        [MenuItem("Tools/Network/Port Free-er")]
        public static void ShowWindow()
        {
            GetWindow<PortFreeerEditor>("Port Free-er");
        }

        private void OnGUI()
        {
            GUILayout.Label("Giải phóng cổng kết nối bị chiếm dụng (Windows)", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            _targetPort = EditorGUILayout.IntField("Cổng cần giải phóng:", _targetPort);
            EditorGUILayout.Space();

            if (GUILayout.Button("Giải phóng Port", GUILayout.Height(30)))
            {
                FreePort(_targetPort);
            }

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(_statusMessage, _messageType);
            }
        }

        private void FreePort(int port)
        {
#if !UNITY_EDITOR_WIN
            _statusMessage = "Công cụ này hiện chỉ hỗ trợ hệ điều hành Windows.";
            _messageType = MessageType.Error;
            UnityEngine.Debug.LogError(_statusMessage);
            return;
#else
            try
            {
                _statusMessage = $"Đang kiểm tra cổng {port}...";
                _messageType = MessageType.Info;
                
                // Chạy netstat để tìm PID
                ProcessStartInfo netstatInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c netstat -ano | findstr :{port}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process netstatProcess = Process.Start(netstatInfo))
                {
                    string output = netstatProcess.StandardOutput.ReadToEnd();
                    netstatProcess.WaitForExit();

                    if (string.IsNullOrEmpty(output))
                    {
                        _statusMessage = $"Không tìm thấy tiến trình nào đang sử dụng cổng {port}.";
                        _messageType = MessageType.Warning;
                        UnityEngine.Debug.LogWarning(_statusMessage);
                        return;
                    }

                    // Parse PID từ output của netstat
                    string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    bool found = false;

                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();
                        string[] tokens = Regex.Split(trimmedLine, @"\s+");
                        if (tokens.Length >= 4)
                        {
                            string localAddress = tokens[1];
                            string pidStr = tokens[tokens.Length - 1]; // PID luôn nằm ở cột cuối cùng

                            // Đảm bảo khớp cổng chính xác (tránh cổng 77777 khớp khi tìm 7777)
                            if (localAddress.EndsWith($":{port}"))
                            {
                                if (int.TryParse(pidStr, out int pid))
                                {
                                    int currentPid = Process.GetCurrentProcess().Id;
                                    if (pid == currentPid)
                                    {
                                        _statusMessage = $"Cổng {port} đang bị chiếm dụng bởi chính Unity Editor hiện tại (PID: {pid}). Vui lòng thoát Play Mode để giải phóng cổng.";
                                        _messageType = MessageType.Warning;
                                        UnityEngine.Debug.LogWarning($"[PortFreeer] {_statusMessage}");
                                        found = true;
                                    }
                                    else
                                    {
                                        KillProcess(pid, port);
                                        found = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    if (!found)
                    {
                        _statusMessage = $"Không xác định được PID cụ thể cho cổng {port}.";
                        _messageType = MessageType.Warning;
                        UnityEngine.Debug.LogWarning(_statusMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _statusMessage = $"Lỗi: {ex.Message}";
                _messageType = MessageType.Error;
                UnityEngine.Debug.LogError($"[PortFreeer] Lỗi khi giải phóng cổng: {ex}");
            }
#endif
        }

        private void KillProcess(int pid, int port)
        {
            try
            {
                ProcessStartInfo killInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c taskkill /F /PID {pid}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process killProcess = Process.Start(killInfo))
                {
                    string output = killProcess.StandardOutput.ReadToEnd();
                    killProcess.WaitForExit();

                    if (killProcess.ExitCode == 0)
                    {
                        _statusMessage = $"Đã giải phóng cổng {port} thành công! Đã kết thúc tiến trình PID: {pid}.";
                        _messageType = MessageType.Info;
                        UnityEngine.Debug.Log($"[PortFreeer] {_statusMessage}");
                    }
                    else
                    {
                        _statusMessage = $"Không thể đóng tiến trình PID {pid} trên cổng {port}. Lỗi: {output.Trim()}";
                        _messageType = MessageType.Error;
                        UnityEngine.Debug.LogError($"[PortFreeer] {_statusMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                _statusMessage = $"Lỗi khi kết thúc tiến trình {pid}: {ex.Message}";
                _messageType = MessageType.Error;
                UnityEngine.Debug.LogError($"[PortFreeer] {_statusMessage}");
            }
        }
    }
}
