using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;


class DynKeyMap
{
    ArrayList XPZP = new ArrayList();
    ArrayList XPZN = new ArrayList();
    ArrayList XNZP = new ArrayList();
    ArrayList XNZN = new ArrayList();


    public DynKeyMap()
    {

    }

    public void add(Vector2i vec2i)
    {
        bool IsXPos = true;
        bool IsZPos = true;
        if (vec2i.x < 0)
        {
            IsXPos = false;
        }
        if(vec2i.z < 0)
        {
            IsZPos = false;
        }
        if(IsXPos && IsZPos)
        {
        }
    }
}
