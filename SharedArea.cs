using Godot;
using System;

public partial class SharedArea : Node2D
{
	public int Round { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Round = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void NewRound() {
		Round++;
		GetNode<Label>("RoundMarker").Text = "Round " + Round;
	}
}
