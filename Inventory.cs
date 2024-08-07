using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Node2D
{
	private Dictionary<Source.Colour, int> _dice = new Dictionary<Source.Colour, int>{
		{Source.Colour.Blue,0},
		{Source.Colour.Red,0},
		{Source.Colour.Green,0},
		{Source.Colour.White,0},
		{Source.Colour.Gold,0},
		{Source.Colour.Black,0}
	};
	private static readonly Dictionary<Source.Colour, (int, int)> _diceCoordinates = new Dictionary<Source.Colour, (int, int)>{
		{Source.Colour.Blue,(0,1)},
		{Source.Colour.Red,(2,1)},
		{Source.Colour.Green,(1,1)},
		{Source.Colour.White,(1,2)},
		{Source.Colour.Gold,(2,2)},
		{Source.Colour.Black,(0,2)}
	};
	public Dictionary<Source.Colour, int> _crystals = new Dictionary<Source.Colour, int>{
		{Source.Colour.Blue,0},
		{Source.Colour.Red,0},
		{Source.Colour.Green,0},
		{Source.Colour.White,0}
	};
	public Dictionary<Source.Colour, int> _tokens =  new Dictionary<Source.Colour, int>{
		{Source.Colour.Blue,0},
		{Source.Colour.Red,0},
		{Source.Colour.Green,0},
		{Source.Colour.White,0},
		{Source.Colour.Gold,0},
		{Source.Colour.Black,0}
	};
	private Dictionary<Source.Colour, ManaDie> _diceScenes = new Dictionary<Source.Colour, ManaDie>();
	private PackedScene _dieScene = GD.Load<PackedScene>("res://ManaDie.tscn");
	private static readonly float _diceSize = 682.66F;
	private static readonly int _offset = 60;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since  the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnDieTaken(int colour)
	{
		var dieColour = (Source.Colour)colour;
		GD.Print(string.Format("die taken of colour {0}", dieColour.ToString()));
		_dice[dieColour]++;
		// update dice images
		foreach (var die in _dice)
		{
			if (die.Value > 0)
			{
				// create die if it does not already exist
				if (!_diceScenes.ContainsKey(die.Key))
				{
					var manaDie = _dieScene.Instantiate<ManaDie>();
					var atlas = (AtlasTexture)Utils.SpriteSheets["dice"].Duplicate();
					atlas.Region = new Rect2(new Vector2(_diceCoordinates[die.Key].Item1 * _diceSize, _diceCoordinates[die.Key].Item2 * _diceSize),
					 new Vector2(_diceSize, _diceSize));
					manaDie.GetNode<Sprite2D>("DieImage").Texture = atlas;
					manaDie.Position = new Vector2(_offset * _diceScenes.Count, 0);
					_diceScenes[die.Key] = manaDie;
					AddChild(manaDie);
				}
				_diceScenes[die.Key].Count = die.Value;
			}
		}
	}

	private void OnEndTurn()
	{
		// tokens disappear at end of turn
		foreach (var colour in _tokens)
		{
			_tokens[colour.Key] = 0;
		}
		// dice is returned at end of turn
		foreach (var colour in _dice)
		{
			_dice[colour.Key] = 0;
			if (_diceScenes.ContainsKey(colour.Key))
			{
				_diceScenes[colour.Key].Count = 0;
				_diceScenes[colour.Key].Visible = false;
			}
		}
	}
}
