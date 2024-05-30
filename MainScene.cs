using Godot;
using System;

public partial class MainScene : Node2D
{
	public MainScene() {
		GD.Print("Hello, JJG!");
	}
	private void _on_credits_pressed()
	{	
		GetTree().ChangeSceneToFile("res://Credits.tscn");
	}
	private void _on_quit_pressed()
	{
		GetTree().Quit();
	}

	private void OnPlayPressed() {
		GetTree().ChangeSceneToFile("res://CharacterChoiceScene/CharacterChoice.tscn");
	}
}






