using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessProject.DataAccess
{
    public class DeckAccess
    {
        private const string DEFAULT_DECK = "1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30";
        private const int INT_VALIDATION_SUCCESS = 1;
        private const int INT_VALIDATION_FAILURE = 0;

        public DeckAccess() { }

        public int CreateDefaultDeck(AstralisDBEntities context, string nickname)
        {
            int result = INT_VALIDATION_FAILURE;
            Deck deck = new Deck
            {
                Card = DEFAULT_DECK
            };

            context.Deck.Add(deck);
            result = context.SaveChanges();

            result += CreateRelationUserDeck(context, deck.DeckId, nickname);

            if (result > INT_VALIDATION_FAILURE)
            {
                result = INT_VALIDATION_SUCCESS;
            }

            return result;
        }

        private int CreateRelationUserDeck(AstralisDBEntities context, int deckId, string nickname)
        {
            int result = INT_VALIDATION_FAILURE;

            UserDeck userDeck = new UserDeck();
            userDeck.Nickname = nickname;
            userDeck.DeckId = deckId;

            context.UserDeck.Add(userDeck);

            result = context.SaveChanges();

            if (result > INT_VALIDATION_FAILURE)
            {
                result = INT_VALIDATION_SUCCESS;
            }

            return result;
        }

        
        public List<int> GetDeckByNickname(string nickname)
        {
            List<int> cardList;

            using (var context = new AstralisDBEntities())
            {
                UserDeck userDeck = new UserDeck();

                userDeck.DeckId = context.UserDeck.FirstOrDefault(ud => ud.Nickname == nickname).DeckId;

                Deck deck = context.Deck.Find(userDeck.DeckId);
                cardList = deck.Card.Split(',').Select(int.Parse).ToList();
            }

            return cardList;
        }
        
    }
}
