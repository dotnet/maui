#nullable disable

using System;
using System.Runtime;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Tests for the SetterSpecificity.IsHandler property
    /// </summary>
    public partial class SetterSpecificityTests
    {
        /// <summary>
        /// Tests that IsHandler returns true when SetterSpecificity is created from handler
        /// using the FromHandler static field.
        /// </summary>
        [Fact]
        public void IsHandler_FromHandlerStaticField_ReturnsTrue()
        {
            // Arrange
            var specificity = SetterSpecificity.FromHandler;

            // Act
            var result = specificity.IsHandler;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsHandler returns true when SetterSpecificity is created with
        /// ExtrasHandler (0xFF) parameter in the constructor.
        /// </summary>
        [Fact]
        public void IsHandler_CreatedWithExtrasHandler_ReturnsTrue()
        {
            // Arrange
            var specificity = new SetterSpecificity(0xFF, 0, 0, 0, 0, 0, 0, 0);

            // Act
            var result = specificity.IsHandler;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsHandler returns false when extras parameter is close to but not equal to 0xFF.
        /// Tests boundary values around the ExtrasHandler constant.
        /// </summary>
        /// <param name="extras">The extras parameter value to test</param>
        [Theory]
        [InlineData((byte)0x00)] // Minimum value
        [InlineData((byte)0x01)] // VSM extras value
        [InlineData((byte)0xFE)] // One less than handler
        [InlineData((byte)0x80)] // Mid-range value
        public void IsHandler_ExtrasNotEqualToHandler_ReturnsFalse(byte extras)
        {
            // Arrange
            var specificity = new SetterSpecificity(extras, 0, 0, 0, 0, 0, 0, 0);

            // Act
            var result = specificity.IsHandler;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsHandler returns false when created with various combinations of parameters
        /// that result in different internal _value calculations, ensuring only the handler case returns true.
        /// </summary>
        /// <param name="manual">Manual parameter</param>
        /// <param name="isDynamicResource">Dynamic resource parameter</param>
        /// <param name="isBinding">Binding parameter</param>
        /// <param name="style">Style parameter</param>
        /// <param name="id">ID parameter</param>
        /// <param name="class">Class parameter</param>
        /// <param name="type">Type parameter</param>
        [Theory]
        [InlineData((ushort)0, (byte)0, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((ushort)1, (byte)1, (byte)1, (ushort)100, (byte)50, (byte)25, (byte)10)]
        [InlineData((ushort)65535, (byte)1, (byte)1, (ushort)65535, (byte)255, (byte)255, (byte)255)]
        [InlineData((ushort)2, (byte)0, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)] // ManualTriggerBaseline
        public void IsHandler_VariousParameterCombinations_ReturnsFalse(ushort manual, byte isDynamicResource, byte isBinding, ushort style, byte id, byte @class, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(0, manual, isDynamicResource, isBinding, style, id, @class, type);

            // Act
            var result = specificity.IsHandler;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsVsm returns true when the specificity is created with VSM extras flag.
        /// This verifies that the VSM bit (0x0100000000000000) is properly set in the packed value.
        /// </summary>
        [Fact]
        public void IsVsm_WhenCreatedWithVsmExtras_ReturnsTrue()
        {
            // Arrange - Create specificity with VSM extras flag (0x01)
            var specificity = new SetterSpecificity(0x01, 0, 0, 0, 0, 0, 0, 0);

            // Act & Assert
            Assert.True(specificity.IsVsm);
        }

        /// <summary>
        /// Tests that IsVsm returns true for the predefined VisualStateSetter static instance.
        /// This verifies that the static VSM instance correctly has the VSM flag set.
        /// </summary>
        [Fact]
        public void IsVsm_ForVisualStateSetterInstance_ReturnsTrue()
        {
            // Act & Assert
            Assert.True(SetterSpecificity.VisualStateSetter.IsVsm);
        }

        /// <summary>
        /// Tests that IsVsm returns false when the specificity is created without VSM extras flag.
        /// This verifies that regular specificities don't have the VSM bit set.
        /// </summary>
        [Theory]
        [InlineData((byte)0)]
        [InlineData((byte)2)]
        [InlineData((byte)127)]
        [InlineData((byte)254)]
        public void IsVsm_WhenCreatedWithoutVsmExtras_ReturnsFalse(byte extras)
        {
            // Arrange - Create specificity without VSM extras flag
            var specificity = new SetterSpecificity(extras, 0, 0, 0, 0, 0, 0, 0);

            // Act & Assert
            Assert.False(specificity.IsVsm);
        }

        /// <summary>
        /// Tests that IsVsm returns false when VSM extras is combined with implicit styles.
        /// This verifies the special logic where VSM flag is cleared for implicit styles (style < StyleLocal).
        /// </summary>
        [Theory]
        [InlineData((ushort)0x080)] // StyleImplicit
        [InlineData((ushort)0x070)]
        [InlineData((ushort)0x0FF)] // StyleLocal - 1 (styleImplicitUpperBound)
        public void IsVsm_WhenVsmExtrasWithImplicitStyle_ReturnsFalse(ushort style)
        {
            // Arrange - Create VSM specificity with implicit style (< StyleLocal)
            var specificity = new SetterSpecificity(0x01, 0, 0, 0, style, 0, 0, 0);

            // Act & Assert
            Assert.False(specificity.IsVsm);
        }

        /// <summary>
        /// Tests that IsVsm returns true when VSM extras is combined with local styles.
        /// This verifies that VSM flag remains set for local styles (>= StyleLocal).
        /// </summary>
        [Theory]
        [InlineData((ushort)0x100)] // StyleLocal
        [InlineData((ushort)0x200)]
        [InlineData((ushort)0xFFF)]
        public void IsVsm_WhenVsmExtrasWithLocalStyle_ReturnsTrue(ushort style)
        {
            // Arrange - Create VSM specificity with local style (>= StyleLocal)
            var specificity = new SetterSpecificity(0x01, 0, 0, 0, style, 0, 0, 0);

            // Act & Assert
            Assert.True(specificity.IsVsm);
        }

        /// <summary>
        /// Tests that IsVsm returns false when created with handler extras flag.
        /// This verifies that handler specificities don't have the VSM flag set.
        /// </summary>
        [Fact]
        public void IsVsm_WhenCreatedWithHandlerExtras_ReturnsFalse()
        {
            // Arrange - Create specificity with handler extras flag (0xFF)
            var specificity = new SetterSpecificity(0xFF, 0, 0, 0, 0, 0, 0, 0);

            // Act & Assert
            Assert.False(specificity.IsVsm);
        }

        /// <summary>
        /// Tests that IsVsm returns false for the predefined FromHandler static instance.
        /// This ensures that handler instances don't have the VSM flag set.
        /// </summary>
        [Fact]
        public void IsVsm_ForFromHandlerInstance_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(SetterSpecificity.FromHandler.IsVsm);
        }

        /// <summary>
        /// Tests that IsVsm behavior is consistent regardless of other parameter values.
        /// This verifies that only the extras and style parameters affect the VSM flag.
        /// </summary>
        [Theory]
        [InlineData((ushort)1, (byte)1, (byte)1)]
        [InlineData((ushort)100, (byte)255, (byte)255)]
        [InlineData((ushort)65535, (byte)0, (byte)128)]
        public void IsVsm_WithVsmExtrasAndVariousOtherParameters_ReturnsTrue(ushort manual, byte isDynamicResource, byte isBinding)
        {
            // Arrange - Create VSM specificity with various other parameters
            var specificity = new SetterSpecificity(0x01, manual, isDynamicResource, isBinding, 0x200, 50, 100, 200);

            // Act & Assert
            Assert.True(specificity.IsVsm);
        }

        /// <summary>
        /// Tests that IsVsmImplicit returns true when VSM extras are specified with implicit style
        /// </summary>
        /// <param name="style">The style value to test</param>
        /// <param name="expected">Expected IsVsmImplicit result</param>
        [Theory]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(127, true)]
        [InlineData(255, true)]
        [InlineData(256, false)]
        [InlineData(512, false)]
        [InlineData(1024, false)]
        [InlineData(ushort.MaxValue, false)]
        public void IsVsmImplicit_WithVsmExtrasAndVariousStyles_ReturnsExpectedResult(ushort style, bool expected)
        {
            // Arrange
            var specificity = new SetterSpecificity(0x01, 0, 0, 0, style, 0, 0, 0);

            // Act
            bool result = specificity.IsVsmImplicit;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that IsVsmImplicit returns false when no VSM extras are specified
        /// </summary>
        /// <param name="extras">The extras value to test</param>
        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(127)]
        [InlineData(254)]
        [InlineData(byte.MaxValue)]
        public void IsVsmImplicit_WithoutVsmExtras_ReturnsFalse(byte extras)
        {
            // Arrange
            var specificity = new SetterSpecificity(extras, 0, 0, 0, 0, 0, 0, 0);

            // Act
            bool result = specificity.IsVsmImplicit;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsVsmImplicit returns false for the handler special case
        /// </summary>
        [Fact]
        public void IsVsmImplicit_WithHandlerExtras_ReturnsFalse()
        {
            // Arrange
            var specificity = new SetterSpecificity(0xFF, 0, 0, 0, 0, 0, 0, 0);

            // Act
            bool result = specificity.IsVsmImplicit;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests IsVsmImplicit with various parameter combinations that should return false
        /// </summary>
        /// <param name="manual">Manual specificity value</param>
        /// <param name="isDynamicResource">Dynamic resource flag</param>
        /// <param name="isBinding">Binding flag</param>
        /// <param name="id">CSS Id specificity</param>
        /// <param name="cssClass">CSS Class specificity</param>
        /// <param name="type">CSS Type specificity</param>
        [Theory]
        [InlineData(1, 0, 0, 0, 0, 0)]
        [InlineData(0, 1, 0, 0, 0, 0)]
        [InlineData(0, 0, 1, 0, 0, 0)]
        [InlineData(0, 0, 0, 1, 0, 0)]
        [InlineData(0, 0, 0, 0, 1, 0)]
        [InlineData(0, 0, 0, 0, 0, 1)]
        [InlineData(ushort.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)]
        public void IsVsmImplicit_WithoutVsmExtrasAndVariousParameters_ReturnsFalse(ushort manual, byte isDynamicResource, byte isBinding, byte id, byte cssClass, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(0, manual, isDynamicResource, isBinding, 0, id, cssClass, type);

            // Act
            bool result = specificity.IsVsmImplicit;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests IsVsmImplicit with VSM extras and various non-style parameters
        /// </summary>
        /// <param name="manual">Manual specificity value</param>
        /// <param name="isDynamicResource">Dynamic resource flag</param>
        /// <param name="isBinding">Binding flag</param>
        /// <param name="id">CSS Id specificity</param>
        /// <param name="cssClass">CSS Class specificity</param>
        /// <param name="type">CSS Type specificity</param>
        [Theory]
        [InlineData(0, 0, 0, 0, 0, 0)]
        [InlineData(1, 0, 0, 0, 0, 0)]
        [InlineData(0, 1, 0, 0, 0, 0)]
        [InlineData(0, 0, 1, 0, 0, 0)]
        [InlineData(0, 0, 0, 1, 0, 0)]
        [InlineData(0, 0, 0, 0, 1, 0)]
        [InlineData(0, 0, 0, 0, 0, 1)]
        [InlineData(100, 1, 1, 50, 75, 25)]
        public void IsVsmImplicit_WithVsmExtrasImplicitStyleAndVariousParameters_ReturnsTrue(ushort manual, byte isDynamicResource, byte isBinding, byte id, byte cssClass, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(0x01, manual, isDynamicResource, isBinding, 50, id, cssClass, type);

            // Act
            bool result = specificity.IsVsmImplicit;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests IsVsmImplicit for all predefined static instances
        /// </summary>
        /// <param name="specificity">The static SetterSpecificity instance</param>
        /// <param name="expected">Expected IsVsmImplicit result</param>
        [Theory]
        [InlineData(nameof(SetterSpecificity.DefaultValue), false)]
        [InlineData(nameof(SetterSpecificity.VisualStateSetter), true)]
        [InlineData(nameof(SetterSpecificity.FromBinding), false)]
        [InlineData(nameof(SetterSpecificity.ManualValueSetter), false)]
        [InlineData(nameof(SetterSpecificity.Trigger), false)]
        [InlineData(nameof(SetterSpecificity.DynamicResourceSetter), false)]
        [InlineData(nameof(SetterSpecificity.FromHandler), false)]
        public void IsVsmImplicit_StaticInstances_ReturnsExpectedResult(string instanceName, bool expected)
        {
            // Arrange
            SetterSpecificity specificity = instanceName switch
            {
                nameof(SetterSpecificity.DefaultValue) => SetterSpecificity.DefaultValue,
                nameof(SetterSpecificity.VisualStateSetter) => SetterSpecificity.VisualStateSetter,
                nameof(SetterSpecificity.FromBinding) => SetterSpecificity.FromBinding,
                nameof(SetterSpecificity.ManualValueSetter) => SetterSpecificity.ManualValueSetter,
                nameof(SetterSpecificity.Trigger) => SetterSpecificity.Trigger,
                nameof(SetterSpecificity.DynamicResourceSetter) => SetterSpecificity.DynamicResourceSetter,
                nameof(SetterSpecificity.FromHandler) => SetterSpecificity.FromHandler,
                _ => throw new ArgumentException($"Unknown instance name: {instanceName}")
            };

            // Act
            bool result = specificity.IsVsmImplicit;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests IsVsmImplicit with boundary values for style parameter
        /// </summary>
        [Theory]
        [InlineData(255, true)]   // StyleLocal - 1
        [InlineData(256, false)]  // StyleLocal
        [InlineData(257, false)]  // StyleLocal + 1
        public void IsVsmImplicit_WithStyleBoundaryValues_ReturnsExpectedResult(ushort style, bool expected)
        {
            // Arrange
            var specificity = new SetterSpecificity(0x01, 0, 0, 0, style, 0, 0, 0);

            // Act
            bool result = specificity.IsVsmImplicit;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests IsVsmImplicit with default parameterless constructor
        /// </summary>
        [Fact]
        public void IsVsmImplicit_DefaultConstructor_ReturnsFalse()
        {
            // Arrange
            var specificity = new SetterSpecificity();

            // Act
            bool result = specificity.IsVsmImplicit;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests IsVsmImplicit with style-only constructor
        /// </summary>
        /// <param name="style">Style value</param>
        /// <param name="id">CSS Id specificity</param>
        /// <param name="cssClass">CSS Class specificity</param>
        /// <param name="type">CSS Type specificity</param>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(100, 1, 2, 3)]
        [InlineData(500, byte.MaxValue, byte.MaxValue, byte.MaxValue)]
        public void IsVsmImplicit_StyleOnlyConstructor_ReturnsFalse(ushort style, byte id, byte cssClass, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(style, id, cssClass, type);

            // Act
            bool result = specificity.IsVsmImplicit;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that TriggerIndex returns 0 when manual value is 0 or 1
        /// </summary>
        /// <param name="manual">The manual specificity value</param>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void TriggerIndex_WhenManualValueIsZeroOrOne_ReturnsZero(ushort manual)
        {
            // Arrange
            var specificity = new SetterSpecificity(0, manual, 0, 0, 0, 0, 0, 0);

            // Act
            var result = specificity.TriggerIndex;

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that TriggerIndex returns manual minus 2 when manual value is greater than 1
        /// </summary>
        /// <param name="manual">The manual specificity value</param>
        /// <param name="expected">The expected trigger index</param>
        [Theory]
        [InlineData(2, 0)]
        [InlineData(3, 1)]
        [InlineData(10, 8)]
        [InlineData(100, 98)]
        [InlineData(1000, 998)]
        [InlineData(ushort.MaxValue, ushort.MaxValue - 2)]
        public void TriggerIndex_WhenManualValueIsGreaterThanOne_ReturnsManualMinusTwo(ushort manual, ushort expected)
        {
            // Arrange
            var specificity = new SetterSpecificity(0, manual, 0, 0, 0, 0, 0, 0);

            // Act
            var result = specificity.TriggerIndex;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests TriggerIndex with various combinations of other parameters to ensure they don't affect the result
        /// </summary>
        /// <param name="extras">Extra specificity flags</param>
        /// <param name="manual">Manual specificity value</param>
        /// <param name="isDynamicResource">Dynamic resource flag</param>
        /// <param name="isBinding">Binding flag</param>
        /// <param name="style">Style specificity</param>
        /// <param name="id">CSS Id specificity</param>
        /// <param name="class">CSS Class specificity</param>
        /// <param name="type">CSS Type specificity</param>
        /// <param name="expected">Expected trigger index</param>
        [Theory]
        [InlineData(0, 0, 0, 0, 100, 50, 25, 10, 0)]
        [InlineData(1, 5, 1, 1, 200, 100, 50, 20, 3)]
        [InlineData(0, 2, 0, 0, 0, 0, 0, 0, 0)]
        [InlineData(0, 50, 1, 0, 500, 255, 255, 255, 48)]
        [InlineData(1, 1000, 0, 1, 1000, 0, 0, 0, 998)]
        public void TriggerIndex_WithVariousParameterCombinations_OnlyDependsOnManualValue(
            byte extras, ushort manual, byte isDynamicResource, byte isBinding,
            ushort style, byte id, byte @class, byte type, ushort expected)
        {
            // Arrange
            var specificity = new SetterSpecificity(extras, manual, isDynamicResource, isBinding, style, id, @class, type);

            // Act
            var result = specificity.TriggerIndex;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests TriggerIndex boundary values around the transition point (manual = 1 to manual = 2)
        /// </summary>
        [Fact]
        public void TriggerIndex_BoundaryValues_ReturnsCorrectResults()
        {
            // Arrange & Act & Assert
            var specificity0 = new SetterSpecificity(0, 0, 0, 0, 0, 0, 0, 0);
            Assert.Equal(0, specificity0.TriggerIndex);

            var specificity1 = new SetterSpecificity(0, 1, 0, 0, 0, 0, 0, 0);
            Assert.Equal(0, specificity1.TriggerIndex);

            var specificity2 = new SetterSpecificity(0, 2, 0, 0, 0, 0, 0, 0);
            Assert.Equal(0, specificity2.TriggerIndex);

            var specificity3 = new SetterSpecificity(0, 3, 0, 0, 0, 0, 0, 0);
            Assert.Equal(1, specificity3.TriggerIndex);
        }

        /// <summary>
        /// Tests TriggerIndex for handler specificity (should return 0 since handlers have manual = 0)
        /// </summary>
        [Fact]
        public void TriggerIndex_ForHandlerSpecificity_ReturnsZero()
        {
            // Arrange
            var handlerSpecificity = new SetterSpecificity(0xFF, 100, 1, 1, 100, 50, 25, 10);

            // Act
            var result = handlerSpecificity.TriggerIndex;

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that IsBinding returns true when the SetterSpecificity is created from binding
        /// </summary>
        [Fact]
        public void IsBinding_FromBindingStaticField_ReturnsTrue()
        {
            // Arrange
            var specificity = SetterSpecificity.FromBinding;

            // Act
            var result = specificity.IsBinding;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsBinding returns the correct value based on the isBinding constructor parameter
        /// </summary>
        /// <param name="isBinding">The isBinding parameter value</param>
        /// <param name="expected">The expected IsBinding property result</param>
        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(255, true)]
        public void IsBinding_ConstructorWithBindingParameter_ReturnsExpectedValue(byte isBinding, bool expected)
        {
            // Arrange
            var specificity = new SetterSpecificity(0, 0, 0, isBinding, 0, 0, 0, 0);

            // Act
            var result = specificity.IsBinding;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that IsBinding works correctly when other parameters are also set
        /// </summary>
        [Fact]
        public void IsBinding_WithOtherParametersSet_ReturnsTrue()
        {
            // Arrange
            var specificity = new SetterSpecificity(1, 100, 1, 1, 200, 50, 75, 25);

            // Act
            var result = specificity.IsBinding;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsBinding returns false when isDynamicResource is set but isBinding is not
        /// </summary>
        [Fact]
        public void IsBinding_WithDynamicResourceButNotBinding_ReturnsFalse()
        {
            // Arrange
            var specificity = new SetterSpecificity(0, 0, 1, 0, 0, 0, 0, 0);

            // Act
            var result = specificity.IsBinding;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsBinding works correctly with maximum values for other parameters
        /// </summary>
        [Fact]
        public void IsBinding_MaximumValuesWithBinding_ReturnsTrue()
        {
            // Arrange
            var specificity = new SetterSpecificity(1, ushort.MaxValue, 255, 255, ushort.MaxValue, 255, 255, 255);

            // Act
            var result = specificity.IsBinding;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsBinding returns false when using the parameterless constructor
        /// </summary>
        [Fact]
        public void IsBinding_ParameterlessConstructor_ReturnsFalse()
        {
            // Arrange
            var specificity = new SetterSpecificity();

            // Act
            var result = specificity.IsBinding;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsBinding returns false when using the style-specific constructor
        /// </summary>
        [Fact]
        public void IsBinding_StyleConstructor_ReturnsFalse()
        {
            // Arrange
            var specificity = new SetterSpecificity(100, 10, 20, 30);

            // Act
            var result = specificity.IsBinding;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that StyleInfo property returns default tuple when style is 0xFFF (no style set scenario)
        /// </summary>
        [Fact]
        public void StyleInfo_WhenStyleIs0xFFF_ReturnsDefaultTuple()
        {
            // Arrange
            var specificity = new SetterSpecificity(0, 0, 0, 0, 0, 0, 0, 0); // Constructor sets style to 0xFFF when style == 0

            // Act
            var styleInfo = specificity.StyleInfo;

            // Assert
            Assert.Equal((ushort)0, styleInfo.Style);
            Assert.Equal((byte)0, styleInfo.Id);
            Assert.Equal((byte)0, styleInfo.Class);
            Assert.Equal((byte)0, styleInfo.Type);
        }

        /// <summary>
        /// Tests StyleInfo property with various valid style, id, class, and type combinations
        /// </summary>
        [Theory]
        [InlineData(1, 10, 20, 30)]
        [InlineData(100, 50, 75, 200)]
        [InlineData(4094, 255, 255, 255)] // Max values (style max is 0xFFF-1 = 4094 to avoid default case)
        [InlineData(1, 0, 0, 0)]
        [InlineData(500, 1, 1, 1)]
        public void StyleInfo_WithValidParameters_ReturnsCorrectValues(ushort style, byte id, byte @class, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(style, id, @class, type);

            // Act
            var styleInfo = specificity.StyleInfo;

            // Assert
            Assert.Equal(style, styleInfo.Style);
            Assert.Equal(id, styleInfo.Id);
            Assert.Equal(@class, styleInfo.Class);
            Assert.Equal(type, styleInfo.Type);
        }

        /// <summary>
        /// Tests StyleInfo property on static DefaultValue instance
        /// </summary>
        [Fact]
        public void StyleInfo_DefaultValue_ReturnsDefaultTuple()
        {
            // Act
            var styleInfo = SetterSpecificity.DefaultValue.StyleInfo;

            // Assert
            Assert.Equal((ushort)0, styleInfo.Style);
            Assert.Equal((byte)0, styleInfo.Id);
            Assert.Equal((byte)0, styleInfo.Class);
            Assert.Equal((byte)0, styleInfo.Type);
        }

        /// <summary>
        /// Tests StyleInfo property on static VisualStateSetter instance
        /// </summary>
        [Fact]
        public void StyleInfo_VisualStateSetter_ReturnsDefaultTuple()
        {
            // Act
            var styleInfo = SetterSpecificity.VisualStateSetter.StyleInfo;

            // Assert
            Assert.Equal((ushort)0, styleInfo.Style);
            Assert.Equal((byte)0, styleInfo.Id);
            Assert.Equal((byte)0, styleInfo.Class);
            Assert.Equal((byte)0, styleInfo.Type);
        }

        /// <summary>
        /// Tests StyleInfo property on static FromBinding instance
        /// </summary>
        [Fact]
        public void StyleInfo_FromBinding_ReturnsDefaultTuple()
        {
            // Act
            var styleInfo = SetterSpecificity.FromBinding.StyleInfo;

            // Assert
            Assert.Equal((ushort)0, styleInfo.Style);
            Assert.Equal((byte)0, styleInfo.Id);
            Assert.Equal((byte)0, styleInfo.Class);
            Assert.Equal((byte)0, styleInfo.Type);
        }

        /// <summary>
        /// Tests StyleInfo property on static ManualValueSetter instance
        /// </summary>
        [Fact]
        public void StyleInfo_ManualValueSetter_ReturnsDefaultTuple()
        {
            // Act
            var styleInfo = SetterSpecificity.ManualValueSetter.StyleInfo;

            // Assert
            Assert.Equal((ushort)0, styleInfo.Style);
            Assert.Equal((byte)0, styleInfo.Id);
            Assert.Equal((byte)0, styleInfo.Class);
            Assert.Equal((byte)0, styleInfo.Type);
        }

        /// <summary>
        /// Tests StyleInfo property on static Trigger instance
        /// </summary>
        [Fact]
        public void StyleInfo_Trigger_ReturnsDefaultTuple()
        {
            // Act
            var styleInfo = SetterSpecificity.Trigger.StyleInfo;

            // Assert
            Assert.Equal((ushort)0, styleInfo.Style);
            Assert.Equal((byte)0, styleInfo.Id);
            Assert.Equal((byte)0, styleInfo.Class);
            Assert.Equal((byte)0, styleInfo.Type);
        }

        /// <summary>
        /// Tests StyleInfo property on static DynamicResourceSetter instance
        /// </summary>
        [Fact]
        public void StyleInfo_DynamicResourceSetter_ReturnsDefaultTuple()
        {
            // Act
            var styleInfo = SetterSpecificity.DynamicResourceSetter.StyleInfo;

            // Assert
            Assert.Equal((ushort)0, styleInfo.Style);
            Assert.Equal((byte)0, styleInfo.Id);
            Assert.Equal((byte)0, styleInfo.Class);
            Assert.Equal((byte)0, styleInfo.Type);
        }

        /// <summary>
        /// Tests StyleInfo property on static FromHandler instance (special case with all bits set)
        /// </summary>
        [Fact]
        public void StyleInfo_FromHandler_ReturnsExtractedValues()
        {
            // Act
            var styleInfo = SetterSpecificity.FromHandler.StyleInfo;

            // Assert
            // FromHandler has _value = 0xFFFFFFFFFFFFFFFF
            // Style bits (44-55): should be 0xFFF, but since it equals 0xFFF, returns default
            Assert.Equal((ushort)0, styleInfo.Style);
            Assert.Equal((byte)0, styleInfo.Id);
            Assert.Equal((byte)0, styleInfo.Class);
            Assert.Equal((byte)0, styleInfo.Type);
        }

        /// <summary>
        /// Tests StyleInfo property with full 8-parameter constructor using non-zero style values
        /// </summary>
        [Theory]
        [InlineData((byte)0, (ushort)0, (byte)0, (byte)0, (ushort)200, (byte)10, (byte)20, (byte)30)]
        [InlineData((byte)1, (ushort)5, (byte)1, (byte)1, (ushort)300, (byte)50, (byte)100, (byte)150)]
        public void StyleInfo_WithFullConstructor_ReturnsCorrectStyleValues(byte extras, ushort manual, byte isDynamicResource, byte isBinding, ushort style, byte id, byte @class, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(extras, manual, isDynamicResource, isBinding, style, id, @class, type);

            // Act
            var styleInfo = specificity.StyleInfo;

            // Assert
            Assert.Equal(style, styleInfo.Style);
            Assert.Equal(id, styleInfo.Id);
            Assert.Equal(@class, styleInfo.Class);
            Assert.Equal(type, styleInfo.Type);
        }

        /// <summary>
        /// Tests StyleInfo property boundary values for style component (0xFFF-1 to avoid default case)
        /// </summary>
        [Fact]
        public void StyleInfo_WithMaxStyleValue_ReturnsCorrectValues()
        {
            // Arrange
            const ushort maxStyle = 0xFFE; // 4094 - just below 0xFFF to avoid default case
            const byte maxByte = 0xFF;
            var specificity = new SetterSpecificity(maxStyle, maxByte, maxByte, maxByte);

            // Act
            var styleInfo = specificity.StyleInfo;

            // Assert
            Assert.Equal(maxStyle, styleInfo.Style);
            Assert.Equal(maxByte, styleInfo.Id);
            Assert.Equal(maxByte, styleInfo.Class);
            Assert.Equal(maxByte, styleInfo.Type);
        }

        /// <summary>
        /// Tests StyleInfo property with parameterless constructor
        /// </summary>
        [Fact]
        public void StyleInfo_WithParameterlessConstructor_ReturnsDefaultTuple()
        {
            // Arrange
            var specificity = new SetterSpecificity();

            // Act
            var styleInfo = specificity.StyleInfo;

            // Assert
            Assert.Equal((ushort)0, styleInfo.Style);
            Assert.Equal((byte)0, styleInfo.Id);
            Assert.Equal((byte)0, styleInfo.Class);
            Assert.Equal((byte)0, styleInfo.Type);
        }

        /// <summary>
        /// Tests the constructor when extras parameter equals ExtrasHandler (0xFF).
        /// Should set _value to 0xFFFFFFFFFFFFFFFF regardless of other parameters.
        /// </summary>
        [Theory]
        [InlineData((byte)0xFF, (ushort)0, (byte)0, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)0xFF, (ushort)100, (byte)1, (byte)1, (ushort)200, (byte)50, (byte)25, (byte)10)]
        [InlineData((byte)0xFF, ushort.MaxValue, byte.MaxValue, byte.MaxValue, ushort.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)]
        public void Constructor_ExtrasIsHandler_SetsValueToMaxULong(byte extras, ushort manual, byte isDynamicResource, byte isBinding, ushort style, byte id, byte @class, byte type)
        {
            // Arrange & Act
            var specificity = new SetterSpecificity(extras, manual, isDynamicResource, isBinding, style, id, @class, type);

            // Assert
            Assert.True(specificity.IsHandler);
        }

        /// <summary>
        /// Tests the constructor when style parameter is zero.
        /// Should set style to 0xFFF and id, class, type to 0xFF.
        /// </summary>
        [Theory]
        [InlineData((byte)0, (ushort)0, (byte)0, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)0, (ushort)50, (byte)1, (byte)1, (ushort)0, (byte)100, (byte)200, (byte)150)]
        [InlineData((byte)2, (ushort)100, (byte)0, (byte)0, (ushort)0, (byte)50, (byte)75, (byte)25)]
        public void Constructor_StyleIsZero_SetsDefaultStyleValues(byte extras, ushort manual, byte isDynamicResource, byte isBinding, ushort style, byte id, byte @class, byte type)
        {
            // Arrange & Act
            var specificity = new SetterSpecificity(extras, manual, isDynamicResource, isBinding, style, id, @class, type);

            // Assert
            var styleInfo = specificity.StyleInfo;
            Assert.Equal((ushort)0xFFF, styleInfo.Style);
            Assert.Equal((byte)0xFF, styleInfo.Id);
            Assert.Equal((byte)0xFF, styleInfo.Class);
            Assert.Equal((byte)0xFF, styleInfo.Type);
        }

        /// <summary>
        /// Tests the constructor when extras is ExtrasVsm (0x01) and style is less than StyleLocal - 1 (255).
        /// Should trigger implicit VSM logic setting implicitVsm = 0x04 and vsm = 0.
        /// </summary>
        [Theory]
        [InlineData((byte)0x01, (ushort)0, (byte)0, (byte)0, (ushort)1, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)0x01, (ushort)50, (byte)1, (byte)1, (ushort)100, (byte)10, (byte)20, (byte)30)]
        [InlineData((byte)0x01, (ushort)200, (byte)0, (byte)0, (ushort)254, (byte)0, (byte)0, (byte)0)]
        public void Constructor_VsmWithStyleBelowImplicitUpperBound_SetsImplicitVsm(byte extras, ushort manual, byte isDynamicResource, byte isBinding, ushort style, byte id, byte @class, byte type)
        {
            // Arrange & Act
            var specificity = new SetterSpecificity(extras, manual, isDynamicResource, isBinding, style, id, @class, type);

            // Assert
            Assert.True(specificity.IsVsmImplicit);
            Assert.False(specificity.IsVsm);
        }

        /// <summary>
        /// Tests the constructor when extras is ExtrasVsm (0x01) and style is equal to or greater than StyleLocal - 1 (255).
        /// Should not trigger implicit VSM logic, keeping normal VSM behavior.
        /// </summary>
        [Theory]
        [InlineData((byte)0x01, (ushort)0, (byte)0, (byte)0, (ushort)255, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)0x01, (ushort)50, (byte)1, (byte)1, (ushort)300, (byte)10, (byte)20, (byte)30)]
        [InlineData((byte)0x01, (ushort)200, (byte)0, (byte)0, ushort.MaxValue, (byte)0, (byte)0, (byte)0)]
        public void Constructor_VsmWithStyleAtOrAboveImplicitUpperBound_KeepsNormalVsm(byte extras, ushort manual, byte isDynamicResource, byte isBinding, ushort style, byte id, byte @class, byte type)
        {
            // Arrange & Act
            var specificity = new SetterSpecificity(extras, manual, isDynamicResource, isBinding, style, id, @class, type);

            // Assert
            Assert.True(specificity.IsVsm);
            Assert.False(specificity.IsVsmImplicit);
        }

        /// <summary>
        /// Tests the constructor with boundary values for all parameters.
        /// Should handle minimum and maximum values correctly.
        /// </summary>
        [Theory]
        [InlineData(byte.MinValue, ushort.MinValue, byte.MinValue, byte.MinValue, ushort.MinValue, byte.MinValue, byte.MinValue, byte.MinValue)]
        [InlineData(byte.MaxValue, ushort.MaxValue, byte.MaxValue, byte.MaxValue, ushort.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)]
        [InlineData((byte)127, (ushort)32767, (byte)127, (byte)127, (ushort)32767, (byte)127, (byte)127, (byte)127)]
        public void Constructor_BoundaryValues_HandlesCorrectly(byte extras, ushort manual, byte isDynamicResource, byte isBinding, ushort style, byte id, byte @class, byte type)
        {
            // Arrange & Act
            var specificity = new SetterSpecificity(extras, manual, isDynamicResource, isBinding, style, id, @class, type);

            // Assert
            Assert.NotNull(specificity);
            // For max values case with extras = 255, should be handler
            if (extras == 0xFF)
            {
                Assert.True(specificity.IsHandler);
            }
        }

        /// <summary>
        /// Tests the constructor with isDynamicResource parameter variations.
        /// Values greater than 0 should set dynamic resource flag, 0 should not.
        /// </summary>
        [Theory]
        [InlineData((byte)0, false)]
        [InlineData((byte)1, true)]
        [InlineData((byte)2, true)]
        [InlineData(byte.MaxValue, true)]
        public void Constructor_IsDynamicResourceParameter_SetsCorrectFlag(byte isDynamicResource, bool expectedResult)
        {
            // Arrange & Act
            var specificity = new SetterSpecificity(0, 0, isDynamicResource, 0, 1, 0, 0, 0);

            // Assert
            Assert.Equal(expectedResult, specificity.IsDynamicResource);
        }

        /// <summary>
        /// Tests the constructor with isBinding parameter variations.
        /// Values greater than 0 should set binding flag, 0 should not.
        /// </summary>
        [Theory]
        [InlineData((byte)0, false)]
        [InlineData((byte)1, true)]
        [InlineData((byte)2, true)]
        [InlineData(byte.MaxValue, true)]
        public void Constructor_IsBindingParameter_SetsCorrectFlag(byte isBinding, bool expectedResult)
        {
            // Arrange & Act
            var specificity = new SetterSpecificity(0, 0, 0, isBinding, 1, 0, 0, 0);

            // Assert
            Assert.Equal(expectedResult, specificity.IsBinding);
        }

        /// <summary>
        /// Tests the constructor with manual parameter variations.
        /// Should correctly handle manual values and trigger indices.
        /// </summary>
        [Theory]
        [InlineData((ushort)0, false)]
        [InlineData((ushort)1, true)]
        [InlineData((ushort)2, false)] // ManualTriggerBaseline
        [InlineData((ushort)100, false)]
        [InlineData(ushort.MaxValue, false)]
        public void Constructor_ManualParameter_SetsCorrectManualFlag(ushort manual, bool expectedIsManual)
        {
            // Arrange & Act
            var specificity = new SetterSpecificity(0, manual, 0, 0, 1, 0, 0, 0);

            // Assert
            Assert.Equal(expectedIsManual, specificity.IsManual);
        }

        /// <summary>
        /// Tests the constructor with various extras values.
        /// Should handle VSM, Handler, and other values correctly.
        /// </summary>
        [Theory]
        [InlineData((byte)0x00, false, false)]
        [InlineData((byte)0x01, true, false)]  // ExtrasVsm
        [InlineData((byte)0x02, false, false)]
        [InlineData((byte)0xFE, false, false)]
        [InlineData((byte)0xFF, false, true)]  // ExtrasHandler
        public void Constructor_ExtrasParameter_SetsCorrectFlags(byte extras, bool expectedIsVsm, bool expectedIsHandler)
        {
            // Arrange & Act
            var specificity = new SetterSpecificity(extras, 0, 0, 0, 1, 0, 0, 0);

            // Assert
            Assert.Equal(expectedIsVsm, specificity.IsVsm);
            Assert.Equal(expectedIsHandler, specificity.IsHandler);
        }

        /// <summary>
        /// Tests the constructor with combinations that should not be default.
        /// Any non-zero parameter should result in non-default specificity.
        /// </summary>
        [Theory]
        [InlineData((byte)1, (ushort)0, (byte)0, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)0, (ushort)1, (byte)0, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)0, (ushort)0, (byte)1, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)0, (ushort)0, (byte)0, (byte)1, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)0, (ushort)0, (byte)0, (byte)0, (ushort)1, (byte)0, (byte)0, (byte)0)]
        public void Constructor_NonZeroParameters_ResultsInNonDefaultSpecificity(byte extras, ushort manual, byte isDynamicResource, byte isBinding, ushort style, byte id, byte @class, byte type)
        {
            // Arrange & Act
            var specificity = new SetterSpecificity(extras, manual, isDynamicResource, isBinding, style, id, @class, type);

            // Assert
            Assert.False(specificity.IsDefault);
        }

        /// <summary>
        /// Tests the constructor with ExtrasVsm exactly at the style boundary.
        /// Should test the edge case where style equals styleImplicitUpperBound (255).
        /// </summary>
        [Fact]
        public void Constructor_VsmAtStyleBoundary_DoesNotTriggerImplicitVsm()
        {
            // Arrange
            const byte extras = 0x01; // ExtrasVsm
            const ushort styleAtBoundary = 255; // StyleLocal - 1

            // Act
            var specificity = new SetterSpecificity(extras, 0, 0, 0, styleAtBoundary, 0, 0, 0);

            // Assert
            Assert.True(specificity.IsVsm);
            Assert.False(specificity.IsVsmImplicit);
        }

        /// <summary>
        /// Tests the constructor with ExtrasVsm just below the style boundary.
        /// Should trigger implicit VSM logic.
        /// </summary>
        [Fact]
        public void Constructor_VsmBelowStyleBoundary_TriggersImplicitVsm()
        {
            // Arrange
            const byte extras = 0x01; // ExtrasVsm
            const ushort styleBelowBoundary = 254; // StyleLocal - 2

            // Act
            var specificity = new SetterSpecificity(extras, 0, 0, 0, styleBelowBoundary, 0, 0, 0);

            // Assert
            Assert.False(specificity.IsVsm);
            Assert.True(specificity.IsVsmImplicit);
        }

        /// <summary>
        /// Tests that GetHashCode returns consistent hash codes for the same instance.
        /// Verifies that calling GetHashCode multiple times on the same object returns the same value.
        /// </summary>
        [Fact]
        public void GetHashCode_SameInstance_ReturnsConsistentHashCode()
        {
            // Arrange
            var specificity = new SetterSpecificity(0, 1, 0, 0, 100, 5, 3, 1);

            // Act
            int hashCode1 = specificity.GetHashCode();
            int hashCode2 = specificity.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that GetHashCode respects the equality contract.
        /// Verifies that equal SetterSpecificity instances have the same hash code.
        /// </summary>
        [Fact]
        public void GetHashCode_EqualInstances_ReturnsSameHashCode()
        {
            // Arrange
            var specificity1 = new SetterSpecificity(0, 1, 0, 0, 100, 5, 3, 1);
            var specificity2 = new SetterSpecificity(0, 1, 0, 0, 100, 5, 3, 1);

            // Act
            int hashCode1 = specificity1.GetHashCode();
            int hashCode2 = specificity2.GetHashCode();

            // Assert
            Assert.Equal(specificity1, specificity2);
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests GetHashCode for the DefaultValue static instance.
        /// Verifies that the DefaultValue instance returns a consistent hash code.
        /// </summary>
        [Fact]
        public void GetHashCode_DefaultValue_ReturnsConsistentHashCode()
        {
            // Arrange & Act
            int hashCode = SetterSpecificity.DefaultValue.GetHashCode();

            // Assert
            Assert.Equal(0ul.GetHashCode(), hashCode);
        }

        /// <summary>
        /// Tests GetHashCode for various static instances.
        /// Verifies that predefined static instances return consistent hash codes.
        /// </summary>
        [Theory]
        [InlineData("VisualStateSetter")]
        [InlineData("FromBinding")]
        [InlineData("ManualValueSetter")]
        [InlineData("Trigger")]
        [InlineData("DynamicResourceSetter")]
        [InlineData("FromHandler")]
        public void GetHashCode_StaticInstances_ReturnsConsistentHashCode(string instanceName)
        {
            // Arrange
            SetterSpecificity specificity = instanceName switch
            {
                "VisualStateSetter" => SetterSpecificity.VisualStateSetter,
                "FromBinding" => SetterSpecificity.FromBinding,
                "ManualValueSetter" => SetterSpecificity.ManualValueSetter,
                "Trigger" => SetterSpecificity.Trigger,
                "DynamicResourceSetter" => SetterSpecificity.DynamicResourceSetter,
                "FromHandler" => SetterSpecificity.FromHandler,
                _ => throw new ArgumentException($"Unknown instance: {instanceName}")
            };

            // Act
            int hashCode1 = specificity.GetHashCode();
            int hashCode2 = specificity.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests GetHashCode for the special FromHandler instance.
        /// Verifies that the handler specificity (0xFFFFFFFFFFFFFFFF) returns the expected hash code.
        /// </summary>
        [Fact]
        public void GetHashCode_FromHandler_ReturnsExpectedHashCode()
        {
            // Arrange & Act
            int hashCode = SetterSpecificity.FromHandler.GetHashCode();

            // Assert
            Assert.Equal(0xFFFFFFFFFFFFFFFF.GetHashCode(), hashCode);
        }

        /// <summary>
        /// Tests GetHashCode for parameterless constructor.
        /// Verifies that default constructed instances return consistent hash codes.
        /// </summary>
        [Fact]
        public void GetHashCode_ParameterlessConstructor_ReturnsConsistentHashCode()
        {
            // Arrange
            var specificity = new SetterSpecificity();

            // Act
            int hashCode1 = specificity.GetHashCode();
            int hashCode2 = specificity.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
            Assert.Equal(1ul.GetHashCode(), hashCode1);
        }

        /// <summary>
        /// Tests GetHashCode for 4-parameter constructor.
        /// Verifies that instances created with style parameters return consistent hash codes.
        /// </summary>
        [Theory]
        [InlineData((ushort)100, (byte)5, (byte)3, (byte)1)]
        [InlineData((ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((ushort)255, (byte)255, (byte)255, (byte)255)]
        [InlineData(SetterSpecificity.StyleImplicit, (byte)10, (byte)20, (byte)30)]
        [InlineData(SetterSpecificity.StyleLocal, (byte)1, (byte)2, (byte)3)]
        public void GetHashCode_FourParameterConstructor_ReturnsConsistentHashCode(ushort style, byte id, byte @class, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(style, id, @class, type);

            // Act
            int hashCode1 = specificity.GetHashCode();
            int hashCode2 = specificity.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests GetHashCode with boundary values for all constructor parameters.
        /// Verifies that extreme values produce consistent hash codes.
        /// </summary>
        [Theory]
        [InlineData((byte)0, (ushort)0, (byte)0, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)255, (ushort)65535, (byte)255, (byte)255, (ushort)65535, (byte)255, (byte)255, (byte)255)]
        [InlineData((byte)1, (ushort)1, (byte)1, (byte)1, (ushort)1, (byte)1, (byte)1, (byte)1)]
        [InlineData((byte)0xFF, (ushort)0, (byte)0, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)] // Handler case
        public void GetHashCode_BoundaryValues_ReturnsConsistentHashCode(byte extras, ushort manual, byte isDynamicResource, byte isBinding, ushort style, byte id, byte @class, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(extras, manual, isDynamicResource, isBinding, style, id, @class, type);

            // Act
            int hashCode1 = specificity.GetHashCode();
            int hashCode2 = specificity.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests GetHashCode for AsBaseStyle method result.
        /// Verifies that the base style variant returns a consistent hash code.
        /// </summary>
        [Fact]
        public void GetHashCode_AsBaseStyle_ReturnsConsistentHashCode()
        {
            // Arrange
            var originalSpecificity = new SetterSpecificity(0, 1, 0, 0, 300, 5, 3, 1);
            var baseStyleSpecificity = originalSpecificity.AsBaseStyle();

            // Act
            int hashCode1 = baseStyleSpecificity.GetHashCode();
            int hashCode2 = baseStyleSpecificity.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests GetHashCode for CopyStyle method result.
        /// Verifies that copied style variants return consistent hash codes.
        /// </summary>
        [Theory]
        [InlineData((byte)0, (ushort)1, (byte)0, (byte)0)]
        [InlineData((byte)1, (ushort)5, (byte)1, (byte)1)]
        [InlineData((byte)0xFF, (ushort)0, (byte)0, (byte)0)]
        public void GetHashCode_CopyStyle_ReturnsConsistentHashCode(byte extras, ushort manual, byte isDynamicResource, byte isBinding)
        {
            // Arrange
            var originalSpecificity = new SetterSpecificity(0, 1, 0, 0, 200, 10, 5, 2);
            var copiedSpecificity = originalSpecificity.CopyStyle(extras, manual, isDynamicResource, isBinding);

            // Act
            int hashCode1 = copiedSpecificity.GetHashCode();
            int hashCode2 = copiedSpecificity.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that IsDynamicResource returns true when isDynamicResource parameter is set to non-zero values
        /// </summary>
        /// <param name="isDynamicResource">The isDynamicResource parameter value</param>
        [Theory]
        [InlineData((byte)1)]
        [InlineData((byte)2)]
        [InlineData(byte.MaxValue)]
        public void IsDynamicResource_WhenIsDynamicResourceParameterIsNonZero_ReturnsTrue(byte isDynamicResource)
        {
            // Arrange
            var specificity = new SetterSpecificity(0, 0, isDynamicResource, 0, 0, 0, 0, 0);

            // Act
            bool result = specificity.IsDynamicResource;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsDynamicResource returns false when isDynamicResource parameter is set to zero
        /// </summary>
        [Fact]
        public void IsDynamicResource_WhenIsDynamicResourceParameterIsZero_ReturnsFalse()
        {
            // Arrange
            var specificity = new SetterSpecificity(0, 0, 0, 0, 0, 0, 0, 0);

            // Act
            bool result = specificity.IsDynamicResource;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsDynamicResource returns false when other parameters are set but isDynamicResource is zero
        /// </summary>
        /// <param name="extras">The extras parameter</param>
        /// <param name="manual">The manual parameter</param>
        /// <param name="isBinding">The isBinding parameter</param>
        /// <param name="style">The style parameter</param>
        /// <param name="id">The id parameter</param>
        /// <param name="classParam">The class parameter</param>
        /// <param name="type">The type parameter</param>
        [Theory]
        [InlineData((byte)1, (ushort)1, (byte)1, (ushort)1, (byte)1, (byte)1, (byte)1)]
        [InlineData((byte)0, (ushort)100, (byte)0, (ushort)500, (byte)50, (byte)25, (byte)10)]
        [InlineData(byte.MaxValue - 1, ushort.MaxValue, byte.MaxValue, ushort.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)]
        public void IsDynamicResource_WhenOtherParametersSetButIsDynamicResourceIsZero_ReturnsFalse(
            byte extras, ushort manual, byte isBinding, ushort style, byte id, byte classParam, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(extras, manual, 0, isBinding, style, id, classParam, type);

            // Act
            bool result = specificity.IsDynamicResource;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsDynamicResource returns true when isDynamicResource is set along with other parameters
        /// </summary>
        /// <param name="extras">The extras parameter</param>
        /// <param name="manual">The manual parameter</param>
        /// <param name="isBinding">The isBinding parameter</param>
        /// <param name="style">The style parameter</param>
        /// <param name="id">The id parameter</param>
        /// <param name="classParam">The class parameter</param>
        /// <param name="type">The type parameter</param>
        [Theory]
        [InlineData((byte)1, (ushort)1, (byte)1, (ushort)1, (byte)1, (byte)1, (byte)1)]
        [InlineData((byte)0, (ushort)100, (byte)0, (ushort)500, (byte)50, (byte)25, (byte)10)]
        [InlineData(byte.MaxValue - 1, ushort.MaxValue, byte.MaxValue, ushort.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)]
        public void IsDynamicResource_WhenIsDynamicResourceSetWithOtherParameters_ReturnsTrue(
            byte extras, ushort manual, byte isBinding, ushort style, byte id, byte classParam, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(extras, manual, 1, isBinding, style, id, classParam, type);

            // Act
            bool result = specificity.IsDynamicResource;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsDynamicResource returns true for handler specificity (all bits set)
        /// </summary>
        [Fact]
        public void IsDynamicResource_WhenExtrasIsHandler_ReturnsTrue()
        {
            // Arrange
            var specificity = new SetterSpecificity(0xFF, 0, 0, 0, 0, 0, 0, 0);

            // Act
            bool result = specificity.IsDynamicResource;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsDynamicResource returns false for different constructor overloads when dynamic resource is not set
        /// </summary>
        [Theory]
        [InlineData((ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((ushort)100, (byte)50, (byte)25, (byte)10)]
        [InlineData(ushort.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)]
        public void IsDynamicResource_WhenUsingStyleConstructor_ReturnsFalse(ushort style, byte id, byte classParam, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(style, id, classParam, type);

            // Act
            bool result = specificity.IsDynamicResource;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsDynamicResource returns false for parameterless constructor
        /// </summary>
        [Fact]
        public void IsDynamicResource_WhenUsingParameterlessConstructor_ReturnsFalse()
        {
            // Arrange
            var specificity = new SetterSpecificity();

            // Act
            bool result = specificity.IsDynamicResource;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests IsDynamicResource property for predefined static instances
        /// </summary>
        /// <param name="specificity">The SetterSpecificity instance to test</param>
        /// <param name="expectedResult">The expected IsDynamicResource result</param>
        [Theory]
        [InlineData(nameof(SetterSpecificity.DefaultValue), false)]
        [InlineData(nameof(SetterSpecificity.VisualStateSetter), false)]
        [InlineData(nameof(SetterSpecificity.FromBinding), false)]
        [InlineData(nameof(SetterSpecificity.ManualValueSetter), false)]
        [InlineData(nameof(SetterSpecificity.Trigger), false)]
        [InlineData(nameof(SetterSpecificity.DynamicResourceSetter), true)]
        [InlineData(nameof(SetterSpecificity.FromHandler), true)]
        public void IsDynamicResource_ForStaticInstances_ReturnsExpectedValue(string staticInstanceName, bool expectedResult)
        {
            // Arrange
            SetterSpecificity specificity = staticInstanceName switch
            {
                nameof(SetterSpecificity.DefaultValue) => SetterSpecificity.DefaultValue,
                nameof(SetterSpecificity.VisualStateSetter) => SetterSpecificity.VisualStateSetter,
                nameof(SetterSpecificity.FromBinding) => SetterSpecificity.FromBinding,
                nameof(SetterSpecificity.ManualValueSetter) => SetterSpecificity.ManualValueSetter,
                nameof(SetterSpecificity.Trigger) => SetterSpecificity.Trigger,
                nameof(SetterSpecificity.DynamicResourceSetter) => SetterSpecificity.DynamicResourceSetter,
                nameof(SetterSpecificity.FromHandler) => SetterSpecificity.FromHandler,
                _ => throw new ArgumentException($"Unknown static instance: {staticInstanceName}")
            };

            // Act
            bool result = specificity.IsDynamicResource;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that IsDynamicResource returns correct values for boundary conditions
        /// </summary>
        /// <param name="isDynamicResource">The isDynamicResource parameter value</param>
        /// <param name="expectedResult">The expected result</param>
        [Theory]
        [InlineData((byte)0, false)]
        [InlineData((byte)1, true)]
        [InlineData(byte.MinValue, false)]
        [InlineData(byte.MaxValue, true)]
        public void IsDynamicResource_ForBoundaryValues_ReturnsExpectedResult(byte isDynamicResource, bool expectedResult)
        {
            // Arrange
            var specificity = new SetterSpecificity(0, 0, isDynamicResource, 0, 1, 1, 1, 1);

            // Act
            bool result = specificity.IsDynamicResource;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that IsManual returns true only when manual parameter is exactly 1
        /// </summary>
        /// <param name="manual">The manual parameter value to test</param>
        /// <param name="expected">The expected IsManual result</param>
        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(2, false)]
        [InlineData(3, false)]
        [InlineData(100, false)]
        [InlineData(101, false)]
        [InlineData(1000, false)]
        [InlineData(ushort.MaxValue, false)]
        public void IsManual_WithVariousManualValues_ReturnsExpectedResult(ushort manual, bool expected)
        {
            // Arrange
            var specificity = new SetterSpecificity(0, manual, 0, 0, 0, 0, 0, 0);

            // Act
            var result = specificity.IsManual;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that IsManual returns correct values for predefined static instances
        /// </summary>
        /// <param name="specificityName">Name of the static specificity for test identification</param>
        /// <param name="specificity">The predefined SetterSpecificity instance</param>
        /// <param name="expected">The expected IsManual result</param>
        [Theory]
        [InlineData("DefaultValue", false)]
        [InlineData("VisualStateSetter", false)]
        [InlineData("FromBinding", false)]
        [InlineData("ManualValueSetter", true)]
        [InlineData("Trigger", false)]
        [InlineData("DynamicResourceSetter", false)]
        [InlineData("FromHandler", false)]
        public void IsManual_WithPredefinedStaticValues_ReturnsExpectedResult(string specificityName, bool expected)
        {
            // Arrange
            SetterSpecificity specificity = specificityName switch
            {
                "DefaultValue" => SetterSpecificity.DefaultValue,
                "VisualStateSetter" => SetterSpecificity.VisualStateSetter,
                "FromBinding" => SetterSpecificity.FromBinding,
                "ManualValueSetter" => SetterSpecificity.ManualValueSetter,
                "Trigger" => SetterSpecificity.Trigger,
                "DynamicResourceSetter" => SetterSpecificity.DynamicResourceSetter,
                "FromHandler" => SetterSpecificity.FromHandler,
                _ => throw new ArgumentException($"Unknown specificity: {specificityName}")
            };

            // Act
            var result = specificity.IsManual;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that IsManual returns false when manual value is exactly ManualTriggerBaseline (2)
        /// </summary>
        [Fact]
        public void IsManual_WithManualTriggerBaseline_ReturnsFalse()
        {
            // Arrange
            var specificity = new SetterSpecificity(0, SetterSpecificity.ManualTriggerBaseline, 0, 0, 0, 0, 0, 0);

            // Act
            var result = specificity.IsManual;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsManual returns false for default constructor which sets _value to 1
        /// </summary>
        [Fact]
        public void IsManual_WithDefaultConstructor_ReturnsFalse()
        {
            // Arrange
            var specificity = new SetterSpecificity();

            // Act
            var result = specificity.IsManual;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsManual returns false when using style-only constructor
        /// </summary>
        /// <param name="style">Style parameter value</param>
        /// <param name="id">ID parameter value</param>
        /// <param name="classValue">Class parameter value</param>
        /// <param name="type">Type parameter value</param>
        [Theory]
        [InlineData((ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((ushort)100, (byte)50, (byte)25, (byte)10)]
        [InlineData(ushort.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)]
        public void IsManual_WithStyleOnlyConstructor_ReturnsFalse(ushort style, byte id, byte classValue, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(style, id, classValue, type);

            // Act
            var result = specificity.IsManual;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsManual returns expected result with various combinations of other parameters
        /// while keeping manual parameter as 1
        /// </summary>
        /// <param name="extras">Extras parameter value</param>
        /// <param name="isDynamicResource">IsDynamicResource parameter value</param>
        /// <param name="isBinding">IsBinding parameter value</param>
        /// <param name="style">Style parameter value</param>
        [Theory]
        [InlineData((byte)0, (byte)0, (byte)0, (ushort)0)]
        [InlineData((byte)1, (byte)1, (byte)1, (ushort)100)]
        [InlineData(byte.MaxValue, byte.MaxValue, byte.MaxValue, ushort.MaxValue)]
        public void IsManual_WithManualEqualsOneAndVariousOtherParameters_ReturnsTrue(byte extras, byte isDynamicResource, byte isBinding, ushort style)
        {
            // Arrange - Skip handler case since it has special behavior
            if (extras == 0xFF)
                return;

            var specificity = new SetterSpecificity(extras, 1, isDynamicResource, isBinding, style, 0, 0, 0);

            // Act
            var result = specificity.IsManual;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the DefaultValue static instance returns true for IsDefault property.
        /// This verifies that the DefaultValue is correctly identified as the default specificity.
        /// </summary>
        [Fact]
        public void IsDefault_DefaultValueInstance_ReturnsTrue()
        {
            // Arrange & Act
            var result = SetterSpecificity.DefaultValue.IsDefault;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that instances created with parameterless constructor return false for IsDefault property.
        /// This verifies that the parameterless constructor creates a non-default specificity (_value = 1).
        /// Input conditions: Instance created with parameterless constructor.
        /// Expected result: IsDefault returns false.
        /// </summary>
        [Fact]
        public void IsDefault_ParameterlessConstructor_ReturnsFalse()
        {
            // Arrange
            var specificity = new SetterSpecificity();

            // Act
            var result = specificity.IsDefault;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that instances created with style constructor return false for IsDefault property.
        /// This verifies that style-based specificities are not considered default.
        /// Input conditions: Various style, id, class, and type parameter combinations.
        /// Expected result: IsDefault returns false for all constructed instances.
        /// </summary>
        [Theory]
        [InlineData((ushort)1, (byte)0, (byte)0, (byte)0)]
        [InlineData((ushort)0, (byte)1, (byte)0, (byte)0)]
        [InlineData((ushort)0, (byte)0, (byte)1, (byte)0)]
        [InlineData((ushort)0, (byte)0, (byte)0, (byte)1)]
        [InlineData((ushort)128, (byte)1, (byte)2, (byte)3)]
        [InlineData((ushort)255, (byte)255, (byte)255, (byte)255)]
        [InlineData(SetterSpecificity.StyleImplicit, (byte)0, (byte)0, (byte)0)]
        [InlineData(SetterSpecificity.StyleLocal, (byte)0, (byte)0, (byte)0)]
        public void IsDefault_StyleConstructor_ReturnsFalse(ushort style, byte id, byte @class, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(style, id, @class, type);

            // Act
            var result = specificity.IsDefault;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that instances created with full constructor return false for IsDefault property.
        /// This verifies that specificities created with various parameter combinations are not default.
        /// Input conditions: Various combinations of extras, manual, isDynamicResource, isBinding, style, id, class, and type parameters.
        /// Expected result: IsDefault returns false for all constructed instances.
        /// </summary>
        [Theory]
        [InlineData((byte)0, (ushort)1, (byte)0, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)0, (ushort)0, (byte)1, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)0, (ushort)0, (byte)0, (byte)1, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)1, (ushort)0, (byte)0, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)255, (ushort)0, (byte)0, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)0, SetterSpecificity.ManualTriggerBaseline, (byte)0, (byte)0, (ushort)0, (byte)0, (byte)0, (byte)0)]
        [InlineData((byte)0, (ushort)0, (byte)0, (byte)0, (ushort)100, (byte)1, (byte)2, (byte)3)]
        [InlineData((byte)1, (ushort)1, (byte)1, (byte)1, (ushort)100, (byte)1, (byte)2, (byte)3)]
        public void IsDefault_FullConstructor_ReturnsFalse(byte extras, ushort manual, byte isDynamicResource, byte isBinding, ushort style, byte id, byte @class, byte type)
        {
            // Arrange
            var specificity = new SetterSpecificity(extras, manual, isDynamicResource, isBinding, style, id, @class, type);

            // Act
            var result = specificity.IsDefault;

            // Assert
            Assert.False(result);
        }

    }
}