using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class MainCameraPostProcessing : MonoBehaviour
{
    PostProcessVolume volume;
    DepthOfField depthOfField;

    private void Start()
    {
        volume = GetComponent<PostProcessVolume>();
        volume.profile.TryGetSettings(out depthOfField);
    }

    private void Update()
    {
        depthOfField.focusDistance.value = transform.position.y * 1.4f;
        depthOfField.aperture.value = 75 / depthOfField.focusDistance.value - .25f;
        depthOfField.focalLength.value = 275;
    }
}
