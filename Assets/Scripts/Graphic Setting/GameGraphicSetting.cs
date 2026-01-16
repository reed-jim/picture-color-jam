using EPOOutline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameGraphicSetting : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Outliner outliner;

    void Start()
    {
        int weakDeviceRAMThreshold = 3000;

        if (SystemInfo.systemMemorySize < weakDeviceRAMThreshold)
        {
            UniversalAdditionalCameraData universalAdditionalCameraData = _mainCamera.GetUniversalAdditionalCameraData();

            universalAdditionalCameraData.renderPostProcessing = false;
            universalAdditionalCameraData.allowHDROutput = false;
            universalAdditionalCameraData.antialiasing = AntialiasingMode.None;

            outliner.DilateIterations = 1;
            outliner.DilateQuality = DilateQuality.Base;
            outliner.BlurShift = 0;
            outliner.BlurIterations = 0;
        }
    }
}
