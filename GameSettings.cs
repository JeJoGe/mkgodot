using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class GameSettings : Node
{
	private static int _numPlayers = 1;
	public static int NumPlayers {
		get => _numPlayers;
	}
	public static int PlayerCharacter { get; set; }
	public static int DummyCharacter { get; set; }
	public static List<(int,int)> EnemyList { get; set; } // monster id, site fortifications: 0 - 2
	public static List<(int,int)> UnitList { get; set; } // unit id, wounds
	public static readonly int CardWidth = 1000;
	public static readonly int CardLength = 1400;

	public static Stack<int> greenMonsterStack = new Stack<int>();
	public static Stack<int> greyMonsterStack = new Stack<int>();
	public static Stack<int> purpleMonsterStack = new Stack<int>();
	public static Stack<int> brownMonsterStack = new Stack<int>();
	public static Stack<int> redMonsterStack = new Stack<int>();
	public static Stack<int> whiteMonsterStack = new Stack<int>();
	public static Dictionary<String,Stack<int>> MonsterStacks = new Dictionary<string, Stack<int>>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Creates a stack of shuffled monster IDs (ints) for each color of monster
	// Creates dictionary with kvp color (string) and stack<int>. Called after loading bestiary in Utils
	public static void createMonsterStacks()
	{
		// Takes monsters from Bestiary
		foreach (var monster in Utils.Bestiary)
		{

			int monsterID = monster.Key;
			// monster keys are token IDs, every 500 is different color
			switch (monsterID/500)
			{
				case 0: // green
					{
						greenMonsterStack.Push(monsterID);
						continue;
					}
				case 1: // grey
					{
						greyMonsterStack.Push(monster.Key);
						continue;
					}
				case 2: // purple
					{
						purpleMonsterStack.Push(monster.Key);
						continue;
					}
				case 3: // brown
					{
						brownMonsterStack.Push(monster.Key);
						continue;
					}
				case 4: // red
					{
						redMonsterStack.Push(monster.Key);
						continue;
					}
				case 5: // white
					{
						whiteMonsterStack.Push(monster.Key);
						continue;
					}
			}
		}
		MonsterStacks.Add("green", new Stack<int> (greenMonsterStack.Shuffle()));
		MonsterStacks.Add("grey", new Stack<int> (greyMonsterStack.Shuffle()));
		MonsterStacks.Add("purple", new Stack<int> (purpleMonsterStack.Shuffle()));
		MonsterStacks.Add("brown", new Stack<int> (brownMonsterStack.Shuffle()));
		MonsterStacks.Add("red", new Stack<int> (redMonsterStack.Shuffle()));
		MonsterStacks.Add("white", new Stack<int> (whiteMonsterStack.Shuffle()));
	}
}
