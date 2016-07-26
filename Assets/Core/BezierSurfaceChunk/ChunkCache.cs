using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading;

public class ChunkCache
{
    /// <summary>
    /// Actuall containing the chunkdata, which can be used with the key from the hashmap
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
    /// List of Chunks which have their noise but the mesh has to be build
    /// </summary>
    public List<BezierChunk> ChunkMeshToGeneration = new List<BezierChunk>();
    /// <summary>
    /// just a reference to the Chunkgenerator for acessing in O(1), a ref doen´t cost anything, getComponent does
    /// </summary>
    public ChunkGenerator GenRef;
    /// <summary>
    /// active Playerposition, which is used to calculate, which chunks needs to be generated or deleted
    /// </summary>
    public Vector3 PlayerPosition;
    /// <summary>
    /// the active chunk, the player is standing on
    /// </summary>
    public Vector2i ActiveChunk;
    private Vector2i lastActiveChunk;
    /// <summary>
    /// amount of chunks between the playerposition and the furthest chunk
    /// </summary>
    public int ChunkGenerationDistance = 4;

    public int StartSeed { get; set; }
    public int Resolution { get; set; }
    public float Steepness { get; set; }
    public float MaxOverhang { get; set; }
    public float OverhangRatio { get; set; }

    BezierChunk refActiveChunk, top, left, bot, right, topLeft, topRight, botLeft, botRight;
    float diffLast, diffTop, diffLeft, diffBot, diffRight, diffTopLeft, diffTopRight, diffBotLeft, diffBotRight;
    List<float> diffs = new List<float>();

    /// <summary>
    /// Amount of patches of a bezierchunk
    /// for the first patch, 4 bezierpoints are required, for every next patch, +3 bezierpoints
    /// </summary>
    public int Patchamount { get; set; }

    public ChunkCache(ChunkGenerator GenRef)
    {
        this.GenRef = GenRef;
    }

    // trigger for first generation
    private bool start = true;
    //only one thread at a time may generate noise for chunks because neighbourhood update
    private bool isThreadActive = false;

    private List<Thread> threads = new List<Thread>();

    public void Update(Vector3 position)
    {
        PlayerPosition = position;
        // at start
        if(start)
        {
            start = false;
            ZRandom rnd = new ZRandom();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if(!(i == 0 && j == 0))
                    {
                        ChunksToGenerate.Add(new Vector2i(i, j));
                    }
                }
            }

            ChunksToGenerate.Add(new Vector2i(0, 0));
            ActiveChunk = new Vector2i(0, 0);
            lastActiveChunk = ActiveChunk;
            UpdateChunksToGenerate(ChunkGenerationDistance);
        }
        
        if(GeneratedChunksMap.Count > 0)
        {
            ActiveChunk = GetActiveChunk(position, ActiveChunk);
        }
        
        if(ActiveChunk != lastActiveChunk)
        {
            Debug.Log("Changed from " + lastActiveChunk + " to " + ActiveChunk);
            //update, because player changed the active chunk
            UpdateChunksToGenerate(ChunkGenerationDistance);
            lastActiveChunk = ActiveChunk;
        }
        // Update Threads
        UpdateThreads();
        // Create the Meshes if rdy by the multithreading
        GenerateMeshs(ChunkGenerationDistance);
    }

    public void UpdateChunksToGenerate(int genDistance)
    {
        for (int x = ActiveChunk.x - genDistance; x <= ActiveChunk.x + genDistance; x++)
        {
            for (int z = ActiveChunk.z - genDistance; z <= ActiveChunk.z + genDistance; z++)
            {                
                if (!GeneratedChunksMap.ContainsKey(new Vector2i(x, z)) && !ChunksInGeneration.Contains(new Vector2i(x, z)) && !ChunksToGenerate.Contains(new Vector2i(x, z)) && GeneratedChunksMap.Count > 8)
                {
                    //Debug.Log(new Vector2i(x, z));
                    ChunksToGenerate.Add(new Vector2i(x, z));
                }
            }
        }
        // generation
        SpawnThreads();
    }

    public void SpawnThreads()
    {
        for (int i = ChunksToGenerate.Count - 1; i >= 0; i--)
        {
            //Debug.Log(ChunksToGenerate.Count + " : " + ChunksToGenerate[i]);
            //Debug.Log("Chunk with " + ChunksToGenerate[i] + " will be generated by thread");
            Vector2i sender = ChunksToGenerate[i];
            Thread t = new Thread(() => GenerateChunk(sender));
            t.IsBackground = true;
            threads.Add(t);
            t.Name = ChunksToGenerate[i].ToString() + "Thread";

            ChunksInGeneration.Add(ChunksToGenerate.ElementAt(i));
            ChunksToGenerate.RemoveAt(i);
        }
    }

    public void UpdateThreads()
    {
        //Debug.Log("Threads: " + threads.Count);
        if (!isThreadActive && threads.Count > 0)
        {
            //Debug.Log(threads[0].Name + " will be started!");
            threads[0].Start();
            threads.RemoveAt(0);
            isThreadActive = true;
        }
    }

    public Vector2i GetActiveChunk(Vector3 position, Vector2i lastActiveChunkVec2)
    {
        GeneratedChunksMap.TryGetValue(lastActiveChunkVec2, out refActiveChunk);
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyLeft(lastActiveChunkVec2), out left);
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyRight(lastActiveChunkVec2), out right);
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyTop(lastActiveChunkVec2), out top);
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyBot(lastActiveChunkVec2), out bot);
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyTopLeft(lastActiveChunkVec2), out topLeft);
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyTopRight(lastActiveChunkVec2), out topRight);
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyBotLeft(lastActiveChunkVec2), out botLeft);
        GeneratedChunksMap.TryGetValue(GetNeighbourKeyBotRight(lastActiveChunkVec2), out botRight);
        try
        {
            diffLast = Vector3.Distance(position, refActiveChunk.AverageMidPoint); 
        }
        catch(NullReferenceException e)
        {
            //Debug.Log(e.Message);
            diffLast = float.MaxValue;
        }
        diffs.Add(diffLast);
        try
        {
            diffTop = Vector3.Distance(position, top.AverageMidPoint); 
        }
        catch (NullReferenceException e)
        {
            //Debug.Log(e.Message);
            diffTop = float.MaxValue;
        }
        diffs.Add(diffTop);
        try
        {
            diffLeft = Vector3.Distance(position, left.AverageMidPoint); 
        }
        catch (NullReferenceException e)
        {
            //Debug.Log(e.Message);
            diffLeft = float.MaxValue;
        }
        diffs.Add(diffLeft);
        try
        {
            diffBot = Vector3.Distance(position, bot.AverageMidPoint); 
        }
        catch (NullReferenceException e)
        {
            //Debug.Log(e.Message);
            diffBot = float.MaxValue;
        }
        diffs.Add(diffBot);
        try
        {
            diffRight = Vector3.Distance(position, right.AverageMidPoint); 
        }
        catch (NullReferenceException e)
        {
            //Debug.Log(e.Message);
            diffRight = float.MaxValue;
        }
        diffs.Add(diffRight);
        try
        {
            diffTopLeft = Vector3.Distance(position, topLeft.AverageMidPoint); 
        }
        catch (NullReferenceException e)
        {
            //Debug.Log(e.Message);
            diffTopLeft = float.MaxValue;
        }
        diffs.Add(diffTopLeft);
        try
        {
            diffTopRight = Vector3.Distance(position, topRight.AverageMidPoint); 
        }
        catch (NullReferenceException e)
        {
            //Debug.Log(e.Message);
            diffTopRight = float.MaxValue;
        }
        diffs.Add(diffTopRight);
        try
        {
            diffBotLeft = Vector3.Distance(position, botLeft.AverageMidPoint); 
        }
        catch (NullReferenceException e)
        {
            //Debug.Log(e.Message);
            diffBotLeft = float.MaxValue;
        }
        diffs.Add(diffBotLeft);
        try
        {
            diffBotRight = Vector3.Distance(position, botRight.AverageMidPoint); 
        }
        catch (NullReferenceException e)
        {
            //Debug.Log(e.Message);
            diffBotRight = float.MaxValue;
        }
        diffs.Add(diffBotRight);
        diffs.Sort();
        float nearestDiff = diffs.ElementAt(0);
        float[] lel = diffs.ToArray();
        for(int i = 0; i < diffs.Count; i++)
        {
            //Debug.Log(i + ". " + lel[i]);
        }
        diffs.Clear();
        if (nearestDiff == diffLast)
            return lastActiveChunkVec2;
        else if (nearestDiff == diffTop)
            return GetNeighbourKeyTop(lastActiveChunkVec2);
        else if (nearestDiff == diffLeft)
            return GetNeighbourKeyLeft(lastActiveChunkVec2);
        else if (nearestDiff == diffRight)
            return GetNeighbourKeyRight(lastActiveChunkVec2);
        else if (nearestDiff == diffBot)
            return GetNeighbourKeyBot(lastActiveChunkVec2);
        else if (nearestDiff == diffTopLeft)
            return GetNeighbourKeyTopLeft(lastActiveChunkVec2);
        else if (nearestDiff == diffTopRight)
            return GetNeighbourKeyTopRight(lastActiveChunkVec2);
        else if (nearestDiff == diffBotLeft)
            return GetNeighbourKeyBotRight(lastActiveChunkVec2);
        else return GetNeighbourKeyBotRight(lastActiveChunkVec2);
    }

    public void GenerateChunk(Vector2i _generateKey)
    {

        //Debug.Log("" + " Start in Thread " + _generateKey);
        if (GeneratedChunksMap.Count > 0 && !HasNighbour(_generateKey))
        {
            //Debug.Log("Chunk isn´t at startpoint and has no neighbours" + _generateKey);
            // Que this thread again
            Thread t = new Thread(() => GenerateChunk(_generateKey));
            t.IsBackground = true;
            threads.Add(t);
            t.Name = _generateKey + "Thread";
            isThreadActive = false;
            return;
        }
        BezierChunk existTest;
        GeneratedChunksMap.TryGetValue(_generateKey, out existTest);
        if(existTest != null)
        {
            existTest.RebuildChunk();
        }
        else
        {
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
            toGenerate.AverageMidPoint = toGenerate.ChunkNoise.AverageMidPoint;
            // let the chunk actually calculate
            toGenerate.AssignPatches();
            // give this chunk free to make the mesh from the noise
            //Thread.Sleep(5000);
            ChunkMeshToGeneration.Add(toGenerate);
            //Debug.Log("Thread finished with generation of " + _generateKey);
        }        
    }

    public void GenerateMeshs(int ChunkGenerationDistance)
    {
        for (int i = ChunkMeshToGeneration.Count - 1; i >= 0 ; i--)
        {
            BezierChunk toGenerate = ChunkMeshToGeneration[i];
            Vector2i _genKey = toGenerate.Positionkey;            
            toGenerate.CalculateMetaMesh();
            // Instantiate the Chunk in Unity
            toGenerate.InstantiateThisChunk();
            // add the chunk to the map
            GeneratedChunksMap.Add(_genKey, toGenerate);
            // update the noise of the chunkneighbours
            //UpdateNeighbours(_genKey);
            // delete the created chunk out of the list of chunks which are in creation
            ChunkMeshToGeneration.RemoveAt(i);
            ChunksInGeneration.Remove(_genKey);
            //Debug.Log(_genKey + " generated");
            isThreadActive = false;
        }
        if (ChunkMeshToGeneration.Count == 0)
        {
            UpdateChunksToGenerate(ChunkGenerationDistance);
        }
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

