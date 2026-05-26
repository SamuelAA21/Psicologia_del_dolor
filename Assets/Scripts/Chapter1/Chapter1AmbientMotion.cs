using UnityEngine;

public class Chapter1AmbientMotion : MonoBehaviour
{
    [SerializeField] private bool bob = true;
    [SerializeField] private float bobAmount = 0.08f;
    [SerializeField] private float bobSpeed = 1.8f;
    [SerializeField] private bool rotate;
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 35f, 0f);
    [SerializeField] private bool pulseScale;
    [SerializeField] private float pulseAmount = 0.06f;
    [SerializeField] private float pulseSpeed = 2.4f;
    [SerializeField] private Light pulseLight;
    [SerializeField] private float lightPulseAmount = 0.35f;

    private Vector3 startLocalPosition;
    private Vector3 startLocalScale;
    private float startLightIntensity;
    private float phaseOffset;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
        startLocalScale = transform.localScale;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);

        if (pulseLight == null)
        {
            pulseLight = GetComponentInChildren<Light>();
        }

        if (pulseLight != null)
        {
            startLightIntensity = pulseLight.intensity;
        }
    }

    private void Update()
    {
        float bobValue = Mathf.Sin(Time.time * bobSpeed + phaseOffset);

        if (bob)
        {
            transform.localPosition = startLocalPosition + Vector3.up * (bobValue * bobAmount);
        }

        if (rotate)
        {
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        }

        if (pulseScale)
        {
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed + phaseOffset) * pulseAmount;
            transform.localScale = startLocalScale * scale;
        }

        if (pulseLight != null)
        {
            pulseLight.intensity = Mathf.Max(0f, startLightIntensity + bobValue * lightPulseAmount);
        }
    }

    public void Configure(bool enableBob, float newBobAmount, bool enableRotate, Vector3 newRotationSpeed, bool enablePulseScale)
    {
        bob = enableBob;
        bobAmount = newBobAmount;
        rotate = enableRotate;
        rotationSpeed = newRotationSpeed;
        pulseScale = enablePulseScale;
    }
}
