using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Services.Utilities.ResultFactories.Common;
using IdentityServiceApi.Services.Utilities.ResultFactories.Authentication;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.Utilities.ResultFactories.Authentication
{
    /// <summary>
    ///     Unit tests for the <see cref="LoginServiceResultFactory"/> class.
    ///     This class contains test cases for various login service result factory scenarios, verifying the 
    ///     behavior of the login service result factory functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class LoginServiceResultFactoryTests
    {
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly LoginServiceResultFactory _loginServiceResultFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoginServiceResultFactoryTests"/> class.
        /// </summary>
        public LoginServiceResultFactoryTests()
        {
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _loginServiceResultFactory = new LoginServiceResultFactory(_parameterValidatorMock.Object);
        }

        /// <summary>
        ///     Verifies that the <see cref="LoginServiceResultFactory.LoginOperationFailure"/> 
        ///     method throws an <see cref="ArgumentNullException"/> when the errors array is null.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures an exception is thrown when null is passed to the method.
        /// </returns>
        [Fact]
        public void LoginOperationFailure_ErrorsArrayIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _loginServiceResultFactory.LoginOperationFailure(null));
        }

        /// <summary>
        ///     Verifies that the <see cref="LoginServiceResultFactory.LoginOperationFailure"/> 
        ///     method returns a new <see cref="LoginServiceResult"/> instance 
        ///     when an errors array is provided.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures a failure result is returned 
        ///     containing the specified error messages.
        /// </returns>
        [Fact]
        public void LoginOperationFailure_ErrorsArrayIsProvided_ReturnsNewLoginServiceResult()
        {
            // Arrange
            const string ExpectedErrorMessage = ErrorMessages.General.GlobalExceptionMessage;
            string[] errors = new[] { ExpectedErrorMessage };

            // Act
            var result = _loginServiceResultFactory.LoginOperationFailure(errors);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);
        }

        /// <summary>
        ///     Verifies that the <see cref="LoginServiceResultFactory.LoginOperationSuccess"/> 
        ///     method throws an <see cref="ArgumentNullException"/> when the provided token is null, empty, or contains only whitespace.
        /// </summary>
        /// <param name="input">
        ///     The token value being tested, which can be null, empty, or whitespace.
        /// </param>
        /// <returns>
        ///     A unit test that ensures an exception is thrown when an invalid token is passed to the method.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void LoginOperationSuccess_NullOrEmptyToken_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _loginServiceResultFactory.LoginOperationSuccess(input));

            VerifyCallsToParameterValidator();
        }

        /// <summary>
        ///     Verifies that the <see cref="LoginServiceResultFactory.LoginOperationSuccess"/> 
        ///     method returns a new <see cref="LoginServiceResult"/> instance 
        ///     when a valid token is provided.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures a successful result is returned 
        ///     containing the specified token.
        /// </returns>
        [Fact]
        public void LoginOperationSuccess_ValidTokenIsProvided_ReturnsNewLoginServiceResult()
        {
            // Arrange
            const string Token = "valid token.";

            // Act
            var result = _loginServiceResultFactory.LoginOperationSuccess(Token);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.True(result.Success);
            Assert.Contains(Token, result.Token);

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
            var result = _loginServiceResultFactory.GeneralOperationSuccess();

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
            Assert.Throws<ArgumentNullException>(() => _loginServiceResultFactory.GeneralOperationFailure(null));
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
            var result = _loginServiceResultFactory.GeneralOperationFailure(errors);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);
        }

        private void VerifyCallsToParameterValidator()
        {
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
