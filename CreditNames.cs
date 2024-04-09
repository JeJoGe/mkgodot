using Godot;
using System;

public partial class CreditNames : RichTextLabel
{	
	private float _speed = 400;
	public float direction = 1;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Position += Vector2.Up * (float)delta * _speed * direction;
		if (Position.Y < -600)
		{
			GetTree().ChangeSceneToFile("res://MainScene.tscn");
		}
	}
}
