using DataAccessProject.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Migrations;
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

        private const int VALIDATION_SUCCESS = 1;
        private const int VALIDATION_FAILURE = 0;
        private const int ERROR = -1;
        private const string GAME_MODE = "normal";

        public int EndGame(int winnerTeam, string gameId)
        {
            int result = VALIDATION_FAILURE;

            using (var context = new AstralisDBEntities())
            {
                try
                {
                    context.Database.Log = Console.WriteLine;
                    var gameRow = context.Game.Find(gameId);

                    if (gameRow != null)
                    {
                        gameRow.winnerTeam = winnerTeam.ToString();
                        gameRow.gameMode = GAME_MODE;

                        result = context.SaveChanges();
                    }
                }
                catch (SqlException sqlException)
                {
                    throw sqlException;
                }
            }

            return result;
        }


        public List<GamesWonInfo> GetTopGamesWon()
        {
            List<GamesWonInfo> results = new List<GamesWonInfo>();

            using (var context = new AstralisDBEntities())
            {
                try
                {
                    var query = context.Database.SqlQuery<GamesWonInfo>(
                        @"SELECT TOP 10 p.Nickname AS Username, COUNT(*) AS GamesWonCount
                          FROM [AstralisDB].[dbo].[Plays] p
                          JOIN Game g ON p.GameId = g.GameId
                          WHERE p.Team = g.WinnerTeam
                          GROUP BY p.Nickname
                          ORDER BY GamesWonCount DESC;"
                    );

                    results = query.ToList();
                }
                catch (SqlException sqlException)
                {
                    Console.WriteLine("Database error: " + sqlException.Message);
                    throw sqlException;
                }
            }

            return results;
        }

        private const string GAME_BAN = "gameBan";

        public int BanUser (string nickname)
        {
            int result = VALIDATION_FAILURE;

            using (var context = new AstralisDBEntities())
            {
                try
                {
                    TimeSpan banDuration = TimeSpan.FromMinutes(30);
                    DateTime currentDateTime = DateTime.Now;
                    TimeSpan banExpirationTime = currentDateTime.TimeOfDay.Add(banDuration);

                    Ban ban = new Ban()
                    {
                        Nickname = nickname,
                        BanTime = banExpirationTime,
                        BanType = GAME_BAN
                    };

                    context.Ban.Add(ban);

                    result = context.SaveChanges();
                }
                catch (EntityException entityException)
                {
                    throw entityException;
                }
            }
                return result;
        }

        public int CanPlay(string nickname)
        {
            int result = VALIDATION_SUCCESS;

            using (var context = new AstralisDBEntities())
            {
                try
                {
                    DateTime currentDateTime = DateTime.Now;
                    TimeSpan currentTimeSpan = currentDateTime.TimeOfDay.Duration();
                    Ban userBan = context.Ban.FirstOrDefault(b => b.Nickname == nickname);

                    if (userBan != null && userBan.BanTime > currentTimeSpan)
                    {
                        result = VALIDATION_FAILURE;
                    }
                    else
                    {
                        result = VALIDATION_SUCCESS;
                    }
                }
                catch (EntityException entityException)
                {
                    result = ERROR;
                    throw entityException;
                }
            }

            return result;
        }

    }
}
