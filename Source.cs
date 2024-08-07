using Godot;
using Godot.Collections;
using System;

public partial class Source : Node2D
{
	[Signal]
	public delegate void DieEventHandler(Colour colour);
	private Dictionary<Colour, int> _dice = new Dictionary<Colour, int>{
		{Colour.Blue, 0},
		{Colour.Red, 0},
		{Colour.Green, 0},
		{Colour.White, 0},
		{Colour.Gold, 0},
		{Colour.Black, 0}
	};
	public int Blue
	{
		get => _dice[Colour.Blue];
	}
	public int Red
	{
		get => _dice[Colour.Red];
	}
	public int Green
	{
		get => _dice[Colour.Green];
	}
	public int White
	{
		get => _dice[Colour.White];
	}
	public int Gold
	{
		get => _dice[Colour.Gold];
	}
	public int Black
	{
		get => _dice[Colour.Black];
	}
	private int _dicePerTurn = 1; // default number of dice allowed to be taken
	public int DicePerTurn
	{
		get => _dicePerTurn;
		set
		{
			if (value > _diceTaken)
			{
				//enable all buttons that have available mana
				UpdateButtons();
			}
			_dicePerTurn = value;
		}
	}
	private int _diceTaken = 0;
	public int DiceTaken
	{
		get => _diceTaken;
		set
		{
			_diceTaken = value;
		}
	}

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

	public int GetDiceCount(Colour colour)
	{
		return _dice[colour];
	}

	public void NewRound()
	{
		// reset all the current dice pool
		foreach (var kvp in _dice)
		{
			_dice[kvp.Key] = 0;
		}
		var numDice = GameSettings.NumPlayers + 2; //number of dice be default
		for (int i = 0; i < numDice; i++)
		{
			SetDie(RollDie());
		}
		// check if half are basics and reroll all black and yellow until true
		var numNonBasic = Gold + Black;
		while (numNonBasic > numDice / 2)
		{
			_dice[Colour.Gold] = 0;
			_dice[Colour.Black] = 0;
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

	private void UpdateLabels()
	{
		UpdateBlueLabel();
		UpdateRedLabel();
		UpdateGreenLabel();
		UpdateWhiteLabel();
		UpdateGoldLabel();
		UpdateBlackLabel();
	}

	private void UpdateBlueLabel()
	{
		GetNode<Label>("BlueLabel").Text = Blue.ToString();
	}

	private void UpdateRedLabel()
	{
		GetNode<Label>("RedLabel").Text = Red.ToString();
	}

	private void UpdateGreenLabel()
	{
		GetNode<Label>("GreenLabel").Text = Green.ToString();
	}

	private void UpdateWhiteLabel()
	{
		GetNode<Label>("WhiteLabel").Text = White.ToString();
	}

	private void UpdateGoldLabel()
	{
		GetNode<Label>("GoldLabel").Text = Gold.ToString();
	}

	private void UpdateBlackLabel()
	{
		GetNode<Label>("BlackLabel").Text = Black.ToString();
	}

	public void SetDie(Colour colour)
	{
		_dice[colour]++;
		switch (colour)
		{
			case Colour.Blue:
				{
					UpdateBlueLabel();
					UpdateBlueButton();
					break;
				}
			case Colour.Red:
				{
					UpdateRedLabel();
					UpdateRedButton();
					break;
				}
			case Colour.Green:
				{
					UpdateGreenLabel();
					UpdateGreenButton();
					break;
				}
			case Colour.White:
				{
					UpdateWhiteLabel();
					UpdateWhiteButton();
					break;
				}
			case Colour.Gold:
				{
					UpdateGoldLabel();
					UpdateGoldButton();
					break;
				}
			case Colour.Black:
				{
					UpdateBlackLabel();
					UpdateBlackButton();
					break;
				}
			default: break;
		}
	}

	public void TakeDie(Colour colour)
	{
		if (_dice[colour] > 0)
		{
			_dice[colour]--;
			switch (colour)
			{
				case Colour.Blue:
					{
						UpdateBlueLabel();
						UpdateBlueButton();
						break;
					}
				case Colour.Red:
					{
						UpdateRedLabel();
						UpdateRedButton();
						break;
					}
				case Colour.Green:
					{
						UpdateGreenLabel();
						UpdateGreenButton();
						break;
					}
				case Colour.White:
					{
						UpdateWhiteLabel();
						UpdateWhiteButton();
						break;
					}
				case Colour.Gold:
					{
						UpdateGoldLabel();
						UpdateGoldButton();
						break;
					}
				case Colour.Black:
					{
						UpdateBlackLabel();
						UpdateBlackButton();
						break;
					}
				default: break;
			}
		}
		else
		{
			throw new InvalidOperationException("No " + colour.ToString().ToLower() + " dice available");
		}
		_diceTaken++;
		if (_diceTaken == DicePerTurn)
		{
			// no more dice allowed to be taken
			GetNode<Button>("BlueButton").Disabled = true;
			GetNode<Button>("RedButton").Disabled = true;
			GetNode<Button>("GreenButton").Disabled = true;
			GetNode<Button>("WhiteButton").Disabled = true;
			GetNode<Button>("GoldButton").Disabled = true;
			GetNode<Button>("BlackButton").Disabled = true;
		}
		EmitSignal(SignalName.Die, (int)colour);
	}

	private void UpdateButtons()
	{
		UpdateBlueButton();
		UpdateRedButton();
		UpdateGreenButton();
		UpdateWhiteButton();
		UpdateGoldButton();
		UpdateBlackButton();
	}

	private void UpdateBlueButton()
	{
		GetNode<Button>("BlueButton").Disabled = Blue == 0;
	}
	private void UpdateRedButton()
	{
		GetNode<Button>("RedButton").Disabled = Red == 0;
	}
	private void UpdateGreenButton()
	{
		GetNode<Button>("GreenButton").Disabled = Green == 0;
	}

	private void UpdateWhiteButton()
	{
		GetNode<Button>("WhiteButton").Disabled = White == 0;
	}

	private void UpdateGoldButton()
	{
		GetNode<Button>("GoldButton").Disabled = Gold == 0;
	}

	private void UpdateBlackButton()
	{
		GetNode<Button>("BlackButton").Disabled = Black == 0;
	}
	public static Colour RollDie()
	{
		var faces = Enum.GetValues<Colour>();
		return (Colour)faces.GetValue(Utils.RandomNumber(0, 6));
	}

	private void OnBlueDiePressed()
	{
		TakeDie(Colour.Blue);
	}

	private void OnRedDiePressed()
	{
		TakeDie(Colour.Red);
	}

	private void OnGreenDiePressed()
	{
		TakeDie(Colour.Green);
	}

	private void OnWhiteDiePressed()
	{
		TakeDie(Colour.White);
	}

	private void OnGoldDiePressed()
	{
		TakeDie(Colour.Gold);
	}

	private void OnBlackDiePressed()
	{
		TakeDie(Colour.Black);
	}
}
