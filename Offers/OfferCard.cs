using Godot;

public partial class OfferCard : Node2D
{
	[Signal]
	public delegate void ConfirmRewardEventHandler();
	private int _cardId;
	public int CardId
	{
		get => _cardId;
	}
	private CardType _cardType;
	public CardType CardType
	{
		get => _cardType;
	}
	private bool _selected = false;
	public bool Selected
	{
		get => _selected;
		set
		{
			_selected = value;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Initialize(CardType type, int id)
	{
		_cardType = type;
		_cardId = id;
	}

	private void OnInputEvent(Node _viewport, InputEvent inputEvent, long _idx)
	{
		var offer = GetParent<Offer>();
		if (Input.IsActionPressed("leftClick"))
		{
			GD.Print(string.Format("offer card selected: {0} {1}", _cardType.ToString(), _cardId));
			if (offer.Reward)
			{
				_selected = true;
				EmitSignal(SignalName.ConfirmReward);
			}
		}
	}
}
