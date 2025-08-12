using Godot;
using System.Collections.Generic;

public partial class UnitArea : Node2D
{
    [Signal]
    public delegate void UnitEnteredEventHandler(Unit unit);
    private static readonly int _cardOffset = 200;
    private PackedScene _unitScene = GD.Load<PackedScene>("res://Unit/Unit.tscn");

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // for testing
        //GameSettings.UnitList = new List<(int, bool, int)>([(1, true, 0), (2, true, 0), (6, true, 2)]);
        // instantiate units
        RefreshUnitList();
    }

    private void OnRecruit()
    {
        RefreshUnitList();
    }

    private void RefreshUnitList()
    {
        var children = GetChildren();
        for (int j = 0; j < children.Count; j++)
        {
            var node = children[j];
            if (node is Unit)
            {
                node.QueueFree();
            }
        }
        for (int i = 0; i < GameSettings.UnitList.Count; i++)
        {
            var unit = GameSettings.UnitList[i];
            var unitCard = (Unit)_unitScene.Instantiate();
            var unitStats = Utils.UnitStats[unit.Item1];
            unitCard.PopulateStats(unitStats);
            var unitSprite = unitCard.GetNode<Sprite2D>("Sprite2D");
            var atlas = (AtlasTexture)Utils.SpriteSheets[unitCard.Level > 2 ? "gold" : "silver"].Duplicate();
            atlas.Region = new Rect2(
                new Vector2(unitStats.X * GameSettings.CardWidth, unitStats.Y * GameSettings.CardLength),
                new Vector2(GameSettings.CardWidth, GameSettings.CardLength));
            unitSprite.Texture = atlas;
            unitCard.Position = new Vector2(_cardOffset * i + 100, 200);
            AddChild(unitCard);
            EmitSignal(SignalName.UnitEntered, unitCard);
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
