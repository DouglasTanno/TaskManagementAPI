using TaskManagementAPI.Data;
using TaskManagementAPI.DTO;
using TaskManagementAPI.Models;
using TaskManagementAPI.Services.Implementations;

public class UserServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly AppDbContext _dbContext;
    private readonly UserService _service;

    public UserServiceTests(DatabaseFixture fixture)
    {
        _dbContext = fixture.DbContext;
        _service = fixture.UserService;
    }

    [Fact]
    public void ValidateUser_DeveRetornarUsuarioValidoComCredenciaisCorretas()
    {
        var user = new User
        {
            Username = "testuser",
            Password = "password",
            IsSuperUser = false
        };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        var validatedUser = _service.ValidateUser("testuser", "password");

        Assert.NotNull(validatedUser);
        Assert.Equal(user.Username, validatedUser.Username);
    }

    [Fact]
    public void ValidateUser_DeveRetornarNullComCredenciaisIncorretas()
    {
        var user = new User
        {
            Username = "testuser",
            Password = "password",
            IsSuperUser = false
        };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        var validatedUser = _service.ValidateUser("testuser", "wrongpassword");

        Assert.Null(validatedUser);
    }

    [Fact]
    public void RegisterUser_DeveCadastrarUsuarioComSucesso()
    {
        var superUser = new User
        {
            Username = "superuser",
            Password = "superpassword",
            IsSuperUser = true
        };
        _dbContext.Users.Add(superUser);
        _dbContext.SaveChanges();

        var userCreateDTO = new UserCreateDTO
        {
            Username = "newuser",
            Password = "newpassword",
            IsSuperUser = false
        };

        _service.RegisterUser(userCreateDTO, superUser.Id);

        var newUser = _dbContext.Users.FirstOrDefault(u => u.Username == "newuser");

        Assert.NotNull(newUser);
        Assert.Equal("newuser", newUser.Username);
        Assert.Equal("newpassword", newUser.Password);
        Assert.False(newUser.IsSuperUser);
    }

    [Fact]
    public void RegisterUser_DeveLancarExcecaoSeUsuarioNaoForSuperUser()
    {
        var user = new User
        {
            Username = "regularuser",
            Password = "regularpassword",
            IsSuperUser = false
        };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        var userCreateDTO = new UserCreateDTO
        {
            Username = "newuser",
            Password = "newpassword",
            IsSuperUser = false
        };

        Action act = () => _service.RegisterUser(userCreateDTO, user.Id);

        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Somente um 'superuser' pode cadastrar novos usuários", exception.Message);
    }

    [Fact]
    public void CreateSuperUser_DeveCriarUsuarioSuperUserComSucesso()
    {
        _service.CreateSuperUser();

        var superUser = _dbContext.Users.FirstOrDefault(u => u.Username == "su");

        Assert.NotNull(superUser);
        Assert.True(superUser.IsSuperUser);
        Assert.Equal("su", superUser.Username);
    }
}
