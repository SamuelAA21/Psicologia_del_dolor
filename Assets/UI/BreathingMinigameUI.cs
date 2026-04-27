using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Yarn.Unity;

/// <summary>
/// Minijuego de respiracion cuadrada (4-4-4) con UI Toolkit.
///
/// SETUP:
/// 1. Crea un GameObject vacio "BreathingMinigame" en la escena
/// 2. Agrega un componente UIDocument a ese GameObject
/// 3. Asigna BreathingMinigame.uxml al campo Source Asset del UIDocument
/// 4. Asigna BreathingMinigame.uss al Panel Settings del UIDocument
/// 5. Agrega este script al mismo GameObject
/// 6. Pon los archivos .uxml y .uss en Assets/UI/
///
/// ACTIVACION DESDE YARN:
///     <<iniciar_respiracion>>
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class BreathingMinigameUI : MonoBehaviour
{
    [Header("Configuracion de respiracion")]
    [SerializeField] private float inhaleTime     = 4f;
    [SerializeField] private float holdTime       = 4f;
    [SerializeField] private float exhaleTime     = 4f;
    [SerializeField] private int   requiredCycles = 4;
    [SerializeField] private float followRadius   = 40f;
    [SerializeField] private float calmGainRate   = 3f;
    [SerializeField] private float calmLossRate   = 1.5f;
    [SerializeField] private float minAccuracy    = 0.55f;

    [Header("Yarn Spinner")]
    [SerializeField] private string completionVar = "$respiracion_completa";
    [SerializeField] private string coinsVar      = "$monedas";

    private UIDocument   uiDoc;
    private VisualElement root;
    private VisualElement phaseBg, guidePoint, followZone, cursorDot, calmFillEl, completePanel, coinsContainer;
    private Label        phaseLabel, phaseSub, calmPct, cyclesLabel, coinCountLabel, completeLabel;
    private Button       startButton;
    private IMGUIContainer curveContainer;

    private InMemoryVariableStorage yarnStorage;

    private bool  isRunning;
    private float elapsed;
    private float calm;
    private int   coins, cycles, followHits, followTotal;
    private Vector2 mousePos;
    private Vector2 gameAreaSize;
    private Rect  gameAreaRect;

    private float CycleTime => inhaleTime + holdTime + exhaleTime;

    private void Awake()
    {
        uiDoc      = GetComponent<UIDocument>();
        yarnStorage = FindObjectOfType<InMemoryVariableStorage>();
    }

    private void OnEnable()
    {
        root = uiDoc.rootVisualElement;

        phaseBg        = root.Q("phase-background");
        guidePoint     = root.Q("guide-point");
        followZone     = root.Q("follow-zone");
        cursorDot      = root.Q("cursor-dot");
        calmFillEl     = root.Q("calm-fill");
        completePanel  = root.Q("complete-panel");
        coinsContainer = root.Q("coins-container");
        curveContainer = root.Q<IMGUIContainer>("curve-container");

        phaseLabel    = root.Q<Label>("phase-label");
        phaseSub      = root.Q<Label>("phase-sub");
        calmPct       = root.Q<Label>("calm-pct");
        cyclesLabel   = root.Q<Label>("cycles-label");
        coinCountLabel = root.Q<Label>("coin-count");
        completeLabel  = root.Q<Label>("complete-label");
        startButton    = root.Q<Button>("start-button");

        startButton?.RegisterCallback<ClickEvent>(_ => OnStartClicked());

        // Registrar dibujado de curva
        if (curveContainer != null)
            curveContainer.onGUIHandler = DrawCurveIMGUI;

        // Seguimiento del mouse
        root.RegisterCallback<MouseMoveEvent>(e => mousePos = e.localMousePosition);

        HideGame();
    }

    // ── Activacion desde Yarn ────────────────────────────────────────────────

    [YarnCommand("iniciar_respiracion")]
    public void StartMinigame()
    {
        if (isRunning) return;
        ShowGame();
        StartCoroutine(RunMinigame());
    }

    private void OnStartClicked()
    {
        StopAllCoroutines();
        ShowGame();
        StartCoroutine(RunMinigame());
    }

    // ── Loop principal ───────────────────────────────────────────────────────

    private IEnumerator RunMinigame()
    {
        isRunning   = true;
        elapsed     = 0f;
        calm        = 0f;
        coins       = 0;
        cycles      = 0;
        followHits  = 0;
        followTotal = 0;

        completePanel?.AddToClassList("hidden");
        startButton?.AddToClassList("hidden");
        UpdateCyclesLabel();
        UpdateCoinLabel();

        while (isRunning && cycles < requiredCycles)
        {
            elapsed += Time.deltaTime;
            float tInCycle = elapsed % CycleTime;
            PhaseInfo phase = GetPhase(tInCycle);

            // Calcular posicion del punto guia dentro del game-area
            Vector2 guidePos = GetGuidePos(tInCycle);

            // Mover punto guia y zona
            SetElementPos(guidePoint,  guidePos, 32f);
            SetElementPos(followZone,  guidePos, followRadius * 2f);
            SetElementPos(cursorDot,   mousePos, 22f);

            // Seguimiento
            float dist      = Vector2.Distance(mousePos, guidePos);
            bool  following = dist < followRadius;

            followTotal++;
            if (following) followHits++;

            calm = following
                ? Mathf.Min(1f, calm + calmGainRate * Time.deltaTime)
                : Mathf.Max(0f, calm - calmLossRate * Time.deltaTime);

            // Actualizar calma
            int pct = Mathf.RoundToInt(calm * 100);
            if (calmFillEl != null) calmFillEl.style.width = Length.Percent(pct);
            if (calmPct    != null) calmPct.text = pct + "%";

            // Colores de fase via clases USS
            ApplyPhaseClasses(phase.type);

            // Labels
            if (phaseLabel != null) phaseLabel.text = phase.name;
            if (phaseSub   != null) phaseSub.text   = phase.sub;

            // Detectar fin de ciclo
            int completedNow = Mathf.FloorToInt(elapsed / CycleTime);
            if (completedNow > cycles)
            {
                float accuracy = followTotal > 0 ? (float)followHits / followTotal : 0f;
                if (accuracy >= minAccuracy) AwardCoin(guidePos);
                followHits  = 0;
                followTotal = 0;
                cycles      = completedNow;
                UpdateCyclesLabel();
            }

            // Redibujar curva
            curveContainer?.MarkDirtyRepaint();

            yield return null;
        }

        OnComplete();
    }

    // ── Curva con IMGUI ──────────────────────────────────────────────────────

    private void DrawCurveIMGUI()
    {
        if (!isRunning) return;

        Rect r       = curveContainer.contentRect;
        float w      = r.width;
        float h      = r.height;
        float tStart = elapsed % CycleTime;

        int   steps  = 120;
        Color col    = GetPhaseColor(GetPhase(elapsed % CycleTime).type);

        // Glow
        Handles_DrawAAPolyLine(w, h, tStart, steps, col * new Color(1,1,1,0.2f), 12f);
        // Linea principal
        Handles_DrawAAPolyLine(w, h, tStart, steps, col, 4f);
    }

    private void Handles_DrawAAPolyLine(float w, float h, float tStart, int steps, Color col, float width)
    {
        UnityEngine.GUI.color = col;
        Vector3 prev = Vector3.zero;
        for (int i = 0; i <= steps; i++)
        {
            float frac = (float)i / steps;
            float t    = (tStart + frac * CycleTime) % CycleTime;
            float x    = frac * w;
            float y    = GetCurveYPixel(t, h);
            Vector3 cur = new Vector3(x, y, 0);
            if (i > 0)
            {
                UnityEngine.GUI.DrawTexture(
                    GetLineRect(prev, cur, width),
                    Texture2D.whiteTexture,
                    ScaleMode.StretchToFill,
                    true
                );
            }
            prev = cur;
        }
    }

    private Rect GetLineRect(Vector3 a, Vector3 b, float thickness)
    {
        float x = Mathf.Min(a.x, b.x);
        float y = Mathf.Min(a.y, b.y);
        float w = Mathf.Max(Mathf.Abs(b.x - a.x), thickness);
        float h = Mathf.Max(Mathf.Abs(b.y - a.y), thickness);
        return new Rect(x, y, w, h);
    }

    // ── Posicion del punto guia ──────────────────────────────────────────────

    private Vector2 GetGuidePos(float tInCycle)
    {
        Rect r = curveContainer != null ? curveContainer.contentRect : new Rect(0,0,600,280);
        float w = r.width; float h = r.height;
        float x = Mathf.Lerp(0, w, tInCycle / CycleTime);
        float y = GetCurveYPixel(tInCycle, h);

        // Convertir a coordenadas del root
        Vector2 offset = curveContainer != null
            ? curveContainer.layout.position
            : Vector2.zero;
        return new Vector2(x + offset.x, y + offset.y);
    }

    private float GetCurveYPixel(float tInCycle, float h)
    {
        float topN = 0.12f, botN = 0.88f;
        float norm;
        if (tInCycle < inhaleTime)
            norm = Mathf.Lerp(botN, topN, EaseInOut(tInCycle / inhaleTime));
        else if (tInCycle < inhaleTime + holdTime)
            norm = topN;
        else
            norm = Mathf.Lerp(topN, botN, EaseInOut((tInCycle - inhaleTime - holdTime) / exhaleTime));
        return norm * h;
    }

    // ── Helpers de UI ────────────────────────────────────────────────────────

    private void SetElementPos(VisualElement el, Vector2 pos, float size)
    {
        if (el == null) return;
        el.style.position = Position.Absolute;
        el.style.left     = pos.x - size * 0.5f;
        el.style.top      = pos.y - size * 0.5f;
        el.style.width    = size;
        el.style.height   = size;
    }

    private void ApplyPhaseClasses(PhaseType type)
    {
        string[] bgClasses   = { "phase-inhale", "phase-hold", "phase-exhale" };
        string[] dotClasses  = { "guide-inhale", "guide-hold", "guide-exhale" };
        string[] zoneClasses = { "zone-inhale",  "zone-hold",  "zone-exhale" };
        string suffix = type == PhaseType.Inhale ? "inhale" : type == PhaseType.Hold ? "hold" : "exhale";

        SwapClass(phaseBg,    bgClasses,   "phase-" + suffix);
        SwapClass(guidePoint, dotClasses,  "guide-" + suffix);
        SwapClass(followZone, zoneClasses, "zone-"  + suffix);
    }

    private void SwapClass(VisualElement el, string[] all, string active)
    {
        if (el == null) return;
        foreach (var c in all) el.RemoveFromClassList(c);
        el.AddToClassList(active);
    }

    private void AwardCoin(Vector2 pos)
    {
        coins++;
        UpdateCoinLabel();
        StartCoroutine(AnimateCoin(pos));

        if (yarnStorage != null && !string.IsNullOrWhiteSpace(coinsVar))
        {
            string key = coinsVar.StartsWith("$") ? coinsVar : "$" + coinsVar;
            yarnStorage.TryGetValue(key, out float prev);
            yarnStorage.SetValue(key, prev + 1f);
        }
    }

    private IEnumerator AnimateCoin(Vector2 startPos)
    {
        if (coinsContainer == null) yield break;

        var coin = new VisualElement();
        coin.AddToClassList("coin-particle");
        var lbl = new Label("$");
        lbl.AddToClassList("coin-particle-label");
        coin.Add(lbl);
        coinsContainer.Add(coin);

        float t = 0f;
        Vector2 pos = startPos;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.2f;
            pos.y -= 60f * Time.deltaTime;
            coin.style.left    = pos.x - 12f;
            coin.style.top     = pos.y - 12f;
            coin.style.opacity = 1f - t;
            yield return null;
        }
        coinsContainer.Remove(coin);
    }

    private void UpdateCyclesLabel()
    {
        if (cyclesLabel != null)
            cyclesLabel.text = $"Ciclos: {Mathf.Min(cycles, requiredCycles)} / {requiredCycles}";
    }

    private void UpdateCoinLabel()
    {
        if (coinCountLabel != null) coinCountLabel.text = coins.ToString();
    }

    private void OnComplete()
    {
        isRunning = false;
        if (phaseLabel  != null) phaseLabel.text = "¡Completado!";
        if (phaseSub    != null) phaseSub.text   = $"Ganaste {coins} monedas";
        completePanel?.RemoveFromClassList("hidden");
        if (completeLabel != null)
            completeLabel.text = coins >= requiredCycles
                ? "¡Excelente control de tu respiración!"
                : "Buen intento. ¡Sigue practicando!";
        startButton?.RemoveFromClassList("hidden");

        if (yarnStorage != null && !string.IsNullOrWhiteSpace(completionVar))
        {
            string key = completionVar.StartsWith("$") ? completionVar : "$" + completionVar;
            yarnStorage.SetValue(key, true);
        }
    }

    private void ShowGame()
    {
        if (root != null) root.style.display = DisplayStyle.Flex;
    }

    private void HideGame()
    {
        if (root != null) root.style.display = DisplayStyle.None;
    }

    // ── Fases ────────────────────────────────────────────────────────────────

    private PhaseInfo GetPhase(float t)
    {
        if (t < inhaleTime)
            return new PhaseInfo("Inhala", "Sube siguiendo la curva", PhaseType.Inhale);
        if (t < inhaleTime + holdTime)
            return new PhaseInfo("Retén", "Mantén el cursor arriba", PhaseType.Hold);
        return new PhaseInfo("Exhala", "Baja siguiendo la curva", PhaseType.Exhale);
    }

    private Color GetPhaseColor(PhaseType type) => type switch
    {
        PhaseType.Inhale => new Color(0.11f, 0.62f, 0.46f),
        PhaseType.Hold   => new Color(0.73f, 0.46f, 0.09f),
        _                => new Color(0.22f, 0.54f, 0.87f)
    };

    private static float EaseInOut(float t) => t < 0.5f ? 2f*t*t : -1f+(4f-2f*t)*t;

    private enum PhaseType { Inhale, Hold, Exhale }

    private readonly struct PhaseInfo
    {
        public readonly string name, sub;
        public readonly PhaseType type;
        public PhaseInfo(string n, string s, PhaseType t) { name=n; sub=s; type=t; }
    }
}
