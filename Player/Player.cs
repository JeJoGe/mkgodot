using Godot;
using System;
using System.Linq;
using System.Numerics;

public partial class Player : Node2D
{
	const int MainLayer = 0;
	const int MainTerrainSet = 0;
	Vector2I playerPos = new Vector2I(0, 0);
	private int movePoints = 100;

	public int MovePoints { get => movePoints; set => movePoints = value; }
	MapGen mapGen;
	Vector2I NewPosition;
	Callable ChangeGlobalPos;
	PackedScene CombatScene;

	public override void _Ready()
	{
		mapGen = GetNode<MapGen>("../MapGen");
		//GD.Print(mapGen.ToGlobal(mapGen.MapToLocal(new Vector2I(0,0))));
		GlobalPosition = mapGen.ToGlobal(mapGen.MapToLocal(playerPos));
		ChangeGlobalPos = Callable.From(() => ChangeGlobalPosition(NewPosition));
		CombatScene = GD.Load<PackedScene>("res://Combat.tscn");
	}
	public override void _Input(InputEvent @event)
	{
		// Called on Input Event. For now, should only process mouse event Leftclick
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
			if (currentAtlasCoords is (-1, -1) && mapGen.GetSurroundingCells(playerPos).Contains(posClicked)) // No tile from atlas exists here and adjacent to player
			{
				mapGen.GenerateTile(currentAtlasCoords, posClicked);
				Utils.undoRedo.ClearHistory();
			}
			// Check if tile clicked is any tiles surrounding player position
			else if (mapGen.GetSurroundingCells(playerPos).Contains(posClicked))
			{
				// Check if next to any enemy
				var gamePlay = GetNode<GamePlay>("..");
				foreach (var enemy in gamePlay.EnemyList)
				{
					// Enemy adjacent and moving to another tile adjacent to enemy
					if ((mapGen.GetSurroundingCells(playerPos).Contains(enemy.MapPosition) && mapGen.GetSurroundingCells(enemy.MapPosition).Contains(posClicked)
					&& (enemy.Colour == "green" || enemy.Colour == "red") && IsWallBetween(posClicked, enemy.MapPosition) == false) || (enemy.MapPosition == posClicked))
					{
						InitiateCombat();
					}
					// Need another else if for if enemy is facedown and time of day
				}
				var cellTerrain = mapGen.GetCellTileData(MainLayer, posClicked).Terrain; // Get terrain of tile
				// GD.Print("Terrain: " + mapGen.TileSet.GetTerrainName(MainTerrainSet, cellTerrain));
				GD.Print("Wall is between: " + IsWallBetween(playerPos, posClicked).ToString());

				if (MovePoints >= mapGen.terrainCosts[cellTerrain])
				{
					PerformMovement(posClicked, cellTerrain);
				}

				else
				{
					GD.Print("Terrain costs more movement than you currently have.");
				}
			}

		}
		else if (@event.IsActionPressed("escape"))
		{
			GetTree().Quit();
		}
	}

	private void InitiateCombat()
	{
		GD.Print("Combat Start!");
		// instantiate Combat scene
		// set player level
		// set enemies
		var CombatStart = (Combat)CombatScene.Instantiate();
		AddChild(CombatStart);
		CombatStart.GlobalPosition = new Godot.Vector2(0,0);
	}

	public void CombatCleanup(bool victory)
	{
		if (victory)
		{
			// do something
		}
		else
		{
			// do something else
		}		
	}

	// Change position of player, update position vector
	private void PerformMovement(Vector2I posClicked, int cellTerrain)
	{
		Utils.undoRedo.CreateAction("Move Player");
		Utils.undoRedo.AddDoProperty(this, "NewPosition", mapGen.ToGlobal(mapGen.MapToLocal(posClicked)));
		Utils.undoRedo.AddUndoProperty(this, "NewPosition", mapGen.ToGlobal(mapGen.MapToLocal(playerPos)));
		Utils.undoRedo.AddDoMethod(ChangeGlobalPos);
		Utils.undoRedo.AddUndoMethod(ChangeGlobalPos);
		//Utils.undoRedo.AddDoProperty(this, "GlobalPosition", mapGen.ToGlobal(mapGen.MapToLocal(posClicked)));
		//Utils.undoRedo.AddUndoProperty(this, "GlobalPosition", mapGen.ToGlobal(mapGen.MapToLocal(playerPos)));
		Utils.undoRedo.AddDoProperty(this, "playerPos", posClicked);
		Utils.undoRedo.AddUndoProperty(this, "playerPos", playerPos);
		Utils.undoRedo.AddDoProperty(this, "MovePoints", MovePoints - (int)mapGen.terrainCosts[cellTerrain]); // Reduce move points
		Utils.undoRedo.AddUndoProperty(this, "MovePoints", MovePoints + (int)mapGen.terrainCosts[cellTerrain]);
		Utils.undoRedo.CommitAction();
		//GD.Print(mapGen.ToGlobal(mapGen.MapToLocal(posClicked)));
		//GD.Print(GlobalPosition);
	}

	// Check walls unique data in current tile and iterate. if Destination vector - Any Walls vector == Source vector, then wall between
	private bool IsWallBetween(Vector2I Start, Vector2I Destination)
	{
		// Get array of walls in current player tile
		var wallArray = mapGen.GetCellTileData(MainLayer, Start).GetCustomData("Walls").As<Godot.Vector2[]>();
		foreach (var wall in wallArray)
		{
			if (Destination - wall == Start)
			{
				return true;
			}
		}
		return false;
	}

	public void ChangeGlobalPosition(Vector2I GPosition)
	{
		this.GlobalPosition = GPosition;
	}

	private void _OnUndoButtonDown()
	{
		Utils.undoRedo.Undo();
	}
}
