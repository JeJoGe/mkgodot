using Godot;
using System;

public partial class GamePlay : Node2D
{
	private static int _numPlayers = 1; // default number of players
	public static int NumPlayers  { 
		get => _numPlayers;
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
