#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class AppThemeBindingTests
    {
        /// <summary>
        /// Tests that Clone creates a new AppThemeBinding instance with identical property values.
        /// Verifies that Light, Dark, Default properties and their associated flags are copied correctly.
        /// </summary>
        [Fact]
        public void Clone_WithAllPropertiesSet_CreatesIdenticalCopy()
        {
            // Arrange
            var original = new AppThemeBinding
            {
                Light = "light-value",
                Dark = "dark-value",
                Default = "default-value"
            };

            // Act
            var clone = (AppThemeBinding)original.Clone();

            // Assert
            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Light, clone.Light);
            Assert.Equal(original.Dark, clone.Dark);
            Assert.Equal(original.Default, clone.Default);
        }

        /// <summary>
        /// Tests Clone method when only Light property is set.
        /// Verifies that Dark remains unset while Light and Default are copied correctly.
        /// </summary>
        [Fact]
        public void Clone_WithOnlyLightSet_CopiesLightAndDefault()
        {
            // Arrange
            var original = new AppThemeBinding
            {
                Light = "light-only",
                Default = "default-value"
            };

            // Act
            var clone = (AppThemeBinding)original.Clone();

            // Assert
            Assert.NotNull(clone);
            Assert.Equal("light-only", clone.Light);
            Assert.Null(clone.Dark);
            Assert.Equal("default-value", clone.Default);
        }

        /// <summary>
        /// Tests Clone method when only Dark property is set.
        /// Verifies that Light remains unset while Dark and Default are copied correctly.
        /// </summary>
        [Fact]
        public void Clone_WithOnlyDarkSet_CopiesDarkAndDefault()
        {
            // Arrange
            var original = new AppThemeBinding
            {
                Dark = "dark-only",
                Default = "default-value"
            };

            // Act
            var clone = (AppThemeBinding)original.Clone();

            // Assert
            Assert.NotNull(clone);
            Assert.Null(clone.Light);
            Assert.Equal("dark-only", clone.Dark);
            Assert.Equal("default-value", clone.Default);
        }

        /// <summary>
        /// Tests Clone method with null property values.
        /// Verifies that null values are copied correctly without throwing exceptions.
        /// </summary>
        [Fact]
        public void Clone_WithNullValues_CopiesNullValues()
        {
            // Arrange
            var original = new AppThemeBinding
            {
                Light = null,
                Dark = null,
                Default = null
            };

            // Act
            var clone = (AppThemeBinding)original.Clone();

            // Assert
            Assert.NotNull(clone);
            Assert.Null(clone.Light);
            Assert.Null(clone.Dark);
            Assert.Null(clone.Default);
        }

        /// <summary>
        /// Tests Clone method with no properties set.
        /// Verifies that an empty AppThemeBinding is cloned correctly.
        /// </summary>
        [Fact]
        public void Clone_WithNoPropertiesSet_CreatesEmptyClone()
        {
            // Arrange
            var original = new AppThemeBinding();

            // Act
            var clone = (AppThemeBinding)original.Clone();

            // Assert
            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Null(clone.Light);
            Assert.Null(clone.Dark);
            Assert.Null(clone.Default);
        }

        /// <summary>
        /// Tests that the cloned AppThemeBinding is independent of the original.
        /// Verifies that changes to the original do not affect the clone and vice versa.
        /// </summary>
        [Fact]
        public void Clone_ModificationOfOriginal_DoesNotAffectClone()
        {
            // Arrange
            var original = new AppThemeBinding
            {
                Light = "original-light",
                Dark = "original-dark",
                Default = "original-default"
            };
            var clone = (AppThemeBinding)original.Clone();

            // Act
            original.Light = "modified-light";
            original.Dark = "modified-dark";
            original.Default = "modified-default";

            // Assert
            Assert.Equal("original-light", clone.Light);
            Assert.Equal("original-dark", clone.Dark);
            Assert.Equal("original-default", clone.Default);
        }

        /// <summary>
        /// Tests that modifications to the clone do not affect the original.
        /// Verifies bidirectional independence between original and clone.
        /// </summary>
        [Fact]
        public void Clone_ModificationOfClone_DoesNotAffectOriginal()
        {
            // Arrange
            var original = new AppThemeBinding
            {
                Light = "original-light",
                Dark = "original-dark",
                Default = "original-default"
            };
            var clone = (AppThemeBinding)original.Clone();

            // Act
            clone.Light = "clone-modified-light";
            clone.Dark = "clone-modified-dark";
            clone.Default = "clone-modified-default";

            // Assert
            Assert.Equal("original-light", original.Light);
            Assert.Equal("original-dark", original.Dark);
            Assert.Equal("original-default", original.Default);
        }

        /// <summary>
        /// Tests Clone method with various object types for properties.
        /// Verifies that different object types (numeric, custom objects) are cloned correctly.
        /// </summary>
        [Theory]
        [InlineData(42, 3.14, true)]
        [InlineData(-100, double.MaxValue, false)]
        [InlineData(int.MinValue, double.NaN, null)]
        [InlineData(0, double.PositiveInfinity, "string-default")]
        public void Clone_WithVariousObjectTypes_CopiesCorrectly(object lightValue, object darkValue, object defaultValue)
        {
            // Arrange
            var original = new AppThemeBinding
            {
                Light = lightValue,
                Dark = darkValue,
                Default = defaultValue
            };

            // Act
            var clone = (AppThemeBinding)original.Clone();

            // Assert
            Assert.NotNull(clone);
            Assert.Equal(lightValue, clone.Light);
            Assert.Equal(darkValue, clone.Dark);
            Assert.Equal(defaultValue, clone.Default);
        }

        /// <summary>
        /// Tests Clone method with string edge cases.
        /// Verifies that empty strings, whitespace, and long strings are handled correctly.
        /// </summary>
        [Theory]
        [InlineData("", "   ", "normal")]
        [InlineData("single", "", "whitespace-only   ")]
        [InlineData("very-long-string-that-might-cause-issues-if-not-handled-properly-during-cloning-operations", "short", "")]
        public void Clone_WithStringEdgeCases_CopiesCorrectly(string lightValue, string darkValue, string defaultValue)
        {
            // Arrange
            var original = new AppThemeBinding
            {
                Light = lightValue,
                Dark = darkValue,
                Default = defaultValue
            };

            // Act
            var clone = (AppThemeBinding)original.Clone();

            // Assert
            Assert.NotNull(clone);
            Assert.Equal(lightValue, clone.Light);
            Assert.Equal(darkValue, clone.Dark);
            Assert.Equal(defaultValue, clone.Default);
        }

        /// <summary>
        /// Tests Clone method with numeric boundary values.
        /// Verifies that extreme numeric values are copied correctly without overflow or precision loss.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue, double.MinValue, long.MaxValue)]
        [InlineData(int.MaxValue, double.MaxValue, long.MinValue)]
        [InlineData(0, 0.0, 0L)]
        [InlineData(-1, double.NegativeInfinity, -9223372036854775808L)]
        public void Clone_WithNumericBoundaryValues_CopiesCorrectly(int lightValue, double darkValue, long defaultValue)
        {
            // Arrange
            var original = new AppThemeBinding
            {
                Light = lightValue,
                Dark = darkValue,
                Default = defaultValue
            };

            // Act
            var clone = (AppThemeBinding)original.Clone();

            // Assert
            Assert.NotNull(clone);
            Assert.Equal(lightValue, clone.Light);
            Assert.Equal(darkValue, clone.Dark);
            Assert.Equal(defaultValue, clone.Default);
        }

        /// <summary>
        /// Tests Clone method to ensure VisualDiagnostics path is exercised.
        /// This test attempts to exercise the VisualDiagnostics conditional branch (line 55).
        /// </summary>
        [Fact]
        public void Clone_ExercisesVisualDiagnosticsPath_CompletesSuccessfully()
        {
            // Arrange
            var original = new AppThemeBinding
            {
                Light = "test-light",
                Dark = "test-dark",
                Default = "test-default"
            };

            // Act & Assert - Should not throw regardless of VisualDiagnostics state
            var clone = (AppThemeBinding)original.Clone();

            Assert.NotNull(clone);
            Assert.Equal("test-light", clone.Light);
            Assert.Equal("test-dark", clone.Dark);
            Assert.Equal("test-default", clone.Default);
        }

        /// <summary>
        /// Tests Clone method returns correct type.
        /// Verifies that the returned object is of type AppThemeBinding and can be cast correctly.
        /// </summary>
        [Fact]
        public void Clone_ReturnsCorrectType_CanBeCastToAppThemeBinding()
        {
            // Arrange
            var original = new AppThemeBinding();

            // Act
            var clone = original.Clone();

            // Assert
            Assert.NotNull(clone);
            Assert.IsType<AppThemeBinding>(clone);
            Assert.IsAssignableFrom<BindingBase>(clone);
        }
    }
}
