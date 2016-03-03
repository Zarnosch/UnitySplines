using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

public class SurfacePatch : System.Object
{
    public Point[,] BezierPoints = new Point[4, 4];
    /// <summary>
    /// Constructor for a Bezier surface patch with regular points
    /// </summary>
    public SurfacePatch ()
    {
        BezierPoints[0, 0] = new Point(new Vector3(0, 0, 0), 1);
        BezierPoints[1, 0] = new Point(new Vector3(1, 0, 0), 1);
        BezierPoints[2, 0] = new Point(new Vector3(2, 0, 0), 1);
        BezierPoints[3, 0] = new Point(new Vector3(3, 0, 0), 1);
        BezierPoints[0, 1] = new Point(new Vector3(0, 0, 1), 1);
        BezierPoints[1, 1] = new Point(new Vector3(1, 1, 1), 1);
        BezierPoints[2, 1] = new Point(new Vector3(2, 1, 1), 1);
        BezierPoints[3, 1] = new Point(new Vector3(3, 0, 1), 1);
        BezierPoints[0, 2] = new Point(new Vector3(0, 0, 2), 1);
        BezierPoints[1, 2] = new Point(new Vector3(1, 1, 2), 1);
        BezierPoints[2, 2] = new Point(new Vector3(2, 1, 2), 1);
        BezierPoints[3, 2] = new Point(new Vector3(3, 0, 2), 1);
        BezierPoints[0, 3] = new Point(new Vector3(0, 0, 3), 1);
        BezierPoints[1, 3] = new Point(new Vector3(1, 0, 3), 1);
        BezierPoints[2, 3] = new Point(new Vector3(2, 0, 3), 1);
        BezierPoints[3, 3] = new Point(new Vector3(3, 0, 3), 1);
    }

    /// <summary>
    /// Constructor for a Bezier surface patch with new points
    /// </summary>
    public SurfacePatch(Point[,] points)
    {
        BezierPoints[0, 0] = points[0, 0];
        BezierPoints[1, 0] = points[1, 0];
        BezierPoints[2, 0] = points[2, 0];
        BezierPoints[3, 0] = points[3, 0];
        BezierPoints[0, 1] = points[0, 1];
        BezierPoints[1, 1] = points[1, 1];
        BezierPoints[2, 1] = points[2, 1];
        BezierPoints[3, 1] = points[3, 1];
        BezierPoints[0, 2] = points[0, 2];
        BezierPoints[1, 2] = points[1, 2];
        BezierPoints[2, 2] = points[2, 2];
        BezierPoints[3, 2] = points[3, 2];
        BezierPoints[0, 3] = points[0, 3];
        BezierPoints[1, 3] = points[1, 3];
        BezierPoints[2, 3] = points[2, 3];
        BezierPoints[3, 3] = points[3, 3];
    }

    /// <summary>
    /// Returns the point/vector at [u,v] of this surface patch
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Vector3 GetPointAt(Vector2 uv)
    {
        Vector3 top = new Vector3(0, 0, 0);
        float bot = 0;
        float b1 = 0;
        float b2 = 0;
        for (int u = 0; u < 4; u++)
        {
            for (int v = 0; v < 4; v++)
            {
                b1 = new Bernstein(u, 3, uv.x).calculate();
                b2 = new Bernstein(v, 3, uv.y).calculate();
                top += b1 * b2 * BezierPoints[u, v].Position * BezierPoints[u, v].Weight;
                bot += b1 * b2 * BezierPoints[u, v].Weight;
            }
        }
        return (1 / bot) * top;
    }

    public Point[] GetDownC0Points()
    {
        Point[] temp = new Point[4];
        temp[0] = BezierPoints[0, 0]; temp[1] = BezierPoints[1, 0]; temp[2] = BezierPoints[2, 0]; temp[3] = BezierPoints[3, 0];
        return temp;
    }

    public override string ToString()
    {
        string temp = "";
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (BezierPoints[i, j] == null)
                {
                    temp += "#";
                }
                else
                {
                    temp += "+";
                }
            }
            temp += "\n";
        }
        return temp;
    }

}
