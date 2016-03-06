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
    public CombineInstance[] combine;
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
    public int Steepness = 41;
    private int prevSteepness = 41;

    // Use this for initialization
    void Start()
    {
        chunk = new BezierChunk(Resolution, Seed, Steepness);
        chunk.AssignPatches();
        chunk.CalculateMeshes();

        MetaMesh = new Mesh();
        metaVertices = new List<Vector3>();
        metaTriangles = new int[Resolution * (Resolution * 6)*chunk.PatchAmount*chunk.PatchAmount];
        metaUVs = new List<Vector2>();
        for (int i = 0; i < chunk.PatchAmount; i++)
        {
            for (int j = 0; j < chunk.PatchAmount; j++)
            {
                for (int k = 0; k <= Resolution; k++)
                {
                    for (int l = 0; l <= Resolution; l++)
                    {
                        Vector2 temp = new Vector2((i + ((float)k / Resolution)), (j + ((float)l / Resolution)));
                        metaVertices.Add(chunk.GetPointAt(temp));
                        metaUVs.Add(new Vector2(i * Resolution + k, j * Resolution + l));
                    }
                }                
                //MetaMesh.SetIndices(chunk.Mesh[i, j].newTriangles, MeshTopology.Triangles, (i * chunk.PatchAmount) + j);
            }
        }
        MetaMesh.SetVertices(metaVertices);
        MetaMesh.SetUVs(0, metaUVs);
        // create the triangles
        int countu = 0;
        int triCount = 0;
        int metaCounter = 0;
        // patches in x direction
        for (int i = 0; i < chunk.PatchAmount; i++)
        {
            // patches in z direction
            for (int j = 0; j < chunk.PatchAmount; j++)
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
                    if ((countu + 1) % (Resolution +1) == 0)
                    {                        
                        countu++;
                        //Debug.Log("Jump: " + countu);
                    }
                    //Debug.Log(countu);

                    //Debug.Log("Counter: " + metaCounter);
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
                //MetaMesh.SetTriangles(newTriangles, i*chunk.PatchAmount + j);
            }
        }
        MetaMesh.SetTriangles(metaTriangles, 0);
        //Debug.Log(MetaMesh.subMeshCount);
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = MetaMesh;
        //combine = new CombineInstance[1];
        //combine[0].mesh = MetaMesh;
        //Mesh tmpMesh = new Mesh();
        //tmpMesh.CombineMeshes(combine, true, true);
        //tmpMesh.name = "Lol";
        //MetaMesh.name = "Omg";
        //meshFilter.mesh = tmpMesh;
        //meshFilter.mesh = MetaMesh;
        //Debug.Log(tmpMesh.subMeshCount);
        //transform.gameObject.active = true;
        
    /*
    //copy
    int i = 0;
    while (i < meshFilters.Length)
    {
        combine[i].mesh = meshFilters[i].sharedMesh;
        combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        meshFilters[i].gameObject.active = false;
        i++;
    }
    transform.GetComponent<MeshFilter>().mesh = new Mesh();
    transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
    transform.gameObject.active = true;
    */
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
        
        /*
        if (prevSeed != Seed || prevSteepness != Steepness)
        {
            chunk = new BezierChunk(Resolution, Seed, Steepness);
            prevSeed = Seed;
            prevSteepness = Steepness;
            chunk.AssignPatches();
        }
        */
        

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

