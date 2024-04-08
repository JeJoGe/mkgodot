using Godot;
using System;

public partial class MainScene : Node2D
{
	public MainScene() {
		GD.Print("Hello, world!");
	}
	private void _on_credits_pressed()
	{	
		GetTree().ChangeSceneToFile("res://Credits.tscn");
		GD.Print("Hello, world!");
	}
	private void _on_quit_pressed()
	{
		GetTree().Quit();
	}
}






