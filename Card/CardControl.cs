using Godot;
using System;

public partial class CardControl : Control
{
	private bool hover;
	private string cardId;
	private Vector2 size;
    public enum CardStates {
        InDeck = 0,
        InHand = 1, 
        MoveDrawnCardToHand = 2,
		ReorganizeHand = 3,
		inFocus = 4,
		InFocusBackToHand = 5,
		InFocusEnlarged = 6
    }
    public double time = 0;
    public double DRAWTIME = 0.5;

    public CardStates cardState = CardStates.InDeck;
    public Godot.Vector2 startPos;
    public Godot.Vector2 targetPos;

	public float startRotation;
	public float targetRotation;

	private Godot.Vector2 originalBeforeFocusPos;
	private bool isFocused = false;



	// Called when the node enters the scene tree for the first time.
	public CardControl(string _cardId, Vector2 _size)
	{
		cardId = _cardId;
		size = _size;
	}
	public override void _Ready()
	{
		this.Size = size;
		this.Scale = new Vector2((float)0.25, (float)0.25);
		this.MouseEntered += OnMouseEntered;
		this.MouseExited += OnMouseExited;
		this.MouseFilter = MouseFilterEnum.Pass;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	// Zoom if shift pressed and hovering token
	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionPressed("shift") && Input.IsActionPressed("leftClick") && hover)
		{
			originalBeforeFocusPos = this.Position;
			this.Scale = new Vector2((float)0.5, (float)0.5);
			this.ZIndex = 1;
			this.startPos = originalBeforeFocusPos;
			this.targetPos = new Vector2(this.Position.X, this.Position.Y - 600);
			this.cardState = CardStates.inFocus;
			this.isFocused = true;
		}
		else if (Input.IsActionJustReleased("shift") && isFocused)
		{
			this.Scale = new Vector2((float)0.25, (float)0.25);
			this.ZIndex = 0;
			this.startPos = this.Position;
			this.targetPos = originalBeforeFocusPos;
			this.cardState = CardStates.InFocusBackToHand;
			this.isFocused = false;
		}
		
	}
	private void OnMouseEntered()
	{
		hover = true;
	}

	private void OnMouseExited()
	{
		hover = false;
	}
	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb)
		{
			if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
			{
				GD.Print("Card been clicked D:");
			}
		}
	}

	public override void _PhysicsProcess(double delta)
    {
        switch (cardState) {
            case CardStates.InHand:
                break;
			case CardStates.InFocusEnlarged:
				break;
            case CardStates.MoveDrawnCardToHand:
                if (time <= 1) {
                    this.Position = startPos.Lerp(targetPos, (float) time);
					this.RotationDegrees = startRotation*(1-(float)time)+(targetRotation*(float)time);
                    time += delta/(float)DRAWTIME;
                } else {
                    this.Position = targetPos;
					this.RotationDegrees = targetRotation;
                    cardState = CardStates.InHand;
                    time = 0;
                }
                break;
			case CardStates.ReorganizeHand:
			 if (time <= 1) {
                    this.Position = startPos.Lerp(targetPos, (float) time);
					this.RotationDegrees = startRotation*(1-(float)time)+(targetRotation*(float)time);
                    time += delta/(float)DRAWTIME;
                } else {
                    this.Position = targetPos;
					this.RotationDegrees = targetRotation;
                    cardState = CardStates.InHand;
                    time = 0;
                }
                break;
			case CardStates.inFocus:
			if (time <= 1) {
                    this.Position = startPos.Lerp(targetPos, (float) time);
                    time += delta/(float)DRAWTIME;
                } else {
                    this.Position = targetPos;
					cardState = CardStates.InFocusEnlarged;
					time = 0;

                }
                break;
			case CardStates.InFocusBackToHand:
			if (time <= 1) {
                    this.Position = startPos.Lerp(targetPos, (float) time);
                    time += delta/(float)DRAWTIME;
                } else {
                    this.Position = targetPos;
					cardState = CardStates.InHand;
					time = 0;
                }
                break;
            default:
                break;
        }
        }
}
