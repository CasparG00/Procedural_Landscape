using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("Base Settings")]
    [Min(1)] public int width = 64;
    [Min(1)] public int height = 64;

    public int borderSize = 5;

    [Header("Noise Settings")]
    public string seed;
    public bool randomizeSeed;

    [Min(0.001f)] public float scale = 10;

    [Min(0)] public int octaves = 1;
    [Range(0, 1)] public float persistence;
    [Min(1)] public float lacunarity;

    [Header("River Settings")] 
    [Min(0.001f)] public float riverScale = 2;
    [Min(0)] public float riverSize = 2;
    
    [Header("Tile Settings")]
    public Tilemap tilemap;
    public TerrainType[] regions;

    private float[,] map;
    private System.Random pseudoRandom;

    private void Start()
    {
        if (randomizeSeed)
        {
            seed = System.DateTime.Now.ToString();
        }
        
        SetMap();
        GenerateMap();
        GenerateRivers();
        SetRegions();
        TrimMap();
    }
    
    private void SetMap()
    {
        map = new float[width, height];
        pseudoRandom = new System.Random(seed.GetHashCode());
    }

    private void GenerateMap()
    {
        //Generate offset based on seed
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
                
                    var addition = Mathf.Lerp(1, 0, t);
                    sample += addition;
                }
                
                if (x > width - borderSize)
                {
                    var t = (float)(width - x) / borderSize;
                
                    var addition = Mathf.Lerp(1, 0, t);
                    sample += addition;
                }

                if (y < borderSize)
                {
                    var t = (float)y / borderSize;
                
                    var addition = Mathf.Lerp(1, 0, t);
                    sample += addition;
                }
                
                if (y > height - borderSize)
                {
                    var t = (float) (height - y) / borderSize;
                
                    var addition = Mathf.Lerp(1, 0, t);
                    sample += addition;
                }
                
                map[x, y] = sample;
            }
        }
    }

    private void SetRegions()
    {
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var value = map[x, y];
                
                var worldPos = new Vector3(-width * 0.5f + x, -height*0.5f + y, 0);
                var tilePos = tilemap.WorldToCell(worldPos);

                for (var i = 0; i < regions.Length; i++)
                {
                    if (!(value <= regions[i].height)) continue;
                    tilemap.SetTile(tilePos, regions[i].tile);
                    break;
                }
            }
        }
    }

    private void GenerateRivers()
    {
        var offsetX = pseudoRandom.Next(-100000, 100000);
        var offsetY = pseudoRandom.Next(-100000, 100000);
        
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var perlinX = (float) x / width * riverScale + offsetX;
                var perlinY = (float) y / height * riverScale + offsetY;

                var perlinValue = Mathf.PerlinNoise(perlinX, perlinY) * 2 - 1;
                perlinValue = Mathf.Abs(perlinValue);
                perlinValue *= riverSize;
                perlinValue = Mathf.Clamp01(perlinValue);

                map[x, y] *= perlinValue;
            }
        }
    }

    private Vector2 QuadraticCurve(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        var p0 = Vector2.Lerp(a, b, t);
        var p1 = Vector2.Lerp(b, c, t);
        
        return Vector2.Lerp(p0, p1, t);
    }
    
    private Vector2 CubicCurve(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        var p0 = QuadraticCurve(a, b, c, t);
        var p1 = QuadraticCurve(b, c, d, t);
        
        return Vector2.Lerp(p0, p1, t);
    }

    private void TrimMap()
    {
        tilemap.CompressBounds();
    }
    
    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Tile tile;
    }

    private void OnDrawGizmos()
    {
        if (map == null) return;
        
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var value = map[x, y];
                Gizmos.color = new Color(value, value, value);
                var pos = new Vector3(-width * 0.5f + x + 0.5f, -height*0.5f + y + 0.5f, 0);
                Gizmos.DrawCube(pos, Vector3.one);
            }
        }
    }
}
