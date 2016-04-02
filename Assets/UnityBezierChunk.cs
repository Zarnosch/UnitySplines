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
    public Vector3 AverageMidPoint;

    [Range(2, 2000)]
    public int Test = 0;

    [Range(2, 20)]
    public int Resolution = 5;
    private int prevResolution = 5;

    [Range(0, 5)]
    public int ShowPatchesX = 0;

    [Range(0, 5)]
    public int ShowPatchesZ = 0;

    [Range(0, 1337)]
    public int Seed = 21;
    private int prevSeed = 21;

    [Range(0, 90)]
    public float Steepness = 10;
    private float prevSteepness = 10;

    [Range(0, 90)]
    public int MaxOverhang = 41;
    private int prevMaxOverhang = 41;

    [Range(1, 0)]
    public float OverhangRatio = 0;
    private float prevOverhangRatio = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (prevSeed != Seed || prevSteepness != Steepness || prevResolution != Resolution || prevMaxOverhang != MaxOverhang || prevOverhangRatio != OverhangRatio)
        {
            // assign the changes to the chunk
            chunk.Resolution = Resolution;
            chunk.Steepness = Steepness;
            chunk.Seed = Seed;
            chunk.MaxOverhang = MaxOverhang;
            chunk.OverhangRatio = OverhangRatio;
            // rebuild chunk with the new settings
            chunk.RebuildChunkWithNeighbourUpdate();
            AverageMidPoint = chunk.AverageMidPoint;     
            // save changes
            prevSeed = Seed;
            prevSteepness = Steepness;
            prevResolution = Resolution;
            prevMaxOverhang = MaxOverhang;
            prevOverhangRatio = OverhangRatio;
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
                    // Control Points (only for surface patches)
                    Gizmos.color = Color.red;
                    for (int k = 0; k < 4; k++)
                    {
                        for (int l = 0; l < 4; l++)
                        {
                            if((k == 0 || k == 3) && (l == 0 || l == 3))
                            {
                                Gizmos.color = Color.cyan;
                                Gizmos.DrawSphere(chunk.SurfacePatches[i, j].BezierPoints[k, l].Position, 1f);
                            }
                            else
                            {
                                Gizmos.color = Color.red;
                                Gizmos.DrawSphere(chunk.SurfacePatches[i, j].BezierPoints[k, l].Position, 0.8f);
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
        
        if(MetaMesh != null)
        {
            for (int t = 0; t < Test; t++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(MetaMesh.vertices[t], MetaMesh.vertices[t] + MetaMesh.normals[t]*10);
            }
        }
        
    }
}

