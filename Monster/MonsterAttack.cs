using Godot;
using System;

public partial class MonsterAttack : Node2D
{
	[Export]
	private Button _button;
	public int Value { get; set; }
	public Element Element { get; set; }
	public bool Blocked { get; set; } = false;
	public bool Attacked { get; set; } = false;
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
			if (monster != null)
			{
				// update monster attacking visual indicator
				monster.UpdateAttackingIndicator();
			}
			_button.Visible = _attacking;
		}
	}
	private static readonly int _attackOffset = 40;

	public void Initialize(AttackObject data, int index, ButtonGroup group)
	{
		Value = data.Value;
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

	public void UpdateButtonText(string newText)
	{
		GetNode<Combat>("../..").UndoRedo.AddUndoProperty(_button, "text", _button.Text);
		_button.Text = newText;
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
