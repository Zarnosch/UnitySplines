﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;


public class ChunkGenerator : MonoBehaviour
{

    [Range(2, 20)]
    public int Resolution = 5;
    private int prevResolution = 5;

    [Range(1, 42)]
    public int PatchAmount = 10;
    private int patchAmount = 10;

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

    public Vector2i genPlace = new Vector2i(0, 0);

    public ChunkCache Cache;

    public GameObject Player;
    // Use this for initialization
    void Start()
    {
        Cache = new ChunkCache(this);
        Cache.Resolution = Resolution;
        Cache.StartSeed = Seed;
        Cache.Steepness = Steepness;
        Cache.MaxOverhang = MaxOverhang;
        Cache.OverhangRatio = OverhangRatio;
        Cache.Patchamount = PatchAmount;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if(PatchAmount != patchAmount && Cache.GeneratedChunksMap.Count == 0)
        {
            Cache.Patchamount = PatchAmount;
            patchAmount = PatchAmount;
        }
        Cache.Update(Player.transform.position);
    }
}

