using Godot;
using System;
using System.Linq;

public partial class Player : Node2D
{
	const int MainLayer = 0;
	const int MainTerrainSet = 0;
	Vector2I playerPos = new Vector2I(0, 0);
	private int movePoints = 100;

	public int MovePoints { get => movePoints; set => movePoints = value; }
	MapGen mapGen;

	public override void _Ready()
	{
		mapGen = GetNode<MapGen>("../MapGen");
		//GD.Print(mapGen.ToGlobal(mapGen.MapToLocal(new Vector2I(0,0))));
		GlobalPosition = mapGen.ToGlobal(mapGen.MapToLocal(playerPos));
	}
	public override void _Input(InputEvent @event)
	{
		// Called on Input Event. For now, should only process mouse event Leftclick
		if (@event.IsActionPressed("leftClick"))
		{
			//GD.Print("CLICK");
			// Converting global pixel coordinates to coordinates on the MapGen node then converting to the Hex coordinates of MapGen
			var globalClicked = GetGlobalMousePosition();
			var posClicked = mapGen.LocalToMap(mapGen.ToLocal(globalClicked));
			//GD.Print("TileMap: " + posClicked.ToString());
			// Atlas coordinates are the tile's coordinates on the atlas the tilemap is pulling tiles from
			var currentAtlasCoords = mapGen.GetCellAtlasCoords(MainLayer, posClicked);
			//GD.Print("Atlas: " + currentAtlasCoords.ToString());

			if (currentAtlasCoords is (-1, -1) && mapGen.GetSurroundingCells(playerPos).Contains(posClicked)) // No tile from atlas exists here and adjacent to player
			{
				mapGen.GenerateTile(currentAtlasCoords, posClicked);
			}

			else
			{
				// Check if tile clicked is any tiles surrounding player position
				if (mapGen.GetSurroundingCells(playerPos).Contains(posClicked))
				{	
					// Check if next to any enemy
					var gamePlay = GetNode<GamePlay>("..");
					foreach (var enemy in gamePlay.EnemyList)
					{	
						// Enemy adjacent and moving to another tile adjacent to enemy
						if (mapGen.GetSurroundingCells(playerPos).Contains(enemy.MapPosition) && mapGen.GetSurroundingCells(enemy.MapPosition).Contains(posClicked)
						&& (enemy.Colour == "green" || enemy.Colour == "red"))
						{
							InitiateCombat();
						}
						// Need another else if for if enemy is facedown and time of day
					}
					var cellTerrain = mapGen.GetCellTileData(MainLayer, posClicked).Terrain; // Get terrain of tile
					// GD.Print("Terrain: " + mapGen.TileSet.GetTerrainName(MainTerrainSet, cellTerrain));
					GD.Print("Wall is between: " + IsWallBetween(posClicked).ToString());

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

	private void InitiateCombat()
	{
		GD.Print("Combat Start!");
		// instantiate Combat scene
		// set player level
		// set enemies
	}

	// Check walls unique data in current tile and iterate. if Destination vector - Any Walls vector == Source vector, then wall between
	private bool IsWallBetween(Vector2I Destination)
	{
		// Get array of walls in current player tile
		var wallArray = mapGen.GetCellTileData(MainLayer, playerPos).GetCustomData("Walls").As<Vector2[]>();
		foreach (var wall in wallArray)
		{
			if (Destination - wall == playerPos)
			{
				return true;
			}
		}
		return false;
	}
}
