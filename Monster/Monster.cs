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
	public bool Blocked
	{
		get
		{
			return !Attacks.Any(attack => !attack.Blocked);
		}
	}
	public bool Defeated { get; set; } = false;
	public bool Summoned { get; set; } = false;
	public int Armour { get; set; }
	public int Fame { get; set; }
	public List<MonsterAttack> Attacks { get; set; } = new List<MonsterAttack>();
	public List<string> Abilities { get; set; }
	public List<Element> Resistances { get; set; }
	public MonsterColour Colour { get; set; }
	public int MonsterId { get; set; }
	private static readonly int _attackOffset = 40;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Area2D>("Area2D").InputEvent += OnInputEvent;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void PopulateStats(MonsterObject data, int id)
	{
		Armour = data.Armour;
		foreach (var attackData in data.Attacks)
		{
            var monsterAttack = new MonsterAttack
            {
                Element = attackData.Element,
                Value = attackData.Value
            };
			Attacks.Add(monsterAttack);
        }
		Abilities = new List<string>(data.Abilities);
		Colour = data.Colour;
		Resistances = new List<Element>(data.Resistances);
		Fame = data.Fame;
		MonsterId = id;
	}

	public void Attack()
	{
		for (int i = 0; i < Attacks.Count; i++)
		{
			var attack = Attacks[i];
			if (attack.Element == Element.Summon)
			{
				// draw brown tokens
				for (int j = 0; j < attack.Value; j++)
				{
					GD.Print("summon brown token");
					var monsterID = GameSettings.DrawMonster(MonsterColour.Brown);
					var monster = GetParent<Combat>().CreateMonsterToken(monsterID);
					monster.Summoned = true;
				}
				Visible = false;
				attack.Attacked = true;
				break;
			}
			var swift = Abilities.Contains("swift");
			var button = new Button
			{
				ButtonGroup = GetParent<Combat>().MonsterAttacks,
				Text = string.Format("{0} {1}", attack.Element, attack.Value + (swift ? attack.Value : 0)),
				ToggleMode = true,
                Position = new Vector2(-46, 60 + _attackOffset * i),
                Name = string.Format("AttackButton{0}", i)
            };
            button.Pressed += () => OnAttackButtonToggled(attack);
			AddChild(button);
		}
	}

	public void Damage()
	{
		for (int i = 0; i < Attacks.Count; i++)
		{
			var attack = Attacks[i];
			var brutal = Abilities.Contains("brutal");
			if (!attack.Blocked && attack.Element != Element.Summon)
			{
				var button = GetNode<Button>("AttackButton" + i.ToString());
				button.Text = string.Format("{0} {1}", attack.Element, attack.Value + (brutal ? attack.Value : 0));
			}
		}
	}

	private void OnAttackButtonToggled(MonsterAttack attack)
	{
		GetParent<Combat>().TargetAttack = attack;
	}

	private void OnInputEvent(Node _viewport, InputEvent inputEvent, long _idx)
	{
		if (Input.IsActionPressed("leftClick"))
		{
			//PrintStats();
			//Need to add in case for anti-fortification
			if (GetParent<Combat>().CurrentPhase == Combat.Phase.Ranged && (SiteFortifications == 2 || (Abilities.Contains("fortified") && SiteFortifications == 1)))
			{
				// can not target if double fortified during ranged phase
				GD.Print("untargetable");
			}
			else
			{
				Selected = !Selected;
				if (Selected)
				{
					GD.Print("Selected");
				}
				else
				{
					GD.Print("unselected");
				}
				GetParent<Combat>().UpdateTargets();
			}

		}
	}

	public void PrintStats()
	{
		GD.Print(string.Format("armor: {0}", Armour));
		GD.Print(string.Format("colour: {0}", Colour));
		if (Abilities.Count > 0)
		{
			GD.Print(string.Format("Abilities: {0}", Abilities.First()));
		}
		var resistances = "";
		foreach (var element in Resistances)
		{
			resistances = resistances + element.ToString("F") + " ";
		}
		GD.Print(string.Format("resists: {0}", resistances));
		GD.Print(string.Format("{0} {1}", Attacks.First().Element, Attacks.First().Value));
		GD.Print("Site fortifications: " + SiteFortifications.ToString());
		GD.Print(string.Format("blocked: {0}", Blocked));
	}


}
