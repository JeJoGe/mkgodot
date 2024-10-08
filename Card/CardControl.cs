using Godot;

public partial class CardControl : Control
{
	private bool hover;
	private string cardId;
	private Vector2 size;
	public enum CardStates
	{
		InDeck = 0,
		InHand = 1,
		MoveDrawnCardToHand = 2,
		ReorganizeHand = 3,
		inFocus = 4,
		InFocusBackToHand = 5,
		InFocusEnlarged = 6,
		PlayedCard = 7,
		MovePlayedCardToHand = 8,
		InDiscard = 9
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
	private Vector2 discardArea;
	private Vector2 backToHandPos;



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

		var deck = GetParent();
		var button = deck.GetChild<TextureButton>(0);
		var buttonPos = button.GetRect().Position;
		discardArea = new Vector2(buttonPos.X - 100, buttonPos.Y - 300);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	// Zoom if shift pressed and hovering token
	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionPressed("shift") && Input.IsActionPressed("leftClick") && hover && !isFocused && cardState == CardStates.InHand)
		{
			this.originalBeforeFocusPos = this.Position;
			this.Scale = new Vector2((float)0.5, (float)0.5);
			this.ZIndex = 1;
			this.startPos = this.Position;
			this.targetPos = new Vector2(this.Position.X, this.Position.Y - 600);
			this.cardState = CardStates.inFocus;
			this.isFocused = true;
		}
		else if (Input.IsActionJustReleased("shift") && isFocused)
		{
			this.ZIndex = 0;
			this.startPos = this.Position;

			if (cardState == CardStates.InFocusEnlarged || cardState == CardStates.inFocus || cardState == CardStates.InFocusBackToHand || cardState == CardStates.InHand)
			{
				this.Scale = new Vector2((float)0.25, (float)0.25);
				this.targetPos = this.originalBeforeFocusPos;
				this.cardState = CardStates.InFocusBackToHand;
			}
			else
			{
				this.Scale = new Vector2((float)0.125, (float)0.125);
				this.targetPos = this.Position;
				this.cardState = CardStates.InDiscard;
			}
			this.isFocused = false;

		}

		if(hover) {
			ZIndex = 1;
		} else if(!hover) {
			ZIndex = 0;
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
		switch (cardState)
		{
			case CardStates.InHand:
				break;
			case CardStates.InFocusEnlarged:
				break;
			case CardStates.MoveDrawnCardToHand:
				if (time <= 1)
				{
					this.Position = startPos.Lerp(targetPos, (float)time);
					this.RotationDegrees = startRotation * (1 - (float)time) + (targetRotation * (float)time);
					time += delta / (float)DRAWTIME;
				}
				else
				{
					this.Position = targetPos;
					this.RotationDegrees = targetRotation;
					cardState = CardStates.InHand;
					time = 0;
				}
				break;
			case CardStates.ReorganizeHand:
				if (time <= 1)
				{
					this.Position = startPos.Lerp(targetPos, (float)time);
					this.RotationDegrees = startRotation * (1 - (float)time) + (targetRotation * (float)time);
					time += delta / (float)DRAWTIME;
				}
				else
				{
					this.Position = targetPos;
					this.RotationDegrees = targetRotation;
					cardState = CardStates.InHand;
					time = 0;
				}
				break;
			case CardStates.inFocus:
				if (time <= 1)
				{
					this.Position = startPos.Lerp(targetPos, (float)time);
					time += delta / (float)DRAWTIME;
				}
				else
				{
					this.Position = targetPos;
					cardState = CardStates.InFocusEnlarged;
					time = 0;

				}
				break;
			case CardStates.InFocusBackToHand:
				if (time <= 1)
				{
					this.Position = startPos.Lerp(targetPos, (float)time);
					time += delta / (float)DRAWTIME;
				}
				else
				{
					this.Position = targetPos;
					cardState = CardStates.InHand;
					time = 0;
				}
				break;
			case CardStates.PlayedCard:
				if (time <= 1)
				{
					this.Position = startPos.Lerp(targetPos, (float)time);
					time += delta / (float)DRAWTIME;
				}
				else
				{
					this.Position = targetPos;
					cardState = CardStates.InDiscard;
					time = 0;
				}
				break;
			case CardStates.MovePlayedCardToHand:
				if (time <= 1)
				{
					this.Position = startPos.Lerp(targetPos, (float)time);
					time += delta / (float)DRAWTIME;
				}
				else
				{
					this.Position = targetPos;
					cardState = CardStates.InHand;
					time = 0;
				}
				break;
			default:
				break;
		}
	}

	public void PlayedCardAnimation()
	{
		startPos = Position;
		if(cardState == CardStates.InFocusEnlarged) {
			backToHandPos = originalBeforeFocusPos;
		} else {
			backToHandPos = startPos;
		}
		targetPos = discardArea;
		this.cardState = CardStates.PlayedCard;
		this.Scale = new Vector2((float)0.125, (float)0.125);
	}

	public void UndoPlayedCardAnimation()
	{
		this.startPos = this.Position;
		this.targetPos = backToHandPos;
		this.cardState = CardStates.MovePlayedCardToHand;
		this.Scale = new Vector2((float)0.25, (float)0.25);
	}

}
