using AutoMapper;
using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.DTOs;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Features.UserManagement.Services;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Results;
using IdentityServiceApi.Shared.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace IdentityServiceApi.UnitTests.Features.UserManagement.UserServiceTests
{
    [Trait("Category", "Unit")]
    public partial class UserServiceTests
    {
        private readonly Mock<IMemoryCache> _cache = new();
        private readonly Mock<IUserCacheKeyService> _cacheKey = new();
        private readonly Mock<IUserCacheService> _cacheService = new();
        private readonly Mock<UserManager<User>> _userManager = UserManagerMock();
        private readonly Mock<RoleManager<IdentityRole>> _roleManager = RoleManagerMock();
        private readonly Mock<IUserResultFactory> _resultFactory = new();
        private readonly Mock<IPasswordHistoryCleanupService> _cleanup = new();
        private readonly Mock<IPermissionService> _permissions = new();
        private readonly Mock<IParameterValidator> _validator = new();
        private readonly Mock<IUserLookupService> _lookup = new();
        private readonly Mock<ICountryService> _country = new();
        private readonly Mock<IRoleService> _roleService = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<ILoggerService> _logger = new();
        private readonly UserService _service;

        public UserServiceTests()
        {
            _service = new UserService(
                _cache.Object,
                _cacheKey.Object,
                _cacheService.Object,
                _userManager.Object,
                _roleManager.Object,
                _resultFactory.Object,
                _cleanup.Object,
                _permissions.Object,
                _validator.Object,
                _lookup.Object,
                _country.Object,
                _roleService.Object,
                _mapper.Object,
                _logger.Object
            );
        }

        private static UserDTO CreateValidDto() => new()
        {
            UserName = "user",
            FirstName = "First",
            LastName = "Last",
            Email = "email@test.com",
            PhoneNumber = "123",
            CountryId = 1
        };

        private void SetupPermissionAndLookup(User user)
        {
            _permissions
                 .Setup(x => x.ValidatePermissionsAsync("id"))
                 .ReturnsAsync(new Result { Success = true });

            _lookup
                .Setup(x => x.FindUserByIdAsync(user.Id))
                .ReturnsAsync(new UserLookupResult
                {
                    Success = true,
                    UserFound = user
                });
        }

        private static Mock<UserManager<User>> UserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object,
                null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private static Mock<RoleManager<IdentityRole>> RoleManagerMock()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(store.Object,
                null!, null!, null!, null!);
        }
    }
}