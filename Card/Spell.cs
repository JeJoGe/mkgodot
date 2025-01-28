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
        Godot.Collections.Array<string> basicAction = [];
        Godot.Collections.Array<string> specialAction = [];
        Godot.Collections.Array<string> manaCosts = [];
        if (currentOption == CardObjOption.top)
        {
            manaCosts.Add(colour);
        }
        else
        {
            manaCosts.Add(colour);
            manaCosts.Add("black");
        }
        EmitSignal(SignalName.CardPlayed, basicAction, specialAction, manaCosts);
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