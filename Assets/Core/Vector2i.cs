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

    public override string ToString()
    {
        return "(" + this.x + ", " + this.z + ")";
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ z.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        var other = obj as Vector2i;
        if (other == null)
            return false;

        return this.x == other.x && this.z == other.z;
    }
}

