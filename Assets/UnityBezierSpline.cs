﻿using UnityEngine;
using System.Collections;

[System.Serializable]
class UnityBezierSpline : MonoBehaviour
{
    public BezierSpline bezierSpline = new BezierSpline(new Spline());
    public bool ShowSplineContolPoints = false;

    public uint DebugSplineRes = 1;
    public uint NumOfShownSplinesegments = 1;
    public bool ShowTangens = false;
    public bool ShowNormal = false;
    public bool ShowBall = false;

    private float pro = 0;

    // Use this for initialization
    void Start()
    {
        Spline spline1 = new Spline(new Point(new Vector3(0, 1, 0), 1), new Point(new Vector3(0, 2, 1), 1f), new Point(new Vector3(1, 4, 2), 1f), new Point(new Vector3(2, 4, 2), 1));
        //Spline spline2 = new Spline(new Point(new Vector3(2, 4, 2), 1), new Point(new Vector3(3, 4, 2), 1f), new Point(new Vector3(8, 4, 3), 1f), new Point(new Vector3(9, 5, 4), 1));
        Spline spline2 = new Spline(new Point(new Vector3(10, 4, 6), 1), new Point(new Vector3(10, 4, 5), 1f), new Point(new Vector3(10, 4, 3), 1f), new Point(new Vector3(10, 4, 2), 1));
        bezierSpline = new BezierSpline(spline1);
        bezierSpline.AddSplineAtFront(spline2);
        //bezierSpline.AddSplineAtFront(spline3);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // DebugDraws
    void OnDrawGizmos()
    {
        if (DebugSplineRes > 0)
        {
            Gizmos.color = Color.yellow;
            if (DebugSplineRes <= 0)
            {
                DebugSplineRes = 1;
            }
            else if (DebugSplineRes > 100)
            {
                DebugSplineRes = 100;
            }
            float steps = 1f / DebugSplineRes;
            for(int a = 0; a < bezierSpline.splineSegemnts.Count && a < NumOfShownSplinesegments; a++)
            {
                Gizmos.color = Color.yellow;
                for (float i = 0; i <= 1; i += steps)
                {
                    Gizmos.DrawSphere(bezierSpline.GetSplineSegmentAt(a).GetPointAt(i), 0.1f);
                }
                // Control points
                Gizmos.color = Color.red;
                if (ShowSplineContolPoints)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawSphere(bezierSpline.GetSplineSegmentAt(a).p0.Position, 0.1f);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(bezierSpline.GetSplineSegmentAt(a).p1.Position, 0.1f);
                    Gizmos.color = Color.gray;
                    Gizmos.DrawSphere(bezierSpline.GetSplineSegmentAt(a).p2.Position, 0.1f);
                    if (bezierSpline.GetSplineSegmentAt(a).p3 != null)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(bezierSpline.GetSplineSegmentAt(a).p3.Position, 0.2f);
                    }
                }
                // Tangent vectors
                Gizmos.color = Color.blue;
                if (ShowTangens)
                {                    
                    for (float i = 0; i <= 1; i += steps)
                    {
                        Vector3 pos = (bezierSpline.GetSplineSegmentAt(a).GetPointAt(i));
                        Gizmos.DrawLine(pos, pos + (bezierSpline.GetSplineSegmentAt(a).GetTangensAt(i, steps)));
                    }
                }
                // Normal vectors
                Gizmos.color = Color.green;
                if (ShowNormal)
                {                   
                    for (float i = 0; i <= 1; i += steps)
                    {
                        Vector3 pos = (bezierSpline.GetSplineSegmentAt(a).GetPointAt(i));
                        Gizmos.DrawLine(pos, pos + (bezierSpline.GetSplineSegmentAt(a).GetNormalAt(i, steps)));
                    }
                }
            }
            // Show the direction and speed of the splines
            if (ShowBall)
            {
                Gizmos.color = Color.green;
                pro += 0.001f;
                Vector3 pro2 = bezierSpline.GetPointAt(pro);
                Gizmos.DrawSphere(pro2, 0.2f);
                if(pro > NumOfShownSplinesegments || pro > bezierSpline.splineSegemnts.Count)
                {
                    pro = 0;
                }
            }
        }
    }
}
