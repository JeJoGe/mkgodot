using Godot;
using System;

public partial class RootNode : Node2D
{
	public RootNode() {
		GD.Print("Hello world!");
	}

	public override void _Ready() {
		var CharacterChoiceScene = ResourceLoader.Load<PackedScene>("res://CharacterChoiceScene/character_choice.tscn").Instantiate();
		AddChild(CharacterChoiceScene);
	}
}
