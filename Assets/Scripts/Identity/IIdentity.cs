using Unity;
// Actors/IIdentity.cs
public interface IIdentity
{
    string Name { get; }
    int HP { get; set; }
    int MaxHP { get; }
    int AP { get; set; }
    int APMax { get; }
    int ATK { get; }

    void Heal(int amount);
    void TakeDamage(int amount);
    bool SpendAP(int cost);
}


