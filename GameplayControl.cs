using Godot;
using System;

public partial class GameplayControl : Control
{
	[Export] 
	private MapGen mapGen;
	[Export]
	private Player player;
	[Export]
	private GamePlay gamePlay;
	const int MainLayer = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _GuiInput(InputEvent @event)
    {
        // Process mouse event Leftclick on gameplay screen
		if (@event.IsActionPressed("leftClick"))
		{
			// Converting global pixel coordinates to coordinates on the MapGen node then converting to the Hex coordinates of MapGen
			var globalClicked = GetGlobalMousePosition();
			var posClicked = mapGen.LocalToMap(mapGen.ToLocal(globalClicked));
			//GD.Print("TileMap: " + posClicked.ToString());
			// Atlas coordinates are the tile's coordinates on the atlas the tilemap is pulling tiles from
			var currentAtlasCoords = mapGen.GetCellAtlasCoords(MainLayer, posClicked);
			//GD.Print("Atlas: " + currentAtlasCoords.ToString());

			//-------------------------------------------------Movement Phase-----------------------------------------------------------
			//if player phase is movement
			if (currentAtlasCoords is (-1, -1) && mapGen.GetSurroundingCells(player.playerPos).Contains(posClicked)) // No tile from atlas exists here and adjacent to player
			{
				mapGen.GenerateTile(currentAtlasCoords, posClicked);
				Utils.undoRedo.ClearHistory();
			}
			// Check if tile clicked is any tiles surrounding player position
			else if (mapGen.GetSurroundingCells(player.playerPos).Contains(posClicked))
			{
				// Check if next to any enemy
				foreach (var enemy in gamePlay.EnemyList)
				{
					// Enemy adjacent and moving to another tile adjacent to enemy
					if ((mapGen.GetSurroundingCells(player.playerPos).Contains(enemy.MapPosition) && mapGen.GetSurroundingCells(enemy.MapPosition).Contains(posClicked)
					&& (enemy.Colour == "green" || enemy.Colour == "red") && player.IsWallBetween(posClicked, enemy.MapPosition) == false) || (enemy.MapPosition == posClicked))
					{
						player.InitiateCombat();
					}
					// Need another else if for if enemy is facedown and time of day
				}
				var cellTerrain = mapGen.GetCellTileData(MainLayer, posClicked).Terrain; // Get terrain of tile
				// GD.Print("Terrain: " + mapGen.TileSet.GetTerrainName(MainTerrainSet, cellTerrain));
				GD.Print("Wall is between: " + player.IsWallBetween(player.playerPos, posClicked).ToString());

				if (player.MovePoints >= mapGen.terrainCosts[cellTerrain])
				{
					player.PerformMovement(posClicked, cellTerrain);
				}

				else
				{
					GD.Print("Terrain costs more movement than you currently have.");
				}
			}

    	}
	}
}