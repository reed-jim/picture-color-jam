using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameGraphicSetting : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;

    void Start()
    {
        int weakDeviceRAMThreshold = 4000;

        if (SystemInfo.systemMemorySize < weakDeviceRAMThreshold)
        {
            UniversalAdditionalCameraData universalAdditionalCameraData = _mainCamera.GetUniversalAdditionalCameraData();

            universalAdditionalCameraData.renderPostProcessing = false;
            universalAdditionalCameraData.allowHDROutput = false;
            universalAdditionalCameraData.antialiasing = AntialiasingMode.None;
        }
    }
}
