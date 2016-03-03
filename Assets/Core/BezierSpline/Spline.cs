using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

[System.Serializable]
public class Spline : System.Object {

    public LinkedList<Point> controlPoints = new LinkedList<Point>();

    //public int points;

	// Constructor for a new spline
	public Spline (Point p0, Point p1,Point p2, Point p3) {
        controlPoints.AddFirst(p0);
        controlPoints.AddLast(p1);
        controlPoints.AddLast(p2);
        controlPoints.AddLast(p3);
        /*
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        points = 4;
        */
	}

    // Costructor for a spline in the 0,0 vector
    public Spline()
    {
        controlPoints.AddFirst(new Point(new Vector3(0, 0, 0), 1));
        controlPoints.AddLast(new Point(new Vector3(0, 0, 0), 1));
        controlPoints.AddLast(new Point(new Vector3(0, 0, 0), 1));
        controlPoints.AddLast(new Point(new Vector3(0, 0, 0), 1));
    }

    // Costructor for a spline with start and end point
    public Spline(Point p0, Point p3)
    {
        /*
        this.p0 = p0;
        Vector3 direct1 = p0.Position - p3.Position;
        p1 = new Point(p0.Position + 1 / 3 * direct1, 1);
        p2 = new Point(p0.Position + 2 / 3 * direct1, 1);
        this.p3 = p3;
        points = 4;
        */
        controlPoints.AddFirst(p0);
        Vector3 direct1 = p0.Position - p3.Position;
        controlPoints.AddLast(new Point(p0.Position + 1 / 3 * direct1, 1));
        controlPoints.AddLast(new Point(p0.Position + 2 / 3 * direct1, 1));
        controlPoints.AddLast(p3);
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
        for (int i = 0; i < controlPoints.Count; i++)
        {
            b = new Bernstein(i, controlPoints.Count - 1, t).calculate();
            top += b * controlPoints.ElementAt(i).Position * controlPoints.ElementAt(i).Weight;
            bot += b * controlPoints.ElementAt(i).Weight;
        }
        
        for (int i = 0; i < controlPoints.Count; i++)
        {
            b = new Bernstein(i, controlPoints.Count - 1, t).calculate();
            //bot += b * controlPoints.ElementAt(i).Weight;
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
            return (controlPoints.ElementAt(1).Position - controlPoints.First.Value.Position) / 2;
        }
        else if(i == 1)
        {
            return (controlPoints.Last.Value.Position - controlPoints.ElementAt(controlPoints.Count - 1).Position) / 2;
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

    /// <summary>
    /// Transform the splinesegment along the given vector
    /// </summary>
    /// <param name="">the vector which is used to translate</param>
    public void Transform(Vector3 transformVector)
    {
        for(int i = 0; i < controlPoints.Count; i++)
        {
            controlPoints.ElementAt(i).Position += transformVector;
        }
    }

    /// <summary>
    /// Returns the fucking point of the linked list, because its not public implemented, because its O(n). Mabey i should look for another data structure
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public Point GetControlPointAt(int i)
    {
        return controlPoints.ElementAt(i);
    }

    /// <summary>
    /// Elevates the degree of the splinesegment. Its not useful to use higher degree than 4, rather split the spline and connect them about the knot vectors
    /// </summary>
    public void ElevateDegree()
    {
        int n = controlPoints.Count;
        LinkedList<Point> higherDegreePoints = new LinkedList<Point>();
        Point lastPoint = controlPoints.Last.Value;
        higherDegreePoints.AddFirst(controlPoints.First.Value);
        for(int i = 1; i < n; i++)
        {
            Vector3 tempPoint = ((i / (n + 1f) * controlPoints.ElementAt(i - 1).Position) + ((1f - (i / (n + 1f))) * controlPoints.ElementAt(i).Position));
            higherDegreePoints.AddLast(new Point(tempPoint, 1));
        }
        higherDegreePoints.AddLast(lastPoint);
        controlPoints = higherDegreePoints;
    }

    override public String ToString()
    {
        String temp = "";
        for(int i = 0; i < controlPoints.Count; i++)
        {
            temp += i + ": " + controlPoints.ElementAt(i).ToString() + "\n";
        }
        return temp;
    }
}
