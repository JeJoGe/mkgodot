using Godot;
using System;
using System.Collections.Generic;

public partial class SpellOffer : Node2D
{
	private LinkedList<int> _offer;
	private Dictionary<int,Spell> _slots;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// initialize spell deck
		var includedCards = new List<int>();
		foreach (var kvp in Utils.SpellBook)
		{
			// TODO: check for cooperative/competitive spells
			if (GameSettings.Expansions.Contains(kvp.Value.Version))
			{
				includedCards.Add(kvp.Key);
			}
		}
		_offer = new LinkedList<int>(includedCards.Shuffle());
		// initialize spell offer
		var currNode = _offer.First;
		for (int i = 0; i < 2; i++)
		{
			var spell = Utils.SpellBook[currNode.Value];
			_slots.Add(i, spell);
			currNode = currNode.Next;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
