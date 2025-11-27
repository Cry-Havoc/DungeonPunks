using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Die : MonoBehaviour
{
    [Header("Die Setup")]
    [Tooltip("Assign the face rotations for values 0-9 (0 represents 10 on d10)")]
    public DiceFace[] faces = new DiceFace[10];

    [Header("Roll Physics")]
    public float rollForce = 10f;
    public float rollTorque = 20f;
    public Vector3 rollStartOffset = new Vector3(5, 2, 0); // Start to the right and above

    [Header("Settle Settings")]
    public float velocityThreshold = 0.1f;
    public float angularVelocityThreshold = 0.1f;

    private Rigidbody rb;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private int targetValue = -1;
    private bool isRolling = false;
    private Coroutine rollCoroutine;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Make sure rigidbody is set up correctly
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    /// <summary>
    /// Sets the material of the die
    /// </summary>
    public void SetMaterial(Material material)
    {
        if (meshRenderer != null && material != null)
        {
            meshRenderer.material = material;
        }
    }

    /// <summary>
    /// Updates the original position (used after swapping)
    /// </summary>
    public void SetOriginalPosition(Vector3 newPosition)
    {
        originalPosition = newPosition;
    }

    /// <summary>
    /// Rolls the die to land on a specific value (0-9, where 0 = 10)
    /// </summary>
    public void RollToValue(int value, float duration)
    {
        if (value < 0 || value > 9)
        {
            Debug.LogError($"Invalid die value: {value}. Must be 0-9.");
            return;
        }

        if (rollCoroutine != null)
        {
            StopCoroutine(rollCoroutine);
        }

        targetValue = value;
        rollCoroutine = StartCoroutine(PerformRoll(duration));
    }

    IEnumerator PerformRoll(float duration)
    {
        isRolling = true;

        // Start from right side, offset from original position
        Vector3 startPosition = originalPosition + rollStartOffset;
        transform.position = startPosition;
        transform.rotation = Random.rotation; // Start with random rotation

        rb.isKinematic = false;
        rb.useGravity = true;

        // Apply force toward original position (leftward and slightly down)
        Vector3 directionToTarget = (originalPosition - startPosition).normalized;
        Vector3 rollDirection = directionToTarget + Vector3.up * 0.3f; // Slight upward arc
        Vector3 forceToApply = rollDirection * rollForce;

        // Add some random variation to make it look natural
        forceToApply += new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.2f, 0.2f),
            Random.Range(-0.5f, 0.5f)
        ) * rollForce * 0.2f;

        Vector3 randomTorque = Random.insideUnitSphere * rollTorque;

        rb.AddForce(forceToApply, ForceMode.Impulse);
        rb.AddTorque(randomTorque, ForceMode.Impulse);

        // Wait for most of the duration
        yield return new WaitForSeconds(duration * 0.6f);

        // Start settling toward target value
        float settleTime = duration * 0.4f;
        float elapsed = 0f;

        while (elapsed < settleTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / settleTime;

            // Ease-out curve for smooth settling
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            // Gradually reduce physics influence
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, smoothT);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, smoothT);

            // Smoothly rotate toward target face
            Quaternion targetRotation = faces[targetValue].rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothT);

            // Move toward original position
            transform.position = Vector3.Lerp(transform.position, originalPosition, smoothT);

            yield return null;
        }

        // Final snap to exact position and rotation
        transform.rotation = faces[targetValue].rotation;
        transform.position = originalPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Disable physics
        rb.isKinematic = true;
        rb.useGravity = false;

        isRolling = false;
    }

    /// <summary>
    /// Setup helper - call this in editor to capture current rotation for a face
    /// </summary>
    [ContextMenu("Capture Current Rotation for Selected Face")]
    public void CaptureRotation()
    {
        Debug.Log($"Current rotation: {transform.rotation.eulerAngles}");
    }

    void OnDrawGizmos()
    {
        // Draw wire sphere at roll start position
        Gizmos.color = Color.yellow;
        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(originalPosition + rollStartOffset, 0.2f);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position + rollStartOffset, 0.2f);
        }

        // Draw line showing roll path
        Gizmos.color = Color.green;
        if (Application.isPlaying)
        {
            Gizmos.DrawLine(originalPosition + rollStartOffset, originalPosition);
        }
        else
        {
            Gizmos.DrawLine(transform.position + rollStartOffset, transform.position);
        }
    }
}

[System.Serializable]
public class DiceFace
{
    [Tooltip("Value this face represents (0-9, where 0 = 10)")]
    public int value;

    [Tooltip("Rotation needed for this face to be on top")]
    public Quaternion rotation;

    public DiceFace(int val, Vector3 eulerAngles)
    {
        value = val;
        rotation = Quaternion.Euler(eulerAngles);
    }
}