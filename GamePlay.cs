using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

public partial class GamePlay : Node2D
{
	[Export]
	private MapGen mapGen;
	[Export]
	private Deck deck;
	[Export]
	private Player player;
	[Export]
	private Tactics tactics;
	[Export]
	private Inventory inventory;

	// Called when the node enters the scene tree for the first time.
	// TODO: Optimize the Callable initialization by calling it in _Ready
	public override void _Ready()
	{
		tactics.StartRound += onStartRound;
		tactics.TacticSelected += tacticChosen => onResolveTactic(tacticChosen);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("escape"))
		{
			GetTree().Quit();
		}
	}

	private Source.Colour determineColour(string color) {
		switch(color) {
			case nameof(Source.Colour.Blue):
				return Source.Colour.Blue;
			case nameof(Source.Colour.Green):
				return Source.Colour.Green;
			case nameof(Source.Colour.Red):
				return Source.Colour.Red;
			case nameof(Source.Colour.White):
				return Source.Colour.White;
			default:
				return Source.Colour.Gold;
		}
	}

	public void OnCardPlayed(Godot.Collections.Array<string> basicAction, Godot.Collections.Array<string> specialAction, CardObj card, CardControl cardControl)
	{
		Utils.undoRedo.CreateAction("Card Play Action");

		if (basicAction != null)
		{
			List<Callable> doMethodList = new List<Callable>();
			List<Callable> undoMethodList = new List<Callable>();

			for (int i = 0; i < basicAction.Count(); i++)
			{
				string[] action = basicAction[i].Split('-');
				int quantity = 0;

				if (action[0] == nameof(BasicCardActions.attack))
				{
					quantity = Convert.ToInt16(action[3]);
				}
				else if( action[0] != nameof(BasicCardActions.gainManaTokens))
				{
					quantity = Convert.ToInt16(action[1]);
				}

				switch (action[0])
				{
					case nameof(BasicCardActions.attack):
						Callable CombatConversionAttackAdd = Callable.From(() =>
						{
							combatConversion(nameof(BasicCardActions.attack), action[1], action[2], quantity);
							card.ManipulateButtons(false);
							cardControl.PlayedCardAnimation();
							deck.onRemoveFromCurrentHand(cardControl);
						});
						Callable CombatConversionAttackMinus = Callable.From(() =>
						{
							combatConversion(nameof(BasicCardActions.attack), action[1], action[2], -quantity);
							card.ManipulateButtons(true);
							cardControl.UndoPlayedCardAnimation();
							deck.onAddToCurrentHand(cardControl);
						});
						doMethodList.Add(CombatConversionAttackAdd);
						undoMethodList.Add(CombatConversionAttackMinus);
						break;
					case nameof(BasicCardActions.heal):
						healingConversion(quantity);
						break;
					case nameof(BasicCardActions.draw):
						drawConversion(quantity);
						cardControl.PlayedCardAnimation();
						Utils.undoRedo.ClearHistory();
						Utils.undoRedo.CommitAction();
						break;
					case nameof(BasicCardActions.move):
						Callable MoveConversionAdd = Callable.From(() =>
						{
							moveConversion(quantity);
							card.ManipulateButtons(false);
							cardControl.PlayedCardAnimation();
							deck.onRemoveFromCurrentHand(cardControl);
						});
						Callable MoveConversionMinus = Callable.From(() =>
						{
							moveConversion(-quantity);
							card.ManipulateButtons(true);
							cardControl.UndoPlayedCardAnimation();
							deck.onAddToCurrentHand(cardControl);
						});
						doMethodList.Add(MoveConversionAdd);
						undoMethodList.Add(MoveConversionMinus);
						break;
					case nameof(BasicCardActions.influence):
						Callable influenceConversionAdd = Callable.From(() =>
						{
							influenceConversion(quantity);
							card.ManipulateButtons(false);
							cardControl.PlayedCardAnimation();
							deck.onRemoveFromCurrentHand(cardControl);
						});
						Callable influenceConversionMinus = Callable.From(() =>
						{
							influenceConversion(-quantity);
							card.ManipulateButtons(true);
							cardControl.UndoPlayedCardAnimation();
							deck.onAddToCurrentHand(cardControl);
						});
						doMethodList.Add(influenceConversionAdd);
						undoMethodList.Add(influenceConversionMinus);
						break;
					case nameof(BasicCardActions.block):
						Callable CombatConversionBlockAdd = Callable.From(() =>
						{
							combatConversion(nameof(BasicCardActions.block), action[1], action[2], quantity);
							card.ManipulateButtons(false);
							cardControl.PlayedCardAnimation();
							deck.onRemoveFromCurrentHand(cardControl);
						});
						Callable CombatConversionBlockMinus = Callable.From(() =>
						{
							combatConversion(nameof(BasicCardActions.block), action[1], action[2], -quantity);
							card.ManipulateButtons(true);
							cardControl.UndoPlayedCardAnimation();
							deck.onAddToCurrentHand(cardControl);
						});
						doMethodList.Add(CombatConversionBlockAdd);
						undoMethodList.Add(CombatConversionBlockMinus);
						break;
					case nameof(BasicCardActions.gainManaTokens):
						Source.Colour colour = determineColour(action[1]);
						if (colour == Source.Colour.Gold) {
							GD.Print("Error in determining color of manatoken");
							break;
						}
						Callable GainManaToken = Callable.From(()=> {
							inventory.AddToken((int)colour);
						});
						Callable RemoveManaToken = Callable.From(() => {
							inventory.ConsumeToken((int)colour);
						});
						break;
					case nameof(BasicCardActions.useAdditionalDice):
						break;
					case nameof(BasicCardActions.gainCrystals):
						break;
					default:
						break;
				}
				

				Callable doCallable = Callable.From(() =>
				{
					foreach (Callable method in doMethodList)
					{
						method.Call();
					}
				});
				Callable undoCallable = Callable.From(() =>
				{
					foreach (Callable method in undoMethodList)
					{
						method.Call();
					}
				});
				Utils.undoRedo.AddDoMethod(doCallable);
				Utils.undoRedo.AddUndoMethod(undoCallable);
				Utils.undoRedo.CommitAction();
			}
		}

		if(specialAction != null) {

		}
	}

	private void combatConversion(string phase, string element, string attackRange, int quantity)
	{
		var combatScene = player.Combat;
		if (combatScene == null) { GD.Print("Currently not in Combat"); return; }

		var type = Element.Physical;
		switch (element)
		{
			case nameof(AttackBlockElement.fire):
				type = Element.Fire;
				break;
			case nameof(AttackBlockElement.ice):
				type = Element.Ice;
				break;
			case nameof(AttackBlockElement.coldFire):
				type = Element.ColdFire;
				break;
		}

		var range = Combat.AttackRange.Melee;
		switch (attackRange)
		{
			case nameof(AttackType.ranged):
				range = Combat.AttackRange.Ranged;
				break;
			case nameof(AttackType.siege):
				range = Combat.AttackRange.Siege;
				break;
		}

		if (phase == nameof(BasicCardActions.attack))
		{
			combatScene.AddAttack(quantity, type, range);
		}
		else
		{
			combatScene.AddBlock(quantity, type);
		}

	}

	private void drawConversion(int quantity)
	{
		deck.OnDeckButtonPressed(quantity);
	}

	private void moveConversion(int quantity)
	{
		player.MovePoints += quantity;
		GD.Print(player.MovePoints);
	}

	private void influenceConversion(int quantity)
	{
		player.influence += quantity;
		GD.Print("Influence Points: ", player.influence);
	}

	private void healingConversion(int quantity)
	{

	}

	// When card enters the tree, tie the signal CardPlayed to OnCardPlayed
	public void OnCardEntered(Node card)
	{
		GD.Print("OnCardEnteredTree: ", card);
		try
		{
			var currCardControl = (CardControl)card;
			var currCard = (CardObj)currCardControl.GetChild(0);
			currCard.CardPlayed += (basicAction, specialAction) => OnCardPlayed(basicAction, specialAction, currCard, currCardControl);
		}
		catch
		{
			GD.Print("ONCARDPLAYED signal not connected; ", card);
		}
	}

	public void onStartRound()
	{
		deck.OnDeckButtonPressed(player.cardDrawLimit);
	}

	public void onResolveTactic(string TacticChosen)
	{
		switch (TacticChosen)
		{
			case "Tactic5":
				deck.OnDeckButtonPressed(2);
				break;
			default:
				break;
		}
	}
}
