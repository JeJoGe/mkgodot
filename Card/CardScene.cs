using Godot;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Data;
using System.Text.RegularExpressions;

public partial class CardScene : Node2D
{
    public Stack<CardObj> InitialDeckOfCards {get; set;} = new Stack<CardObj>();
    public List<CardControl> RecordDeckOfCards {get; set;} = new List<CardControl>();
    public Stack<CardControl> DeckOfCards {get; set;} = new Stack<CardControl>();
    private int InitialDeckLength {get; set;}
    public List<CardControl> discardPile {get; set;} = new List<CardControl>();
    public override void _Ready() {
        var atlas = new AtlasTexture();

    try {
        var cardImage = Image.LoadFromFile("assets/basics.jpg");
        var atlasTexture = ImageTexture.CreateFromImage(cardImage);
        atlas.Atlas = atlasTexture;
        var region = new Rect2(new Vector2(0, 0), new Vector2(atlasTexture.GetWidth(), atlasTexture.GetHeight()));
        atlas.Region = region;
    } catch {
            GD.Print("Failed to load Basic Card Image");
    
    }

        StreamReader sr = new StreamReader("./Card/basicCard.json");
        string jsonObj = sr.ReadToEnd();
        var cardsObj = JsonConvert.DeserializeObject<List<CardObj>>(jsonObj);
        int index = 0;
        foreach (var card in cardsObj) {
                card.id = index;
                card.ImageCropping(atlas);
                InitialDeckOfCards.Push(card);
                index++;
            if (card.copies > 0) {
                for (int i = 0; i < card.copies; i++) {
                    CardObj newCard = new CardObj() {
                        id = index,
                        cardId = card.cardId + "-1",
                        color = card.color,
                        xCoord = card.xCoord,
                        yCoord = card.yCoord,
                        copies = card.copies,
                        topFunction = card.topFunction,
                        bottomFunction = card.bottomFunction
                    };
                    newCard.ImageCropping(atlas);
                    InitialDeckOfCards.Push(newCard);
                    index++;
                }
            }
        }
        foreach (var card in InitialDeckOfCards) {
            CardControl cardControl = new CardControl(card.cardId, card.Texture.GetSize());
            cardControl.AddChild(card);
            RecordDeckOfCards.Add(cardControl);
        }
        DeckOfCards = new Stack<CardControl>(RecordDeckOfCards.Shuffle());
        InitialDeckLength = InitialDeckOfCards.Count;        
    }


    public CardControl DrawCard()
    {
        CardControl card = DeckOfCards.Pop();
        return card;
    }

}
