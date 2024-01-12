using DataAccessProject.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessProject.DataAccess
{
    public class GameAccess
    {
        private const int INT_VALIDATION_SUCCESS = 1;
        private const int INT_VALIDATION_FAILURE = 0;
        private const string GAME_MODE = "normal";
        private const string GAME_BAN = "gameBan";

        public GameAccess() { }

        public bool CreateGame(string gameId)
        {
            bool isSuccesfullyCreated = false;

            using (var context = new AstralisDBEntities())
            {
                if (!GameIdIsRepeated(gameId))
                {
                    context.Database.Log = Console.WriteLine;

                    context.Game.Add(new Game() { gameId = gameId });

                    if (context.SaveChanges() > 0)
                    {
                        isSuccesfullyCreated = true;
                    }
                }
            };

            return isSuccesfullyCreated;
        }

        public int CreatePlaysRelation(string userNickname, string gameId, int team)
        {
            int result = INT_VALIDATION_FAILURE;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;

                context.Plays.Add(new Plays()
                {
                    nickName = userNickname,
                    gameId = gameId,
                    team = team
                });

                result = context.SaveChanges();

                if (result == INT_VALIDATION_FAILURE)
                {
                    result = INT_VALIDATION_SUCCESS;
                }

                return result;
            }
        }

        public bool GameIdIsRepeated(string gameId)
        {
            bool isRepeated = false;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;
                var databaseGameId = context.Game.Find(gameId);

                if (databaseGameId != null)
                {
                    isRepeated = true;
                }
            }

            return isRepeated;
        }

        public int EndGame(int winnerTeam, string gameId)
        {
            int result = INT_VALIDATION_FAILURE;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;
                var gameRow = context.Game.Find(gameId);

                if (gameRow != null)
                {
                    gameRow.winnerTeam = winnerTeam.ToString();
                    gameRow.gameMode = GAME_MODE;
                    result = context.SaveChanges();
                }

                if (result > INT_VALIDATION_FAILURE)
                {
                    result = INT_VALIDATION_SUCCESS;
                }
            }

            return result;
        }

        public List<GamesWonInfo> GetTopGamesWon()
        {
            List<GamesWonInfo> gameInfo = new List<GamesWonInfo>();
            using (var context = new AstralisDBEntities())
            {
                var query = context.Database.SqlQuery<GamesWonInfo>(
                        @"SELECT TOP 10 p.Nickname AS Username, COUNT(*) AS GamesWonCount
                          FROM [AstralisDB].[dbo].[Plays] p
                          JOIN Game g ON p.GameId = g.GameId
                          WHERE p.Team = g.WinnerTeam
                          GROUP BY p.Nickname
                          ORDER BY GamesWonCount DESC;"
                    );

                var results = query.ToList();

                foreach (var result in results)
                {
                    GamesWonInfo gamesWonInfo = new GamesWonInfo()
                    {
                        Username = result.Username,
                        GamesWonCount = result.GamesWonCount
                    };

                    gameInfo.Add(gamesWonInfo);
                }
            }

            return gameInfo;
        }

        public int BanUser (string nickname)
        {
            int result = INT_VALIDATION_FAILURE;
            UserAccess userAccess = new UserAccess();

            using (var context = new AstralisDBEntities())
            {
                if (userAccess.FindUserByNickname(nickname) == INT_VALIDATION_SUCCESS)
                {
                    TimeSpan banDuration = TimeSpan.FromMinutes(30);
                    DateTime currentDateTime = DateTime.Now;
                    DateTime banExpirationTime = currentDateTime.Add(banDuration);

                    Ban ban = new Ban()
                    {
                        Nickname = nickname,
                        BanTime = banExpirationTime.TimeOfDay,
                        BanType = GAME_BAN
                    };

                    context.Ban.Add(ban);

                    result = context.SaveChanges();

                    if (result > INT_VALIDATION_FAILURE)
                    {
                        result = INT_VALIDATION_SUCCESS;
                    }
                }
            }

            return result;
        }

        public int CanPlay(string nickname)
        {
            int result = INT_VALIDATION_FAILURE;

            using (var context = new AstralisDBEntities())
            {
                DateTime currentDateTime = DateTime.Now;
                TimeSpan currentTimeSpan = currentDateTime.TimeOfDay.Duration();
                Ban userBan = context.Ban.FirstOrDefault(b => b.Nickname == nickname);

                if (userBan != null && userBan.BanTime > currentTimeSpan)
                {
                    result = INT_VALIDATION_FAILURE;
                }
                else
                {
                    result = INT_VALIDATION_SUCCESS;
                }
            }

            return result;
        }
        public int CleanupGame(string gameId)
        {
            int result = INT_VALIDATION_FAILURE;

            using (var context = new AstralisDBEntities())
            {
                var existingGame = context.Game.Find(gameId);

                if (existingGame != null)
                {
                    context.Game.Remove(existingGame);
                    context.SaveChanges();

                    result = INT_VALIDATION_SUCCESS;
                }
            }

            return result;
        }

        public int RemoveBan(string nickname)
        {
            int result = INT_VALIDATION_FAILURE;

            using (var context = new AstralisDBEntities())
            {
                Ban existingBan = context.Ban.FirstOrDefault(b => b.Nickname == nickname);

                if (existingBan != null)
                {
                    context.Ban.Remove(existingBan);

                    result = context.SaveChanges();

                    if (result > INT_VALIDATION_FAILURE)
                    {
                        result = INT_VALIDATION_SUCCESS;
                    }
                }
            }

            return result;
        }
    }
}
