using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.RequestModels.UserManagement;
using IdentityServiceApi.Services.UserManagement;
using IdentityServiceApi.Tests.Unit.Helpers;
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
        private readonly Mock<IPasswordHistoryCleanupService> _cleanupServiceMock;
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly PasswordHistoryService _passwordHistoryService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PasswordHistoryServiceTests"/> class.
        /// </summary>
        public PasswordHistoryServiceTests()
        {
            _dbContextMock = new Mock<ApplicationDbContext>();
            _cleanupServiceMock = new Mock<IPasswordHistoryCleanupService>();
            _passwordHasherMock = new Mock<IPasswordHasher<User>>();
            _parameterValidatorMock = new Mock<IParameterValidator>();

            _passwordHistoryService = new PasswordHistoryService(
                _dbContextMock.Object,
                _cleanupServiceMock.Object,
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
            Assert.Throws<ArgumentNullException>(() => new PasswordHistoryService(null, null, null, null));
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
        public async Task AddPasswordHistory_MoreThanFivePasswords_RemovesOldPasswordHistories()
        {
            // Arrange
            var passwordHasher = new PasswordHasher<User>();
            const string Password = "password123";
            const string UserId = "User-id";
            string passwordHash = passwordHasher.HashPassword(null, Password);

            var request = new StorePasswordHistoryRequest { UserId = UserId, PasswordHash = passwordHash };

            _dbContextMock
                .Setup(s => s.PasswordHistories
                .Add(It.IsAny<PasswordHistory>()));
            _cleanupServiceMock
                .Setup(c => c.RemoveOldPasswords(UserId))
                .Returns(Task.CompletedTask);
            _dbContextMock
                .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _passwordHistoryService.AddPasswordHistory(request);

            // Assert
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cleanupServiceMock.Verify(c => c.RemoveOldPasswords(UserId), Times.Once);
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

        /// <summary>
        ///     Tests that the <see cref="PasswordHistoryService.FindPasswordHash"/> method returns true when the 
        ///     provided password matches the stored hash in the password history. This simulates a scenario where 
        ///     the password verification is successful and the provided password matches the stored hash.
        /// </summary>
        /// <returns> 
        ///     A task representing the asynchronous operation. 
        /// </returns
        [Fact]
        public async Task FindPasswordHash_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            const string UserId = "User-id";
            const string Password = "password123";

            string passwordHash = SetupMockPasswordHistory(UserId, Password);

            _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(It.Is<User>(u => u.Id == UserId), passwordHash, Password))
                .Returns(PasswordVerificationResult.Success);

            var request = ArrangeSearchPasswordHistoryRequest(UserId, Password);

            // Act
            var result = await _passwordHistoryService.FindPasswordHash(request);

            // Assert
            Assert.True(result);

            VerifyCallsToParameterService(2);
        }

        /// <summary>
        ///     Tests that the <see cref="PasswordHistoryService.FindPasswordHash"/> method returns false when the provided
        ///     password does not match the stored hash in the password history.This simulates a scenario where the password 
        ///     verification fails and the provided password does not match the stored hash.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation. 
        /// </returns>
        [Fact]
        public async Task FindPasswordHash_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            const string UserId = "User-id";
            const string Password = "password123";

            string passwordHash = SetupMockPasswordHistory(UserId, Password);

            _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(It.Is<User>(u => u.Id == UserId), passwordHash, Password))
                .Returns(PasswordVerificationResult.Failed);

            var request = ArrangeSearchPasswordHistoryRequest(UserId, Password);

            // Act
            var result = await _passwordHistoryService.FindPasswordHash(request);

            // Assert
            Assert.False(result);

            VerifyCallsToParameterService(2);
        }

        private string SetupMockPasswordHistory(string UserId, string Password)
        {
            var passwordHasher = new PasswordHasher<User>();
            string passwordHash = passwordHasher.HashPassword(null, Password);

            var passwords = new List<PasswordHistory>
            {
                new() { Id = "id-1", UserId = UserId, PasswordHash = passwordHash, CreatedDate = DateTime.UtcNow},
            }.AsQueryable();

            var passwordHistoryDbSetMock = new Mock<DbSet<PasswordHistory>>();

            passwordHistoryDbSetMock.As<IQueryable<PasswordHistory>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<PasswordHistory>(passwords.Provider));

            passwordHistoryDbSetMock.As<IQueryable<PasswordHistory>>()
                .Setup(m => m.Expression)
                .Returns(passwords.Expression);

            passwordHistoryDbSetMock.As<IQueryable<PasswordHistory>>()
                .Setup(m => m.ElementType)
                .Returns(passwords.ElementType);

            passwordHistoryDbSetMock.As<IQueryable<PasswordHistory>>()
                .Setup(m => m.GetEnumerator())
                .Returns(passwords.GetEnumerator());

            passwordHistoryDbSetMock.As<IAsyncEnumerable<PasswordHistory>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<PasswordHistory>(passwords.GetEnumerator()));

            _dbContextMock.Setup(c => c.PasswordHistories).Returns(passwordHistoryDbSetMock.Object);
            return passwordHash;
        }

        private static SearchPasswordHistoryRequest ArrangeSearchPasswordHistoryRequest(string UserId, string Password)
        {
            return new SearchPasswordHistoryRequest { UserId = UserId, Password = Password };
        }

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
        //[Theory]
        //[InlineData(null)]
        //[InlineData("")]
        //[InlineData(" ")]
        //public async Task DeletePasswordHistory_InvalidUserId_ThrowsArgumentNullException(string input)
        //{
        //    // Arrange
        //    _parameterValidatorMock
        //        .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
        //        .Throws<ArgumentNullException>();

        //    // Act & Assert
        //    await Assert.ThrowsAsync<ArgumentNullException>(() => _passwordHistoryService.DeletePasswordHistory(input));

        //    _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        //}

        private void VerifyCallsToParameterService(int numberOfTimes)
        {
            _parameterValidatorMock.Verify(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(numberOfTimes));
        }
    }
}
