using Godot;
using System;

public partial class ManaPopup : VSplitContainer
{
	[Export]
	private HBoxContainer _optionContainer;
	[Export]
	private HSplitContainer _bottomContainer;
	[Export]
	private Window _window;
	[Export]
	private Button _confirmButton;
	private ButtonGroup _optionGroup = new ButtonGroup();
	private int _optionSize = 200;
	public enum ManaType
	{
		Dice, Token, Crystal
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//TESTING ONLY
		AddOption(Source.Colour.Green, ManaType.Token);
		AddOption(Source.Colour.Blue, ManaType.Crystal);
		AddOption(Source.Colour.Red, ManaType.Crystal);
		AddOption(Source.Colour.White, ManaType.Crystal);
		AddOption(Source.Colour.Gold, ManaType.Dice);
		GD.Print("hbox x:", _optionContainer.Size.X);
		GD.Print("hbox y:", _optionContainer.Size.Y);
		GD.Print("this x:", Size.X);
		GD.Print("this y:", Size.Y);
		ResizeWindow();
	}

	private void ResizeWindow()
	{
		var numOptions = _optionContainer.GetChildCount();
		_window.Size = new Vector2I((_optionSize * numOptions)+100, (int)Size.Y+100);
		var halfway = _optionSize * numOptions / 2;
		_bottomContainer.SplitOffset = halfway;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void AddOption(Source.Colour colour, ManaType type)
	{
		var textureRect = new TextureRect
		{
			ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
			CustomMinimumSize = new Vector2(_optionSize,_optionSize)
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
					atlas.Region = new Rect2(new Vector2(0,0), new Vector2(246,246));
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
						atlas.Region = new Rect2(new Vector2(0,0), new Vector2(306,280)); //TODO: remake white crystal sprite to meet dimensions
					}
					break;
				}
			default: break;
		}
		textureRect.Texture = atlas;
		var button = new Button
		{
			CustomMinimumSize = new Vector2(_optionSize, _optionSize),
			ButtonGroup = _optionGroup,
			
		};
		button.Pressed += () => OnOptionSelected(colour,type);
		button.AddChild(textureRect);
		_optionContainer.AddChild(button);
	}

	private void OnOptionSelected(Source.Colour colour, ManaType type)
	{
		// enable confirm button
		_confirmButton.Disabled = false;
		GD.Print(string.Format("{0} {1}",colour.ToString(),type.ToString()));
	}

	private void OnConfirmButtonPressed()
	{
		GD.Print("confirm pressed");
		// consume selected option
		_optionGroup.GetPressedButton();
	}

	private void OnCancelButtonPressed()
	{
		GD.Print("cancel pressed");
		_window.QueueFree();
	}
}
