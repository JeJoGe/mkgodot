using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Combat : Node2D
{
	private List<Monster> _enemyList = new List<Monster>();
	private List<Unit> _unitList = new List<Unit>();
	public ButtonGroup MonsterAttacks;
	public Dictionary<int,int> PlayerAttacks = new Dictionary<int,int>();
	public Dictionary<int,int> PlayerBlocks = new Dictionary<int, int>();
	private int _targetArmour, _totalAttack, _totalBlock;
	private List<Element> _targetResistances = new List<Element>();
	private bool _targetFortified;
	private int _totalFame = 0;
	private int _armour = 2;
	private int _maxHandSize = 5;
	private MonsterAttack _targetAttack;
	public MonsterAttack TargetAttack
	{
		get => _targetAttack;
		set
		{
			_targetAttack = value;
			if (CurrentPhase == Phase.Block)
			{
				UpdateBlock();
			} else {
				UpdateDamage();
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
			UpdateDamage();
		}
	}
	public enum Phase
	{
		Ranged,Block,Damage,Attack
	}
	// represented table here:
	/*
	*          Melee=0 Ranged=4  Siege=8
	* Physical. 0		4			8		
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

	public Phase CurrentPhase { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// FOR INITIAL TESTING
		PlayerBlocks[0] = PlayerBlocks[1] = PlayerBlocks[2] = PlayerBlocks[3] = 10;
		PlayerBlocks[4] = PlayerBlocks[5] = PlayerBlocks[6] = PlayerBlocks[7] = 10;
		PlayerAttacks[0] = PlayerAttacks[1] = PlayerAttacks[2] = PlayerAttacks[3] = 10;
		PlayerAttacks[4] = PlayerAttacks[5] = PlayerAttacks[6] = PlayerAttacks[7] = 10;
		PlayerAttacks[8] = PlayerAttacks[9] = PlayerAttacks[10] = PlayerAttacks[11] = 10;
		GameSettings.EnemyList = new List<(int,int)>([(0,0),(1,0),(500,1),(501,0),(2004,2)]);
		GameSettings.UnitList = new List<(int,int)>([(1,0),(2,0),(6,2)]);
		//Utils.PrintBestiary();
		// create  enemy tokens
		var monsterScene = GD.Load<PackedScene>("res://Monster/Monster.tscn");
		for (var i = 0; i < GameSettings.EnemyList.Count; i++)
		{
			var enemy = GameSettings.EnemyList[i];
			var monsterToken = (Monster)monsterScene.Instantiate();
			monsterToken.SiteFortifications = enemy.Item2;
			var monsterStats = Utils.Bestiary[enemy.Item1];
			monsterToken.PopulateStats(monsterStats);
			var enemySprite = monsterToken.GetNode<Sprite2D>("Sprite2D");
			var atlas = (AtlasTexture)Utils.SpriteSheets[monsterToken.Colour].Duplicate();
			atlas.Region = new Rect2(new Vector2(monsterStats.X * _spriteSize,monsterStats.Y * _spriteSize),new Vector2(_spriteSize,_spriteSize));
			enemySprite.Texture = atlas;
			enemySprite.Scale = new Vector2((float)0.4,(float)0.4);
			monsterToken.Position = new Vector2(_offset*i+60,80);
			AddChild(monsterToken);
			_enemyList.Add(monsterToken);
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
				new Vector2(unitStats.X * GameSettings.CardWidth,unitStats.Y * GameSettings.CardLength),
				new Vector2(GameSettings.CardWidth,GameSettings.CardLength));
			unitSprite.Texture = atlas;
			unitSprite.Scale = new Vector2((float)0.2,(float)0.2);
			unitCard.Position = new Vector2(_cardOffset*i+100,400);
			AddChild(unitCard);
			_unitList.Add(unitCard);
		}
 
		CurrentPhase = Phase.Ranged;
	}

	public void UpdateTargets()
	{
		_targetArmour = 0;
		_targetResistances.Clear();
		_targetFortified = false;
		foreach (var enemy in _enemyList)
		{
			if (enemy.Selected) {
				_targetArmour += enemy.Armour;
				_targetResistances = _targetResistances.Union(enemy.Resistances).ToList();
				// add coldfire resistance to simplify calculations
				if (_targetResistances.Contains(Element.Fire) && _targetResistances.Contains(Element.Ice)) {
					_targetResistances.Add(Element.ColdFire);
				}
				// only need to check for fortififcations during ranged phase
				if (CurrentPhase == Phase.Ranged && !_targetFortified && (enemy.SiteFortifications == 1 || enemy.Abilities.Contains("fortified"))) {
					_targetFortified = true;
				}
			}
		}
		GD.Print("target armour: "+_targetArmour);
		var resistances = "";
		foreach (var item in _targetResistances)
		{
			resistances += item + " ";
		}
		GD.Print("targets resistances: "+ resistances);
		GD.Print("targets fortified: "+_targetFortified.ToString());
		UpdateAttack();
	}

	private void UpdateBlock()
	{
		var swift = MonsterAttacks.GetPressedButton().GetParent<Monster>().Abilities.Contains("swift");
		var inefficientBlock = 0;
		var efficientBlock = PlayerBlocks[(int)Element.ColdFire] + (swift ? PlayerBlocks[7] : 0); // cold fire block is always efficient
		//GD.Print(string.Format("{0} {1}",attackElement.ToString("F"),attackValue));
		switch (TargetAttack.Element)
		{
			case Element.Physical: {
				// all blocks are efficient
				efficientBlock += PlayerBlocks[(int)Element.Physical] + PlayerBlocks[(int)Element.Fire] + PlayerBlocks[(int)Element.Ice] +
				(swift ? PlayerBlocks[(int)Element.Physical+4] + PlayerBlocks[(int)Element.Fire+4] + PlayerBlocks[(int)Element.Ice+4] : 0);
				break;
			}
			case Element.Fire: {
				// ice is efficient
				efficientBlock += PlayerBlocks[(int)Element.Ice] + (swift ? PlayerBlocks[(int)Element.Ice+4] : 0);
				inefficientBlock += PlayerBlocks[(int)Element.Physical] + PlayerBlocks[(int)Element.Fire] +
				(swift ? PlayerBlocks[(int)Element.Physical+4] + PlayerBlocks[(int)Element.Fire+4] : 0);
				break;
			}
			case Element.Ice: {
				// fire is efficient
				efficientBlock += PlayerBlocks[(int)Element.Fire] + (swift ? PlayerBlocks[(int)Element.Fire+4] : 0);
				inefficientBlock += PlayerBlocks[(int)Element.Physical] + PlayerBlocks[(int)Element.Ice] +
				(swift ? PlayerBlocks[(int)Element.Physical+4] + PlayerBlocks[(int)Element.Ice+4] : 0);
				break;
			}
			case Element.ColdFire:{
				// all blocks are inefficient
				inefficientBlock += PlayerBlocks[(int)Element.Physical] + PlayerBlocks[(int)Element.Fire] + PlayerBlocks[(int)Element.Ice] +
				(swift ? PlayerBlocks[(int)Element.Physical+4] + PlayerBlocks[(int)Element.Fire+4] + PlayerBlocks[(int)Element.Ice+4] : 0);
				break;
			}
			default: break;
		}
		_totalBlock = efficientBlock + inefficientBlock / 2;
		GD.Print("total block: "+_totalBlock.ToString());
		GetNode<Button>("ConfirmButton").Disabled = _totalBlock < TargetAttack.Value + (swift ? TargetAttack.Value : 0);
	}

	private void UpdateDamage()
	{
		var abilities = MonsterAttacks.GetPressedButton().GetParent<Monster>().Abilities;
		var brutal = abilities.Contains("brutal") ? 1 : 0;
		var paralyze = abilities.Contains("paralyze");
		var poison = abilities.Contains("poison") ? 1 : 0;
		var assassin = abilities.Contains("assassin");
		var attackValue = _targetAttack.Value + brutal * _targetAttack.Value;
		var wounds = 0;
		// check if any unit selected otherwise assume hero takes damage
		Unit selectedUnit = null;
		for (int i = 0; i < _unitList.Count; i++)
		{
			var unit = _unitList[i];
			if (unit.Selected) {
				selectedUnit = unit;
				break;
			}
		}
		if (selectedUnit != null && !assassin) // unit selected and enemy does not have assassination ability
		{
			
			selectedUnit.Damaged = true;
			// check if unit resists attack
			if (selectedUnit.Resistances.Contains(_targetAttack.Element)) {
				attackValue -= selectedUnit.Armour;
			}
			if (attackValue > 0) { // wound unit
				attackValue -= selectedUnit.Armour;
				if (paralyze) {
					// destroy unit
					GD.Print("unit destroyed");
					selectedUnit.Visible = false;
				} else {
					selectedUnit.Wounds = 1 + poison;
				}
			}
			if (attackValue > 0) { // wound hero
				wounds = DamageHero(attackValue);
			}
		} else {
			wounds = DamageHero(attackValue);
		}
		GD.Print(string.Format("wounds received: {0}",wounds));
		if (wounds >= _maxHandSize) {
			GD.Print("hero is knocked out");
		}
	}

	private int DamageHero(int damage) // damage must always be greater than 0
	{
		return (damage - 1) / _armour + 1; // integer division without having to use Math.Ceiling
	}

	private void UpdateAttack()
	{
		var resistedAttack = 0;
		var effectiveAttack = 0;
		for (int element = 0; element < 4; element++)
		{
			if (_targetResistances.Contains((Element)element)) {
				resistedAttack += PlayerAttacks[(int)AttackRange.Siege + element] +
				(_targetFortified ? 0 : 1) * PlayerAttacks[(int)AttackRange.Ranged + element] +
				// current phase is ranged, melee attacks ignored
				(CurrentPhase == Phase.Ranged ? 0 : 1) * PlayerAttacks[element];
			} else {
				effectiveAttack += PlayerAttacks[(int)AttackRange.Siege + element] +
				(_targetFortified ? 0 : 1) * PlayerAttacks[(int)AttackRange.Ranged + element] +
				(CurrentPhase == Phase.Ranged ? 0 : 1) * PlayerAttacks[element];
			}
		}
		_totalAttack = effectiveAttack + resistedAttack / 2; // integer math automatically rounds down
		GD.Print("final attack: "+_totalAttack.ToString());
		GetNode<Button>("ConfirmButton").Disabled = _totalAttack < _targetArmour;
	}
	
	private void OnNextButtonPressed()
	{
		switch (CurrentPhase)
		{
			//attacking/blocking is optional
			case Phase.Ranged:
			{
				CurrentPhase = Phase.Block;
				GetNode<Button>("NextButton").Text = "Skip Blocking";
				GetNode<Button>("ConfirmButton").Text = "Confirm Block";
				MonsterAttacks = new ButtonGroup();
				// create enemy attacks for undefeated enemies
				for (int i = 0; i < _enemyList.Count; i++)
				{
					var enemy = _enemyList[i];
					if (!enemy.Defeated) {
						enemy.Attack();
					}
				}
				break;
			}
			case Phase.Block:
			{
				CurrentPhase = Phase.Damage;
				GetNode<Button>("NextButton").Text = "Assign All Remaining Damage to Hero";
				GetNode<Button>("ConfirmButton").Text = "Confirm Damage";
				for (int i = 0; i < _enemyList.Count; i++)
				{
					var enemy = _enemyList[i];
					if (!enemy.Defeated && !enemy.Blocked) {
						enemy.Damage();
					}
				}
				break;
			}
			case Phase.Attack:
			{
				GetNode<Button>("NextButton").Text = "Skip Attacking";
				// return to Map with results of combat (either all enemies defeated or not)
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
				if (CheckVictory()) {
					GD.Print("all enemies defeated");
					// exit combat
				}
				GetNode<Button>("ConfirmButton").Disabled = true;
				foreach (var kvp in PlayerAttacks)
				{
					PlayerAttacks[kvp.Key] = 0;
				}
				_totalAttack = 0;
				break;
			}
			case Phase.Block: {
				// block attack
				_targetAttack.Blocked = true;
				MonsterAttacks.GetPressedButton().QueueFree();
				GetNode<Button>("ConfirmButton").Disabled = true;
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
					if (!enemy.Defeated && !enemy.Blocked) {
						allBlocked = false;
						break;
					}
				}
				if (allBlocked) {
					// skip damage phase
					CurrentPhase = Phase.Attack;
					GetNode<Button>("ConfirmButton").Text = "Confirm Attack";
					GetNode<Button>("NextButton").Text = "Skip Attacking";
				}
				break;
			}
			case Phase.Damage: {
				break;
			}
			case Phase.Attack:
			{
				DefeatEnemies();
				// exit combat if all enemies defeated
				if (CheckVictory()) {
					GD.Print("all enemies defeated");
					// exit combat
				}
				GetNode<Button>("ConfirmButton").Disabled = true;
				foreach (var kvp in PlayerAttacks)
				{
					PlayerAttacks[kvp.Key] = 0;
				}
				_totalAttack = 0;
				break;
			}
			default: break;
		}
	}

	private bool CheckVictory()
	{
		var result = true;
		for (int i = 0; i < _enemyList.Count; i++)
		{
			if (!_enemyList[i].Defeated) {
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
			if (enemy.Defeated) {
				remaining--;
			}
		}
		GD.Print("total fame: "+_totalFame.ToString());
		GD.Print("enemies remaining: "+remaining);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
