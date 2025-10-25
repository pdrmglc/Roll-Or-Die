using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "TileSet", menuName = "Scriptable Objects/TileSet")]
public class TileSet : ScriptableObject
{
    public List<TileBase> tiles;
}
