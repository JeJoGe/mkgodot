using Godot;
using System;
using System.Data.Common;

public partial class PlayButton : Button
{
    private Guid _cardId;
    public PlayButton (Guid cardId) {
        _cardId = cardId;
    }
    public override void _Ready()
    {
        base._Ready();
        this.Text = "Play";
        this.Size = new Vector2I(50, 30);
        this.Scale = new Vector2I(3, 3);
        this.Position = new Vector2I(350, -650);
    }
}
