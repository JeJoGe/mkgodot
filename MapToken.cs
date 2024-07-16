using Godot;
using System;

public partial class MapToken : Node2D
{
	[Export]
	ColorRect colorRect;
	public string Colour { get; set; }
	public Vector2I MapPosition { get; set; }
	public int SiteFortifications { get; set; }
	public int TokenId { get; set; }
	private Color _oldPosColour = Colors.Black;
	public Color OldPosColour { get; set;}
	private Color _posColour = Colors.Black;
	public Color PosColour { 
		get => _posColour; 
		set
		{
			if (OldPosColour != PosColour)
			{
				OldPosColour = PosColour;
			}
			_posColour = value;
			colorRect.Color = _posColour;
			if (_posColour == Colors.Black)
			{	
				colorRect.Visible = false;
			}
			else if (colorRect.Visible == false)
			{
				colorRect.Visible = true;
			}
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		colorRect.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
