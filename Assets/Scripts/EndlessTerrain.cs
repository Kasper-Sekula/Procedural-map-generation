using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    [SerializeField] private const float MAXVIEWDISTANCE = 300;
    [SerializeField] private Transform _viewer;

    [SerializeField] private Material _mapMaterial;

    [SerializeField] private static Vector2 _viewerPosition;
    private static MapGenerator _mapGenerator;
    public static Vector2 ViewerPosition => _viewerPosition;

    private int chunkSize;
    private int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        _mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.MAPCHUNKSIZE - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(MAXVIEWDISTANCE / chunkSize);
    }

    private void Update()
    {
        _viewerPosition = new Vector2(_viewer.position.x, _viewer.position.z);
        UpdateVisibleChunks();
    }

    private void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(_viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(_viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey (viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, _mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk
    {
        private GameObject _meshObject;
        private Vector2 _position;
        private Bounds _bounds;

        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
        {
            _position = coord * size;
            _bounds = new Bounds(_position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(_position.x, 0, _position.y);

            _meshObject = new GameObject("Terrain Chunk");
            _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
            _meshFilter = _meshObject.AddComponent<MeshFilter>();
            _meshRenderer.material = material;

            _meshObject.transform.position = positionV3;
            _meshObject.transform.parent = parent;
            SetVisible(false);

            _mapGenerator.RequestMapData(OnMapDataReceived);
        }

        private void OnMapDataReceived(MapData mapData)
        {
            _mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }

        private void OnMeshDataReceived(MeshData meshData)
        {
            _meshFilter.mesh = meshData.CreateMesh();
        }

        public void UpdateTerrainChunk()
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(ViewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= MAXVIEWDISTANCE;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            _meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return _meshObject.activeSelf;
        }
    }
}
