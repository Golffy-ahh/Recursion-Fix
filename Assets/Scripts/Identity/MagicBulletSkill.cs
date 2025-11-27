// Skills/MagicBulletSkill.cs
using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class MagicBulletSkill : ISkill
{
    [SerializeField] string label = "Magic Bullet";
    [SerializeField] int damage = 50;
    [SerializeField] int cost = 3;

    public string Name => $"{label} ({damage})";
    public string Label => label;
    public int Cost => cost;
    public bool RequiresQTE => true;

    public IEnumerator Perform(EncounterManager ctx, IIdentity user, IIdentity target, Action<bool> onDone)
    {
        if (RequiresQTE && ctx.ArrowQTE != null)
        {
            bool ok = false;
            yield return ctx.ArrowQTE.RunQTE(s => ok = s);
            if (!ok)
            {
                onDone?.Invoke(false);
                yield break;
            }
        }

        if (!user.SpendAP(Cost))
        {
            onDone?.Invoke(false);
            yield break;
        }

        ctx.PlayPlayerSkillAnimation("Player_Attack_WithEffect");

        yield return new WaitForSeconds(0.1f);

        target.TakeDamage(damage);
        ctx.FlashInfo($"{label} blasts {damage}!");

        onDone?.Invoke(true);
    }
}
