using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;


public class SurfacePatchMesh : System.Object
{
    public SurfacePatch CoreSurfacePatch = new SurfacePatch();
    public Vector3[] newVertices;
    public Vector2[] newUV;
    public int[] newTriangles;
    public int Resolution = 4;
    public Mesh SufacePatchMesh;

    public SurfacePatchMesh(SurfacePatch bezierPoints, int resolution)
    {
        CoreSurfacePatch = bezierPoints;
        Resolution = resolution;
        rebuild();
        //ZNoise test = new ZNoise(EBiom.Flat);
        //test.calculatePoints();
        //Debug.Log(test.ToString());
    }

    /// <summary>
    /// builds the mesh again, with the assinged resolution
    /// </summary>
    public void rebuild()
    {
        newVertices = new Vector3[(Resolution + 1) * (Resolution + 1)];
        newUV = new Vector2[(Resolution + 1) * (Resolution + 1)];
        newTriangles = new int[Resolution * (Resolution * 6)];
        int countu = 0;
        int countv = 0;
        // create the vertices
        for (int i = 0; i <= Resolution; i++)
        {
            for (int j = 0; j <= Resolution; j++)
            {
                //Debug.Log("I: " + i + " J: " + j + " Index: " + ((countu * Resolution) + countv) + " SurfacePoint: " + CoreSurfacePatch.GetPointAt(new Vector2((float)i/Resolution, (float)j/Resolution)));
                newVertices[(countu * (Resolution + 1)) + countv] = CoreSurfacePatch.GetPointAt(new Vector2((float)i / Resolution, (float)j / Resolution));
                newUV[((Resolution + 1) * countu) + countv] = new Vector2(countu, countv);
                countv++;
            }
            countv = 0;
            countu++;
        }
        countu = 0;
        int triCount = 0;
        // create the triangles
        for (int i = 0; i < newTriangles.Length; i += 6)
        {
            if ((countu + 1) % (Resolution + 1) == 0)
            {
                countu++;
            }
            newTriangles[i] = countu;
            newTriangles[i + 1] = countu + 1;
            newTriangles[i + 2] = countu + Resolution + 1;
            newTriangles[i + 3] = countu + 1;
            newTriangles[i + 4] = countu + Resolution + 2;
            newTriangles[i + 5] = countu + Resolution + 1;
            triCount += 2;
            countu++;
        }
        //Debug.Log(newVertices[0].ToString());
    }


    public List<Vector3> GetVertices()
    {
        List<Vector3> temp = new List<Vector3>();
        for(int i = 0; i < newVertices.Length; i++)
        {
            temp.Add(newVertices[i]);
        }
        return temp;
    }

    public List<Vector3> GetUVIndices()
    {
        List<Vector3> temp = new List<Vector3>();
        for (int i = 0; i < newUV.Length; i++)
        {
            temp.Add(newUV[i]);
        }
        return temp;
    }
}

