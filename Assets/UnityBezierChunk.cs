using UnityEngine;
using System.Collections;

public class UnityBezierChunk : MonoBehaviour
{
    public BezierChunk chunk;
    public Mesh[,] mesh;
    public Mesh AMesh;

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
    public int Steepness = 41;
    private int prevSteepness = 41;
    // Use this for initialization
    void Start()
    {
        Mesh[,] mesh = new Mesh[5, 5];
        chunk = new BezierChunk(Resolution, Seed, Steepness);
        chunk.AssignPatches();
        chunk.CalculateMeshes();
        /*
        for (int i = 0; i < chunk.PatchAmount; i++)
        {
            for (int j = 0; j < chunk.PatchAmount; j++)
            {
                mesh[i, j].vertices = chunk.Mesh[i, j].newVertices;
                mesh[i, j].uv = chunk.Mesh[i, j].newUV;
                mesh[i, j].triangles = chunk.Mesh[i, j].newTriangles;
            }
        }
        */


    }

    // Update is called once per frame
    void Update()
    {
        /*
        Mesh[,] mesh = new Mesh[5,5];
        for (int i = 0; i < chunk.PatchAmount; i++)
        {
            for (int j = 0; j < chunk.PatchAmount; j++)
            {
                if (prevResolution != Resolution)
                {
                    chunk.Mesh[i, j].Resolution = Resolution;
                    chunk.Mesh[i, j].rebuild();
                }
                if(mesh[i, j] == null)
                {
                    Debug.Log("Fuck!");
                }
                else if(chunk.Mesh[i, j] == null)
                {
                    Debug.Log("Null Mesh!");
                }
                else
                {
                    mesh[i,j].Clear();
                    mesh[i, j].vertices = chunk.Mesh[i, j].newVertices;
                    mesh[i, j].uv = chunk.Mesh[i, j].newUV;
                    mesh[i, j].triangles = chunk.Mesh[i, j].newTriangles;                    
                    GetComponent<MeshFilter>().mesh = mesh[i,j];

                }

            }
        }
        */
        
        
        if (prevSeed != Seed || prevSteepness != Steepness)
        {
            chunk = new BezierChunk(Resolution, Seed, Steepness);
            prevSeed = Seed;
            prevSteepness = Steepness;
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
                    // Sampled Surface Points, identical to the Mesh vertices
                    /*
                    Gizmos.color = Color.yellow;
                    for (int i = 0; i < CoreSurfacePatchMesh.newVertices.Length; i++)
                    {
                        Gizmos.DrawSphere(CoreSurfacePatchMesh.newVertices[i], 0.01f);
                    }
                    */
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
                            Gizmos.DrawSphere(chunk.SurfacePatches[i, j].GetPointAt(new Vector2((float)k/Resolution, (float)l/Resolution)), 0.05f);
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
    }
}

