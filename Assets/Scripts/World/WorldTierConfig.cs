using UnityEngine;

[CreateAssetMenu(menuName="IGD347/World Tier")]
public class WorldTierConfig : ScriptableObject
{
    [Range(1, 4)] public int tierId = 1;

    [Header("Difficulty")]
    [Tooltip("Multiplies enemy HP/ATK/DEF")]
    public float enemyStatMult = 1f;

    [Tooltip("Seconds for a full defense QTE sweep before auto-fail")]
    public float qteTimeToFail = 2.0f;   // T1=2.0, T2=1.5, T3=1.25, Boss=1.0
    public float ArrowQTETimer = 4.0f;
}
