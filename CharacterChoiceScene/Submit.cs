using Godot;
using System;

public partial class Submit : Button
{
	private bool _disabled {get; set;} = true;
	private string label {get; set;} = "PLAY";

	// IN PROGRESS
	public Submit() {

	}

	public void setDisabled(bool value) {
		_disabled = value;
	}
	public override void _Ready() {
		var submitButton = new Button();
		submitButton.Disabled = true;
		AddChild(submitButton);

	}
}
