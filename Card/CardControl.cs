using Godot;
using System;

public partial class CardControl : Control
{
	private bool hover;
	private string cardId;
	private Vector2 size;
	
	// Called when the node enters the scene tree for the first time.
    public CardControl(string _cardId, Vector2 _size) {
		cardId = _cardId;
		size = _size;
	}
	public override void _Ready()
	{
		this.Size = size;
		this.Scale = new Vector2((float)0.25,(float)0.25);
		this.MouseEntered += OnMouseEntered;
		this.MouseExited += OnMouseExited;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	// Zoom if shift pressed and hovering token
	public override void _Input(InputEvent @event)
	{
        if (Input.IsActionPressed("shift") && Input.IsActionPressed("leftClick") && hover) {
			this.Scale = new Vector2((float)0.5, (float)0.5);
        } else if (Input.IsActionJustReleased("shift")) {
			this.Scale = new Vector2((float) 0.25, (float) 0.25);
        } 
	}
	private void OnMouseEntered() {
		hover = true;
	}

	private void OnMouseExited() {
		hover = false;
	}
}
