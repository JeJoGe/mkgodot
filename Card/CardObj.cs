using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Numerics;
using System.Security;

public partial class CardObj : Sprite2D
{
    public Guid id {get; set;}
    public string cardId {get; set;}
    public string color {get; set;}
    public int xCoord {get; set;}
    public int yCoord {get; set;}
    public int copies {get; set;}
    public ExpandoObject topFunction {get; set;}
    public ExpandoObject bottomFunction {get; set;}

    public override void _Ready()
    {
        base._Ready();
        var image = Image.LoadFromFile("assets/basics.jpg");
        var texture = ImageTexture.CreateFromImage(image);
        this.Texture = texture; 
        this.Hframes = 7;
        this.Vframes = 4;
        this.FrameCoords = new Vector2I(xCoord, yCoord);
        this.Scale = new Godot.Vector2((float) 0.5, (float) 0.5);
    }

    public void onPlayButtonPressed() {

    }

    public void onPowerUpPressed() {
        
    }
}
