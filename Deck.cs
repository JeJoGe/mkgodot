using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

public enum DrawCardType
{
	actionCard,
	wound
}

public partial class Deck : Node2D
{
	/** Deck organization variables **/
	public List<string> _replaces { get; set; } = new List<string>();
	// Record of the initialDeck, reused during each round
	public List<CardObj> InitialDeckOfCards { get; set; } = new List<CardObj>();
	// Deck that is being currently drawn from
	public Stack<CardObj> DeckOfCards { get; set; } = new Stack<CardObj>();
	private int InitialDeckLength { get; set; }
	public List<CardControl> DiscardPile { get; set; } = new List<CardControl>();
	public AtlasTexture BasicCardAtlas;
	public int WoundsInHand { get; set; } = 0;
	public int WoundsInDiscard { get; set; } = 0;
	public int CardLimit { get; set; } = 5;

	/** Deck Draw Visual variables*/
	public Godot.Collections.Array<CardControl> CurrentHand = new Godot.Collections.Array<CardControl>();
	public int CardDrawnNumber = 0;
	private Vector2I CardSize = new Vector2I(140, 100);
	// starting angle of the card
	private double Angle = 0;
	private double CardSpread = 0.10;
	private int CardNumber = 0;


	// Called when the node enters the scene tree for the first time.
	// Instantiates the InitialDeckofCards & basicCardAtlas;
	public override void _Ready()
	{
		BasicCardAtlas = new AtlasTexture();
		try
		{
			var cardImage = Image.LoadFromFile("assets/basics.jpg");
			var atlasTexture = ImageTexture.CreateFromImage(cardImage);
			BasicCardAtlas.Atlas = atlasTexture;
			var region = new Rect2(new Vector2(0, 0), new Vector2(atlasTexture.GetWidth(), atlasTexture.GetHeight()));
			BasicCardAtlas.Region = region;
		}
		catch
		{
			GD.Print("Failed to load Basic Card Image");
		}
		StreamReader sr = new StreamReader("./Card/basicCard.json");
		string jsonObj = sr.ReadToEnd();
		var cardsObj = JsonConvert.DeserializeObject<List<CardObj>>(jsonObj);
		int index = 0;

		foreach (var card in cardsObj)
		{
			if (card.character == GameSettings.PlayerCharacterName || card.character == "basic")
			{
				card.id = index;
				InitialDeckOfCards.Add(card);
				index++;
				if (card.copies > 0)
				{
					for (int i = 0; i < card.copies; i++)
					{
						CardObj newCard = new CardObj()
						{
							id = index,
							cardId = card.cardId + "-1",
							color = card.color,
							xCoord = card.xCoord,
							yCoord = card.yCoord,
							copies = card.copies,
							topFunction = card.topFunction,
							bottomFunction = card.bottomFunction,
							phase = card.phase,
							character = card.character,
							replaces = card.replaces
						};
						InitialDeckOfCards.Add(newCard);
						index++;
					}
				}
				if (card.replaces != "")
				{
					_replaces.Add(card.replaces);
				}
			}
		}

		// Filter out cards with each name specified in _replaces
		foreach (string replace in _replaces)
		{
			CardObj cardReplace = InitialDeckOfCards.Where((card) => card.cardId == replace).FirstOrDefault();
			InitialDeckOfCards.Remove(cardReplace);
		}
		DeckOfCards = new Stack<CardObj>(InitialDeckOfCards.Shuffle());
		InitialDeckLength = InitialDeckOfCards.Count;

	}

	/*****************************************************
	* Deck Organization
	******************************************************/

	public CardControl AttachCardControl(CardObj card)
	{
		CardControl cardControl = new CardControl(card.cardId, card.Texture.GetSize());
		cardControl.AddChild(card);
		return cardControl;
	}

	public CardControl InstantiateCard(CardObj card)
	{
		card.ImageCropping();
		CardControl basicCard = AttachCardControl(card);
		return basicCard;

	}

	// Instantiation of the atlas & CardControl happens when card is drawn;
	public CardControl DrawCard()
	{
		CardObj topCard;
		CardControl drawnCard;
		var card = DeckOfCards.Peek();

		if (card == null)
		{
			GD.Print("no more cards in deck");
			return null;
		}
		else
		{
			topCard = DeckOfCards.Pop();
			drawnCard = InstantiateCard(topCard);
		}

		return drawnCard;
	}

	/*****************************************************
	* Deck Draw Visuals
	******************************************************/

	public void DrawCardsVisual(DrawCardType type)
	{
		CardControl card;
		if (type == DrawCardType.actionCard)
		{
			card = DrawCard();
		}
		else
		{
			Wound wound = new Wound();
			card = InstantiateCard(wound);
		}
		Angle = Math.PI / 2 + CardSpread * (CardDrawnNumber / 2 - CardDrawnNumber);
		// center oval point on the screen
		var CentreCardOval = GetViewportRect().Size * new Vector2((float)0.5, (float)1.5);
		// horizontal radius of the oval scaled by *#
		var Hor_rad = GetViewportRect().Size.X * 0.70;
		// vertical radius of oval scaled by by *#
		var Ver_rad = GetViewportRect().Size.Y * 0.85;
		var OvalAngleVector = new Vector2();

		OvalAngleVector = new Vector2((float)(Hor_rad * Mathf.Cos(Angle)), (float)(-Ver_rad * Mathf.Sin(Angle)));
		var button = GetChild(0);
		var deckButton = (TextureButton)button;
		card.Position = deckButton.GetRect().Position;
		card.startPos = deckButton.GetRect().Position;
		card.targetPos = CentreCardOval + OvalAngleVector - card.GetRect().Size;
		card.startRotation = 0;
		card.targetRotation = 90 - (float)Mathf.RadToDeg(Angle);
		card.cardState = CardControl.CardStates.MoveDrawnCardToHand;
		CardNumber = 0;
		foreach (var cardInHand in this.GetChildren())
		{
			if (cardInHand is CardControl)
			{
				var actualCardInHand = (CardControl)cardInHand;
				Angle = Math.PI / 2 + CardSpread * (CardDrawnNumber / 2 - CardNumber);
				OvalAngleVector = new Vector2((float)(Hor_rad * Mathf.Cos(Angle)), (float)(-Ver_rad * Mathf.Sin(Angle)));
				actualCardInHand.targetPos = CentreCardOval + OvalAngleVector - card.GetRect().Size;
				actualCardInHand.startRotation = actualCardInHand.RotationDegrees;
				actualCardInHand.targetRotation = 90 - (float)Mathf.RadToDeg(Angle);
				if (actualCardInHand.cardState == CardControl.CardStates.InHand)
				{
					actualCardInHand.startPos = actualCardInHand.Position;
					actualCardInHand.cardState = CardControl.CardStates.ReorganizeHand;
				}
				CardNumber += 1;
			}
		}
		AddChild(card);
		CurrentHand.Add(card);
		CardDrawnNumber += 1;
		// angle in which its offset by
		Angle += 0.1;
	}

	/*****************************************************
	* Deck Interactions
	******************************************************/

	public void OnDeckButtonPressed(int numOfCards)
	{
		int cardsDrawn = 0;
		while (CurrentHand.Count() < this.CardLimit && cardsDrawn < numOfCards)
		{
			DrawCardsVisual(DrawCardType.actionCard);
			cardsDrawn++;
		}
	}

	public void onRemoveFromCurrentHand(CardControl cardControl)
	{
		CurrentHand.Remove(cardControl);
		DiscardPile.Add(cardControl);
	}

	public void onAddToCurrentHand(CardControl cardControl)
	{
		CurrentHand.Add(cardControl);
		DiscardPile.Remove(cardControl);
	}

	public void onAddCardToDeck(CardObj card)
	{
		if (card != null)
		{
			InitialDeckOfCards.Add(card);
			DeckOfCards.Push(card);
			InitialDeckLength++;
		}
	}

	/*****************************************************
	* Wounds
	******************************************************/
	public void AddWoundToDiscard(int numberOfWounds)
	{
		for (int i = 0; i < numberOfWounds; i++)
		{
			Wound newWound = new Wound();
			CardControl woundControl = InstantiateCard(newWound);
			AddChild(woundControl);
			woundControl.Position = woundControl.discardArea;
			woundControl.Scale = new Vector2((float)0.125, (float)0.125);
			woundControl.cardId = "Wound";
			DiscardPile.Add(woundControl);
			InitialDeckOfCards.Add(newWound);
			InitialDeckLength++;
			WoundsInDiscard++;
		}
	}

	public void AddWoundToHand(int numberOfWounds)
	{
		for (int i = 0; i < numberOfWounds; i++)
		{
			DrawCardsVisual(DrawCardType.wound);
			var newWound = new Wound();
			InitialDeckOfCards.Add(newWound);
			InitialDeckLength++;
			WoundsInHand++;
		}
	}

	public void RemoveWoundFromHand(int numberOfWounds)
	{
		int currWoundRemoved = numberOfWounds;
		while (currWoundRemoved > 0 && WoundsInHand > 0)
		{
			CardControl wound = CurrentHand.Where((cardcontrol) => cardcontrol.GetChild<CardObj>(0).id == 0).FirstOrDefault();
			CurrentHand.Remove(wound);
			CardObj woundCardObj = (CardObj)wound.GetChild(0);
			RemoveChild(wound);
			RemoveCardFromInitialDeck(woundCardObj);
			WoundsInHand--;
			currWoundRemoved--;
		}
	}

	public void RemoveWoundFromDiscard(int numberOfWounds)
	{
		int currWoundRemoved = numberOfWounds;
		while (currWoundRemoved > 0 && WoundsInDiscard > 0)
		{
			CardControl wound = DiscardPile.Where((card) => card.cardId == "Wound").FirstOrDefault();
			GD.Print(wound);
			CardObj woundCardObj = (CardObj)wound.GetChild(0);
			RemoveChild(wound);
			RemoveCardFromInitialDeck(woundCardObj);
			DiscardPile.Remove(wound);
			WoundsInDiscard--;
			currWoundRemoved--;
		}
	}

	/*****************************************************
	* Helpers
	******************************************************/
	private void RemoveCardFromInitialDeck(CardObj card)
	{
		CardObj cardInInitial = InitialDeckOfCards.Where(c => c.id == card.id).First();
		InitialDeckOfCards.Remove(cardInInitial);

	}
}
