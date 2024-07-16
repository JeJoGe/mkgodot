using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Combat : Node2D
{
	[Export]
	private ConfirmationDialog _confirmDialog;
	[Signal]
	public delegate void KnockoutEventHandler();
	[Signal]
	public delegate void PoisonEventHandler(int wounds);
	[Signal]
	public delegate void WoundEventHandler(int wounds);
	private List<Monster> _enemyList = new List<Monster>();
	private List<Unit> _unitList = new List<Unit>();
	public ButtonGroup MonsterAttacks;
	public Dictionary<int, int> PlayerAttacks = new Dictionary<int, int>();
	public Dictionary<int, int> PlayerBlocks = new Dictionary<int, int>();
	private int _targetArmour, _totalAttack, _totalBlock;
	private List<Element> _targetResistances = new List<Element>();
	private bool _targetFortified;
	private int _totalFame = 0;
	// total wounds added to hand this combat
	private int _totalWounds = 0;
	private int _unitWounds = 0;
	private bool _unitDestroyed = false;
	private (int, int) _currentAttackWounds = (0, 0); // includes wounds added to discard by poison
	private int _armour = 2;
	private bool _knockout = false;
	private int _maxHandSize = 5;
	private bool _resolvingAction = false;
	public bool ResolvingAction { get => _resolvingAction; } // prevent another action from being activated while current action resolves
	private int _enemiesNotAttacking;
	public int EnemiesNotAttacking { get => _enemiesNotAttacking; } // number of enemies to be prevented from attacking, 0 -> cancel single attack
	public bool PreventOnlyUnfortified { get; set; } // only cancel attacks from unfortified enemy
	private MonsterAttack _targetAttack;
	public MonsterAttack TargetAttack
	{
		get => _targetAttack;
		set
		{
			_targetAttack = value;
			switch (CurrentPhase)
			{
				case Phase.PreventAttacks:
					UpdateCancelledAttacks();
					break;
				case Phase.Block:
					UpdateBlock();
					break;
				case Phase.Damage:
					UpdateDamage();
					break;
				default: break;
			}
		}
	}
	private Unit _targetUnit = null;
	public Unit TargetUnit
	{
		get => _targetUnit;
		set
		{
			_targetUnit = value;
			// check if attack is selected before updating
			if (MonsterAttacks.GetPressedButton() != null)
			{
				UpdateDamage();
			}
		}
	}
	public enum Phase
	{
		Ranged, PreventAttacks, Block, Damage, Attack
	}
	// represented table here:
	/*
	*          Melee=0 Ranged=4  Siege=8
	* Physical  0		4			8		
	* Fire		1		5			9
	* Ice		2		6			10
	* ColdFire	3		7			11
	*
	* for block here:
	*			Normal=0 Swift=4
	* Physical	0		4
	* Fire		1		5
	* Ice		2		6
	* ColdFire	3		7
	*/
	public enum AttackRange
	{
		Melee, Ranged = 4, Siege = 8
	}

	private static readonly int _spriteSize = 258;
	private static readonly int _offset = 120;
	private static readonly int _cardOffset = 200;
	private PackedScene _monsterScene = GD.Load<PackedScene>("res://Monster/Monster.tscn");

	public Phase CurrentPhase { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// FOR INITIAL TESTING
		//GD.Print("Combat Start!");
		//GD.Print("TEST: " + GameSettings.EnemyList.Count.ToString());
		PlayerBlocks[0] = PlayerBlocks[1] = PlayerBlocks[2] = PlayerBlocks[3] = 10;
		PlayerBlocks[4] = PlayerBlocks[5] = PlayerBlocks[6] = PlayerBlocks[7] = 10;
		PlayerAttacks[0] = PlayerAttacks[1] = PlayerAttacks[2] = PlayerAttacks[3] = 10;
		PlayerAttacks[4] = PlayerAttacks[5] = PlayerAttacks[6] = PlayerAttacks[7] = 10;
		PlayerAttacks[8] = PlayerAttacks[9] = PlayerAttacks[10] = PlayerAttacks[11] = 10;
		//GameSettings.EnemyList = new List<(int, int)>([(0, 0), (1600, 0), (1000, 1), (501, 0), (2002, 0)]);
		GameSettings.UnitList = new List<(int, int)>([(1, 0), (2, 0), (6, 2)]);
		//Utils.PrintBestiary();
		// create  enemy tokens
		for (var i = 0; i < GameSettings.EnemyList.Count; i++)
		{
			var enemy = GameSettings.EnemyList[i];
			GD.Print("Monster ID: " + enemy.Item1.ToString());
			var monsterToken = CreateMonsterToken(enemy.Item1);
			monsterToken.SiteFortifications = enemy.Item2;
			monsterToken.PosColour = enemy.Item3;
		}
		// instantiate units
		var unitScene = GD.Load<PackedScene>("res://Unit/Unit.tscn");
		for (int i = 0; i < GameSettings.UnitList.Count; i++)
		{
			var unit = GameSettings.UnitList[i];
			var unitCard = (Unit)unitScene.Instantiate();
			// get unit stats
			unitCard.Wounds = unit.Item2;
			var unitStats = Utils.UnitStats[unit.Item1];
			unitCard.PopulateStats(unitStats);
			var unitSprite = unitCard.GetNode<Sprite2D>("Sprite2D");
			var atlas = (AtlasTexture)Utils.SpriteSheets[unitCard.Level > 2 ? "gold" : "silver"].Duplicate();
			atlas.Region = new Rect2(
				new Vector2(unitStats.X * GameSettings.CardWidth, unitStats.Y * GameSettings.CardLength),
				new Vector2(GameSettings.CardWidth, GameSettings.CardLength));
			unitSprite.Texture = atlas;
			unitCard.Position = new Vector2(_cardOffset * i + 100, 400);
			//add child to control
			AddChild(unitCard);
			_unitList.Add(unitCard);
		}

		CurrentPhase = Phase.Ranged;
	}

	public Monster CreateMonsterToken(int id)
	{
		var monsterToken = (Monster)_monsterScene.Instantiate();
		var monsterStats = Utils.Bestiary[id];
		monsterToken.PopulateStats(monsterStats, id);
		var enemySprite = monsterToken.GetNode<Sprite2D>("Sprite2D");
		var atlas = (AtlasTexture)Utils.SpriteSheets[Utils.ConvertMonsterColourToString(monsterToken.Colour)].Duplicate();
		atlas.Region = new Rect2(new Vector2(monsterStats.X * _spriteSize, monsterStats.Y * _spriteSize), new Vector2(_spriteSize, _spriteSize));
		enemySprite.Texture = atlas;
		monsterToken.Position = new Vector2(_offset * _enemyList.Count + 60, 80);
		AddChild(monsterToken);
		_enemyList.Add(monsterToken);
		return monsterToken;
	}

	public void UpdateTargets()
	{
		_targetArmour = 0;
		_targetResistances.Clear();
		_targetFortified = false;
		foreach (var enemy in _enemyList)
		{
			if (enemy.Selected)
			{
				_targetArmour += enemy.Armour;
				_targetResistances = _targetResistances.Union(enemy.Resistances).ToList();
				// add coldfire resistance to simplify calculations
				if (_targetResistances.Contains(Element.Fire) && _targetResistances.Contains(Element.Ice))
				{
					_targetResistances.Add(Element.ColdFire);
				}
				// only need to check for fortififcations during ranged phase
				if (CurrentPhase == Phase.Ranged && !_targetFortified && (enemy.SiteFortifications == 1 || enemy.Abilities.Contains("fortified")))
				{
					_targetFortified = true;
				}
			}
		}
		GD.Print("target armour: " + _targetArmour);
		var resistances = "";
		foreach (var item in _targetResistances)
		{
			resistances += item + " ";
		}
		GD.Print("targets resistances: " + resistances);
		GD.Print("targets fortified: " + _targetFortified.ToString());
		UpdateAttack();
	}

	public void UpdateCancelledAttacks()
	{
		GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = true;
		if (EnemiesNotAttacking == 0)
		{
			// cancel single attack
			GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = MonsterAttacks.GetPressedButton() == null;
		}
		else
		{
			// cancel all attacks of specified units
			var selectedEnemies = _enemyList.Count(enemy => enemy.Selected);
			GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = selectedEnemies > EnemiesNotAttacking;
		}
	}

	private void UpdateBlock()
	{
		var swift = MonsterAttacks.GetPressedButton().GetParent<Monster>().Abilities.Contains("swift");
		var inefficientBlock = 0;
		var efficientBlock = PlayerBlocks[(int)Element.ColdFire] + (swift ? PlayerBlocks[7] : 0); // cold fire block is always efficient
		switch (TargetAttack.Element)
		{
			case Element.Physical:
				{
					// all blocks are efficient
					efficientBlock += PlayerBlocks[(int)Element.Physical] + PlayerBlocks[(int)Element.Fire] + PlayerBlocks[(int)Element.Ice] +
					(swift ? PlayerBlocks[(int)Element.Physical + 4] + PlayerBlocks[(int)Element.Fire + 4] + PlayerBlocks[(int)Element.Ice + 4] : 0);
					break;
				}
			case Element.Fire:
				{
					// ice is efficient
					efficientBlock += PlayerBlocks[(int)Element.Ice] + (swift ? PlayerBlocks[(int)Element.Ice + 4] : 0);
					inefficientBlock += PlayerBlocks[(int)Element.Physical] + PlayerBlocks[(int)Element.Fire] +
					(swift ? PlayerBlocks[(int)Element.Physical + 4] + PlayerBlocks[(int)Element.Fire + 4] : 0);
					break;
				}
			case Element.Ice:
				{
					// fire is efficient
					efficientBlock += PlayerBlocks[(int)Element.Fire] + (swift ? PlayerBlocks[(int)Element.Fire + 4] : 0);
					inefficientBlock += PlayerBlocks[(int)Element.Physical] + PlayerBlocks[(int)Element.Ice] +
					(swift ? PlayerBlocks[(int)Element.Physical + 4] + PlayerBlocks[(int)Element.Ice + 4] : 0);
					break;
				}
			case Element.ColdFire:
				{
					// all blocks are inefficient
					inefficientBlock += PlayerBlocks[(int)Element.Physical] + PlayerBlocks[(int)Element.Fire] + PlayerBlocks[(int)Element.Ice] +
					(swift ? PlayerBlocks[(int)Element.Physical + 4] + PlayerBlocks[(int)Element.Fire + 4] + PlayerBlocks[(int)Element.Ice + 4] : 0);
					break;
				}
			default: break;
		}
		_totalBlock = efficientBlock + inefficientBlock / 2;
		GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = _totalBlock < TargetAttack.Value + (swift ? TargetAttack.Value : 0);
	}

	private void UpdateDamage()
	{
		var abilities = MonsterAttacks.GetPressedButton().GetParent<Monster>().Abilities;
		CalculateDamage(abilities);
		GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = false;
	}

	private void UpdateDamage(Monster monster)
	{
		var abilities = monster.Abilities;
		CalculateDamage(abilities);
	}

	private void CalculateDamage(List<string> abilities)
	{
		var brutal = abilities.Contains("brutal") ? 1 : 0;
		var paralyze = abilities.Contains("paralyze");
		var poison = abilities.Contains("poison") ? 1 : 0;
		var assassin = abilities.Contains("assassin");
		var attackValue = _targetAttack.Value + brutal * _targetAttack.Value;
		_knockout = false;
		// check if any unit selected otherwise assume hero takes damage
		if (TargetUnit != null && !assassin) // unit selected and enemy does not have assassination ability
		{
			// check if unit resists attack
			if (TargetUnit.Resistances.Contains(_targetAttack.Element))
			{
				attackValue -= TargetUnit.Armour;
			}
			if (attackValue > 0)
			{
				// wound unit
				attackValue -= TargetUnit.Armour;
				if (paralyze)
				{
					// print warning that unit will be destroyed
					GD.Print("unit will be destroyed");
					_unitDestroyed = true;
				}
				else
				{
					_unitWounds = 1 + poison;
					GD.Print(string.Format("wounds to unit: {0}", _unitWounds));
				}
			}
			if (attackValue > 0)
			{
				// calc wounds fors hero
				CalculateHeroWounds(attackValue, poison, paralyze);
			}
			else
			{
				// take no damage
				_currentAttackWounds = (0, 0);
			}
		}
		else
		{
			CalculateHeroWounds(attackValue, poison, paralyze);
		}
		GetNode<Label>("NinePatchRect/WoundsLabel").Text = string.Format("Wounds {0}", _currentAttackWounds.Item1);
		GD.Print(string.Format("wounds to hand: {0}", _currentAttackWounds.Item1));
		GD.Print(string.Format("wounds to discard: {0}", _currentAttackWounds.Item2));
	}

	private void UpdateAttack()
	{
		var resistedAttack = 0;
		var effectiveAttack = 0;
		for (int element = 0; element < 4; element++)
		{
			if (_targetResistances.Contains((Element)element))
			{
				resistedAttack += PlayerAttacks[(int)AttackRange.Siege + element] +
				(_targetFortified ? 0 : 1) * PlayerAttacks[(int)AttackRange.Ranged + element] +
				// current phase is ranged, melee attacks ignored
				(CurrentPhase == Phase.Ranged ? 0 : 1) * PlayerAttacks[element];
			}
			else
			{
				effectiveAttack += PlayerAttacks[(int)AttackRange.Siege + element] +
				(_targetFortified ? 0 : 1) * PlayerAttacks[(int)AttackRange.Ranged + element] +
				(CurrentPhase == Phase.Ranged ? 0 : 1) * PlayerAttacks[element];
			}
		}
		_totalAttack = effectiveAttack + resistedAttack / 2; // integer math automatically rounds down
		GD.Print("final attack: " + _totalAttack.ToString());
		GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = _totalAttack < _targetArmour || _targetArmour == 0;
	}

	private void OnNextButtonPressed()
	{
		switch (CurrentPhase)
		{
			//attacking/blocking is optional
			case Phase.Ranged:
				{
					NextCombatPhase(Phase.PreventAttacks);
					break;
				}
			case Phase.PreventAttacks:
				{
					NextCombatPhase(Phase.Block);
					break;
				}
			case Phase.Block:
				{
					NextCombatPhase(Phase.Damage);
					break;
				}
			case Phase.Damage:
				{
					// assign all damage to hero
					_targetUnit = null;
					DeselectUnits();
					for (int i = 0; i < _enemyList.Count; i++)
					{
						var enemy = _enemyList[i];
						for (int j = 0; j < enemy.Attacks.Count; j++)
						{
							var attack = enemy.Attacks[j];
							if (!attack.Blocked && !attack.Attacked && attack.Attacking)
							{
								_targetAttack = attack;
								UpdateDamage(enemy);
								ApplyWounds();
							}
						}
					}
					// hide all remaining attack buttons
					HideAttackButtons();
					NextCombatPhase(Phase.Attack);
					break;
				}
			case Phase.Attack:
				{
					// return to Map with results of combat (either all enemies defeated or not)
					GD.Print("return to map");
					EndCombat(false);
					break;
				}
			default: break;
		}
	}

	private void OnConfirmButtonPressed()
	{
		switch (CurrentPhase)
		{
			case Phase.Ranged:
				{
					// remove defeated enemies
					DefeatEnemies();
					// exit combat if all enemies defeated		
					if (CheckVictory())
					{
						GD.Print("all enemies defeated");
						// exit combat
						EndCombat(true);
					}
					GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = true;
					ResetAttacks();
					break;
				}
			case Phase.PreventAttacks:
				{
					// prevent selected attack from happening or prevent selected monsters from attacking
					if (EnemiesNotAttacking == 0)
					{
						_targetAttack.Attacking = false;
						_resolvingAction = false;
						MonsterAttacks.GetPressedButton().QueueFree();
					}
					else
					{
						foreach (var enemy in _enemyList)
						{
							if (enemy.Selected)
							{
								enemy.Attacking = false;
							}
						}
					}
					// skip to attack phase if no attacks remaining
					var noAttacks = true;
					for (int i = 0; i < _enemyList.Count; i++)
					{
						var enemy = _enemyList[i];
						if (!enemy.Defeated && enemy.Attacking)
						{
							noAttacks = false;
							break;
						}
					}
					if (noAttacks)
					{
						GD.Print("no enemies attacking");
						NextCombatPhase(Phase.Attack);
					}
					GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = true;
					break;
				}
			case Phase.Block:
				{
					// block attack
					_targetAttack.Blocked = true;
					var button = MonsterAttacks.GetPressedButton();
					button.QueueFree();
					GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = true;
					foreach (var kvp in PlayerBlocks)
					{
						PlayerBlocks[kvp.Key] = 0;
					}
					_totalBlock = 0;
					// check if all enemies blocked
					var allBlocked = true;
					for (int i = 0; i < _enemyList.Count; i++)
					{
						var enemy = _enemyList[i];
						if (!enemy.Defeated && !enemy.Blocked)
						{
							allBlocked = false;
							break;
						}
					}
					if (allBlocked)
					{
						// skip damage phase
						NextCombatPhase(Phase.Attack);
					}
					break;
				}
			case Phase.Damage:
				{
					ApplyWounds();
					var button = MonsterAttacks.GetPressedButton();
					button.QueueFree();
					GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = true;
					// go to attack phase if no attacks remaining
					var skipDamage = true;
					for (int i = 0; i < _enemyList.Count; i++)
					{
						var enemy = _enemyList[i];
						for (int j = 0; j < enemy.Attacks.Count; j++)
						{
							var attack = enemy.Attacks[j];
							if (!attack.Blocked && !attack.Attacked)
							{
								i = _enemyList.Count; // break out of outer loop
								skipDamage = false;
							}
						}
					}
					if (skipDamage)
					{
						NextCombatPhase(Phase.Attack);
					}
					break;
				}
			case Phase.Attack:
				{
					DefeatEnemies();
					// exit combat if all enemies defeated
					if (CheckVictory())
					{
						GD.Print("all enemies defeated");
						// exit combat
						EndCombat(true);
					}
					GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = true;
					ResetAttacks();
					break;
				}
			default: break;
		}
	}

	private void NextCombatPhase(Phase nextPhase)
	{
		CurrentPhase = nextPhase;
		switch (CurrentPhase)
		{
			case Phase.PreventAttacks:
				{
					// zero any remaining attack
					ResetAttacks();
					GetNode<Button>("NinePatchRect/NextButton").Text = "Enemies Attack";
					GetNode<Button>("NinePatchRect/ConfirmButton").Text = "Target Enemy Will Not Attack";
					MonsterAttacks = new ButtonGroup();
					foreach (var enemy in _enemyList)
					{
						enemy.Selected = false;
					}
					// FOR TESTING ONLY
					PreventAttack(3);
					break;
				}
			case Phase.Block:
				{
					// zero any remaining attack
					ResetAttacks();
					// reset monster attack buttons
					HideAttackButtons();
					GetNode<Button>("NinePatchRect/NextButton").Text = "Skip Blocking";
					GetNode<Button>("NinePatchRect/ConfirmButton").Text = "Confirm Block";
					GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = true;

					MonsterAttacks = new ButtonGroup();
					// create enemy attacks for undefeated enemies
					for (int i = 0; i < _enemyList.Count; i++)
					{
						var enemy = _enemyList[i];
						if (!enemy.Defeated && enemy.Attacking)
						{
							enemy.Attack();
						}
					}
					break;
				}
			case Phase.Damage:
				{
					GetNode<Button>("NinePatchRect/NextButton").Text = "Assign All Remaining Damage to Hero";
					GetNode<Button>("NinePatchRect/ConfirmButton").Text = "Confirm Damage";
					GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = true;
					for (int i = 0; i < _enemyList.Count; i++)
					{
						var enemy = _enemyList[i];
						if (!enemy.Defeated && !enemy.Blocked && enemy.Attacking)
						{
							enemy.Damage();
						}
					}
					break;
				}
			case Phase.Attack:
				{
					// discard all summoned enemies
					for (int i = _enemyList.Count - 1; i >= 0; i--)
					{
						var enemy = _enemyList[i];
						if (enemy.Summoned)
						{
							enemy.Visible = false;
							_enemyList.RemoveAt(i);
							GameSettings.DiscardToken(enemy.MonsterId);
						}
						else if (enemy.Attacks.First().Element == Element.Summon)
						{
							// reveal summoners
							enemy.Visible = true;
						}
					}
					GetNode<Button>("NinePatchRect/NextButton").Text = "Skip Attacking";
					GetNode<Button>("NinePatchRect/ConfirmButton").Text = "Confirm Attack";
					GetNode<Button>("NinePatchRect/ConfirmButton").Disabled = true;
					break;
				}
			default: break;
		}
	}

	private void ApplyWounds()
	{
		if (_currentAttackWounds.Item1 > 0)
		{
			_totalWounds += _currentAttackWounds.Item1;
			GetNode<Label>("NinePatchRect/TotalWoundsLabel").Text = string.Format("Total Wounds {0}", _totalWounds);
			// add wounds to hand
			EmitSignal(SignalName.Wound, _currentAttackWounds.Item1);
			if (_currentAttackWounds.Item2 > 0)
			{
				// add wounds to discard
				EmitSignal(SignalName.Poison, _currentAttackWounds.Item2);
			}
			_currentAttackWounds = (0, 0);
			GetNode<Label>("NinePatchRect/WoundsLabel").Text = string.Format("Wounds {0}", _currentAttackWounds.Item1);
		}
		if (_unitDestroyed)
		{
			// destroy unit
			TargetUnit.Visible = false;
			_unitDestroyed = false;
		}
		else if (_unitWounds > 0)
		{
			TargetUnit.Wounds = _unitWounds;
			_unitWounds = 0;
		}
		if (_targetUnit != null)
		{
			TargetUnit.Damaged = true;
			TargetUnit.Selected = false;
			_targetUnit = null;
		}
		// check for ko
		if (_knockout)
		{
			GD.Print("knocked out");
			// discard hand of all non wounds
			EmitSignal(SignalName.Knockout);
		}
		TargetAttack.Attacked = true;
		_targetAttack = null;
	}

	public bool PreventAttack(int numAttacksPrevented, bool targetUnfortified = false)
	{
		var result = false; // return false if still in middle of resolving a cancel attack action
		if (!ResolvingAction)
		{
			_resolvingAction = result = true;
			PreventOnlyUnfortified = targetUnfortified;
			_enemiesNotAttacking = numAttacksPrevented; // if 0 then cancel single attack
			var str = "Target Enemies Will Not Attack";
			if (numAttacksPrevented == 0)
			{
				str = "Cancel Single Attack";
			}
			GetNode<Button>("NinePatchRect/ConfirmButton").Text = str;
		}
		return result;
	}

	private bool CheckVictory()
	{
		var result = true;
		for (int i = 0; i < _enemyList.Count; i++)
		{
			if (!_enemyList[i].Defeated)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void DefeatEnemies()
	{
		var remaining = _enemyList.Count; // just for debugging
		for (int i = _enemyList.Count - 1; i >= 0; i--)
		{
			var enemy = _enemyList[i];
			if (enemy.Selected)
			{
				_totalFame += enemy.Fame;
				enemy.Defeated = true;
				enemy.Visible = false;
				enemy.Selected = false;
			}
			if (enemy.Defeated)
			{
				remaining--;
			}
		}
		GD.Print("total fame: " + _totalFame.ToString());
		GD.Print("enemies remaining: " + remaining);
	}

	private void CalculateHeroWounds(int damage, int poison, bool paralyze) // damage must always be greater than 0
	{
		var wounds = (damage - 1) / _armour + 1; // integer division without having to use Math.Ceiling
		_currentAttackWounds.Item1 = wounds;
		_currentAttackWounds.Item2 = wounds * poison;
		if (wounds + _totalWounds >= _maxHandSize || paralyze)
		{
			_knockout = true;
		}
	}

	private void ResetAttacks()
	{
		foreach (var kvp in PlayerAttacks)
		{
			PlayerAttacks[kvp.Key] = 0;
		}
		_totalAttack = 0;
	}

	public void DeselectUnits()
	{
		foreach (var unit in _unitList)
		{
			unit.Deselect();
		}
	}

	public void DeselectMonsters()
	{
		foreach (var enemy in _enemyList)
		{
			enemy.Deselect();
		}
	}

	private void DamageHero(int damage, int poison, bool paralyze) // damage must always be greater than 0
	{
		var wounds = (damage - 1) / _armour + 1; // integer division without having to use Math.Ceiling
		_currentAttackWounds.Item1 = wounds;
		_currentAttackWounds.Item2 = wounds * poison;
		if (wounds + _totalWounds >= _maxHandSize || paralyze)
		{
			_knockout = true;
		}
	}

	private void EndCombat(bool victory)
	{
		_confirmDialog.Visible = true;
	}

	private void OnFinishCombatConfirmed()
	{
		GD.Print("Combat finished");
		GetParent<Player>().CombatCleanup(false); //pass victory status to player
		QueueFree();
	}

	public void HideAttackButtons()
	{
		var buttons = MonsterAttacks.GetButtons();
		foreach (var button in buttons)
		{
			button.QueueFree();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
