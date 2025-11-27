using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowQTEImages : MonoBehaviour
{
    [Header("Panel & Slots")]
    [SerializeField] private GameObject panelRoot;     // e.g., QTE_SkillPanel (can be inactive by default)
    [SerializeField] private List<Image> arrowSlots;   // EXACTLY 4 Image slots, left->right order

    [Header("Sprites")]
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite rightSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite downSprite;

    [Header("UI (optional)")]
    [SerializeField] private Text timerText;           // shows countdown
    [SerializeField] private Color pending = Color.white;
    [SerializeField] private Color passed  = Color.green;
    [SerializeField] private Color failed  = Color.red;

    private enum Arrow { Up, Right, Left, Down }
    private readonly System.Random _rng = new System.Random();
    private readonly Arrow[] _pool = new[] { Arrow.Up, Arrow.Right, Arrow.Left, Arrow.Down };

    private float _totalTime = 3.5f; // default; set via SetTotalTime

    public void SetTotalTime(float seconds) => _totalTime = Mathf.Max(0.5f, seconds);

    public IEnumerator RunQTE(Action<bool> onDone)
    {
        // Build a permutation of all 4 arrows
        var seq = new List<Arrow>(_pool);
        // Fisher-Yates shuffle
        for (int i = seq.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (seq[i], seq[j]) = (seq[j], seq[i]);
        }

        // Assign sprites to slots left->right
        for (int i = 0; i < arrowSlots.Count; i++)
        {
            var slot = arrowSlots[i];
            slot.enabled = true;
            slot.color   = pending;
            slot.sprite  = SpriteOf(seq[i]);
        }

        panelRoot?.SetActive(true);

        float t = _totalTime;
        int index = 0;
        bool ok = true;

        while (t > 0f && index < 4)
        {
            // show countdown
            if (timerText) timerText.text = $"{t:0.0}s";

            // check input: only arrow/WASD mapped to expected arrow pass; any other mapped arrow = fail
            Arrow expected = seq[index];
            if (Pressed(expected))
            {
                arrowSlots[index].color = passed;
                index++;
            }
            else
            {
                // if user pressed a different arrow key, fail immediately
                if (PressedOtherThan(expected))
                {
                    ok = false;
                    if (index < arrowSlots.Count) arrowSlots[index].color = failed;
                    break;
                }
            }

            t -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (index < 4) ok = false; // time out
        if (timerText) timerText.text = ok ? "OK" : "Fail";

        yield return new WaitForSecondsRealtime(0.25f);
        panelRoot?.SetActive(false);
        onDone?.Invoke(ok);
    }

    private Sprite SpriteOf(Arrow a)
    {
        switch (a)
        {
            case Arrow.Up:    return upSprite;
            case Arrow.Right: return rightSprite;
            case Arrow.Left:  return leftSprite;
            default:          return downSprite;
        }
    }

    private static bool Pressed(Arrow a)
    {
        switch (a)
        {
            case Arrow.Up:    return Input.GetKeyDown(KeyCode.UpArrow)    || Input.GetKeyDown(KeyCode.W);
            case Arrow.Right: return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
            case Arrow.Left:  return Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A);
            default:          return Input.GetKeyDown(KeyCode.DownArrow)  || Input.GetKeyDown(KeyCode.S);
        }
    }

    private static bool PressedOtherThan(Arrow expected)
    {
        bool up    = Input.GetKeyDown(KeyCode.UpArrow)    || Input.GetKeyDown(KeyCode.W);
        bool right = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
        bool left  = Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A);
        bool down  = Input.GetKeyDown(KeyCode.DownArrow)  || Input.GetKeyDown(KeyCode.S);

        switch (expected)
        {
            case Arrow.Up:    return right || left || down;
            case Arrow.Right: return up || left || down;
            case Arrow.Left:  return up || right || down;
            default:          return up || right || left;
        }
    }
}