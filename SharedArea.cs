using Godot;
using System;

public partial class SharedArea : Node2D
{
	[Signal]
	public delegate void NewRoundEventHandler();
	[Export]
	private Label _roundMarker { get; set;}
	[Export]
	private GameplayControl gameplayControl;
	public static int Round { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Round = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnNewRoundButtonPressed() {
		Round++;
		_roundMarker.Text = "Round " + Round;
		GameSettings.NightTime = !GameSettings.NightTime;
		if (!GameSettings.NightTime)
		{
			foreach (var ruin in gameplayControl.RuinList)
			{
				
			}
		}
		EmitSignal(SignalName.NewRound);
	}
}
