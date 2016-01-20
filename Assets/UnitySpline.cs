using UnityEngine;
using System.Collections;

[System.Serializable]
public class UnitySpline : MonoBehaviour
{

    public Spline spline = new Spline();

    public bool ShowSplineContolPoints = false;

    public uint DebugSplineRes = 1;
    public bool ShowTangens = false;
    public bool ShowNormal = false;
    //public bool ShowZyliner = false;
    //public float ZylinderRadius = 1.0f;

    // Use this for initialization
    void Start()
    {
        spline = new Spline(new Point(new Vector3(0, 1, 0), 1), new Point(new Vector3(0, 2, 1), 1f), new Point(new Vector3(1, 4, 2), 1f), new Point(new Vector3(2, 4, 2), 1));
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
            if(DebugSplineRes <= 0)
            {
                DebugSplineRes = 1;
            }
            else if(DebugSplineRes > 100)
            {
                DebugSplineRes = 100;
            }
            float steps = 1f / DebugSplineRes;
            for (float i = 0; i <= 1; i += steps)
            {
                Gizmos.DrawSphere(spline.GetPointAt(i), 0.1f);
            }

            if (ShowSplineContolPoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(spline.p0.Position, 0.1f);
                Gizmos.DrawSphere(spline.p1.Position, 0.1f);
                Gizmos.DrawSphere(spline.p2.Position, 0.1f);
                if (spline.p3 != null)
                {
                    Gizmos.DrawSphere(spline.p3.Position, 0.1f);
                }
            }
            if (ShowTangens)
            {
                Gizmos.color = Color.blue;
                for (float i = 0; i <= 1; i += steps)
                {
                    Vector3 pos = spline.GetPointAt(i);
                    Gizmos.DrawLine(pos, pos + spline.GetTangensAt(i, steps));
                }
            }
            if (ShowNormal)
            {
                Gizmos.color = Color.green;
                for (float i = 0; i <= 1; i += steps)
                {
                    Vector3 pos = spline.GetPointAt(i);
                    Gizmos.DrawLine(pos, pos + spline.GetNormalAt(i, steps));
                }
            }
        }
    }
}
