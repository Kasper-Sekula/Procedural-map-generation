using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
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

    private Queue<MapThreadInfo<MapData>> _mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    private Queue<MapThreadInfo<MeshData>> _meshThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public bool AutoUpdate
    {
        get => _autoUpdate;
        set => _autoUpdate = value;
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData();
        float[,] heightMap = mapData.heightMap;
        Color[] colorMap = mapData.colorMap;

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (_drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        }
        else if (_drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, MAPCHUNKSIZE, MAPCHUNKSIZE));
        }
        else if (_drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap, _meshHeightMultiplier, _meshHeightCurve, _levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, MAPCHUNKSIZE, MAPCHUNKSIZE));
        }
    }

    public void RequestMapData(Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callback);
        };

        new Thread(threadStart).Start();
    }

    private void MapDataThread(Action<MapData> callback)
    {
        MapData mapData = GenerateMapData();
        lock (_mapDataThreadInfoQueue)
        {
            _mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, callback);
        };

        new Thread(threadStart).Start();
    }

    private void MeshDataThread(MapData mapData, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, _meshHeightMultiplier, _meshHeightCurve, _levelOfDetail);
        lock (_meshThreadInfoQueue)
        {
            _meshThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        if (_mapDataThreadInfoQueue.Count > 0)
        {
            for (int i=0; i < _mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = _mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }


        if (_meshThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < _meshThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = _meshThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    private MapData GenerateMapData()
    {
        float[,] noiseMap = Noise.GenerateNoiceMap(MAPCHUNKSIZE, MAPCHUNKSIZE, _seed, _noiseScale, _octaves, _persistance, _lacunarity, _offset);

        Color[] colorMap = new Color[MAPCHUNKSIZE * MAPCHUNKSIZE];

        for (int y = 0; y < MAPCHUNKSIZE; y++)
        {
            for (int x = 0; x < MAPCHUNKSIZE; x++)
            {
                float currentHeigth = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeigth <= regions[i].Height)
                    {
                        colorMap[y * MAPCHUNKSIZE + x] = regions[i].Color;
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);
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

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
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

public struct MapData
{
    public readonly float[,] heightMap;

    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
