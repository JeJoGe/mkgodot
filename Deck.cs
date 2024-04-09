using Godot;
using System;
using System.Collections.Generic;

public partial class Deck : Node2D
{
    private Stack<int> _deck = new Stack<int>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnDeckPressed()
	{
		DrawCard();
	}

	public int DrawCard() {
		int result = 0;
		return result;
	}
}
