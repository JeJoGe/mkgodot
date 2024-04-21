using Godot;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

public partial class CardScene : Node2D
{
    private List<CardObj> deckOfCards {get; set;} = new List<CardObj>();
    private int initialDeckLength {get; set;}

    public override void _Ready() {
        base._Ready();
        StreamReader sr = new StreamReader("./Card/basicCard.json");
        string jsonObj = sr.ReadToEnd();
        var cardsObj = JsonConvert.DeserializeObject<List<CardObj>>(jsonObj);

        foreach (var card in cardsObj) {
            Guid _id = Guid.NewGuid();
            card.id = _id;
            GD.Print(card.cardId);
            deckOfCards.Add(card); 
        };
        initialDeckLength = deckOfCards.Count;
    }


    public void drawCard()
    {
        Random rand = new Random();
        int indexToBeDrawn = rand.Next(0, deckOfCards.Count);
        GD.Print(deckOfCards.Count);
        GD.Print(indexToBeDrawn);
        CardObj card = deckOfCards[indexToBeDrawn];
        GD.Print(card.cardId);
        int posMultiplier = initialDeckLength - deckOfCards.Count;
        int xPos = -300 - (100 * posMultiplier);
        card.Position = new Vector2I(xPos, -500);
        AddChild(card);
        deckOfCards.RemoveAt(indexToBeDrawn);
    }

}
