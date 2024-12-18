using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DummyArea : Node2D
{
	[Signal]
	public delegate void EndTurnEventHandler();
	private int[] _deck = [0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3];
	private Stack<int> stack;
	private int _blue = 4;
	private int _red = 4;
	private int _green = 4;
	private int _white = 4;
	private Dictionary<int, int> _crystals; // colour -> number
	private static readonly Dictionary<int, Dictionary<int, int>> _initCrystals = new Dictionary<int, Dictionary<int, int>>{
		{0,new Dictionary<int,int>{ // hero -> numbers
			{0,2},{1,1},{2,0},{3,0}
		}},
		{1,new Dictionary<int, int>{
			{0,0},{1,2},{2,0},{3,1}
		}},
		{2,new Dictionary<int, int>{
			{0,1},{1,0},{2,2},{3,0}
		}},
		{3,new Dictionary<int, int>{
			{0,0},{1,0},{2,1},{3,2}
		}},
		{4,new Dictionary<int, int>{
			{0,1},{1,0},{2,0},{3,2}
		}},
		{5,new Dictionary<int, int>{
			{0,0},{1,2},{2,1},{3,0}
		}},
		{6,new Dictionary<int, int>{
			{0,2},{1,0},{2,1},{3,0}
		}}
	};
	public enum Colour
	{
		Blue, Red, Green, White
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_crystals = _initCrystals[GameSettings.DummyCharacter];
		UpdateCrystals();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnCurrentTurn(long characterId)
	{
		if (characterId == GameSettings.DummyCharacter)
		{
			// perform dummy turn
			DummyTurn();
			EmitSignal(SignalName.EndTurn);
		}
	}

	private void DummyTurn()
	{
		if (stack.Count == 0)
		{
			// call end of round
			GD.Print("Dummy calls end of round");
		}
		else
		{
			GD.Print("Dummy takes its turn");
			var cardsToDraw = 3;
			// draw 3 cards by default
			for (int i = 0; i < cardsToDraw; i++)
			{
				if (stack.Count == 0)
				{
					break;
				}
				var card = stack.Pop();
				GD.Print("Colour flipped: " + card);
				if (i == 2)
				{
					cardsToDraw += _crystals[card];
				}
				switch (card)
				{
					case 0:
						{
							_blue--;
							break;
						}
					case 1:
						{
							_red--;
							break;
						}
					case 2:
						{
							_green--;
							break;
						}
					case 3:
						{
							_white--;
							break;
						}
					default: break;
				}
			}
			GD.Print("Cards remaining: " + stack.Count);
		}
		UpdateLabels();
	}


	private void UpdateLabels() {
		GetNode<Label>("Total").Text = stack.Count.ToString();
		GetNode<Label>("DeckPopup/BlueCount").Text = _blue.ToString();
		GetNode<Label>("DeckPopup/RedCount").Text = _red.ToString();
		GetNode<Label>("DeckPopup/GreenCount").Text = _green.ToString();
		GetNode<Label>("DeckPopup/WhiteCount").Text = _white.ToString();
	}

	private void UpdateCrystals() {
		GetNode<Label>("BlueCrystal/Count").Text = _crystals[0].ToString();
		GetNode<Label>("RedCrystal/Count").Text = _crystals[1].ToString();
		GetNode<Label>("GreenCrystal/Count").Text = _crystals[2].ToString();
		GetNode<Label>("WhiteCrystal/Count").Text = _crystals[3].ToString();
	}

	private void NewRound()
	{
		// add card to deck then shuffle
		if (SharedArea.Round != 1) { // skip for round 1
			//TODO: add card according once offers are implemented
			_deck = _deck.Append(Utils.RandomNumber(0,4)).ToArray(); // adding a random card
			_blue = 0;
			_red = 0;
			_green = 0;
			_white = 0;
			foreach (var card in _deck)
			{
				switch (card) {
					case 0: {
						_blue++;
						break;
					}
					case 1: {
						_red++;
						break;
					}
					case 2: {
						_green++;
						break;
					}
					case 3: {
						_white++;
						break;
					}
					default:break;
				}	
			}
		}
		stack = new Stack<int>(_deck.Shuffle());
		UpdateLabels();
	}

	private void AddCrystal(int colour)
	{
		_crystals[colour]++;
		UpdateCrystals();
	}

	private void OnMouseEntered() {
		GetNode<Node2D>("DeckPopup").Visible = true;
	}

	private void OnMouseExited() {
		GetNode<Node2D>("DeckPopup").Visible = false;
	}
}
