using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameGraphicSetting : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Volume postProcessingVolumn;

    // void Awake()
    // {
    //     if (SystemInfo.systemMemorySize < 4000)
    //     {
    //         postProcessingVolumn.enabled = false;

    //         UniversalAdditionalCameraData universalAdditionalCameraData = _mainCamera.GetUniversalAdditionalCameraData();

    //         universalAdditionalCameraData.renderPostProcessing = false;
    //         universalAdditionalCameraData.allowHDROutput = false;
    //         universalAdditionalCameraData.antialiasing = AntialiasingMode.None;
    //     }
    // }

    void Start()
    {
        int weakDeviceRAMThreshold = RemoteConfigController.GetIntConfig(FirebaseConfig.WEAK_DEVICE_RAM_THRESHOLD, 4000);

        if (SystemInfo.systemMemorySize < weakDeviceRAMThreshold)
        {
            postProcessingVolumn.enabled = false;

            UniversalAdditionalCameraData universalAdditionalCameraData = _mainCamera.GetUniversalAdditionalCameraData();

            universalAdditionalCameraData.renderPostProcessing = false;
            universalAdditionalCameraData.allowHDROutput = false;
            universalAdditionalCameraData.antialiasing = AntialiasingMode.None;
        }
    }
}
