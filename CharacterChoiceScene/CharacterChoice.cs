using Godot;
using System;

public partial class CharacterChoice : Control
{
	private string[] characters = ["Tovak", "Arythea"];

	public override void _Ready() {
		var characterDropdown = GetNode<OptionButton>("CharacterDropdown");
		for(int i = 0; i < characters.Length; ++i ) {
			characterDropdown.AddItem(characters[i], i);
		}
		characterDropdown.Selected = -1;

		var submitButton = GetNode<Button>("Submit");
		submitButton.Disabled = true;
	}

	private void OnCharacterSelected(long index)
	{
		GD.Print("Selected ", characters[index]);
		GetNode<Button>("Submit").Disabled = false;
	}
	
	private void OnSubmitPressed()
	{
		GD.Print("PLAY");
		GetTree().ChangeSceneToFile("res://GamePlay.tscn");
	}
	
}
