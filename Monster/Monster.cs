using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Monster : Node2D
{
	[Export]
	public int SiteFortifications { get; set; }
	[Export]
	public bool Selected { get; set; } = false;
	public bool Blocked {
		get
		{
			return !Attacks.Any(attack => !attack.Blocked);
		} 
	}
	public bool Defeated { get; set; } = false;
	public int Armour { get; set; }
	public int Fame { get; set; }
	public List<MonsterAttack> Attacks { get; set; }
	public List<string> Abilities { get; set; }
	public List<Element> Resistances { get; set; }
	public string Colour { get; set; }
	private static readonly int _attackOffset = 20;

	public Vector2I mapPos { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Area2D>("Area2D").InputEvent += OnInputEvent;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void PopulateStats(MonsterObject data)
	{
		Armour = data.Armour;
		Attacks = data.Attacks;
		Abilities = data.Abilities;
		Colour = data.Colour;
		Resistances = data.Resistances;
		Fame = data.Fame;
	}

	public void Attack()
	{
		for (int i = 0; i < Attacks.Count; i++)
		{
			var attack = Attacks[i];
			if (attack.Element == Element.Summon) {
				// draw brown tokens
				for (int j = 0; j < attack.Value; j++)
				{
					GD.Print("summon brown token");
				}
			}
			var swift = Abilities.Contains("swift");
			var button = new Button();
			button.ButtonGroup = GetParent<Combat>().MonsterAttacks;
			button.Text = string.Format("{0} {1}",attack.Element,attack.Value + (swift ? attack.Value : 0));
			button.Position = new Vector2(-46,100+_attackOffset*i);
			button.ToggleMode = true;
			button.Name = string.Format("AttackButton{0}",i);
			button.Pressed += () => GetParent<Combat>().UpdateBlock(attack,swift);
			AddChild(button);
		}
	}

	public void Damage()
	{
		for (int i = 0; i < Attacks.Count; i++)
		{
			var attack = Attacks[i];
			var brutal = Abilities.Contains("brutal");
			if (!attack.Blocked) {
				var button = GetNode<Button>("AttackButton"+i.ToString());
				button.Text = string.Format("{0} {1}",attack.Element, attack.Value + (brutal ? attack.Value : 0));
			}
		}
	}

	private void OnInputEvent(Node _viewport, InputEvent inputEvent, long _idx)
	{
        if (typeof(InputEventMouseButton) == inputEvent.GetType())
        {
            var mouseEvent = (InputEventMouseButton)inputEvent;
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsPressed()) {
				PrintStats();
				//Need to add in case for anti-fortification
				if (GetParent<Combat>().CurrentPhase == Combat.Phase.Ranged && (SiteFortifications == 2 || (Abilities.Contains("fortified") && SiteFortifications == 1)))
				{
					// can not target if double fortified during ranged phase
					GD.Print("untargetable");
				} else {
					Selected = !Selected;
					if (Selected) {
						GD.Print("Selected");
					} else {
						GD.Print("unselected");
					}
					GetParent<Combat>().UpdateTargets();
				}
			}
		}
	}

	public void PrintStats()
	{
		GD.Print(string.Format("armor: {0}",Armour));
		GD.Print(string.Format("colour: {0}",Colour));
		if (Abilities.Count > 0)
		{
			GD.Print(string.Format("Abilities: {0}",Abilities.First()));
		}
		var resistances = "";
		foreach (var element in Resistances)
		{
			resistances = resistances + element.ToString("F") + " ";
		}
		GD.Print(string.Format("resists: {0}", resistances));
		GD.Print(string.Format("{0} {1}",Attacks.First().Element, Attacks.First().Value));
		GD.Print("Site fortifications: "+SiteFortifications.ToString());
		GD.Print(string.Format("blocked: {0}",Blocked));
	}

	
}
