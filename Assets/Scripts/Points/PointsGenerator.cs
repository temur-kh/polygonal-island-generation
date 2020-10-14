using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PointsGenerator
{
    public static List<Vector2> getPoints(Config conf)
    {
        List<Vector2> points = new List<Vector2>();
        int number = (conf.meshSize / conf.pointSpacing) * (conf.meshSize / conf.pointSpacing);
        for (int i = 0; i < number; i++)
        {
            points.Add(new Vector2(Random.Range(0, conf.meshSize), Random.Range(0, conf.meshSize)));
        }
        return points;
    }
}