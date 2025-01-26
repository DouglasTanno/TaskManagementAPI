using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskManagementAPI.Controllers;
using TaskManagementAPI.DTO;
using TaskManagementAPI.Services;
using Xunit;
using TaskManagementAPI.Models;

public class AuthControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _controller = new AuthController(_mockConfiguration.Object, _mockUserService.Object);
    }

    [Fact]
    public void Login_DeveRetornarTokenValido_QuandoCredenciaisCorretas()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "user1",
            Password = "password123"
        };

        var user = new User
        {
            Id = 1,
            Username = "user1",
            Password = "password123"
        };

        _mockUserService.Setup(x => x.ValidateUser(request.Username, request.Password)).Returns(user);

        var secretKey = "your-secret-key-1234567890abcdef1234567890abcdef";
        _mockConfiguration.Setup(c => c["JwtSettings:SecretKey"]).Returns(secretKey);
        _mockConfiguration.Setup(c => c["JwtSettings:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(c => c["JwtSettings:Audience"]).Returns("TestAudience");

        var result = _controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<JwtResponse>(okResult.Value);
        Assert.NotNull(response.Token);

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(response.Token);

        Assert.Equal("user1", token.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal("1", token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
    }



    [Fact]
    public void Login_DeveRetornarUnauthorized_QuandoCredenciaisIncorretas()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "user1",
            Password = "wrongpassword"
        };

        _mockUserService.Setup(x => x.ValidateUser(request.Username, request.Password)).Returns((User)null);

        // Act
        var result = _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Login_DeveRetornarBadRequest_QuandoDadosIncompletos()
    {
        var request = new LoginRequest
        {
            Username = "",
            Password = "" 
        };

        var result = _controller.Login(request);

        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
    }


    [Fact]
    public void Register_DeveRegistrarUsuarioComSucesso_QuandoUsuarioAutenticado()
    {
        // Arrange
        var userCreateDTO = new UserCreateDTO
        {
            Username = "newuser",
            Password = "newpassword"
        };

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1") // ID do usuário autenticado
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _mockUserService.Setup(x => x.RegisterUser(It.IsAny<UserCreateDTO>(), It.IsAny<int>()));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
        };

        // Act
        var result = _controller.Register(userCreateDTO);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Usuário cadastrado com sucesso.", okResult.Value);
    }

    [Fact]
    public void Register_DeveRetornarUnauthorized_QuandoUsuarioNaoAutenticado()
    {
        // Arrange
        var userCreateDTO = new UserCreateDTO
        {
            Username = "newuser",
            Password = "newpassword"
        };

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() // Usuário não autenticado
        };

        // Act
        var result = _controller.Register(userCreateDTO);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Register_DeveRetornarBadRequest_QuandoOcorrerExcecaoNoCadastro()
    {
        // Arrange
        var userCreateDTO = new UserCreateDTO
        {
            Username = "newuser",
            Password = "newpassword"
        };

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _mockUserService.Setup(x => x.RegisterUser(It.IsAny<UserCreateDTO>(), It.IsAny<int>())).Throws(new InvalidOperationException("Erro no cadastro"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
        };

        // Act
        var result = _controller.Register(userCreateDTO);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Erro no cadastro", badRequestResult.Value);
    }

    [Fact]
    public void Register_DeveRetornarBadRequest_QuandoDadosIncompletos()
    {
        var userCreateDTO = new UserCreateDTO
        {
            Username = "",
            Password = ""
        };

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
        };

        var result = _controller.Register(userCreateDTO);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    }
}
