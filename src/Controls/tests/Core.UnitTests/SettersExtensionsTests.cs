#nullable disable

using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the SettersExtensions class.
    /// </summary>
    public class SettersExtensionsTests
    {
        /// <summary>
        /// Tests that AddBinding throws ArgumentNullException when binding parameter is null.
        /// This test ensures the null check on line 20 is properly covered.
        /// </summary>
        [Fact]
        public void AddBinding_NullBinding_ThrowsArgumentNullException()
        {
            // Arrange
            var setters = Substitute.For<IList<Setter>>();
            var property = Substitute.For<BindableProperty>();
            Binding binding = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                setters.AddBinding(property, binding));
            Assert.Equal("binding", exception.ParamName);
        }

        /// <summary>
        /// Tests that AddBinding successfully adds a new Setter when valid parameters are provided.
        /// This test verifies the main functionality works correctly.
        /// </summary>
        [Fact]
        public void AddBinding_ValidParameters_AddsSetter()
        {
            // Arrange
            var setters = Substitute.For<IList<Setter>>();
            var property = Substitute.For<BindableProperty>();
            var binding = Substitute.For<Binding>();

            // Act
            setters.AddBinding(property, binding);

            // Assert
            setters.Received(1).Add(Arg.Is<Setter>(s => s.Property == property && s.Value == binding));
        }

        /// <summary>
        /// Tests that AddBinding works when property parameter is null.
        /// This verifies that null properties are accepted and passed through to the Setter.
        /// </summary>
        [Fact]
        public void AddBinding_NullProperty_AddsSetter()
        {
            // Arrange
            var setters = Substitute.For<IList<Setter>>();
            BindableProperty property = null;
            var binding = Substitute.For<Binding>();

            // Act
            setters.AddBinding(property, binding);

            // Assert
            setters.Received(1).Add(Arg.Is<Setter>(s => s.Property == null && s.Value == binding));
        }

        /// <summary>
        /// Tests that AddBinding throws NullReferenceException when setters parameter is null.
        /// This test verifies behavior with null extension method target.
        /// </summary>
        [Fact]
        public void AddBinding_NullSetters_ThrowsNullReferenceException()
        {
            // Arrange
            IList<Setter> setters = null;
            var property = Substitute.For<BindableProperty>();
            var binding = Substitute.For<Binding>();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                setters.AddBinding(property, binding));
        }

        /// <summary>
        /// Tests AddDynamicResource with a valid non-empty key to ensure the setter is added successfully.
        /// This test covers the path where string.IsNullOrEmpty(key) returns false.
        /// Expected: Setter is added to the collection with correct property and DynamicResource value.
        /// </summary>
        [Fact]
        public void AddDynamicResource_ValidKey_AddsSetter()
        {
            // Arrange
            var setters = Substitute.For<IList<Setter>>();
            var property = Substitute.For<BindableProperty>();
            var key = "TestKey";

            // Act
            setters.AddDynamicResource(property, key);

            // Assert
            setters.Received(1).Add(Arg.Is<Setter>(s =>
                s.Property == property &&
                s.Value is DynamicResource dr &&
                dr.Key == key));
        }

        /// <summary>
        /// Tests AddDynamicResource with various valid key values to ensure proper handling.
        /// This parameterized test covers different valid string scenarios.
        /// Expected: Setter is added successfully for all valid key values.
        /// </summary>
        [Theory]
        [InlineData("ValidKey")]
        [InlineData("Key_With_Underscores")]
        [InlineData("Key-With-Dashes")]
        [InlineData("KeyWithNumbers123")]
        [InlineData("Key.With.Dots")]
        [InlineData(" ")]  // Single space - not null or empty
        [InlineData("  ")]  // Multiple spaces - not null or empty
        [InlineData("\t")]  // Tab character - not null or empty
        [InlineData("\n")]  // Newline character - not null or empty
        [InlineData("VeryLongKeyNameThatExceedsNormalLengthExpectationsButShouldStillBeValid")]
        [InlineData("特殊字符Key")]  // Special/Unicode characters
        public void AddDynamicResource_VariousValidKeys_AddsSetter(string key)
        {
            // Arrange
            var setters = Substitute.For<IList<Setter>>();
            var property = Substitute.For<BindableProperty>();

            // Act
            setters.AddDynamicResource(property, key);

            // Assert
            setters.Received(1).Add(Arg.Is<Setter>(s =>
                s.Property == property &&
                s.Value is DynamicResource dr &&
                dr.Key == key));
        }

        /// <summary>
        /// Tests AddDynamicResource with null key to verify ArgumentNullException is thrown.
        /// This test validates the null key validation logic.
        /// Expected: ArgumentNullException with parameter name "key".
        /// </summary>
        [Fact]
        public void AddDynamicResource_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var setters = Substitute.For<IList<Setter>>();
            var property = Substitute.For<BindableProperty>();
            string key = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => setters.AddDynamicResource(property, key));
            Assert.Equal("key", exception.ParamName);
        }

        /// <summary>
        /// Tests AddDynamicResource with empty string key to verify ArgumentNullException is thrown.
        /// This test validates the empty string key validation logic.
        /// Expected: ArgumentNullException with parameter name "key".
        /// </summary>
        [Fact]
        public void AddDynamicResource_EmptyStringKey_ThrowsArgumentNullException()
        {
            // Arrange
            var setters = Substitute.For<IList<Setter>>();
            var property = Substitute.For<BindableProperty>();
            var key = string.Empty;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => setters.AddDynamicResource(property, key));
            Assert.Equal("key", exception.ParamName);
        }

        /// <summary>
        /// Tests AddDynamicResource with null setters collection to verify NullReferenceException is thrown.
        /// This test validates behavior when the extension method is called on null collection.
        /// Expected: NullReferenceException when attempting to call Add on null collection.
        /// </summary>
        [Fact]
        public void AddDynamicResource_NullSettersCollection_ThrowsNullReferenceException()
        {
            // Arrange
            IList<Setter> setters = null;
            var property = Substitute.For<BindableProperty>();
            var key = "TestKey";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => setters.AddDynamicResource(property, key));
        }

        /// <summary>
        /// Tests AddDynamicResource with null property to ensure it's handled correctly.
        /// Since Setter.Property can be null, this should work without throwing.
        /// Expected: Setter is added with null property and valid DynamicResource value.
        /// </summary>
        [Fact]
        public void AddDynamicResource_NullProperty_AddsSetter()
        {
            // Arrange
            var setters = Substitute.For<IList<Setter>>();
            BindableProperty property = null;
            var key = "TestKey";

            // Act
            setters.AddDynamicResource(property, key);

            // Assert
            setters.Received(1).Add(Arg.Is<Setter>(s =>
                s.Property == null &&
                s.Value is DynamicResource dr &&
                dr.Key == key));
        }

        /// <summary>
        /// Tests AddDynamicResource to verify that the created DynamicResource has the correct key.
        /// This test ensures the DynamicResource constructor is called with the provided key.
        /// Expected: DynamicResource.Key property matches the input key.
        /// </summary>
        [Fact]
        public void AddDynamicResource_ValidInputs_CreatesDynamicResourceWithCorrectKey()
        {
            // Arrange
            var setters = Substitute.For<IList<Setter>>();
            var property = Substitute.For<BindableProperty>();
            var key = "ResourceKey";

            // Act
            setters.AddDynamicResource(property, key);

            // Assert
            setters.Received(1).Add(Arg.Is<Setter>(s =>
                s.Value is DynamicResource dr && dr.Key == key));
        }

        /// <summary>
        /// Tests AddDynamicResource to verify that exactly one setter is added to the collection.
        /// This test ensures the method doesn't add multiple setters or fail to add any.
        /// Expected: Add method is called exactly once on the setters collection.
        /// </summary>
        [Fact]
        public void AddDynamicResource_ValidInputs_AddsExactlyOneSetter()
        {
            // Arrange
            var setters = Substitute.For<IList<Setter>>();
            var property = Substitute.For<BindableProperty>();
            var key = "TestKey";

            // Act
            setters.AddDynamicResource(property, key);

            // Assert
            setters.Received(1).Add(Arg.Any<Setter>());
        }
    }
}