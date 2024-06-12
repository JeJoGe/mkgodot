using Godot;
using System.Collections.Generic;
using System.Linq;

public enum CardObjOption {
    top,
    bottom
}
public partial class CardObj : Sprite2D
{
    [Signal]
	public delegate void CardPlayedEventHandler(string cardAction);
    public int id {get; set;}
    public string cardId {get; set;}
    public string color {get; set;}
    public int xCoord {get; set;}
    public int yCoord {get; set;}
    public int copies {get; set;}
    public string topFunction {get; set;}
    public string bottomFunction {get; set;}
    public OptionButton topOptionsButton = new OptionButton();
    public OptionButton bottomOptionsButton = new OptionButton();
    public  CardObjOption currentOption {get; set;} = CardObjOption.top;
    public bool topOptionExists = true;
    public bool bottomOptionExists = true;
    private string specialOption;
    public string effects;
    public Dictionary<int, string> topOptionActions = new Dictionary<int, string>();
    public Dictionary<int, string> bottomOptionActions = new Dictionary<int, string>();

    public override void _Ready()
    {
        this.Position = new Vector2(500, 700);
        PlayButton play = new PlayButton(id);
        PowerUpButton powerUp = new PowerUpButton(id, "Using Top Action");
        AddChild(play);
        AddChild(powerUp);
        play.Pressed += onPlayButtonPressed;
        powerUp.Pressed += toggleActionPressed;
        parseFunction(topFunction, CardObjOption.top);
        parseFunction(bottomFunction, CardObjOption.bottom);
        topOptionsButton.Size = new Vector2I(50, 30);
        topOptionsButton.Scale = new Vector2I(3, 3);
        topOptionsButton.Position = new Vector2I(350, -550);

        bottomOptionsButton.Size = new Vector2I(50, 30);
        bottomOptionsButton.Scale = new Vector2I(3, 3);
        bottomOptionsButton.Position = new Vector2I(350, -550);
        topOptionsButton.Visible = topOptionExists;
        AddChild(topOptionsButton);
        bottomOptionsButton.Visible = false;
        AddChild(bottomOptionsButton);
        
        // var gamePlay = GetNode<GamePlay>(".");
        // GD.Print("GAMEPLAY:", gamePlay);
        // this.CardPlayed += cardAction => gamePlay.OnCardPlayed(cardAction);
    }


    /* Card Parsing Rules:
    * Actions:
    * 1. If there are more than one action to choose on card, separate by comma (,)
    * 2. Options to choose within one action (gainManaTokens), separated by forward slash (/)
    * 3. If there is one multiple actions that all happen on card, separated by (&)
    * 4, Special Actions are denoted by (*)
    * Quantity of Actions:
    * 5. The quantity of an action is denoted by (-) followed by a number. ie: move-4***** attack-fire-3
    */
    public void parseFunction(string cardFunction, CardObjOption position) {
        GD.Print("cardFunction", cardFunction);
       string[] function = cardFunction.Split(",");
       if (function.Count() > 1) {
            for (int i = 0; i < function.Count(); ++i) {
                string [] otherAction = function[i].Split("&");
                if (position == CardObjOption.top) {
                    topOptionsButton.AddItem(otherAction[0], i);
                    topOptionActions.Add(i, otherAction[0]);
                } else {
                    bottomOptionsButton.AddItem(otherAction[0], i);
                    bottomOptionActions.Add(i, otherAction[0]);
                }
                if (otherAction.Count() > 1) {
                   if(otherAction[1].Contains("*")) {
                    specialOption = otherAction[1];
                   } else {
                    effects = otherAction[1];
                   }
                }
            }
       } else {
            if(position == CardObjOption.top) {
                topOptionExists = false;
                topOptionActions.Add(0, function[0]);
            } else {
                bottomOptionExists = false;
                bottomOptionActions.Add(0, function[0]);

            }
       }
    }

    public void onPlayButtonPressed() {
        int selectedId = 0;
        string action = "";
        if(currentOption == CardObjOption.top) {
            selectedId = topOptionsButton.GetSelectedId();
            if(selectedId == -1) {
                action = topOptionActions[0];
            } else {
                action = topOptionActions[selectedId];
            }
        } else {
            selectedId = bottomOptionsButton.GetSelectedId();
            if (selectedId == -1) {
                action = bottomOptionActions[0];
            } else {
                action = bottomOptionActions[selectedId];
            }
        }
        GD.Print(action);
        EmitSignal(SignalName.CardPlayed, (string) action);
    }

    public void toggleActionPressed() {
        var getPowerUp = GetChild<Button>(1);
        if(currentOption == CardObjOption.top) {
            GD.Print("Using Bottom Action" + id);
            getPowerUp.Text = "Use Top Option";
            currentOption = CardObjOption.bottom;
            topOptionsButton.Visible = false;
            bottomOptionsButton.Visible = bottomOptionExists && true; 
        } else {
            getPowerUp.Text = "Use Bottom Option";
            GD.Print("Using Top Action" + id);
            currentOption = CardObjOption.top;
            bottomOptionsButton.Visible = false; 
            topOptionsButton.Visible = topOptionExists && true;
        }
    }

    public void ImageCropping(AtlasTexture atlas) {
        var frame = (AtlasTexture) atlas.Duplicate();
        frame.Region = new Rect2(new Vector2(1000 * xCoord, 1400 * yCoord), new Vector2(1000, 1400));
        this.Texture = frame;
    }

}