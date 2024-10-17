using Godot;
using System;
using System.Collections.Generic;

public partial class SpellOffer : Node2D
{
	[Signal]
	public delegate void DummyEventHandler(Source.Colour colour);
	[Export]
	private ConfirmationDialog _confirmDialog;
	[Export]
	private Deck _deck;
	private LinkedList<int> _offer;
	private Dictionary<int,OfferCard> _slots; // keys 0-2 where 0 is the oldest card
	private static int _offset = 112;
	private PackedScene _cardScene = GD.Load<PackedScene>("res://Offers/OfferCard.tscn");

	private bool _reward = false;
	public bool Reward
	{
		get => _reward;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// initialize spell deck
		var includedCards = new List<int>();
		foreach (var kvp in Utils.SpellBook)
		{
			// TODO: check for cooperative/competitive spells
			if (GameSettings.Expansions.Contains(kvp.Value.version))
			{
				includedCards.Add(kvp.Key);
			}
		}
		_offer = new LinkedList<int>(includedCards.Shuffle());
		// initialize spell offer
		_slots = new Dictionary<int, OfferCard>(3);
		RefreshOffer();
	}

	private void NewRound()
	{
		if (SharedArea.Round > 1)
		{
			// move first card to bottom of deck
			var currNode = _offer.First;
			var colour = Utils.ConvertStringToSourceColour(Utils.SpellBook[currNode.Value].color);
			_offer.RemoveFirst();
			_offer.AddLast(currNode);
			RefreshOffer();
			EmitSignal(SignalName.Dummy, (int)colour); //tell dummy to add crystal of colour
		}
	}

	private void OnConfirmReward()
	{
		_confirmDialog.Visible = true;
	}

	private void OnConfirmSelection()
	{
		var offerIndex = 0;
		var currNode = _offer.First;
		while (_reward)
		{
			if (_slots[offerIndex].Selected)
			{
				_deck.onAddCardToDeck(Utils.SpellBook[currNode.Value]);
				_offer.Remove(currNode);
				GD.Print(string.Format("add spell #{0} to deck", currNode.Value));
				_reward = false;
			}
			offerIndex++;
			currNode = currNode.Next;
		}

		RefreshOffer();
	}

	private void OnCancelSelection()
	{
		for (int i = 0; i < 3; i++)
		{
			_slots[i].Selected = false;
		}
	}

	private void RefreshOffer()
	{
		var children = GetChildren();
		for (int j = 0; j < children.Count; j++)
		{
			var node = children[j];
			if (node is OfferCard)
			{
				node.QueueFree();
			}
		}
		var currNode = _offer.First;
		for (int i = 0; i < Math.Min(3, _offer.Count); i++)
		{
			var spell = Utils.SpellBook[currNode.Value];
			currNode = currNode.Next;
			var spellCard = (OfferCard)_cardScene.Instantiate();
			spellCard.ConfirmReward += OnConfirmReward;
			var spellSprite = spellCard.GetNode<Sprite2D>("Sprite2D");
			var atlas = (AtlasTexture)Utils.SpriteSheets["spell"].Duplicate();
			atlas.Region = new Rect2(
				new Vector2(spell.xCoord * GameSettings.CardWidth, spell.yCoord * GameSettings.CardLength),
				new Vector2(GameSettings.CardWidth, GameSettings.CardLength));
			spellSprite.Texture = atlas;
			spellCard.Position = new Vector2(_offset * (3 - i), 0);
			AddChild(spellCard);
			_slots[i] = spellCard;
		}
	}

	private void OnAwardSpell() // testing function
	{
		_reward = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
