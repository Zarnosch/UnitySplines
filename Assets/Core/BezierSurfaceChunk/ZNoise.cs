using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using LibNoise.Generator;


public class ZNoise : System.Object
{
    /// <summary>
    /// Distance between different Bezier Control Points
    /// </summary>
    public int Range { get; set; }
    /// <summary>
    /// Maximum heigt, where bezierpoints can be places
    /// </summary>
    public float MaxHeight { get; set; }
    /// <summary>
    /// Minimum height, where bezierpoints can be placed
    /// </summary>
    public float MinHeight { get; set; }
    /// <summary>
    /// The average midpoint, used to initialize, and to get in touch of global position and height of the noisechunk
    /// </summary>
    public Vector3 AverageMidPoint{ get; set; }
    /// <summary>
    /// The height of the upper-left point of a chunk (+x,+z) for initialisation if the chunk has no neighbours in +x and +z direction
    /// </summary>
    public float CommonHeight { get; set; }
    /// <summary>
    /// The Amount of bezierpoints, which need to be created per direction (x and z)
    /// </summary>
    public int SizeToGenerate { get; set; }
    /// <summary>
    /// Used to make low and high detailed surfaces
    /// </summary>
    public int Ocatves { get; set; }
    /// <summary>
    /// Highest amount of the steepnes, which is going up (use something between 0 and 90) 0 is even, 90 is rectangular up etc
    /// </summary>
    public float PositiveSteepnes { get; set; }
    /// <summary>
    /// Highest amount of the steepnes, which is going down (use something between 0 and 90) 0 is even, 90 is rectangular down etc
    /// </summary>
    public float NegativeSteepnes { get; set; }
    /// <summary>
    /// Highest amount of the Overhang (use something between 0 and 90) 0 is even, 90 is staight back 0 is no overhang at all
    /// </summary>
    public float Overhang { get; set; }
    /// <summary>
    /// 1 is very often 0 is never
    /// </summary>
    public float OverhangRatio { get; set; }

    /*****************************************************************/
    /// <summary>
    /// The noise, which is used in the top-chunk
    /// </summary>
    public ZNoise TopZNoise { get; set; }
    /// <summary>
    /// The noise, which is used in the top-chunk
    /// </summary>
    public ZNoise BotZNoise { get; set; }
    /// <summary>
    /// The noise, which is used in the top-chunk
    /// </summary>
    public ZNoise LeftZNoise { get; set; }
    /// <summary>
    /// The noise, which is used in the top-chunk
    /// </summary>
    public ZNoise RightZNoise { get; set; }
    /*****************************************************************/
    /// <summary>
    /// Is the continuity between 0 = g0, 1 = g1 and 2 = g2 and 3 = g3
    /// </summary>
    public int Continuity { get; set; }
    /// <summary>
    /// Seed for the unity random generator
    /// </summary>
    public Vector2i Positionkey { get; set; }
    /// <summary>
    /// The result, null if not calculated
    /// </summary>
    public Point[,] calculatedPoints;
    /// <summary>
    /// copy of the given meta perlinNoise with all its variables
    /// </summary>
    private NoiseProvider Noise;

    public ZNoise(Vector2i positionKey, NoiseProvider noise, int range, int sizeToGenerate)
    {
        //Debug.Log("Omg!");
        Positionkey = positionKey;
        SizeToGenerate = sizeToGenerate;
        int Seed = Positionkey.GetHashCode();
        Noise = noise;
        //Debug.Log(PerlinNoise.Seed);
        Range = range;
    }

    public Point[,] calculatePoints()
    {
        /* This is just a sketch for the algorithm:
        (0,0) (1,0) (2,0)
        (0,1) (1,1) (2,1)
        (0,2) (1,2) (2,2)

        +x
        ^
        |
         -> -z

        -> top is the top row etc
        -> so the indices need to be assigned, depending from which side we start to generate the points
        */
        //Debug.Log("Start Chunk Generation: " + Positionkey);
        calculatedPoints = new Point[SizeToGenerate, SizeToGenerate];
        //Debug.Log(ToString());
        CalculateKnotvectors();
        //Debug.Log(ToString());
        CalculateNaturalEndCondition();
        //Debug.Log(ToString());
        CalculateTwirstVectors();
        //Debug.Log(ToString());
        CalculateInBetweenPointsC2();
        //Debug.Log(ToString());
        calculateAverageMidPoint();
        //Debug.Log(AverageMidPoint);
        return calculatedPoints;
    }

    public Point[,] recalculatePoints()
    {
        calculatedPoints = new Point[SizeToGenerate, SizeToGenerate];
        //Debug.Log(ToString());
        CalculateNaturalEndCondition();
        //Debug.Log(ToString());
        CalculateTwirstVectors();
        //Debug.Log(ToString());
        CalculateInBetweenPointsC2();
        //Debug.Log(ToString());
        return calculatedPoints;
    }

    public void calculateAverageMidPoint()
    {
        for(int x = 0; x < SizeToGenerate; x++)
        {
            for (int z = 0; z < SizeToGenerate; z++)
            {
                AverageMidPoint += calculatedPoints[x, z].Position;
            }
        }
        AverageMidPoint /= SizeToGenerate * SizeToGenerate;
    }

    /// <summary>
    /// Calculates the KnotVectors, if the endpoints aren´t given, calculate with the range
    /// </summary>
    public void CalculateKnotvectors()
    {
        // first detect, if there are points needed to be assigned from other chunks
        /*
        //Debug.Log("CalculateKnotvectors!" + Positionkey);
        if (TopZNoise != null)
        {
            //Debug.Log("TopNoiseFound");
            for(int z = 0; z < SizeToGenerate; z++)
            {
                // c0 continuity
                calculatedPoints[0, z] = TopZNoise.calculatedPoints[SizeToGenerate-1, z];
                calculatedPoints[0, z].Weight = 1;
                // c1 continuity
                calculatedPoints[1, z] = calculatedPoints[0, z] + calculatedPoints[0, z] - TopZNoise.calculatedPoints[SizeToGenerate - 2, z];
                calculatedPoints[1, z].Weight = 1;
            }
        }
        if (BotZNoise != null)
        {
            //Debug.Log("BotNoiseFound");
            for (int z = 0; z < SizeToGenerate; z++)
            {
                // c0 continuity
                calculatedPoints[SizeToGenerate-1, z] = BotZNoise.calculatedPoints[0, z];
                calculatedPoints[SizeToGenerate-1, z].Weight = 1;
                // c1 continuity
                calculatedPoints[SizeToGenerate - 2, z] = calculatedPoints[SizeToGenerate- 1, z] + calculatedPoints[SizeToGenerate-1, z] - BotZNoise.calculatedPoints[1, z];
                calculatedPoints[SizeToGenerate - 2, z].Weight = 1;
            }
        }
        if (LeftZNoise != null)
        {
            //Debug.Log("LeftNoiseFound");
            for (int x = 0; x < SizeToGenerate; x++)
            {
                // c0 continuity
                calculatedPoints[x, 0] = LeftZNoise.calculatedPoints[x, SizeToGenerate-1];
                calculatedPoints[x, 0].Weight = 1;
                // c1 continuity
                calculatedPoints[x, 1] = calculatedPoints[x, 0] + calculatedPoints[x, 0] - LeftZNoise.calculatedPoints[x, SizeToGenerate - 2];
                calculatedPoints[x, 1].Weight = 1;
            }
        }
        if (RightZNoise != null)
        {
            //Debug.Log("RightNoiseFound");
            for (int x = 0; x < SizeToGenerate; x++)
            {
                // c0 continuity
                calculatedPoints[x, SizeToGenerate-1] = RightZNoise.calculatedPoints[x, 0];
                calculatedPoints[x, SizeToGenerate-1].Weight = 1;
                // c1 continuity
                calculatedPoints[x, SizeToGenerate - 2] = calculatedPoints[x, SizeToGenerate-1] + calculatedPoints[x, SizeToGenerate-1] - RightZNoise.calculatedPoints[x, 1];
                calculatedPoints[x, SizeToGenerate - 2].Weight = 1;
            }
        }
        */
        // now create the knotvectors, starting from top
        for (int x = 0; x < SizeToGenerate; x += 3)
        {
            for (int z = 0; z < SizeToGenerate; z += 3)
            {
                //check, if they aren´t already calculated through the neighbourhood chunks
                /*
                if (calculatedPoints[x, z] == null)
                {
                    //first entry
                    if (x == 0 && z == 0)
                    {
                        //Debug.Log("First Entry");
                        if (RightZNoise != null || BotZNoise != null)
                        {
                            Vector3 botRight = calculatedPoints[SizeToGenerate-1, SizeToGenerate-1].Position;
                            Vector3 topLeft = new Vector3(botRight.x + (Range * (SizeToGenerate - 1)), CommonHeight, botRight.z + (Range * (SizeToGenerate - 1)));
                            calculatedPoints[x, z] = new Point(topLeft, 1);
                        }
                        else
                        {
                            calculatedPoints[x, z] = new Point(new Vector3(0, CommonHeight, 0), 1);
                        }
                    }

                    //first row in z direction
                    else if (x == 0 && z >= 1)
                    {
                        calculatedPoints[x, z] = new Point(calculatedPoints[x, z - 3].Position + rndKnotVector(EDirection.MinusZ), 1);
                    }
                    //first row in x direction
                    else if (x >= 1 && z == 0)
                    {
                        calculatedPoints[x, z] = new Point(calculatedPoints[x - 3, z].Position + rndKnotVector(EDirection.MinusX), 1);
                    }
                    //all other rows
                    else if (x >= 1 && z >= 1)
                    {
                        Vector3 a = rndKnotVector(EDirection.MinusX);
                        Vector3 b = rndKnotVector(EDirection.MinusZ);
                        //Vector3 temp = new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
                        calculatedPoints[x, z] = new Point(calculatedPoints[x - 3, z - 3].Position + b + a, 1);
                    }
                }
                */ // es gibt kein höhenfeld
                int xKey = ((Positionkey.x * (SizeToGenerate-1)) - (x)) * Range;
                int zKey = ((Positionkey.z * (SizeToGenerate-1)) - (z)) * Range;
                float yVal = Noise.GetValue(xKey, zKey);
                //Debug.Log(xKey + " - " +  yVal + " - " + zKey);
                calculatedPoints[x, z] = new Point(new Vector3(xKey, yVal, zKey), 1);
            }
        }
    }

    /// <summary>
    /// calculates the points near the end so it goes with the natural end condition
    /// </summary>
    public void CalculateNaturalEndCondition()
    {
        // now create thePoints for the natural end condition
        for (int z = 0; z < SizeToGenerate; z += 3)
        {
            if (calculatedPoints[1, z] == null)
            {
                calculatedPoints[1, z] = (calculatedPoints[0, z] + calculatedPoints[3, z]) * 0.5f;
                calculatedPoints[1, z].Weight = 1;
            }
        }
        for (int z = 0; z < SizeToGenerate; z += 3)
        {
            if(calculatedPoints[SizeToGenerate - 2, z] == null)
            {
                calculatedPoints[SizeToGenerate - 2, z] = (calculatedPoints[SizeToGenerate - 1, z] + calculatedPoints[SizeToGenerate - 4, z]) * 0.5f;
                calculatedPoints[SizeToGenerate - 2, z].Weight = 1;
            }
        }
        for (int x = 0; x < SizeToGenerate; x += 3)
        {
            if(calculatedPoints[x, 1] == null)
            {
                calculatedPoints[x, 1] = (calculatedPoints[x, 0] + calculatedPoints[x, 3]) * 0.5f;
                calculatedPoints[x, 1].Weight = 1;
            }
        }
        for (int x = 0; x < SizeToGenerate; x += 3)
        {
            if(calculatedPoints[x, SizeToGenerate - 2] == null)
            {
                calculatedPoints[x, SizeToGenerate - 2] = (calculatedPoints[x, SizeToGenerate - 1] + calculatedPoints[x, SizeToGenerate - 4]) * 0.5f;
                calculatedPoints[x, SizeToGenerate - 2].Weight = 1;
            }
        }
    }

    /// <summary>
    /// Calculates the twirstvectors
    /// </summary>
    public void CalculateTwirstVectors()
    {
        // now create the twist vectors
        if(calculatedPoints[1, 1] == null)
        {
            calculatedPoints[1, 1] = calculatedPoints[0, 0] + (calculatedPoints[1, 0] - calculatedPoints[0, 0]) + (calculatedPoints[0, 1] - calculatedPoints[0, 0]);
            calculatedPoints[1, 1].Weight = 1;
        }
        if(calculatedPoints[SizeToGenerate - 2, 1] == null)
        {
            calculatedPoints[SizeToGenerate - 2, 1] = calculatedPoints[SizeToGenerate - 1, 0] + (calculatedPoints[SizeToGenerate - 2, 0] - calculatedPoints[SizeToGenerate - 1, 0]) + (calculatedPoints[SizeToGenerate - 1, 1] - calculatedPoints[SizeToGenerate - 1, 0]);
            calculatedPoints[SizeToGenerate - 2, 1].Weight = 1;
        }
        if(calculatedPoints[1, SizeToGenerate - 2] == null)
        {
            calculatedPoints[1, SizeToGenerate - 2] = calculatedPoints[0, SizeToGenerate - 1] + (calculatedPoints[0, SizeToGenerate - 2] - calculatedPoints[0, SizeToGenerate - 1]) + (calculatedPoints[1, SizeToGenerate - 1] - calculatedPoints[0, SizeToGenerate - 1]);
            calculatedPoints[1, SizeToGenerate - 2].Weight = 1;
        }
        if(calculatedPoints[SizeToGenerate - 2, SizeToGenerate - 2] == null)
        {
            calculatedPoints[SizeToGenerate - 2, SizeToGenerate - 2] = calculatedPoints[SizeToGenerate - 1, SizeToGenerate - 1] + (calculatedPoints[SizeToGenerate - 1, SizeToGenerate - 2] - calculatedPoints[SizeToGenerate - 1, SizeToGenerate - 1]) + (calculatedPoints[SizeToGenerate - 2, SizeToGenerate - 1] - calculatedPoints[SizeToGenerate - 1, SizeToGenerate - 1]);
            calculatedPoints[SizeToGenerate - 2, SizeToGenerate - 2].Weight = 1;
        }
        
    }

    /// <summary>
    /// helper method, which calculates the missing bezierpoints if the knotvecors, the endcondition and the twirstvectors are calculated
    /// </summary>
    public void CalculateInBetweenPointsC2()
    {
        // first create the points in the rows we already hald filled:
        for (int x = 0; x < SizeToGenerate; x++)
        {
            for (int z = 0; z < SizeToGenerate; z++)
            {
                if (calculatedPoints[x, z] == null)
                {                
                if (x % 3 == 0 || z % 3 == 0 || x == 1 || z == 1 || x == SizeToGenerate - 2 || z == SizeToGenerate - 2)
                    {
                    bool set = false;
                        // orthogonal
                        if (z >= 2 && z <= SizeToGenerate - 4)
                        {
                            if (!set && calculatedPoints[x, z + 1] != null && calculatedPoints[x, z - 2] != null && calculatedPoints[x, z + 4] != null)
                           {
                                bool set1 = false;
                                bool set2 = false;
                                Vector3 v1 = calculatedPoints[x, z + 1].Position - calculatedPoints[x, z - 2].Position;
                                Vector3 v2 = calculatedPoints[x, z + 1].Position - calculatedPoints[x, z + 4].Position;
                                Vector3 t1 = calculatedPoints[x, z - 2].Position + (v1 * 2 / 3);
                                Vector3 t2 = calculatedPoints[x, z + 4].Position + (v2 * 2 / 3);
                                Vector3 t = t2 - t1;
                                calculatedPoints[x, z] = new Point();
                                calculatedPoints[x, z].Position = calculatedPoints[x, z + 1].Position - (t / 2);
                                set1 = true;
                                calculatedPoints[x, z + 2] = new Point();
                                calculatedPoints[x, z + 2].Position = calculatedPoints[x, z + 1].Position + (t / 2);
                                set2 = true;
                                if(set1 && set2)
                                {
                                    set = true;
                                }                            
                            }
                        }
                        // vertical
                        if (x >= 2 && x <= SizeToGenerate - 4)
                        {
                            if (!set && calculatedPoints[x + 1, z] != null && calculatedPoints[x - 2, z] != null && calculatedPoints[x + 4, z] != null)
                            {
                                Vector3 v1 = calculatedPoints[x + 1, z].Position - calculatedPoints[x - 2, z].Position;
                                Vector3 v2 = calculatedPoints[x + 1, z].Position - calculatedPoints[x + 4, z].Position;
                                Vector3 t1 = calculatedPoints[x - 2, z].Position + (v1 * 2 / 3);
                                Vector3 t2 = calculatedPoints[x + 4, z].Position + (v2 * 2 / 3);
                                Vector3 t = t2 - t1;
                                calculatedPoints[x, z] = new Point();
                                calculatedPoints[x, z].Position = calculatedPoints[x + 1, z].Position - (t / 2);
                                calculatedPoints[x + 2, z] = new Point();
                                calculatedPoints[x + 2, z].Position = calculatedPoints[x + 1, z].Position + (t / 2);
                            }
                        }
                        if (!set)
                        {
                            //Debug.Log("Couldn´t calculate c2 control points first try at X: " + x + " Z: " + z);
                        }
                    }                        
                }
            }
        }
        //Debug.Log("After first Try");
        //Debug.Log(ToString());
        // now the rows and columns just 1 index away from boundary
        // now create the other points with g1 continuity (for the beginning) mabey need some rework
        for (int x = 0; x < SizeToGenerate; x++)
        {
            for (int z = 0; z < SizeToGenerate; z++)
            {
                if (calculatedPoints[x, z] == null)
                {
                    bool set = false;
                    // orthogonal
                    if (z >= 2 && z <= SizeToGenerate - 4)
                    {
                        if (!set && calculatedPoints[x, z + 1] != null && calculatedPoints[x, z - 2] != null && calculatedPoints[x, z + 4] != null)
                        {
                            Vector3 v1 = calculatedPoints[x, z + 1].Position - calculatedPoints[x, z - 2].Position;
                            Vector3 v2 = calculatedPoints[x, z + 1].Position - calculatedPoints[x, z + 4].Position;
                            Vector3 t1 = calculatedPoints[x, z - 2].Position + (v1 * 2 / 3);
                            Vector3 t2 = calculatedPoints[x, z + 4].Position + (v2 * 2 / 3);
                            Vector3 t = t2 - t1;
                            calculatedPoints[x, z] = new Point();
                            calculatedPoints[x, z].Position = calculatedPoints[x, z + 1].Position - (t / 2);
                            calculatedPoints[x, z + 2] = new Point();
                            calculatedPoints[x, z + 2].Position = calculatedPoints[x, z + 1].Position + (t / 2);
                            set = true;
                        }
                    }
                    // vertical
                    if (x >= 2 && x <= SizeToGenerate - 4)
                    {
                        if (!set && calculatedPoints[x + 1, z] != null && calculatedPoints[x - 2, z] != null && calculatedPoints[x + 4, z] != null)
                        {
                            Vector3 v1 = calculatedPoints[x + 1, z].Position - calculatedPoints[x - 2, z].Position;
                            Vector3 v2 = calculatedPoints[x + 1, z].Position - calculatedPoints[x + 4, z].Position;
                            Vector3 t1 = calculatedPoints[x - 2, z].Position + (v1 * 2 / 3);
                            Vector3 t2 = calculatedPoints[x + 4, z].Position + (v2 * 2 / 3);
                            Vector3 t = t2 - t1;
                            calculatedPoints[x, z] = new Point();
                            calculatedPoints[x, z].Position = calculatedPoints[x + 1, z].Position - (t / 2);
                            calculatedPoints[x + 2, z] = new Point();
                            calculatedPoints[x + 2, z].Position = calculatedPoints[x + 1, z].Position + (t / 2);
                            set = true;
                        }
                    }
                    if (!set)
                    {
                        //Debug.Log("Couldn´t calculate c2 control points second try at X: " + x + " Z: " + z);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns a vector, in the direction, asked for, with perspective to the maxSteepnes and scaled with the Range*3 because its every third point (knotvector)
    /// </summary>
    /// <returns>returns a Point, which is in the direction</returns>
    private Vector3 rndKnotVector(EDirection direction)
    {
        Debug.Log("Don´t use this method!");
        
        Vector3 value = new Vector3();
        /*
        Vector2 temp = new Vector2();
        float maxSteep = Mathf.Sin(Mathf.Deg2Rad * PositiveSteepnes);
        float minSteep = Mathf.Sin(Mathf.Deg2Rad * NegativeSteepnes);
        float tempSteepness = rnd.Next(maxSteep, -minSteep);
        float maxOverhang = Mathf.Sin(Mathf.Deg2Rad * Overhang);
        float tempOverhang = rnd.Next((-maxOverhang)/10, 1);
        //Debug.Log("Max Overhang: " + maxOverhang + " TempOverhang: " + tempOverhang);
        if((float)rnd.NextDouble() >= OverhangRatio)
        {
            tempOverhang = 1;
        }
        switch (direction)
        {
            case EDirection.MinusX:
                temp = new Vector2(-1 * tempOverhang, tempSteepness);
                temp = temp.normalized * Range*3;
                value.x = temp.x;
                value.y = temp.y;
                break;
            case EDirection.PlusZ:
                temp = new Vector2(1 * tempOverhang, tempSteepness);
                temp = temp.normalized * Range*3;
                value.z = temp.x;
                value.y = temp.y;
                break;
            case EDirection.PlusX:
                temp = new Vector2(1 * tempOverhang, tempSteepness);
                temp = temp.normalized * Range*3;
                value.x = temp.x;
                value.y = temp.y;
                break;
            case EDirection.MinusZ:
                temp = new Vector2(-1 * tempOverhang, tempSteepness);
                temp = temp.normalized * Range*3;
                value.z = temp.x;
                value.y = temp.y;
                break;
        }
        */
        return value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>a string which contains a 2d map, which shows, which bezierpoints are calculated</returns>
    public override string ToString()
    {
        //Debug.Log(SizeToGenerate);
        //Debug.Log(calculatedPoints.LongLength);
        string temp = "";
        if(calculatedPoints == null)
        {
            temp += "Calculated Points 2dArray is not initialized, it should have a size of: " + SizeToGenerate;
        }
        else
        {
            for (int i = 0; i < SizeToGenerate; i++)
            {
                for (int j = 0; j < SizeToGenerate; j++)
                {
                    if (calculatedPoints[i, j] == null)
                    {
                        temp += "#";
                    }
                    else
                    {
                        if (calculatedPoints[i, j].Position != null)
                        {
                            //temp += "X: " + i + " Z: " + j + "Y: " + calculatedPoints[i, j].Position.y + "\n";
                            temp += "+";
                        }
                    }
                }
                temp += "\n";
            }
        }
        return temp;
    }
}

