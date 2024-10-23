using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

public partial class Deck : Node2D
{
	/** Deck organization variables **/
	public List<string> _replaces { get; set; } = new List<string>();
	// Record of the initialDeck, reused during each round
	public List<CardObj> InitialDeckOfCards { get; set; } = new List<CardObj>();
	// Deck that is being currently drawn from
	public Stack<CardObj> DeckOfCards { get; set; } = new Stack<CardObj>();
	private int InitialDeckLength { get; set; }
	public List<CardControl> discardPile { get; set; } = new List<CardControl>();
	public AtlasTexture basicCardAtlas;

	/** Deck Draw Visual variables*/
	public List<CardControl> CurrentHand = new List<CardControl>();
	public int cardDrawnNumber = 0;
	private Vector2I CardSize = new Vector2I(140, 100);
	// starting angle of the card
	private double Angle = 0;
	private double CardSpread = 0.10;
	private int CardNumber = 0;


	// Called when the node enters the scene tree for the first time.
	// Instantiates the InitialDeckofCards & basicCardAtlas;
	public override void _Ready()
	{
		basicCardAtlas = new AtlasTexture();
		try
		{
			var cardImage = Image.LoadFromFile("assets/basics.jpg");
			var atlasTexture = ImageTexture.CreateFromImage(cardImage);
			basicCardAtlas.Atlas = atlasTexture;
			var region = new Rect2(new Vector2(0, 0), new Vector2(atlasTexture.GetWidth(), atlasTexture.GetHeight()));
			basicCardAtlas.Region = region;
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
			CardObj cardReplace = InitialDeckOfCards.Where((card) => card.cardId == replace).First();
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

	public CardControl InstantiateSpell(CardObj card)
	{
		var SpellCardAtlas = (AtlasTexture)Utils.SpriteSheets["spell"].Duplicate();
		SpellCardAtlas.Region = new Rect2(
			new Vector2(card.xCoord * GameSettings.CardWidth, card.yCoord * GameSettings.CardLength),
			new Vector2(GameSettings.CardWidth, GameSettings.CardLength));
		card.Texture = SpellCardAtlas;
		InitialDeckOfCards.Add(card);
		CardControl spellCard = AttachCardControl(card);
		return spellCard;
	}

	public CardControl InstantiateBasicCard(CardObj card)
	{
		card.ImageCropping(basicCardAtlas);
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
		else if (card is Spell)
		{
			topCard = DeckOfCards.Pop();
			drawnCard = InstantiateSpell(topCard);
		}
		else
		{
			topCard = DeckOfCards.Pop();
			drawnCard = InstantiateBasicCard(topCard);
		}

		return drawnCard;
	}

	/*****************************************************
	* Deck Draw Visuals
	******************************************************/

	public void DrawCardsVisual()
	{
		CardControl card = DrawCard();
		Angle = Math.PI / 2 + CardSpread * (cardDrawnNumber / 2 - cardDrawnNumber);
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
				Angle = Math.PI / 2 + CardSpread * (cardDrawnNumber / 2 - CardNumber);
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
		cardDrawnNumber += 1;
		// angle in which its offset by
		Angle += 0.1;
	}

	public void OnDeckButtonPressed(int cardLimit)
	{
		for (int cardDraw = 0; cardDraw < cardLimit; cardDraw++)
		{
			DrawCardsVisual();
		}
	}

	public void onRemoveFromCurrentHand(CardControl cardControl)
	{
		CurrentHand.Remove(cardControl);
	}

	public void onAddToCurrentHand(CardControl cardControl)
	{
		CurrentHand.Add(cardControl);
	}

	public void onAddCardToDeck(CardObj card)
	{
		if (card != null)
		{
			InitialDeckOfCards.Add(card);
			DeckOfCards.Push(card);
		}
	}
}
