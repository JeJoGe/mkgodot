using Godot;
using System;

public partial class MapToken : Node2D
{
	public string Colour { get; set; }
	public Vector2I MapPosition { get; set; }
	public int SiteFortifications { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
