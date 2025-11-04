using Godot;
using System;
using System.Collections.Generic;

public partial class ManaPopup : VSplitContainer
{
	[Export]
	private HBoxContainer _optionContainer;
	[Export]
	private GridContainer _bottomContainer;
	[Export]
	private Window _window;
	[Export]
	private Button _confirmButton;
	[Export]
	private CheckButton _polarizationToggle;
	private ButtonGroup _optionGroup = new ButtonGroup();
	private ButtonGroup _polarizationButtonGroup = new ButtonGroup();
	private bool _polarization = false;
	private (Source.Colour, ManaType) _selectedOption;
	private int _optionSize = 200;
	public Source.Colour _manaRequired;
	public bool _changeConfirm = false;

	// if true, its Crystallize
	private string _cardName = "";
	private bool _isTop = false;
	public enum ManaType
	{
		Dice, Token, Crystal
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public void PopulatePopup(List<(Source.Colour, ManaType)> options, bool changeConfirm, Source.Colour manaRequired, string cardName = "", bool isTop = false)
	{

		_cardName = cardName;
		_isTop = isTop;
		_manaRequired = manaRequired;
		_changeConfirm = changeConfirm;

		foreach (var item in options)
		{
			AddOption(item.Item1, item.Item2);
		}
		ResizeWindow();
	}

	public void PopulatePopup(List<(Source.Colour, ManaType)> options, List<(Source.Colour, ManaType)> polarizationOptions, bool changeConfirm, Source.Colour manaRequired, string cardName = "", bool isTop = false)
	{
		_cardName = cardName;
		_isTop = isTop;
		_manaRequired = manaRequired;
		_changeConfirm = changeConfirm;
		_polarizationToggle.Visible = true;

		PopulatePopup(options, _changeConfirm, manaRequired, cardName, isTop);
		foreach (var item in polarizationOptions)
		{
			AddOption(item.Item1, item.Item2, true);
		}
		ResizeWindow();
	}

	private void ResizeWindow()
	{
		var numOptions = _polarization ? _polarizationButtonGroup.GetButtons().Count : _optionGroup.GetButtons().Count;
		_window.Size = new Vector2I((_optionSize * numOptions) + 100, (int)Size.Y + 100);
		//use below when using hsplitcontainer for bottom container
		//var halfway = _optionSize * numOptions / 2;
		//_bottomContainer.SplitOffset = halfway;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void AddOption(Source.Colour colour, ManaType type, bool polarization = false)
	{
		var textureRect = new TextureRect
		{
			ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
			CustomMinimumSize = new Vector2(_optionSize, _optionSize)
		};
		AtlasTexture atlas = new AtlasTexture();
		switch (type)
		{
			case ManaType.Dice:
				{
					atlas = (AtlasTexture)Utils.SpriteSheets["dice"].Duplicate();
					atlas.Region = new Rect2(new Vector2(Utils.DiceCoordinates[colour].Item1 * Utils.DiceSize,
						Utils.DiceCoordinates[colour].Item2 * Utils.DiceSize),
						new Vector2(Utils.DiceSize, Utils.DiceSize));
					break;
				}
			case ManaType.Token:
				{
					atlas = (AtlasTexture)Utils.ManaSprites[colour].Duplicate();
					atlas.Region = new Rect2(new Vector2(0, 0), new Vector2(246, 246));
					break;
				}
			case ManaType.Crystal:
				{
					if (colour == Source.Colour.Gold || colour == Source.Colour.Black)
					{
						GD.Print(string.Format("impossible option: {0} crystal", colour.ToString()));
						// throw error
					}
					else
					{
						atlas = (AtlasTexture)Utils.CrystalSprites[colour].Duplicate();
						atlas.Region = new Rect2(new Vector2(0, 0), new Vector2(306, 280)); //TODO: remake white crystal sprite to meet dimensions
					}
					break;
				}
			default: break;
		}
		textureRect.Texture = atlas;
		var button = new Button
		{
			CustomMinimumSize = new Vector2(_optionSize, _optionSize),
			ButtonGroup = polarization ? _polarizationButtonGroup : _optionGroup,
			Visible = !polarization
		};
		button.Pressed += () => OnOptionSelected(colour, type);
		button.AddChild(textureRect);
		_optionContainer.AddChild(button);
	}

	private void OnOptionSelected(Source.Colour colour, ManaType type)
	{
		// enable confirm button
		_confirmButton.Disabled = false;
		GD.Print(string.Format("{0} {1}", colour.ToString(), type.ToString()));
		_selectedOption = (colour, type);
	}

	private void OnConfirmButtonPressed()
	{
		GD.Print("confirm pressed");
		var playerArea = GetNode<PlayerArea>("../..");
		if (_polarization)
		{
			// set polarization as used this turn
			if (!playerArea.ActivateSkill("AR08"))
			{
				throw new InvalidOperationException("Failed to activate Polarization skill");
			}
		}
		if (_changeConfirm)
		{
			GD.Print("CONFIRM CHANGED,", (int)_selectedOption.Item1);
			playerArea._inventory.AddCrystal((int)_selectedOption.Item1);
		}
		else
		{
			GD.Print("CONSUME");
			playerArea.ConsumeMana(_selectedOption.Item1, _selectedOption.Item2, _manaRequired, _cardName, _isTop);
		}
		// consume selected option
		_window.QueueFree();
	}

	private void OnCancelButtonPressed()
	{
		GD.Print("cancel pressed");
		GetTree().Paused = false;
		_window.QueueFree();
	}

	private void OnPolarizationToggled(bool toggledOn)
	{
		GD.Print("polarization toggled" + toggledOn);
		_polarization = toggledOn;
		foreach (var button in _optionGroup.GetButtons())
		{
			button.Visible = !toggledOn;
		}
		foreach (var button in _polarizationButtonGroup.GetButtons())
		{
			button.Visible = toggledOn;
		}
		ResizeWindow();
	}
}
