using Godot;
using System;
using System.Collections.Generic;

public partial class GameplayControl : Control
{
	[Export]
	private MapGen mapGen;
	[Export]
	private Player player;
	[Export]
	private GamePlay gamePlay;
	const int MainLayer = 0;
	const int MainTerrainSet = 0;
	PackedScene monsterScene = GD.Load<PackedScene>("res://MapToken.tscn");
	public List<MapToken> EnemyList = new List<MapToken>();
	private static readonly int _monsterSpriteSize = 258;

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
				var cellTerrain = mapGen.GetCellTileData(MainLayer, posClicked).Terrain; // Get terrain of tile
				// GD.Print("Terrain: " + mapGen.TileSet.GetTerrainName(MainTerrainSet, cellTerrain));
				if (player.MovePoints >= mapGen.terrainCosts[cellTerrain]) 
				{	
					var prevPos = player.playerPos;
					// Check if next to any enemy
					bool clickedRampage = false;
					foreach (var enemy in EnemyList)
					{
						if (posClicked == enemy.MapPosition && (enemy.Colour == "green" || enemy.Colour == "red"))
						{
							clickedRampage = true;
							break;
						}
						// Enemy adjacent and moving to another tile adjacent to enemy
						if ((mapGen.GetSurroundingCells(prevPos).Contains(enemy.MapPosition) && mapGen.GetSurroundingCells(enemy.MapPosition).Contains(posClicked)
						&& (enemy.Colour == "green" || enemy.Colour == "red") && player.IsWallBetween(posClicked, enemy.MapPosition) == false) || (enemy.MapPosition == posClicked))
						{
							var wallBetweenPlayerAndEnemy = player.IsWallBetween(player.playerPos, enemy.MapPosition);
							var monsterTerrain = mapGen.TileSet.GetTerrainName(MainTerrainSet, mapGen.GetCellTileData(MainLayer, enemy.MapPosition).Terrain);
							if (wallBetweenPlayerAndEnemy == true && (monsterTerrain == "tower" || monsterTerrain == "keep" || monsterTerrain.Contains("castle"))) // double fortified
							{
								GameSettings.EnemyList.Add(new Vector2I(enemy.MonsterId, 2));
							}
							else if (monsterTerrain == "tower" || monsterTerrain == "keep" || monsterTerrain.Contains("castle"))
							{
								GameSettings.EnemyList.Add(new Vector2I(enemy.MonsterId, 1));
							}
							else
							{
								GameSettings.EnemyList.Add(new Vector2I(enemy.MonsterId, 0));
							}
						}
						// Need another else if for if enemy is facedown and time of day
					}
					if (!clickedRampage)
					{
						player.PerformMovement(posClicked, cellTerrain);
					}
					//GD.Print("Wall is between: " + player.IsWallBetween(player.playerPos, posClicked).ToString());
				}

				else
				{
					GD.Print("Terrain costs more movement than you currently have.");
				}
			}

		}
	}
	// Generate Monster Token and stats, may need to add in variable for whether flipped
    public void MonsterGen(string colour, int siteFortifications, Vector2I localPos)
	{
		var enemy = GameSettings.DrawMonster(Utils.ConvertStringToMonsterColour(colour));
		var monsterToken = (MapToken)monsterScene.Instantiate();
		monsterToken.MapPosition = localPos;
		monsterToken.SiteFortifications = siteFortifications;
		monsterToken.Colour = colour;
		monsterToken.MonsterId = enemy;
		var monsterStats = Utils.Bestiary[enemy];
		var enemySprite = monsterToken.GetNode<Sprite2D>("MapTokenControl/Sprite2D");
		var atlas = (AtlasTexture)Utils.SpriteSheets[monsterToken.Colour].Duplicate();
		atlas.Region = new Rect2(new Vector2(monsterStats.X * _monsterSpriteSize,monsterStats.Y * _monsterSpriteSize),new Vector2(_monsterSpriteSize,_monsterSpriteSize));
		enemySprite.Texture = atlas;
		enemySprite.Scale = new Vector2((float)0.25,(float)0.25);
		monsterToken.GlobalPosition = mapGen.ToGlobal(mapGen.MapToLocal(localPos));
		AddChild(monsterToken);
		EnemyList.Add(monsterToken);
	}
}