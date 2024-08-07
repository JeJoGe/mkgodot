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
	private ManaDie _manaDieScene;
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

	public int CrytalCount(Source.Colour colour)
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
		}
		return result;
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
		}
		return result;
	}

	private void OnDieTaken(int colour)
	{
		// update die image
		if (colour != -1 && colour < 6)
		{
			var dieColour = (Source.Colour)colour;
			GD.Print(string.Format("die taken of colour {0}", dieColour.ToString()));
			_manaStolenDie = colour;
			// create die scene
			var manaDie = _dieScene.Instantiate<ManaDie>();
			var atlas = (AtlasTexture)Utils.SpriteSheets["dice"].Duplicate();
			atlas.Region = new Rect2(new Vector2(Utils.DiceCoordinates[dieColour].Item1 * Utils.DiceSize,
				Utils.DiceCoordinates[dieColour].Item2 * Utils.DiceSize),
				new Vector2(Utils.DiceSize, Utils.DiceSize));
			manaDie.GetNode<Sprite2D>("DieImage").Texture = atlas;
			_manaDieScene = manaDie;
			AddChild(manaDie);
		}
	}

	private void ConsumeDie()
	{
		_manaStolenDie = -1;
		_manaDieScene.QueueFree();
	}

	private void OnEndTurn()
	{
		// tokens disappear at end of turn
		foreach (var colour in _tokens)
		{
			_tokens[colour.Key] = 0;
		}
	}
}
