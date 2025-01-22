using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Node2D
{
	// only time a die can remain in inventory without being used
	private int _manaStolenDie = -1; // -1 indicates no mana die stolen
	public int ManaStolenDie
	{
		get => _manaStolenDie;
	}
	private Dictionary<Source.Colour, int> _crystals = new Dictionary<Source.Colour, int>{
		{Source.Colour.Blue,0},
		{Source.Colour.Red,0},
		{Source.Colour.Green,0},
		{Source.Colour.White,0}
	};
	private Dictionary<Source.Colour, int> _tokens = new Dictionary<Source.Colour, int>{
		{Source.Colour.Blue,0},
		{Source.Colour.Red,0},
		{Source.Colour.Green,0},
		{Source.Colour.White,0},
		{Source.Colour.Gold,0},
		{Source.Colour.Black,0}
	};
	[Export]
	private Label _blueCrystalLabel, _redCrystalLabel, _greenCrystalLabel, _whiteCrystalLabel;
	[Export]
	private Polygon2D _blueCrystal, _redCrystal, _greenCrystal, _whiteCrystal;
	[Export]
	private Sprite2D _blueMana, _redMana, _greenMana, _whiteMana, _goldMana, _blackMana;
	[Export]
	private Label _blueManaLabel, _redManaLabel, _greenManaLabel, _whiteManaLabel, _goldManaLabel, _blackManaLabel;
	private List<ManaDie> _usedDiceScenes = new List<ManaDie>();
	private ManaDie _manaStolenDieScene;
	private PackedScene _dieScene = GD.Load<PackedScene>("res://ManaDie.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// TESTING ONLY
		_crystals[Source.Colour.Blue] = 3;
		_crystals[Source.Colour.Red] = 3;
		_crystals[Source.Colour.Green] = 3;
		_crystals[Source.Colour.White] = 3;
	}

	// Called every frame. 'delta' is the elapsed time since  the previous frame.
	public override void _Process(double delta)
	{
	}

	public int CrystalCount(Source.Colour colour)
	{
		return _crystals[colour];
	}

	public int TokenCount(Source.Colour colour)
	{
		return _tokens[colour];
	}

	public bool AddCrystal(int colour)
	{
		var crystalColour = (Source.Colour)colour;
		var result = false;
		if (_crystals[crystalColour] < 3 && colour >= 0 && colour < 4) // max of 3 crystals and must be blue/red/green/white
		{
			result = true;
			_crystals[crystalColour] = _crystals[crystalColour] + 1;
			GD.Print(string.Format("{0} token added", crystalColour.ToString()));
			GD.Print(string.Format("{0} {1} tokens currently", _crystals[crystalColour], crystalColour.ToString()));
			UpdateCrystal(crystalColour);
		}
		return result;
	}

	public bool ConsumeCrystal(int colour)
	{
		var crystalColour = (Source.Colour)colour;
		return ConsumeCrystal(crystalColour);
	}

	public bool ConsumeCrystal(Source.Colour colour)
	{
		var result = false;
		if (_crystals[colour] > 0)
		{
			result = true;
			_crystals[colour] = _crystals[colour] - 1;
			UpdateCrystal(colour);
		}
		return result;
	}

	private void UpdateCrystal(Source.Colour colour)
	{
		var newString = _crystals[colour].ToString();
		var visible = _crystals[colour] > 0;
		switch (colour)
		{
			case Source.Colour.Blue:
				{
					_blueCrystalLabel.Text = newString;
					_blueCrystalLabel.Visible = visible;
					_blueCrystal.Visible = visible;
					break;
				}
			case Source.Colour.Red:
				{
					_redCrystalLabel.Text = newString;
					_redCrystalLabel.Visible = visible;
					_redCrystal.Visible = visible;
					break;
				}
			case Source.Colour.Green:
				{
					_greenCrystalLabel.Text = newString;
					_greenCrystalLabel.Visible = visible;
					_greenCrystal.Visible = visible;
					break;
				}
			case Source.Colour.White:
				{
					_whiteCrystalLabel.Text = newString;
					_whiteCrystalLabel.Visible = visible;
					_whiteCrystal.Visible = visible;
					break;
				}
			default: break;
		}
	}

	private void UpdateToken(Source.Colour colour)
	{
		var newString = _tokens[colour].ToString();
		var visible = _tokens[colour] > 0;
		switch (colour) 
		{
			case Source.Colour.Blue:
				{
					_blueManaLabel.Text = newString;
					_blueManaLabel.Visible = visible;
					_blueMana.Visible = visible;
					break;
				}
			case Source.Colour.Red:
				{
					_redManaLabel.Text = newString;
					_redManaLabel.Visible = visible;
					_redMana.Visible = visible;
					break;
				}
			case Source.Colour.Green:
				{
					_greenManaLabel.Text = newString;
					_greenManaLabel.Visible = visible;
					_greenMana.Visible = visible;
					break;
				}
			case Source.Colour.White:
				{
					_whiteManaLabel.Text = newString;
					_whiteManaLabel.Visible = visible;
					_whiteMana.Visible = visible;
					break;
				}
			case Source.Colour.Gold:
				{
					_goldManaLabel.Text = newString;
					_goldManaLabel.Visible = visible;
					_goldMana.Visible = visible;
					break;
				}
			case Source.Colour.Black:
				{
					_blackManaLabel.Text = newString;
					_blackManaLabel.Visible = visible;
					_blackMana.Visible = visible;
					break;
				}
			default: break;
		}
	}

	public bool AddToken(int colour)
	{
		var tokenColour = (Source.Colour)colour;
		var result = false;
		if (colour >= 0 && colour < 6) // no limit on tokens
		{
			result = true;
			_tokens[tokenColour] = _tokens[tokenColour] + 1;
			GD.Print(string.Format("{0} token added", tokenColour.ToString()));
			GD.Print(string.Format("{0} {1} tokens currently", _tokens[tokenColour], tokenColour.ToString()));
			UpdateToken(tokenColour);
		}
		return result;
	}

	public bool ConsumeToken(int colour)
	{
		var tokenColour = (Source.Colour)colour;
		return ConsumeToken(tokenColour);
	}

	public bool ConsumeToken(Source.Colour colour)
	{
		var result = false;
		if (_tokens[colour] > 0)
		{
			result = true;
			_tokens[colour] = _tokens[colour] - 1;
			UpdateToken(colour);
		}
		return result;
	}

	// create die image for each used die this turn
	private void OnDieTaken(int colour)
	{
		// update die image
		if (colour != -1 && colour < 6)
		{
			var dieColour = (Source.Colour)colour;
			GD.Print(string.Format("die taken of colour {0}", dieColour.ToString()));
			// TODO: mana stolen die should send different signal
			//_manaStolenDie = colour;
			// create die scene
			var manaDie = _dieScene.Instantiate<ManaDie>();
			var atlas = (AtlasTexture)Utils.SpriteSheets["dice"].Duplicate();
			atlas.Region = new Rect2(new Vector2(Utils.DiceCoordinates[dieColour].Item1 * Utils.DiceSize,
				Utils.DiceCoordinates[dieColour].Item2 * Utils.DiceSize),
				new Vector2(Utils.DiceSize, Utils.DiceSize));
			manaDie.GetNode<Sprite2D>("DieImage").Texture = atlas;
			manaDie.Position = new Vector2(_usedDiceScenes.Count * 50, 0);
			_usedDiceScenes.Add(manaDie);
			AddChild(manaDie);
		}
	}

	private void ConsumeDie()
	{
		_manaStolenDie = -1;
		_manaStolenDieScene.QueueFree();
		// TODO: return mana stolen die at end of turn
	}

	private void OnEndTurn()
	{
		// tokens disappear at end of turn
		foreach (var colour in _tokens)
		{
			_tokens[colour.Key] = 0;
		}
		// remove dice imagess
		for (int i = _usedDiceScenes.Count - 1; i >= 0; i--)
		{
			var dieImage = _usedDiceScenes[i];
			_usedDiceScenes.RemoveAt(i);
			dieImage.QueueFree();
		}
	}
}
