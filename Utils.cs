using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

public partial class Utils : Node
{
	private static readonly Random random = new Random();
	private static readonly object syncLock = new object();
	public static UndoRedo undoRedo = new UndoRedo();
	private static readonly Dictionary<string, string> _sheetPaths = new Dictionary<string, string>{
		{"green", "res://assets/TokenImages/GreenTokens/green_tokens_sheet.png"},
		{"grey","res://assets/TokenImages/GreyTokens/grey_tokens_sheet.png"},
		{"purple","res://assets/TokenImages/PurpleTokens/purple_tokens_sheet.png"},
		{"brown","res://assets/TokenImages/BrownTokens/brown_tokens_sheet.png"},
		{"red","res://assets/TokenImages/RedTokens/red_tokens_sheet.png"},
		{"white","res://assets/TokenImages/WhiteTokens/white_tokens_sheet.png"},
		{"yellow","res://assets/TokenImages/YellowTokens/yellow_tokens_sheet.png"},
		{"silver","res://assets/silverunits.jpg"},
		{"gold","res://assets/goldunits.jpg"},
		{"dice","res://assets/dice.jpg"}
	};
	private static readonly Dictionary<string,MonsterColour> _stringToColours = new Dictionary<string, MonsterColour>{
		{"green", MonsterColour.Green},
		{"grey", MonsterColour.Grey},
		{"purple", MonsterColour.Purple},
		{"brown", MonsterColour.Brown},
		{"red", MonsterColour.Red},
		{"white", MonsterColour.White}
	};
	private static readonly Dictionary<MonsterColour,string> _coloursToStrings = new Dictionary<MonsterColour,string>{
		{MonsterColour.Green, "green"},
		{MonsterColour.Grey, "grey"},
		{MonsterColour.Purple, "purple"},
		{MonsterColour.Brown, "brown"},
		{MonsterColour.Red, "red"},
		{MonsterColour.White, "white"}
	};	
	public static Dictionary<string, AtlasTexture> SpriteSheets = new Dictionary<string, AtlasTexture>();
	public static Dictionary<int,MonsterObject> Bestiary;
	public static Dictionary<int,UnitObject> UnitStats;
	public static Dictionary<int,RuinObject> RuinEvents;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadSprites();
		LoadBestiary();
		GameSettings.createMonsterStacks();
		LoadUnits();
		LoadRuins();
		GameSettings.createYellowStack();
	}

	public static MonsterColour ConvertStringToMonsterColour(string colour)
	{
		return _stringToColours[colour];
	}

	public static string ConvertMonsterColourToString(MonsterColour colour)
	{
		return _coloursToStrings[colour];
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public static int RandomNumber(int min, int max)
	{
		lock (syncLock)
		{
			return random.Next(min, max);
		}
	}

	private void LoadSprites()
	{
		foreach (var kvp in _sheetPaths)
		{
			var atlas = new AtlasTexture();
			atlas.Atlas = (Texture2D)GD.Load(_sheetPaths[kvp.Key]);
			SpriteSheets[kvp.Key] = atlas;
		}
	}

	private void LoadBestiary()
	{
		StreamReader sr = new StreamReader("./Monster/monsters.json");
		string json = sr.ReadToEnd();
		Bestiary = JsonConvert.DeserializeObject<Dictionary<int, MonsterObject>>(json);
	}

	private void LoadUnits()
	{
		StreamReader sr = new StreamReader("./Unit/units.json");
		string json = sr.ReadToEnd();
		UnitStats = JsonConvert.DeserializeObject<Dictionary<int, UnitObject>>(json);
	}

	private void LoadRuins()
	{
		StreamReader sr = new StreamReader("./Ruin/ruins.json");
		string json = sr.ReadToEnd();
		RuinEvents = JsonConvert.DeserializeObject<Dictionary<int, RuinObject>>(json);
	}

	public static void PrintBestiary()
	{
		GD.Print("Bestiary\n=====");
		foreach (var kvp in Bestiary)
		{
			GD.Print(string.Format("{0} {1}",kvp.Key,kvp.Value.Name));
			if (kvp.Value.Abilities.Count > 0) {
				GD.Print(string.Format("Abilities: {0}",kvp.Value.Abilities.First()));
			}
			var resistances = "";
			foreach (var element in kvp.Value.Resistances)
			{
				resistances = resistances + element.ToString("F") + " ";
			}
			GD.Print(string.Format("Armour: {0} Resists: {1}",kvp.Value.Armour,resistances));
			GD.Print(string.Format("{0} {1}",kvp.Value.Attacks.First().Element, kvp.Value.Attacks.First().Value.ToString("F")));
			GD.Print("=====");
		}
	}	
}

public class MonsterObject
{
	public string Name { get; set; }
	public int Armour { get; set; }
	public int Fame { get; set; }
	public List<string> Abilities { get; set; }
	public List<Element> Resistances { get; set; }
	public List<MonsterAttack> Attacks { get; set; }
	public MonsterColour Colour { get; set; }
	public int X { get; set; } // this corresponds to the x offset on its corresponding spritesheet
	public int Y { get; set; } // this corresponds to the y offset on its corresponding spritesheet
	public string Version { get; set; }
	public int Count { get; set; }
}

public class UnitObject
{
	public string Name { get; set; }
	public int Armour { get; set; }
	public List<string> Abilities { get; set; }
	public List<Element> Resistances { get; set; }
	public List<UnitType> Types { get; set; }
	public int Level { get; set; }
	public int Cost { get; set; }
	public int X { get; set; }
	public int Y { get; set; }
	public string Version { get; set; }
	public int Count { get; set; }
}

public class RuinObject
{
	public string Name { get; set; }
	public string Event { get; set; }
	public List<string> Requirements { get; set; }
	public List<string> Rewards { get; set; }
	public int X { get; set; }
	public int Y { get; set; }
	public string Version { get; set; }
}
public class MonsterAttack
{
	public int Value { get; set; }
	public Element Element { get; set; }
	public bool Blocked { get; set; } = false;
	public bool Attacked { get; set; } = false;
	public bool Attacking { get; set; } = true;
}

public enum MonsterColour
{
	Green, Grey, Purple, Brown, Red, White
}

public enum Element
{
	Physical, Fire, Ice, ColdFire, Summon = 9
}

public enum UnitType
{
	Village, Keep, MageTower, Monastery, City, Glade
}

public static class EnumerableExtensions
{
	public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
	{
		return source.ShuffleIterator();
	}

	// shuffle using fisher-yates algo
	public static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source)
	{
		var buffer = source.ToList();
		//Step 1: For each unshuffled item in the collection
		for (int i = 0; i < buffer.Count; i++)
		{
			//Step 2: Randomly pick an item which has not been shuffled
			int j = Utils.RandomNumber(i, buffer.Count);
			yield return buffer[j];

			//Step 3: Swap the selected item with the last "unstruck" letter in the collection
			buffer[j] = buffer[i];
		}
	}
}