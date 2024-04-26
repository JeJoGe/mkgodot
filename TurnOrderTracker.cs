using Godot;
using System;
using System.Collections.Generic;

public partial class TurnOrderTracker : Node2D
{
	[Signal]
	public delegate void CurrentTurnEventHandler(int characterId);
	private Vector2[] _positions = new Vector2[GameSettings.NumPlayers + 1];
	private int _offset = 45;
	private int _indent = 20;
	private Vector2 _scale = new Vector2((float)0.2, (float)0.2);
	private Dictionary<int,string> _tokenImages = new Dictionary<int, string>{
		{0, "res://assets/TurnOrder/Token Turn Order Front Tovak.jpg"},
		{1, "res://assets/TurnOrder/Token Turn Order Front Arythea.jpg"}
	};
	private Dictionary<int,int> _tracker = new Dictionary<int, int>();
	private int _currentTurn = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// create sprite for dummy
		var dummySprite = new Sprite2D();
		AddChild(dummySprite);
		var dummyPosition = new Vector2(0, 0);
		dummySprite.Position = dummyPosition;
		_positions[0] = dummyPosition;
		dummySprite.Texture = (Texture2D)GD.Load(_tokenImages[GameSettings.DummyCharacter]);
		dummySprite.Scale = _scale;
		dummySprite.Name = "DummyTurnToken";
		// create sprites for tokens
		for (int i = 0; i < GameSettings.NumPlayers; i++)
		{
			var tokenSprite = new Sprite2D();
			AddChild(tokenSprite);
			tokenSprite.Name = "PlayerTurnToken";
			var y = (_offset * (i + 1)) + 20;
			var tokenPosition = new Vector2(0, y);
			tokenSprite.Position = tokenPosition;
			_positions[i + 1] = tokenPosition;
			tokenSprite.Texture = GD.Load<Texture2D>(_tokenImages[GameSettings.PlayerCharacter]);
			tokenSprite.Scale = _scale;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Update turn order after tactic selection
	private void OnTurnOrder(int playerTactic, int dummyTactic)
	{
		GD.Print("Player tactic: " + playerTactic + " Dummy tactic: " + dummyTactic);
		int playerTurn, dummyTurn;
		if (playerTactic > dummyTactic)
		{
			GetNode<Sprite2D>("PlayerTurnToken").Position = _positions[1];
			GetNode<Sprite2D>("DummyTurnToken").Position = new Vector2(_positions[0].X + _indent, _positions[0].Y);
			dummyTurn = 0;
			playerTurn = 1;
		}
		else
		{
			GetNode<Sprite2D>("PlayerTurnToken").Position = new Vector2(_positions[0].X + _indent, _positions[0].Y);
			GetNode<Sprite2D>("DummyTurnToken").Position = _positions[1];
			playerTurn = 0;
			dummyTurn = 1;
		}
		_tracker.Clear();
		_tracker.Add(dummyTurn,GameSettings.DummyCharacter);
		_tracker.Add(playerTurn,GameSettings.PlayerCharacter);
		_currentTurn = 0;
		EmitSignal(SignalName.CurrentTurn,_tracker[_currentTurn]);
	}

	private void OnEndTurnPressed() {
		if (_currentTurn == GameSettings.NumPlayers) {
			_currentTurn = 0;
		} else {
			_currentTurn++;
		}
		UpdateTrackerPositions();
		EmitSignal(SignalName.CurrentTurn,_tracker[_currentTurn]);
	}

	private void UpdateTrackerPositions() {
		var playerY = GetNode<Sprite2D>("PlayerTurnToken").Position.Y;
		var dummyY = GetNode<Sprite2D>("DummyTurnToken").Position.Y;
		if (_tracker[_currentTurn] == GameSettings.PlayerCharacter) {
			GetNode<Sprite2D>("PlayerTurnToken").Position = new Vector2(_indent, playerY);
			GetNode<Sprite2D>("DummyTurnToken").Position = new Vector2(0, dummyY);
		} else {
			GetNode<Sprite2D>("PlayerTurnToken").Position = new Vector2(0, playerY);
			GetNode<Sprite2D>("DummyTurnToken").Position = new Vector2(_indent, dummyY);
		}
	}
}
