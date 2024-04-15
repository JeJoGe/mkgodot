using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;

public partial class Tactics : Control
{
	[Signal]
	public delegate void TurnOrderEventHandler(int playerTurn, int dummyTurn);

	private List<string> _availableDay = ["Tactic1", "Tactic2", "Tactic3", "Tactic4", "Tactic5", "Tactic6"];
	//private List<int> _availableNight = [1,2,3,4,5,6];
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

	private void OnConfirmTacticPressed() {
		if (GamePlay.NumPlayers == 1)
		{
			SinglePlayerConquest();
		} else {
			// handle different scenarios
		}
	}

	private void SinglePlayerConquest() {
		var group = GetNode<Button>("Tactic1").ButtonGroup;
		var selectedButton = group.GetPressedButton();
		GD.Print(selectedButton.Name + " selected");
		_availableDay.Remove(selectedButton.Name);
		var tacticButtons = group.GetButtons();
		foreach (var button in tacticButtons) {
			button.Disabled = true;
		}
		int max = _availableDay.Count;
		int dummyNum = Source.RandomNumber(0,max);
		string dummyTactic = _availableDay[dummyNum];
		GD.Print(dummyTactic);
		_availableDay.RemoveAt(dummyNum);
	} 
}
