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

	public void OnCardPlayed(string cardAction) {
		// GD.Print("INONCARDPLAYED: ", cardAction);
		string[] action = cardAction.Split('-');
		int quantity;
		if (action[0] == "attack") {
			quantity = Convert.ToInt16(action[3]);
		} else {
			quantity = Convert.ToInt16(action[1]);
		}
		switch(action[0]) {
			case "attack": combatConversion("attack", action[1], action[2], quantity);
				break;
			case "heal": healingConversion(quantity);
				break;
			case "draw": drawConversion(quantity);
				break;
			case "move": moveConversion(quantity);
				break;
			case "influence": influenceConversion(quantity);
				break;
			case "block": combatConversion("block", action[1], action[2], quantity);
				break;

		}
    }

    private void combatConversion(string phase, string element, string attackRange, int quantity) {
		var player = GetNode<Player>("Player");
		var combatScene =  player.Combat;
		if (combatScene == null) { GD.Print("Currently not in Combat"); return;}
		
		int index = 0;
		switch(element) {
			case "fire":
				index += (int) Element.Fire;
				break;
			case "ice":
				index += (int) Element.Ice;
				break;
			case "coldFire":
				index += (int) Element.ColdFire;
				break;
		}

		switch(attackRange) {
			case "ranged":
				index += (int) Combat.AttackRange.Ranged;
				break;
			case "siege":
				index += (int) Combat.AttackRange.Siege;
				break;
		}

		if (phase == "attack") {
			combatScene.PlayerAttacks[index] = quantity;
		} else {
			combatScene.PlayerBlocks[index] = quantity;
		}

		GD.Print("Index: ", index);
	}

    private void drawConversion(int quantity) {
        for (int i = 0; i < quantity; i++) {
            deck.OnDeckButtonPressed();
        }
    }

    private void moveConversion(int quantity) {
        player.MovePoints += quantity;
        GD.Print(player.MovePoints);
    }

	private void influenceConversion(int quantity){
		var player = GetNode<Player>("Player");
		player.influence = quantity;
		GD.Print("Influence Points: ", quantity);
	}

	private void healingConversion(int quantity) {
		var deck = GetNode<Deck>("Player UI/PlayerArea/Deck");
		
	}

	// When card enters the tree, tie the signal CardPlayed to OnCard
	public void OnCardEntered(Node card) {
		GD.Print("OnCardEnteredTree: ", card);
		try {
			var currCardFrame = (CardControl) card;
			var currCard = (CardObj) currCardFrame.GetChild(0);
			currCard.CardPlayed += cardAction => OnCardPlayed(cardAction);
		} catch {
			GD.Print("ONCARDPLAYED signal not connected; ", card);
		}
	}
}
