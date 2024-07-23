using Godot;
using System;
using System.Collections.Generic;

public partial class ChallengePopUp : Control
{	
	[Export]
	private ChallengeOptions ChallengeOptions;
	[Export]
	private Button Challenge;
	[Export]
	private Button Cancel;
	List<CheckBox> enemyCheckboxes;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var optionOffset = 0;
		enemyCheckboxes = new List<CheckBox>();
		GetNode<ColorRect>("ColorRect").Size = new Vector2 (350, 132 + (32 * GameSettings.EnemyList.Count));
		GetNode<Window>("..").Size = new Vector2I(350, 164 + (32 * GameSettings.EnemyList.Count));
		GetNode<PanelContainer>("PanelContainer").Size = new Vector2 (310, 32 * GameSettings.EnemyList.Count);
		ChallengeOptions.Size = new Vector2 (310, 32 * GameSettings.EnemyList.Count);
		foreach (var enemy in GameSettings.EnemyList)
		{
			enemyCheckboxes.Add(ChallengeOptions.createChallengeOption(enemy.Item1, optionOffset, enemy.Item3, enemy.Item4));
			optionOffset += 32;
		}
		Challenge.Position = new Vector2 (0, 132 + optionOffset);
		Cancel.Position = new Vector2 (175, 132 + optionOffset);
		GetTree().Paused = true;
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_challenge_pressed()
	{

		foreach (var checkBox in enemyCheckboxes)
		{
			for (var enemy = GameSettings.EnemyList.Count -1; enemy >= 0; enemy--)
			{
				//if checkbox text (subject to change) equals monster id and it's checkbox is unchecked. go backwards because removing from list while iterating
				if (checkBox.Text == Utils.Bestiary[GameSettings.EnemyList[enemy].Item1].Name && checkBox.ButtonPressed == false)
				{
					GameSettings.EnemyList.Remove(GameSettings.EnemyList[enemy]);
				}
			}
		}
		GD.Print(GameSettings.EnemyList.Count);
		if (GameSettings.EnemyList.Count != 0){
			GetNode<Player>("../../Player").InitiateCombat();
		}
		GetTree().Paused = false;
		GetNode<Window>("..").QueueFree();
	}
	private void _on_cancel_pressed()
	{
		Utils.undoRedo.Undo();
		GameSettings.EnemyList.Clear();
		GetTree().Paused = false;
		GetNode<Window>("..").QueueFree();
	}
}
