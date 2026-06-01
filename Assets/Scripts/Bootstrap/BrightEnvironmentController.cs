using UnityEngine;
using UnityEngine.SceneManagement;

public class BrightEnvironmentController : MonoBehaviour
{
    private const string TargetSceneName = "SampleScene";

    [SerializeField] private Color ambientSky = new Color(0.72f, 0.87f, 1f, 1f);
    [SerializeField] private Color ambientEquator = new Color(0.58f, 0.74f, 0.82f, 1f);
    [SerializeField] private Color ambientGround = new Color(0.36f, 0.44f, 0.34f, 1f);
    [SerializeField] private float ambientIntensity = 1.28f;
    [SerializeField] private Color sunColor = new Color(1f, 0.94f, 0.78f, 1f);
    [SerializeField] private float sunIntensity = 1.35f;
    [SerializeField] private Vector3 sunEulerAngles = new Vector3(42f, -34f, 0f);

    private Light sunLight;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureForScene(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureForScene(scene);
    }

    private static void EnsureForScene(Scene scene)
    {
        if (!scene.name.Equals(TargetSceneName, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (FindAnyObjectByType<BrightEnvironmentController>(FindObjectsInactive.Include) != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject("BrightEnvironmentController");
        controllerObject.AddComponent<BrightEnvironmentController>();
    }

    private void Awake()
    {
        ApplyEnvironment();
    }

    private void ApplyEnvironment()
    {
        RenderSettings.fog = false;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = ambientSky;
        RenderSettings.ambientEquatorColor = ambientEquator;
        RenderSettings.ambientGroundColor = ambientGround;
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.reflectionIntensity = 1f;

        EnsureSun();
        RenderSettings.sun = sunLight;
    }

    private void EnsureSun()
    {
        if (sunLight == null)
        {
            GameObject sunObject = new GameObject("Daylight Sun");
            sunObject.transform.SetParent(transform, false);
            sunLight = sunObject.AddComponent<Light>();
            sunLight.type = LightType.Directional;
        }

        sunLight.transform.rotation = Quaternion.Euler(sunEulerAngles);
        sunLight.color = sunColor;
        sunLight.intensity = sunIntensity;
        sunLight.shadows = LightShadows.Soft;
    }
}
