using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(fileName = "New Level Tile" , menuName = "2D/Tiles/Level Tile")]
public class LevelTile : AnimatedTile
{
    public TileType type;
}
