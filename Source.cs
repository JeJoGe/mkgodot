using Godot;
using Godot.Collections;
using System;

public partial class Source : Node2D
{
	[Signal]
	public delegate void DieEventHandler(Colour colour);

	private int _blue = 0;
	public int Blue => _blue;
	private int _red = 0;
	public int Red => _red;
	private int _green = 0;
	public int Green => _green;
	private int _white = 0;
	public int White => _white;
	private int _gold = 0;
	public int Gold => _gold;
	private int _black = 0;
	public int Black => _black;
	private int _dicePerTurn = 1; // default number of dice allowed to be taken
	public int DicePerTurn 
	{ 
		get => _dicePerTurn;
		set
		{
			if (value > _diceTaken) {
				//enable all buttons that have available mana
				UpdateButtons();
			}
			_dicePerTurn = value;
		} 
	}
	private int _diceTaken = 0;

	public enum Colour
	{
		Blue, Red, Green, White, Gold, Black
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void NewRound()
	{
		// reset all the current dice pool
		_blue = 0;
		_red = 0;
		_green = 0;
		_white = 0;
		_gold = 0;
		_black = 0;
		var numDice = GameSettings.NumPlayers + 2; //number of dice be default
		for (int i = 0; i < numDice; i++)
		{
			SetDie(RollDie());
		}
		// check if half are basics and reroll all black and yellow until true
		var numNonBasic = Gold + Black;
		while (numNonBasic > numDice/2 )
		{
			_gold = 0;
			_black = 0;
			for (int j = 0; j < numNonBasic; j++)
			{
				SetDie(RollDie());
			}
			numNonBasic = Gold + Black;
		}
		UpdateLabels();
		UpdateButtons();
	}

	public void EndTurn()
	{
		if (_diceTaken > 0)
		{
			for (int i = 0; i < _diceTaken; i++)
			{
				SetDie(RollDie());
			}
		}
		_dicePerTurn = 1;
		_diceTaken = 0;
		UpdateButtons();
	}

	private void UpdateLabels() {
		UpdateBlueLabel();
		UpdateRedLabel();
		UpdateGreenLabel();
		UpdateWhiteLabel();
		UpdateGoldLabel();
		UpdateBlackLabel();
	}

	private void UpdateBlueLabel() {
		GetNode<Label>("BlueLabel").Text = Blue.ToString();
	}

	private void UpdateRedLabel() {
		GetNode<Label>("RedLabel").Text = Red.ToString();
	}

	private void UpdateGreenLabel() {
		GetNode<Label>("GreenLabel").Text = Green.ToString();
	}

	private void UpdateWhiteLabel() {
		GetNode<Label>("WhiteLabel").Text = White.ToString();
	}

	private void UpdateGoldLabel() {
		GetNode<Label>("GoldLabel").Text = Gold.ToString();
	}

	private void UpdateBlackLabel() {
		GetNode<Label>("BlackLabel").Text = Black.ToString();
	}

	public void SetDie(Colour colour) {
		switch (colour)
		{
			case Colour.Blue:
			{
				_blue++;
				UpdateBlueLabel();
				UpdateBlueButton();
				break;
			}
			case Colour.Red:
			{
				_red++;
				UpdateRedLabel();
				UpdateRedButton();
				break;
			}
			case Colour.Green:
			{
				_green++;
				UpdateGreenLabel();
				UpdateGreenButton();
				break;
			}
			case Colour.White:
			{
				_white++;
				UpdateWhiteLabel();
				UpdateWhiteButton();
				break;
			}
			case Colour.Gold:
			{
				_gold++;
				UpdateGoldLabel();
				UpdateGoldButton();
				break;
			}
			case Colour.Black:
			{
				_black++;
				UpdateBlackLabel();
				UpdateBlackButton();
				break;
			}
			default: break;
		}
	}

	public void TakeDie(Colour colour)
	{
		switch (colour)
		{
			case Colour.Blue:
			{
				if (Blue > 0)
				{
					_blue--;
					UpdateBlueLabel();
					UpdateBlueButton();
				} else {
					throw new InvalidOperationException("No blue dice available");
				}
				break;
			}
			case Colour.Red:
			{
				if (Red > 0)
				{
					_red--;
					UpdateRedLabel();
					UpdateRedButton();
				} else {
					throw new InvalidOperationException("No red dice available");
				}
				break;
			}
			case Colour.Green:
			{
				if (Green > 0)
				{
					_green--;
					UpdateGreenLabel();
					UpdateGreenButton();
				} else {
					throw new InvalidOperationException("No green dice available");
				}
				break;
			}
			case Colour.White:
			{
				if (White > 0)
				{
					_white--;
					UpdateWhiteLabel();
					UpdateWhiteButton();
				} else {
					throw new InvalidOperationException("No white dice available");
				}
				break;
			}
			case Colour.Gold:
			{
				if (Gold > 0)
				{
					_gold--;
					UpdateGoldLabel();
					UpdateGoldButton();
				} else {
					throw new InvalidOperationException("No gold dice available");
				}
				break;
			}
			case Colour.Black:
			{
				if (Black > 0)
				{
					_black--;
					UpdateBlackLabel();
					UpdateBlackButton();
				} else {
					throw new InvalidOperationException("No black dice available");
				}
				break;
			}
			default: break;
		}
		_diceTaken++;
		if (_diceTaken == DicePerTurn) {
			// no more dice allowed to be taken
			GetNode<Button>("BlueButton").Disabled = true;
			GetNode<Button>("RedButton").Disabled = true;
			GetNode<Button>("GreenButton").Disabled = true;
			GetNode<Button>("WhiteButton").Disabled = true;
			GetNode<Button>("GoldButton").Disabled = true;
			GetNode<Button>("BlackButton").Disabled = true;
		}
		EmitSignal(SignalName.Die, (int) colour);
	}

	private void UpdateButtons() {
		UpdateBlueButton();
		UpdateRedButton();
		UpdateGreenButton();
		UpdateWhiteButton();
		UpdateGoldButton();
		UpdateBlackButton();
	}

	private void UpdateBlueButton() {
		GetNode<Button>("BlueButton").Disabled = Blue == 0;
	}
	private void UpdateRedButton() {
		GetNode<Button>("RedButton").Disabled = Red == 0;
	}
	private void UpdateGreenButton() {
		GetNode<Button>("GreenButton").Disabled = Green == 0;
	}

	private void UpdateWhiteButton() {
		GetNode<Button>("WhiteButton").Disabled = White == 0;
	}

	private void UpdateGoldButton() {
		GetNode<Button>("GoldButton").Disabled = Gold == 0;
	}

	private void UpdateBlackButton() {
		GetNode<Button>("BlackButton").Disabled = Black == 0;
	}
	public static Colour RollDie()
	{
		var faces = Enum.GetValues<Colour>();
		return (Colour)faces.GetValue(Utils.RandomNumber(0,6));
	}

	private void OnBlueDiePressed() {
		TakeDie(Colour.Blue);
	}

	private void OnRedDiePressed() {
		TakeDie(Colour.Red);
	}

	private void OnGreenDiePressed() {
		TakeDie(Colour.Green);
	}

	private void OnWhiteDiePressed() {
		TakeDie(Colour.White);
	}

	private void OnGoldDiePressed() {
		TakeDie(Colour.Gold);
	}

	private void OnBlackDiePressed() {
		TakeDie(Colour.Black);
	}
}
