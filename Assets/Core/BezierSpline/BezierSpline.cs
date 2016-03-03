using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;


class BezierSpline : System.Object
{
    public LinkedList<Spline> splineSegemnts = new LinkedList<Spline>();

    /// <summary>
    /// Initalize a BezierSpline which can hold multiple spline segments and checks for c0, c1 and c2 continutity
    /// </summary>
    /// <param name="spline">the initial spline</param>
    public BezierSpline(Spline spline)
    {
        splineSegemnts.AddFirst(spline);
    }
    
    /// <summary>
    /// Adds a spline in front of this bezier spline. So the last control point of the new spline will be connected with the 
    /// first control point of the first bezier spline-segment. If the connection isn´t c2 continuous, a new spline segment is created
    /// in a way, everything is c2 continuous
    /// </summary>
    /// <param name="spline">the spline which should be added to the front of the bezier spline segment</param>
    /// <returns></returns>
    public void AddSplineAtFront(Spline spline)
    {
        if(CheckC2(splineSegemnts.First.Value, spline))
        {
            splineSegemnts.AddFirst(spline);
        }
        else
        {
            Point pos1 = spline.controlPoints.Last.Value;
            Point pos2 = splineSegemnts.First.Value.controlPoints.First.Value;
            Point direct1 = pos1 - spline.GetControlPointAt(spline.controlPoints.Count -1);
            Point direct2 = splineSegemnts.First.Value.controlPoints.First.Value - splineSegemnts.First.Value.GetControlPointAt(1);
            Spline _spline = new Spline(pos1, pos1 + direct1, pos2 + direct2, pos2);
            splineSegemnts.AddFirst(_spline);
            splineSegemnts.AddFirst(spline);
            Debug.Log("Created new Spline at front");
        }
    }

    /// <summary>
    /// Adds a spline at the back of this bezier spline. So the first control point of the new spline will be connected with the 
    /// last control point of the first bezier spline-segment. If the connection isn´t c2 continuous, a new spline segment is created
    /// in a way, everything is c2 continuous
    /// </summary>
    /// <param name="spline">the spline which should be added at the back of the bezier spline segment</param>
    /// <returns></returns>
    public void AddSplineAtBack(Spline spline)
    {
        if (CheckC2(spline, splineSegemnts.Last.Value))
        {
            splineSegemnts.AddLast(spline);
        }
        else
        {
            Point pos2 = spline.controlPoints.First.Value;
            Point pos1 = splineSegemnts.Last.Value.controlPoints.Last.Value;
            Point direct2 = spline.controlPoints.First.Value - spline.GetControlPointAt(1);
            Point direct1 = splineSegemnts.Last.Value.controlPoints.Last.Value - splineSegemnts.Last.Value.GetControlPointAt(splineSegemnts.Last.Value.controlPoints.Count -1);
            Spline _spline = new Spline(pos1, pos1 + direct1, pos2 + direct2, pos2);
            splineSegemnts.AddLast(_spline);
            splineSegemnts.AddLast(spline);
            Debug.Log("Created new Spline at back");
        }
    }

    /// <summary>
    /// Adds a new spline segment in front of the bezier spline depending on the new point. creates everything in c2 continuous behaviour
    /// </summary>
    /// <param name="point">The new point which is the front point of the new spline segment</param>
    public void AddPointAtFront(Point point)
    {
        Point pos1 = point;
        Point pos2 = splineSegemnts.First.Value.controlPoints.First.Value;
        Point direct2 = pos2 - splineSegemnts.First.Value.GetControlPointAt(1);
        Point direct1 = pos2 + direct2 - ((pos2 + direct2) - pos1) * 0.5f;
        direct1.Weight = 1;
        Spline _spline = new Spline(pos1, direct1, pos2 + direct2, pos2);
        splineSegemnts.AddFirst(_spline);
        Debug.Log("Created new Spline at front with Point");
    }

    /// <summary>
    /// Checks if 2 spline segments are c0 continuous
    /// </summary>
    /// <param name="_firstS">The first spline, so it will be tested with the last point of this spline</param>
    /// <param name="_secondS">The second spline, so it will be tested with the first point of this spline</param>
    /// <returns></returns>
    public bool CheckC0(Spline _firstS, Spline _secondS)
    {
        if (_firstS.controlPoints.Last.Value.Position == _secondS.controlPoints.First.Value.Position)
        {
            return true;
        }
        else return false;
    }

    /// <summary>
    /// Checks if 2 spline segments are c1 continuous
    /// </summary>
    /// <param name="_firstS">The first spline, so it will be tested with the last 2 points of this spline</param>
    /// <param name="_secondS">The second spline, so it will be tested with the second point of this spline</param>
    /// <returns></returns>
    public bool CheckC1(Spline _firstS, Spline _secondS)
    {
        if(CheckC0(_firstS, _secondS))
        {
            if (Vector3.Normalize(_firstS.GetControlPointAt(_firstS.controlPoints.Count - 1).Position - _firstS.controlPoints.Last.Value.Position) == Vector3.Normalize(_secondS.controlPoints.First.Value.Position - _secondS.GetControlPointAt(1).Position))
            {
                return true;
            }
            else return false;
        }
        else return false;
    }

    /// <summary>
    /// Checks if 2 spline segments are c2 continuous
    /// </summary>
    /// <param name="_firstS">The first spline, so it will be tested with the last 2 points of this spline</param>
    /// <param name="_secondS">The second spline, so it will be tested with the second point of this spline</param>
    /// <returns></returns>
    public bool CheckC2(Spline _firstS, Spline _secondS)
    {
        if (CheckC1(_firstS, _secondS))
        {
            Point pos1 = _firstS.GetControlPointAt(_firstS.controlPoints.Count -1);
            Point pos2 = _secondS.GetControlPointAt(1);
            Point direct1 = pos1 - _firstS.GetControlPointAt(2);
            Point direct2 = pos2 - _secondS.GetControlPointAt(2);

            Point intersec = Point.intersect(pos1, direct1, pos2, direct2);
            if (intersec.Position == new Vector3(0, 0, 0))
            {
                return false;
            }
            else return true;
        }
        else return false;       
    }

    /// <summary>
    /// Returns a Vector3, which is the point on the bezier spline.
    /// T should be between 0 and "number of spline-segments)
    /// </summary>
    /// <param name="t">parameter, when we want to get the point on the spline</param>
    /// <returns></returns>
    public Vector3 GetPointAt(float t)
    {
        return GetSplineSegmentAt(t).GetPointAt(t % 1);
    }


    /// <summary>
    /// Returns the spline segment which would be accsessed with parameter t
    /// </summary>
    /// <param name="t">T should be used between 0 and the maximal number of spline segments in this bezier spline</param>
    /// <returns></returns>
    public Spline GetSplineSegmentAt(float t)
    {
        int splineSegmentsCount = splineSegemnts.Count;
        if (t < 0)
        {
            Debug.Log("Wrong Point acess with t = " + t);
            return splineSegemnts.ElementAt(0);
        }
        else
        {
            if (t > splineSegmentsCount)
            {
                t %= splineSegmentsCount;
            }
            int splineSegmentNum = (int)t;
            return splineSegemnts.ElementAt(splineSegmentNum);
        }
    }

    /// <summary>
    /// Returns the tangent vector of the spline at i
    /// </summary>
    /// <param name="i">The point on the spline, where the tangent vector should be calculated</param>
    /// <param name="res">The resolution, for the spline. The tangent is calculated with near by points of the spline, no real derivate is calculated</param>
    /// <returns></returns>
    public Vector3 GetTangensAt(float i, float res)
    {
        return GetSplineSegmentAt(i).GetTangensAt(i % 1, res);
    }

    /// <summary>
    /// Returns the normal vector of the spline at i
    /// </summary>
    /// <param name="i">The point on the spline, where the normal vector should be calculated</param>
    /// <param name="res">The resolution, for the spline. The tangent for the normaldirection is calculated with near by points of the spline, no real derivate is calculated</param>
    /// <returns></returns>
    public Vector3 GetNormalAt(float i, float res)
    {
        return GetSplineSegmentAt(i).GetNormalAt(i % 1, res);
    }
}

