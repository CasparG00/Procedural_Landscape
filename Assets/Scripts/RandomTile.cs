using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomTile : Tile
{
    public Sprite[] tiles;
    
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        var index = Random.Range(0, tiles.Length);
        tileData.sprite = tiles[index];
        tileData.colliderType = ColliderType.Grid;
    }
    
    [MenuItem("Assets/Create/RandomTile")]
    public static void CreateRandomTile()
    {
        var path = EditorUtility.SaveFilePanelInProject("Save Random Tile", "New Random Tile", "Asset", "Save Random Tile", "Assets");
        if (path == "") return;
        AssetDatabase.CreateAsset(CreateInstance<RandomTile>(), path);
    }
}
