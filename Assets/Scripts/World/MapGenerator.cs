using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Art for nodes")]
    public Sprite encounterSprite;
    public Sprite restSprite;
    public Sprite eventSprite;
    public Sprite bossSprite;

    readonly System.Random _rng = new();

    public enum NodeType { Encounter, Rest, Event, Boss }

    public List<MapNode> Generate(int tierIndex1to4)
    {
        var nodes = new List<MapNode>(3);

        bool isBossTier = tierIndex1to4 >= 4;

        if (!isBossTier)
        {
            // pool without boss
            var pool = new List<MapNode>
            {
                Make(NodeType.Encounter, "Encounter", "Fight!", encounterSprite),
                Make(NodeType.Rest,      "Rest",      "+30 HP", restSprite),
                Make(NodeType.Event,     "Event",     "50/50 Heal/DMG", eventSprite),
                Make(NodeType.Encounter, "Encounter", "Fight!", encounterSprite),
                Make(NodeType.Event,     "Event",     "50/50", eventSprite),
            };

            // pick 3
            while (nodes.Count < 3 && pool.Count > 0)
            {
                int r = _rng.Next(pool.Count);
                nodes.Add(pool[r]);
                pool.RemoveAt(r);
            }

            // guarantee at least one encounter
            if (nodes.Find(n => n.Type == NodeType.Encounter) == null)
                nodes[_rng.Next(nodes.Count)] = Make(NodeType.Encounter, "Encounter", "Fight!", encounterSprite);
        }
        else
        {
            // Tier 4: Boss only in the middle
            var left  = RandomSide();
            var right = RandomSide();

            nodes.Add(Make(NodeType.Boss, "Boss", "Final Test", bossSprite));
            nodes.Add(Make(NodeType.Boss, "Boss", "Final Test", bossSprite));
            nodes.Add(Make(NodeType.Boss, "Boss", "Final Test", bossSprite));
        }

        return nodes;

        MapNode RandomSide()
        {
            // pick from Encounter/Rest/Event only
            int r = _rng.Next(3);
            return r switch
            {
                0 => Make(NodeType.Encounter, "Encounter", "Fight!", encounterSprite),
                1 => Make(NodeType.Rest,      "Rest",      "+30 HP", restSprite),
                _ => Make(NodeType.Event,     "Event",     "50/50 Heal/DMG", eventSprite),
            };
        }

        static MapNode Make(NodeType t, string title, string desc, Sprite art)
        {
            return new MapNode { Type = t, Title = title, Description = desc, Image = art };
        }
    }
}
