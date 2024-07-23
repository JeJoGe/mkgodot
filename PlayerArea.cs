using Godot;
using System;

public partial class PlayerArea : Node2D
{	
	private PackedScene _manaPopup = GD.Load<PackedScene>("res://ManaPopUp/ManaPopup.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CreateManaPopup(Source.Colour.Blue);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnCurrentTurn(long characterId) {
		//GD.Print("start turn:" +characterId);
		if (characterId == GameSettings.PlayerCharacter) {
			//GD.Print("Disabled: " + GetNode<Button>("EndTurnButton").Disabled.ToString());
			GetNode<Button>("EndTurnButton").Disabled = false;
			//GD.Print("Disabled: " + GetNode<Button>("EndTurnButton").Disabled.ToString());
			GD.Print("It's your turn!");
		}
	}

	public void CreateManaPopup(Source.Colour colour)
	{
		var popup = (Control)_manaPopup.Instantiate();
		AddChild(popup);
	}
}
