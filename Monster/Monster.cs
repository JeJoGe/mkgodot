using Godot;
using System;
using System.Collections.Generic;

public partial class Monster : Node2D
{
	public int Armour { get; set; }
	public List<Attack> Attacks { get; set; }
	public int Fame { get; set; }
	public string Type { get; set; }
	public List<string> Resistances { get; set; }
	public List<string> Abilities { get; set; }
	public string Image { get; set; }
	public bool SiteFortifications { get; set; }
	public string MonsterName { get; set; }

	public Monster() {

	}

	public Monster(Monster monster) {
		Armour = monster.Armour;
		Attacks = new List<Attack>(monster.Attacks);
		Resistances = new List<string>(monster.Resistances);
		Abilities = new List<string>(monster.Resistances);
		Fame = monster.Fame;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public class Attack
	{
		public int Value { get; set; }
		public string Type { get; set; }
	}
}
