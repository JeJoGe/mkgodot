using Godot;
using System;

public partial class MapTokenControl : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnMouseEntered() {
		if (Input.IsActionPressed("shift"))
		{
			GetNode<Sprite2D>("Sprite2D").Scale = new Vector2((float)1,(float)1);
			GetNode<Sprite2D>("Sprite2D").ZIndex = 1;
		}
	}

	private void OnMouseExited() {
		GetNode<Node2D>("Sprite2D").Scale = new Vector2((float)0.25,(float)0.25);
		GetNode<Sprite2D>("Sprite2D").ZIndex = 0;
	}
}
