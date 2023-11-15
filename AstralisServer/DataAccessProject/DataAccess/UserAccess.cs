﻿using DataAccessProject.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        private const bool BOOL_VALIDATION_SUCCESS = true;
        private const bool BOOL_VALIDATION_FAILURE = false;
        private const string USER_NOT_FOUND = "UserNotFound";

        public UserAccess() { }

        public int CreateUser(Contracts.User user)
        {
            int result = INT_VALIDATION_SUCCESS;

            using (var context = new AstralisDBEntities())
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

            };

            return result;
        }

        public int UpdateUser(Contracts.User user) 
        {
            int result = 0;

            using (var context = new AstralisDBEntities())
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

            return result;
        }

        public Contracts.User GetUserByNickname (string nickname)
        {
            Contracts.User foundUser = new Contracts.User();

            using (var context = new AstralisDBEntities())
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
            return foundUser;
        }

        public bool FindUserByNickname(string nickname)
        {
            bool isFound = BOOL_VALIDATION_FAILURE;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;

                var databaseUser = context.User.Find(nickname);

                if (databaseUser != null)
                {
                    isFound = BOOL_VALIDATION_SUCCESS;
                }
            }

            return isFound;
        }

        public int ConfirmUser(string nickname, string password)
        {
            int result = 0;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;

                var databaseUser = context.User.Find(nickname);
                var databaseUsersession = context.UserSession.Find(databaseUser.userSessionFk);

                if (databaseUsersession != null && databaseUsersession.password == password)
                {
                    result = INT_VALIDATION_SUCCESS;
                }
                else
                {
                    result = INT_VALIDATION_FAILURE;
                }
            }

            return result;
        }
    }
}