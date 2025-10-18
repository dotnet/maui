#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class ShellAppearanceTests : ShellTestBase
    {
        [Fact]
        public void ColorSetCorrectly()
        {
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            testShell.Items[0].SetValue(Shell.DisabledColorProperty, Colors.Purple);

            ShellAppearance result = new ShellAppearance();
            result.Ingest(testShell.Items[0]);
            Assert.Equal(Colors.Purple, result.DisabledColor);
        }

        /// <summary>
        /// Tests that the FlyoutBackdrop property returns the default brush value when the ShellAppearance is initialized.
        /// This test verifies that the constructor properly initializes the _brushArray[0] element to Brush.Default
        /// and that the FlyoutBackdrop property correctly returns this default value.
        /// </summary>
        [Fact]
        public void FlyoutBackdrop_DefaultInitialization_ReturnsBrushDefault()
        {
            // Arrange
            var appearance = new ShellAppearance();

            // Act
            var flyoutBackdrop = appearance.FlyoutBackdrop;

            // Assert
            Assert.Equal(Brush.Default, flyoutBackdrop);
        }

        /// <summary>
        /// Tests that TabBarUnselectedColor returns the default Color value when no color is set.
        /// Verifies the property correctly accesses _colorArray[7] in its default state.
        /// Expected result: Should return default Color value (null for Color struct).
        /// </summary>
        [Fact]
        public void TabBarUnselectedColor_DefaultValue_ReturnsDefaultColor()
        {
            // Arrange
            var appearance = new ShellAppearance();

            // Act
            var result = appearance.TabBarUnselectedColor;

            // Assert
            Assert.Equal(default(Color), result);
        }

        /// <summary>
        /// Tests that TabBarUnselectedColor returns the correct color after ingesting from an element with TabBarUnselectedColorProperty set.
        /// Verifies the property correctly reflects the ingested value from _colorArray[7].
        /// Expected result: Should return the color that was set on the shell element.
        /// </summary>
        [Theory]
        [InlineData(255, 0, 0, 255)] // Red
        [InlineData(0, 255, 0, 255)] // Green  
        [InlineData(0, 0, 255, 255)] // Blue
        [InlineData(255, 255, 0, 255)] // Yellow
        [InlineData(128, 128, 128, 255)] // Gray
        [InlineData(0, 0, 0, 255)] // Black
        [InlineData(255, 255, 255, 255)] // White
        public void TabBarUnselectedColor_AfterIngest_ReturnsSetColor(byte red, byte green, byte blue, byte alpha)
        {
            // Arrange
            var expectedColor = Color.FromRgba(red, green, blue, alpha);
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            testShell.Items[0].SetValue(Shell.TabBarUnselectedColorProperty, expectedColor);
            var appearance = new ShellAppearance();

            // Act
            appearance.Ingest(testShell.Items[0]);
            var result = appearance.TabBarUnselectedColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that TabBarUnselectedColor returns the correct predefined color values.
        /// Verifies the property works with common predefined Color values.
        /// Expected result: Should return the exact color that was set.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetPredefinedColors))]
        public void TabBarUnselectedColor_WithPredefinedColors_ReturnsSetColor(Color expectedColor)
        {
            // Arrange
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            testShell.Items[0].SetValue(Shell.TabBarUnselectedColorProperty, expectedColor);
            var appearance = new ShellAppearance();

            // Act
            appearance.Ingest(testShell.Items[0]);
            var result = appearance.TabBarUnselectedColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that TabBarUnselectedColor remains default when Ingest is called with an element that has no TabBarUnselectedColorProperty set.
        /// Verifies the property maintains its default value when no relevant property is ingested.
        /// Expected result: Should return default Color value.
        /// </summary>
        [Fact]
        public void TabBarUnselectedColor_IngestWithoutProperty_RemainsDefault()
        {
            // Arrange
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            // Note: Not setting TabBarUnselectedColorProperty
            var appearance = new ShellAppearance();

            // Act
            appearance.Ingest(testShell.Items[0]);
            var result = appearance.TabBarUnselectedColor;

            // Assert
            Assert.Equal(default(Color), result);
        }

        public static IEnumerable<object[]> GetPredefinedColors()
        {
            yield return new object[] { Colors.Red };
            yield return new object[] { Colors.Green };
            yield return new object[] { Colors.Blue };
            yield return new object[] { Colors.Yellow };
            yield return new object[] { Colors.Purple };
            yield return new object[] { Colors.Orange };
            yield return new object[] { Colors.Pink };
            yield return new object[] { Colors.Brown };
            yield return new object[] { Colors.Gray };
            yield return new object[] { Colors.Black };
            yield return new object[] { Colors.White };
            yield return new object[] { Colors.Transparent };
        }

        /// <summary>
        /// Tests that UnselectedColor returns the default color value when no ingestion has occurred.
        /// Verifies the initial state of the UnselectedColor property after construction.
        /// Expected result: Returns the default Color value.
        /// </summary>
        [Fact]
        public void UnselectedColor_WhenNotIngested_ReturnsDefaultColor()
        {
            // Arrange
            var shellAppearance = new ShellAppearance();

            // Act
            var result = shellAppearance.UnselectedColor;

            // Assert
            Assert.Equal(default(Color), result);
        }

        /// <summary>
        /// Tests that UnselectedColor returns the correct color after ingesting from an element with UnselectedColorProperty set.
        /// Verifies that the Ingest method properly populates the UnselectedColor property.
        /// Expected result: Returns the color that was set on the shell element.
        /// </summary>
        [Theory]
        [InlineData(255, 0, 0, 255)] // Red
        [InlineData(0, 255, 0, 255)] // Green  
        [InlineData(0, 0, 255, 255)] // Blue
        [InlineData(255, 255, 255, 255)] // White
        [InlineData(0, 0, 0, 255)] // Black
        [InlineData(128, 128, 128, 255)] // Gray
        [InlineData(255, 255, 0, 255)] // Yellow
        [InlineData(255, 0, 255, 255)] // Magenta
        [InlineData(0, 255, 255, 255)] // Cyan
        public void UnselectedColor_AfterIngest_ReturnsSetColor(byte red, byte green, byte blue, byte alpha)
        {
            // Arrange
            var expectedColor = Color.FromRgba(red, green, blue, alpha);
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            testShell.Items[0].SetValue(Shell.UnselectedColorProperty, expectedColor);
            var shellAppearance = new ShellAppearance();

            // Act
            shellAppearance.Ingest(testShell.Items[0]);
            var result = shellAppearance.UnselectedColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that UnselectedColor is not overridden when Ingest is called multiple times.
        /// Verifies that once a color is set, subsequent Ingest calls don't change it.
        /// Expected result: Returns the first color that was ingested.
        /// </summary>
        [Fact]
        public void UnselectedColor_MultipleIngests_DoesNotOverrideExistingColor()
        {
            // Arrange
            var firstColor = Colors.Red;
            var secondColor = Colors.Blue;

            var firstShell = new TestShell(CreateShellItem<FlyoutItem>());
            firstShell.Items[0].SetValue(Shell.UnselectedColorProperty, firstColor);

            var secondShell = new TestShell(CreateShellItem<FlyoutItem>());
            secondShell.Items[0].SetValue(Shell.UnselectedColorProperty, secondColor);

            var shellAppearance = new ShellAppearance();

            // Act
            shellAppearance.Ingest(firstShell.Items[0]);
            shellAppearance.Ingest(secondShell.Items[0]);
            var result = shellAppearance.UnselectedColor;

            // Assert
            Assert.Equal(firstColor, result);
            Assert.NotEqual(secondColor, result);
        }

        /// <summary>
        /// Tests that UnselectedColor remains default when ingesting from element without UnselectedColorProperty set.
        /// Verifies that Ingest doesn't change UnselectedColor when no value is provided.
        /// Expected result: Returns the default Color value.
        /// </summary>
        [Fact]
        public void UnselectedColor_IngestWithoutUnselectedColorSet_RemainsDefault()
        {
            // Arrange
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            // Intentionally not setting Shell.UnselectedColorProperty
            var shellAppearance = new ShellAppearance();

            // Act
            shellAppearance.Ingest(testShell.Items[0]);
            var result = shellAppearance.UnselectedColor;

            // Assert
            Assert.Equal(default(Color), result);
        }

        /// <summary>
        /// Tests UnselectedColor with extreme color values including transparent and edge cases.
        /// Verifies that the property handles all valid color values correctly.
        /// Expected result: Returns the exact color values including transparent and extreme values.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)] // Transparent
        [InlineData(255, 255, 255, 0)] // White but transparent
        [InlineData(1, 2, 3, 4)] // Minimal non-zero values
        [InlineData(254, 253, 252, 251)] // Near-maximum values
        public void UnselectedColor_ExtremeColorValues_HandlesCorrectly(byte red, byte green, byte blue, byte alpha)
        {
            // Arrange
            var extremeColor = Color.FromRgba(red, green, blue, alpha);
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            testShell.Items[0].SetValue(Shell.UnselectedColorProperty, extremeColor);
            var shellAppearance = new ShellAppearance();

            // Act
            shellAppearance.Ingest(testShell.Items[0]);
            var result = shellAppearance.UnselectedColor;

            // Assert
            Assert.Equal(extremeColor, result);
        }

        /// <summary>
        /// Tests that FlyoutWidth property returns the initial default value of -1 before any ingestion occurs.
        /// This verifies the constructor properly initializes the _doubleArray with -1 values.
        /// Expected result: FlyoutWidth should return -1.
        /// </summary>
        [Fact]
        public void FlyoutWidth_InitialValue_ReturnsNegativeOne()
        {
            // Arrange
            var appearance = new ShellAppearance();

            // Act
            var result = appearance.FlyoutWidth;

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that FlyoutWidth property returns the correct value after ingesting from an element with FlyoutWidthProperty set.
        /// This verifies the Ingest method properly populates the _doubleArray and the property returns the correct value.
        /// Expected result: FlyoutWidth should return the ingested value.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(100.0)]
        [InlineData(200.5)]
        [InlineData(-50.0)]
        [InlineData(1000.0)]
        [InlineData(0.1)]
        public void FlyoutWidth_AfterIngestion_ReturnsIngestedValue(double expectedWidth)
        {
            // Arrange
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            testShell.Items[0].SetValue(Shell.FlyoutWidthProperty, expectedWidth);
            var appearance = new ShellAppearance();

            // Act
            appearance.Ingest(testShell.Items[0]);
            var result = appearance.FlyoutWidth;

            // Assert
            Assert.Equal(expectedWidth, result);
        }

        /// <summary>
        /// Tests that FlyoutWidth property handles boundary and special double values correctly.
        /// This verifies edge cases including extreme values and special floating-point constants.
        /// Expected result: FlyoutWidth should return the exact special value that was ingested.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void FlyoutWidth_SpecialDoubleValues_ReturnsCorrectValue(double specialValue)
        {
            // Arrange
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            testShell.Items[0].SetValue(Shell.FlyoutWidthProperty, specialValue);
            var appearance = new ShellAppearance();

            // Act
            appearance.Ingest(testShell.Items[0]);
            var result = appearance.FlyoutWidth;

            // Assert
            if (double.IsNaN(specialValue))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(specialValue, result);
            }
        }

        /// <summary>
        /// Tests that FlyoutWidth property retains the first ingested value and ignores subsequent ingestions.
        /// This verifies the Ingest method's behavior of only setting values when current value is -1.
        /// Expected result: FlyoutWidth should return the first ingested value, not the second.
        /// </summary>
        [Fact]
        public void FlyoutWidth_MultipleIngestions_RetainsFirstValue()
        {
            // Arrange
            var testShell1 = new TestShell(CreateShellItem<FlyoutItem>());
            var testShell2 = new TestShell(CreateShellItem<FlyoutItem>());
            testShell1.Items[0].SetValue(Shell.FlyoutWidthProperty, 150.0);
            testShell2.Items[0].SetValue(Shell.FlyoutWidthProperty, 250.0);
            var appearance = new ShellAppearance();

            // Act
            appearance.Ingest(testShell1.Items[0]);
            appearance.Ingest(testShell2.Items[0]);
            var result = appearance.FlyoutWidth;

            // Assert
            Assert.Equal(150.0, result);
        }

        /// <summary>
        /// Tests that FlyoutWidth property remains -1 when ingesting from an element without FlyoutWidthProperty set.
        /// This verifies the property behavior when no relevant value is available for ingestion.
        /// Expected result: FlyoutWidth should remain -1.
        /// </summary>
        [Fact]
        public void FlyoutWidth_IngestionWithoutProperty_RemainsNegativeOne()
        {
            // Arrange
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            // Note: Not setting Shell.FlyoutWidthProperty
            var appearance = new ShellAppearance();

            // Act
            appearance.Ingest(testShell.Items[0]);
            var result = appearance.FlyoutWidth;

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that TabBarBackgroundColor returns the default Color value when no data has been ingested.
        /// </summary>
        [Fact]
        public void TabBarBackgroundColor_WithoutIngest_ReturnsDefaultColor()
        {
            // Arrange
            var appearance = new ShellAppearance();

            // Act
            var result = appearance.TabBarBackgroundColor;

            // Assert
            Assert.Equal(default(Color), result);
        }

        /// <summary>
        /// Tests that TabBarBackgroundColor returns the correct color value after ingesting data from an element with TabBarBackgroundColorProperty set.
        /// </summary>
        [Theory]
        [InlineData(1.0, 0.0, 0.0, 1.0)] // Red
        [InlineData(0.0, 1.0, 0.0, 1.0)] // Green
        [InlineData(0.0, 0.0, 1.0, 1.0)] // Blue
        [InlineData(0.5, 0.5, 0.5, 0.8)] // Semi-transparent gray
        [InlineData(0.0, 0.0, 0.0, 0.0)] // Transparent
        [InlineData(1.0, 1.0, 1.0, 1.0)] // White
        public void TabBarBackgroundColor_AfterIngestWithColorSet_ReturnsCorrectColor(float red, float green, float blue, float alpha)
        {
            // Arrange
            var expectedColor = new Color(red, green, blue, alpha);
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            testShell.Items[0].SetValue(Shell.TabBarBackgroundColorProperty, expectedColor);
            var appearance = new ShellAppearance();

            // Act
            appearance.Ingest(testShell.Items[0]);
            var result = appearance.TabBarBackgroundColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that TabBarBackgroundColor returns the default Color value when ingesting from an element without TabBarBackgroundColorProperty set.
        /// </summary>
        [Fact]
        public void TabBarBackgroundColor_AfterIngestWithoutColorSet_ReturnsDefaultColor()
        {
            // Arrange
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            var appearance = new ShellAppearance();

            // Act
            appearance.Ingest(testShell.Items[0]);
            var result = appearance.TabBarBackgroundColor;

            // Assert
            Assert.Equal(default(Color), result);
        }

        /// <summary>
        /// Tests that TabBarBackgroundColor is independent of other color properties and returns the correct value when multiple colors are set.
        /// </summary>
        [Fact]
        public void TabBarBackgroundColor_WithMultipleColorsSet_ReturnsOnlyTabBarBackgroundColor()
        {
            // Arrange
            var tabBarBackgroundColor = Colors.Purple;
            var backgroundColor = Colors.Red;
            var foregroundColor = Colors.Blue;
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            testShell.Items[0].SetValue(Shell.TabBarBackgroundColorProperty, tabBarBackgroundColor);
            testShell.Items[0].SetValue(Shell.BackgroundColorProperty, backgroundColor);
            testShell.Items[0].SetValue(Shell.ForegroundColorProperty, foregroundColor);
            var appearance = new ShellAppearance();

            // Act
            appearance.Ingest(testShell.Items[0]);
            var result = appearance.TabBarBackgroundColor;

            // Assert
            Assert.Equal(tabBarBackgroundColor, result);
            Assert.NotEqual(backgroundColor, result);
            Assert.NotEqual(foregroundColor, result);
        }
    }

    public partial class ShellAppearanceTabBarDisabledColorTests : ShellTestBase
    {
        /// <summary>
        /// Tests that TabBarDisabledColor returns default Color value when no color has been set.
        /// Verifies the property accesses the correct array index and returns the default value.
        /// Expected result: Default Color (typically black with zero alpha).
        /// </summary>
        [Fact]
        public void TabBarDisabledColor_DefaultState_ReturnsDefaultColor()
        {
            // Arrange
            var shellAppearance = new ShellAppearance();

            // Act
            var result = shellAppearance.TabBarDisabledColor;

            // Assert
            Assert.Equal(default(Color), result);
        }

        /// <summary>
        /// Tests that TabBarDisabledColor returns the correct color after ingesting an element with TabBarDisabledColor set.
        /// Verifies the Ingest method properly populates the color array and the property returns the ingested value.
        /// Expected result: The color that was set on the shell element.
        /// </summary>
        [Theory]
        [InlineData(0.5f, 0.3f, 0.8f, 1.0f)]
        [InlineData(1.0f, 0.0f, 0.0f, 0.5f)]
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)]
        [InlineData(0.0f, 0.0f, 1.0f, 0.8f)]
        public void TabBarDisabledColor_AfterIngest_ReturnsIngestedColor(float red, float green, float blue, float alpha)
        {
            // Arrange
            var testColor = new Color(red, green, blue, alpha);
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            testShell.Items[0].SetValue(Shell.TabBarDisabledColorProperty, testColor);
            var shellAppearance = new ShellAppearance();

            // Act
            var ingestResult = shellAppearance.Ingest(testShell.Items[0]);
            var result = shellAppearance.TabBarDisabledColor;

            // Assert
            Assert.True(ingestResult);
            Assert.Equal(testColor, result);
        }

        /// <summary>
        /// Tests that TabBarDisabledColor returns predefined color values correctly after ingestion.
        /// Verifies the property works with common predefined colors from the Colors class.
        /// Expected result: The predefined color that was set on the shell element.
        /// </summary>
        [Theory]
        [InlineData("Purple")]
        [InlineData("Red")]
        [InlineData("Blue")]
        [InlineData("Green")]
        [InlineData("Transparent")]
        public void TabBarDisabledColor_WithPredefinedColors_ReturnsCorrectColor(string colorName)
        {
            // Arrange
            var testColor = GetColorByName(colorName);
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            testShell.Items[0].SetValue(Shell.TabBarDisabledColorProperty, testColor);
            var shellAppearance = new ShellAppearance();

            // Act
            var ingestResult = shellAppearance.Ingest(testShell.Items[0]);
            var result = shellAppearance.TabBarDisabledColor;

            // Assert
            Assert.True(ingestResult);
            Assert.Equal(testColor, result);
        }

        /// <summary>
        /// Tests that TabBarDisabledColor maintains its value across multiple property accesses.
        /// Verifies the property getter is stable and doesn't modify the underlying array.
        /// Expected result: Same color value returned on multiple calls.
        /// </summary>
        [Fact]
        public void TabBarDisabledColor_MultipleAccess_ReturnsSameValue()
        {
            // Arrange
            var testColor = Colors.Orange;
            var testShell = new TestShell(CreateShellItem<FlyoutItem>());
            testShell.Items[0].SetValue(Shell.TabBarDisabledColorProperty, testColor);
            var shellAppearance = new ShellAppearance();
            shellAppearance.Ingest(testShell.Items[0]);

            // Act
            var result1 = shellAppearance.TabBarDisabledColor;
            var result2 = shellAppearance.TabBarDisabledColor;
            var result3 = shellAppearance.TabBarDisabledColor;

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
            Assert.Equal(testColor, result1);
        }

        /// <summary>
        /// Tests TabBarDisabledColor with null element passed to Ingest method.
        /// Verifies the property behavior when Ingest is called with null.
        /// Expected result: Default color value maintained.
        /// </summary>
        [Fact]
        public void TabBarDisabledColor_IngestWithNull_RetainsDefaultValue()
        {
            // Arrange
            var shellAppearance = new ShellAppearance();
            var originalValue = shellAppearance.TabBarDisabledColor;

            // Act
            var ingestResult = shellAppearance.Ingest(null);
            var result = shellAppearance.TabBarDisabledColor;

            // Assert
            Assert.False(ingestResult);
            Assert.Equal(originalValue, result);
        }

        private static Color GetColorByName(string colorName)
        {
            return colorName switch
            {
                "Purple" => Colors.Purple,
                "Red" => Colors.Red,
                "Blue" => Colors.Blue,
                "Green" => Colors.Green,
                "Transparent" => Colors.Transparent,
                _ => Colors.Black
            };
        }
    }
}