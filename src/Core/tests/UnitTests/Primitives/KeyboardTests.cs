using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Converters;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
    /// <summary>
    /// Unit tests for the Keyboard class, focusing on the Plain property functionality.
    /// </summary>
    public partial class KeyboardTests
    {
        /// <summary>
        /// Tests that the Plain property returns a CustomKeyboard instance with KeyboardFlags.None on first access.
        /// This verifies the lazy initialization creates the correct keyboard type with appropriate flags.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Plain_FirstAccess_ReturnsCustomKeyboardWithNoneFlags()
        {
            // Act
            var result = Keyboard.Plain;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CustomKeyboard>(result);
            var customKeyboard = (CustomKeyboard)result;
            Assert.Equal(KeyboardFlags.None, customKeyboard.Flags);
        }

        /// <summary>
        /// Tests that multiple accesses to the Plain property return the same instance (singleton behavior).
        /// This verifies the lazy initialization caching mechanism works correctly.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Plain_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Keyboard.Plain;
            var second = Keyboard.Plain;
            var third = Keyboard.Plain;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that the Plain property consistently returns the expected keyboard configuration.
        /// This parameterized test verifies multiple sequential accesses maintain consistency.
        /// </summary>
        /// <param name="accessCount">The number of times to access the Plain property</param>
        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Plain_MultipleSequentialAccesses_MaintainsConsistency(int accessCount)
        {
            // Arrange
            Keyboard firstInstance = null;

            // Act & Assert
            for (int i = 0; i < accessCount; i++)
            {
                var current = Keyboard.Plain;

                // Verify type and flags on each access
                Assert.IsType<CustomKeyboard>(current);
                Assert.Equal(KeyboardFlags.None, ((CustomKeyboard)current).Flags);

                // Verify singleton behavior
                if (firstInstance == null)
                {
                    firstInstance = current;
                }
                else
                {
                    Assert.Same(firstInstance, current);
                }
            }
        }

        /// <summary>
        /// Tests that the Plain property returns a non-null keyboard instance.
        /// This verifies the basic contract that the property never returns null.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Plain_Access_ReturnsNonNullKeyboard()
        {
            // Act
            var result = Keyboard.Plain;

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the Plain property creates a keyboard with the correct inheritance hierarchy.
        /// This verifies that CustomKeyboard properly inherits from the base Keyboard class.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Plain_ReturnsCustomKeyboard_InheritsFromKeyboard()
        {
            // Act
            var result = Keyboard.Plain;

            // Assert
            Assert.IsType<CustomKeyboard>(result);
            Assert.IsAssignableFrom<Keyboard>(result);
        }

        /// <summary>
        /// Tests that the Chat property returns a non-null keyboard instance.
        /// Verifies that the lazy initialization creates a valid ChatKeyboard instance.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Chat_WhenAccessed_ReturnsNonNull()
        {
            // Act
            var result = Keyboard.Chat;

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the Chat property returns an instance of ChatKeyboard.
        /// Verifies that the lazy initialization creates the correct type of keyboard.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Chat_WhenAccessed_ReturnsInstanceOfChatKeyboard()
        {
            // Act
            var result = Keyboard.Chat;

            // Assert
            Assert.IsType<ChatKeyboard>(result);
        }

        /// <summary>
        /// Tests that multiple accesses to the Chat property return the same instance.
        /// Verifies the singleton behavior of the lazy initialization pattern.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Chat_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Keyboard.Chat;
            var secondAccess = Keyboard.Chat;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Chat property is assignable to the base Keyboard type.
        /// Verifies proper inheritance and type compatibility.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Chat_WhenAccessed_IsAssignableToKeyboard()
        {
            // Act
            var result = Keyboard.Chat;

            // Assert
            Assert.IsAssignableFrom<Keyboard>(result);
        }

        /// <summary>
        /// Tests that the Default property returns a non-null Keyboard instance on first access.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Default_FirstAccess_ReturnsNonNullKeyboard()
        {
            // Act
            var defaultKeyboard = Keyboard.Default;

            // Assert
            Assert.NotNull(defaultKeyboard);
            Assert.IsType<Keyboard>(defaultKeyboard);
        }

        /// <summary>
        /// Tests that multiple accesses to the Default property return the same instance (singleton behavior).
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Default_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Keyboard.Default;
            var secondAccess = Keyboard.Default;
            var thirdAccess = Keyboard.Default;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the Default property consistently returns the same instance across multiple calls.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Default_ConsistentAccess_AlwaysReturnsSameReference()
        {
            // Arrange
            var expectedInstance = Keyboard.Default;

            // Act & Assert
            for (int i = 0; i < 10; i++)
            {
                var currentInstance = Keyboard.Default;
                Assert.Same(expectedInstance, currentInstance);
            }
        }

        /// <summary>
        /// Tests that the Email property returns a non-null keyboard instance.
        /// Verifies that the lazy initialization creates an EmailKeyboard instance on first access.
        /// Expected result: Returns a non-null Keyboard instance.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Email_FirstAccess_ReturnsNonNullKeyboard()
        {
            // Act
            var emailKeyboard = Keyboard.Email;

            // Assert
            Assert.NotNull(emailKeyboard);
        }

        /// <summary>
        /// Tests that the Email property returns an instance of EmailKeyboard type.
        /// Verifies that the lazy initialization creates the correct specialized keyboard type.
        /// Expected result: Returns an EmailKeyboard instance.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Email_FirstAccess_ReturnsEmailKeyboardType()
        {
            // Act
            var emailKeyboard = Keyboard.Email;

            // Assert
            Assert.IsType<EmailKeyboard>(emailKeyboard);
        }

        /// <summary>
        /// Tests that multiple accesses to the Email property return the same instance.
        /// Verifies the singleton behavior of the lazy-initialized static property.
        /// Expected result: All accesses return the identical instance (reference equality).
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Email_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Keyboard.Email;
            var secondAccess = Keyboard.Email;
            var thirdAccess = Keyboard.Email;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the Email property consistently returns the same type across multiple accesses.
        /// Verifies type consistency in the singleton pattern implementation.
        /// Expected result: All returned instances are of EmailKeyboard type.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Email_MultipleAccesses_ConsistentType()
        {
            // Act
            var firstAccess = Keyboard.Email;
            var secondAccess = Keyboard.Email;

            // Assert
            Assert.IsType<EmailKeyboard>(firstAccess);
            Assert.IsType<EmailKeyboard>(secondAccess);
        }

        /// <summary>
        /// Tests that the Email property returns a keyboard that inherits from the base Keyboard class.
        /// Verifies the inheritance relationship is maintained in the lazy initialization.
        /// Expected result: The returned instance is assignable to Keyboard type.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Email_ReturnsKeyboardBaseType()
        {
            // Act
            var emailKeyboard = Keyboard.Email;

            // Assert
            Assert.IsAssignableFrom<Keyboard>(emailKeyboard);
        }

        /// <summary>
        /// Tests that the Numeric property returns a non-null NumericKeyboard instance on first access.
        /// This test verifies the lazy initialization behavior and ensures the property doesn't return null.
        /// Expected result: A non-null instance of NumericKeyboard.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Numeric_FirstAccess_ReturnsNonNullNumericKeyboard()
        {
            // Act
            var result = Keyboard.Numeric;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<NumericKeyboard>(result);
        }

        /// <summary>
        /// Tests that multiple accesses to the Numeric property return the same instance.
        /// This test verifies the singleton pattern implementation using the null-coalescing assignment operator.
        /// Expected result: All calls return the same cached instance.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Numeric_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Keyboard.Numeric;
            var second = Keyboard.Numeric;
            var third = Keyboard.Numeric;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that the Numeric property returns an instance that is assignable to the base Keyboard type.
        /// This test verifies the inheritance relationship between NumericKeyboard and Keyboard.
        /// Expected result: The returned instance is assignable to Keyboard type.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Numeric_ReturnedInstance_IsAssignableToKeyboard()
        {
            // Act
            var result = Keyboard.Numeric;

            // Assert
            Assert.IsAssignableFrom<Keyboard>(result);
        }

        /// <summary>
        /// Tests concurrent access to the Numeric property to verify thread safety of lazy initialization.
        /// This test ensures that the null-coalescing assignment operator works correctly under concurrent access.
        /// Expected result: All concurrent accesses return the same instance without throwing exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public async Task Numeric_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<Keyboard>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Keyboard.Numeric);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            var firstResult = results[0];
            Assert.NotNull(firstResult);

            for (int i = 1; i < results.Length; i++)
            {
                Assert.Same(firstResult, results[i]);
            }
        }

        /// <summary>
        /// Tests that the Telephone property returns a non-null TelephoneKeyboard instance.
        /// Verifies the lazy initialization creates a TelephoneKeyboard when first accessed.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Telephone_FirstAccess_ReturnsNonNullTelephoneKeyboard()
        {
            // Act
            var result = Keyboard.Telephone;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TelephoneKeyboard>(result);
        }

        /// <summary>
        /// Tests that multiple accesses to the Telephone property return the same instance.
        /// Verifies the singleton behavior of the lazily initialized static field.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Telephone_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Keyboard.Telephone;
            var secondAccess = Keyboard.Telephone;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Telephone property returns an instance that is assignable to the base Keyboard type.
        /// Verifies the inheritance relationship and type compatibility.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Telephone_ReturnedInstance_IsAssignableToKeyboard()
        {
            // Act
            var result = Keyboard.Telephone;

            // Assert
            Assert.IsAssignableFrom<Keyboard>(result);
        }

        /// <summary>
        /// Tests that the Text property returns a non-null TextKeyboard instance.
        /// Verifies that the lazy initialization creates a proper instance.
        /// Expected result: Returns a non-null instance of TextKeyboard.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Text_WhenAccessed_ReturnsNonNullTextKeyboardInstance()
        {
            // Act
            var result = Keyboard.Text;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TextKeyboard>(result);
        }

        /// <summary>
        /// Tests that multiple accesses to the Text property return the same instance.
        /// Verifies the singleton behavior of the lazy initialization pattern.
        /// Expected result: All accesses return the exact same instance (reference equality).
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Text_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Keyboard.Text;
            var second = Keyboard.Text;
            var third = Keyboard.Text;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that the Text property consistently returns a TextKeyboard type.
        /// Verifies that the type is correct and inherits from Keyboard.
        /// Expected result: Returns an instance that is assignable to Keyboard.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Text_WhenAccessed_ReturnsKeyboardCompatibleInstance()
        {
            // Act
            var result = Keyboard.Text;

            // Assert
            Assert.IsAssignableFrom<Keyboard>(result);
            Assert.IsType<TextKeyboard>(result);
        }

        /// <summary>
        /// Tests that the Time property returns a non-null TimeKeyboard instance on first access.
        /// This test verifies the lazy initialization behavior and ensures the property
        /// creates and returns a valid TimeKeyboard instance.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Time_FirstAccess_ReturnsNonNullTimeKeyboard()
        {
            // Act
            var result = Keyboard.Time;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TimeKeyboard>(result);
        }

        /// <summary>
        /// Tests that the Time property returns the same instance on multiple accesses.
        /// This test verifies the singleton behavior of the lazy initialization pattern,
        /// ensuring that the same TimeKeyboard instance is returned for subsequent calls.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Time_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Keyboard.Time;
            var second = Keyboard.Time;
            var third = Keyboard.Time;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that the Time property consistently returns a TimeKeyboard instance.
        /// This test verifies type consistency across multiple property accesses,
        /// ensuring the returned type never changes.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Time_MultipleAccesses_ConsistentlyReturnsTimeKeyboardType()
        {
            // Act & Assert
            for (int i = 0; i < 5; i++)
            {
                var result = Keyboard.Time;
                Assert.IsType<TimeKeyboard>(result);
            }
        }

        /// <summary>
        /// Tests that the Url property returns a non-null Keyboard instance.
        /// Verifies that the lazy initialization works correctly and returns a valid UrlKeyboard.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Url_FirstAccess_ReturnsNonNullKeyboard()
        {
            // Act
            var result = Keyboard.Url;

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the Url property returns an instance of UrlKeyboard type.
        /// Verifies that the correct keyboard type is instantiated by the lazy initialization.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Url_ReturnsUrlKeyboardType()
        {
            // Act
            var result = Keyboard.Url;

            // Assert
            Assert.IsType<UrlKeyboard>(result);
        }

        /// <summary>
        /// Tests that multiple accesses to the Url property return the same instance.
        /// Verifies the singleton pattern behavior of the lazy-initialized static property.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Url_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Keyboard.Url;
            var secondAccess = Keyboard.Url;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Url property consistently returns the same instance across multiple calls.
        /// Verifies the stability of the singleton pattern implementation.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Url_ConsistentAccess_MaintainsSameReference()
        {
            // Arrange
            var initialInstance = Keyboard.Url;

            // Act
            var subsequentInstance1 = Keyboard.Url;
            var subsequentInstance2 = Keyboard.Url;
            var subsequentInstance3 = Keyboard.Url;

            // Assert
            Assert.Same(initialInstance, subsequentInstance1);
            Assert.Same(initialInstance, subsequentInstance2);
            Assert.Same(initialInstance, subsequentInstance3);
        }

        /// <summary>
        /// Tests that the Date property returns a non-null Keyboard instance on first access.
        /// Verifies the lazy initialization works correctly and returns a valid keyboard.
        /// Expected result: A non-null Keyboard instance is returned.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Date_FirstAccess_ReturnsNonNullKeyboard()
        {
            // Act
            var result = Keyboard.Date;

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the Date property returns a DateKeyboard instance.
        /// Verifies that the lazy initialization creates the correct keyboard type.
        /// Expected result: The returned instance is of type DateKeyboard.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Date_FirstAccess_ReturnsDateKeyboardInstance()
        {
            // Act
            var result = Keyboard.Date;

            // Assert
            Assert.IsType<DateKeyboard>(result);
        }

        /// <summary>
        /// Tests that multiple accesses to the Date property return the same cached instance.
        /// Verifies the singleton behavior of the lazy initialization pattern.
        /// Expected result: Both accesses return the exact same object reference.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Date_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Keyboard.Date;
            var secondAccess = Keyboard.Date;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Date property returns an instance that is assignable to Keyboard type.
        /// Verifies type compatibility and inheritance hierarchy.
        /// Expected result: The returned instance is assignable to Keyboard type.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Date_ReturnsKeyboardType()
        {
            // Act
            var result = Keyboard.Date;

            // Assert
            Assert.IsAssignableFrom<Keyboard>(result);
        }

        /// <summary>
        /// Tests that the Password property returns a non-null PasswordKeyboard instance.
        /// Verifies that the lazy initialization works correctly and returns the expected type.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Password_Get_ReturnsNonNullPasswordKeyboardInstance()
        {
            // Act
            var result = Keyboard.Password;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PasswordKeyboard>(result);
        }

        /// <summary>
        /// Tests that the Password property returns the same instance on multiple accesses.
        /// Verifies the singleton behavior of the lazy initialization pattern.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Password_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Keyboard.Password;
            var secondAccess = Keyboard.Password;
            var thirdAccess = Keyboard.Password;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(firstAccess, thirdAccess);
            Assert.Same(secondAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the Password property returns an instance that inherits from Keyboard.
        /// Verifies the inheritance relationship is maintained in the returned instance.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Password_Get_ReturnsKeyboardInstance()
        {
            // Act
            var result = Keyboard.Password;

            // Assert
            Assert.IsAssignableFrom<Keyboard>(result);
        }

        /// <summary>
        /// Tests various properties of the Password keyboard instance.
        /// Verifies multiple aspects of the returned instance in a single parameterized test.
        /// </summary>
        /// <param name="expectedType">The expected type of the returned instance.</param>
        /// <param name="shouldBeNull">Whether the instance should be null.</param>
        [Theory]
        [InlineData(typeof(PasswordKeyboard), false)]
        [InlineData(typeof(Keyboard), false)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Password_Get_ReturnsExpectedTypeAndNullability(System.Type expectedType, bool shouldBeNull)
        {
            // Act
            var result = Keyboard.Password;

            // Assert
            if (shouldBeNull)
            {
                Assert.Null(result);
            }
            else
            {
                Assert.NotNull(result);
                Assert.IsAssignableFrom(expectedType, result);
            }
        }

        /// <summary>
        /// Tests that the internal Keyboard constructor can be called successfully and creates a valid instance.
        /// This test ensures the parameterless constructor works as expected without throwing exceptions.
        /// Expected result: A valid Keyboard instance is created.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_DefaultParameterless_CreatesValidInstance()
        {
            // Arrange & Act
            var keyboard = new Keyboard();

            // Assert
            Assert.NotNull(keyboard);
        }

        /// <summary>
        /// Tests that Create method returns a CustomKeyboard instance with the specified flags for individual flag values.
        /// </summary>
        /// <param name="flags">The keyboard flags to test.</param>
        [Theory]
        [InlineData(KeyboardFlags.None)]
        [InlineData(KeyboardFlags.CapitalizeSentence)]
        [InlineData(KeyboardFlags.Spellcheck)]
        [InlineData(KeyboardFlags.Suggestions)]
        [InlineData(KeyboardFlags.CapitalizeWord)]
        [InlineData(KeyboardFlags.CapitalizeCharacter)]
        [InlineData(KeyboardFlags.CapitalizeNone)]
        [InlineData(KeyboardFlags.All)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Create_WithValidIndividualFlags_ReturnsCustomKeyboardWithCorrectFlags(KeyboardFlags flags)
        {
            // Act
            var result = Keyboard.Create(flags);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CustomKeyboard>(result);
            var customKeyboard = (CustomKeyboard)result;
            Assert.Equal(flags, customKeyboard.Flags);
        }

        /// <summary>
        /// Tests that Create method returns a CustomKeyboard instance with the specified combined flags.
        /// </summary>
        /// <param name="flags">The combined keyboard flags to test.</param>
        [Theory]
        [InlineData(KeyboardFlags.CapitalizeSentence | KeyboardFlags.Spellcheck)]
        [InlineData(KeyboardFlags.Spellcheck | KeyboardFlags.Suggestions)]
        [InlineData(KeyboardFlags.CapitalizeWord | KeyboardFlags.CapitalizeCharacter)]
        [InlineData(KeyboardFlags.CapitalizeSentence | KeyboardFlags.Spellcheck | KeyboardFlags.Suggestions)]
        [InlineData(KeyboardFlags.CapitalizeWord | KeyboardFlags.Spellcheck | KeyboardFlags.Suggestions | KeyboardFlags.CapitalizeSentence)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Create_WithCombinedFlags_ReturnsCustomKeyboardWithCorrectFlags(KeyboardFlags flags)
        {
            // Act
            var result = Keyboard.Create(flags);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CustomKeyboard>(result);
            var customKeyboard = (CustomKeyboard)result;
            Assert.Equal(flags, customKeyboard.Flags);
        }

        /// <summary>
        /// Tests that Create method handles invalid enum values by creating a CustomKeyboard with the specified value.
        /// </summary>
        /// <param name="invalidValue">The invalid enum value to test.</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Create_WithInvalidEnumValues_ReturnsCustomKeyboardWithSpecifiedValue(int invalidValue)
        {
            // Arrange
            var invalidFlags = (KeyboardFlags)invalidValue;

            // Act
            var result = Keyboard.Create(invalidFlags);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CustomKeyboard>(result);
            var customKeyboard = (CustomKeyboard)result;
            Assert.Equal(invalidFlags, customKeyboard.Flags);
        }

        /// <summary>
        /// Tests that Create method with None flag returns a CustomKeyboard with no flags set.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Create_WithNoneFlag_ReturnsCustomKeyboardWithNoFlags()
        {
            // Act
            var result = Keyboard.Create(KeyboardFlags.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CustomKeyboard>(result);
            var customKeyboard = (CustomKeyboard)result;
            Assert.Equal(KeyboardFlags.None, customKeyboard.Flags);
            Assert.Equal(0, (int)customKeyboard.Flags);
        }

        /// <summary>
        /// Tests that Create method with All flag returns a CustomKeyboard with all flags set.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Create_WithAllFlag_ReturnsCustomKeyboardWithAllFlags()
        {
            // Act
            var result = Keyboard.Create(KeyboardFlags.All);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CustomKeyboard>(result);
            var customKeyboard = (CustomKeyboard)result;
            Assert.Equal(KeyboardFlags.All, customKeyboard.Flags);
        }

        /// <summary>
        /// Tests that Create method returns Keyboard type but actual instance is CustomKeyboard.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Create_ReturnsKeyboardTypeButCustomKeyboardInstance()
        {
            // Act
            Keyboard result = Keyboard.Create(KeyboardFlags.Spellcheck);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<Keyboard>(result);
            Assert.IsType<CustomKeyboard>(result);
        }

        /// <summary>
        /// Tests that multiple calls to Create with the same flags return different instances.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Create_MultipleCalls_ReturnsDifferentInstances()
        {
            // Act
            var result1 = Keyboard.Create(KeyboardFlags.Spellcheck);
            var result2 = Keyboard.Create(KeyboardFlags.Spellcheck);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
            Assert.Equal(((CustomKeyboard)result1).Flags, ((CustomKeyboard)result2).Flags);
        }
    }
}