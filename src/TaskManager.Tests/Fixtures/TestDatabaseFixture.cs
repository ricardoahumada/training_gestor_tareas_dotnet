using TaskManager.Infrastructure.Data;

namespace TaskManager.Tests.Fixtures
{
    /// <summary>
    /// Fixture para pruebas de base de datos en memoria.
    /// </summary>
    public class TestDatabaseFixture : IDisposable
    {
        public ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        public void Dispose()
        {
            // La base de datos en memoria se elimina automáticamente cuando el contexto se elimina
        }
    }
}
