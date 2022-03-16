using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh
    }
    [SerializeField] private DrawMode _drawMode;

    public const int MAPCHUNKSIZE = 241;

    [Range(0,6)]
    [SerializeField] private int _levelOfDetail;

    [SerializeField] private float _noiseScale;
    [SerializeField] private int _octaves;
    [Range(0,1)]
    [SerializeField] private float _persistance;
    [SerializeField] private float _lacunarity;
    [SerializeField] private int _seed;
    [SerializeField] private Vector2 _offset;

    [SerializeField] private float _meshHeightMultiplier;
    [SerializeField] private AnimationCurve _meshHeightCurve;

    [SerializeField] private bool _autoUpdate;
    [SerializeField] private TerrainType[] regions;

    public bool AutoUpdate
    {
        get => _autoUpdate;
        set => _autoUpdate = value;
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiceMap(MAPCHUNKSIZE, MAPCHUNKSIZE, _seed, _noiseScale, _octaves, _persistance, _lacunarity, _offset);

        Color[] colorMap = new Color[MAPCHUNKSIZE * MAPCHUNKSIZE];

        for (int y = 0; y < MAPCHUNKSIZE; y++)
        {
            for (int x = 0; x <MAPCHUNKSIZE; x++)
            {
                float currentHeigth = noiseMap[x, y];
                for (int i=0; i < regions.Length; i++)
                {
                    if (currentHeigth <= regions[i].Height)
                    {
                        colorMap[y * MAPCHUNKSIZE + x] = regions[i].Color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (_drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }else if (_drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, MAPCHUNKSIZE, MAPCHUNKSIZE));
        }else if (_drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, _meshHeightMultiplier, _meshHeightCurve, _levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, MAPCHUNKSIZE, MAPCHUNKSIZE));
        }
    }

    private void OnValidate()
    {
        if (_lacunarity < 1)
        {
            _lacunarity = 1;
        }

        if (_octaves < 0)
        {
            _octaves = 0;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    [SerializeField] private string _name;

    [SerializeField] private float _height;
    public float Height => _height;

    [SerializeField] private Color _color;
    public Color Color => _color;
}
