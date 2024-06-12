using Godot;
using System;
using System.Collections.Generic;

public partial class Unit : Node2D
{
	public int Armour { get; set; }
	public int Level { get; set; }
	public List<Element> Resistances { get; set; }
	public int Wounds { get; set; }
	public bool Selected { get; set; } = false;
	public bool Damaged { get; set; } = false;
	private bool _flag { get; set; } = false;
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
		if (data.Resistances.Contains(Element.Fire) && data.Resistances.Contains(Element.Ice))
		{
			Resistances.Add(Element.ColdFire);
		}
	}

	private void OnInputEvent(Node _viewport, InputEvent inputEvent, long _idx)
	{
		if (typeof(InputEventMouseButton) == inputEvent.GetType())
		{
			var mouseEvent = (InputEventMouseButton)inputEvent;
			//can only select if not damaged this combat and not wounded
			if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsPressed() &&
			GetParent<Combat>().CurrentPhase == Combat.Phase.Damage && !Damaged && Wounds == 0)
			{
				GetParent<Combat>().TargetUnit = Selected ? null : this; // if currently selected deselect current unit
				Selected = !Selected;
				GD.Print(string.Format("{0} armour unit {1}", Armour, Selected ? "selected" : "deselected"));
				if (Selected)
				{
					// deselect all other units
					_flag = true;
					GetParent<Combat>().DeselectUnits();
				}
			}
		}
	}

	public void Deselect()
	{
		if (!_flag)
		{
			Selected = false;
			GD.Print(string.Format("{0} armour unit {1}", Armour, Selected ? "selected" : "deselected"));
		}
		else
		{
			_flag = false;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
