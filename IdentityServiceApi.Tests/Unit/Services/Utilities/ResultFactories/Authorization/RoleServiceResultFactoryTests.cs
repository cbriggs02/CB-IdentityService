using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Services.Utilities.ResultFactories.Authorization;
using IdentityServiceApi.Services.Utilities.ResultFactories.Common;
using Moq;


namespace IdentityServiceApi.Tests.Unit.Services.Utilities.ResultFactories.Authorization
{
    /// <summary>
    ///     Unit tests for the <see cref="RoleServiceResultFactory"/> class.
    ///     This class contains test cases for various role service result factory scenarios, verifying the 
    ///     behavior of the role service result factory functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class RoleServiceResultFactoryTests
    {
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly RoleServiceResultFactory _roleServiceResultFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoleServiceResultFactoryTests"/> class
        ///     and sets up the required mocks for unit testing the result factory.
        /// </summary>
        public RoleServiceResultFactoryTests()

        {
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _roleServiceResultFactory = new RoleServiceResultFactory(_parameterValidatorMock.Object);
        }

        /// <summary>
        ///     Verifies that the <see cref="RoleServiceResultFactory.RoleOperationFailure"/> method
        ///     throws an <see cref="ArgumentNullException"/> when the errors array parameter is null.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures input validation is enforced for the error array.
        /// </returns>
        [Fact]
        public void RoleOperationFailure_ErrorsArrayIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _roleServiceResultFactory.RoleOperationFailure(null));
        }

        /// <summary>
        ///     Verifies that the <see cref="RoleServiceResultFactory.RoleOperationFailure"/> method
        ///     returns a failed <see cref="RoleServiceResult"/> with the expected error messages
        ///     when a valid errors array is provided.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures failure results are constructed properly when errors are present.
        /// </returns>
        [Fact]
        public void RoleOperationFailure_ErrorsArrayIsProvided_ReturnsNewRoleServiceResult()
        {
            // Arrange
            const string ExpectedErrorMessage = ErrorMessages.General.GlobalExceptionMessage;
            string[] errors = new[] { ExpectedErrorMessage };

            // Act
            var result = _roleServiceResultFactory.RoleOperationFailure(errors);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);
        }

        /// <summary>
        ///     Verifies that the <see cref="RoleServiceResultFactory.RoleOperationSuccess"/> method
        ///     throws an <see cref="ArgumentNullException"/> when a null role DTO is provided,
        ///     ensuring input validation is enforced for successful result creation.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures the method enforces non-null input validation for role data.
        /// </returns>
        [Fact]
        public void RoleOperationSuccess_NullOrEmptyRoleDTO_ThrowsArgumentNullException()
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _roleServiceResultFactory.RoleOperationSuccess(null));

            VerifyCallsToParameterValidator();
        }

        /// <summary>
        ///     Verifies that the <see cref="RoleServiceResultFactory.RoleOperationSuccess"/> method
        ///     returns a new <see cref="RoleServiceResult"/> indicating success
        ///     when a valid <see cref="RoleDTO"/> is provided.
        /// </summary>
        /// <returns>
        ///     A unit test that ensures a successful result is returned when the role DTO is valid.
        /// </returns>
        [Fact]
        public void RoleOperationSuccess_ValidRoleDTOIsProvided_ReturnsNewRoleServiceResult()

        {
            // Arrange
            var roleDTO = new RoleDTO
            {
                Id = "role-id",
                Name = "Name",
            };

            // Act
            var result = _roleServiceResultFactory.RoleOperationSuccess(roleDTO);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Role);
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
            var result = _roleServiceResultFactory.GeneralOperationSuccess();

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
            Assert.Throws<ArgumentNullException>(() => _roleServiceResultFactory.GeneralOperationFailure(null));
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
            var result = _roleServiceResultFactory.GeneralOperationFailure(errors);

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
