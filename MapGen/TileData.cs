using Godot;
using System;
using System.Collections.Generic;

public class TileData
{
    public int CellTerrain { get; set; }
    public MapToken Token{ get; set; }
    public string TileEvent{ get; set; }
    public List<MapToken> MonsterGroup{ get; set; }

    public TileData(int cellTerrain, MapToken mapToken, string tileEvent, List<MapToken> monsterGroup)
    {
        CellTerrain = cellTerrain;
        Token = mapToken;
        TileEvent = tileEvent;
        MonsterGroup = monsterGroup;
        GD.Print("Terrain is " + CellTerrain.ToString());
        GD.Print("Token is " + Token.TokenId);
        GD.Print("Event is " + TileEvent);
        GD.Print("MonsterGroup first id should be " + MonsterGroup.ToString());
    }

}