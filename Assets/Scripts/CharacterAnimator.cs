using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimator : MonoBehaviour, ICharacterAnimator
{
    [SerializeField] private Animator animator;

    [Header("State Names (Animator Controller)")]
    public string idleState = "Player_Idle";
    public string attackState = "Player_Attack";
    public string guardState = "Player_Sprite_Guard";
    public string skillState = "Player_Skills_Attack";
    public string healState = "Player_Sprite_Heal";
    public string dodgeState = "Player_Sprite_Dodge";

    [Header("Audio Service")]
    [SerializeField] private MonoBehaviour audioServiceSource;
    private IAudioService audioService;

    [Header("SFX Clips")]
    public AudioClip attackSFX;
    public AudioClip guardSFX;
    public AudioClip dodgeSFX;

    private void Awake()
    {
        if (!animator)
            animator = GetComponent<Animator>();

        if (audioServiceSource != null)
            audioService = audioServiceSource as IAudioService;
    }

    private void CrossFadeTo(string stateName, float duration = 0.1f)
    {
        if (!animator) return;
        if (string.IsNullOrEmpty(stateName)) return;

        animator.CrossFadeInFixedTime(stateName, duration, 0, 0f);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        if (audioService == null) return;

        audioService.PlayOneShot(clip);
    }

    public void PlayIdle()
    {
        CrossFadeTo(idleState);
    }

    public void PlayAttack()
    {
        CrossFadeTo(attackState);
        PlaySFX(attackSFX);
    }

    public void PlayGuard()
    {
        CrossFadeTo(guardState);
        PlaySFX(guardSFX);
    }

    public void PlaySkill(string skillName = null)
    {
        if (!string.IsNullOrEmpty(skillName))
            CrossFadeTo(skillName);
        else
            CrossFadeTo(skillState);
    }

    public void PlayHeal(string skillName = null)
    {
        if (!string.IsNullOrEmpty(skillName))
            CrossFadeTo(skillName);
        else
            CrossFadeTo(healState);
    }

    public void PlayDodge(string skillName = null)
    {
        if (!string.IsNullOrEmpty(skillName))
            CrossFadeTo(skillName);
        else
            CrossFadeTo(dodgeState);

        PlaySFX(dodgeSFX);
    }
}
