using Godot;
using System;

public partial class Inventory : Node2D
{
	private PackedScene _dieScene = GD.Load<PackedScene>("res://ManaDie.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since  the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnDieTaken(int colour)
	{
		GD.Print(string.Format("die taken of colour {0}",colour));
	}
}
