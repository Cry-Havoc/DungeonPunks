using UnityEngine;

/// <summary>
/// Makes a light flicker realistically like a torch or candle
/// </summary>
[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    private Light lightComponent;
    private float baseIntensity;
    private float minIntensity;
    private float maxIntensity;
    private float speed;
    private float smoothing;

    private float targetIntensity;
    private float currentVelocity;

    /// <summary>
    /// Initializes the flickering light with parameters
    /// </summary>
    public void Initialize(float baseInt, float minMult, float maxMult, float flickerSpeed, float smoothFactor)
    {
        baseIntensity = baseInt;
        minIntensity = baseIntensity * minMult;
        maxIntensity = baseIntensity * maxMult;
        speed = flickerSpeed;
        smoothing = smoothFactor;

        lightComponent = GetComponent<Light>();
        if (lightComponent != null)
        {
            lightComponent.intensity = baseIntensity;
            targetIntensity = baseIntensity;
        }
    }

    void Start()
    {
        // Fallback initialization if Initialize() wasn't called
        if (lightComponent == null)
        {
            lightComponent = GetComponent<Light>();
            baseIntensity = lightComponent != null ? lightComponent.intensity : 1f;
            minIntensity = baseIntensity * 0.8f;
            maxIntensity = baseIntensity * 1.2f;
            speed = 10f;
            smoothing = 0.1f;
            targetIntensity = baseIntensity;
        }
    }

    void Update()
    {
        if (lightComponent == null) return;

        // Randomly change target intensity
        if (Random.value < speed * Time.deltaTime)
        {
            targetIntensity = Random.Range(minIntensity, maxIntensity);
        }

        // Smoothly interpolate to target
        lightComponent.intensity = Mathf.SmoothDamp(
            lightComponent.intensity,
            targetIntensity,
            ref currentVelocity,
            smoothing
        );
    }

    /// <summary>
    /// Pauses flickering at current intensity
    /// </summary>
    public void PauseFlicker()
    {
        enabled = false;
    }

    /// <summary>
    /// Resumes flickering
    /// </summary>
    public void ResumeFlicker()
    {
        enabled = true;
    }

    /// <summary>
    /// Sets a new base intensity and updates min/max
    /// </summary>
    public void SetBaseIntensity(float newBase)
    {
        baseIntensity = newBase;
        minIntensity = baseIntensity * (minIntensity / baseIntensity); // Maintain ratio
        maxIntensity = baseIntensity * (maxIntensity / baseIntensity); // Maintain ratio
    }
}