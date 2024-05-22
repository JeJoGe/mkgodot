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
	private static readonly Dictionary<string, string> _sheetPaths = new Dictionary<string, string>{
		{"green", "res://assets/TokenImages/GreenTokens/green_tokens_sheet.png"},
		{"grey","res://assets/TokenImages/GreyTokens/grey_tokens_sheet.png"},
		{"purple","res://assets/TokenImages/PurpleTokens/purple_tokens_sheet.png"},
		{"brown","res://assets/TokenImages/BrownTokens/brown_tokens_sheet.png"},
		{"red","res://assets/TokenImages/RedTokens/red_tokens_sheet.png"},
		{"white","res://assets/TokenImages/WhiteTokens/white_tokens_sheet.png"}
	};
	public static Dictionary<string, AtlasTexture> SpriteSheets = new Dictionary<string, AtlasTexture>();
	public static Dictionary<int,MonsterObject> Bestiary;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadSprites();
		LoadBestiary();
		GameSettings.createMonsterStacks();
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
	public string Colour { get; set; }
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

public class MonsterAttack
{
	public int Value { get; set; }
	public Element Element { get; set; }
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