using Godot;
using System;
using System.Linq;

public partial class Player : Node2D
{
	const int MainLayer = 0;
	const int MainTerrainSet = 0;
	Vector2I playerPos = new Vector2I(0, 0);
	private int movePoints = 20;

	public int MovePoints { get => movePoints; set => movePoints = value; }

	public override void _Ready()
	{
		var mapGen = GetNode<MapGen>("../MapGen");
		//GD.Print(mapGen.ToGlobal(mapGen.MapToLocal(new Vector2I(0,0))));
		GlobalPosition = mapGen.ToGlobal(mapGen.MapToLocal(playerPos));
	}
	public override void _Input(InputEvent @event)
	{
		var mapGen = GetNode<MapGen>("../MapGen");
		// Called on Input Event. For now, should only process mouse event Leftclick
		if (@event is InputEventMouseButton eventMouseButton)
		{
			if (eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
			{
				// Converting global pixel coordinates to coordinates on the MapGen node then converting to the Hex coordinates of MapGen
				var globalClicked = GetGlobalMousePosition();
				var posClicked = mapGen.LocalToMap(mapGen.ToLocal(globalClicked));
				//GD.Print("TileMap: " + posClicked.ToString());
				// Atlas coordinates are the tile's coordinates on the atlas the tilemap is pulling tiles from
				var currentAtlasCoords = mapGen.GetCellAtlasCoords(MainLayer, posClicked);
				//GD.Print("Atlas: " + currentAtlasCoords.ToString());

				if (currentAtlasCoords is (-1, -1)) // No tile from atlas exists here
				{
					mapGen.GenerateTile(currentAtlasCoords, posClicked);
				}

				else
				{
					// Check if tile clicked is any tiles surrounding player position
					if (mapGen.GetSurroundingCells(playerPos).Contains(posClicked))
					{
						var cellTerrain = mapGen.GetCellTileData(MainLayer, posClicked).Terrain; // Get terrain of tile
						GD.Print("Terrain: " + mapGen.TileSet.GetTerrainName(MainTerrainSet, cellTerrain));

						if (MovePoints >= mapGen.terrainCosts[cellTerrain])
						{
							// Change position of player, update position vector
							GlobalPosition = mapGen.ToGlobal(mapGen.MapToLocal(posClicked));
							playerPos = posClicked;
							MovePoints -= (int)mapGen.terrainCosts[cellTerrain]; // Reduce move points
						}

						else
						{
							GD.Print("Terrain costs more movement than you currently have.");
						}
					}
				}
			}
		}
	}
}
