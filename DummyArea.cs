using Godot;
using System;

public partial class DummyArea : Node2D
{
	[Signal]
	public delegate void  EndTurnEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnCurrentTurn(long characterId) {
		if (characterId == GameSettings.DummyCharacter) {
			// perform dummy turn
			GD.Print("Dummy takes its turn");
			EmitSignal(SignalName.EndTurn);
		}
	}
}
