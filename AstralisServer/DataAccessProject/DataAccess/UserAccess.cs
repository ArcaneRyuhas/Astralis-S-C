using DataAccessProject.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessProject.DataAccess
{
    public class UserAccess
    {
        private const int INT_VALIDATION_SUCCESS = 1;
        private const int INT_VALIDATION_FAILURE = 0;
        private const string USER_NICKNAME_ERROR = "Error";
        private const bool BOOL_VALIDATION_SUCCESS = true;
        private const bool BOOL_VALIDATION_FAILURE = false;
        private const string USER_NOT_FOUND = "UserNotFound";

        public UserAccess() { }

        public int GetHigherGuests()
        {
            int maxGuestNumber = 0;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;

                var guestUsers = context.User
                .Where(user => user.nickName.StartsWith("Guest"))
                .Select(databaseUser => new Contracts.User
                {
                    Nickname = databaseUser.nickName,
                    Mail = databaseUser.mail,
                    ImageId = databaseUser.imageId
                })
                .ToList();

                if (guestUsers.Any())
                {
                    maxGuestNumber = guestUsers.Max(user =>
                    {
                        int number = 0;
                        if (int.TryParse(user.Nickname.Substring("Guest".Length), out number))
                        {
                            return number;
                        }
                        return number;
                    });
                }
                return maxGuestNumber;
            }
        }
        public int CreateGuest(Contracts.User user)
        {
            int result = INT_VALIDATION_SUCCESS;

            using (var context = new AstralisDBEntities())
            {
                try
                {
                    context.Database.Log = Console.WriteLine;

                    User databaseUser = new User();
                    databaseUser.nickName = user.Nickname;
                    databaseUser.imageId = (short)user.ImageId;
                    databaseUser.mail = user.Mail;

                    var newUser = context.User.Add(databaseUser);

                    result = context.SaveChanges();

                    DeckAccess deckAccess = new DeckAccess();
                    deckAccess.CreateDefaultDeck(context, user.Nickname);

                    return result;
                }
                catch (SqlException sqlException)
                {
                    throw sqlException;
                }
            }
        }

        public int CreateUser(Contracts.User user)
        {
            int result = INT_VALIDATION_SUCCESS;

            using (var context = new AstralisDBEntities())
            {
                using (var transactionContext = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.Database.Log = Console.WriteLine;

                        string cleanedPassword = user.Password.Trim();

                        var newSession = context.UserSession.Add(new UserSession() { password = cleanedPassword });

                        User databaseUser = new User();
                        databaseUser.nickName = user.Nickname;
                        databaseUser.mail = user.Mail;
                        databaseUser.imageId = (short)user.ImageId;
                        databaseUser.userSessionFk = newSession.userSessionId;
                        databaseUser.UserSession = newSession;

                        var newUser = context.User.Add(databaseUser);

                        result = context.SaveChanges();

                        DeckAccess deckAccess = new DeckAccess();
                        deckAccess.CreateDefaultDeck(context, user.Nickname);

                        transactionContext.Commit();
                    }
                    catch (SqlException sqlException)
                    {
                        transactionContext.Rollback();
                        throw sqlException;
                    }
                }
            };

            return result;
        }

        public int UpdateUser(Contracts.User user)
        {
            int result = 0;

            using (var context = new AstralisDBEntities())
            {
                try
                {
                    context.Database.Log = Console.WriteLine;
                    var databaseUser = context.User.Find(user.Nickname);

                    if (databaseUser != null)
                    {
                        databaseUser.mail = user.Mail;
                        databaseUser.imageId = (short)user.ImageId;

                        var databaseUserSession = context.UserSession.Find(databaseUser.userSessionFk);

                        if (databaseUserSession != null)
                        {
                            if (!string.IsNullOrEmpty(user.Password))
                            {
                                databaseUserSession.password = user.Password;
                            }
                        }

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

        public Contracts.User GetUserByNickname (string nickname)
        {
            Contracts.User foundUser = new Contracts.User();
            foundUser.Nickname = USER_NICKNAME_ERROR;

            using (var context = new AstralisDBEntities())
            {
                try
                {
                    context.Database.Log = Console.WriteLine;
                    var databaseUser = context.User.Find(nickname);

                    if (databaseUser != null)
                    {
                        foundUser.Nickname = databaseUser.nickName;
                        foundUser.Mail = databaseUser.mail;
                        foundUser.ImageId = databaseUser.imageId;
                    }
                    else
                    {
                        foundUser.Nickname = USER_NOT_FOUND;
                    }
                }
                catch (SqlException sqlException)
                {
                    throw sqlException;
                }
                
            }
            return foundUser;
        }

        public bool FindUserByNickname(string nickname)
        {
            bool isFound = BOOL_VALIDATION_FAILURE;

            using (var context = new AstralisDBEntities())
            {
                try
                {
                    context.Database.Log = Console.WriteLine;

                    var databaseUser = context.User.Find(nickname);

                    if (databaseUser != null)
                    {
                        isFound = BOOL_VALIDATION_SUCCESS;
                    }
                }
                catch (SqlException sqlException)
                {
                    throw sqlException;
                }
                
            }
            return isFound;
        }

        public int ConfirmUser(string nickname, string password)
        {
            int result = INT_VALIDATION_FAILURE;

            using (var context = new AstralisDBEntities())
            {
                try
                {
                    context.Database.Log = Console.WriteLine;

                    var databaseUser = context.User.Find(nickname);
                    var databaseUsersession = context.UserSession.Find(databaseUser.userSessionFk);

                    if (databaseUsersession != null && databaseUsersession.password == password)
                    {
                        result = INT_VALIDATION_SUCCESS;
                    }
                }
                catch(SqlException sqlException)
                {
                    throw sqlException;
                }
                
            }

            return result;
        }
    }
}
