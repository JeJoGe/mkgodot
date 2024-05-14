using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

public partial class Combat : Node2D
{
	private List<Monster> _enemyList = new List<Monster>();
	private int _siegeFire, _siegeIce, _siegeColdFire, _siege, _ranged, _rangedFire, _rangedIce, _rangedColdFire;
	private int _block, _fireBlock, _iceBlock, _coldFireBlock;
	private int _attack, _fireAttack, _iceAttack, _coldFireAttack;
	public enum Phase
	{
		Ranged,Block,Damage,Attack
	}

	public Phase CurrentPhase { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// FOR INITIAL TESTING
		_ranged = _rangedFire = _rangedIce = _rangedColdFire = 10;
		_siege = _siegeColdFire = _siegeFire = _siegeIce = 10;
		_block = _iceBlock = _fireBlock = _coldFireBlock = 10;
		_attack = _fireAttack = _iceAttack = _coldFireAttack = 10;
		GameSettings.EnemyList = new List<(int,bool)>([(1,false),(2,false),(2,true)]);
		// get dictionary of enemy tokens
		StreamReader sr = new StreamReader("./Monster/monsters.json");
		string json = sr.ReadToEnd();
		var monsters = JsonConvert.DeserializeObject<Dictionary<int,Monster>>(json);
		// create enemy tokens
		foreach (var enemy in GameSettings.EnemyList)
		{
			var monsterToken = new Monster(monsters[enemy.Item1]);
			monsterToken.SiteFortifications = enemy.Item2;
			_enemyList.Add(monsterToken);
		}
		//PrintBestiary(monsters);
		PrintEnemies();

		CurrentPhase = Phase.Ranged;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void PrintBestiary(Dictionary<int,Monster> bestiary)
	{
		foreach (var kvp in bestiary)
		{
			GD.Print(string.Format("{0} {1}",kvp.Key,kvp.Value.Name));
			if (kvp.Value.Abilities.Count > 0) {
				GD.Print(string.Format("Abilities: {0}",kvp.Value.Abilities.First()));
			}
			var resistances = "";
			foreach (var element in kvp.Value.Resistances)
			{
				resistances = resistances + element + " ";
			}
			GD.Print(string.Format("Armour: {0} Resists: {1}",kvp.Value.Armour,resistances));
			GD.Print(string.Format("{0} {1}",kvp.Value.Attacks.First().Type, kvp.Value.Attacks.First().Value));
			GD.Print("=====");
		}
	}

	private void PrintEnemies() {
		foreach (var enemy in _enemyList)
		{
			GD.Print(string.Format("{0}",enemy.Name));
			if (enemy.Abilities.Count > 0) {
				GD.Print(string.Format("Abilities: {0}",enemy.Abilities.First()));
			}
			var resistances = "";
			foreach (var element in enemy.Resistances)
			{
				resistances = resistances + element + " ";
			}
			GD.Print(string.Format("Armour: {0} Resists: {1}",enemy.Armour,resistances));
			GD.Print(string.Format("{0} {1}",enemy.Attacks.First().Type, enemy.Attacks.First().Value));
			GD.Print("Fortified by site: "+enemy.SiteFortifications.ToString());
			GD.Print("=====");
		}
	}
}
