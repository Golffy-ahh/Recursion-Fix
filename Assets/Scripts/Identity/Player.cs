// Actors/Player.cs
using System.Collections.Generic;
using Unity;
public class Player : Character
{
    public List<ISkill> Skills { get; } = new();

    public Player(string name = "Hero", int maxHp = 100, int atk = 15, int apMax = 6, int startAP = 0)
        : base(name, maxHp, atk, apMax, startAP)
    {
        Skills.Add(new HealSkill());
        Skills.Add(new HeavySlashSkill());
        Skills.Add(new MagicBulletSkill());
    }
}
