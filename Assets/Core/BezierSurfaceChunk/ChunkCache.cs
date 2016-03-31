using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

public class ChunkCache
{
    /// <summary>
    /// Actuall containing the cunkdata, which can be used with the key from the hashmap
    /// </summary>    
    public Dictionary<Vector2i, BezierChunk> GeneratedChunksMap = new Dictionary<Vector2i, BezierChunk>();
    /// <summary>
    /// List of all chunks, which need to be generated
    /// </summary>
    public List<Vector2i> ChunksToGenerate = new List<Vector2i>();
    /// <summary>
    /// List of chunks, which are actually generated
    /// </summary>
    public List<Vector2i> ChunksToDelete = new List<Vector2i>();
    /// <summary>
    /// List of chunks, which are actually generated
    /// </summary>
    public List<Vector2i> ChunksInGeneration = new List<Vector2i>();
    /// <summary>
    /// just a reference to the Chunkgenerator for acessing in O(1), a ref doen´t cost anything, getComponent does
    /// </summary>
    public ChunkGenerator GenRef;
    
    public int StartSeed { get; set; }
    public int Resolution { get; set; }
    public float Steepness { get; set; }
    public float MaxOverhang { get; set; }
    public float OverhangRatio { get; set; }
    /// <summary>
    /// Amount of patches of a bezierchunk
    /// for the first patch, 4 bezierpoints are required, for every next patch, +3 bezierpoints
    /// </summary>
    public int Patchamount { get; set; }

    public ChunkCache(ChunkGenerator GenRef)
    {
        this.GenRef = GenRef;
    }


    public void Update(Vector3 position)
    {

    }

    public bool GenerateChunk(Vector2i _generateKey)
    {
        ChunksInGeneration.Add(_generateKey);
        if(GeneratedChunksMap.Count > 0 && !HasNighbour(_generateKey))
        {
            Debug.Log("Chunk isn´t at startpoint and has no neighbours");
            return false;
        }
        // create new empty chunk
        BezierChunk toGenerate = new BezierChunk();
        toGenerate.Resolution = Resolution;
        toGenerate.Seed = StartSeed + _generateKey.GetHashCode();
        //Debug.Log(toGenerate.Seed);
        toGenerate.Steepness = Steepness;
        toGenerate.MaxOverhang = MaxOverhang;
        toGenerate.OverhangRatio = OverhangRatio;
        toGenerate.PatchAmount = Patchamount;
        toGenerate.Positionkey = _generateKey;
        toGenerate.GenRef = GenRef;
        // link neighbours
        BezierChunk tempLeft;
        BezierChunk tempRight;
        BezierChunk tempTop;
        BezierChunk tempBot;
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyLeft(_generateKey), out tempLeft);
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyRight(_generateKey), out tempRight);
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyTop(_generateKey), out tempTop);
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyBot(_generateKey), out tempBot);
        toGenerate.Left = tempLeft;
        toGenerate.Right = tempRight;
        toGenerate.Up = tempTop;
        toGenerate.Down = tempBot;
        // Assign the noise and generates it
        toGenerate.AssignNoise();
        // let the chunk actually calculate
        toGenerate.AssignPatches();
        toGenerate.CalculateMetaMesh();
        // Instantiate the Chunk in Unity
        toGenerate.InstantiateThisChunk();
        // add the chunk to the map
        GeneratedChunksMap.Add(_generateKey, toGenerate);
        // update the noise of the chunkneighbours
        UpdateNeighbours(_generateKey);
        // delete the created chunk out of the list of chunks which are in creation
        ChunksInGeneration.Remove(_generateKey);
        return true;
    }

    #region update ChunkNeighbourNoise and rebuild the NeighbourChunks
    public void UpdateNeighbours(Vector2i midVec)
    {
        BezierChunk temp;
        GeneratedChunksMap.TryGetValue(midVec, out temp);
        //Debug.Log(temp.ToString());
        if (GetNeighbourChunkTop(midVec) != null)
        {           
            GetNeighbourChunkTop(midVec).ChunkNoise.BotZNoise = temp.ChunkNoise;
            temp.Up = GetNeighbourChunkTop(midVec);
            GetNeighbourChunkTop(midVec).RebuildChunk();
            if (GetNeighbourChunkTopLeft(midVec) != null)
                GetNeighbourChunkTopLeft(midVec).RebuildChunk();
            if (GetNeighbourChunkTopRight(midVec) != null)
                GetNeighbourChunkTopRight(midVec).RebuildChunk();
        }
        if (GetNeighbourChunkBot(midVec) != null)
        {
            GetNeighbourChunkBot(midVec).ChunkNoise.TopZNoise = temp.ChunkNoise;
            temp.Down = GetNeighbourChunkBot(midVec);
            GetNeighbourChunkBot(midVec).RebuildChunk();
            if (GetNeighbourChunkBotLeft(midVec) != null)
                GetNeighbourChunkBotLeft(midVec).RebuildChunk();
            if (GetNeighbourChunkBotRight(midVec) != null)
                GetNeighbourChunkBotRight(midVec).RebuildChunk();
        }
        if (GetNeighbourChunkLeft(midVec) != null)
        {
            GetNeighbourChunkLeft(midVec).ChunkNoise.RightZNoise = temp.ChunkNoise;
            temp.Left = GetNeighbourChunkLeft(midVec);
            GetNeighbourChunkLeft(midVec).RebuildChunk();
            if (GetNeighbourChunkBotLeft(midVec) != null)
                GetNeighbourChunkBotLeft(midVec).RebuildChunk();
            if (GetNeighbourChunkTopLeft(midVec) != null)
                GetNeighbourChunkTopLeft(midVec).RebuildChunk();
        }
        if (GetNeighbourChunkRight(midVec) != null)
        {
            GetNeighbourChunkRight(midVec).ChunkNoise.LeftZNoise = temp.ChunkNoise;
            temp.Right = GetNeighbourChunkRight(midVec);
            GetNeighbourChunkRight(midVec).RebuildChunk();
            if (GetNeighbourChunkBotRight(midVec) != null)
                GetNeighbourChunkBotRight(midVec).RebuildChunk();
            if (GetNeighbourChunkTopRight(midVec) != null)
                GetNeighbourChunkTopRight(midVec).RebuildChunk();
        }
        temp.AssignNeighboursToZNoise();
    }
    #endregion
    #region getting Neighbour Keys
    public Vector2i GetNeighbourKeyTop(Vector2i _midKey)
    {
        return new Vector2i(_midKey.x + 1, _midKey.z);
    }
    public Vector2i GetNeighbourKeyBot(Vector2i _midKey)
    {
        return new Vector2i(_midKey.x - 1, _midKey.z);
    }
    public Vector2i GetNeighbourKeyLeft(Vector2i _midKey)
    {
        return new Vector2i(_midKey.x, _midKey.z + 1);
    }
    public Vector2i GetNeighbourKeyRight(Vector2i _midKey)
    {
        return new Vector2i(_midKey.x, _midKey.z - 1);
    }
    public Vector2i GetNeighbourKeyTopLeft(Vector2i _midKey)
    {
        return new Vector2i(_midKey.x + 1, _midKey.z +1);
    }
    public Vector2i GetNeighbourKeyTopRight(Vector2i _midKey)
    {
        return new Vector2i(_midKey.x + 1, _midKey.z - 1);
    }
    public Vector2i GetNeighbourKeyBotLeft(Vector2i _midKey)
    {
        return new Vector2i(_midKey.x - 1, _midKey.z + 1);
    }
    public Vector2i GetNeighbourKeyBotRight(Vector2i _midKey)
    {
        return new Vector2i(_midKey.x - 1, _midKey.z - 1);
    }


    #endregion
    public bool HasNighbour(Vector2i _midKey)
    {
        if (!GeneratedChunksMap.ContainsKey(GetNeighbourKeyTop(_midKey)) && !GeneratedChunksMap.ContainsKey(GetNeighbourKeyBot(_midKey)) && !GeneratedChunksMap.ContainsKey(GetNeighbourKeyLeft(_midKey)) && !GeneratedChunksMap.ContainsKey(GetNeighbourKeyRight(_midKey)))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    #region getting Chunks with Key
    public BezierChunk GetNeighbourChunkTop(Vector2i _midKey)
    {
        BezierChunk tempChunk;
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyTop(_midKey), out tempChunk);
        return tempChunk;
    }
    public BezierChunk GetNeighbourChunkBot(Vector2i _midKey)
    {
        BezierChunk tempChunk;
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyBot(_midKey), out tempChunk);
        return tempChunk;
    }
    public BezierChunk GetNeighbourChunkLeft(Vector2i _midKey)
    {
        BezierChunk tempChunk;
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyLeft(_midKey), out tempChunk);
        return tempChunk;
    }
    public BezierChunk GetNeighbourChunkRight(Vector2i _midKey)
    {
        BezierChunk tempChunk;
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyRight(_midKey), out tempChunk);
        return tempChunk;
    }
    public BezierChunk GetNeighbourChunkTopLeft(Vector2i _midKey)
    {
        BezierChunk tempChunk;
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyTopLeft(_midKey), out tempChunk);
        return tempChunk;
    }
    public BezierChunk GetNeighbourChunkTopRight(Vector2i _midKey)
    {
        BezierChunk tempChunk;
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyTopRight(_midKey), out tempChunk);
        return tempChunk;
    }
    public BezierChunk GetNeighbourChunkBotLeft(Vector2i _midKey)
    {
        BezierChunk tempChunk;
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyBotLeft(_midKey), out tempChunk);
        return tempChunk;
    }
    public BezierChunk GetNeighbourChunkBotRight(Vector2i _midKey)
    {
        BezierChunk tempChunk;
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyBotRight(_midKey), out tempChunk);
        return tempChunk;
    }
    #endregion

}

