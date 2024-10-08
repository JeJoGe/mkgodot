using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class GameSettings : Node
{
	private static int _numPlayers = 1;
	public static int NumPlayers
	{
		get => _numPlayers;
	}
	private static List<string> _expansions = ["base"];
	public static List<string> Expansions
	{
		get => _expansions;
	}
	public static int PlayerCharacter { get; set; }
	public static string PlayerCharacterName { get; set;}
	public static int DummyCharacter { get; set; }
	public static List<(int, int, Color)> EnemyList { get; set; } // monster id, site fortifications: 0 - 2, token colour, old token colour
	public static List<MapToken> ChallengeList = new List<MapToken>(); // may need to make a dictionary of Lists, one list for each player
	public static List<(int, int)> UnitList { get; set; } // unit id, wounds
	public static readonly int CardWidth = 1000;
	public static readonly int CardLength = 1400;

	private static Dictionary<MonsterColour, List<int>> _discardPiles = [];
	private static List<int> _yellowDiscardPile = [];
	private static Stack<int> greenMonsterStack = new Stack<int>();
	private static Stack<int> greyMonsterStack = new Stack<int>();
	private static Stack<int> purpleMonsterStack = new Stack<int>();
	private static Stack<int> brownMonsterStack = new Stack<int>();
	private static Stack<int> redMonsterStack = new Stack<int>();
	private static Stack<int> whiteMonsterStack = new Stack<int>();
	private static Stack<int> yellowTokenStack = new Stack<int>();
	private static Dictionary<MonsterColour, Stack<int>> MonsterStacks = new Dictionary<MonsterColour, Stack<int>>();
	public static bool CombatSim = false;
	public static bool NightTime = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		EnemyList = new List<(int, int, Color)>();
		UnitList = new List<(int, int)> ();

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
			switch (monsterID / 500)
			{
				case 0: // green
					{
						for (int i = 0; i < monster.Value.Count; i++)
						{
							greenMonsterStack.Push(monsterID);
						}
						continue;
					}
				case 1: // grey
					{
						for (int i = 0; i < monster.Value.Count; i++)
						{
							greyMonsterStack.Push(monsterID);
						}
						continue;
					}
				case 2: // purple
					{
						for (int i = 0; i < monster.Value.Count; i++)
						{
							purpleMonsterStack.Push(monsterID);
						}
						continue;
					}
				case 3: // brown
					{
						for (int i = 0; i < monster.Value.Count; i++)
						{
							brownMonsterStack.Push(monsterID);
						}
						continue;
					}
				case 4: // red
					{
						for (int i = 0; i < monster.Value.Count; i++)
						{
							redMonsterStack.Push(monsterID);
						}
						continue;
					}
				case 5: // white
					{
						for (int i = 0; i < monster.Value.Count; i++)
						{
							whiteMonsterStack.Push(monsterID);
						}
						continue;
					}
			}
		}
		MonsterStacks.Add(MonsterColour.Green, new Stack<int>(greenMonsterStack.Shuffle()));
		MonsterStacks.Add(MonsterColour.Grey, new Stack<int>(greyMonsterStack.Shuffle()));
		MonsterStacks.Add(MonsterColour.Purple, new Stack<int>(purpleMonsterStack.Shuffle()));
		MonsterStacks.Add(MonsterColour.Brown, new Stack<int>(brownMonsterStack.Shuffle()));
		MonsterStacks.Add(MonsterColour.Red, new Stack<int>(redMonsterStack.Shuffle()));
		MonsterStacks.Add(MonsterColour.White, new Stack<int>(whiteMonsterStack.Shuffle()));
		// create discard piles for each stack
		_discardPiles.Add(MonsterColour.Green,new List<int>());
		_discardPiles.Add(MonsterColour.Grey,new List<int>());
		_discardPiles.Add(MonsterColour.Purple,new List<int>());
		_discardPiles.Add(MonsterColour.Brown,new List<int>());
		_discardPiles.Add(MonsterColour.Red,new List<int>());
		_discardPiles.Add(MonsterColour.White,new List<int>());
	}

	public static void createYellowStack()
	{
		foreach (var ruin in Utils.RuinEvents)
		{
			int ruinID = ruin.Key;
			yellowTokenStack.Push(ruinID);
		}
		yellowTokenStack = new Stack<int>( yellowTokenStack.Shuffle() );
	}

	public static int DrawRuin()
	{
		if (!yellowTokenStack.TryPop(out int id))
		{
			if(_yellowDiscardPile.Count > 0)
			{
				yellowTokenStack = new Stack<int>(_yellowDiscardPile.Shuffle());
				_yellowDiscardPile.Clear();
				id = yellowTokenStack.Pop();
			}
			else
			{
				id = -1;
			}
			
		}
		return id;
	}
	public static int DrawMonster(MonsterColour colour)
	{
		if (!MonsterStacks[colour].TryPop(out int id))
		{
			if (_discardPiles[colour].Count > 0)
			{
				// reshuffle discard piles of colour and draw again
				MonsterStacks[colour] = new Stack<int>(_discardPiles[colour].Shuffle());
				_discardPiles[colour].Clear();
				id = MonsterStacks[colour].Pop();
			}
			else
			{
				// TODO: need to handle case where stack is empty and no discarded tokens are available
				id = -1;
			}
		}
		return id;
	}

	public static void DiscardToken(int id)
	{
		MonsterColour colour = (MonsterColour)(id / 500);
		_discardPiles[colour].Add(id);
	}
	public static void DiscardYellowToken(int id)
	{
		_yellowDiscardPile.Add(id);
	}
}
