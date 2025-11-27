using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum QTEResult { Miss, Good, Perfect }

public class CircularQTE : MonoBehaviour
{
    [Header("Panel & Blocking")]
    [SerializeField] private GameObject panelRoot;     // QTE_Panel (keep INACTIVE in scene)
    [SerializeField] private CanvasGroup panelGroup;   // optional CanvasGroup on QTE_Panel

    [Header("Needle")]
    [SerializeField] private RectTransform needlePivot; // rotate THIS
    [Tooltip("Where the needle starts drawing from (deg). Mostly cosmetic.")]
    [SerializeField] private float startAngle = 90f;
    [SerializeField] private bool clockwise = true;

    [Header("Judge (calibrate these to your visuals)")]
    [Tooltip("Degrees for the center of the GREEN window. Bottom = 270, Right = 0, Top = 90, Left = 180.")]
    [SerializeField] private float centerAngleDeg = 270f;     // bottom (your new layout)
    [Tooltip("Total GREEN width in degrees (e.g., 20 => ±10°).")]
    [SerializeField] private float greenWidthDeg = 20f;       // Perfect window total width
    [Tooltip("Total YELLOW width in degrees (outside green).")]
    [SerializeField] private float yellowWidthDeg = 90f;      // Good window total width

    [Header("Timing")]
    [SerializeField] private float timeToFail = 2.0f;         // set by tier
    public void SetDuration(float seconds) => timeToFail = Mathf.Max(0.1f, seconds);
    public void SetCenterAngle(float deg) => centerAngleDeg = ((deg % 360f) + 360f) % 360f;

    [Header("Outcome Texts (optional)")]
    [SerializeField] private Text failedText; // "FAILED"
    [SerializeField] private Text guardText;  // "GUARD"
    [SerializeField] private Text dodgeText;  // "DODGE"
    [SerializeField] private float outcomeFlash = 0.6f;

    [Header("Debug (optional)")]
    [SerializeField] private Text debugText; // shows ang/err/center while running

    bool _running;
    Coroutine _routine;

    void Awake()
    {
        // make sure panel is hidden at boot
        ShowPanel(false);
        if (failedText) failedText.gameObject.SetActive(false);
        if (guardText)  guardText.gameObject.SetActive(false);
        if (dodgeText)  dodgeText.gameObject.SetActive(false);
    }

    public IEnumerator RunDefense(Action<QTEResult> onDone)
    {
        if (_running) yield break;
        _running = true;
        _routine = StartCoroutine(RunDefenseRoutine(onDone));
        yield return _routine;
    }

    IEnumerator RunDefenseRoutine(Action<QTEResult> onDone)
    {
        ShowPanel(true);

        // reset
        if (needlePivot) needlePivot.localEulerAngles = new Vector3(0, 0, startAngle);

        float elapsed = 0f;
        QTEResult result = QTEResult.Miss;

        while (elapsed < timeToFail)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / timeToFail);
            float spin = (clockwise ? -360f : 360f) * t + startAngle;
            if (needlePivot) needlePivot.localEulerAngles = new Vector3(0, 0, spin);

            if (debugText)
            {
                float a = GetCurrentAngleDeg();
                float err = Mathf.DeltaAngle(a, centerAngleDeg);
                debugText.text = $"ang:{a:0}°  err:{err:0}°  center:{centerAngleDeg:0}°";
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                result = Judge(GetCurrentAngleDeg());
                ShowOutcome(result);
                break;
            }

            yield return null;
        }

        // if time ran out without SPACE, result stays Miss; flash FAILED so player knows
        if (elapsed >= timeToFail) ShowOutcome(QTEResult.Miss);

        yield return new WaitForSecondsRealtime(0.05f); // small settle

        ShowPanel(false);
        _running = false;
        _routine = null;

        onDone?.Invoke(result);
    }

    // --- Helpers -------------------------------------------------------------

    void ShowPanel(bool show)
    {
        if (panelRoot) panelRoot.SetActive(show);
        if (panelGroup)
        {
            panelGroup.alpha          = show ? 1f : 0f;
            panelGroup.interactable   = show;
            panelGroup.blocksRaycasts = show;
        }
    }

    float GetCurrentAngleDeg()
    {
        // Normalize [0..360)
        float z = needlePivot ? needlePivot.localEulerAngles.z : 0f;
        return (z % 360f + 360f) % 360f;
    }

    QTEResult Judge(float currentAngleDeg)
    {
        // shortest signed difference between needle and center
        float err = Mathf.Abs(Mathf.DeltaAngle(currentAngleDeg, centerAngleDeg));
        float halfGreen  = Mathf.Max(0.1f, greenWidthDeg)  * 0.5f;
        float halfYellow = Mathf.Max(0.1f, yellowWidthDeg) * 0.5f;

        if (err <= halfGreen)              return QTEResult.Perfect; // GREEN (DODGE)
        if (err <= halfGreen + halfYellow) return QTEResult.Good;    // YELLOW (GUARD)
        return QTEResult.Miss;                                        // RED (FAILED)
    }

    void ShowOutcome(QTEResult r)
    {
        if (failedText) failedText.gameObject.SetActive(false);
        if (guardText)  guardText.gameObject.SetActive(false);
        if (dodgeText)  dodgeText.gameObject.SetActive(false);

        Text t = null;
        if (r == QTEResult.Perfect) t = dodgeText;
        else if (r == QTEResult.Good) t = guardText;
        else t = failedText;

        if (t) StartCoroutine(Flash(t));
    }

    IEnumerator Flash(Text t)
    {
        t.gameObject.SetActive(true);
        yield return new WaitForSeconds(outcomeFlash);
        t.gameObject.SetActive(false);
    }
}