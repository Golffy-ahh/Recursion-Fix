// Actors/Character.cs
using UnityEngine;

public abstract class Character : IIdentity
{
    public string Name { get; private set; }
    public int HP { get; set; }
    public int MaxHP { get; private set; }
    public int AP { get; set; }
    public int APMax { get; private set; }
    public int ATK { get; private set; }

    protected Character(string name, int maxHP, int atk, int apMax = 6, int startAP = 0)
    {
        Name = name; MaxHP = maxHP; HP = maxHP;
        ATK = atk; APMax = apMax; AP = startAP;
    }

    public void Heal(int amount)       => HP = Mathf.Min(MaxHP, HP + Mathf.Max(0, amount));
    public void TakeDamage(int amount) => HP = Mathf.Max(0, HP - Mathf.Max(0, amount));
    public bool SpendAP(int cost)
    {
        if (AP < cost) return false;
        AP -= cost; return true;
    }
}
