using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerArea : Node2D
{
	[Export]
	private Source _source;
	[Export]
	private Inventory _inventory;
	private PackedScene _manaPopup = GD.Load<PackedScene>("res://ManaPopUp/ManaPopup.tscn");
	private Dictionary<string, bool> _skills = new();
	private ManaPopup _popup;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// TESTING ONLY
		_skills.Add("AR08", true); // add polarization to skill list
		PayMana(Source.Colour.Blue);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public bool PayMana(Source.Colour colour)
	{
		// get options for colour
		var result = false;
		var list = GetOptions(colour);
		result = list.Count > 0;
		CreateManaPopup();
		if (_skills.ContainsKey("AR08") && _skills["AR08"])
		{
			// get options for colour with polarization
			var polarizationList = GetOptions(colour, true);
			result = polarizationList.Count > 0;
			_popup.PopulatePopup(list, polarizationList);
		}
		else
		{
			_popup.PopulatePopup(list);
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

	private void CreateManaPopup()
	{
		_popup = (ManaPopup)_manaPopup.Instantiate();
		AddChild(_popup);
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
		if (colour != Source.Colour.Gold && colour != Source.Colour.Black)
		{
			// only check for matching crystals if basic colour
			foreach (var matchingColour in matchingColours)
			{
				if (_inventory.CrytalCount(matchingColour) > 0)
				{
					options.Add((matchingColour, ManaPopup.ManaType.Crystal));
				}
			}
		}
		// check for matching mana tokens
		foreach (var matchingColour in matchingColours)
		{
			if (_inventory.TokenCount(matchingColour) > 0)
			{
				options.Add((matchingColour, ManaPopup.ManaType.Token));
			}
		}
		return options;
	}

	private List<Source.Colour> GetColours(Source.Colour colour, bool polarization)
	{
		// colour matches itself
		var result = new List<Source.Colour>
		{
			colour
		};
		if (colour != Source.Colour.Gold && colour != Source.Colour.Black)
		{
			// if basic colour include gold
			result.Add(Source.Colour.Gold);
		}
		if (polarization)
		{
			// include opposite colour
			result.Add(Utils.GetOppositeColour(colour));
		}
		return result;
	}

	public void ConsumeMana(Source.Colour colour, ManaPopup.ManaType type)
	{
		switch (type)
		{
			case ManaPopup.ManaType.Dice:
				{
					_source.TakeDie(colour);
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
	}
}
