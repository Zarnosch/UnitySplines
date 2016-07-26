using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class ZRandom
{
    // Seeds
    static uint m_w = 362436069;    /* must not be zero */
    static uint m_z = 521288629;    /* must not be zero */

    public ZRandom()
    {
    }

    public ZRandom (int seed)
    {
        if(seed == 0)
        {
            m_w = 362436069;
            m_z = 521288629;
        }
        else
        {
            m_w = 362436069 + (uint)seed;
            m_w = 521288629 + (uint)seed;
        }
    }

    public int Next()
    {
        m_z = 36969 * (m_z & 65535) + (m_z >> 16);
        m_w = 18000 * (m_w & 65535) + (m_w >> 16);
        return (int)((m_z << 16) + m_w);
    }

    public int Next(int maxVal)
    {
        return (int) NextDouble() * maxVal;
    }

    public int Next(int minVal, int maxVal)
    {
        return (int) (minVal + (NextDouble() * (maxVal - minVal)));
    }

    public float Next(float minVal, float maxVal)
    {
        return (float)(minVal + (NextDouble() * (maxVal - minVal)));
    }

    public double NextDouble()
    {
        return (((double)Next() / (float)int.MaxValue)+1)/2d;
    }
}

