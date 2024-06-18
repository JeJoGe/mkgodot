using Godot;
using System;

public partial class MapTokenControl : Control
{

	private bool hover;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//this.MouseFilter = MouseFilterEnum.Stop;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	// Zoom if shift pressed and hovering token
	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionPressed("shift") && hover == true)
		{
			GetNode<Sprite2D>("Sprite2D").Scale = new Vector2((float)1,(float)1);
			GetNode<Sprite2D>("Sprite2D").ZIndex = 1;
		}
		else if (Input.IsActionJustReleased("shift") || hover == false)
		{
			GetNode<Node2D>("Sprite2D").Scale = new Vector2((float)0.25,(float)0.25);
			GetNode<Sprite2D>("Sprite2D").ZIndex = 0;
		}
		
	}
	private void OnMouseEntered() {
		hover = true;
	}

	private void OnMouseExited() {
		hover = false;
	}
	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb)
		{
			if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
			{
				GD.Print("Monster been clicked D:");
			}
		}
	}
}
