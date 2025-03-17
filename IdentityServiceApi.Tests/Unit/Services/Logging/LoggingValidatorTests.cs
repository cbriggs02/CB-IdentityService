using IdentityServiceApi.Services.Logging;

namespace IdentityServiceApi.Tests.Unit.Services.Logging
{
    /// <summary>
    ///     Unit tests for the <see cref="LoggingValidator"/> class.
    ///     This class contains test cases for various logging validating scenarios, verifying the 
    ///     behavior of the audit logger validator functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class LoggingValidatorTests
    {
        private readonly LoggingValidator _validator;

        /// <summary>
        ///     Initializes a new instance of the LoggingValidatorTests class.
        ///     This constructor sets up the LoggingValidator instance for each test.
        /// </summary>
        public LoggingValidatorTests()
        {
            _validator = new LoggingValidator();
        }

        /// <summary>
        ///     Tests that an ArgumentException is thrown when an invalid (null, empty) field name is provided.
        /// </summary>
        /// <param name="input">
        ///     The invalid field name (null, empty).
        /// </param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ValidateContextData_InvalidFieldName_ThrowsArgumentNullException(string input)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _validator.ValidateContextData("data", input));
        }

        /// <summary>
        ///     Tests that no exception is thrown when a valid field name is provided.
        /// </summary>
        [Fact]
        public void ValidateContextData_ValidFieldName_NoExceptionThrown()
        {
            // Act
            _validator.ValidateContextData("data", "data");
        }

        /// <summary>
        ///     Tests that an InvalidOperationException is thrown when an invalid (null or empty) value is provided.
        /// </summary>
        /// <param name="input">
        ///     The invalid value (null or empty).
        /// </param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ValidateContextData_InvalidValue_ThrowsInvalidOperationException(string input)
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _validator.ValidateContextData(input, nameof(input)));
        }

        /// <summary>
        ///     Tests that an ArgumentNullException is thrown when the input string parameter is null, empty.
        /// </summary>
        /// <param name="input">
        ///     The input string parameter (null, empty).
        /// </param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ValidateNotNullOrEmpty_InvalidStringParameter_ThrowsArgumentNullException(string input)
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _validator.ValidateNotNullOrEmpty(input, nameof(input)));
        }

        /// <summary>
        ///     Tests that no exception is thrown when the input string parameter is valid (non-null and non-whitespace).
        /// </summary>
        [Fact]
        public void ValidateNotNullOrEmpty_ValidStringParameter_NoExceptionThrown()
        {
            // Act
            _validator.ValidateNotNullOrEmpty("parameter", "parameterName");
        }

        /// <summary>
        ///     Tests that an ArgumentNullException is thrown when a null object parameter is passed.
        /// </summary>
        [Fact]
        public void ValidateObjectNotNull_NullObjectParameter_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _validator.ValidateObjectNotNull(null, "object name"));
        }

        /// <summary>
        ///     Tests that no exception is thrown when a valid object parameter is passed.
        /// </summary>
        [Fact]
        public void ValidateObjectNotNull_ValidObjectParameter_NoExceptionThrown()
        {
            var testObject = new object { };
            // Act
            _validator.ValidateObjectNotNull(testObject, "parameterName");
        }

        /// <summary>
        ///     Tests that an ArgumentNullException is thrown when a null collection is passed.
        /// </summary>
        [Fact]
        public void ValidateCollectionNotEmpty_NullCollection_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _validator.ValidateCollectionNotEmpty(null, "object name"));
        }

        /// <summary>
        ///     Tests that an ArgumentException is thrown when an empty collection (List) is passed.
        /// </summary>
        [Fact]
        public void ValidateCollectionNotEmpty_EmptyCollection_ThrowsArgumentException()
        {
            // Arrange
            var emptyList = new List<string>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _validator.ValidateCollectionNotEmpty(emptyList, nameof(emptyList)));
        }

        /// <summary>
        ///     Tests that an ArgumentException is thrown when an empty collection (Array) is passed.
        /// </summary>
        [Fact]
        public void ValidateCollectionNotEmpty_EmptyArray_ThrowsArgumentException()
        {
            // Arrange
            var emptyArray = Array.Empty<string>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _validator.ValidateCollectionNotEmpty(emptyArray, nameof(emptyArray)));
        }

        /// <summary>
        ///     Tests that an ArgumentException is thrown when an empty collection (HashSet) is passed.
        /// </summary>
        [Fact]
        public void ValidateCollectionNotEmpty_EmptyHashSet_ThrowsArgumentException()
        {
            // Arrange
            var emptyHashSet = new HashSet<string>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _validator.ValidateCollectionNotEmpty(emptyHashSet, nameof(emptyHashSet)));
        }

        /// <summary>
        ///     Tests that no exception is thrown when a non-empty collection (List) is passed.
        /// </summary>
        [Fact]
        public void ValidateCollectionNotEmpty_NonEmptyList_NoExceptionThrown()
        {
            // Arrange
            var nonEmptyList = new List<string> { "Item1" };

            // Act
            _validator.ValidateCollectionNotEmpty(nonEmptyList, nameof(nonEmptyList));
        }
    }
}
