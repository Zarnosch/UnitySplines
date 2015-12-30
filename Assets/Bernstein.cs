using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class Bernstein : System.Object
{
    public int i, n;
    public float t;

    public Bernstein(int i, int n, float t)
    {
        //Debug.Log("I: " + i + " N: " + n + " T: " + t);
        this.i = i;
        this.n = n;
        this.t = t;
    }

    public float calculate()
    {
        return binom(n, i) * exp(t, i) * exp((1 - t), (n - i));

    }

    public float binom(int n, int k)
    {
        if (k >= 0 && k <= n)
        {
            return fac(n) / (fac(n - i) * fac(i));
        }
        else return 0;
    }
    // Returns the faculty of n
    public float fac(int n)
    {
        float temp = 1;
        for (int i = 1; i <= n; i++)
        {
            temp *= i;
        }
        return temp;
    }
    public float exp(float a, float b)
    {
        return Mathf.Pow(a, b);
    }
}
