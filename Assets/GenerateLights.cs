using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;

public class TorchLightGenerator : EditorWindow
{
    public Tilemap tilemap;
    public GameObject lightPrefab;
    public string torchTileName = "TorchTile"; // Name of the torch tile

    [MenuItem("Tools/Generate Torch Lights")]
    public static void ShowWindow()
    {
        GetWindow<TorchLightGenerator>("Generate Torch Lights");
    }

    private void OnGUI()
    {
        GUILayout.Label("Torch Light Generator", EditorStyles.boldLabel);

        tilemap = (Tilemap)EditorGUILayout.ObjectField("Tilemap", tilemap, typeof(Tilemap), true);
        lightPrefab = (GameObject)EditorGUILayout.ObjectField("Light Prefab", lightPrefab, typeof(GameObject), false);
        torchTileName = EditorGUILayout.TextField("Torch Tile Name", torchTileName);

        if (GUILayout.Button("Generate Lights"))
        {
            if (tilemap == null || lightPrefab == null)
            {
                Debug.LogError("Tilemap and Light Prefab must be assigned!");
                return;
            }

            GenerateLights();
        }
    }

    private void GenerateLights()
    {
        int count = 0;

        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(position);
            if (tile != null && tile.name == torchTileName)
            {
                Vector3 worldPosition = tilemap.CellToWorld(position) + new Vector3(0.5f, 0.5f, 0); // Center the light in the tile
                GameObject lightInstance = (GameObject)PrefabUtility.InstantiatePrefab(lightPrefab, tilemap.transform);
                lightInstance.transform.position = worldPosition;
                count++;
            }
        }

        Debug.Log($"Generated {count} torch lights.");
    }
}
