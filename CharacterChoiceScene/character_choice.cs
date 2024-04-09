using Godot;
using System;

public partial class character_choice : Control
{
	private string[] characters = ["Tovak", "Areythea"];
	private Submit submitButton { get; set; }

	public override void _Ready() {
		var characterDropdown = GetNode<OptionButton>("CharacterDropdown");
		for(int i = 0; i < characters.Length; ++i ) {
			characterDropdown.AddItem(characters[i], i);
		}
		characterDropdown.Selected = -1;
		characterDropdown.ItemSelected += OnCharacterSelected;
		var submitButton = GetNode<Button>("Submit");
		submitButton.Disabled = true;
		submitButton.Pressed += OnSubmitPressed;
	}

	private void OnCharacterSelected(long index)
	{
		GD.Print("Selected ", characters[index]);
		GetNode<Button>("Submit").Disabled = false;
	}
	
	private void OnSubmitPressed()
	{
		GD.Print("PLAY");
	}
	
}
