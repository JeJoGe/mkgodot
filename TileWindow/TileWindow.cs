using Godot;
using System;
using System.Collections.Generic;

public partial class TileWindow : Node2D
{
	public int CellTerrain { get; set; }
    public MapToken Token{ get; set; }
    public string TileEvent{ get; set; }
    public List<MapToken> MonsterGroup{ get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var terrainEnum = (MapGen.Terrain)CellTerrain;
		GetNode<Label>("TileWindowControl/TerrainText").Text = terrainEnum.ToString();
		GetNode<Label>("TileWindowControl/ActionsText").Text = TileEvent;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
