using Godot;

public partial class OfferCard : Node2D
{
	[Signal]
	public delegate void ConfirmRewardEventHandler();
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

	private void OnInputEvent(Node _viewport, InputEvent inputEvent, long _idx)
	{
		var offer = GetParent<SpellOffer>();
		if (Input.IsActionPressed("leftClick"))
		{
			GD.Print("offer card selected");
			if (offer.Reward)
			{
				_selected = true;
				EmitSignal(SignalName.ConfirmReward);
			}
		}
	}
}
