using Godot;
using System;
using System.Collections.Generic;

public partial class DummyArea : Node2D
{
	[Signal]
	public delegate void EndTurnEventHandler();
	private int[] _deck = [0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3];
	private Stack<int> stack;
	private static readonly Random random = new Random();
	private int _blue = 4;
	private int _red = 4;
	private int _green = 4;
	private int _white = 4;
	private Dictionary<int, int> _crystals;
	private static readonly Dictionary<int, Dictionary<int, int>> _initCrystals = new Dictionary<int, Dictionary<int, int>>{
		{0,new Dictionary<int,int>{
			{0,2},{1,1},{2,0},{3,0}
		}},
		{1,new Dictionary<int, int>{
			{0,0},{1,2},{2,0},{3,1}
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
			GD.Print("Dummy takes its turn");
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
	}

	private void ShuffleDeck()
	{
		MapGen.ShuffleArray(random, _deck);
	}

	private void OnNewRound()
	{
		// add to card to deck then shuffle
		ShuffleDeck();
		stack = new Stack<int>(_deck);
	}
}
