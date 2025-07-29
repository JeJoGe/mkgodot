using Godot;
using System;

public abstract partial class Offer : Node2D
{
    [Export]
    protected ConfirmationDialog _confirmDialog;
    protected bool _reward = false;
    public bool Reward
    {
        get => _reward;
    }
    public abstract void RefreshOffer();
    protected static readonly int _offset = 112;
    protected PackedScene _cardScene = GD.Load<PackedScene>("res://Offers/OfferCard.tscn");
}
