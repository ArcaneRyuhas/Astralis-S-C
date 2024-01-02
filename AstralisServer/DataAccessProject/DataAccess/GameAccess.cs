using DataAccessProject.Contracts;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessProject.DataAccess
{
    public class GameAccess
    {
        public GameAccess() { }

        public bool CreateGame(string gameId)
        {
            bool isSuccesfullyCreated = false;

            using (var context = new AstralisDBEntities())
            {
                try
                {
                    context.Database.Log = Console.WriteLine;
                    var newSession = context.Game.Add(new Game() { gameId = gameId });
                    if (context.SaveChanges() > 0)
                    {
                        isSuccesfullyCreated = true;
                    }
                }
                catch (SqlException sqlException)
                {
                    throw sqlException;
                }
            };

            return isSuccesfullyCreated;
        }

        public int CreatePlaysRelation(string userNickname, string gameId, int team)
        {
            using (var context = new AstralisDBEntities())
            {
                try
                {
                    context.Database.Log = Console.WriteLine;

                    var newSession = context.Plays.Add(new Plays()
                    {
                        nickName = userNickname,
                        gameId = gameId,
                        team = team
                    });
                }
                catch (SqlException sqlException)
                {
                    throw sqlException;
                }
                return context.SaveChanges();
            }
        }

        public bool GameIdIsRepeated(string gameId)
        {
            bool isRepeated = false;

            using (var context = new AstralisDBEntities())
            {
                try
                {
                    context.Database.Log = Console.WriteLine;

                    var databaseGameId = context.User.Find(gameId);

                    if (databaseGameId != null)
                    {
                        isRepeated = true;
                    }
                }
                catch (SqlException sqlException)
                {
                    throw sqlException;
                }
            }
            return isRepeated;
        }
    }
}
