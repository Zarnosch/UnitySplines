using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

// Make sure, that all needed components are there
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class UnityBezierChunk : MonoBehaviour
{
    public BezierChunk chunk;
    private List<Vector3> metaVertices;
    private List<Vector2> metaUVs;
    private int[] metaTriangles;
    public Mesh MetaMesh;
    //public CombineInstance[] combine;
    public MeshFilter meshFilter;

    [Range(2, 2000)]
    public int Test = 1;

    [Range(2, 20)]
    public int Resolution = 5;
    private int prevResolution = 5;

    [Range(1, 5)]
    public int ShowPatchesX = 5;

    [Range(1, 5)]
    public int ShowPatchesZ = 5;

    [Range(0, 1337)]
    public int Seed = 21;
    private int prevSeed = 21;

    [Range(0, 90)]
    public float Steepness = 41;
    private float prevSteepness = 41;

    [Range(0, 90)]
    public int MaxOverhang = 41;
    private int prevMaxOverhang = 41;

    [Range(1, 0)]
    public float OverhangRatio = 0;
    private float prevOverhangRatio = 0;

    // Use this for initialization
    void Start()
    {
        chunk = new BezierChunk(Resolution, Seed, Steepness, MaxOverhang, OverhangRatio);
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = chunk.MetaMesh;

}

    // Update is called once per frame
    void Update()
    {
       
        if (prevSeed != Seed || prevSteepness != Steepness || prevResolution != Resolution || prevMaxOverhang != MaxOverhang || prevOverhangRatio != OverhangRatio)
        {
            chunk = new BezierChunk(Resolution, Seed, Steepness, MaxOverhang, OverhangRatio);
            meshFilter.mesh = chunk.MetaMesh;
            //chunk.CalculateMetaMesh();
            prevSeed = Seed;
            prevSteepness = Steepness;
            prevResolution = Resolution;
            prevMaxOverhang = MaxOverhang;
            prevOverhangRatio = OverhangRatio;
            chunk.AssignPatches();
        }
    }

    // DebugDraws
    void OnDrawGizmos()
    {
        for (int i = 0; i < ShowPatchesX; i++)
        {
            for (int j = 0; j < ShowPatchesZ; j++)
            {
                if (chunk.SurfacePatches[i, j] != null)
                {
                    // Control Points
                    Gizmos.color = Color.red;
                    for (int k = 0; k < 4; k++)
                    {
                        for (int l = 0; l < 4; l++)
                        {
                            if((k == 0 || k == 3) && (l == 0 || l == 3))
                            {
                                Gizmos.color = Color.cyan;
                                Gizmos.DrawSphere(chunk.SurfacePatches[i, j].BezierPoints[k, l].Position, 0.5f);
                            }
                            else
                            {
                                Gizmos.color = Color.red;
                                Gizmos.DrawSphere(chunk.SurfacePatches[i, j].BezierPoints[k, l].Position, 0.2f);
                            }
                            
                        }
                    }
                    //sample points
                    Gizmos.color = Color.yellow;
                    for (int k = 0; k < Resolution; k++)
                    {
                        for (int l = 0; l < Resolution; l++)
                        {
                            Gizmos.DrawSphere(chunk.SurfacePatches[i, j].GetPointAt(new Vector2((float)k/Resolution, (float)l/Resolution)), 0.4f);
                        }
                    }
                    // z- direction
                    Gizmos.color = Color.blue;
                    for (int k = 0; k < 4; k++)
                    {
                        for (int l = 1; l < 4; l++)
                        {
                            Gizmos.DrawLine(chunk.SurfacePatches[i, j].BezierPoints[k, l].Position, chunk.SurfacePatches[i, j].BezierPoints[k, l - 1].Position);
                        }
                    }
                    // x- direction
                    Gizmos.color = Color.green;
                    for (int k = 1; k < 4; k++)
                    {
                        for (int l = 0; l < 4; l++)
                        {
                            Gizmos.DrawLine(chunk.SurfacePatches[i, j].BezierPoints[k, l].Position, chunk.SurfacePatches[i, j].BezierPoints[k - 1, l].Position);
                        }
                    }
                }
            }
        }
    /*
    for (int t = 0; t < Test; t++)
    {
        Gizmos.color = Color.black;
        Gizmos.DrawCube(metaVertices[t], new Vector3(0.5f, 0.9f, 0.5f));
    }
    */
    
    if(MetaMesh != null)
        {
            for (int t = 0; t < Test; t++)
            {
                /*
                for (int u = 0; u < Test; u++)
                {
                    for (int l = 0; l < 2; l++)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawCube(MetaMesh.vertices[l], new Vector3(0.5f, 0.9f, 0.5f));
                    }
                }    
                */
                Gizmos.color = Color.black;
                Gizmos.DrawCube(MetaMesh.vertices[t], new Vector3(0.5f, 0.9f, 0.5f));
            }
        }        
    }
}

