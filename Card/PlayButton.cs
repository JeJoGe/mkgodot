using Godot;

public partial class PlayButton : Button
{
    private int _cardId;
    public PlayButton (int cardId) {
        _cardId = cardId;
    }
    public override void _Ready()
    {
        base._Ready(); // is this needed?
        this.Text = "Play";
        this.Size = new Vector2I(50, 30);
        this.Scale = new Vector2I(3, 3);
        this.Position = new Vector2I(350, -650);
    }
}
