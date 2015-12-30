using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainS : MonoBehaviour {
    Spline testSpline = new Spline (new Point(new Vector3(0, 1, 0), 1), new Point(new Vector3(0, 2, 1), 1f), new Point(new Vector3(1, 4, 2), 1f), new Point(new Vector3(2, 4, 2), 1));
    Spline testSpline2 = new Spline(new Point(new Vector3(6, 4, 4), 1), new Point(new Vector3(6, 2, 6.5f), 1f), new Point(new Vector3(6, 1, 8), 1f), new Point(new Vector3(8, 1, 8), 1));
    Spline testSpline3 = new Spline(new Point(new Vector3(16, 4, 14), 1), new Point(new Vector3(16, 2, 16.5f), 1f), new Point(new Vector3(16, 1, 18), 1f), new Point(new Vector3(18, 1, 18), 1));
    public Vector3 p0 = Vector3.zero;
    List<Vector3> testMeshVert = new List<Vector3>();
    // Use this for initialization
    public float r = 0.5f;
    int res = 10;
    float pro = 0f;

    void Start () {
        // p0 = testSpline.GetPointAt(0.5f);
        testSpline.addBackSplineC2(testSpline2);
        testSpline2.addBackSplineC2(testSpline3);
        Mesh testMesh = new Mesh();
        
        for(float i = 0; i <= 1; i+= 0.1f)
        {
            Vector3 midP = testSpline.GetPointAt(i);
            Vector3 direc = midP - testSpline.GetPointAt(i+0.1f);
            Vector3 norm = Vector3.Cross(midP, direc);
            for (int j = 0; j < res; res++)
            {
                
            }
            //testMeshVert.Add();
        }
        
        


        //testMesh.SetVertices();
    }
	
	// Update is called once per frame
	void Update () {
        testSpline.GetPointOverAll(1);
        testSpline2.GetPointOverAll(1);
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.yellow;
        
        for (float i = 0; i < 1; i += 0.1f)
        {
            if(i == 0 || i == 1)
            {
                Gizmos.DrawSphere(testSpline.GetPointAt(i), 0.1f);
                Gizmos.DrawSphere(testSpline2.GetPointAt(i), 0.1f);
                Gizmos.DrawSphere(testSpline3.GetPointAt(i), 0.1f);
                if (testSpline.connectBack != null)
                {
                    Gizmos.DrawSphere(testSpline.connectBack.GetPointAt(i), 0.1f);
                }
                if (testSpline2.connectBack != null)
                {
                    Gizmos.DrawSphere(testSpline2.connectBack.GetPointAt(i), 0.1f);
                }
            }
            else
            {
                Gizmos.DrawSphere(testSpline.GetPointAt(i), 0.05f);
                Gizmos.DrawSphere(testSpline2.GetPointAt(i), 0.05f);
                Gizmos.DrawSphere(testSpline3.GetPointAt(i), 0.05f);
                if (testSpline.connectBack != null)
                {
                    Gizmos.DrawSphere(testSpline.connectBack.GetPointAt(i), 0.05f);
                }
                if (testSpline2.connectBack != null)
                {
                    Gizmos.DrawSphere(testSpline2.connectBack.GetPointAt(i), 0.05f);
                }
            }
            
        }
        
        //Gizmos.DrawSphere(p0, 0.05f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(testSpline.p0.Position, 0.07f);
        Gizmos.DrawSphere(testSpline.p1.Position, 0.07f);
        Gizmos.DrawSphere(testSpline.p2.Position, 0.07f);
        Gizmos.DrawSphere(testSpline.p3.Position, 0.07f);
        Gizmos.DrawSphere(testSpline2.p0.Position, 0.07f);
        Gizmos.DrawSphere(testSpline2.p1.Position, 0.07f);
        Gizmos.DrawSphere(testSpline2.p2.Position, 0.07f);
        Gizmos.DrawSphere(testSpline2.p3.Position, 0.07f);
        Gizmos.DrawSphere(testSpline3.p0.Position, 0.07f);
        Gizmos.DrawSphere(testSpline3.p1.Position, 0.07f);
        Gizmos.DrawSphere(testSpline3.p2.Position, 0.07f);
        Gizmos.DrawSphere(testSpline3.p3.Position, 0.07f);
        if (testSpline.connectBack != null)
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawSphere(testSpline.connectBack.p0.Position, 0.1f);
            Gizmos.DrawSphere(testSpline.connectBack.p3.Position, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(testSpline.connectBack.p1.Position, 0.07f);
            Gizmos.DrawSphere(testSpline.connectBack.p2.Position, 0.07f);
            
        }


        Gizmos.color = Color.black;
        //Gizmos.DrawSphere(testSpline.intersec.Position, 0.1f);

        //Debug.Log(Time.fixedTime);
        
        for (float i = 0; i <= 1; i += 0.1f)
        {
            Vector3 midP = testSpline.GetPointAt(i);
            Vector3 tang = testSpline.GetPointAt(i + 0.1f) - midP;
            Vector3 direc = new Vector3(-tang.y, tang.x, 0);
            Vector3 norm = Vector3.Cross(midP.normalized, direc.normalized);
            Vector3 norm2 = Vector3.Cross(direc.normalized, midP.normalized);
            //Vector3 test = Vector3.RotateTowards(norm, norm2, (Mathf.PI / 4 * Time.fixedTime ) % 2* Mathf.PI, 1);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(midP, midP + tang);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(midP, midP + direc);

            Gizmos.color = Color.black;

            for(int j = 0; j < 360; j++)
            {
                float rad1 = Mathf.Deg2Rad * j;
                float rad2 = Mathf.Deg2Rad * (j+1)%360;
                Vector3 test = Vector3.RotateTowards(norm, norm2, rad1, 1);
                Vector3 test2 = Vector3.RotateTowards(norm, norm2, rad2, 1);
                Gizmos.DrawLine(midP + test, midP + test2);
            }            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(midP + norm.normalized * r, 0.05f);
            Gizmos.DrawSphere(midP + norm2.normalized * r, 0.05f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(midP, midP + norm.normalized * r);
            Gizmos.DrawLine(midP, midP + norm2.normalized * r);
        }

        
            //Gizmos.DrawLine(testSpline.p2.Position, testSpline.p2.Position + testSpline.direct1.Position);
        Gizmos.color = Color.green;
        
        pro += 0.001f;
        Vector3 pro2 = testSpline.GetPointOverAll(pro);
        Gizmos.DrawSphere(pro2, 0.2f);
    }
}
