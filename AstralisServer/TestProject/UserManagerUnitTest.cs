using DataAccessProject;


namespace TestProject
{
    public class UserManagerUnitTest : IDisposable
    {
        public UserManagerUnitTest() 
        {
            using (var context = new AstralisDBEntities())
            {

            };
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
