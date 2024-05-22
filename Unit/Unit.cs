using Godot;
using System;
using System.Collections.Generic;

public partial class Unit : Node2D
{
	public int Armour { get; set; }
	public int Level { get; set; }
	public List<Element> Resistances { get; set; }
	public int Wounds { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Area2D>("Area2D").InputEvent += OnInputEvent;
	}

	public void PopulateStats(UnitObject data)
	{
		Armour = data.Armour;
		Level = data.Level;
		Resistances = data.Resistances;
	}

	private void OnInputEvent(Node _viewport, InputEvent inputEvent, long _idx)
	{
		if (typeof(InputEventMouseButton) == inputEvent.GetType())
        {
            var mouseEvent = (InputEventMouseButton)inputEvent;
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsPressed()) {	
				GD.Print("unit clicked");
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
