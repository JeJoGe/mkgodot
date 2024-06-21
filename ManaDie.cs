using Godot;
using System;
using System.Collections.Generic;

public partial class ManaDie : Area2D
{
    private int _count = 0;
    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            GetNode<Label>("DieLabel").Text = _count.ToString();
        }
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void OnInputEvent(Node _viewport, InputEvent inputEvent, long _idx, long colour) {
        if (typeof(InputEventMouseButton) == inputEvent.GetType())
        {
            var mouseEvent = (InputEventMouseButton)inputEvent;
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsPressed()) {
                GD.Print(((Source.Colour)(int)colour).ToString() + " die clicked");          
            }
        }
    }
}
