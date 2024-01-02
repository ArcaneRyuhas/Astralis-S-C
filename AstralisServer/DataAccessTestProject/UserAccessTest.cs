using DataAccessProject.DataAccess;
using DataAccessProject.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DataAccessTestProject
{
    [TestClass]
    public class UserAccessTest
    {
        private const int INT_VALIDATION_SUCCESS = 1;
        private static UserAccess userAccess = new UserAccess();


        [TestMethod]
        public void SuccesfullyCreateUser()
        {
            User userToAdd = new User()
            {
                Nickname = "AddUserTest",
                ImageId = 1,
                Mail = "mariom.portilla@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(userAccess.CreateUser(userToAdd) >= INT_VALIDATION_SUCCESS);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            userAccess.DeleteUser("AddUserTest");
        }
    }
}
