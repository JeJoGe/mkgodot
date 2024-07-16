using Godot;
using System;

public partial class GamePlay : Node2D
{
	[Export]
	private MapGen mapGen;
	[Export]
	private Deck deck;
	[Export]
	private Player player;

	// Called when the node enters the scene tree for the first time.
	// TODO: Optimize the Callable initialization by calling it in _Ready
	public override void _Ready()
	{

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

	public void OnCardPlayed(string cardAction, CardObj card, CardControl cardControl)
	{
		Utils.undoRedo.CreateAction("Card Play Action");
		string[] action = cardAction.Split('-');
		int quantity;
		if (action[0] == nameof(BasicCardActions.attack))
		{
			quantity = Convert.ToInt16(action[3]);
		}
		else
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
				});
				Callable CombatConversionAttackMinus = Callable.From(() =>
				{
					combatConversion(nameof(BasicCardActions.attack), action[1], action[2], -quantity);
					card.ManipulateButtons(true);
					cardControl.UndoPlayedCardAnimation();
				});
				Utils.undoRedo.AddDoMethod(CombatConversionAttackAdd);
				Utils.undoRedo.AddUndoMethod(CombatConversionAttackMinus);
				Utils.undoRedo.CommitAction();
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

				});
				Callable MoveConversionMinus = Callable.From(() =>
				{
					moveConversion(-quantity);
					card.ManipulateButtons(true);
					cardControl.UndoPlayedCardAnimation();
				});
				Utils.undoRedo.AddDoMethod(MoveConversionAdd);
				Utils.undoRedo.AddUndoMethod(MoveConversionMinus);
				Utils.undoRedo.CommitAction();
				break;
			case nameof(BasicCardActions.influence):
				Callable influenceConversionAdd = Callable.From(() =>
				{
					influenceConversion(quantity);
					card.ManipulateButtons(false);
					cardControl.PlayedCardAnimation();
					
				});
				Callable influenceConversionMinus = Callable.From(() =>
				{
					influenceConversion(-quantity);
					card.ManipulateButtons(true);
					cardControl.UndoPlayedCardAnimation();
				});
				Utils.undoRedo.AddDoMethod(influenceConversionAdd);
				Utils.undoRedo.AddUndoMethod(influenceConversionMinus);
				Utils.undoRedo.CommitAction();
				break;
			case nameof(BasicCardActions.block):
				Callable CombatConversionBlockAdd = Callable.From(() =>
				{
					combatConversion(nameof(BasicCardActions.block), action[1], action[2], quantity);
					card.ManipulateButtons(false);
					cardControl.PlayedCardAnimation();

				});
				Callable CombatConversionBlockMinus = Callable.From(() =>
				{
					combatConversion(nameof(BasicCardActions.block), action[1], action[2], -quantity);
					card.ManipulateButtons(true);
					cardControl.UndoPlayedCardAnimation();
				});
				Utils.undoRedo.AddDoMethod(CombatConversionBlockAdd);
				Utils.undoRedo.AddUndoMethod(CombatConversionBlockMinus);
				Utils.undoRedo.CommitAction();
				break;


		}
	}

	private void combatConversion(string phase, string element, string attackRange, int quantity)
	{
		var combatScene = player.Combat;
		if (combatScene == null) { GD.Print("Currently not in Combat"); return; }

		int index = 0;
		switch (element)
		{
			case nameof(AttackBlockElement.fire):
				index += (int)Element.Fire;
				break;
			case nameof(AttackBlockElement.ice):
				index += (int)Element.Ice;
				break;
			case nameof(AttackBlockElement.coldFire):
				index += (int)Element.ColdFire;
				break;
		}

		switch (attackRange)
		{
			case nameof(AttackType.ranged):
				index += (int)Combat.AttackRange.Ranged;
				break;
			case nameof(AttackType.siege):
				index += (int)Combat.AttackRange.Siege;
				break;
		}

		if (phase == nameof(BasicCardActions.attack))
		{
			combatScene.PlayerAttacks[index] += quantity;
			GD.Print("Attack: ", combatScene.PlayerAttacks[index]);
		}
		else
		{
			combatScene.PlayerBlocks[index] += quantity;
			GD.Print("Block: ", combatScene.PlayerBlocks[index]);
		}

		GD.Print("Index: ", index);
	}

	private void drawConversion(int quantity)
	{
		for (int i = 0; i < quantity; i++)
		{
			deck.OnDeckButtonPressed();
		}
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
			currCard.CardPlayed += cardAction => OnCardPlayed(cardAction, currCard, currCardControl);
		}
		catch
		{
			GD.Print("ONCARDPLAYED signal not connected; ", card);
		}
	}
}
