using Godot;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

public partial class CardScene : Node2D
{
    private List<CardObj> DeckOfCards {get; set;} = new List<CardObj>();
    private int InitialDeckLength {get; set;}
    private Random rand = new Random();

    public override void _Ready() {
        base._Ready();
        StreamReader sr = new StreamReader("./Card/basicCard.json");
        string jsonObj = sr.ReadToEnd();
        var cardsObj = JsonConvert.DeserializeObject<List<CardObj>>(jsonObj);


        foreach (var card in cardsObj) {
                Guid _id = Guid.NewGuid();
                card.id = _id;
                DeckOfCards.Add(card); 
            if (card.copies > 0) {
                for (int i = 0; i < card.copies; i++) {
                    Guid _id1 = Guid.NewGuid();
                    CardObj newCard = new CardObj() {
                        id = _id1,
                        cardId = card.cardId,
                        color = card.color,
                        xCoord = card.xCoord,
                        copies = card.copies,
                        topFunction = card.topFunction,
                        bottomFunction = card.bottomFunction
                    };
                    DeckOfCards.Add(newCard); 
                }
            }
        };
        InitialDeckLength = DeckOfCards.Count;
    }


    public void DrawCard()
    {
        int indexToBeDrawn = rand.Next(0, DeckOfCards.Count);
        CardObj card = DeckOfCards[indexToBeDrawn];
        int posMultiplier = InitialDeckLength - DeckOfCards.Count;
        int xPos = -300 - (100 * posMultiplier);
        card.Position = new Vector2I(xPos, -500);
        AddChild(card);
        DeckOfCards.RemoveAt(indexToBeDrawn);
    }

}
