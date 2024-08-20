using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

public partial class Player : Node2D
{	
	GameplayControl gameplayControl;
	const int MainLayer = 0;
	const int MainTerrainSet = 0;
	private Vector2I _playerPos;
	public Vector2I PlayerPos { get => _playerPos; set => _playerPos = value;}
	public int movePoints = 100;

	public int MovePoints { get => movePoints; set => movePoints = value; }
	MapGen mapGen;
	Vector2I NewPosition;
	Callable ChangeGlobalPos;
	Callable UpdateTColors;
	PackedScene CombatScene;
	public Combat Combat;
	public bool isCombatSceneActive = false;
	public int influence = 0;
	public int cardDrawLimit = 5;
	private List<(string,bool)> _skills = new();

	public override void _Ready()
	{
		PlayerPos = new Vector2I(0, 0);
		mapGen = GetNode<MapGen>("../MapGen");
		gameplayControl = GetNode<GameplayControl>("..");
		//GD.Print(mapGen.ToGlobal(mapGen.MapToLocal(new Vector2I(0,0))));
		GlobalPosition = mapGen.ToGlobal(mapGen.MapToLocal(PlayerPos));
		ChangeGlobalPos = Callable.From(() => ChangeGlobalPosition(NewPosition));
		UpdateTColors = Callable.From(() => UpdateTokenColors(PlayerPos));
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
		GetTree().Paused = true;
	}

	public void CombatCleanup(List<(int, Color)> defeated)
	{
		GetTree().Paused = false;
		var currHexSafe = true; // check if current hex is safe
		if (!currHexSafe) 
		{
			// retreat to last safe space
		}
		// give awards based on defeated enemies
		GameSettings.EnemyList.Clear();
		GameSettings.ChallengeList.Clear();
	}

	// Change position of player, update position vector
	public void PerformMovement(Vector2I posClicked, int cellTerrain, int modifier)
	{
			var terrainCost = (int)mapGen.terrainCosts[cellTerrain];
			Utils.undoRedo.CreateAction("Move Player");
			Utils.undoRedo.AddDoProperty(this, "NewPosition", mapGen.ToGlobal(mapGen.MapToLocal(posClicked)));
			Utils.undoRedo.AddUndoProperty(this, "NewPosition", mapGen.ToGlobal(mapGen.MapToLocal(PlayerPos)));
			Utils.undoRedo.AddDoMethod(ChangeGlobalPos);
			Utils.undoRedo.AddUndoMethod(ChangeGlobalPos);
			//Utils.undoRedo.AddDoProperty(this, "GlobalPosition", mapGen.ToGlobal(mapGen.MapToLocal(posClicked)));
			//Utils.undoRedo.AddUndoProperty(this, "GlobalPosition", mapGen.ToGlobal(mapGen.MapToLocal(PlayerPos)));
			Utils.undoRedo.AddDoProperty(this, "PlayerPos", posClicked);
			Utils.undoRedo.AddUndoProperty(this, "PlayerPos", PlayerPos);
			Utils.undoRedo.AddDoProperty(this, "MovePoints", MovePoints - terrainCost + modifier); // Reduce move points
			Utils.undoRedo.AddUndoProperty(this, "MovePoints", MovePoints + terrainCost + modifier);
			Utils.undoRedo.AddDoMethod(UpdateTColors);
			Utils.undoRedo.AddUndoMethod(UpdateTColors);
			Utils.undoRedo.CommitAction();
			//GD.Print(mapGen.ToGlobal(mapGen.MapToLocal(posClicked)));
			//GD.Print(GlobalPosition);
			//gameplayControl.UpdateTokenColors();
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

	public void UpdateTokenColors(Vector2I Pos)
	{
		gameplayControl.UpdateTokenColors(Pos);
		//GD.Print("It's working");
	}

	private void _OnUndoButtonDown()
	{
		Utils.undoRedo.Undo();
	}
}
