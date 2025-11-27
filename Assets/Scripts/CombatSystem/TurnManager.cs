// Combat/TurnManager.cs
public enum Turn { Player, Enemy }

public class TurnManager
{
    public Turn Current { get; private set; } = Turn.Player;
    public void Reset(Turn start = Turn.Player) => Current = start;
    public void Next() => Current = (Current == Turn.Player) ? Turn.Enemy : Turn.Player;
}
