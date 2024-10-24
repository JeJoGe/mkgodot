using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
public enum CardObjOption
{
    top,
    bottom
}
public partial class CardObj : Sprite2D
{
    [Signal]
    public delegate void CardPlayedEventHandler(Godot.Collections.Array<string> basicAction, Godot.Collections.Array<string> specialAction);
    public int id { get; set; }
    public string cardId { get; set; }
    public string color { get; set; }
    public int xCoord { get; set; }
    public int yCoord { get; set; }
    public int copies { get; set; }
    public string phase {get; set;}
    public string character {get; set;}
    public string replaces { get; set;} = "";
    public string topFunction { get; set; }
    public string bottomFunction { get; set; }    
    public OptionButton topOptionsButton = new OptionButton();
    public OptionButton bottomOptionsButton = new OptionButton();
    public CardObjOption currentOption { get; set; } = CardObjOption.top;
    public bool topOptionExists = true;
    public bool bottomOptionExists = true;
    private Dictionary<int, Godot.Collections.Array<string>> topSpecialOptionsActions = new Dictionary<int, Godot.Collections.Array<string>>();
    private Dictionary<int, Godot.Collections.Array<string>> bottomSpecialOptionsActions = new Dictionary<int, Godot.Collections.Array<string>>();
    public Dictionary<int, Godot.Collections.Array<string>> topOptionActions = new Dictionary<int, Godot.Collections.Array<string>>();
    public Dictionary<int, Godot.Collections.Array<string>> bottomOptionActions = new Dictionary<int, Godot.Collections.Array<string>>();
    public override void _Ready()
    {
        this.Position = new Godot.Vector2(500, 700);
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
    public void parseFunction(string cardFunction, CardObjOption position)
    {
        string[] function = cardFunction.Split(",");
        if (function.Count() > 1)
        {
            for (int i = 0; i < function.Count(); ++i)
            {
                if (position == CardObjOption.top)
                {
                    topOptionsButton.AddItem(function[i], i);
                }
                else
                {
                    bottomOptionsButton.AddItem(function[i], i);
                }
                string[] otherAction = function[i].Split("&");
                for (int x = 0; x < otherAction.Count(); ++x)
                {
                    if (position == CardObjOption.top)
                    {
                        if (otherAction[x].Contains("*"))
                        {
                            ActionAdd(topSpecialOptionsActions, i, otherAction[x]);
                        } else { 
                            ActionAdd(topOptionActions, i, otherAction[x]);
                        }
                    }
                    else
                    {
                        if (otherAction[x].Contains("*"))
                        {
                            ActionAdd(bottomSpecialOptionsActions, i, otherAction[x]);
                        } else { 
                            ActionAdd(bottomOptionActions, i, otherAction[x]);
                        }
                    }
                }
            }
        }
        else
        {
            if (position == CardObjOption.top)
            {
                topOptionExists = false;
                if (function[0].Contains("*"))
                {
                    ActionAdd(topSpecialOptionsActions, 0, function[0]);
                } else 
                { 
                    NewActionAdd(topOptionActions, 0, function[0]);
                }
            }
            else
            {
                bottomOptionExists = false;
                if (function[0].Contains("*"))
                {
                    ActionAdd(bottomSpecialOptionsActions, 0, function[0]);
                } else 
                { 
                    NewActionAdd(bottomOptionActions, 0, function[0]);
                }
            }
        }
    }

    public void NewActionAdd(Dictionary<int, Godot.Collections.Array<string>> actionList, int index, string action)
    {
        Godot.Collections.Array<string> currList = new Godot.Collections.Array<string>() {
                action
            };
        actionList.Add(index, currList);
    }

    public void ActionAdd(Dictionary<int, Godot.Collections.Array<string>> actionList, int index, string action)
    {
        bool doesIndexExist = actionList.ContainsKey(index);
        if (doesIndexExist)
        {
            actionList[index].Add(action);
        }
        else
        {
            NewActionAdd(actionList, index, action);
        }
    }

    public Godot.Collections.Array<string> getSpecificAction(Dictionary<int, Godot.Collections.Array<string>> actionList, int index) {
        bool doesIndexExist = actionList.ContainsKey(index);
        if(doesIndexExist) {
            return actionList[index];
        } else return null;
    }

    public void onPlayButtonPressed()
    {
        int selectedId = 0;
        Godot.Collections.Array<string> basicAction = [];
        Godot.Collections.Array<string> specialAction = [];
        if (currentOption == CardObjOption.top)
        {
            selectedId = topOptionsButton.GetSelectedId();
            
            if (selectedId == -1)
            {
                selectedId = 0;
            }
            basicAction = getSpecificAction(topOptionActions, selectedId);
            specialAction = getSpecificAction(topSpecialOptionsActions, selectedId);

        }
        else
        {
            selectedId = bottomOptionsButton.GetSelectedId();
            if (selectedId == -1)
            {
                selectedId = 0;
            }
            basicAction = getSpecificAction(bottomOptionActions, selectedId);
            specialAction = getSpecificAction(bottomSpecialOptionsActions, selectedId);

        }
        EmitSignal(SignalName.CardPlayed, basicAction, specialAction);
    }

    public void toggleActionPressed()
    {
        var getPowerUp = GetChild<Button>(1);
        if (currentOption == CardObjOption.top)
        {
            getPowerUp.Text = "Use Top Option";
            currentOption = CardObjOption.bottom;
            topOptionsButton.Visible = false;
            bottomOptionsButton.Visible = bottomOptionExists && true;
        }
        else
        {
            getPowerUp.Text = "Use Bottom Option";
            currentOption = CardObjOption.top;
            bottomOptionsButton.Visible = false;
            topOptionsButton.Visible = topOptionExists && true;
        }
    }

    public void ImageCropping(AtlasTexture atlas)
    {
        var frame = (AtlasTexture) atlas.Duplicate();
        frame.Region = new Rect2(new Godot.Vector2(1000 * xCoord, 1400 * yCoord), new Godot.Vector2(1000, 1400));
        this.Texture = frame;
    }

    public void ManipulateButtons(bool visible)
    {
        var playButton = GetChild<Button>(0);
        playButton.Visible = visible;
        playButton.Disabled = !visible;
    }
}