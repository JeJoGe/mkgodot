using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static CardScene;

public partial class Deck : Node2D
{
    private Stack<int> _deck = new Stack<int>();
	private CardScene _cards = new CardScene();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		AddChild(_cards);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnDeckButtonPressed()
	{
		_cards.DrawCard();
	}
}
