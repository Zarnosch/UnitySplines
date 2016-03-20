using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;


public class BezierChunk : System.Object
{
    public Vector2i Positionkey {get; set; }
    public Vector2 Position { get; set; }
    public int PointAmount { get; set; }
    public int PatchAmount { get; set; }
    public UnityBezierChunk Up { get; set; }
    public UnityBezierChunk Down { get; set; }
    public UnityBezierChunk Left { get; set; }
    public UnityBezierChunk Right { get; set; }
    public UnityBezierChunk In { get; set; }
    public UnityBezierChunk Out { get; set; }
    public SurfacePatch[,] SurfacePatches { get; set; }
    public ZNoise ChunkNoise { get; set; }
    public SurfacePatchMesh[,] Mesh { get; set; }
    public int Resolution { get; set; }
    public int Seed { get; set; }
    public float Steepness { get; set; }
    public float MaxOverhang { get; set; }
    public float OverhangRatio { get; set; }
    public Mesh MetaMesh;
    private List<Vector3> metaVertices;
    private List<Vector2> metaUVs;
    private int[] metaTriangles;

    public BezierChunk(int resolution, int seed, float steepness, float maxOverhang, float overhangRatio)
    {
        Resolution = resolution;
        Positionkey = new Vector2i(0, 0);
        PointAmount = 16;
        Seed = seed;
        Steepness = steepness;
        MaxOverhang = maxOverhang;
        OverhangRatio = overhangRatio;
        ChunkNoise = new ZNoise(EBiom.Flat, Seed, Steepness, maxOverhang, overhangRatio);
        ChunkNoise.SizeToGenerate = PointAmount;
        ChunkNoise.calculatePoints();
        PatchAmount = PointAmount / 3;
        SurfacePatches = new SurfacePatch[PatchAmount, PatchAmount];
        AssignPatches();
        CalculateMetaMesh();
        //Mesh = new SurfacePatchMesh[PatchAmount, PatchAmount];
    }

    public void AssignPatches()
    {
        for (int x = 0; x < PatchAmount; x++)
        {
            for (int z = 0; z < PatchAmount; z++)
            {
                SurfacePatches[x, z] = new SurfacePatch();
                for (int x2 = 0; x2 < 4; x2++)
                {
                    for (int z2 = 0; z2 < 4; z2++)
                    {
                        SurfacePatches[x, z].BezierPoints[x2,z2] = ChunkNoise.calculatedPoints[(x*3)+x2, (z*3)+z2];
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculates a mesh for each Surface Patch
    /// </summary>
    public void CalculateMeshes()
    {
        for (int i = 0; i < PatchAmount; i++)
        {
            for (int j = 0; j < PatchAmount; j++)
            {
                Mesh[i, j] = new SurfacePatchMesh(SurfacePatches[i, j], Resolution);
                Mesh[i, j].rebuild();
            }
        }
    }

    /// <summary>
    /// Calculates one big Metamesh out of all BezierPatches
    /// </summary>
    public void CalculateMetaMesh()
    {
        //Debug.Log(Resolution);
        MetaMesh = new Mesh();
        metaVertices = new List<Vector3>();
        metaTriangles = new int[Resolution * (Resolution * 6) * PatchAmount * PatchAmount];
        metaUVs = new List<Vector2>();
        for (int i = 0; i < PatchAmount; i++)
        {
            for (int j = 0; j < PatchAmount; j++)
            {
                for (int k = 0; k <= Resolution; k++)
                {
                    for (int l = 0; l <= Resolution; l++)
                    {
                        Vector2 temp = new Vector2((i + ((float)k / Resolution)), (j + ((float)l / Resolution)));
                        metaVertices.Add(GetPointAt(temp));
                        metaUVs.Add(new Vector2(i * Resolution + k, j * Resolution + l));
                    }
                }
            }
        }
        MetaMesh.SetVertices(metaVertices);
        MetaMesh.SetUVs(0, metaUVs);
        // create the triangles
        int countu = 0;
        int triCount = 0;
        int metaCounter = 0;
        // patches in x direction
        for (int i = 0; i < PatchAmount; i++)
        {
            // patches in z direction
            for (int j = 0; j < PatchAmount; j++)
            {
                // runs per patch
                for (int k = 0; k < Resolution * (Resolution * 6); k += 6)
                {
                    //Debug.Log(countu);
                    if (k == 0 && countu != 0)
                    {
                        countu += 6;
                        //Debug.Log("Trigger: " + countu);

                    }
                    if ((countu + 1) % (Resolution + 1) == 0)
                    {
                        countu++;
                        //Debug.Log("Jump: " + countu);
                    }
                    //Debug.Log(countu);

                    //Debug.Log("Counter: " + metaCounter + " Max: " + metaTriangles.Length);
                    metaTriangles[metaCounter] = countu;
                    metaTriangles[metaCounter + 1] = countu + 1;
                    metaTriangles[metaCounter + 2] = countu + Resolution + 1;
                    metaTriangles[metaCounter + 3] = countu + 1;
                    metaTriangles[metaCounter + 4] = countu + Resolution + 2;
                    metaTriangles[metaCounter + 5] = countu + Resolution + 1;
                    triCount += 2;
                    countu++;
                    metaCounter += 6;
                    //Debug.Log(countu);
                }
            }
        }
        MetaMesh.SetTriangles(metaTriangles, 0);
    }

    public override string ToString()
    {
        string temp = "";
        for (int i = 0; i < PatchAmount; i++)
        {
            for (int j = 0; j < PatchAmount; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    for (int l = 0; l < 4; l++)
                    {
                        temp += "Patch: [" + i + "," + j + "] " + "Index: [" + l + "," + k + "]" + SurfacePatches[i, j].BezierPoints[k, l].Position.y + "\n";
                    }
                }
                temp += "\n";
            }
            temp += "\n";
        }
        return temp;
    }

    public Vector3 GetPointAt(Vector2 uv)
    {
        Vector2i patch = new Vector2i((int)uv.x, (int)uv.y);
        if (patch.z == PatchAmount && patch.x == PatchAmount)
        {
            return SurfacePatches[patch.x - 1, patch.z - 1].GetPointAt(new Vector2(1, 1));
        }
        else if (patch.x == PatchAmount)
        {
            return SurfacePatches[patch.x-1, patch.z].GetPointAt(new Vector2(1, uv.y % 1));
        }
        else if(patch.z == PatchAmount)
        {
            return SurfacePatches[patch.x, patch.z-1].GetPointAt(new Vector2(uv.x % 1, 1));
        }

        else return SurfacePatches[patch.x, patch.z].GetPointAt(new Vector2(uv.x % 1, uv.y % 1));
    }
}

