using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Services.Utilities.ResultFactories.Common;
using IdentityServiceApi.Services.Utilities.ResultFactories.UserManagement;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.Utilities.ResultFactories.UserManagement
{
    /// <summary>
    ///     Unit tests for the <see cref="UserServiceResultFactory"/> class.
    ///     This class contains test cases for various user service result factory scenarios, verifying the 
    ///     behavior of the user service result factory functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class UserServiceResultFactoryTests
    {
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly UserServiceResultFactory _userServiceResultFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserServiceResultFactoryTests"/> class.
        /// </summary>
        public UserServiceResultFactoryTests()
        {
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _userServiceResultFactory = new UserServiceResultFactory(_parameterValidatorMock.Object);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserServiceResultFactory.UserOperationFailure"/> 
        ///     method throws an <see cref="ArgumentNullException"/> when the errors array is null.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures an exception is thrown when null is passed to the method.
        /// </returns>
        [Fact]
        public void UserOperationFailure_ErrorsArrayIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _userServiceResultFactory.UserOperationFailure(null));
        }

        /// <summary>
        ///     Verifies that the <see cref="UserServiceResultFactory.UserOperationFailure"/> 
        ///     method returns a new <see cref="UserServiceResult"/> instance 
        ///     when an errors array is provided.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures a failure result is returned 
        ///     containing the specified error messages.
        /// </returns>
        [Fact]
        public void UserOperationFailure_ErrorsArrayIsProvided_ReturnsNewLoginServiceResult()
        {
            // Arrange
            const string ExpectedErrorMessage = ErrorMessages.General.GlobalExceptionMessage;
            string[] errors = new[] { ExpectedErrorMessage };

            // Act
            var result = _userServiceResultFactory.UserOperationFailure(errors);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserServiceResultFactory.UserOperationSuccess"/> 
        ///     method throws an <see cref="ArgumentNullException"/> when the provided user is null.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures an exception is thrown when a null user is passed to the method.
        /// </returns>
        [Fact]
        public void UserOperationSuccess_UserProvidedIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _userServiceResultFactory.UserOperationSuccess(null));

            VerifyCallsToParameterValidator();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserServiceResultFactory.UserOperationSuccess"/> 
        ///     method returns a new <see cref="UserServiceResult"/> instance 
        ///     when a valid user is provided.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures a successful result is returned 
        ///     containing the specified user.
        /// </returns>
        [Fact]
        public void UserOperationSuccess_UserIsProvided_ReturnsNewUserLookupServiceResult()
        {
            // Arrange
            var user = new UserDTO
            {
                UserName = "user123"
            };

            // Act
            var result = _userServiceResultFactory.UserOperationSuccess(user);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.True(result.Success);

            VerifyCallsToParameterValidator();
        }

        /// <summary>
        ///     Verifies that the <see cref="ServiceResultFactory.GeneralOperationSuccess"/> 
        ///     method returns a new <see cref="ServiceResult"/> instance 
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
            var result = _userServiceResultFactory.GeneralOperationSuccess();

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
            Assert.Throws<ArgumentNullException>(() => _userServiceResultFactory.GeneralOperationFailure(null));
        }

        /// <summary>
        ///     Verifies that the <see cref="ServiceResultFactory.GeneralOperationFailure"/> 
        ///     method returns a new <see cref="ServiceResult"/> instance 
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
            var result = _userServiceResultFactory.GeneralOperationFailure(errors);

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
