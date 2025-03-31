using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Services.Utilities.ResultFactories.Common;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.Utilities.ResultFactories.Common
{
    /// <summary>
    ///     Unit tests for the <see cref="ServiceResultFactory"/> class.
    ///     This class contains test cases for various service result factory scenarios, verifying the 
    ///     behavior of the service result factory functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class ServiceResultFactoryTests
    {
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly ServiceResultFactory _serviceResultFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceResultFactoryTests"/> class.
        /// </summary>
        public ServiceResultFactoryTests()
        {
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _serviceResultFactory = new ServiceResultFactory(_parameterValidatorMock.Object);
        }

        /// <summary>
        ///     Tests that an <see cref="ArgumentNullException"/> is thrown when <see cref="ServiceResultFactory"/> is 
        ///     instantiated with a null dependencies.
        /// </summary>
        [Fact]
        public void ServiceResultFactory_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ServiceResultFactory(null));
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
            var result = _serviceResultFactory.GeneralOperationSuccess();

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
            Assert.Throws<ArgumentNullException>(() => _serviceResultFactory.GeneralOperationFailure(null));
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
            var result = _serviceResultFactory.GeneralOperationFailure(errors);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);
        }
    }
}
