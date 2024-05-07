using Godot;
using System;

public partial class PowerUpButton : Button
{
    private int _cardId;
    public PowerUpButton (int cardId, string text) {
        _cardId = cardId;
        this.Text = text;
    }
    public override void _Ready()
    {
        base._Ready();
        this.Size = new Vector2I(50, 30);
        this.Scale = new Vector2I(3, 3);
        this.Position = new Vector2I(-100, 330);
    }
}
