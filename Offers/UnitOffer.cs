using Godot;
using System;
using System.Collections.Generic;

public partial class UnitOffer : Offer
{
    [Signal]
    public delegate void RecruitEventHandler();
    private Stack<int> _silverUnits;
    private Stack<int> _goldUnits;
    private List<OfferCard> _offer;
    private PackedScene _unitScene = GD.Load<PackedScene>("res://Offers/OfferCard.tscn");


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // initialize unit decks
        var silverUnits = new List<int>();
        foreach (var kvp in Utils.UnitStats)
        {
            // initialize silver units
            if (kvp.Value.Level < 3)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    silverUnits.Add(kvp.Key);
                }
            }
        }
        _silverUnits = new Stack<int>(silverUnits.Shuffle());
        // TODO: initialize gold units
        RefreshOffer();
    }

    private void NewRound()
    {
        if (SharedArea.Round > 1)
        {
            RefreshOffer();
        }
    }

    public override void RefreshOffer()
    {
        _offer = [];
        var children = GetChildren();
        for (int j = 0; j < children.Count; j++)
        {
            var node = children[j];
            if (node is OfferCard)
            {
                node.QueueFree();
            }
        }
        // draw cards from silver deck equal to number of players + 2
        for (int i = 0; i < GameSettings.NumPlayers + 2; i++)
        {
            var unitId = _silverUnits.Pop();
            var unitCard = (OfferCard)_cardScene.Instantiate();
            unitCard.ConfirmReward += OnConfirmReward;
            unitCard.Initialize(CardType.Unit, unitId);
            var unitStats = Utils.UnitStats[unitId];
            var unitSprite = unitCard.GetNode<Sprite2D>("Sprite2D");
            var atlas = (AtlasTexture)Utils.SpriteSheets[unitStats.Level > 2 ? "gold" : "silver"].Duplicate();
            atlas.Region = new Rect2(
                new Vector2(unitStats.X * GameSettings.CardWidth, unitStats.Y * GameSettings.CardLength),
                new Vector2(GameSettings.CardWidth, GameSettings.CardLength));
            unitSprite.Texture = atlas;
            unitCard.Position = new Vector2(_offset * (i + 1), 0);
            AddChild(unitCard);
            _offer.Add(unitCard);
        }
    }

    private void OnConfirmReward()
    {
        _confirmDialog.Visible = true;
    }

    private void OnConfirmSelection()
    {
        var offerIndex = 0;
        while (_reward)
        {
            var card = _offer[offerIndex];
            if (card.Selected)
            {
                if (card.CardType is CardType.Unit)
                {
                    GameSettings.UnitList.Add((card.CardId, true, 0));
                    EmitSignal(SignalName.Recruit);
                }
                else
                {
                    // TODO: implement adding advanced actions from unit offer
                    GD.Print("add action card to deck");
                }
                _offer.RemoveAt(offerIndex);
                card.QueueFree();
                _reward = false;
            }
            offerIndex++;
        }
    }

    private void OnCancelSelection()
    {
        GD.Print("cancel unit selection");
        foreach (var card in _offer)
        {
            card.Selected = false;
        }
    }

    private void OnRecruitUnit() // function for testing only
    {
        _reward = true;
    }

    // TODO: needed for monasteries
    private void OnAddActionCard(int id)
    {

    }
}
