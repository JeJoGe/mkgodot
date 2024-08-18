using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class GameplayControl : Control
{
	[Export]
	private MapGen mapGen;
	[Export]
	private Player player;
	[Export]
	private GamePlay gamePlay;
	[Export]
	private Button challengeButton;
	[Export]
	private Button interactButton;
	PackedScene mapTokenScene = GD.Load<PackedScene>("res://MapToken.tscn");
	public List<MapToken> EnemyList = new List<MapToken>();
	public List<MapToken> RuinList = new List<MapToken>();
	PackedScene ChallengeScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		interactButton.Disabled = true;
		challengeButton.Disabled = true;
		ChallengeScene = GD.Load<PackedScene>("res://ChallengePopUp/ChallengeWindow.tscn");
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
			var currentAtlasCoords = mapGen.GetCellAtlasCoords(MapGen.MainLayer, posClicked);
			//GD.Print("Atlas: " + currentAtlasCoords.ToString());

			//-------------------------------------------------Movement Phase-----------------------------------------------------------
			//if player phase is movement
			if (currentAtlasCoords is (-1, -1) && mapGen.GetSurroundingCells(player.PlayerPos).Contains(posClicked)) // No tile from atlas exists here and adjacent to player
			{
				mapGen.GenerateTile(currentAtlasCoords, posClicked);
				foreach (var enemy in EnemyList)
				{
					if ((enemy.Colour == "green" || enemy.Colour == "red") && mapGen.GetSurroundingCells(player.PlayerPos).Contains(enemy.MapPosition))
					{
						if (player.besideRampage != true)
						{
							player.besideRampage = true;
							challengeButton.Disabled = false;
						}
					}
				}
				UpdateTokenColors(player.PlayerPos);
				Utils.undoRedo.ClearHistory();
			}
			// Check if tile clicked is any tiles surrounding player position
			else if (mapGen.GetSurroundingCells(player.PlayerPos).Contains(posClicked))
			{
				var cellTerrain = mapGen.GetCellTileData(MapGen.MainLayer, posClicked).Terrain; // Get terrain of tile
																						 // GD.Print("Terrain: " + mapGen.TileSet.GetTerrainName(MainTerrainSet, cellTerrain));
				var movementMod = 0;
				if (player.MovePoints >= mapGen.terrainCosts[cellTerrain])
				{
					// Check if next to any enemy
					bool clickedRampage = false;
					bool clickedBesideRampage = false;
					var mapEvent = mapGen.GetCellTileData(MapGen.MainLayer, posClicked).GetCustomData("Event").ToString();
					var mapToken = mapGen.GetCellTileData(MapGen.MainLayer, posClicked).GetCustomData("Token").ToString();
					foreach (var enemy in EnemyList)
					{

						if (posClicked == enemy.MapPosition && (enemy.Colour == "green" || enemy.Colour == "red"))
						{
							clickedRampage = true;
							break;
						}
						// Enemy adjacent and moving to another tile adjacent to enemy
						if ((mapGen.GetSurroundingCells(player.PlayerPos).Contains(enemy.MapPosition) && mapGen.GetSurroundingCells(enemy.MapPosition).Contains(posClicked)
						&& (enemy.Colour == "green" || enemy.Colour == "red") && player.IsWallBetween(posClicked, enemy.MapPosition) == false) || (enemy.MapPosition == posClicked))
						{
							UpdateTokenColors(posClicked);
							var wallBetweenPlayerAndEnemy = player.IsWallBetween(player.PlayerPos, enemy.MapPosition);
							var monsterTerrain = mapGen.TileSet.GetTerrainName(MapGen.MainTerrainSet, mapGen.GetCellTileData(MapGen.MainLayer, enemy.MapPosition).Terrain);
							if (wallBetweenPlayerAndEnemy == true && (mapEvent == "tower" || mapEvent == "keep" || mapEvent.Contains("city"))) // double fortified
							{
								GameSettings.EnemyList.Add((enemy.TokenId, 2, enemy.PosColour, enemy.OldPosColour));
							}
							else if (mapEvent == "tower" || mapEvent == "keep" || mapEvent.Contains("city"))
							{
								GameSettings.EnemyList.Add((enemy.TokenId, 1, enemy.PosColour, enemy.OldPosColour));
							}
							else
							{
								GameSettings.EnemyList.Add((enemy.TokenId, 0, enemy.PosColour, enemy.OldPosColour));
							}
						}
						// Need another else if for if enemy is facedown and time of day

						if (clickedBesideRampage == false && mapGen.GetSurroundingCells(posClicked).Contains(enemy.MapPosition) && (enemy.Colour == "green" || enemy.Colour == "red"))
						{
							clickedBesideRampage = true;
						}
					}
					if (!clickedRampage)
					{
						if (clickedBesideRampage == true && player.besideRampage != true)
						{
							player.besideRampage = true;
							challengeButton.Disabled = false;
						}
						else if (clickedBesideRampage == false && player.besideRampage != false)
						{
							player.besideRampage = false;
							challengeButton.Disabled = true;
						}
						if (mapEvent != "" && !mapEvent.Contains("mine") && mapEvent != "glade") // need to change later to check if magic familiars out
						{
							if (interactButton.Disabled == true) { interactButton.Disabled = false; }
						}
						else if (mapToken == "yellow")
						{
							foreach (var ruin in RuinList)
							{
								if (ruin.MapPosition == posClicked)
								{
									if (interactButton.Disabled == true) { interactButton.Disabled = false; }
									break;
								}
							}
						}
						else if (interactButton.Disabled == false)
						{
							interactButton.Disabled = true;
						}
						player.PerformMovement(posClicked, cellTerrain, movementMod);
						
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
		var monsterToken = (MapToken)mapTokenScene.Instantiate();
		monsterToken.MapPosition = localPos;
		monsterToken.SiteFortifications = siteFortifications;
		monsterToken.Colour = colour;
		monsterToken.TokenId = enemy;
		var mapEvent = mapGen.GetCellTileData(MapGen.MainLayer, monsterToken.MapPosition).GetCustomData("Event").ToString();
		if ((monsterToken.Colour == "green" || monsterToken.Colour == "red") || //rampaging
		(mapGen.GetSurroundingCells(player.PlayerPos).Contains(monsterToken.MapPosition) && mapEvent.Contains("city")) || // City monsters and next to
		(mapGen.GetSurroundingCells(player.PlayerPos).Contains(monsterToken.MapPosition) && !GameSettings.NightTime) // Next to keep or tower in day
		)
		{
			monsterToken.Facedown = false;
		}
		else
		{
			monsterToken.Facedown = true;
		}
		monsterToken.GlobalPosition = mapGen.ToGlobal(mapGen.MapToLocal(localPos));
		AddChild(monsterToken);
		EnemyList.Add(monsterToken);
	}

	// Generate Ruin Token, may need to add in variable for whether flipped
	public void RuinGen(Vector2I localPos)
	{
		var ruin = GameSettings.DrawRuin();
		var ruinToken = (MapToken)mapTokenScene.Instantiate();
		ruinToken.MapPosition = localPos;
		ruinToken.Colour = "yellow";
		ruinToken.TokenId = ruin;
		if (!GameSettings.NightTime)
		{
			ruinToken.Facedown = false;
		}
		else
		{
			ruinToken.Facedown = true;	
		}
		ruinToken.GlobalPosition = mapGen.ToGlobal(mapGen.MapToLocal(localPos));
		AddChild(ruinToken);
		RuinList.Add(ruinToken);
	}

	public void _on_challenge_button_pressed()
	{
		player.PerformMovement(player.PlayerPos, 0, (int)mapGen.terrainCosts[0]); // movement 0 so cancel on challenge undoes this move
		challengeEnemies();
	}

	public void challengeEnemies()
	{
		foreach (var enemy in EnemyList)
		{
			if ((enemy.Colour == "green" || enemy.Colour == "red") && mapGen.GetSurroundingCells(player.PlayerPos).Contains(enemy.MapPosition))
			{
				var already_fighting = false;
				foreach (var fight in GameSettings.EnemyList)
				{
					if (fight.Item3 == enemy.PosColour)
					{
						already_fighting = true;
					}
				}
				if (already_fighting)
				{
					continue;
				}
				else if (player.IsWallBetween(player.PlayerPos, enemy.MapPosition))
				{
					GameSettings.EnemyList.Add((enemy.TokenId, 1, enemy.PosColour, enemy.OldPosColour));
				}
				else
				{
					GameSettings.EnemyList.Add((enemy.TokenId, 0, enemy.PosColour, enemy.OldPosColour));
				}
			}
		}
		var ChallengeStart = (Window)ChallengeScene.Instantiate();
		AddChild(ChallengeStart);
		ChallengeStart.Position = new Godot.Vector2I(300, 500);
		//GetTree().Paused = true;
	}
	// Change the identification color of tokens adjacent to player according to given position
	public void UpdateTokenColors(Vector2I Pos)
	{
		foreach (var enemy in EnemyList)
		{
			if (mapGen.GetSurroundingCells(Pos).Contains(enemy.MapPosition))
			{
				var direction = enemy.MapPosition - Pos;
				if (direction == new Vector2I(1, -1) && enemy.PosColour != Colors.Red)
				{
					enemy.PosColour = Colors.Red;
				}
				else if (direction == new Vector2I(1, 0) && enemy.PosColour != Colors.Gold)
				{
					enemy.PosColour = Colors.Gold;
				}
				else if (direction == new Vector2I(0, 1) && enemy.PosColour != Colors.Green)
				{
					enemy.PosColour = Colors.Green;
				}
				else if (direction == new Vector2I(-1, 1) && enemy.PosColour != Colors.Blue)
				{
					enemy.PosColour = Colors.Blue;
				}
				else if (direction == new Vector2I(-1, 0) && enemy.PosColour != Colors.White)
				{
					enemy.PosColour = Colors.White;
				}
				else if (direction == new Vector2I(0, -1) && enemy.PosColour != Colors.Purple)
				{
					enemy.PosColour = Colors.Purple;
				}
			}
			else
			{
				if (enemy.PosColour != Colors.Black) { enemy.PosColour = Colors.Black; }
			}
		}
	}
	public void _on_interact_button_pressed()
	{
		var mapEvent = mapGen.GetCellTileData(MapGen.MainLayer, player.PlayerPos).GetCustomData("Event").ToString();
		switch (mapEvent)
		{
			case "village":
				break;
			case "monastery":
				break;
			case "keep":
				break;
			case "tower":
				break;
			case "den":
				break;
			case "labyrinth":
				break;
			case "tomb":
				break;
			case "grounds":
				break;
			case "maze":
				break;
			case "dungeon":
				break;
			case "gcastle":
				break;
			case "bcastle":
				break;
			case "rcastle":
				break;
			case "wcastle":
				break;
			case "glade":
				break;
			default:
				var mapToken = mapGen.GetCellTileData(MapGen.MainLayer, player.PlayerPos).GetCustomData("Token").ToString();
				if (mapToken == "")
				{
					GD.Print("Unknown Interaction");
				}
				else
				{
					foreach (var ruin in RuinList)
					{
						if (ruin.MapPosition == player.PlayerPos)
						{
							if (Utils.RuinEvents[ruin.TokenId].Event == "monster")
							{
								foreach (var monsterColour in Utils.RuinEvents[ruin.TokenId].Requirements)
								{
									var enemy = GameSettings.DrawMonster(Utils.ConvertStringToMonsterColour(monsterColour));
									GameSettings.EnemyList.Add((enemy, 0, Colors.Black, Colors.Black)); // may need to add another variable for unseen
								}
								var ChallengeStart = (Window)ChallengeScene.Instantiate();
								AddChild(ChallengeStart);
								ChallengeStart.Position = new Godot.Vector2I(300, 500);
							}
							else if (Utils.RuinEvents[ruin.TokenId].Event == "mana")
							{

							}
							else
							{
								GD.Print("Error with identifying ruin event");
							}
							break;
						}
					}
				}
				break;
		}
	}
}
