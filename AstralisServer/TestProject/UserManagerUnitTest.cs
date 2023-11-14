

namespace TestProject
{
    public class UserManagerUnitTest : IDisposable
    {
        public UserManagerUnitTest() 
        {
            using (var context = new MessageService.Database.AstralisDBEntities())
            {

            };
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
