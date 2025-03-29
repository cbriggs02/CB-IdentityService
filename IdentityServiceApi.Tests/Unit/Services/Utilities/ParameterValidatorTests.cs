using IdentityServiceApi.Services.Utilities;

namespace IdentityServiceApi.Tests.Unit.Services.Utilities
{
    /// <summary>
    ///     Unit tests for the <see cref="ParameterValidator"/> class.
    ///     This class contains test cases for various parameter validation scenarios, verifying the 
    ///     behavior of the parameter validator functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class ParameterValidatorTests
    {
        private readonly ParameterValidator _validator;

        /// <summary>
        ///     Initializes a new instance of the ParameterValidatorTests class.
        ///     This constructor sets up the ParameterValidator instance for each test.
        /// </summary>
        public ParameterValidatorTests()
        {
            _validator = new ParameterValidator();
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
