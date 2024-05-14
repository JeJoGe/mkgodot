using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Combat : Node2D
{
	private List<Monster> _enemyList = new List<Monster>();
	public Dictionary<int,int> _playerAttacks = new Dictionary<int,int>();
	private int _block, _fireBlock, _iceBlock, _coldFireBlock;
	private int _targetArmour, _totalAttack, _totalBlock;
	private List<Element> _targetResistances = new List<Element>();
	private bool _targetFortified;
	private int _totalFame = 0;
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
	* IceFire	3		7			11
	*/
	public enum AttackRange
	{
		Melee, Ranged = 4, Siege = 8
	}

	private static readonly int _spriteSize = 258;
	private static readonly int _offset = 120;

	public Phase CurrentPhase { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// FOR INITIAL TESTING
		_block = _iceBlock = _fireBlock = _coldFireBlock = 10;
		_playerAttacks[0] = _playerAttacks[1] = _playerAttacks[2] = _playerAttacks[3] = 10;
		_playerAttacks[4] = _playerAttacks[5] = _playerAttacks[6] = _playerAttacks[7] = 10;
		_playerAttacks[8] = _playerAttacks[9] = _playerAttacks[10] = _playerAttacks[11] = 10;
		GameSettings.EnemyList = new List<(int,int)>([(0,0),(1,0),(500,1),(501,0),(2004,2)]);
		Utils.PrintBestiary();
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
				if (!_targetFortified && (enemy.SiteFortifications == 1 || enemy.Abilities.Contains("fortified"))) {
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

	private void UpdateAttack()
	{
		var resistedAttack = 0;
		var effectiveAttack = 0;
		for (int element = 0; element < 4; element++)
		{
			if (_targetResistances.Contains((Element)element)) {
				resistedAttack += _playerAttacks[(int)AttackRange.Siege + element] +
				(_targetFortified ? 0 : 1) * _playerAttacks[(int)AttackRange.Ranged + element] +
				// current phase is ranged, melee attacks ignored
				(CurrentPhase == Phase.Ranged ? 0 : 1) * _playerAttacks[element];
			} else {
				effectiveAttack += _playerAttacks[(int)AttackRange.Siege + element] +
				(_targetFortified ? 0 : 1) * _playerAttacks[(int)AttackRange.Ranged + element] +
				(CurrentPhase == Phase.Ranged ? 0 : 1) * _playerAttacks[element];
			}
		}
		_totalAttack = effectiveAttack + resistedAttack / 2; // integer math automatically rounds down
		GD.Print("final attack: "+_totalAttack.ToString());
		if (_totalAttack >= _targetArmour) {
			GetNode<Button>("ConfirmButton").Disabled = false;
		}
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
				break;
			}
			case Phase.Block:
			{
				CurrentPhase = Phase.Damage;
				break;
			}
			case Phase.Attack:
			{
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
				GetNode<Button>("ConfirmButton").Disabled = true;
				// TODO: 0 player attacks
				break;
			}
			case Phase.Attack:
			{
				DefeatEnemies();
				GetNode<Button>("ConfirmButton").Disabled = true;
				// TODO: 0 player attacks
				break;
			}
			default: break;
		}
	}

	private void DefeatEnemies()
	{
		for (int i = _enemyList.Count - 1; i >= 0; i--)
		{
			var enemy = _enemyList[i];
			if (enemy.Selected)
			{
				_totalFame += enemy.Fame;
				_enemyList.RemoveAt(i);
				enemy.QueueFree();
			}
		}
		GD.Print("total fame: "+_totalFame.ToString());
		GD.Print("enemies remaining: "+_enemyList.Count);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
