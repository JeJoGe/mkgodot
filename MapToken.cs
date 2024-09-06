using Godot;
using System;

public partial class MapToken : Node2D
{
	[Export]
	ColorRect colorRect;
	public string Colour { get; set; }
	public Vector2I MapPosition { get; set; }
	public int SiteFortifications { get; set; }
	public int TokenId { get; set; }
	private MapGen mapGen;
	private static readonly int _tokenSpriteSize = 258;
	private Color _oldPosColour = Colors.Black;
	public Color OldPosColour { get; set; }
	private Color _posColour = Colors.Black;
	PackedScene MonsterGroupScene = GD.Load<PackedScene>("res://MonsterGroup.tscn");

	public Color PosColour
	{
		get => _posColour;
		set
		{
			if (OldPosColour != PosColour)
			{
				OldPosColour = PosColour;
			}
			_posColour = value;
			colorRect.Color = _posColour;
			if (_posColour == Colors.Black)
			{
				colorRect.Visible = false;
			}
			else if (colorRect.Visible == false)
			{
				colorRect.Visible = true;
			}
		}
	}
	private bool _facedown = true;
	public bool Facedown
	{
		get => _facedown;
		set
		{
			int X;
			int Y;
			if(TokenId == -1)
			{
				X = 0;
				Y = 0;
			}
			else if (Colour != "yellow")
			{
				var stats = Utils.Bestiary[TokenId];
				X = stats.X;
				Y = stats.Y;
			}
			else
			{
				var stats = Utils.RuinEvents[TokenId];
				X = stats.X;
				Y = stats.Y;
			}
			var Sprite = GetNode<Sprite2D>("MapTokenControl/Sprite2D");
			var atlas = (AtlasTexture)Utils.SpriteSheets[Colour].Duplicate();

			if (this._facedown != value)
			{
				atlas.Region = new Rect2(new Vector2(X * _tokenSpriteSize, Y * _tokenSpriteSize), new Vector2(_tokenSpriteSize, _tokenSpriteSize));
				if (Colour == "yellow")
				{
					if (Utils.RuinEvents[TokenId].Event == "monster")
					{
						var monsterGroup = (MonsterGroup)MonsterGroupScene.Instantiate();
						monsterGroup.MapPosition = MapPosition;
					}
				}
				_facedown = value;
			}
			else
			{
				atlas.Region = new Rect2(new Vector2(0,0), new Vector2(_tokenSpriteSize, _tokenSpriteSize));
			}
			Sprite.Texture = atlas;
			Sprite.Scale = new Vector2((float)0.25, (float)0.25);
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		colorRect.Visible = false;
		mapGen = GetNode<MapGen>("../MapGen");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
