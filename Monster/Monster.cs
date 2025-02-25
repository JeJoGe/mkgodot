using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Monster : Node2D
{
	[Export]
	public int SiteFortifications { get; set; }
	[Export]
	private Sprite2D _halo;
	private bool _selected = false;
	public bool Selected
	{
		get => _selected;
		set
		{
			_selected = value;
			_halo.Visible = _selected;
		}
	}
	[Export]
	private ColorRect _colorRect;
	[Export]
	private Sprite2D _noAttack;
	[Export]
	private Label _armourLabel;
	public Color PosColour { get; set; }
	public bool Blocked
	{
		get
		{
			return !Attacks.Any(attack => !attack.Blocked);
		}
	}
	public bool Defeated { get; set; } = false;
	public bool Attacking
	{
		get
		{
			return Attacks.Any(attack => attack.Attacking);
		}
		set
		{
			var undoRedo = GetParent<Combat>().UndoRedo;
			foreach (var attack in Attacks)
			{
				undoRedo.AddUndoProperty(attack, "Attacking", attack.Attacking);
				attack.Attacking = value;
			}
			_noAttack.Visible = !value;
		}
	}
	public bool Summoned { get; set; } = false;
	private int _armour;
	public int Armour
	{
		get => _armour;
		set
		{
			_armour = value < 1 ? 1 : value; // armour can not be less than 1
			_armourLabel.Text = _armour.ToString();
		}
	}
	public int Fame { get; set; }
	public List<MonsterAttack> Attacks { get; set; } = new List<MonsterAttack>();
	public List<string> Abilities { get; set; }
	public List<Element> Resistances { get; set; }
	public MonsterColour Colour { get; set; }
	// MapPosition is direction relative to player
	public Vector2I MapPosition { get; set; }
	public int MonsterId { get; set; }
	private bool _flag { get; set; } = false;
	private PackedScene _attackScene = GD.Load<PackedScene>("res://Monster/MonsterAttack.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Area2D>("Area2D").InputEvent += OnInputEvent;
		_colorRect.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void PopulateStats(MonsterObject data, int id)
	{
		var combat = GetParent<Combat>();
		Armour = data.Armour;
		// create attack buttons
		for (int i = 0; i < data.Attacks.Count; i++)
		{
			var attackObject = data.Attacks[i];
			var monsterAttack = (MonsterAttack)_attackScene.Instantiate();
			AddChild(monsterAttack);
			monsterAttack.Initialize(attackObject, i, combat.MonsterAttacks);
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
			if (attack.Attacking && !attack.Attacked)
			{
				var combat = GetParent<Combat>();
				if (attack.Element == Element.Summon)
				{
					// draw brown tokens
					for (int j = 0; j < attack.Value; j++)
					{
						GD.Print("summon brown token");
						var monsterID = GameSettings.DrawMonster(MonsterColour.Brown);
						var monster = combat.CreateMonsterToken(monsterID);
						monster.Summoned = true;
					}
					Visible = false;
					attack.Attacked = true;
					break;
				}
				attack.ShowAttackButton();
			}
		}
	}

	public void RefreshAttacks()
	{
		for (int i = 0; i < Attacks.Count; i++)
		{
			var attack = Attacks[i];
			if (!attack.Blocked && attack.Element != Element.Summon && attack.Attacking)
			{
				attack.UpdateButtonText();
			}
		}
	}

	private void OnInputEvent(Node _viewport, InputEvent inputEvent, long _idx)
	{
		var combatInstance = GetParent<Combat>();
		if (Input.IsActionPressed("leftClick"))
		{
			switch (combatInstance.CurrentPhase)
			{
				case Combat.Phase.Ranged:
					{
						// only allowed to select monster to have armour reduced if action is resolving and
						// the monster does not have arcane immunity and the effect is not red while the monster
						// has fire resistance and the effect is not blue while the monster has ice resistance
						if (combatInstance.ResolvingAction)
						{
							if (!Abilities.Contains("immunity") &&
							!((combatInstance.ActionColour == Source.Colour.Blue) && Resistances.Contains(Element.Ice)) &&
							!((combatInstance.ActionColour == Source.Colour.Red) && Resistances.Contains(Element.Fire)))
							{
								_flag = true;
								Selected = !Selected;
								combatInstance.DeselectMonsters();
							}
							else
							{
								GD.Print("can not target due to immunity or resistance");
							}
						}
						else if (SiteFortifications == 2 || (Abilities.Contains("fortified") && SiteFortifications == 1))
						{
							// can not target if double fortified during ranged phase
							GD.Print("untargetable");
						}
						else
						{
							Selected = !Selected;
							combatInstance.UpdateTargets();
						}
						break;
					}
				case Combat.Phase.PreventAttacks:
					{
						// only allowed to select monster to be cancelled if cancel action is resolving and
						// the monster does not have arcane immunity and check for fortifications if cancel
						// effect only works on unfortified enemies
						if (combatInstance.ResolvingAction && !Abilities.Contains("immunity") &&
						(!combatInstance.PreventOnlyUnfortified || Abilities.Contains("unfortified") ||
						(SiteFortifications == 0 && !Abilities.Contains("fortified"))))
						{
							if (combatInstance.EnemiesNotAttacking == 0)
							{
								if (!Selected)
								{
									// delist any other attacks already created
									var pressedButton = combatInstance.MonsterAttacks.GetPressedButton();
									pressedButton?.SetPressedNoSignal(false);
									combatInstance.HideAttackButtons();
									_flag = true;
									Selected = !Selected;
									combatInstance.DeselectMonsters();
									// list all attacks to be cancelled
									for (int i = 0; i < Attacks.Count; i++)
									{
										var attack = Attacks[i];
										if (attack.Attacking)
										{
											attack.ShowAttackButton();
										}
									}
									combatInstance.UpdateCancelledAttacks();
								}

							}
							else
							{
								Selected = !Selected;
								combatInstance.UpdateCancelledAttacks();
							}
						}
						// otherwise do nothing
						break;
					}
				case Combat.Phase.Attack:
					{
						Selected = !Selected;
						combatInstance.UpdateTargets();
						break;
					}
				default: break;
			}

		}
	}

	public void Deselect()
	{
		if (!_flag)
		{
			Selected = false;
		}
		else
		{
			_flag = false;
		}
	}

	public void ShowColourIdentifier()
	{
		_colorRect.Color = PosColour;
		_colorRect.Visible = true;
	}

	public void UpdateAttackingIndicator()
	{
		var attacking = false;
		foreach (var attack in Attacks)
		{
			if (attack.Attacking)
			{
				attacking = true;
				break;
			}
		}
		_noAttack.Visible = !attacking;
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
