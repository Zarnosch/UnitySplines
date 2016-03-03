using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class Vector2i : System.Object
{
    public int x { get; set; }
    public int z { get; set; }
    public Vector2i()
    {
        this.x = 0;
        this.x = 0;
    }

    public Vector2i(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
}

