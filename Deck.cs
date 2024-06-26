using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Deck : Node2D
{   
    public List<CardControl> CurrentHand = new List<CardControl>();
	public CardScene _cards = new CardScene();
	private Vector2I CardSize = new Vector2I(140, 100);
	// starting angle of the card
	private double Angle = 0;
	private double CardSpread = 0.10;
	private int CardNumber = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		AddChild(_cards);
	}

	public void OnDeckButtonPressed()
	{
		CardControl card = _cards.DrawCard();
		Angle = Math.PI/2 + CardSpread*(CurrentHand.Count()/2 - CurrentHand.Count());
		// center oval point on the screen
		var CentreCardOval = GetViewportRect().Size * new Vector2((float)0.5, (float) 1.5);
		// horizontal radius of the oval scaled by *#
		var Hor_rad = GetViewportRect().Size.X * 0.70;
		// vertical radius of oval scaled by by *#
		var Ver_rad = GetViewportRect().Size.Y * 0.85;
		var OvalAngleVector = new Vector2();

		OvalAngleVector = new Vector2((float)(Hor_rad * Mathf.Cos(Angle)), (float) (-Ver_rad * Mathf.Sin(Angle)));
		var button = GetChild(0);
		var deckButton = (TextureButton) button;
		card.Position = deckButton.GetRect().Position;
		card.startPos = deckButton.GetRect().Position;
		card.targetPos = CentreCardOval + OvalAngleVector - card.GetRect().Size;
		card.startRotation = 0;
		card.targetRotation = 90 - (float) Mathf.RadToDeg(Angle);
		card.cardState = CardControl.CardStates.MoveDrawnCardToHand;
		CardNumber = 0;
		foreach(var cardInHand in this.GetChildren()) {
			if(cardInHand is CardControl) {
				var actualCardInHand = (CardControl) cardInHand;
				Angle = Math.PI/2 + CardSpread*(CurrentHand.Count()/2 - CardNumber);
				OvalAngleVector = new Vector2((float)(Hor_rad * Mathf.Cos(Angle)), (float) (-Ver_rad * Mathf.Sin(Angle)));
				actualCardInHand.targetPos = CentreCardOval + OvalAngleVector - card.GetRect().Size;
				actualCardInHand.startRotation = actualCardInHand.RotationDegrees;
				actualCardInHand.targetRotation = 90 - (float) Mathf.RadToDeg(Angle);
			if (actualCardInHand.cardState == CardControl.CardStates.InHand) {
				actualCardInHand.startPos = actualCardInHand.Position;
				actualCardInHand.cardState = CardControl.CardStates.ReorganizeHand;
			}
				CardNumber += 1;
			}
		}
		AddChild(card);
		CurrentHand.Add(card);
		// angle in which its offset by
		Angle += 0.1;	
	}
}
