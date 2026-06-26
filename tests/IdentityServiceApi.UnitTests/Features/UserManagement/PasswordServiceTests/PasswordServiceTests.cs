using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Requests;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Features.UserManagement.Services;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Results;
using IdentityServiceApi.Shared.Utilities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace IdentityServiceApi.UnitTests.Features.UserManagement.PasswordServiceTests
{
    [Trait("Category", "Unit")]
    public partial class PasswordServiceTests
    {
        private readonly Mock<UserManager<User>> _userManager = UserManagerMock();
        private readonly Mock<IPasswordHistoryService> _history = new();
        private readonly Mock<IPermissionService> _permissions = new();
        private readonly Mock<IParameterValidator> _validator = new();
        private readonly Mock<IResultFactory> _resultFactory = new();
        private readonly Mock<IUserLookupService> _userLookup = new();
        private readonly Mock<ILoggerService> _logger = new();
        private readonly PasswordService _service;

        public PasswordServiceTests()
        {
            _service = new PasswordService(
                _userManager.Object,
                _history.Object,
                _permissions.Object,
                _validator.Object,
                _resultFactory.Object,
                _userLookup.Object,
                _logger.Object
            );
        }

       protected void SetupValidPermissionAndUser(User user)
        {
            _permissions
                .Setup(x => x.ValidatePermissionsAsync(user.Id))
                .ReturnsAsync(new Result { Success = true });

            _userLookup
                .Setup(x => x.FindUserByIdAsync(user.Id))
                .ReturnsAsync(new UserLookupResult
                {
                    Success = true,
                    UserFound = user
                });
        }

        protected async Task<Result> PrepareSetPassword(string id)
        {
            return await _service.SetPasswordAsync(id, new SetPasswordRequest
            {
                Password = "pass",
                PasswordConfirmed = "pass"
            });
        }

        protected async Task<Result> PrepareUpdatePassword(string id)
        {
            return await _service.UpdatePasswordAsync(id, new UpdatePasswordRequest
            {
                CurrentPassword = "old",
                NewPassword = "new"
            });
        }

        protected static Mock<UserManager<User>> UserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object,
                null!, null!, null!, null!, null!, null!, null!, null!);
        }
    }
}