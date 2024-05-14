using Godot;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;

public enum CardObjOption {
    top,
    bottom
}
public partial class CardObj : Sprite2D
{
    public int id {get; set;}
    public string cardId {get; set;}
    public string color {get; set;}
    public int xCoord {get; set;}
    public int yCoord {get; set;}
    public int copies {get; set;}
    public string topFunction {get; set;}
    public string bottomFunction {get; set;}
    public OptionButton topOptions = new OptionButton();
    public OptionButton bottomOptions = new OptionButton();
    public  CardObjOption currentOption {get; set;}

    [Signal]
    public delegate void ItemSelectedEventHandler(int index);

    public override void _Ready()
    {
        base._Ready();
        PlayButton play = new PlayButton(id);
        PowerUpButton powerUp = new PowerUpButton(id, "Using Top Action");
        AddChild(play);
        AddChild(powerUp);
        play.Pressed += onPlayButtonPressed;
        powerUp.Pressed += onPowerUpPressed;
        parseFunction(topFunction, CardObjOption.top);
        parseFunction(bottomFunction, CardObjOption.bottom);
        topOptions.Size = new Vector2I(50, 30);
        topOptions.Scale = new Vector2I(3, 3);
        topOptions.Position = new Vector2I(350, -550);

        bottomOptions.Size = new Vector2I(50, 30);
        bottomOptions.Scale = new Vector2I(3, 3);
        bottomOptions.Position = new Vector2I(350, -550);
        AddChild(topOptions);
        bottomOptions.Visible = false;
        AddChild(bottomOptions);
        currentOption = CardObjOption.top;
    }

    public void parseFunction(string cardFunction, CardObjOption position) {
       string[] function = cardFunction.Split(',');
       if (function.Count() > 1) {
            foreach (var option in function) {
                if (position == CardObjOption.top) {
                    topOptions.AddItem(option);
                } else {
                    bottomOptions.AddItem(option);
                }
            }
       } 
    }

    public void onTopOptionsSelected(int index) {

    }

    public void onPlayButtonPressed() {
        GD.Print("Play " + id);
    }

    public void onPowerUpPressed() {
        var getPowerUp = GetChild<Button>(1);
        if(currentOption == CardObjOption.top) {
            GD.Print("Using Bottom Action" + id);
            getPowerUp.Text = "Use Top Option";
            currentOption = CardObjOption.bottom;
            topOptions.Visible = false;
            bottomOptions.Visible = true; 
        } else {
            getPowerUp.Text = "Use Bottom Option";
            GD.Print("Using Top Action" + id);
            currentOption = CardObjOption.top;
            bottomOptions.Visible = false; 
            topOptions.Visible = true;
        }
    }

    public void ImageCropping(AtlasTexture atlas) {
        var frame = (AtlasTexture) atlas.Duplicate();
        frame.Region = new Rect2(new Vector2(1000 * xCoord, 1400 * yCoord), new Vector2(1000, 1400));
        this.Texture = frame;
        this.Scale = new Godot.Vector2((float) 0.25, (float) 0.25);
    }
}
