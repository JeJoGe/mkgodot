using Godot;
using System;

public partial class TurnOrderTracker : Node2D
{
	private Vector2[] _positions;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// create sprite for dummy
		var dummySprite = new Sprite2D();
		AddChild(dummySprite);
		var dummyPosition = new Vector2(0,0);
		dummySprite.Position = dummyPosition;
		dummySprite.Texture = (Texture2D)GD.Load("res://assets/TurnOrder/Token Turn Order Front Tovak.jpg");
		dummySprite.Scale = new Vector2((float)0.2,(float)0.2);
		dummySprite.Name = "DummyToken";
		// create sprites for tokens
		var offset = 45;
		for (int i = 0; i < GamePlay.NumPlayers ;i++)
		{
			var tokenSprite = new Sprite2D();
			AddChild(tokenSprite);
			tokenSprite.Name = "PlayerTurnToken";
			var y = (offset * (i + 1)) + 20;
			var tokenPosition = new Vector2(0,y);
			tokenSprite.Position = tokenPosition;
			tokenSprite.Texture = GD.Load<Texture2D>("res://assets/TurnOrder/Token Turn Order Front Arythea.jpg");
			tokenSprite.Scale = new Vector2((float)0.2,(float)0.2);
			tokenSprite.Name = "PlayerToken";
		}
		GD.Print(GetNode<Sprite2D>("DummyToken").Position);
		GD.Print(GetNode<Sprite2D>("PlayerToken").Position);


	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnTurnOrder(int playerTactic, int dummyTactic) {
		GD.Print("Player tactic: "+playerTactic+" Dummy tactic: "+dummyTactic);
	}
}
