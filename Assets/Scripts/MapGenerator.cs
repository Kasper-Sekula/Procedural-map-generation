using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int _mapWidth;
    [SerializeField] private int _mapHeight;
    [SerializeField] private float _noiseScale;

    [SerializeField] private bool _autoUpdate;
    public bool AutoUpdate
    {
        get => _autoUpdate;
        set => _autoUpdate = value;
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateVoiceMap(_mapWidth, _mapHeight, _noiseScale);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
    }
}
