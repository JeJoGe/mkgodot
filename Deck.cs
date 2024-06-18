using Godot;
using System;
using System.Collections.Generic;

public partial class Deck : Node2D
{   
    public List<CardControl> currentHand = new List<CardControl>();
	public CardScene _cards = new CardScene();
	private Vector2I CardSize = new Vector2I(140, 100);
	private double angle = Mathf.DegToRad(90) - 0.5;

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
		CardControl card = _cards.DrawCard();
		var CentreCardOval = GetViewportRect().Size * new Vector2((float)0.5, (float)1.25);
		var Hor_rad = GetViewportRect().Size.X * 0.45;
		var Ver_rad = GetViewportRect().Size.Y * 0.4;
		var OvalAngleVector = new Vector2();

		OvalAngleVector = new Vector2((float)(Hor_rad * Mathf.Cos(angle)), (float) (- Ver_rad * Mathf.Sin(angle)));
		card.Position = CentreCardOval + OvalAngleVector - card.GetRect().Size/2;
		card.Scale *= CardSize / card.GetRect().Size;
		card.Rotation = (90 - (float) Mathf.RadToDeg(angle))/4;
		AddChild(card);
		angle += 0.25;

		currentHand.Add(card);
	}
}
