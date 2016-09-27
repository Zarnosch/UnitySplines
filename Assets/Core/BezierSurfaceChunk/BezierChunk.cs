using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using LibNoise.Generator;

public class BezierChunk : System.Object
{
    public Vector2i Positionkey {get; set; }
    public Vector3 AverageMidPoint { get; set; }
    private int PointAmount;
    public int PatchAmount { get; set; }
    public BezierChunk Up { get; set; }
    public BezierChunk Down { get; set; }
    public BezierChunk Left { get; set; }
    public BezierChunk Right { get; set; }
    public BezierChunk In { get; set; }
    public BezierChunk Out { get; set; }
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
    private List<Vector3> metaNormals;
    private List<Vector2> metaUVs;
    private int[] metaTriangles;
    public GameObject GOChunk;
    public bool JustUpdated;
    public ChunkGenerator GenRef;
    
    public BezierChunk()
    {
    }

    /// <summary>
    /// Initializes the noise and also asignes the noise from neighbourjood chunks if available
    /// </summary>
    public void AssignNoise()
    {
        //Debug.Log("AssignNoise of" + Positionkey.ToString());
        //Debug.Log(PatchAmount);
        PointAmount = 1 + 3 * PatchAmount;
        ChunkNoise = new ZNoise(Positionkey, GenRef.Noise, GenRef.Range, PointAmount);
        AssignNeighboursToZNoise();
        ChunkNoise.calculatePoints();
    }

    public void AssignNeighboursToZNoise()
    {
        //Debug.Log("NOISE: " + Positionkey);
        if(Left != null)
        {
            ChunkNoise.LeftZNoise = Left.ChunkNoise;
            //Debug.Log("NOISE: Left found");
        }
        if (Right != null)
        {
            ChunkNoise.RightZNoise = Right.ChunkNoise;
            //Debug.Log("NOISE: Right found");
        }
        if (Up != null)
        {
            ChunkNoise.TopZNoise = Up.ChunkNoise;
            //Debug.Log("NOISE: Top found");
        }
        if (Down != null)
        {
            ChunkNoise.BotZNoise = Down.ChunkNoise;
            //Debug.Log("NOISE: Bot found");
        }
    }

    public void AssignPatches()
    {
        SurfacePatches = new SurfacePatch[PatchAmount, PatchAmount];
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
        //Debug.Log("Pachtes of Chunk " + Positionkey + " have been assigned!");
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
        float eps = 0.001f;
        MetaMesh = new Mesh();
        metaVertices = new List<Vector3>();
        metaNormals = new List<Vector3>();
        metaTriangles = new int[(Resolution*PatchAmount)* (Resolution * PatchAmount) * 6];
        metaUVs = new List<Vector2>();
        
        // assign whole chunk
        for (int x = 0; x <= PatchAmount * Resolution; x++)
        {
            for (int z = 0; z <= PatchAmount * Resolution; z++)
            {
                Vector2 temp = new Vector2(((x/ Resolution) + ((float)x / Resolution)%1), ((z/Resolution) + ((float)z / Resolution)%1));
                //Debug.Log(temp);
                metaVertices.Add(GetPointAt(temp));
                metaUVs.Add(new Vector2(x, z));
                
                // normal calculation needs the 1 ringneighbourhood vertices of the mesh
                Vector2 rightV2 = new Vector2(((x / Resolution) + ((float)x / Resolution) % 1), ((z / Resolution) + ((float)(z) / Resolution) % 1));
                rightV2 += new Vector2(0, eps);
                Vector2 leftV2 = new Vector2(((x / Resolution) + ((float)x / Resolution) % 1), ((z / Resolution) + ((float)(z) / Resolution) % 1));
                leftV2 += new Vector2(0, -eps);
                Vector2 upV2 = new Vector2(((x / Resolution) + ((float)(x) / Resolution) % 1), ((z / Resolution) + ((float)z / Resolution) % 1));
                upV2 += new Vector2(-eps, 0);
                Vector2 downV2 = new Vector2(((x / Resolution) + ((float)(x) / Resolution) % 1), ((z / Resolution) + ((float)z / Resolution) % 1));
                downV2 += new Vector2(eps, 0);
                //Debug.Log(temp + " | " + rightV2 + " | " + downV2);
                Vector3 rightV3 = GetPointAt(rightV2);
                Vector3 leftV3 = GetPointAt(leftV2);
                Vector3 upV3 = GetPointAt(upV2);
                Vector3 downV3 = GetPointAt(downV2);
                //Debug.Log(GetPointAt(temp) + " | " + GetPointAt(downV2) + " | " + GetPointAt(rightV2));
                Vector3 normal;
                if (x == PatchAmount * Resolution || z == PatchAmount * Resolution)
                {

                    normal = GetNormal(GetPointAt(temp), leftV3, upV3);
                }
                else
                {
                    normal = GetNormal(GetPointAt(temp), rightV3, downV3);
                }

                normal.Normalize();
                metaNormals.Add(normal);
                
            }
        }
        MetaMesh.SetVertices(metaVertices);
        MetaMesh.SetUVs(0, metaUVs);
        // create the triangles for whole chunk
        int countu = 0;
        int triCount = 0;
        int metaCounter = 0;
        // patches in x direction
        for (int x = 0; x < PatchAmount * Resolution; x++)
        {
            // patches in z direction
            for (int z = 0; z < PatchAmount * Resolution; z++)
            {
                if ((countu+1) % ((PatchAmount * Resolution)+1)  ==  0)
                {
                    //Debug.Log(countu);
                    countu++;
                }
                int temp = countu;
                metaTriangles[metaCounter] = countu;
                metaTriangles[metaCounter + 1] = countu + 1;
                metaTriangles[metaCounter + 2] = countu + Resolution*PatchAmount + 1;
                metaTriangles[metaCounter + 3] = countu + 1;
                metaTriangles[metaCounter + 4] = countu + Resolution*PatchAmount + 2;
                metaTriangles[metaCounter + 5] = countu + Resolution*PatchAmount + 1;
                triCount += 2;
                countu++;
                metaCounter += 6;
                //Debug.Log(metaCounter);
            }
        }
        MetaMesh.SetTriangles(metaTriangles, 0);
        MetaMesh.SetNormals(metaNormals);
        //MetaMesh.RecalculateNormals();
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
        //Debug.Log(patch);
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

    public void InstantiateThisChunk()
    {
        GOChunk = new GameObject();
        //GOChunk.transform.position = SurfacePatches[0, 0].BezierPoints[0, 0].Position;
        GOChunk.name = "BezierChunk" + Positionkey;
        GOChunk.AddComponent<MeshFilter>();
        GOChunk.AddComponent<MeshRenderer>();
        GOChunk.AddComponent<UnityBezierChunk>();
        GOChunk.AddComponent<MeshCollider>();
        MeshFilter tempFilter = GOChunk.GetComponent<MeshFilter>();
        tempFilter.mesh = MetaMesh;
        MeshRenderer tempRenderer = GOChunk.GetComponent<MeshRenderer>();
        tempRenderer.material = new Material(Shader.Find("Custom/CellShading_Ground"));
        UnityBezierChunk UBChunk = GOChunk.GetComponent<UnityBezierChunk>();
        UBChunk.chunk = this;
        MeshCollider collider = GOChunk.GetComponent<MeshCollider>();
        collider.sharedMesh = MetaMesh;
        UBChunk.MetaMesh = MetaMesh;
        UBChunk.AverageMidPoint = AverageMidPoint;
        GOChunk.SetActive(true);
        GOChunk.transform.parent = GenRef.transform;
    }

    public void RebuildChunk()
    {
        Debug.Log("Rebuild1");
        //AssignNoise();
        AssignPatches();
        CalculateMetaMesh();
        
        MeshFilter tempFilter = GOChunk.GetComponent<MeshFilter>();
        tempFilter.mesh = MetaMesh;
        MeshRenderer tempRenderer = GOChunk.GetComponent<MeshRenderer>();
        tempRenderer.material = new Material(Shader.Find("Custom/CellShading_Ground"));
        UnityBezierChunk UBChunk = GOChunk.GetComponent<UnityBezierChunk>();
        UBChunk.chunk = this;
        MeshCollider collider = GOChunk.GetComponent<MeshCollider>();
        collider.sharedMesh = MetaMesh;
        UBChunk.MetaMesh = MetaMesh;
        UBChunk.AverageMidPoint = AverageMidPoint;
        GOChunk.SetActive(true);
    }
    
    public void RebuildChunkWithNeighbourUpdate()
    {
        AssignNoise();
        AssignPatches();
        GenRef.Cache.UpdateNeighbours(Positionkey);
        CalculateMetaMesh();

        MeshFilter tempFilter = GOChunk.GetComponent<MeshFilter>();
        tempFilter.mesh = MetaMesh;
        MeshRenderer tempRenderer = GOChunk.GetComponent<MeshRenderer>();
        tempRenderer.material = new Material(Shader.Find("Custom/CellShading_Ground"));
        UnityBezierChunk UBChunk = GOChunk.GetComponent<UnityBezierChunk>();
        UBChunk.chunk = this;
        MeshCollider collider = GOChunk.GetComponent<MeshCollider>();
        collider.sharedMesh = MetaMesh;
        UBChunk.MetaMesh = MetaMesh;
        UBChunk.AverageMidPoint = AverageMidPoint;
        GOChunk.SetActive(true);
    }

    Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {       
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        return Vector3.Cross(side1, side2).normalized;
    }

}

