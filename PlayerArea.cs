using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public partial class PlayerArea : Node2D
{
	[Signal]
	public delegate void ManaPaidEventHandler();
	[Signal]
	public delegate void BlackManaPaidEventHandler(int colourPaid, string type, string cardName, bool isGoldChosen);

	[Export]
	private Source _source;
	[Export]
	public Inventory _inventory;
	private PackedScene _manaPopup = GD.Load<PackedScene>("res://ManaPopUp/ManaPopup.tscn");
	private Dictionary<string, bool> _skills = new();
	private ManaPopup _popup;
	private bool _night = false;
	private string _cardName = "";
	private bool _isGoldChosen = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// TESTING ONLY
		_skills.Add("AR08", true); // add polarization to skill list

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public bool PayMana(Source.Colour colour, bool isTop = false, string cardName = "")
	{
		// sets the name of the card
		if (cardName != "")
		{
			_cardName = cardName;
		}
		else
		{
			_cardName = "";
		}
		var list = new List<(Source.Colour, ManaPopup.ManaType)>();
		// If card requires Black Mana, it is Crystallize
		if (colour == Source.Colour.Black && isTop && cardName == nameof(CardRequiringInput.Crystallize))
		{
			list = GetAllOptions();

		}
		else if (cardName == nameof(CardRequiringInput.Crystallize) && colour == Source.Colour.Gold)
		{
			list = GetBasicCrystalOptions();
		}
		else
		{
			list = GetOptions(colour);
		}
		// get options for colour
		bool result = list.Count > 0;
		CreateManaPopup(colour);
		if (_skills.ContainsKey("AR08") && _skills["AR08"])
		{
			GD.Print("SKILLS");
			// get options for colour with polarization
			var polarizationList = GetOptions(colour, true);
			result = polarizationList.Count > 0;
			if (colour == Source.Colour.Gold && isTop || _isGoldChosen)
			{
				GD.Print("S2");
				_popup.PopulatePopup(list, polarizationList, true, colour, cardName, isTop);
			}
			else
			{
				_popup.PopulatePopup(list, polarizationList, false, colour, cardName, isTop);

			}
		}
		else
		{
			GD.Print("NOSKILLS");
			if (_isGoldChosen)
			{
				GD.Print("1");
				_popup.PopulatePopup(list, true, colour, cardName, isTop);

			}
			else
			{
				GD.Print("4");
				_popup.PopulatePopup(list, false, colour, cardName, isTop);
			}
		}
		return result;
	}

	public bool ActivateSkill(string skillKey)
	{
		var result = false;
		if (_skills.ContainsKey(skillKey) && _skills[skillKey])
		{
			result = true;
			_skills[skillKey] = false;
		}
		return result;
	}

	private void OnCurrentTurn(long characterId)
	{
		//GD.Print("start turn:" +characterId);
		if (characterId == GameSettings.PlayerCharacter)
		{
			//GD.Print("Disabled: " + GetNode<Button>("EndTurnButton").Disabled.ToString());
			GetNode<Button>("EndTurnButton").Disabled = false;
			//GD.Print("Disabled: " + GetNode<Button>("EndTurnButton").Disabled.ToString());
			GD.Print("It's your turn!");
		}
	}

	public void CreateManaPopup(Source.Colour manaRequired)
	{
		var popup = (Window)_manaPopup.Instantiate();
		_popup = (ManaPopup)popup.GetChild(0);
		_popup._manaRequired = manaRequired;
		AddChild(popup);
	}

	private List<(Source.Colour, ManaPopup.ManaType)> GetBasicCrystalOptions()
	{
		var options = new List<(Source.Colour, ManaPopup.ManaType)>();
		options.Add((Source.Colour.Green, ManaPopup.ManaType.Crystal));
		options.Add((Source.Colour.Blue, ManaPopup.ManaType.Crystal));
		options.Add((Source.Colour.Red, ManaPopup.ManaType.Crystal));
		options.Add((Source.Colour.White, ManaPopup.ManaType.Crystal));
		return options;
	}


	private List<(Source.Colour, ManaPopup.ManaType)> GetAllOptions()
	{
		var options = new List<(Source.Colour, ManaPopup.ManaType)>();
		var colourList = Enum.GetValues(typeof(Source.Colour)).Cast<Source.Colour>();

		foreach (var colour in colourList)
		{
			int diceNum;
			int tokenNum;
			int crystalNum = 0;
			if (colour != Source.Colour.Black)
			{
				if (_source.DiceTaken < _source.DicePerTurn)
				{
					diceNum = _source.GetDiceCount(colour);
					if (diceNum > 0)
					{
						options.Add((colour, ManaPopup.ManaType.Dice));
					}
				}
				tokenNum = _inventory.TokenCount(colour);
				if (tokenNum > 0)
				{
					options.Add((colour, ManaPopup.ManaType.Token));
				}
				var exists = _inventory._crystals.TryGetValue(colour, out crystalNum);
				GD.Print(crystalNum);
				if (exists && crystalNum > 0)
				{
					options.Add((colour, ManaPopup.ManaType.Crystal));
				}
			}
		}
		return options;
	}

	private List<(Source.Colour, ManaPopup.ManaType)> GetOptions(Source.Colour colour, bool polarization = false)
	{
		var options = new List<(Source.Colour, ManaPopup.ManaType)>();
		var matchingColours = GetColours(colour, polarization);
		// check source if dice taken is less than dice per turn
		if (_source.DiceTaken < _source.DicePerTurn)
		{
			foreach (var matchingColour in matchingColours)
			{
				if (_source.GetDiceCount(matchingColour) > 0)
				{
					options.Add((matchingColour, ManaPopup.ManaType.Dice));
				}
			}
		}
		// check for mana stolen die
		if (_inventory.ManaStolenDie != -1 && matchingColours.Contains((Source.Colour)_inventory.ManaStolenDie))
		{
			// TODO: special option to indicate this is the mana stolen die
		}
		// check for matching mana tokens
		foreach (var matchingColour in matchingColours)
		{
			if (_inventory.TokenCount(matchingColour) > 0)
			{
				options.Add((matchingColour, ManaPopup.ManaType.Token));
			}
		}
		if (colour != Source.Colour.Gold && colour != Source.Colour.Black)
		{
			// only check for matching crystals if basic colour
			matchingColours.Remove(Source.Colour.Gold);
			matchingColours.Remove(Source.Colour.Black);
			foreach (var matchingColour in matchingColours)
			{
				if (_inventory.CrystalCount(matchingColour) > 0)
				{
					options.Add((matchingColour, ManaPopup.ManaType.Crystal));
				}
			}
		}
		return options;
	}

	private HashSet<Source.Colour> GetColours(Source.Colour colour, bool polarization)
	{
		// colour matches itself
		var result = new HashSet<Source.Colour>
		{
			colour
		};
		if (colour != Source.Colour.Gold && colour != Source.Colour.Black && !_night)
		{
			// if basic colour include gold during daytime
			result.Add(Source.Colour.Gold);
		}
		if (polarization)
		{
			// include opposite colour
			result.Add(Utils.GetOppositeColour(colour));
			if (!_night)
			{
				// during daytime can use black mana as any other colour
				result.Add(Source.Colour.Black);
			}
		}
		return result;
	}

	public void ConsumeMana(Source.Colour colour, ManaPopup.ManaType type, Source.Colour manaRequired, string cardName = "", bool isTop = false)
	{

		switch (type)
		{
			case ManaPopup.ManaType.Dice:
				{
					_source.TakeDie(colour);
					if (colour == Source.Colour.Gold && manaRequired == Source.Colour.Black)
					{
						_isGoldChosen = true;
						PayMana(colour, isTop, _cardName);
					}
					break;
				}
			case ManaPopup.ManaType.Crystal:
				{
					_inventory.ConsumeCrystal(colour);
					break;
				}
			case ManaPopup.ManaType.Token:
				{
					_inventory.ConsumeToken(colour);
					break;
				}
			default: break;
		}
		if (manaRequired == Source.Colour.Black && _isGoldChosen)
		{
			EmitSignal(SignalName.BlackManaPaid, (int)colour, nameof(type), _cardName, true);
			return;
		}
		if (manaRequired == Source.Colour.Black && isTop)
		{
			GD.Print("EMITTING SIGNAL");
			EmitSignal(SignalName.BlackManaPaid, (int)colour, nameof(type), _cardName, false);
		}
		else
		{
			EmitSignal(SignalName.ManaPaid, _cardName);
		}
	}

	private void OnEndTurn()
	{
		// refresh all once per turn skills
		// TESTING ONLY
		_skills["AR08"] = true;
	}
}
