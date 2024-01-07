using DataAccessProject.Contracts;
using DataAccessProject.DataAccess;

namespace TestProject
{
    public class UserAccessTest: IDisposable
    {
        private const int INT_VALIDATION_SUCCESS = 1;
        private UserAccess userAccess = new();

        public UserAccessTest() 
        {
        
        }

        [Fact]
        public void SuccesfullyCreateUser ()
        {
            User userToAdd = new User()
            {
                Nickname = "AddUserTest",
                ImageId = 1,
                Mail = "mariom.portilla@hotmail.com",
                Password = "password"
            };

            Assert.Equal(INT_VALIDATION_SUCCESS, userAccess.CreateUser(userToAdd));
        }


        public void Dispose()
        {
            userAccess.DeleteUser("AddUserTest");
        }
    }
}