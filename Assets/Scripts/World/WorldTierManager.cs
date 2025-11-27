// World/WorldTierManager.cs
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class WorldTierManager : MonoBehaviour
{
    [Header("Ordered lowest → highest (last = Boss tier)")]
    [SerializeField] private WorldTierConfig[] tiers;

    [Header("Stages per non-boss tier")]
    [SerializeField, Min(1)] private int stagesPerTier = 3;

    [SerializeField, Tooltip("0-based tier index")]
    private int currentIndex = 0;

    // 1-based stage number within the current tier (resets to 1 on tier change)
    [SerializeField, Min(1)] private int currentStage = 1;

    // --- Events ---
    public event Action<WorldTierConfig> OnTierChanged;
    public event Action<int,int,WorldTierConfig> OnProgressChanged; // (tierIdx, stage, config)

    // --- Properties used by other systems (kept stable) ---
    public int CurrentIndex => Mathf.Clamp(currentIndex, 0, Mathf.Max(0, TierCount - 1));
    public WorldTierConfig Current => tiers == null || tiers.Length == 0
        ? null
        : tiers[Mathf.Clamp(currentIndex, 0, tiers.Length - 1)];

    // --- Convenience ---
    public int TierCount => tiers?.Length ?? 0;
    public bool CanAdvanceTier => currentIndex < TierCount - 1;
    public bool IsBossTier => TierCount > 0 && currentIndex == TierCount - 1;
    public int CurrentStage => Mathf.Max(1, currentStage);
    public int StagesPerTier => stagesPerTier;

    // Label helper for UI: "Layer X - Y"
    public string GetLayerStageLabel() => $"Layer {CurrentIndex + 1} - {CurrentStage}";

    // Reset everything (useful when starting a new run)
    public void ResetProgress(int tierIndex = 0)
    {
        currentIndex = Mathf.Clamp(tierIndex, 0, Mathf.Max(0, TierCount - 1));
        currentStage = 1;
        FireProgressEvents();
    }

    // Advance just the stage (auto-advances tier when needed)
    public void AdvanceStage()
    {
        if (IsBossTier)
        {
            // Boss tier has a single "boss stage"
            // You can decide what to do next (stay, end run, or allow tier loop)
            FireProgressEvents();
            return;
        }

        currentStage++;

        if (currentStage > stagesPerTier)
        {
            AdvanceTier();
            return;
        }

        FireProgressEvents();
    }

    // Advance to the next tier (resets stage to 1)
    public void AdvanceTier()
    {
        if (!CanAdvanceTier) { FireProgressEvents(); return; }

        currentIndex++;
        currentStage = 1;

        OnTierChanged?.Invoke(Current);
        FireProgressEvents();
    }

    // Force-set tier & optional stage (used by debug buttons or loading)
    public void SetTierAndStage(int tierIndex, int stage = 1)
    {
        currentIndex = Mathf.Clamp(tierIndex, 0, Mathf.Max(0, TierCount - 1));
        currentStage = Mathf.Max(1, stage);
        OnTierChanged?.Invoke(Current);
        FireProgressEvents();
    }
    public void SetCurrentIndex(int tierIndex)
    {
        SetTierAndStage(tierIndex, 1);
    }


    private void FireProgressEvents()
    {
        OnProgressChanged?.Invoke(CurrentIndex, CurrentStage, Current);
    }
}