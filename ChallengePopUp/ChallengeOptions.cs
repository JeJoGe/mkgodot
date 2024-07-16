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

	public CheckBox createChallengeOption(int enemyId, int optionOffset, Color PosColour)
	{
		var enemyCheckBox = new CheckBox
		{
			Text = Utils.Bestiary[enemyId].Name,
			Position = new Vector2(1000, optionOffset),// realized vbox means x position doesn't matter
			Name = string.Format("Monster{0}", enemyId)
		};
		enemyCheckBox.AddThemeColorOverride("font_color", PosColour);
		enemyCheckBox.SetPressedNoSignal(true);
		if (PosColour == Colors.Black)
		{
			enemyCheckBox.Disabled = true;
		}
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
