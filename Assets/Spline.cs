using UnityEngine;
using System.Collections;

[System.Serializable]
public class Spline : System.Object {

    public Point p0;
    public Point p1;
    public Point p2;
    public Point p3;

    //public Point direct1;
    //public Point direct2;

    //public Point intersec = new Point(Vector3.zero, 0);

    public int points;

	// Constructor for a new spline
	public Spline (Point p0, Point p1,Point p2, Point p3) {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        points = 4;
	}

    public Spline()
    {
        p0 = new Point(new Vector3(0, 0, 0), 1);
        p1 = new Point(new Vector3(0, 0, 0), 1);
        p2 = new Point(new Vector3(0, 0, 0), 1);
        p3 = new Point(new Vector3(0, 0, 0), 1);
        points = 4;
    }

    /// <summary>
    /// Returns the point/vector at t for this spline
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Vector3 GetPointAt(float t)
    {
        Vector3 top = new Vector3(0, 0, 0);
        float bot = 0;
        float b = 0;
        for (int i = 0; i < points; i++)
        {
            if(i == 0)
            {
                b = new Bernstein(i, points-1, t).calculate();
                top += b * p0.Weight * p0.Position;
            }
            else if(i == 1)
            {
                b = new Bernstein(i, points - 1, t).calculate();
                top += b * p1.Weight * p1.Position;
            }
            else if(i == 2)
            {
                b = new Bernstein(i, points - 1, t).calculate();
                top += b * p2.Weight * p2.Position;
            }
            else if (i == 3)
            {
                b = new Bernstein(i, points - 1, t).calculate();
                top += b * p3.Weight * p3.Position;
            }

        }
        
        for (int i = 0; i < points; i++)
        {
            if (i == 0)
            {
                b = new Bernstein(i, points - 1, t).calculate();
                bot += b * p0.Weight;
            }
            else if (i == 1)
            {
                b = new Bernstein(i, points - 1, t).calculate();
                bot += b * p1.Weight;
            }
            else if (i == 2)
            {
                b = new Bernstein(i, points - 1, t).calculate();
                bot += b * p2.Weight;
            }
            else if (i == 3)
            {
                b = new Bernstein(i, points - 1, t).calculate();
                bot += b * p3.Weight;
            }
        }
        return (1 / bot) * top;
    }

    /// <summary>
    /// Returns the tangent vector of the spline at i
    /// </summary>
    /// <param name="i">The point on the spline, where the tangent vector should be calculated</param>
    /// <param name="res">The resolution, for the spline. The tangent is calculated with near by points of the spline, no real derivate is calculated</param>
    /// <returns></returns>
    public Vector3 GetTangensAt(float i, float res)
    {
        if(i < 0)
        {
            return (p1.Position - p0.Position) / 2;
        }
        else if(i == 1)
        {
            return (p3.Position - p2.Position) / 2;
        }
        else return GetPointAt(i + res) - GetPointAt(i);
    }

    /// <summary>
    /// Returns the normal vector of the spline at i
    /// </summary>
    /// <param name="i">The point on the spline, where the normal vector should be calculated</param>
    /// <param name="res">The resolution, for the spline. The tangent for the normaldirection is calculated with near by points of the spline, no real derivate is calculated</param>
    /// <returns></returns>
    public Vector3 GetNormalAt(float i, float res)
    {
        Vector3 midP = GetPointAt(i);
        Vector3 tang = GetTangensAt(i, res);
        Vector3 direc = new Vector3(-tang.y, tang.x, 0);
        return Vector3.Cross(midP.normalized, direc.normalized);
    }
}
