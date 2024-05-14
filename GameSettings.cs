using Godot;
using System;
using System.Collections.Generic;

public partial class GameSettings : Node
{
	private static int _numPlayers = 1;
	public static int NumPlayers {
		get => _numPlayers;
	}
	public static int PlayerCharacter { get; set; }
	public static int DummyCharacter { get; set; }
	public static List<(int,bool)> EnemyList { get; set; } // monster id, site fortifications

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
