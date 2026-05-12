using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapRegistry : MonoBehaviour
{
    [SerializeField] private GameObject tilemapsRoot;
    [SerializeField] private Tilemap[] tilemaps;

    public GameObject TilemapsRoot => tilemapsRoot;
    public Tilemap[] Tilemaps => tilemaps;
}

