using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;

public partial class Tactics : Control
{
	[Signal]
	public delegate void TurnOrderEventHandler(int playerTurn, int dummyTurn);

	private List<string> _availableDay = ["Tactic1", "Tactic2", "Tactic3", "Tactic4", "Tactic5", "Tactic6"];
	private bool _night = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void NewRound() {
		_night = !_night; //switch from day to night
		//enable available tactics
		foreach (var path in _availableDay)
		{
			GetNode<Button>(path).Disabled = false;
		}
		Show();
	}

	private void OnTacticButtonPressed()
	{
		GetNode<Button>("ConfirmTactic").Disabled = false;
	}

	private void OnConfirmTacticPressed() {
		if (GameSettings.NumPlayers == 1)
		{
			SinglePlayerConquest();
		} else {
			// handle different scenarios and player numbers
		}
	}

	private void SinglePlayerConquest() {
		var group = GetNode<Button>("Tactic1").ButtonGroup;
		var selectedButton = (string)group.GetPressedButton().Name;
		var playerTurn = selectedButton.Substring(6);
		_availableDay.Remove(selectedButton);
		var tacticButtons = group.GetButtons();
		// reset buttons
		foreach (var button in tacticButtons) {
			button.Disabled = true;
		}
		GetNode<Button>("ConfirmTactic").Disabled = true;
		// get dummy tactic
		int dummyNum = Source.RandomNumber(0,_availableDay.Count);
		string dummyTurn = _availableDay[dummyNum].Substring(6);
		GD.Print("Player: "+playerTurn + " Dummy: "+dummyTurn);
		_availableDay.RemoveAt(dummyNum);
		Hide();
		EmitSignal(SignalName.TurnOrder,playerTurn,dummyTurn);
	} 
}
