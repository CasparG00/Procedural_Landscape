using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Min(1)] public int width = 100;
    [Min(1)] public int height = 100;

    public int borderSize = 5;
    
    public int seed;
    public bool randomizeSeed;

    [Range(0, 1)] public float fillPercent;

    [Min(0.001f)] public float scale = 10;

    [Min(0)] public int octaves = 1;
    [Range(0, 1)] public float persistence;
    [Min(1)] public float lacunarity;
    
    private int[,] map;

    void Start()
    {
        if (randomizeSeed)
        {
            seed = System.DateTime.Now.GetHashCode();
        }
        
        GenerateMap();
        FillMap();
    }
    
    void GenerateMap()
    {
        map = new int[width, height];
    }

    void FillMap()
    {
        //Generate offset based on seed
        var pseudoRandom = new System.Random(seed);

        var octaveOffsets = new Vector2[octaves];
        for (var i = 0; i < octaveOffsets.Length; i++)
        {
            var offsetX = pseudoRandom.Next(-100000, 100000);
            var offsetY = pseudoRandom.Next(-100000, 100000);

            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        var maxNoiseHeight = float.MinValue;
        var minNoiseHeight = float.MaxValue;
        
        //iterate through every point on the map
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var amplitude = 1f;
                var frequency = 1f;
                var noiseHeight = 0f;

                //Layer noise with octaves
                for (var i = 0; i < octaves; i++)
                {
                    var perlinX = (float) x / width * scale * frequency + octaveOffsets[i].x;
                    var perlinY = (float) y / height * scale * frequency + octaveOffsets[i].y;

                    var perlinValue = Mathf.PerlinNoise(perlinX, perlinY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                //Clamp the values between 0 and 1
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                } else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                
                var sample = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseHeight);

                //Add a border around the edges
                //by interpolating the edges will be less noticeable
                if (x < borderSize)
                {
                    var t = (float)x / borderSize;
                
                    var addition = Mathf.Lerp(fillPercent, 0, t);
                    sample += addition;
                }
                
                if (x > width - borderSize)
                {
                    var t = (float)(width - x) / borderSize;
                
                    var addition = Mathf.Lerp(fillPercent, 0, t);
                    sample += addition;
                }

                if (y < borderSize)
                {
                    var t = (float)y / borderSize;
                
                    var addition = Mathf.Lerp(fillPercent, 0, t);
                    sample += addition;
                }
                
                if (y > height - borderSize)
                {
                    var t = (float) (height - y) / borderSize;
                
                    var addition = Mathf.Lerp(fillPercent, 0, t);
                    sample += addition;
                }
                
                //normalize the values
                map[x, y] = sample < fillPercent ? 0 : 1;
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (map == null) return;
        
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                Gizmos.color = map[x, y] == 0 ? Color.white : Color.black;
                var pos = new Vector3(-width * 0.5f + x + 0.5f, -height*0.5f + y + 0.5f, 0);
                Gizmos.DrawCube(pos, Vector3.one);
            }
        }
    }
}
