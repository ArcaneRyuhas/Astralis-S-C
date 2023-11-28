using System.Collections.Generic;
using System.Linq;

namespace Astralis.Views.Game.GameLogic
{
    public class CardManager
    {
        private const string TANK = "Tank";
        private const string WARRIOR = "Warrior";
        private const string MAGE = "Mage";
        private const string ERROR = "error";
        private const string NO_CLASS = "empty";
        private const int ERROR_CARD_ID = 0;

        private static CardManager instance;
        private readonly Dictionary<int, ICardPrototype> cardLibrary;
        

        public static CardManager Instance()
        {
            if (instance == null)
            {
                instance = new CardManager();
            }

            return instance;
        }

        private CardManager()
        {
            cardLibrary = new Dictionary<int, ICardPrototype>();

            AddErrorCard();
            PopulateLibrary();
        }

        private void AddErrorCard()
        {
            cardLibrary.Add(ERROR_CARD_ID, new Card(1, 1, 1, ERROR));
        }

        private void PopulateLibrary()
        {
            cardLibrary.Add(-1, new Card(0, 0, 0, NO_CLASS));
            cardLibrary.Add(1, new Card(1, 2, 2, TANK));
            cardLibrary.Add(2, new Card(1, 1, 3, TANK));
            cardLibrary.Add(3, new Card(1, 3, 1, WARRIOR));
            cardLibrary.Add(4, new Card(1, 1, 2, MAGE));
            cardLibrary.Add(5, new Card(2, 2, 4, TANK));
            cardLibrary.Add(6, new Card(2, 3, 2, WARRIOR));
            cardLibrary.Add(7, new Card(2, 3, 2, TANK));
            cardLibrary.Add(8, new Card(2, 2, 4, MAGE));
            cardLibrary.Add(9, new Card(3, 4, 4, TANK));
            cardLibrary.Add(10, new Card(3, 2, 5, MAGE));
            cardLibrary.Add(11, new Card(3, 3, 3, WARRIOR));
            cardLibrary.Add(12, new Card(4, 5, 1, WARRIOR));
            cardLibrary.Add(13, new Card(4, 1, 8, TANK));
            cardLibrary.Add(14, new Card(4, 2, 6, MAGE));
            cardLibrary.Add(15, new Card(5, 5, 5, TANK));
            cardLibrary.Add(16, new Card(5, 6, 2, WARRIOR));
            cardLibrary.Add(17, new Card(5, 4, 5, MAGE));
            cardLibrary.Add(18, new Card(6, 6, 6, TANK));
            cardLibrary.Add(19, new Card(6, 4, 10, MAGE));
            cardLibrary.Add(20, new Card(6, 6, 6, WARRIOR));
            cardLibrary.Add(21, new Card(7, 5, 10, TANK));
            cardLibrary.Add(22, new Card(7, 8, 6, WARRIOR));
            cardLibrary.Add(23, new Card(7, 6, 10 , MAGE));
            cardLibrary.Add(24, new Card(8, 10, 10, WARRIOR));
            cardLibrary.Add(25, new Card(8, 8, 12, TANK));
            cardLibrary.Add(26, new Card(8, 7, 12, MAGE));
            cardLibrary.Add(27, new Card(9, 12, 10, WARRIOR));
            cardLibrary.Add(28, new Card(9, 10, 12, MAGE));
            cardLibrary.Add(29, new Card(10, 15, 15, TANK));
            cardLibrary.Add(30, new Card(10, 12, 12, MAGE));
        }


        public int GetCardId(Card card)
        {
            int cardId;

            cardId = FindKeyByValue(card);

            return cardId;
        }

        private int FindKeyByValue(Card value)
        {
            int cardId = ERROR_CARD_ID;

            if (cardLibrary.ContainsValue(value))
            {
                cardId = cardLibrary.FirstOrDefault(x => x.Value.Equals(value)).Key;
            }

            return cardId;
        }

        public Card GetCard(int cardId)
        {
            Card card; 

            if (cardLibrary.ContainsKey(cardId))
            {
                card = cardLibrary[cardId].Clone();
            }
            else
            {
                card = cardLibrary[ERROR_CARD_ID].Clone();
            }

            return card;
        }
    }
}
