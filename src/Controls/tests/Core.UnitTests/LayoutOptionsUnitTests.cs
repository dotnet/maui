#nullable disable

using System;
using System.ComponentModel;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Primitives;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class LayoutOptionsUnitTests : BaseTestFixture
    {
        [Fact]
        public void TestTypeConverter()
        {
            var converter = new LayoutOptionsConverter();
            Assert.True(converter.CanConvertFrom(typeof(string)));
            Assert.Equal(LayoutOptions.Center, converter.ConvertFromInvariantString("LayoutOptions.Center"));
            Assert.Equal(LayoutOptions.Center, converter.ConvertFromInvariantString("Center"));
            Assert.NotEqual(LayoutOptions.CenterAndExpand, converter.ConvertFromInvariantString("Center"));
            Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("foo"));
            Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("foo.bar"));
            Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("foo.bar.baz"));
        }
    }

    public partial class LayoutOptionsTests
    {
        /// <summary>
        /// Tests that the Alignment getter correctly returns the alignment value when expands is false.
        /// Verifies that the getter properly extracts the lowest 2 bits from _flags.
        /// </summary>
        /// <param name="alignment">The LayoutAlignment value to test</param>
        [Theory]
        [InlineData(LayoutAlignment.Start)]
        [InlineData(LayoutAlignment.Center)]
        [InlineData(LayoutAlignment.End)]
        [InlineData(LayoutAlignment.Fill)]
        public void Alignment_GetWithExpandsFalse_ReturnsCorrectAlignment(LayoutAlignment alignment)
        {
            // Arrange
            var layoutOptions = new LayoutOptions(alignment, false);

            // Act
            var result = layoutOptions.Alignment;

            // Assert
            Assert.Equal(alignment, result);
        }

        /// <summary>
        /// Tests that the Alignment getter correctly returns the alignment value when expands is true.
        /// Verifies that the getter properly masks out the expand flag using bitwise AND with 3.
        /// </summary>
        /// <param name="alignment">The LayoutAlignment value to test</param>
        [Theory]
        [InlineData(LayoutAlignment.Start)]
        [InlineData(LayoutAlignment.Center)]
        [InlineData(LayoutAlignment.End)]
        [InlineData(LayoutAlignment.Fill)]
        public void Alignment_GetWithExpandsTrue_ReturnsCorrectAlignment(LayoutAlignment alignment)
        {
            // Arrange
            var layoutOptions = new LayoutOptions(alignment, true);

            // Act
            var result = layoutOptions.Alignment;

            // Assert
            Assert.Equal(alignment, result);
        }

        /// <summary>
        /// Tests that the Alignment setter correctly updates the alignment value while preserving the expand state as false.
        /// Verifies that setting alignment doesn't affect the expand flag when initially false.
        /// </summary>
        /// <param name="initialAlignment">The initial LayoutAlignment value</param>
        /// <param name="newAlignment">The new LayoutAlignment value to set</param>
        [Theory]
        [InlineData(LayoutAlignment.Start, LayoutAlignment.Center)]
        [InlineData(LayoutAlignment.Center, LayoutAlignment.End)]
        [InlineData(LayoutAlignment.End, LayoutAlignment.Fill)]
        [InlineData(LayoutAlignment.Fill, LayoutAlignment.Start)]
        public void Alignment_SetWithExpandsFalse_UpdatesAlignmentPreservesExpandsFalse(LayoutAlignment initialAlignment, LayoutAlignment newAlignment)
        {
            // Arrange
            var layoutOptions = new LayoutOptions(initialAlignment, false);

            // Act
            layoutOptions.Alignment = newAlignment;

            // Assert
            Assert.Equal(newAlignment, layoutOptions.Alignment);
            Assert.False(layoutOptions.Expands);
        }

        /// <summary>
        /// Tests that the Alignment setter correctly updates the alignment value while preserving the expand state as true.
        /// Verifies that setting alignment doesn't affect the expand flag when initially true.
        /// </summary>
        /// <param name="initialAlignment">The initial LayoutAlignment value</param>
        /// <param name="newAlignment">The new LayoutAlignment value to set</param>
        [Theory]
        [InlineData(LayoutAlignment.Start, LayoutAlignment.Center)]
        [InlineData(LayoutAlignment.Center, LayoutAlignment.End)]
        [InlineData(LayoutAlignment.End, LayoutAlignment.Fill)]
        [InlineData(LayoutAlignment.Fill, LayoutAlignment.Start)]
        public void Alignment_SetWithExpandsTrue_UpdatesAlignmentPreservesExpandsTrue(LayoutAlignment initialAlignment, LayoutAlignment newAlignment)
        {
            // Arrange
            var layoutOptions = new LayoutOptions(initialAlignment, true);

            // Act
            layoutOptions.Alignment = newAlignment;

            // Assert
            Assert.Equal(newAlignment, layoutOptions.Alignment);
            Assert.True(layoutOptions.Expands);
        }

        /// <summary>
        /// Tests that setting the same alignment value multiple times works correctly.
        /// Verifies that the setter handles idempotent operations properly.
        /// </summary>
        /// <param name="alignment">The LayoutAlignment value to test</param>
        [Theory]
        [InlineData(LayoutAlignment.Start)]
        [InlineData(LayoutAlignment.Center)]
        [InlineData(LayoutAlignment.End)]
        [InlineData(LayoutAlignment.Fill)]
        public void Alignment_SetSameValueMultipleTimes_WorksCorrectly(LayoutAlignment alignment)
        {
            // Arrange
            var layoutOptions = new LayoutOptions(LayoutAlignment.Start, false);

            // Act
            layoutOptions.Alignment = alignment;
            layoutOptions.Alignment = alignment;
            layoutOptions.Alignment = alignment;

            // Assert
            Assert.Equal(alignment, layoutOptions.Alignment);
            Assert.False(layoutOptions.Expands);
        }

        /// <summary>
        /// Tests the Alignment property with all predefined static LayoutOptions instances.
        /// Verifies that the getter works correctly with the predefined static values.
        /// </summary>
        /// <param name="layoutOptions">The predefined LayoutOptions instance</param>
        /// <param name="expectedAlignment">The expected alignment value</param>
        [Theory]
        [InlineData(typeof(LayoutOptions), nameof(LayoutOptions.Start), LayoutAlignment.Start)]
        [InlineData(typeof(LayoutOptions), nameof(LayoutOptions.Center), LayoutAlignment.Center)]
        [InlineData(typeof(LayoutOptions), nameof(LayoutOptions.End), LayoutAlignment.End)]
        [InlineData(typeof(LayoutOptions), nameof(LayoutOptions.Fill), LayoutAlignment.Fill)]
        public void Alignment_GetFromPredefinedStaticInstances_ReturnsCorrectAlignment(Type type, string propertyName, LayoutAlignment expectedAlignment)
        {
            // Arrange
            var property = type.GetProperty(propertyName);
            var layoutOptions = (LayoutOptions)property.GetValue(null);

            // Act
            var result = layoutOptions.Alignment;

            // Assert
            Assert.Equal(expectedAlignment, result);
        }

        /// <summary>
        /// Tests that ToCore() correctly converts Microsoft.Maui.Controls.LayoutAlignment values
        /// to their corresponding Microsoft.Maui.Primitives.LayoutAlignment values.
        /// </summary>
        /// <param name="controlsAlignment">The Controls LayoutAlignment value to test</param>
        /// <param name="expectedPrimitivesAlignment">The expected Primitives LayoutAlignment result</param>
        [Theory]
        [InlineData(LayoutAlignment.Start, Microsoft.Maui.Primitives.LayoutAlignment.Start)]
        [InlineData(LayoutAlignment.Center, Microsoft.Maui.Primitives.LayoutAlignment.Center)]
        [InlineData(LayoutAlignment.End, Microsoft.Maui.Primitives.LayoutAlignment.End)]
        [InlineData(LayoutAlignment.Fill, Microsoft.Maui.Primitives.LayoutAlignment.Fill)]
        public void ToCore_WithValidAlignment_ReturnsCorrectPrimitivesAlignment(LayoutAlignment controlsAlignment, Microsoft.Maui.Primitives.LayoutAlignment expectedPrimitivesAlignment)
        {
            // Arrange
            var layoutOptions = new LayoutOptions(controlsAlignment, false);

            // Act
            var result = layoutOptions.ToCore();

            // Assert
            Assert.Equal(expectedPrimitivesAlignment, result);
        }

        /// <summary>
        /// Tests that ToCore() works correctly regardless of the Expands value,
        /// as the method should only consider the Alignment property.
        /// </summary>
        /// <param name="expands">The expand value to test with</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToCore_WithDifferentExpandsValues_ReturnsCorrectAlignment(bool expands)
        {
            // Arrange
            var layoutOptions = new LayoutOptions(LayoutAlignment.Center, expands);

            // Act
            var result = layoutOptions.ToCore();

            // Assert
            Assert.Equal(Microsoft.Maui.Primitives.LayoutAlignment.Center, result);
        }

        /// <summary>
        /// Tests ToCore() with static predefined LayoutOptions values to ensure
        /// they return the correct corresponding Primitives.LayoutAlignment values.
        /// </summary>
        [Fact]
        public void ToCore_WithStaticLayoutOptions_ReturnsCorrectAlignments()
        {
            // Arrange & Act & Assert
            Assert.Equal(Microsoft.Maui.Primitives.LayoutAlignment.Start, LayoutOptions.Start.ToCore());
            Assert.Equal(Microsoft.Maui.Primitives.LayoutAlignment.Center, LayoutOptions.Center.ToCore());
            Assert.Equal(Microsoft.Maui.Primitives.LayoutAlignment.End, LayoutOptions.End.ToCore());
            Assert.Equal(Microsoft.Maui.Primitives.LayoutAlignment.Fill, LayoutOptions.Fill.ToCore());
        }

        /// <summary>
        /// Tests that GetHashCode returns consistent hash codes for the same LayoutOptions instance across multiple calls.
        /// Input: Same LayoutOptions instance called multiple times.
        /// Expected: All hash code calls return the same value.
        /// </summary>
        [Fact]
        public void GetHashCode_SameInstance_ReturnsConsistentHashCode()
        {
            // Arrange
            var layoutOptions = new LayoutOptions(LayoutAlignment.Center, true);

            // Act
            var hashCode1 = layoutOptions.GetHashCode();
            var hashCode2 = layoutOptions.GetHashCode();
            var hashCode3 = layoutOptions.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
            Assert.Equal(hashCode2, hashCode3);
        }

        /// <summary>
        /// Tests that GetHashCode returns equal hash codes for equal LayoutOptions instances.
        /// Input: Two LayoutOptions instances with identical alignment and expansion values.
        /// Expected: Both instances return the same hash code.
        /// </summary>
        [Theory]
        [InlineData(LayoutAlignment.Start, false)]
        [InlineData(LayoutAlignment.Center, false)]
        [InlineData(LayoutAlignment.End, false)]
        [InlineData(LayoutAlignment.Fill, false)]
        [InlineData(LayoutAlignment.Start, true)]
        [InlineData(LayoutAlignment.Center, true)]
        [InlineData(LayoutAlignment.End, true)]
        [InlineData(LayoutAlignment.Fill, true)]
        public void GetHashCode_EqualLayoutOptions_ReturnsSameHashCode(LayoutAlignment alignment, bool expands)
        {
            // Arrange
            var layoutOptions1 = new LayoutOptions(alignment, expands);
            var layoutOptions2 = new LayoutOptions(alignment, expands);

            // Act
            var hashCode1 = layoutOptions1.GetHashCode();
            var hashCode2 = layoutOptions2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that GetHashCode returns the same hash code for predefined static instances and equivalent constructed instances.
        /// Input: Predefined static LayoutOptions and equivalent constructed instances.
        /// Expected: Hash codes are equal for equivalent instances.
        /// </summary>
        [Theory]
        [InlineData(nameof(LayoutOptions.Start), LayoutAlignment.Start, false)]
        [InlineData(nameof(LayoutOptions.Center), LayoutAlignment.Center, false)]
        [InlineData(nameof(LayoutOptions.End), LayoutAlignment.End, false)]
        [InlineData(nameof(LayoutOptions.Fill), LayoutAlignment.Fill, false)]
        public void GetHashCode_StaticInstancesAndEquivalentConstructed_ReturnsSameHashCode(string staticFieldName, LayoutAlignment alignment, bool expands)
        {
            // Arrange
            var staticInstance = GetStaticLayoutOptions(staticFieldName);
            var constructedInstance = new LayoutOptions(alignment, expands);

            // Act
            var staticHashCode = staticInstance.GetHashCode();
            var constructedHashCode = constructedInstance.GetHashCode();

            // Assert
            Assert.Equal(staticHashCode, constructedHashCode);
        }

        /// <summary>
        /// Tests that GetHashCode returns different hash codes for LayoutOptions with different alignments.
        /// Input: LayoutOptions with different alignment values but same expansion setting.
        /// Expected: Hash codes should be different (though collisions are theoretically possible).
        /// </summary>
        [Theory]
        [InlineData(LayoutAlignment.Start, LayoutAlignment.Center)]
        [InlineData(LayoutAlignment.Start, LayoutAlignment.End)]
        [InlineData(LayoutAlignment.Start, LayoutAlignment.Fill)]
        [InlineData(LayoutAlignment.Center, LayoutAlignment.End)]
        [InlineData(LayoutAlignment.Center, LayoutAlignment.Fill)]
        [InlineData(LayoutAlignment.End, LayoutAlignment.Fill)]
        public void GetHashCode_DifferentAlignments_ReturnsDifferentHashCodes(LayoutAlignment alignment1, LayoutAlignment alignment2)
        {
            // Arrange
            var layoutOptions1 = new LayoutOptions(alignment1, false);
            var layoutOptions2 = new LayoutOptions(alignment2, false);

            // Act
            var hashCode1 = layoutOptions1.GetHashCode();
            var hashCode2 = layoutOptions2.GetHashCode();

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that GetHashCode returns different hash codes for LayoutOptions with different expansion settings.
        /// Input: LayoutOptions with same alignment but different expansion values.
        /// Expected: Hash codes should be different.
        /// </summary>
        [Theory]
        [InlineData(LayoutAlignment.Start)]
        [InlineData(LayoutAlignment.Center)]
        [InlineData(LayoutAlignment.End)]
        [InlineData(LayoutAlignment.Fill)]
        public void GetHashCode_DifferentExpansionSettings_ReturnsDifferentHashCodes(LayoutAlignment alignment)
        {
            // Arrange
            var layoutOptionsNoExpand = new LayoutOptions(alignment, false);
            var layoutOptionsExpand = new LayoutOptions(alignment, true);

            // Act
            var hashCodeNoExpand = layoutOptionsNoExpand.GetHashCode();
            var hashCodeExpand = layoutOptionsExpand.GetHashCode();

            // Assert
            Assert.NotEqual(hashCodeNoExpand, hashCodeExpand);
        }

        /// <summary>
        /// Tests that GetHashCode returns consistent results for all predefined static LayoutOptions instances.
        /// Input: All predefined static LayoutOptions fields.
        /// Expected: Each static instance returns a consistent hash code.
        /// </summary>
        [Fact]
        public void GetHashCode_AllStaticInstances_ReturnsConsistentHashCodes()
        {
            // Arrange & Act
            var startHash = LayoutOptions.Start.GetHashCode();
            var centerHash = LayoutOptions.Center.GetHashCode();
            var endHash = LayoutOptions.End.GetHashCode();
            var fillHash = LayoutOptions.Fill.GetHashCode();

            // Assert - Verify consistency by calling again
            Assert.Equal(startHash, LayoutOptions.Start.GetHashCode());
            Assert.Equal(centerHash, LayoutOptions.Center.GetHashCode());
            Assert.Equal(endHash, LayoutOptions.End.GetHashCode());
            Assert.Equal(fillHash, LayoutOptions.Fill.GetHashCode());
        }

        /// <summary>
        /// Tests that GetHashCode returns expected hash codes for boundary LayoutAlignment values.
        /// Input: LayoutAlignment values at the boundaries of the valid range (0-3).
        /// Expected: Hash codes are generated without exceptions.
        /// </summary>
        [Theory]
        [InlineData((LayoutAlignment)0, false)]
        [InlineData((LayoutAlignment)1, false)]
        [InlineData((LayoutAlignment)2, false)]
        [InlineData((LayoutAlignment)3, false)]
        [InlineData((LayoutAlignment)0, true)]
        [InlineData((LayoutAlignment)1, true)]
        [InlineData((LayoutAlignment)2, true)]
        [InlineData((LayoutAlignment)3, true)]
        public void GetHashCode_BoundaryAlignmentValues_ReturnsHashCodeWithoutException(LayoutAlignment alignment, bool expands)
        {
            // Arrange
            var layoutOptions = new LayoutOptions(alignment, expands);

            // Act & Assert
            var hashCode = layoutOptions.GetHashCode();

            // Verify it's a valid integer (no exception thrown)
            Assert.True(hashCode is int);
        }

        private static LayoutOptions GetStaticLayoutOptions(string fieldName)
        {
            return fieldName switch
            {
                nameof(LayoutOptions.Start) => LayoutOptions.Start,
                nameof(LayoutOptions.Center) => LayoutOptions.Center,
                nameof(LayoutOptions.End) => LayoutOptions.End,
                nameof(LayoutOptions.Fill) => LayoutOptions.Fill,
                _ => throw new ArgumentException($"Unknown static field: {fieldName}")
            };
        }

        /// <summary>
        /// Tests that the Expands property getter returns false when the expand flag is not set
        /// for various alignment values.
        /// </summary>
        /// <param name="alignment">The layout alignment value to test (0-3).</param>
        [Theory]
        [InlineData(0)] // Start
        [InlineData(1)] // Center  
        [InlineData(2)] // End
        [InlineData(3)] // Fill
        public void Expands_Get_ReturnsFalse_WhenExpandFlagNotSet(int alignment)
        {
            // Arrange
            var layoutOptions = new LayoutOptions((LayoutAlignment)alignment, false);

            // Act
            bool result = layoutOptions.Expands;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the Expands property getter returns true when the expand flag is set
        /// for various alignment values.
        /// </summary>
        /// <param name="alignment">The layout alignment value to test (0-3).</param>
        [Theory]
        [InlineData(0)] // Start
        [InlineData(1)] // Center
        [InlineData(2)] // End  
        [InlineData(3)] // Fill
        public void Expands_Get_ReturnsTrue_WhenExpandFlagSet(int alignment)
        {
            // Arrange
            var layoutOptions = new LayoutOptions((LayoutAlignment)alignment, true);

            // Act
            bool result = layoutOptions.Expands;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the Expands property setter correctly sets the expand flag to true
        /// while preserving the alignment for all alignment values.
        /// </summary>
        /// <param name="alignment">The layout alignment value to test (0-3).</param>
        [Theory]
        [InlineData(0)] // Start
        [InlineData(1)] // Center
        [InlineData(2)] // End
        [InlineData(3)] // Fill
        public void Expands_Set_SetsExpandFlagTrue_PreservesAlignment(int alignment)
        {
            // Arrange
            var layoutOptions = new LayoutOptions((LayoutAlignment)alignment, false);

            // Act
            layoutOptions.Expands = true;

            // Assert
            Assert.True(layoutOptions.Expands);
            Assert.Equal((LayoutAlignment)alignment, layoutOptions.Alignment);
        }

        /// <summary>
        /// Tests that the Expands property setter correctly sets the expand flag to false
        /// while preserving the alignment for all alignment values.
        /// </summary>
        /// <param name="alignment">The layout alignment value to test (0-3).</param>
        [Theory]
        [InlineData(0)] // Start
        [InlineData(1)] // Center
        [InlineData(2)] // End
        [InlineData(3)] // Fill
        public void Expands_Set_SetsExpandFlagFalse_PreservesAlignment(int alignment)
        {
            // Arrange
            var layoutOptions = new LayoutOptions((LayoutAlignment)alignment, true);

            // Act
            layoutOptions.Expands = false;

            // Assert
            Assert.False(layoutOptions.Expands);
            Assert.Equal((LayoutAlignment)alignment, layoutOptions.Alignment);
        }

        /// <summary>
        /// Tests that the Expands property getter and setter work correctly together
        /// in round-trip scenarios with different initial states.
        /// </summary>
        /// <param name="initialExpands">The initial expand state.</param>
        /// <param name="newExpands">The new expand state to set.</param>
        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void Expands_GetSet_RoundTrip_WorksCorrectly(bool initialExpands, bool newExpands)
        {
            // Arrange
            var layoutOptions = new LayoutOptions(LayoutAlignment.Center, initialExpands);

            // Act
            layoutOptions.Expands = newExpands;
            bool result = layoutOptions.Expands;

            // Assert
            Assert.Equal(newExpands, result);
        }

        /// <summary>
        /// Tests the Expands property using predefined static LayoutOptions instances
        /// to verify they return the expected expand values.
        /// </summary>
        /// <param name="layoutOptions">The LayoutOptions instance to test.</param>
        /// <param name="expectedExpands">The expected Expands property value.</param>
        [Theory]
        [InlineData(nameof(LayoutOptions.Start), false)]
        [InlineData(nameof(LayoutOptions.Center), false)]
        [InlineData(nameof(LayoutOptions.End), false)]
        [InlineData(nameof(LayoutOptions.Fill), false)]
        public void Expands_Get_StaticInstances_ReturnsExpectedValue(string staticFieldName, bool expectedExpands)
        {
            // Arrange
            var layoutOptions = staticFieldName switch
            {
                nameof(LayoutOptions.Start) => LayoutOptions.Start,
                nameof(LayoutOptions.Center) => LayoutOptions.Center,
                nameof(LayoutOptions.End) => LayoutOptions.End,
                nameof(LayoutOptions.Fill) => LayoutOptions.Fill,
                _ => throw new ArgumentException($"Unknown static field: {staticFieldName}")
            };

            // Act
            bool result = layoutOptions.Expands;

            // Assert
            Assert.Equal(expectedExpands, result);
        }
    }
}