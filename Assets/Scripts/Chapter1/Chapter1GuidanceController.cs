using UnityEngine;
using UnityEngine.UI;

public class Chapter1GuidanceController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform currentTarget;

    [Header("HUD")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Text objectiveText;
    [SerializeField] private Text distanceText;

    [Header("Marcador en mundo")]
    [SerializeField] private GameObject waypointMarker;
    [SerializeField] private float markerHeight = 3.2f;
    [SerializeField] private float markerBobAmount = 0.25f;
    [SerializeField] private float markerBobSpeed = 2.2f;
    [SerializeField] private float markerRotateSpeed = 80f;

    private string currentObjective;
    private Light markerLight;
    private Renderer markerRenderer;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        EnsureHud();
        EnsureWaypointMarker();
        SetVisible(false);
    }

    private void Update()
    {
        if (currentTarget == null)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);
        UpdateWaypointMarker();
        UpdateDistanceText();
    }

    public void SetObjective(string objective, Transform target)
    {
        currentObjective = objective;
        currentTarget = target;

        if (objectiveText != null)
        {
            objectiveText.text = objective;
        }

        UpdateDistanceText();
        SetVisible(target != null);
    }

    public void ClearObjective()
    {
        currentTarget = null;
        currentObjective = string.Empty;
        SetVisible(false);
    }

    private void EnsureHud()
    {
        if (hudCanvas != null && objectiveText != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("Chapter1_ObjectiveHUD");
        hudCanvas = canvasObject.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 20;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("ObjectivePanel");
        panel.transform.SetParent(canvasObject.transform, false);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.03f, 0.05f, 0.07f, 0.7f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(24f, -24f);
        panelRect.sizeDelta = new Vector2(410f, 86f);

        objectiveText = CreateHudText(panel.transform, "ObjectiveText", 18, FontStyle.Bold, new Vector2(18f, -16f), new Vector2(374f, 30f));
        distanceText = CreateHudText(panel.transform, "DistanceText", 14, FontStyle.Normal, new Vector2(18f, -52f), new Vector2(374f, 22f));
    }

    private static Text CreateHudText(Transform parent, string name, int fontSize, FontStyle fontStyle, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        return text;
    }

    private void EnsureWaypointMarker()
    {
        if (waypointMarker == null)
        {
            waypointMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            waypointMarker.name = "Chapter1_WaypointMarker";
            waypointMarker.transform.localScale = Vector3.one * 0.32f;
        }

        markerRenderer = waypointMarker.GetComponent<Renderer>();
        if (markerRenderer != null)
        {
            Material material = new Material(markerRenderer.sharedMaterial);
            Color markerColor = new Color(0.35f, 0.9f, 1f, 1f);
            material.color = markerColor;
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", markerColor * 1.4f);
            }

            markerRenderer.material = material;
        }

        markerLight = waypointMarker.GetComponentInChildren<Light>();
        if (markerLight == null)
        {
            GameObject lightObject = new GameObject("WaypointLight");
            lightObject.transform.SetParent(waypointMarker.transform, false);
            markerLight = lightObject.AddComponent<Light>();
            markerLight.type = LightType.Point;
            markerLight.color = new Color(0.35f, 0.9f, 1f);
            markerLight.range = 3f;
        }
    }

    private void UpdateWaypointMarker()
    {
        float bob = Mathf.Sin(Time.time * markerBobSpeed) * markerBobAmount;
        waypointMarker.transform.position = currentTarget.position + Vector3.up * (markerHeight + bob);
        waypointMarker.transform.Rotate(Vector3.up, markerRotateSpeed * Time.deltaTime, Space.World);

        if (playerCamera != null)
        {
            Vector3 cameraDirection = waypointMarker.transform.position - playerCamera.transform.position;
            if (cameraDirection.sqrMagnitude > 0.001f)
            {
                waypointMarker.transform.rotation = Quaternion.LookRotation(cameraDirection.normalized, Vector3.up);
            }
        }

        if (markerLight != null)
        {
            markerLight.intensity = 1.6f + Mathf.Sin(Time.time * 3f) * 0.45f;
        }
    }

    private void UpdateDistanceText()
    {
        if (distanceText == null)
        {
            return;
        }

        if (currentTarget == null || playerController == null)
        {
            distanceText.text = string.Empty;
            return;
        }

        float distance = Vector3.Distance(playerController.transform.position, currentTarget.position);
        distanceText.text = $"{Mathf.RoundToInt(distance)} m";
    }

    private void SetVisible(bool visible)
    {
        if (hudCanvas != null)
        {
            hudCanvas.gameObject.SetActive(visible);
        }

        if (waypointMarker != null)
        {
            waypointMarker.SetActive(visible);
        }
    }
}
