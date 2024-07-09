using Godot;
using System;

public partial class ChallengeOptions : VBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public CheckBox createChallengeOption(int enemyId, int optionOffset)
	{
		var enemyCheckBox = new CheckBox
		{
			Text = enemyId.ToString(),
			Position = new Vector2(0, optionOffset),
			Name = string.Format("Monster{0}", enemyId)
		};
		enemyCheckBox.SetPressedNoSignal(true);
		AddChild(enemyCheckBox);
		//enemyCheckBox.Pressed += () => OnEnemyCheckBoxToggled(enemyCheckBox);
		return (enemyCheckBox);
	}

	/*private void OnEnemyCheckBoxToggled(CheckBox enemyCheckBox)
	{
		GD.Print("TEST");
		enemyCheckBox.ButtonPressed = false;
	}*/
}
