using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class Point : System.Object
{
    public Vector3 Position = new Vector3(0, 0, 0);
    public float Weight = 0;

    public Point (Vector3 position, float weight)
    {
        this.Position = position;
        this.Weight = weight;
    }

    public Point()
    {
        this.Position = Vector3.zero;
        this.Weight = 1;
    }

    public static Point operator - (Point p0, Point p1)
    {
        Vector3 temp = p0.Position - p1.Position;
        return new Point(temp, (p0.Weight + p1.Weight) / 2);
    }

    public static Point operator +(Point p0, Point p1)
    {
        Vector3 temp = p0.Position + p1.Position;
        return new Point(temp, (p0.Weight + p1.Weight) / 2);
    }

    public static Point operator *(Point p0, Point p1)
    {
        Vector3 temp = new Vector3(p0.Position.x * p1.Position.x, p0.Position.y * p1.Position.y, p0.Position.z * p1.Position.z);
        return new Point(temp, (p0.Weight * p1.Weight));
    }

    public static Point operator *(Point p0, float p1)
    {
        Vector3 temp = new Vector3(p0.Position.x * p1, p0.Position.y * p1, p0.Position.z * p1);
        return new Point(temp, (p0.Weight * p1));
    }


    public static Point intersect(Point p_0, Point v0, Point p_1, Point v1)
    {
        Vector2 p0 = new Vector2(p_0.Position.x, p_0.Position.z);
        Vector2 p1 = new Vector2(p_0.Position.x - v0.Position.x, p_0.Position.z - v0.Position.z);
        Vector2 p2 = new Vector2(p_1.Position.x, p_1.Position.z);
        Vector2 p3 = new Vector2(p_1.Position.x - v1.Position.x, p_1.Position.z - v1.Position.z);
        float A1 = p1.y - p0.y;
        float B1 = p0.x - p1.x;
        float C1 = A1 * p0.x + B1 * p0.y;
        float A2 = p3.y - p2.y;
        float B2 = p2.x - p3.x;
        float C2 = A2 * p2.x + B2 * p2.y;
        float denominator = A1 * B2 - A2 * B1;

        return new Point(new Vector3((B2 * C1 - B1 * C2) / denominator, 0, (A1 * C2 - A2 * C1) / denominator), 0.5f);
    }

    override public String ToString()
    {
        String temp = Position.ToString() + " Weight: " + Weight;
        return temp;
    }
}

