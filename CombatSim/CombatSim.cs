using Godot;
using System;
using System.Collections.Generic;
using System.Data;

public partial class CombatSim : Node2D
{
	private ButtonGroup _monsterList = new ButtonGroup();
	private ButtonGroup _enemyList = new ButtonGroup();
	private static readonly int _spriteSize = 258;
	private static readonly int _offset = 120;
	private static readonly int _voffset = 120;
	private int _monstersPerRow = 14; // this should depend on viewport size
	private List<(int, int, Color)> _enemies = new List<(int, int, Color)>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var count = 0;
		var row = 0;
		foreach (var kvp in Utils.Bestiary)
		{
			var monsterId = kvp.Key;
			var monster = kvp.Value;
			var atlas = (AtlasTexture)Utils.SpriteSheets[Utils.ConvertMonsterColourToString(monster.Colour)].Duplicate();
			atlas.Region = new Rect2(new Vector2(monster.X * _spriteSize, monster.Y * _spriteSize), new Vector2(_spriteSize, _spriteSize));
            var button = new TextureButton
            {
                TextureNormal = atlas,
                Position = new Vector2(_offset * count + 60, 80 + (_voffset * row)),
                ButtonGroup = _monsterList,
                Scale = new Vector2((float)0.4, (float)0.4)
            };
            count++;
			if (count > _monstersPerRow)
			{
				count = 0;
				row++;
			}
			button.Pressed += () => OnMonsterButtonPressed(monsterId);
			AddChild(button);
		}
	}

	private void OnMonsterButtonPressed(int id)
	{
		var fortifications = GetNode<CheckBox>("SiteCheckBox").ButtonPressed;
		var walls = GetNode<CheckBox>("WallCheckBox").ButtonPressed;
		GD.Print(string.Format("monster #{0} clicked   fortified: {1}   walls: {2}",id,fortifications,walls));
		// add to enemy list
		_enemies.Add((id,(fortifications ? 1 : 0) + (walls ? 1 : 0), Colors.Black));
		UpdateEnemyList();
	}

	private void OnEnemyListButtonPressed(int index)
	{
		GD.Print(string.Format("button with index {0} clicked",index));
		_enemies.RemoveAt(index);
		UpdateEnemyList();
	}

	private void UpdateEnemyList()
	{
		foreach(var oldButton in _enemyList.GetButtons())
		{
			oldButton.QueueFree();
		}
		foreach (var child in GetChildren())
		{
			if (child is Label)
			{
				child.QueueFree();
			}
		}
		for (int i = 0; i < _enemies.Count; i++)
		{
			var monsterId = _enemies[i].Item1;
			var monster = Utils.Bestiary[monsterId];
			var atlas = (AtlasTexture)Utils.SpriteSheets[Utils.ConvertMonsterColourToString(monster.Colour)].Duplicate();
			atlas.Region = new Rect2(new Vector2(monster.X * _spriteSize, monster.Y * _spriteSize), new Vector2(_spriteSize, _spriteSize));
			var horizontalPosition = (_offset + 30) * i + 60;
			var index = i;
            var button = new TextureButton
            {
                TextureNormal = atlas,
                Position = new Vector2(horizontalPosition, 850),
                ButtonGroup = _enemyList,
                Scale = new Vector2((float)0.4, (float)0.4)
            };
			var label = new Label
			{
				Position = new Vector2(horizontalPosition, 980),
				Text = string.Format("Fortifications: {0}",_enemies[i].Item2),
			};
			GD.Print(string.Format("new button with index {0}",index));
			button.Pressed += () => OnEnemyListButtonPressed(index);
			AddChild(button);
			AddChild(label);
		}
	}

	private void OnInitiateSimPressed()
	{
		GameSettings.CombatSim = true;
		GameSettings.EnemyList = _enemies;
		GetTree().ChangeSceneToFile("res://Combat.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
