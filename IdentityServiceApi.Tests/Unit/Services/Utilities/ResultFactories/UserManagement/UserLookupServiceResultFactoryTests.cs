using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Services.Utilities.ResultFactories.Common;
using IdentityServiceApi.Services.Utilities.ResultFactories.UserManagement;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.Utilities.ResultFactories.UserManagement
{
    /// <summary>
    ///     Unit tests for the <see cref="UserLookupServiceResultFactory"/> class.
    ///     This class contains test cases for various user lookup service result factory scenarios, verifying the 
    ///     behavior of the user lookup service result factory functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class UserLookupServiceResultFactoryTests
    {
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly UserLookupServiceResultFactory _userLookupServiceResultFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserLookupServiceResultFactoryTests"/> class.
        /// </summary>
        public UserLookupServiceResultFactoryTests()
        {
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _userLookupServiceResultFactory = new UserLookupServiceResultFactory(_parameterValidatorMock.Object);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserLookupServiceResultFactory.UserLookupOperationFailure"/> 
        ///     method throws an <see cref="ArgumentNullException"/> when the errors array is null.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures an exception is thrown when null is passed to the method.
        /// </returns>
        [Fact]
        public void UserLookupOperationFailure_ErrorsArrayIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _userLookupServiceResultFactory.UserLookupOperationFailure(null));
        }

        /// <summary>
        ///     Verifies that the <see cref="UserLookupServiceResultFactory.UserLookupOperationFailure"/> 
        ///     method returns a new <see cref="UserLookupServiceResult"/> instance 
        ///     when an errors array is provided.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures a failure result is returned 
        ///     containing the specified error messages.
        /// </returns>
        [Fact]
        public void UserLookupOperationFailure_ErrorsArrayIsProvided_ReturnsNewLoginServiceResult()
        {
            // Arrange
            const string ExpectedErrorMessage = ErrorMessages.General.GlobalExceptionMessage;
            string[] errors = new[] { ExpectedErrorMessage };

            // Act
            var result = _userLookupServiceResultFactory.UserLookupOperationFailure(errors);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserLookupServiceResultFactory.UserLookupOperationSuccess"/> 
        ///     method throws an <see cref="ArgumentNullException"/> when the provided user is null.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures an exception is thrown when a null user is passed to the method.
        /// </returns>
        [Fact]
        public void UserLookupOperationSuccess_UserProvidedIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _userLookupServiceResultFactory.UserLookupOperationSuccess(null));

            VerifyCallsToParameterValidator();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserLookupServiceResultFactory.UserLookupOperationSuccess"/> 
        ///     method returns a new <see cref="UserLookupServiceResult"/> instance 
        ///     when a valid user is provided.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures a successful result is returned 
        ///     containing the specified user.
        /// </returns>
        [Fact]
        public void UserLookupOperationSuccess_UserIsProvided_ReturnsNewUserLookupServiceResult()
        {
            // Arrange
            var user = new User
            {
                Id = "id-123",
                UserName = "user123"
            };

            // Act
            var result = _userLookupServiceResultFactory.UserLookupOperationSuccess(user);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.UserFound);
            Assert.True(result.Success);

            VerifyCallsToParameterValidator();
        }

        /// <summary>
        ///     Verifies that the <see cref="ServiceResultFactory.GeneralOperationSuccess"/> 
        ///     method returns a new <see cref="LoginServiceResult"/> instance 
        ///     indicating a successful operation.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures a successful result is returned 
        ///     when the method is invoked.
        /// </returns>
        [Fact]
        public void GeneralOperationSuccess_CreatesNewServiceResult_ReturnsNewServiceResult()
        {
            // Act
            var result = _userLookupServiceResultFactory.GeneralOperationSuccess();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        /// <summary>
        ///     Verifies that the <see cref="ServiceResultFactory.GeneralOperationFailure"/> 
        ///     method throws an <see cref="ArgumentNullException"/> when the errors array is null.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures an exception is thrown when null is passed to the method.
        /// </returns>
        [Fact]
        public void GeneralOperationFailure_ErrorsArrayIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _userLookupServiceResultFactory.GeneralOperationFailure(null));
        }

        /// <summary>
        ///     Verifies that the <see cref="ServiceResultFactory.GeneralOperationFailure"/> 
        ///     method returns a new <see cref="LoginServiceResult"/> instance 
        ///     when an errors array is provided.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures a failure result is returned 
        ///     containing the specified error messages.
        /// </returns>
        [Fact]
        public void GeneralOperationFailure_ErrorsArrayIsProvided_ReturnsNewServiceResult()
        {
            // Arrange
            const string ExpectedErrorMessage = ErrorMessages.General.GlobalExceptionMessage;
            string[] errors = new[] { ExpectedErrorMessage };

            // Act
            var result = _userLookupServiceResultFactory.GeneralOperationFailure(errors);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);
        }

        private void VerifyCallsToParameterValidator()
        {
            _parameterValidatorMock.Verify(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }
    }
}
