using System;
using Godot;

public partial class Spell : CardObj
{
    public string version { get; set; }

    public new void parseFunction(string cardFunction, CardObjOption position)
    {
        throw new NotImplementedException();
    }

    public new void onPlayButtonPressed()
    {
        throw new NotImplementedException();
    }

    public new void ImageCropping()
    {
        var SpellCardAtlas = (AtlasTexture)Utils.SpriteSheets["spell"].Duplicate();
        SpellCardAtlas.Region = new Rect2(
            new Vector2(this.xCoord * GameSettings.CardWidth, this.yCoord * GameSettings.CardLength),
            new Vector2(GameSettings.CardWidth, GameSettings.CardLength));
        this.Texture = SpellCardAtlas;
    }
}