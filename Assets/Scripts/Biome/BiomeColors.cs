using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BiomeColors
{   
    public static Dictionary<NodeType, Color> colors = new Dictionary<NodeType, Color>
    {
        { NodeType.Beach, new Color(248f / 255f, 240f / 255f, 164f / 255f) },
        { NodeType.FreshWater, new Color(29f / 255f, 162f / 255f, 216f / 255f) },
        { NodeType.SaltWater, new Color(6f / 255f, 66f / 255f, 115f / 255f) },

        { NodeType.TropicalRainForest, new Color(51f / 255f, 119f / 255f, 85f / 255f) },
        { NodeType.TropicalSeasonalForest, new Color(85f / 255f, 153f / 255f, 68f / 255f) },
        { NodeType.SubtropicalDesert, new Color(210f / 255f, 185f / 255f, 139f / 255f) },

        { NodeType.TemperateRainForest, new Color(68f / 255f, 136f / 255f, 85f / 255f) },
        { NodeType.TemperateDeciduousForest, new Color(103f / 255f, 148f / 255f, 89f / 255f) },
        { NodeType.Grassland, new Color(136f / 255f, 170f / 255f, 85f / 255f) },

        { NodeType.Taiga, new Color(153f / 255f, 170f / 255f, 119f / 255f) },
        { NodeType.Shrubland, new Color(136f / 255f, 153f / 255f, 119f / 255f) },
        { NodeType.TemperateDesert, new Color(201f / 255f, 210f / 255f, 155f / 255f) },

        { NodeType.Snow, new Color(255f / 255f, 255f / 255f, 255f / 255f) },
        { NodeType.Tundra, new Color(187f / 255f, 187f / 255f, 170f / 255f) },
        { NodeType.Bare, new Color(136f / 255f, 136f / 255f, 136f / 255f) },
        { NodeType.Scorched, new Color(85f / 255f, 85f / 255f, 85f / 255f) },

        { NodeType.Error,  Color.red }
    };
    
}