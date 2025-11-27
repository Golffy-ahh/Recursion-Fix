// Skills/ISkill.cs
using System;
using System.Collections;
using UnityEngine;

public interface ISkill
{
    string Name { get; }
    int Cost { get; }
    bool RequiresQTE { get; }  // true => use ArrowQTE before applying
    string Label { get; }      // e.g. "Heal", "Heavy Slash", "Magic Bullet"

    // Perform is an IEnumerator so the skill can run its own little sequence if needed.
    IEnumerator Perform(EncounterManager ctx, IIdentity user, IIdentity target, Action<bool> onDone);
}
