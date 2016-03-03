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

    public BezierChunk(int resolution, int seed, float steepness)
    {
        Resolution = resolution;
        Positionkey = new Vector2i(0, 0);
        PointAmount = 16;
        Seed = seed;
        Steepness = steepness;
        ChunkNoise = new ZNoise(EBiom.Flat, Seed, Steepness);
        ChunkNoise.SizeToGenerate = PointAmount;
        ChunkNoise.calculatePoints();
        PatchAmount = PointAmount / 3;
        SurfacePatches = new SurfacePatch[PatchAmount, PatchAmount];
        Mesh = new SurfacePatchMesh[PatchAmount, PatchAmount];
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
}

