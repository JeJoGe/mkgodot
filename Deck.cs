using Godot;
using System.Collections.Generic;

public partial class Deck : Node2D
{   
    public List<CardObj> currentHand = new List<CardObj>();
	public CardScene _cards = new CardScene();

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

	public void OnDeckButtonPressed()
	{
		CardObj card = _cards.DrawCard();
		AddChild(card);
		currentHand.Add(card);
	}
}
