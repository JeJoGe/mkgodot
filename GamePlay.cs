using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class GamePlay : Node2D
{
	PackedScene monsterScene = GD.Load<PackedScene>("res://Monster/Monster.tscn");
	public string test = "This is test";
	public List<Monster> _enemyList = new List<Monster>();
	
	private static readonly int _monsterSpriteSize = 258;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	// Generate Monster Token and stats, may need to add in variable for whether flipped
	public void MonsterGen(string Colour, int SiteFortifications, Vector2I LocalPos)
	{	
		var mapGen = GetNode<MapGen>("MapGen");
		var enemy = GameSettings.MonsterStacks[Colour].Pop();
		var monsterToken = (Monster)monsterScene.Instantiate();
		monsterToken.mapPos = LocalPos;
		monsterToken.SiteFortifications = SiteFortifications;
		var monsterStats = Utils.Bestiary[enemy];
		monsterToken.PopulateStats(monsterStats);
		var enemySprite = monsterToken.GetNode<Sprite2D>("MonsterSpriteControl/Sprite2D");
		var atlas = (AtlasTexture)Utils.SpriteSheets[monsterToken.Colour].Duplicate();
		atlas.Region = new Rect2(new Vector2(monsterStats.X * _monsterSpriteSize,monsterStats.Y * _monsterSpriteSize),new Vector2(_monsterSpriteSize,_monsterSpriteSize));
		enemySprite.Texture = atlas;
		enemySprite.Scale = new Vector2((float)0.25,(float)0.25);
		monsterToken.GlobalPosition = mapGen.ToGlobal(mapGen.MapToLocal(LocalPos));
		AddChild(monsterToken);
		_enemyList.Add(monsterToken);
	}
}
