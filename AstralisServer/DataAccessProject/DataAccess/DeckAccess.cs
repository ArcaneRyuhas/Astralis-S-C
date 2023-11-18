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

        public DeckAccess() { }

        public int CreateDefaultDeck(AstralisDBEntities context, string nickname)
        {
            int result = 0;

            Deck deck = new Deck();
            deck.Card = DEFAULT_DECK;

            try
            {
                context.Deck.Add(deck);
                result = context.SaveChanges();

                CreateRelationUserDeck(context, deck.DeckId, nickname);
            }
            catch (EntityException entityException)
            {
                throw entityException;
            }        

            return result;
        }

        private int CreateRelationUserDeck(AstralisDBEntities context, int deckId, string nickname) 
        {
            int result = 0;

            try
            {
                UserDeck userDeck = new UserDeck();
                userDeck.Nickname = nickname;
                userDeck.DeckId = deckId;

                context.SaveChanges();
            }
            catch (EntityException entityException)
            {
                throw entityException;
            }

            return result;
        }

        /*
        public List<int> GetDeckByNicknameAndDeckId(int deckId, string nickname)
        {

            List<int> numbers = input.Split(',')
                                     .Select(int.Parse)
                                     .ToList();
        }
        */
    }
