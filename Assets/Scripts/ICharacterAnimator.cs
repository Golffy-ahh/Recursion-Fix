// ICharacterAnimator.cs
using UnityEngine;

public interface ICharacterAnimator
{
    void PlayIdle();
    void PlayAttack();
    void PlayGuard();
    void PlaySkill(string skillName = null);
    void PlayHeal(string skillName = null);
    void PlayDodge(string skillName = null);
}
