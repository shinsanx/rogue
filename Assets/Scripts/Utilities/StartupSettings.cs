using UnityEngine;

public class StartupSettings : MonoBehaviour {
    void Awake() {
        QualitySettings.vSyncCount = 0;              // VSyncを無効化
        Application.targetFrameRate = 60;           // 最大FPSを上げてGPU待ち回避
    }
}