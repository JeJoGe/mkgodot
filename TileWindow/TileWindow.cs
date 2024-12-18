using Godot;
using System;
<<<<<<< HEAD
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
=======

public partial class TileWindow : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
>>>>>>> f21002d9f6a9c8e4fb7b5dfe5dc95dc0632a915d
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
