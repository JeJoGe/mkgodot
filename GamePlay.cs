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
	[Export]
	private PlayerArea _playerArea;
	private bool _resolvingAction = false;
	private CardObj _currentCard;
	private CardControl _currentCardControl;
	private Godot.Collections.Array<string> _currentBasicActions;
	private Godot.Collections.Array<string> _currentSpecialActions;
	private Godot.Collections.Array<string> _currentManaCosts;
	private Godot.Collections.Array<string> _currentRewards;
	public bool ResolvingAction { get => _resolvingAction; } // prevent another action from being activated while current action resolves

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

	private Source.Colour determineColour(string color)
	{
		switch (color)
		{
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

	public void OnCardPlayed(Godot.Collections.Array<string> manaCosts,
	Godot.Collections.Array<string> basicAction, Godot.Collections.Array<string> specialAction, CardObj card, CardControl cardControl)
	{
		if (!ResolvingAction)
		{
			_currentManaCosts = manaCosts;
			_currentBasicActions = basicAction;
			_currentSpecialActions = specialAction;
			_currentCard = card;
			_currentCardControl = cardControl;
			ResolveManaCosts();
		}
	}

	public void OnManaRuinsInteract(Godot.Collections.Array<string> manaCosts, Godot.Collections.Array<string> rewards)
	{
		_currentManaCosts = manaCosts;
		_currentRewards = rewards;
		ResolveManaCosts();
	}

	private void OnManaPaid()
	{
		_currentManaCosts.RemoveAt(0);
		ResolveManaCosts();
	}

	private void ResolveManaCosts()
	{
		//GD.Print("Color: " + _currentManaCosts[0]);
		if (_currentManaCosts.Count == 0)
		{
			if (_currentCard != null)
			{
				// all mana costs paid TODO: this will not necessarily be a card action (could be unit/skill)
				PerformCardActions(_currentBasicActions, _currentSpecialActions, _currentCard, _currentCardControl);
				_resolvingAction = false;
				_currentCard = null;
			}
			else if (_currentRewards.Count != 0)
			{
				while(_currentRewards.Count != 0)
				{
					var isNumeric = int.TryParse(_currentRewards[0], out int n);
					if(isNumeric)
					{
						_playerArea.GainReward(PlayerArea.Reward.fame, n);
					}
					else if(_currentRewards[0] == "unit")
					{
						_playerArea.GainReward(PlayerArea.Reward.unit);
					}
					else if(_currentRewards[0] == "spell")
					{
						_playerArea.GainReward(PlayerArea.Reward.spell);
					}
					else if(_currentRewards[0] == "advanced")
					{
						_playerArea.GainReward(PlayerArea.Reward.advanced);
					}
					else if(_currentRewards[0] == "artifact")
					{
						_playerArea.GainReward(PlayerArea.Reward.artifact);
					}
					else
					{
						_playerArea.GainReward(PlayerArea.Reward.crystal, (int)Utils.ConvertStringToSourceColour(_currentRewards[0]));
					}
				}
				GetTree().Paused = false;
				// Perform rewards
			}
		}
		else if (!_playerArea.PayMana(Utils.ConvertStringToSourceColour(_currentManaCosts[0]), true)) // TODO
		{
			// no mana available to complete action TODO: return already spent mana
			_resolvingAction = false;
		}
	}

	private void PerformCardActions(Godot.Collections.Array<string> basicAction, Godot.Collections.Array<string> specialAction, CardObj card, CardControl cardControl)
	{
		Utils.undoRedo.CreateAction("Card Play Action");
		GD.Print(basicAction);


		if (basicAction != null)
		{
			List<Callable> doMethodList = new List<Callable>();
			List<Callable> undoMethodList = new List<Callable>();

			for (int i = 0; i < basicAction.Count(); i++)
			{
				string[] actions = basicAction[i].Split("&");
				foreach (string completeAction in actions)
				{
					string[] action = completeAction.Split('-');
					int quantity = 0;
					bool isQuantity = true;

					if (action[0] == nameof(BasicCardActions.attack))
					{
						quantity = Convert.ToInt16(action[3]);
					}
					else
					{
						isQuantity = int.TryParse(action[1], out quantity);
					}

					switch (action[0])
					{
						case nameof(BasicCardActions.attack):
							Callable CombatConversionAttackAdd = Callable.From(() =>
							{
								combatConversion(nameof(BasicCardActions.attack), action[1], action[2], quantity);
								doMethodCardDeckUpdates(card, cardControl);
							});
							Callable CombatConversionAttackMinus = Callable.From(() =>
							{
								combatConversion(nameof(BasicCardActions.attack), action[1], action[2], -quantity);
								undoMethodCardDeckUpdates(card, cardControl);
							});
							doMethodList.Add(CombatConversionAttackAdd);
							undoMethodList.Add(CombatConversionAttackMinus);
							break;
						case nameof(BasicCardActions.heal):
							Callable HealingConversion = Callable.From(() =>
							{
								deck.RemoveWoundFromHand(quantity);
							});
							Callable ReinstateWound = Callable.From(() =>
							{
								deck.AddWoundToHand(quantity);
							});
							doMethodList.Add(HealingConversion);
							undoMethodList.Add(ReinstateWound);
							break;
						case nameof(BasicCardActions.draw):
							deck.OnDeckButtonPressed(quantity);
							cardControl.PlayedCardAnimation();
							Utils.undoRedo.ClearHistory();
							Utils.undoRedo.CommitAction();
							break;
						case nameof(BasicCardActions.move):
							Callable MoveConversionAdd = Callable.From(() =>
							{
								player.MovePoints += quantity;
								doMethodCardDeckUpdates(card, cardControl);

							});
							Callable MoveConversionMinus = Callable.From(() =>
							{
								player.MovePoints -= quantity;
								undoMethodCardDeckUpdates(card, cardControl);
							});
							doMethodList.Add(MoveConversionAdd);
							undoMethodList.Add(MoveConversionMinus);
							break;
						case nameof(BasicCardActions.influence):
							Callable influenceConversionAdd = Callable.From(() =>
							{
								player.influence += quantity;
								doMethodCardDeckUpdates(card, cardControl);
							});
							Callable influenceConversionMinus = Callable.From(() =>
							{
								player.influence -= quantity;
								undoMethodCardDeckUpdates(card, cardControl);
							});
							doMethodList.Add(influenceConversionAdd);
							undoMethodList.Add(influenceConversionMinus);
							break;
						case nameof(BasicCardActions.block):
							Callable CombatConversionBlockAdd = Callable.From(() =>
							{
								combatConversion(nameof(BasicCardActions.block), action[1], action[2], quantity);
								doMethodCardDeckUpdates(card, cardControl);
							});
							Callable CombatConversionBlockMinus = Callable.From(() =>
							{
								combatConversion(nameof(BasicCardActions.block), action[1], action[2], -quantity);
								undoMethodCardDeckUpdates(card, cardControl);
							});
							doMethodList.Add(CombatConversionBlockAdd);
							undoMethodList.Add(CombatConversionBlockMinus);
							break;
						case nameof(BasicCardActions.gainManaTokens):
							Source.Colour colour = determineColour(action[1]);
							if (colour == Source.Colour.Gold)
							{
								GD.Print("Error in determining color of manatoken");
								break;
							}
							Callable GainManaToken = Callable.From(() =>
							{
								inventory.AddToken((int)colour);
								doMethodCardDeckUpdates(card, cardControl);

							});
							Callable RemoveManaToken = Callable.From(() =>
							{
								inventory.ConsumeToken((int)colour);
								undoMethodCardDeckUpdates(card, cardControl);

							});
							doMethodList.Add(GainManaToken);
							undoMethodList.Add(RemoveManaToken);
							break;
						case nameof(BasicCardActions.useAdditionalDice):
							break;
						case nameof(BasicCardActions.gainCrystals):
							break;
						case nameof(BasicCardActions.payMana):
							break;
						default:
							break;
					}
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
			}
			Utils.undoRedo.CommitAction();

		}

		if (specialAction != null)
		{

		}
	}

	private void doMethodCardDeckUpdates(CardObj card, CardControl cardControl)
	{
		card.ManipulateButtons(false);
		cardControl.PlayedCardAnimation();
		deck.onRemoveFromCurrentHand(cardControl);

	}

	private void undoMethodCardDeckUpdates(CardObj card, CardControl cardControl)
	{
		card.ManipulateButtons(true);
		cardControl.UndoPlayedCardAnimation();
		deck.onAddToCurrentHand(cardControl);

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

	// When card enters the tree, tie the signal CardPlayed to OnCardPlayed
	public void OnCardEntered(Node card)
	{
		GD.Print("OnCardEnteredTree: ", card);
		try
		{
			var currCardControl = (CardControl)card;
			var currCard = (CardObj)currCardControl.GetChild(0);
			currCard.CardPlayed += (basicAction, specialAction, manaCosts) => OnCardPlayed(manaCosts, basicAction, specialAction, currCard, currCardControl);
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

	private void OnEndTurn()
	{
		deck.OnDeckButtonPressed(player.cardDrawLimit - deck.CurrentHand.Count);
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
