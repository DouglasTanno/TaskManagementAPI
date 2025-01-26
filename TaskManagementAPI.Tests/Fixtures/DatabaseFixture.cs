using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Services.Implementations;

public class DatabaseFixture : IDisposable
{
    public AppDbContext DbContext { get; private set; }
    public TodoService TodoService { get; private set; }
    public UserService UserService { get; private set; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        DbContext = new AppDbContext(options);
        TodoService = new TodoService(DbContext);
        UserService = new UserService(DbContext);

        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();

        TodoService.CreateTodoExamples();
        UserService.CreateSuperUser();
    }

    public void Dispose()
    {
        DbContext.Dispose();
    }
}
