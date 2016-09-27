using LibNoise.Generator;

public class NoiseProvider : INoiseProvider
{
    private Perlin PerlinNoiseGenerator;

    public NoiseProvider()
    {
        PerlinNoiseGenerator = new Perlin();
    }

    public float GetValue(float x, float z)
    {
        return (float)(PerlinNoiseGenerator.GetValue(x/50, 0d, z/50) / 2f) + 0.5f;
    }
}
