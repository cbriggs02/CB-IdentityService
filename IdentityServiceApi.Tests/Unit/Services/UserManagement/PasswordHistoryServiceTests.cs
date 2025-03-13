using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.RequestModels.UserManagement;
using IdentityServiceApi.Services.UserManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.UserManagement
{
    /// <summary>
    ///     Unit tests for the <see cref="PasswordHistoryService"/> class.
    ///     This class contains test cases for various password history scenarios, verifying the 
    ///     behavior of the password history functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class PasswordHistoryServiceTests
    {
        private readonly Mock<ApplicationDbContext> _dbContextMock;
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly PasswordHistoryService _passwordHistoryService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PasswordHistoryServiceTests"/> class.
        /// </summary>
        public PasswordHistoryServiceTests()
        {
            _dbContextMock = new Mock<ApplicationDbContext>();
            _passwordHasherMock = new Mock<IPasswordHasher<User>>();
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _passwordHistoryService = new PasswordHistoryService(
                _dbContextMock.Object, 
                _passwordHasherMock.Object, 
                _parameterValidatorMock.Object
            );
        }

        /// <summary>
        ///     Tests that an <see cref="ArgumentNullException"/> is thrown when <see cref="PasswordHistoryService"/> is 
        ///     instantiated with a null dependencies.
        /// </summary>
        [Fact]
        public void PasswordHistoryService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PasswordHistoryService(null, null, null));
        }

        /// <summary>
        ///     Ensures that calling the <see cref="PasswordHistoryService.AddPasswordHistory(StorePasswordHistoryRequest)"/> 
        ///     method  with a null parameter throws an ArgumentNullException.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task AddPasswordHistory_NullParameterObject_ThrowsArgumentNullException()
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _passwordHistoryService.AddPasswordHistory(null));

            VerifyCallsToParameterService(0);
        }

        /// <summary>
        ///     Ensures that calling the <see cref="PasswordHistoryService.AddPasswordHistory(StorePasswordHistoryRequest)"/> 
        ///     with an invalid StorePasswordHistoryRequest (null, empty, or whitespace values) throws an ArgumentNullException.
        /// </summary>
        /// <param name="input">
        ///     The input value for UserId and PasswordHash.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task AddPasswordHistory_InvalidStorePasswordHistoryRequestData_ThrowsArgumentNullException(string input)
        {
            // Arrange
            var request = new StorePasswordHistoryRequest { UserId = input, PasswordHash = input };

            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _passwordHistoryService.AddPasswordHistory(request));

            VerifyCallsToParameterService(1);
        }

        /// <summary>
        ///     Tests that the <see cref="PasswordHistoryService.AddPasswordHistory(StorePasswordHistoryRequest)"/> method
        ///     successfully adds a password history record when provided with valid input data.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task AddPasswordHistory_validStorePasswordHistoryRequestData_SuccessfullyAddsPasswordHistoryRecord()
        {
            // Arrange
            var passwordHasher = new PasswordHasher<User>();
            const string Password = "password123";
            const string UserId = "User-id";
            string passwordHash = passwordHasher.HashPassword(null, Password);

            var request = new StorePasswordHistoryRequest { UserId = UserId, PasswordHash = passwordHash };

            var passwordHistory = new PasswordHistory
            {
                UserId = request.UserId,
                PasswordHash = request.PasswordHash,
                CreatedDate = DateTime.UtcNow
            };

            _dbContextMock.Setup(a => a.Add(passwordHistory));

            // Act
            await _passwordHistoryService.AddPasswordHistory(request);

            // Assert
            using var context = new ApplicationDbContext();
            var historyRecord = context.PasswordHistories.FirstAsync(x => x.UserId == UserId);

            Assert.NotNull(historyRecord);

            _dbContextMock.Verify(a => a.Add(passwordHistory), Times.Once());
        }

        /// <summary>
        ///     Ensures that calling the <see cref="PasswordHistoryService.FindPasswordHash(SearchPasswordHistoryRequest)"/> 
        ///     method  with a null parameter throws an ArgumentNullException.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task FindPasswordHash_NullParameterObject_ThrowsArgumentNullException()
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _passwordHistoryService.FindPasswordHash(null));

            VerifyCallsToParameterService(0);
        }

        /// <summary>
        ///     Ensures that calling the <see cref="PasswordHistoryService.FindPasswordHash(SearchPasswordHistoryRequest)"/> 
        ///     with an invalid StorePasswordHistoryRequest (null, empty, or whitespace values) throws an ArgumentNullException.
        /// </summary>
        /// <param name="input">
        ///     The input value for UserId and Password.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task FindPasswordHash_InvalidStorePasswordHistoryRequestData_ThrowsArgumentNullException(string input)
        {
            // Arrange
            var request = new SearchPasswordHistoryRequest { UserId = input, Password = input };

            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _passwordHistoryService.FindPasswordHash(request));

            VerifyCallsToParameterService(1);
        }

        //[Fact]
        //public async Task FindPasswordHash_ValidRequestObjectParameter_ReturnsTrueOrFalse()
        //{

        //}

        /// <summary>
        ///     Ensures that calling the <see cref="PasswordHistoryService.DeletePasswordHistory(string)"/> 
        ///     with an invalid user id (null, empty, or whitespace values) throws an ArgumentNullException.
        /// </summary>
        /// <param name="input">
        ///     The input value for UserId.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task DeletePasswordHistory_InvalidUserId_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _passwordHistoryService.DeletePasswordHistory(input));

            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private void VerifyCallsToParameterService(int numberOfTimes)
        {
            _parameterValidatorMock.Verify(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(numberOfTimes));
        }
    }
}
