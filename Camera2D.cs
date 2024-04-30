using Godot;
using System;
using System.Runtime.Serialization;

public partial class Camera2D : Godot.Camera2D
{
	[Export]
	private int camSpeed = 400;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// if move cam buttons pressed, move cam position (right-left, down-up) as window coords start 0,0 from top left corner
		if ((Input.IsActionPressed("camLeft")) || (Input.IsActionPressed("camRight")) || (Input.IsActionPressed("camUp")) || (Input.IsActionPressed("camDown")))
    	{
			Position+= new Vector2((Convert.ToInt32(Input.IsActionPressed("camRight")) - Convert.ToInt32(Input.IsActionPressed("camLeft"))) * camSpeed * (float)delta, (Convert.ToInt32(Input.IsActionPressed("camDown")) - Convert.ToInt32(Input.IsActionPressed("camUp"))) * camSpeed * (float)delta);
		}
	}
}
