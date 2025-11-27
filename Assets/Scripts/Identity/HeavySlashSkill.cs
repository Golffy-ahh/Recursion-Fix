// Skills/HeavySlashSkill.cs
using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class HeavySlashSkill : ISkill
{
    [SerializeField] string label = "Heavy Slash";
    [SerializeField] int damage = 30;
    [SerializeField] int cost = 2;

    public string Name => $"{label} ({damage})";
    public string Label => label;
    public int Cost => cost;
    public bool RequiresQTE => true;

    public IEnumerator Perform(EncounterManager ctx, IIdentity user, IIdentity target, Action<bool> onDone)
    {
        bool ok = false;
        yield return ctx.ArrowQTE.RunQTE(s => ok = s);
        if (!ok) { onDone?.Invoke(false); yield break; }

        if (!user.SpendAP(Cost)) { onDone?.Invoke(false); yield break; }
        target.TakeDamage(damage);
        ctx.FlashInfo($"{label} hits {damage}!");
        onDone?.Invoke(true);
    }
}
