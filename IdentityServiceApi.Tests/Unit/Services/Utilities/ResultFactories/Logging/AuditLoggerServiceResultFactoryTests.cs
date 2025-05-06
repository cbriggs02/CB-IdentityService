using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Services.Utilities.ResultFactories.Common;
using IdentityServiceApi.Services.Utilities.ResultFactories.Authentication;
using Moq;
using IdentityServiceApi.Services.Utilities.ResultFactories.Logging;
using IdentityServiceApi.Models.DTO;

namespace IdentityServiceApi.Tests.Unit.Services.Utilities.ResultFactories.Logging
{
    /// <summary>
    ///     Unit tests for the <see cref="AuditLoggerServiceResultFactory"/> class.
    ///     This class contains test cases for various audit logger service result factory scenarios, verifying the 
    ///     behavior of the audit logger service result factory functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class AuditLoggerServiceResultFactoryTests
    {
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly AuditLoggerServiceResultFactory _audutLoggerServiceResultFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoginServiceResultFactoryTests"/> class.
        /// </summary>
        public AuditLoggerServiceResultFactoryTests()
        {
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _audutLoggerServiceResultFactory = new AuditLoggerServiceResultFactory(_parameterValidatorMock.Object);
        }

        /// <summary>
        ///     Verifies that the <see cref="LoginServiceResultFactory.LoginOperationFailure"/> 
        ///     method throws an <see cref="ArgumentNullException"/> when the errors array is null.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures an exception is thrown when null is passed to the method.
        /// </returns>
        [Fact]
        public void AuditLoggerOperationFailure_ErrorsArrayIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _audutLoggerServiceResultFactory.AuditLoggerOperationFailure(null));
        }

        /// <summary>
        ///     Verifies that the <see cref="AuditLoggerServiceResultFactory.AuditLoggerOperationFailure"/> 
        ///     method returns a new <see cref="AuditLogServiceResult"/> instance 
        ///     when an errors array is provided.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures a failure result is returned 
        ///     containing the specified error messages.
        /// </returns>
        [Fact]
        public void AuditLoggerOperationFailure_ErrorsArrayIsProvided_ReturnsNewLoginServiceResult()
        {
            // Arrange
            const string ExpectedErrorMessage = ErrorMessages.General.GlobalExceptionMessage;
            string[] errors = new[] { ExpectedErrorMessage };

            // Act
            var result = _audutLoggerServiceResultFactory.AuditLoggerOperationFailure(errors);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);
        }

        /// <summary>
        ///     Verifies that the <see cref="AuditLoggerServiceResultFactory.AuditLoggerOperationSuccess"/> 
        ///     method throws an <see cref="ArgumentNullException"/> when the provided audit log is null.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures an exception is thrown when a null audit log is passed to the method.
        /// </returns>
        [Fact]
        public void AuditLoggerOperationSuccess_AuditLogProvidedIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _audutLoggerServiceResultFactory.AuditLoggerOperationSuccess(null));

            VerifyCallsToParameterValidator();
        }

        /// <summary>
        ///     Verifies that the <see cref="AuditLoggerServiceResultFactory.AuditLoggerOperationSuccess"/> 
        ///     method returns a new <see cref="AuditLoggerServiceResult"/> instance 
        ///     when a valid audit log is provided.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures a successful result is returned 
        ///     containing the specified audit log.
        /// </returns>
        [Fact]
        public void AuditLoggerOperationSuccess_AuditLogIsProvided_ReturnsNewAuditLogServiceResult()
        {
            // Arrange
            var log = new AuditLogDTO
            {
                Details = "details..."
            };

            // Act
            var result = _audutLoggerServiceResultFactory.AuditLoggerOperationSuccess(log);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AuditLog);
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
            var result = _audutLoggerServiceResultFactory.GeneralOperationSuccess();

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
            Assert.Throws<ArgumentNullException>(() => _audutLoggerServiceResultFactory.GeneralOperationFailure(null));
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
            var result = _audutLoggerServiceResultFactory.GeneralOperationFailure(errors);

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
