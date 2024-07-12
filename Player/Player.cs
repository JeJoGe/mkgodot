using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

public partial class Player : Node2D
{
	const int MainLayer = 0;
	const int MainTerrainSet = 0;
	public Vector2I playerPos = new Vector2I(0, 0);
	public int movePoints = 100;

	public int MovePoints { get => movePoints; set => movePoints = value; }
	MapGen mapGen;
	Vector2I NewPosition;
	Callable ChangeGlobalPos;
	PackedScene CombatScene;
	public Combat Combat;
	public bool isCombatSceneActive = false;
	public int influence = 0;
	public bool besideRampage = false;
	private List<int> enemiesBeside = new List<int>();

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
	}

	public void InitiateCombat()
	{
		//GD.Print("Combat Start!");
		// instantiate Combat scene
		// set player level
		// set enemies
		//GetTree().Paused = true;
		var CombatStart = (Combat)CombatScene.Instantiate();
		isCombatSceneActive = true;
		Combat = CombatStart;
		AddChild(CombatStart);
		CombatStart.GlobalPosition = new Godot.Vector2(270,0);
	}

	public void CombatCleanup(bool victory)
	{
		GetTree().Paused = false;
		GameSettings.EnemyList.Clear();
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
	public void PerformMovement(Vector2I posClicked, int cellTerrain)
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
		if (GameSettings.EnemyList.Count != 0)
		{
			InitiateCombat();
		}
	}

	// Check walls unique data in current tile and iterate. if Destination vector - Any Walls vector == Source vector, then wall between
	public bool IsWallBetween(Vector2I Start, Vector2I Destination)
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
		//GD.Print("It's working");
	}

	private void _OnUndoButtonDown()
	{
		Utils.undoRedo.Undo();
	}
}
