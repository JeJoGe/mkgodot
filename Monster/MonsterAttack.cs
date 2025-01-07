using Godot;
using System;

public partial class MonsterAttack : Node2D
{
	[Export]
	private Button _button;
	private int _value;
	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
			UpdateButtonText();
		}
	}
	public Element Element { get; set; }
	private bool _blocked = false;
	public bool Blocked
	{
		get
		{
			return _blocked;
		}
		set
		{
			_blocked = value;
			_button.Visible = !_blocked;
		}
	}
	private bool _attacked = false;
	public bool Attacked
	{
		get => _attacked;
		set
		{
			_attacked = value;
			_button.Visible = !_attacked;
		}
	}
	private bool _attacking = true;
	public bool Attacking
	{
		get
		{
			return _attacking;
		}
		set
		{
			_attacking = value;
			var monster = GetParent<Monster>();
            // update monster attacking visual indicator
            monster?.UpdateAttackingIndicator();
            _button.Visible = _attacking;
		}
	}
	private static readonly int _attackOffset = 40;

	public void Initialize(AttackObject data, int index, ButtonGroup group)
	{
		_value = data.Value;
		Element = data.Element;
		_button.Position = new Vector2(-46, 60 + _attackOffset * index);
		_button.ButtonGroup = group;
		_button.Text = string.Format("{0} {1}", Element, Value);
		_button.Name = string.Format("AttackButton{0}", index.ToString());
		_button.ToggleMode = true;
	}

	private void OnAttackButtonToggled()
	{
		GetNode<Combat>("../..").TargetAttack = this;
	}

	public void UpdateButtonText()
	{
		// update button text depending on current phase
		var monster = GetParent<Monster>();
		bool swift, brutal;
		swift = brutal = false;
		if (monster != null)
		{
			swift = monster.Abilities.Contains("swift");
			brutal = monster.Abilities.Contains("brutal");
		}
		var buttonText = string.Format("{0} {1}", Element, _value);
		switch (GetNode<Combat>("../..").CurrentPhase)
		{
			case Combat.Phase.Block:
				{
					buttonText = string.Format("{0} {1}", Element, _value + (swift ? _value : 0));
					break;
				}
			case Combat.Phase.Damage:
				{
					buttonText = string.Format("{0} {1}", Element, _value + (brutal ? _value : 0));
					break;
				}
			default: break;
		}
		_button.Text = buttonText;
		ShowAttackButton();
	}

	public void ShowAttackButton()
	{
		_button.Visible = true;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
