using UnityEngine;
using System.Collections;

[System.Serializable]
public class UnitySurfacePatch : MonoBehaviour
{

    public SurfacePatchMesh CoreSurfacePatchMesh;
    public SurfacePatch bezierPoints;
    [Range(2, 20)]
    public int Resolution = 10;
    private int prevResolution;

    /// <summary>
    /// Called when loading the script
    /// </summary>
    void Awake()
    {
        bezierPoints = new SurfacePatch();
        CoreSurfacePatchMesh = new SurfacePatchMesh(bezierPoints, Resolution);
    }
        // Called after awake
        void Start()
    {
        CoreSurfacePatchMesh.Resolution = Resolution;
        CoreSurfacePatchMesh.rebuild();
    }

    // Update is called once per frame
    void Update()
    {
        if(prevResolution != Resolution)
        {
            CoreSurfacePatchMesh.Resolution = Resolution;
            CoreSurfacePatchMesh.rebuild();
        }
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.Clear();
        mesh.vertices = CoreSurfacePatchMesh.newVertices;
        mesh.uv = CoreSurfacePatchMesh.newUV;
        mesh.triangles = CoreSurfacePatchMesh.newTriangles;
    }

    // DebugDraws
    void OnDrawGizmos()
    {
        // Gizmos are only available when the game is running
        if(CoreSurfacePatchMesh != null)
        {
            // Sampled Surface Points, identical to the Mesh vertices
            Gizmos.color = Color.yellow;
            for (int i = 0; i < CoreSurfacePatchMesh.newVertices.Length; i++)
            {
                Gizmos.DrawSphere(CoreSurfacePatchMesh.newVertices[i], 0.01f);
            }
            // Control Points
            Gizmos.color = Color.red;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Gizmos.DrawSphere(CoreSurfacePatchMesh.CoreSurfacePatch.BezierPoints[i, j].Position, 0.01f * CoreSurfacePatchMesh.CoreSurfacePatch.BezierPoints[i, j].Weight);
                }
            }
        }
    }
}
