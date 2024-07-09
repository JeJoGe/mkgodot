using Godot;
using System;
using System.Collections.Generic;

public partial class ChallengePopUp : Control
{	
	[Export]
	private ChallengeOptions ChallengeOptions;
	List<CheckBox> enemyCheckboxes;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var optionOffset = 0;
		enemyCheckboxes = new List<CheckBox>();
		GetNode<ColorRect>("ColorRect").Size = new Vector2 (350,70 + (30 * GameSettings.EnemyList.Count));
		GetNode<PanelContainer>("PanelContainer").Size = new Vector2 (310, 32 * GameSettings.EnemyList.Count);
		ChallengeOptions.Size = new Vector2 (310, 32 * GameSettings.EnemyList.Count);
		foreach (var enemy in GameSettings.EnemyList)
		{
			enemyCheckboxes.Add(ChallengeOptions.createChallengeOption(enemy[0], optionOffset));
			optionOffset += 32;
		}
		foreach (var check in enemyCheckboxes)
		{
			check.ButtonPressed = !check.ButtonPressed;
			GD.Print("test");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
