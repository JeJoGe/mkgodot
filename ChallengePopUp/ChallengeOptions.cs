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

	public CheckBox createChallengeOption(int enemyId, int optionOffset, Color PosColour, Color OldPosColour, string monColour, bool Facedown)
	{
		var tokenText = "";
		if (enemyId == -1 || Facedown == true)
		{
			tokenText = monColour + " Token";
		}
		else
		{
			tokenText = Utils.Bestiary[enemyId].Name;
		}
		var enemyCheckBox = new CheckBox
		{
			
			Text = tokenText,
			Position = new Vector2(1000, optionOffset),// realized vbox means x position doesn't matter
			Name = string.Format("Monster{0}", enemyId)
		};
		enemyCheckBox.FocusMode = FocusModeEnum.None;
		enemyCheckBox.AddThemeColorOverride("font_color", PosColour);
		enemyCheckBox.AddThemeColorOverride("font_pressed_color", PosColour);
		enemyCheckBox.AddThemeColorOverride("font_hover_pressed_color", PosColour);
		enemyCheckBox.SetPressedNoSignal(true);
		if (PosColour == Colors.Black || OldPosColour != Colors.Black)
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
