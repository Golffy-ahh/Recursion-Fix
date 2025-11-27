using UnityEngine;
// Example: put this on an empty GameObject
public class DemoStart : MonoBehaviour
{
    public EncounterManager encounter; // assign in Inspector

    void Start()
    {
        var player = new Player("Hero", 100, 15, 6, 0);
        var enemy  = new Enemy("Skeleton", 20, 10);
        encounter.StartEncounter(player, enemy);
    }
}


