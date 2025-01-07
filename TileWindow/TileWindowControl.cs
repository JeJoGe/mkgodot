using Godot;
using System;

public partial class TileWindowControl : NinePatchRect
{
	TileWindow TWindow;
	private static readonly int _tokenSpriteSize = 258;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TWindow = GetNode<TileWindow>("..");
		GetNode<Label>("TerrainText").Text = TWindow.Terrain;
		GetNode<Label>("ActionsText").Text = TWindow.TileEvent;
		Sprite2D tokenSprite = GetNode<Sprite2D>("TokenSprite");
		tokenSprite.Texture = createTokenSprite(TWindow.Token);
		var x = 1;
		var y = 720;
		foreach (var monster in TWindow.MonsterGroup)
		{
			var monsterSprite = new Sprite2D();
			monsterSprite.Position = new Vector2(550*x, y);
			monsterSprite.Texture = createTokenSprite(monster);
			AddChild(monsterSprite);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void on_button_pressed()
	{
		TWindow.QueueFree();
	}

	public AtlasTexture createTokenSprite(MapToken mapToken)
	{
		var atlas = (AtlasTexture)Utils.SpriteSheets[TWindow.Token.Colour].Duplicate();
		int X;
		int Y;
		if(TWindow.Token.TokenId == -1)
		{
			X = 0;
			Y = 0;
		}
		else if (TWindow.Token.Colour != "yellow")
		{
			var stats = Utils.Bestiary[TWindow.Token.TokenId];
			X = stats.X;
			Y = stats.Y;
		}
		else
		{
			var stats = Utils.RuinEvents[TWindow.Token.TokenId];
			X = stats.X;
			Y = stats.Y;
		}
		atlas.Region = new Rect2(new Vector2(X * _tokenSpriteSize, Y * _tokenSpriteSize), new Vector2(_tokenSpriteSize, _tokenSpriteSize));
		return atlas;
	}
}
