using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessProject.DataAccess
{
    public class UserAccess
    {
        private const int INT_VALIDATION_SUCCESS = 1;
        private const int INT_VALIDATION_FAILURE = 0;
        private const string USER_NICKNAME_ERROR = "Error";
        private const string USER_NOT_FOUND = "UserNotFound";

        public UserAccess() { }

        public int GetHigherGuests()
        {
            int maxGuestNumber = 0;

            using (AstralisDBEntities context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;

                List<Contracts.User> guestUsers = context.User
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
                        int result = 0;

                        if (int.TryParse(user.Nickname.Substring("Guest".Length), out int number))
                        {
                            result = number;
                        }

                        return result;
                    });
                }
                else
                {
                    maxGuestNumber = INT_VALIDATION_FAILURE;
                }

                return maxGuestNumber;
            }
        }

        public int CreateGuest(Contracts.User user)
        {
            int result = INT_VALIDATION_FAILURE;

            using (AstralisDBEntities context = new AstralisDBEntities())
            {
                if (FindUserByNickname(user.Nickname) == INT_VALIDATION_FAILURE)
                {
                    context.Database.Log = Console.WriteLine;
                    User databaseUser = new User
                    {
                        nickName = user.Nickname,
                        imageId = (short)user.ImageId,
                        mail = user.Mail
                    };

                    context.User.Add(databaseUser);

                    result = context.SaveChanges();
                    DeckAccess deckAccess = new DeckAccess();

                    deckAccess.CreateDefaultDeck(context, user.Nickname);

                    if (result > INT_VALIDATION_FAILURE)
                    {
                        result = INT_VALIDATION_SUCCESS;
                    }
                }
            }

            return result;
        }

        public int CreateUser(Contracts.User user)
        {
            int result = INT_VALIDATION_FAILURE;

            using (var context = new AstralisDBEntities())
            {
                if (FindUserByNickname(user.Nickname) == INT_VALIDATION_FAILURE)
                {
                    using (var transactionContext = context.Database.BeginTransaction())
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

                        context.User.Add(databaseUser);

                        result = context.SaveChanges();
                        DeckAccess deckAccess = new DeckAccess();

                        deckAccess.CreateDefaultDeck(context, user.Nickname);
                        transactionContext.Commit();

                        if (result > INT_VALIDATION_FAILURE)
                        {
                            result = INT_VALIDATION_SUCCESS;
                        }
                    }
                }
                else
                {
                    result = INT_VALIDATION_FAILURE;
                }
            };

            return result;
        }

        public int UpdateUser(Contracts.User user)
        {
            int result = INT_VALIDATION_FAILURE;

            if (FindUserByNickname(user.Nickname) == INT_VALIDATION_SUCCESS)
            {
                using (AstralisDBEntities context = new AstralisDBEntities())
                {
                    context.Database.Log = Console.WriteLine;
                    User databaseUser = context.User.Find(user.Nickname);

                    if (databaseUser != null)
                    {
                        databaseUser.mail = user.Mail;
                        databaseUser.imageId = (short)user.ImageId;
                        var databaseUserSession = context.UserSession.Find(databaseUser.userSessionFk);

                        if (databaseUserSession != null && !string.IsNullOrEmpty(user.Password))
                        {
                            databaseUserSession.password = user.Password;
                        }

                        result = context.SaveChanges();
                    }
                    if (result > INT_VALIDATION_FAILURE)
                    {
                        result = INT_VALIDATION_SUCCESS;
                    }
                }
            }

            return result;
        }

        public Contracts.User GetUserByNickname (string nickname)
        {
            Contracts.User foundUser = new Contracts.User();
            foundUser.Nickname = USER_NICKNAME_ERROR;

            using (AstralisDBEntities context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;
                User databaseUser = context.User.Find(nickname);

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

            return foundUser;
        }

        public int FindUserByNickname(string nickname)
        {
            int isFound = INT_VALIDATION_FAILURE;

            using (AstralisDBEntities context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;
                User databaseUser = context.User.Find(nickname);

                if (databaseUser != null)
                {
                    isFound = INT_VALIDATION_SUCCESS;
                }
            }

            return isFound;
        }

        public int ConfirmUserCredentials(string nickname, string password)
        {
            int result = INT_VALIDATION_FAILURE;

            using (AstralisDBEntities context = new AstralisDBEntities())
            {
                if (FindUserByNickname(nickname) == INT_VALIDATION_SUCCESS)
                {
                    context.Database.Log = Console.WriteLine;
                    User databaseUser = context.User.Find(nickname);
                    UserSession databaseUsersession = context.UserSession.Find(databaseUser.userSessionFk);

                    if (databaseUsersession != null && databaseUsersession.password == password)
                    {
                        result = INT_VALIDATION_SUCCESS;
                    }
                }
            }

            return result;
        }

        public int DeleteUser(string nickname)
        {
            int result = INT_VALIDATION_FAILURE;

            using (AstralisDBEntities context = new AstralisDBEntities())
            {
                if (FindUserByNickname(nickname) == INT_VALIDATION_SUCCESS)
                {
                    User userToDelete = context.User.FirstOrDefault(u => u.nickName == nickname);

                    if (userToDelete != null)
                    {
                        context.User.Remove(userToDelete);
                        result = context.SaveChanges();
                    }
                }
            }

            return result;
        }
    }
}
