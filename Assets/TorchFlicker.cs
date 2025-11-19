using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    public float minIntensity = 0.8f;
    public float maxIntensity = 1.2f;
    public float flickerSpeed = 10f;
    public float smoothing = 0.1f;

    private Light torchLight;
    private float baseIntensity;
    private float targetIntensity;
    private float currentVelocity;

    void Start()
    {
        torchLight = GetComponent<Light>();
        if (torchLight != null)
        {
            baseIntensity = torchLight.intensity;
            targetIntensity = baseIntensity;
        }
    }

    void Update()
    {
        if (torchLight == null) return;

        // Randomly change target intensity
        if (Random.value < flickerSpeed * Time.deltaTime)
        {
            targetIntensity = baseIntensity * Random.Range(minIntensity, maxIntensity);
        }

        // Smoothly interpolate to target
        torchLight.intensity = Mathf.SmoothDamp(
            torchLight.intensity,
            targetIntensity,
            ref currentVelocity,
            smoothing
        );
    }
}