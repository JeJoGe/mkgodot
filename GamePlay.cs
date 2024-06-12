using Godot;
using System;
using System.Collections.Generic;

public partial class GamePlay : Node2D
{
	PackedScene monsterScene = GD.Load<PackedScene>("res://MapToken.tscn");
	public List<MapToken> EnemyList = new List<MapToken>();
	
	private static readonly int _monsterSpriteSize = 258;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	// Generate Monster Token and stats, may need to add in variable for whether flipped
	public void MonsterGen(string colour, int siteFortifications, Vector2I localPos)
	{	
		var mapGen = GetNode<MapGen>("MapGen");
		var enemy = GameSettings.DrawMonster(Utils.ConvertStringToMonsterColour(colour));
		var monsterToken = (MapToken)monsterScene.Instantiate();
		monsterToken.MapPosition = localPos;
		monsterToken.SiteFortifications = siteFortifications;
		monsterToken.Colour = colour;
		var monsterStats = Utils.Bestiary[enemy];
		var enemySprite = monsterToken.GetNode<Sprite2D>("MapTokenControl/Sprite2D");
		var atlas = (AtlasTexture)Utils.SpriteSheets[monsterToken.Colour].Duplicate();
		atlas.Region = new Rect2(new Vector2(monsterStats.X * _monsterSpriteSize,monsterStats.Y * _monsterSpriteSize),new Vector2(_monsterSpriteSize,_monsterSpriteSize));
		enemySprite.Texture = atlas;
		enemySprite.Scale = new Vector2((float)0.25,(float)0.25);
		monsterToken.GlobalPosition = mapGen.ToGlobal(mapGen.MapToLocal(localPos));
		AddChild(monsterToken);
		EnemyList.Add(monsterToken);
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
			case "attack": attackConversion(action[1], action[2], quantity);
				break;
			case "heal": GD.Print("HEALING: ", quantity);
				break;
			case "draw": drawConversion(quantity);
				break;
			case "move": moveConversion(quantity);
				break;
		}
    }

    private void attackConversion(string element, string attackRange, int quantity) {
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

		GD.Print("Attack Index: ", index);
        // var combat = GetNode<Combat>("Combat");
        // int attackQuantity = combat.PlayerAttacks[index];

        // combat.PlayerAttacks.Add(index, attackQuantity + quantity);
        // GD.Print("PlayerAttacks: ", combat.PlayerAttacks[index]);
	}

    private void drawConversion(int quantity) {
        var deck = GetNode<Deck>("Player UI/PlayerArea/Deck");

        for (int i = 0; i < quantity; i++) {
            deck.OnDeckButtonPressed();
        }
    }

    private void moveConversion(int quantity) {
        var player = GetNode<Player>("Player");
        player.MovePoints += quantity;
        GD.Print(player.MovePoints);
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
