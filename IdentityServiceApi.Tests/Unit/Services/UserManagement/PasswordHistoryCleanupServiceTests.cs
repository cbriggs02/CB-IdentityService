using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Services.UserManagement;
using IdentityServiceApi.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.UserManagement
{
    /// <summary>
    ///     Unit tests for the <see cref="PasswordHistoryCleanupService"/> class.
    ///     This class contains test cases for various password history clean up scenarios, verifying the 
    ///     behavior of the password history clean up functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class PasswordHistoryCleanupServiceTests
    {
        private readonly Mock<ApplicationDbContext> _contextMock;
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly PasswordHistoryCleanupService _cleanupService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PasswordHistoryCleanupServiceTests"/> class.
        ///     This constructor sets up the mocked dependencies and creates an instance 
        ///     of the <see cref="PasswordHistoryCleanupService"/> for testing.
        /// </summary>
        public PasswordHistoryCleanupServiceTests()
        {
            _contextMock = new Mock<ApplicationDbContext>();
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _cleanupService = new PasswordHistoryCleanupService(_contextMock.Object, _parameterValidatorMock.Object);
        }

        /// <summary>
        ///     Tests that an <see cref="ArgumentNullException"/> is thrown when <see cref="PasswordHistoryCleanupService"/> is 
        ///     instantiated with a null dependencies.
        /// </summary>
        [Fact]
        public void PasswordService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PasswordHistoryCleanupService(null, null));
        }

        /// <summary>
        ///     Ensures that calling the <see cref="PasswordHistoryCleanupService.DeletePasswordHistoryAsync(string)"/> 
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
        public async Task DeletePasswordHistoryAsync_InvalidUserId_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _cleanupService.DeletePasswordHistoryAsync(input));

            VerifyCallsToParameterValidator();
        }

        /// <summary>
        ///     Tests the <see cref="PasswordHistoryCleanupService.DeletePasswordHistoryAsync(string)"/> method of the cleanup 
        ///     service when no password history records are found for the given user ID. Ensures that the method completes 
        ///     successfully without errors.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task DeletePasswordHistoryAsync_NoHistoryFound_ReturnsTaskCompleted()
        {
            // Arrange
            var passwords = new List<PasswordHistory>().AsQueryable();

            SetupPasswordHistoryMock(passwords);

            // Act
            await _cleanupService.DeletePasswordHistoryAsync("id-123");

            // Assert
            VerifyCallsToParameterValidator();
            VerifyCallsToPasswordHistorySet(1);
        }

        /// <summary>
        ///     Tests the <see cref="PasswordHistoryCleanupService.DeletePasswordHistoryAsync(string)"/> method of 
        ///     the cleanup service when password history records are found for the given user ID.
        ///     Ensures that the password history entries are removed and changes are saved.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task DeletePasswordHistoryAsync_PasswordHistoryFound_ReturnsTaskCompleted()
        {
            // Arrange
            const string UserId = "id-123";
            const string Password = "password123";

            var passwordHasher = new PasswordHasher<User>();
            string passwordHash = passwordHasher.HashPassword(null, Password);

            var passwords = new List<PasswordHistory>
            {
                new() { Id = "id-1", UserId = UserId, PasswordHash = passwordHash, CreatedDate = DateTime.UtcNow},
            }.AsQueryable();

            SetupPasswordHistoryMock(passwords);

            _contextMock
                .Setup(r => r.PasswordHistories
                .RemoveRange(It.IsAny<PasswordHistory>()));
            _contextMock
                .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _cleanupService.DeletePasswordHistoryAsync(UserId);

            // Assert
            VerifyCallsToParameterValidator();
            VerifyCallsToPasswordHistorySet(2);

            _contextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        ///     Ensures that calling the <see cref="PasswordHistoryCleanupService.RemoveOldPasswordsAsync(string)"/> 
        ///     with an invalid id (null, empty, or whitespace values) throws an ArgumentNullException.
        /// </summary>
        /// <param name="input">
        ///     The input value for id.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task RemoveOldPasswordsAsync_InvalidId_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _cleanupService.RemoveOldPasswordsAsync(input));

            VerifyCallsToParameterValidator();
        }

        /// <summary>
        ///     Tests the behavior of the <see cref="PasswordHistoryCleanupService.RemoveOldPasswordsAsync(string)"/> 
        ///     method when the user has less than 5 passwords in their history. Ensures that no passwords 
        ///     are removed and the task completes successfully.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task RemoveOldPasswordsAsync_UserHasLessThen5PasswordsInHistory_ReturnsTaskCompleted()
        {
            // Arrange
            const string UserId = "id-123";
            const string Password = "password123";

            var passwordHasher = new PasswordHasher<User>();
            string passwordHash = passwordHasher.HashPassword(null, Password);

            var passwords = new List<PasswordHistory>
            {
                new() { Id = "id-1", UserId = UserId, PasswordHash = passwordHash, CreatedDate = DateTime.UtcNow},
            }.AsQueryable();

            SetupPasswordHistoryMock(passwords);

            // Act
            await _cleanupService.RemoveOldPasswordsAsync(UserId);

            // Assert
            VerifyCallsToParameterValidator();
            VerifyCallsToPasswordHistorySet(1);
        }

        /// <summary>
        ///     Tests the behavior of the <see cref="PasswordHistoryCleanupService.RemoveOldPasswordsAsync(string)"/> 
        ///     method when the user has more than 5 passwords in their history. Ensures that only the oldest 
        ///     passwords are removed and the task completes successfully.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task RemoveOldPasswordsAsync_UserHasMoreThen5PasswordsInHistory_ReturnsTaskCompleted()
        {
            // Arrange
            const string UserId = "id-123";
            const string Password = "password123";

            var passwordHasher = new PasswordHasher<User>();
            string passwordHash = passwordHasher.HashPassword(null, Password);

            var passwords = new List<PasswordHistory>
            {
                new() { Id = "id-1", UserId = UserId, PasswordHash = passwordHash, CreatedDate = DateTime.UtcNow},
                new() { Id = "id-2", UserId = UserId, PasswordHash = passwordHash, CreatedDate = DateTime.UtcNow},
                new() { Id = "id-3", UserId = UserId, PasswordHash = passwordHash, CreatedDate = DateTime.UtcNow},
                new() { Id = "id-4", UserId = UserId, PasswordHash = passwordHash, CreatedDate = DateTime.UtcNow},
                new() { Id = "id-5", UserId = UserId, PasswordHash = passwordHash, CreatedDate = DateTime.UtcNow},
                new() { Id = "id-6", UserId = UserId, PasswordHash = passwordHash, CreatedDate = DateTime.UtcNow},
                new() { Id = "id-7", UserId = UserId, PasswordHash = passwordHash, CreatedDate = DateTime.UtcNow}
            }.AsQueryable();

            SetupPasswordHistoryMock(passwords);

            // Act
            await _cleanupService.RemoveOldPasswordsAsync(UserId);

            // Assert
            VerifyCallsToParameterValidator();
            VerifyCallsToPasswordHistorySet(2);
        }

        private void SetupPasswordHistoryMock(IQueryable<PasswordHistory> passwords)
        {
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

            _contextMock.Setup(c => c.PasswordHistories).Returns(passwordHistoryDbSetMock.Object);
        }

        private void VerifyCallsToParameterValidator()
        {
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private void VerifyCallsToPasswordHistorySet(int num)
        {
            _contextMock.Verify(c => c.PasswordHistories, Times.Exactly(num));
        }
    }
}
