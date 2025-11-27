// Skills/HealSkill.cs
using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class HealSkill : ISkill
{
    [SerializeField] string label = "Heal";
    [SerializeField] int amount = 20;
    [SerializeField] int cost = 2;

    public string Name => $"{label} (+{amount} HP)";
    public string Label => label;
    public int Cost => cost;
    public bool RequiresQTE => true;

    public IEnumerator Perform(EncounterManager ctx, IIdentity user, IIdentity target, Action<bool> onDone)
    {
        // Arrow QTE gate
        if (RequiresQTE && ctx.ArrowQTE != null)
        {
            bool ok = false;
            yield return ctx.ArrowQTE.RunQTE(s => ok = s);
            if (!ok) { onDone?.Invoke(false); yield break; }
        }

        if (!user.SpendAP(Cost)) { onDone?.Invoke(false); yield break; }
        user.Heal(amount);
        ctx.FlashInfo($"+{amount} HP");
        onDone?.Invoke(true);
    }
}
