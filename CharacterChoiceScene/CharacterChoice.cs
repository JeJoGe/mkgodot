using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CharacterChoice : Control
{
	private Dictionary<int,string> _characters = new Dictionary<int,string>{
		{ 0, "Tovak"},
		{ 1, "Arythea"}//,"Goldyx","Norowas","Wolfhawk","Krang","Braevalar"];
	};

	public override void _Ready() {
		var characterDropdown = GetNode<OptionButton>("CharacterDropdown");
		for(int i = 0; i < _characters.Count; ++i ) {
			characterDropdown.AddItem(_characters[i], i);
		}
		characterDropdown.Selected = -1;

		var submitButton = GetNode<Button>("Submit");
		submitButton.Disabled = true;
	}

	private void OnCharacterSelected(long index)
	{
		GD.Print("Selected ", _characters[(int)index]);
		GetNode<Button>("Submit").Disabled = false;
	}
	
	private void OnSubmitPressed()
	{
		var playerId = GetNode<OptionButton>("CharacterDropdown").GetSelectedId();
		GameSettings.PlayerCharacter = playerId;
		GD.Print("Player: "+_characters[GameSettings.PlayerCharacter]);
		// randomly select dummy character
		_characters.Remove(playerId);
		var characterIds = _characters.Keys.ToList(); 
		GameSettings.DummyCharacter = characterIds[Source.RandomNumber(0,_characters.Count)];
		GD.Print( "Dummy: "+_characters[GameSettings.DummyCharacter]);
		GD.Print("PLAY");
		GetTree().ChangeSceneToFile("res://GamePlay.tscn");
	}
	
}
