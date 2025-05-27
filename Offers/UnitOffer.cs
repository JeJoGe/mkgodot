 using Godot;
using System;
using System.Collections.Generic;

public partial class UnitOffer : Node2D
{
    private Stack<int> _silverUnits;
    private Stack<int> _goldUnits;
    private PackedScene _unitScene = GD.Load<PackedScene>("res://Unit/Unit.tscn");
    private static readonly int _cardOffset = 200;


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
    }

    private void NewRound()
    {
        // draw cards from silver deck equal to number of players + 2
        for (int i = 0; i < GameSettings.NumPlayers + 2; i++)
        {
            var unitId = _silverUnits.Pop();
            var unitCard = (Unit)_unitScene.Instantiate();
            var unitStats = Utils.UnitStats[unitId];
            unitCard.PopulateStats(unitStats);
            var unitSprite = unitCard.GetNode<Sprite2D>("Sprite2D");
            var atlas = (AtlasTexture)Utils.SpriteSheets[unitCard.Level > 2 ? "gold" : "silver"].Duplicate();
            atlas.Region = new Rect2(
                new Vector2(unitStats.X * GameSettings.CardWidth, unitStats.Y * GameSettings.CardLength),
                new Vector2(GameSettings.CardWidth, GameSettings.CardLength));
            unitSprite.Texture = atlas;
            unitCard.Position = new Vector2(_cardOffset * i + 100, 0);
            AddChild(unitCard);
        }
    }
}
