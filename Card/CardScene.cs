using Godot;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public partial class CardScene : Node2D
{
    public List<string> _replaces { get; set; } = new List<string>();
    public List<CardObj> InitialDeckOfCards { get; set; } = new List<CardObj>();
    public Stack<CardObj> DeckOfCards { get; set; } = new Stack<CardObj>();
    private int InitialDeckLength { get; set; }
    public List<CardControl> discardPile { get; set; } = new List<CardControl>();
    public AtlasTexture basicCardAtlas;
    public override void _Ready()
    {
        basicCardAtlas = new AtlasTexture();

        try
        {
            var cardImage = Image.LoadFromFile("assets/basics.jpg");
            var atlasTexture = ImageTexture.CreateFromImage(cardImage);
            basicCardAtlas.Atlas = atlasTexture;
            var region = new Rect2(new Vector2(0, 0), new Vector2(atlasTexture.GetWidth(), atlasTexture.GetHeight()));
            basicCardAtlas.Region = region;
        }
        catch
        {
            GD.Print("Failed to load Basic Card Image");

        }

        StreamReader sr = new StreamReader("./Card/basicCard.json");
        string jsonObj = sr.ReadToEnd();
        var cardsObj = JsonConvert.DeserializeObject<List<CardObj>>(jsonObj);
        int index = 0;

        foreach (var card in cardsObj)
        {
            if (card.character == GameSettings.PlayerCharacterName || card.character == "basic")
            {
                card.id = index;
                InitialDeckOfCards.Add(card);
                index++;
                if (card.copies > 0)
                {
                    for (int i = 0; i < card.copies; i++)
                    {
                        CardObj newCard = new CardObj()
                        {
                            id = index,
                            cardId = card.cardId + "-1",
                            color = card.color,
                            xCoord = card.xCoord,
                            yCoord = card.yCoord,
                            copies = card.copies,
                            topFunction = card.topFunction,
                            bottomFunction = card.bottomFunction,
                            phase = card.phase,
                            character = card.character,
                            replaces = card.replaces
                        };
                        InitialDeckOfCards.Add(newCard);
                        index++;
                    }
                }
                if (card.replaces != "")
                {
                    _replaces.Add(card.replaces);
                }
            }
        }

        // Filter out cards with each name specified in _replaces
        foreach (string replace in _replaces)
        {
            CardObj cardReplace = InitialDeckOfCards.Where((card) => card.cardId == replace).First();
            InitialDeckOfCards.Remove(cardReplace);
        }
        DeckOfCards = new Stack<CardObj>(InitialDeckOfCards.Shuffle());
        InitialDeckLength = InitialDeckOfCards.Count;
    }

    // Instantiation of the atlas & CardControl happens when card is drawn;
    public CardControl DrawCard()
    {
        CardObj topCard;
        CardControl drawnCard;
        var card = DeckOfCards.Peek();

        if (card == null)
        {
            GD.Print("no more cards in deck");
            return null;
        }
        else if (card is Spell)
        {
            topCard = DeckOfCards.Pop();
            drawnCard = InstantiateSpell(topCard);

        }
        else
        {
            topCard = DeckOfCards.Pop();
            drawnCard = InstantiateBasicCard(topCard);
        }

        return drawnCard;
    }

    public CardControl AttachCardControl(CardObj card)
    {
        CardControl cardControl = new CardControl(card.cardId, card.Texture.GetSize());
        cardControl.AddChild(card);
        return cardControl;
    }

    public CardControl InstantiateSpell(CardObj card)
    {
        var SpellCardAtlas = (AtlasTexture)Utils.SpriteSheets["spell"].Duplicate();
        SpellCardAtlas.Region = new Rect2(
            new Vector2(card.xCoord * GameSettings.CardWidth, card.yCoord * GameSettings.CardLength),
            new Vector2(GameSettings.CardWidth, GameSettings.CardLength));
        card.Texture = SpellCardAtlas;
        InitialDeckOfCards.Add(card);
        CardControl spellCard = AttachCardControl(card);
        return spellCard;
    }

    public CardControl InstantiateBasicCard(CardObj card)
    {
        card.ImageCropping(basicCardAtlas);
        CardControl basicCard = AttachCardControl(card);
        return basicCard;
    }

    public void AddCardToDeck(CardObj card)
    {
        DeckOfCards.Push(card);
    }
}
