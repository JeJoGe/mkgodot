using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class TileWindow : Node2D
{
	private int _cellTerrain{ get; set; }
	public int CellTerrain { get => _cellTerrain; set { _cellTerrain = value; 
	switch(value)
	{
		case 0:
			Terrain = "Plains";
			break;
		case 1:
			Terrain = "Forest";
			break;
		case 2:
			Terrain = "Hills";
			break;
		case 3:
			Terrain = "Swamp";
			break;
		case 4:
			Terrain = "Wasteland";
			break;
		case 5:
			Terrain = "Desert";
			break;
		case 6:
			Terrain = "City";
			break;
		case 7:
			Terrain = "Mountain";
			break;
		case 8:
			Terrain = "Lake";
			break;
		case 9:
			Terrain = "Nothing";
			break;
		default:
			Terrain = "Nowhere";
			break;
	}} }
	private string _terrain = "";
	public string Terrain { get => _terrain; set{ _terrain = value;}}
    public MapToken Token{ get; set; }
    public string TileEvent{ get; set; }
    public List<MapToken> MonsterGroup{ get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
