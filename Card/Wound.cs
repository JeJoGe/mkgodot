using System;
using Godot;

public partial class Wound : CardObj
{
    public bool hasTapFunction { get; set; }
    public string chosenPlayer { get; set; }
    public string chosenSkills { get; set; }
    public string tapFunction { get; set; }

    /*****************************************
    * HardCoded CardObj fields
    ******************************************/
    public new int id = 0;
    public new string cardId = "Wound";
    public new string color = null;
    public new int xCoord = 0;
    public new int yCoord = 0;
    public new string phase = "";
    public new string character = "";
    public new string topFunction = "";

    public override void _Ready()
    {
        this.Position = new Godot.Vector2(500, 700);

    }

    public override void parseFunction(string cardFunction, CardObjOption position = CardObjOption.top)
    {
        throw new NotImplementedException();
    }

    public override void ImageCropping()
    {
        var frame = (AtlasTexture)Utils.SpriteSheets["wound"].Duplicate();
        frame.Region = new Rect2(new Godot.Vector2(1000 * xCoord, 1400 * yCoord), new Godot.Vector2(1000, 1400));
        this.Texture = frame;
    }

    public override void onPlayButtonPressed()
    {
        throw new NotImplementedException();
    }


}