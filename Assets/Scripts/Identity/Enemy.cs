// Actors/Enemy.cs
using System.Collections.Generic;

public class Enemy : Character
{
    //Add enemy skills later 
    public Enemy(string name = "Skeleton", int maxHp = 20, int atk = 10)
        : base(name, maxHp, atk) { }
}