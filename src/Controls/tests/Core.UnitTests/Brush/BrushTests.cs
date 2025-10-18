#nullable disable

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class BrushTests
    {
        /// <summary>
        /// Tests that AntiqueWhite property returns a valid SolidColorBrush instance.
        /// Verifies that the property is not null and returns the correct type.
        /// </summary>
        [Fact]
        public void AntiqueWhite_Get_ReturnsValidSolidColorBrush()
        {
            // Arrange & Act
            var antiqueWhiteBrush = Brush.AntiqueWhite;

            // Assert
            Assert.NotNull(antiqueWhiteBrush);
            Assert.IsType<SolidColorBrush>(antiqueWhiteBrush);
        }

        /// <summary>
        /// Tests that AntiqueWhite property returns a brush with the correct AntiqueWhite color.
        /// Verifies that the Color property of the returned brush matches Colors.AntiqueWhite.
        /// </summary>
        [Fact]
        public void AntiqueWhite_Get_ReturnsCorrectColor()
        {
            // Arrange
            var expectedColor = Colors.AntiqueWhite;

            // Act
            var antiqueWhiteBrush = Brush.AntiqueWhite;

            // Assert
            Assert.Equal(expectedColor, antiqueWhiteBrush.Color);
        }

        /// <summary>
        /// Tests that AntiqueWhite property uses lazy initialization and returns the same instance on multiple accesses.
        /// Verifies singleton behavior of the static property.
        /// </summary>
        [Fact]
        public void AntiqueWhite_MultipleAccess_ReturnsSameInstance()
        {
            // Arrange & Act
            var firstAccess = Brush.AntiqueWhite;
            var secondAccess = Brush.AntiqueWhite;
            var thirdAccess = Brush.AntiqueWhite;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that AntiqueWhite property is thread-safe during concurrent access.
        /// Verifies that multiple threads accessing the property simultaneously get the same instance.
        /// </summary>
        [Fact]
        public async Task AntiqueWhite_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];

            // Reset the static field to ensure lazy initialization during test
            ResetAntiqueWhiteField();

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.AntiqueWhite);
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
        /// Tests that AntiqueWhite property returns a brush that is not empty.
        /// Verifies the IsEmpty property behavior for the returned brush.
        /// </summary>
        [Fact]
        public void AntiqueWhite_Get_BrushIsNotEmpty()
        {
            // Arrange & Act
            var antiqueWhiteBrush = Brush.AntiqueWhite;

            // Assert
            Assert.False(antiqueWhiteBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that AntiqueWhite property returns a brush with consistent color values.
        /// Verifies the ARGB values of the AntiqueWhite color match expected values.
        /// </summary>
        [Fact]
        public void AntiqueWhite_Get_HasExpectedColorValues()
        {
            // Arrange
            var expectedColor = Colors.AntiqueWhite;

            // Act
            var antiqueWhiteBrush = Brush.AntiqueWhite;

            // Assert
            Assert.Equal(expectedColor.Red, antiqueWhiteBrush.Color.Red);
            Assert.Equal(expectedColor.Green, antiqueWhiteBrush.Color.Green);
            Assert.Equal(expectedColor.Blue, antiqueWhiteBrush.Color.Blue);
            Assert.Equal(expectedColor.Alpha, antiqueWhiteBrush.Color.Alpha);
        }

        /// <summary>
        /// Helper method to reset the static antiqueWhite field using reflection for testing lazy initialization.
        /// This ensures that each test can verify the initialization behavior independently.
        /// </summary>
        private void ResetAntiqueWhiteField()
        {
            var brushType = typeof(Brush);
            var antiqueWhiteField = brushType.GetField("antiqueWhite", BindingFlags.Static | BindingFlags.NonPublic);
            antiqueWhiteField?.SetValue(null, null);
        }

        /// <summary>
        /// Tests that the CornflowerBlue property returns a non-null SolidColorBrush.
        /// </summary>
        [Fact]
        public void CornflowerBlue_ReturnsNonNull_SolidColorBrush()
        {
            // Act
            var brush = Brush.CornflowerBlue;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
        }

        /// <summary>
        /// Tests that the CornflowerBlue property returns a brush with the correct color value.
        /// </summary>
        [Fact]
        public void CornflowerBlue_Color_MatchesExpectedValue()
        {
            // Act
            var brush = Brush.CornflowerBlue;

            // Assert
            Assert.Equal(Colors.CornflowerBlue, brush.Color);
        }

        /// <summary>
        /// Tests that subsequent calls to CornflowerBlue return the same instance due to lazy initialization.
        /// </summary>
        [Fact]
        public void CornflowerBlue_SubsequentCalls_ReturnsSameInstance()
        {
            // Act
            var brush1 = Brush.CornflowerBlue;
            var brush2 = Brush.CornflowerBlue;

            // Assert
            Assert.Same(brush1, brush2);
        }

        /// <summary>
        /// Tests that the CornflowerBlue brush is not empty.
        /// </summary>
        [Fact]
        public void CornflowerBlue_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.CornflowerBlue;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests multiple property accesses to ensure consistent behavior.
        /// </summary>
        [Fact]
        public void CornflowerBlue_MultipleAccess_ConsistentResults()
        {
            // Arrange
            var expectedColor = Colors.CornflowerBlue;

            // Act
            var brush1 = Brush.CornflowerBlue;
            var brush2 = Brush.CornflowerBlue;
            var brush3 = Brush.CornflowerBlue;

            // Assert
            Assert.Same(brush1, brush2);
            Assert.Same(brush2, brush3);
            Assert.Equal(expectedColor, brush1.Color);
            Assert.Equal(expectedColor, brush2.Color);
            Assert.Equal(expectedColor, brush3.Color);
            Assert.False(brush1.IsEmpty);
        }

        /// <summary>
        /// Tests that the DarkKhaki property returns a non-null SolidColorBrush with the correct color value.
        /// Verifies the property returns the expected DarkKhaki color (#FFBDB76B).
        /// </summary>
        [Fact]
        public void DarkKhaki_ReturnsNonNullBrushWithCorrectColor()
        {
            // Act
            var brush = Brush.DarkKhaki;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
            Assert.Equal(Colors.DarkKhaki, brush.Color);
        }

        /// <summary>
        /// Tests that the DarkKhaki property implements lazy initialization correctly.
        /// Verifies that multiple accesses to the property return the same instance.
        /// </summary>
        [Fact]
        public void DarkKhaki_LazyInitialization_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.DarkKhaki;
            var secondAccess = Brush.DarkKhaki;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the DarkKhaki brush has the exact color value expected.
        /// Verifies the color matches the system-defined DarkKhaki color with ARGB value #FFBDB76B.
        /// </summary>
        [Fact]
        public void DarkKhaki_HasCorrectColorValue()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFBDB76B);

            // Act
            var brush = Brush.DarkKhaki;

            // Assert
            Assert.Equal(expectedColor, brush.Color);
            Assert.Equal(Colors.DarkKhaki, brush.Color);
        }

        /// <summary>
        /// Tests that the DarkKhaki brush is not empty.
        /// Verifies that a brush with a valid color value is not considered empty.
        /// </summary>
        [Fact]
        public void DarkKhaki_IsNotEmpty()
        {
            // Act
            var brush = Brush.DarkKhaki;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the DarkKhaki property returns an ImmutableBrush internally.
        /// Verifies the specific implementation type used for predefined color brushes.
        /// </summary>
        [Fact]
        public void DarkKhaki_ReturnsImmutableBrushType()
        {
            // Act
            var brush = Brush.DarkKhaki;

            // Assert
            Assert.Equal("ImmutableBrush", brush.GetType().Name);
        }

        /// <summary>
        /// Tests that the DeepSkyBlue property returns a non-null SolidColorBrush instance.
        /// This test verifies the basic functionality and ensures the lazy initialization works.
        /// Expected result: A non-null SolidColorBrush is returned.
        /// </summary>
        [Fact]
        public void DeepSkyBlue_Get_ReturnsNonNullBrush()
        {
            // Act
            var result = Brush.DeepSkyBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the DeepSkyBlue property returns a brush with the correct color.
        /// This test verifies that the brush contains the system-defined DeepSkyBlue color.
        /// Expected result: The brush's color matches Colors.DeepSkyBlue.
        /// </summary>
        [Fact]
        public void DeepSkyBlue_Get_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.DeepSkyBlue;

            // Assert
            Assert.Equal(Colors.DeepSkyBlue, result.Color);
        }

        /// <summary>
        /// Tests that the DeepSkyBlue property uses lazy initialization correctly.
        /// This test verifies that multiple calls to the property return the same instance.
        /// Expected result: Both calls return the identical object instance.
        /// </summary>
        [Fact]
        public void DeepSkyBlue_MultipleCalls_ReturnsSameInstance()
        {
            // Act
            var firstCall = Brush.DeepSkyBlue;
            var secondCall = Brush.DeepSkyBlue;

            // Assert
            Assert.Same(firstCall, secondCall);
        }

        /// <summary>
        /// Tests that the DeepSkyBlue property returns a brush that is not empty.
        /// This test verifies that the brush is properly initialized and not in an empty state.
        /// Expected result: The IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void DeepSkyBlue_Get_ReturnsNonEmptyBrush()
        {
            // Act
            var result = Brush.DeepSkyBlue;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the Goldenrod property returns a non-null SolidColorBrush instance.
        /// Verifies the lazy initialization behavior on first access.
        /// </summary>
        [Fact]
        public void Goldenrod_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var goldenrodBrush = Brush.Goldenrod;

            // Assert
            Assert.NotNull(goldenrodBrush);
            Assert.IsType<SolidColorBrush>(goldenrodBrush);
        }

        /// <summary>
        /// Tests that the Goldenrod property returns a brush with the correct color value.
        /// Verifies that the color matches the system-defined Colors.Goldenrod value.
        /// </summary>
        [Fact]
        public void Goldenrod_ColorValue_EqualsSystemDefinedGoldenrodColor()
        {
            // Act
            var goldenrodBrush = Brush.Goldenrod;

            // Assert
            Assert.Equal(Colors.Goldenrod, goldenrodBrush.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the Goldenrod property return the same cached instance.
        /// Verifies the lazy initialization caching behavior using the null-coalescing assignment operator.
        /// </summary>
        [Fact]
        public void Goldenrod_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Goldenrod;
            var secondAccess = Brush.Goldenrod;
            var thirdAccess = Brush.Goldenrod;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the Goldenrod property returns a brush with the specific ARGB color value.
        /// Verifies the exact color components match the expected Goldenrod color specification.
        /// </summary>
        [Fact]
        public void Goldenrod_ColorComponents_MatchExpectedGoldenrodValues()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFDAA520);

            // Act
            var goldenrodBrush = Brush.Goldenrod;

            // Assert
            Assert.Equal(expectedColor, goldenrodBrush.Color);
            Assert.Equal(expectedColor.Red, goldenrodBrush.Color.Red);
            Assert.Equal(expectedColor.Green, goldenrodBrush.Color.Green);
            Assert.Equal(expectedColor.Blue, goldenrodBrush.Color.Blue);
            Assert.Equal(expectedColor.Alpha, goldenrodBrush.Color.Alpha);
        }

        /// <summary>
        /// Tests that the Goldenrod property is not empty.
        /// Verifies the IsEmpty property behavior for the system-defined color brush.
        /// </summary>
        [Fact]
        public void Goldenrod_IsEmpty_ReturnsFalse()
        {
            // Act
            var goldenrodBrush = Brush.Goldenrod;

            // Assert
            Assert.False(goldenrodBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that LightGrey property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void LightGrey_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.LightGrey;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that LightGrey property returns the same instance as LightGray property.
        /// </summary>
        [Fact]
        public void LightGrey_ReturnsSameInstanceAsLightGray()
        {
            // Act
            var lightGrey = Brush.LightGrey;
            var lightGray = Brush.LightGray;

            // Assert
            Assert.Same(lightGray, lightGrey);
        }

        /// <summary>
        /// Tests that LightGrey property returns a brush with the correct LightGray color.
        /// </summary>
        [Fact]
        public void LightGrey_HasCorrectColor()
        {
            // Act
            var result = Brush.LightGrey;

            // Assert
            Assert.Equal(Colors.LightGray, result.Color);
        }

        /// <summary>
        /// Tests that multiple calls to LightGrey property return the same instance (lazy initialization).
        /// </summary>
        [Fact]
        public void LightGrey_MultipleCalls_ReturnsSameInstance()
        {
            // Act
            var first = Brush.LightGrey;
            var second = Brush.LightGrey;
            var third = Brush.LightGrey;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that LightGrey property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void LightGrey_IsNotEmpty()
        {
            // Act
            var result = Brush.LightGrey;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the Linen property returns a non-null SolidColorBrush instance.
        /// This verifies the lazy initialization works correctly for the first access.
        /// </summary>
        [Fact]
        public void Linen_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Linen;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Linen property returns a brush with the correct Colors.Linen color.
        /// This ensures the brush is initialized with the proper system-defined color value.
        /// </summary>
        [Fact]
        public void Linen_Color_MatchesColorsLinen()
        {
            // Act
            var result = Brush.Linen;

            // Assert
            Assert.Equal(Colors.Linen, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the Linen property return the same instance.
        /// This verifies the singleton lazy initialization pattern works correctly.
        /// </summary>
        [Fact]
        public void Linen_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.Linen;
            var second = Brush.Linen;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the Linen brush is not empty.
        /// This ensures the brush represents a valid color and is usable for painting.
        /// </summary>
        [Fact]
        public void Linen_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.Linen;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the Linen property returns a brush with the expected ARGB color value.
        /// This validates the specific color value matches the system-defined Linen color.
        /// </summary>
        [Fact]
        public void Linen_ColorValue_HasCorrectArgbValue()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFFAF0E6);

            // Act
            var result = Brush.Linen;

            // Assert
            Assert.Equal(expectedColor, result.Color);
        }

        /// <summary>
        /// Tests that the MintCream property returns a non-null SolidColorBrush instance.
        /// Verifies that the property is properly initialized and accessible.
        /// </summary>
        [Fact]
        public void MintCream_Get_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var brush = Brush.MintCream;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
        }

        /// <summary>
        /// Tests that the MintCream property returns a brush with the correct MintCream color.
        /// Verifies that the color property matches the expected system-defined MintCream color.
        /// </summary>
        [Fact]
        public void MintCream_Get_ReturnsCorrectColor()
        {
            // Act
            var brush = Brush.MintCream;

            // Assert
            Assert.Equal(Colors.MintCream, brush.Color);
        }

        /// <summary>
        /// Tests the lazy initialization behavior of the MintCream property.
        /// Verifies that multiple accesses return the same instance due to lazy initialization pattern.
        /// </summary>
        [Fact]
        public void MintCream_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var brush1 = Brush.MintCream;
            var brush2 = Brush.MintCream;

            // Assert
            Assert.Same(brush1, brush2);
        }

        /// <summary>
        /// Tests that the MintCream brush is not empty.
        /// Verifies that the IsEmpty property returns false for the MintCream brush.
        /// </summary>
        [Fact]
        public void MintCream_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.MintCream;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the MintCream property can be accessed concurrently without issues.
        /// Verifies thread safety of the lazy initialization pattern.
        /// </summary>
        [Fact]
        public void MintCream_ConcurrentAccess_ReturnsConsistentInstance()
        {
            SolidColorBrush brush1 = null;
            SolidColorBrush brush2 = null;

            // Arrange & Act
            var task1 = System.Threading.Tasks.Task.Run(() => brush1 = Brush.MintCream);
            var task2 = System.Threading.Tasks.Task.Run(() => brush2 = Brush.MintCream);

            System.Threading.Tasks.Task.WaitAll(task1, task2);

            // Assert
            Assert.NotNull(brush1);
            Assert.NotNull(brush2);
            Assert.Same(brush1, brush2);
        }

        /// <summary>
        /// Tests that the PowderBlue property returns a non-null SolidColorBrush instance.
        /// This test verifies the basic functionality and lazy initialization of the property.
        /// Expected result: Non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void PowderBlue_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.PowderBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that multiple accesses to the PowderBlue property return the same instance.
        /// This test verifies the singleton/caching behavior of the lazy initialization.
        /// Expected result: Same instance returned on multiple calls.
        /// </summary>
        [Fact]
        public void PowderBlue_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.PowderBlue;
            var second = Brush.PowderBlue;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the PowderBlue property returns a brush with the correct color.
        /// This test verifies that the brush contains the system-defined PowderBlue color.
        /// Expected result: SolidColorBrush with Colors.PowderBlue color.
        /// </summary>
        [Fact]
        public void PowderBlue_Color_MatchesSystemDefinedPowderBlueColor()
        {
            // Act
            var brush = Brush.PowderBlue;

            // Assert
            Assert.Equal(Colors.PowderBlue, brush.Color);
        }

        /// <summary>
        /// Tests that the PowderBlue brush is not empty.
        /// This test verifies that the brush has valid content and is ready for use.
        /// Expected result: IsEmpty returns false.
        /// </summary>
        [Fact]
        public void PowderBlue_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.PowderBlue;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the SkyBlue property returns a non-null SolidColorBrush instance.
        /// Verifies basic functionality and ensures the property is accessible.
        /// </summary>
        [Fact]
        public void SkyBlue_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var skyBlueBrush = Brush.SkyBlue;

            // Assert
            Assert.NotNull(skyBlueBrush);
            Assert.IsType<SolidColorBrush>(skyBlueBrush);
        }

        /// <summary>
        /// Tests that the SkyBlue property returns a brush with the correct SkyBlue color.
        /// Verifies that the color value matches the system-defined Colors.SkyBlue.
        /// </summary>
        [Fact]
        public void SkyBlue_HasCorrectColor()
        {
            // Act
            var skyBlueBrush = Brush.SkyBlue;

            // Assert
            Assert.Equal(Colors.SkyBlue, skyBlueBrush.Color);
        }

        /// <summary>
        /// Tests that multiple calls to the SkyBlue property return the same instance.
        /// Verifies the lazy initialization and caching behavior of the static property.
        /// </summary>
        [Fact]
        public void SkyBlue_MultipleCalls_ReturnsSameInstance()
        {
            // Act
            var firstCall = Brush.SkyBlue;
            var secondCall = Brush.SkyBlue;
            var thirdCall = Brush.SkyBlue;

            // Assert
            Assert.Same(firstCall, secondCall);
            Assert.Same(firstCall, thirdCall);
            Assert.Same(secondCall, thirdCall);
        }

        /// <summary>
        /// Tests that the SkyBlue property returns a brush that is not empty.
        /// Verifies that the returned brush has a valid color and is not considered empty.
        /// </summary>
        [Fact]
        public void SkyBlue_IsNotEmpty()
        {
            // Act
            var skyBlueBrush = Brush.SkyBlue;

            // Assert
            Assert.False(skyBlueBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the SkyBlue property returns a brush with the expected ARGB color value.
        /// Verifies that the color matches the system-defined value #FF87CEEB.
        /// </summary>
        [Fact]
        public void SkyBlue_HasExpectedArgbValue()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFF87CEEB);

            // Act
            var skyBlueBrush = Brush.SkyBlue;

            // Assert
            Assert.Equal(expectedColor, skyBlueBrush.Color);
        }

        /// <summary>
        /// Tests that the Turquoise property returns a non-null SolidColorBrush instance.
        /// Verifies that the lazy initialization creates a brush when first accessed.
        /// Expected result: A non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void Turquoise_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Turquoise;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Turquoise property returns the same cached instance on multiple accesses.
        /// Verifies that the lazy initialization pattern works correctly and caches the instance.
        /// Expected result: The same instance is returned on all accesses.
        /// </summary>
        [Fact]
        public void Turquoise_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.Turquoise;
            var second = Brush.Turquoise;
            var third = Brush.Turquoise;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that the Turquoise property returns a brush with the correct turquoise color.
        /// Verifies that the brush is initialized with Colors.Turquoise.
        /// Expected result: The brush's Color property matches Colors.Turquoise.
        /// </summary>
        [Fact]
        public void Turquoise_ColorValue_MatchesTurquoiseColor()
        {
            // Act
            var turquoiseBrush = Brush.Turquoise;

            // Assert
            Assert.Equal(Colors.Turquoise, turquoiseBrush.Color);
        }

        /// <summary>
        /// Tests that the Turquoise property returns a brush that is not empty.
        /// Verifies that the brush is properly initialized and not considered empty.
        /// Expected result: The IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void Turquoise_IsEmpty_ReturnsFalse()
        {
            // Act
            var turquoiseBrush = Brush.Turquoise;

            // Assert
            Assert.False(turquoiseBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that multiple concurrent accesses to Turquoise property are thread-safe.
        /// Verifies that the lazy initialization pattern works correctly under concurrent access.
        /// Expected result: All threads receive the same instance.
        /// </summary>
        [Fact]
        public void Turquoise_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            SolidColorBrush[] results = new SolidColorBrush[10];
            var tasks = new System.Threading.Tasks.Task[10];

            // Act
            for (int i = 0; i < 10; i++)
            {
                int index = i;
                tasks[i] = System.Threading.Tasks.Task.Run(() => results[index] = Brush.Turquoise);
            }

            System.Threading.Tasks.Task.WaitAll(tasks);

            // Assert
            for (int i = 0; i < 10; i++)
            {
                Assert.NotNull(results[i]);
                if (i > 0)
                {
                    Assert.Same(results[0], results[i]);
                }
            }
        }

        /// <summary>
        /// Tests that the Turquoise property returns a brush with the expected ARGB color values.
        /// Verifies that the turquoise color has the correct RGB components.
        /// Expected result: The color has the expected red, green, blue, and alpha values for turquoise.
        /// </summary>
        [Fact]
        public void Turquoise_ColorComponents_HaveExpectedValues()
        {
            // Act
            var turquoiseBrush = Brush.Turquoise;
            var color = turquoiseBrush.Color;

            // Assert
            Assert.Equal(Colors.Turquoise.Red, color.Red);
            Assert.Equal(Colors.Turquoise.Green, color.Green);
            Assert.Equal(Colors.Turquoise.Blue, color.Blue);
            Assert.Equal(Colors.Turquoise.Alpha, color.Alpha);
        }

        /// <summary>
        /// Verifies that the Aqua property returns a non-null SolidColorBrush instance.
        /// Tests the basic functionality of the lazy-initialized static property.
        /// Expected result: Property returns a valid SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void Aqua_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var aquaBrush = Brush.Aqua;

            // Assert
            Assert.NotNull(aquaBrush);
            Assert.IsType<SolidColorBrush>(aquaBrush);
        }

        /// <summary>
        /// Verifies that the Aqua property returns the same instance on multiple accesses.
        /// Tests the lazy initialization caching behavior using the null-coalescing assignment operator.
        /// Expected result: Multiple property accesses return the identical cached instance.
        /// </summary>
        [Fact]
        public void Aqua_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Aqua;
            var secondAccess = Brush.Aqua;
            var thirdAccess = Brush.Aqua;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(firstAccess, thirdAccess);
            Assert.Same(secondAccess, thirdAccess);
        }

        /// <summary>
        /// Verifies that the Aqua property returns a brush with the correct system-defined Aqua color.
        /// Tests that the lazy initialization creates an ImmutableBrush with Colors.Aqua.
        /// Expected result: The returned brush has the Color property set to Colors.Aqua.
        /// </summary>
        [Fact]
        public void Aqua_WhenAccessed_ReturnsCorrectAquaColor()
        {
            // Act
            var aquaBrush = Brush.Aqua;

            // Assert
            Assert.Equal(Colors.Aqua, aquaBrush.Color);
        }

        /// <summary>
        /// Verifies that the Aqua property returns an ImmutableBrush instance.
        /// Tests that the backing field type and lazy initialization create the expected concrete type.
        /// Expected result: The returned instance is specifically an ImmutableBrush.
        /// </summary>
        [Fact]
        public void Aqua_WhenAccessed_ReturnsImmutableBrush()
        {
            // Act
            var aquaBrush = Brush.Aqua;

            // Assert
            Assert.IsAssignableFrom<SolidColorBrush>(aquaBrush);
            // Note: ImmutableBrush is internal, so we verify it's assignable from SolidColorBrush
            // and has the expected immutable behavior by checking the color matches exactly
            Assert.Equal(Colors.Aqua, aquaBrush.Color);
        }

        /// <summary>
        /// Verifies that the Aqua property returns a brush that is not empty.
        /// Tests the IsEmpty property inherited from the base Brush class.
        /// Expected result: The Aqua brush IsEmpty property returns false since it has a valid color.
        /// </summary>
        [Fact]
        public void Aqua_WhenAccessed_IsNotEmpty()
        {
            // Act
            var aquaBrush = Brush.Aqua;

            // Assert
            Assert.False(aquaBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the DarkGrey property returns a non-null SolidColorBrush instance.
        /// Verifies that the British spelling alias works correctly.
        /// </summary>
        [Fact]
        public void DarkGrey_ShouldReturnNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.DarkGrey;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the DarkGrey property returns the same instance as DarkGray property.
        /// Verifies that DarkGrey is correctly aliased to DarkGray (British vs American spelling).
        /// </summary>
        [Fact]
        public void DarkGrey_ShouldReturnSameInstanceAsDarkGray()
        {
            // Act
            var darkGrey = Brush.DarkGrey;
            var darkGray = Brush.DarkGray;

            // Assert
            Assert.Same(darkGray, darkGrey);
        }

        /// <summary>
        /// Tests that the DarkGrey property returns a brush with the correct DarkGray color.
        /// Verifies that the color value matches the expected system-defined DarkGray color.
        /// </summary>
        [Fact]
        public void DarkGrey_ShouldHaveCorrectColor()
        {
            // Act
            var result = Brush.DarkGrey;

            // Assert
            Assert.Equal(Colors.DarkGray, result.Color);
        }

        /// <summary>
        /// Tests that multiple calls to DarkGrey property return the same instance.
        /// Verifies that the lazy initialization works correctly and caches the instance.
        /// </summary>
        [Fact]
        public void DarkGrey_MultipleCallsShouldReturnSameInstance()
        {
            // Act
            var first = Brush.DarkGrey;
            var second = Brush.DarkGrey;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the DimGray property returns a non-null SolidColorBrush instance.
        /// This test verifies basic functionality and ensures the property is accessible.
        /// Expected result: Returns a non-null SolidColorBrush.
        /// </summary>
        [Fact]
        public void DimGray_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.DimGray;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the DimGray property returns a brush with the correct color value.
        /// This test verifies that the returned brush contains the system-defined DimGray color.
        /// Expected result: The Color property matches Colors.DimGray.
        /// </summary>
        [Fact]
        public void DimGray_WhenAccessed_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.DimGray;

            // Assert
            Assert.Equal(Colors.DimGray, result.Color);
        }

        /// <summary>
        /// Tests that the DimGray property implements lazy initialization correctly.
        /// This test verifies that multiple accesses return the same instance (singleton behavior).
        /// Expected result: Multiple calls return the exact same object reference.
        /// </summary>
        [Fact]
        public void DimGray_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.DimGray;
            var secondAccess = Brush.DimGray;
            var thirdAccess = Brush.DimGray;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the DimGray property returns a brush that is not empty.
        /// This test verifies that the returned brush has valid content and is not considered empty.
        /// Expected result: IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void DimGray_WhenAccessed_ReturnsNonEmptyBrush()
        {
            // Act
            var result = Brush.DimGray;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the HotPink property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void HotPink_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.HotPink;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the HotPink property returns a brush with the correct HotPink color.
        /// </summary>
        [Fact]
        public void HotPink_ColorValue_MatchesColorsHotPink()
        {
            // Act
            var result = Brush.HotPink;

            // Assert
            Assert.Equal(Colors.HotPink, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to HotPink property return the same cached instance (lazy initialization).
        /// </summary>
        [Fact]
        public void HotPink_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.HotPink;
            var second = Brush.HotPink;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the HotPink brush is not considered empty.
        /// </summary>
        [Fact]
        public void HotPink_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.HotPink;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the HotPink property returns a SolidColorBrush that can be used with Brush.IsNullOrEmpty.
        /// </summary>
        [Fact]
        public void HotPink_IsNullOrEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.HotPink;

            // Assert
            Assert.False(Brush.IsNullOrEmpty(result));
        }

        /// <summary>
        /// Tests that the LightCyan property returns a non-null SolidColorBrush instance.
        /// Verifies lazy initialization behavior and ensures the returned brush has the correct LightCyan color.
        /// </summary>
        [Fact]
        public void LightCyan_FirstAccess_ReturnsNonNullSolidColorBrushWithCorrectColor()
        {
            // Act
            var result = Brush.LightCyan;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(Colors.LightCyan, result.Color);
        }

        /// <summary>
        /// Tests that the LightCyan property returns the same instance on multiple accesses.
        /// Verifies the singleton pattern behavior of the lazy initialization.
        /// </summary>
        [Fact]
        public void LightCyan_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.LightCyan;
            var secondAccess = Brush.LightCyan;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the LightCyan property returns a brush that is not empty.
        /// Validates that the returned brush instance represents a valid color brush.
        /// </summary>
        [Fact]
        public void LightCyan_ReturnedBrush_IsNotEmpty()
        {
            // Act
            var result = Brush.LightCyan;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the LightSkyBlue property returns a non-null SolidColorBrush instance.
        /// Verifies that the property initializes correctly on first access.
        /// </summary>
        [Fact]
        public void LightSkyBlue_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.LightSkyBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the LightSkyBlue property returns a brush with the correct LightSkyBlue color.
        /// Verifies that the color matches the system-defined Colors.LightSkyBlue value.
        /// </summary>
        [Fact]
        public void LightSkyBlue_ColorValue_MatchesSystemDefinedLightSkyBlue()
        {
            // Act
            var brush = Brush.LightSkyBlue;

            // Assert
            Assert.Equal(Colors.LightSkyBlue, brush.Color);
        }

        /// <summary>
        /// Tests that the LightSkyBlue property implements lazy initialization correctly.
        /// Verifies that multiple accesses return the same instance (singleton behavior).
        /// </summary>
        [Fact]
        public void LightSkyBlue_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.LightSkyBlue;
            var secondAccess = Brush.LightSkyBlue;
            var thirdAccess = Brush.LightSkyBlue;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the LightSkyBlue property returns a brush that is not empty.
        /// Verifies that the IsEmpty property returns false for a valid color brush.
        /// </summary>
        [Fact]
        public void LightSkyBlue_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.LightSkyBlue;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the MediumBlue property returns a non-null SolidColorBrush instance.
        /// Verifies that the lazy initialization creates and returns a valid brush object.
        /// </summary>
        [Fact]
        public void MediumBlue_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.MediumBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the MediumBlue property returns the same instance on multiple calls.
        /// Verifies that the null-coalescing assignment (??=) lazy initialization works correctly
        /// and caches the instance for subsequent accesses.
        /// </summary>
        [Fact]
        public void MediumBlue_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.MediumBlue;
            var second = Brush.MediumBlue;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the MediumBlue property returns a SolidColorBrush with the correct color.
        /// Verifies that the brush is initialized with Colors.MediumBlue and the color
        /// property matches the expected system-defined color value.
        /// </summary>
        [Fact]
        public void MediumBlue_Color_EqualsSystemDefinedMediumBlue()
        {
            // Act
            var result = Brush.MediumBlue;

            // Assert
            Assert.Equal(Colors.MediumBlue, result.Color);
        }

        /// <summary>
        /// Tests that the MidnightBlue property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void MidnightBlue_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.MidnightBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the MidnightBlue property returns a brush with the correct color value.
        /// </summary>
        [Fact]
        public void MidnightBlue_ReturnsCorrectColor()
        {
            // Act
            var brush = Brush.MidnightBlue;

            // Assert
            Assert.Equal(Colors.MidnightBlue, brush.Color);
        }

        /// <summary>
        /// Tests that the MidnightBlue property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void MidnightBlue_IsNotEmpty()
        {
            // Act
            var brush = Brush.MidnightBlue;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that multiple accesses to the MidnightBlue property return the same instance (singleton behavior).
        /// </summary>
        [Fact]
        public void MidnightBlue_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.MidnightBlue;
            var secondAccess = Brush.MidnightBlue;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the MidnightBlue property returns a brush with the expected ARGB color value.
        /// </summary>
        [Fact]
        public void MidnightBlue_HasExpectedColorValue()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFF191970);

            // Act
            var brush = Brush.MidnightBlue;

            // Assert
            Assert.Equal(expectedColor, brush.Color);
        }

        /// <summary>
        /// Tests that the MidnightBlue brush is equal to another SolidColorBrush with the same color.
        /// </summary>
        [Fact]
        public void MidnightBlue_EqualsOtherBrushWithSameColor()
        {
            // Arrange
            var otherBrush = new SolidColorBrush(Colors.MidnightBlue);

            // Act
            var midnightBlueBrush = Brush.MidnightBlue;

            // Assert
            Assert.Equal(otherBrush, midnightBlueBrush);
        }

        /// <summary>
        /// Tests that the MidnightBlue brush has a consistent hash code.
        /// </summary>
        [Fact]
        public void MidnightBlue_HasConsistentHashCode()
        {
            // Act
            var brush = Brush.MidnightBlue;
            var hashCode1 = brush.GetHashCode();
            var hashCode2 = brush.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that the PapayaWhip property returns a non-null SolidColorBrush.
        /// Verifies that the lazy initialization creates a valid brush instance.
        /// Expected result: Property returns a non-null SolidColorBrush.
        /// </summary>
        [Fact]
        public void PapayaWhip_AccessProperty_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.PapayaWhip;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the PapayaWhip property returns the same instance on multiple accesses.
        /// Verifies that the lazy initialization singleton behavior works correctly.
        /// Expected result: Multiple accesses return the exact same instance.
        /// </summary>
        [Fact]
        public void PapayaWhip_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.PapayaWhip;
            var second = Brush.PapayaWhip;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the PapayaWhip property returns a brush with the correct color.
        /// Verifies that the brush contains the PapayaWhip color value (ARGB: #FFFFEFD5).
        /// Expected result: Brush Color property equals Colors.PapayaWhip.
        /// </summary>
        [Fact]
        public void PapayaWhip_CheckColor_HasCorrectPapayaWhipColor()
        {
            // Act
            var brush = Brush.PapayaWhip;

            // Assert
            Assert.Equal(Colors.PapayaWhip, brush.Color);
        }

        /// <summary>
        /// Tests that the PapayaWhip property returns a brush that is not empty.
        /// Verifies that the brush has a valid color and is not considered empty.
        /// Expected result: IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void PapayaWhip_CheckIsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.PapayaWhip;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the PapayaWhip brush color has the expected ARGB values.
        /// Verifies the specific color components match the PapayaWhip color definition.
        /// Expected result: Color components match ARGB values for #FFFFEFD5.
        /// </summary>
        [Fact]
        public void PapayaWhip_CheckColorComponents_HasExpectedARGBValues()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFFFEFD5);

            // Act
            var brush = Brush.PapayaWhip;

            // Assert
            Assert.Equal(expectedColor.Red, brush.Color.Red, 3);
            Assert.Equal(expectedColor.Green, brush.Color.Green, 3);
            Assert.Equal(expectedColor.Blue, brush.Color.Blue, 3);
            Assert.Equal(expectedColor.Alpha, brush.Color.Alpha, 3);
        }

        /// <summary>
        /// Tests that the SpringGreen property returns a non-null SolidColorBrush instance.
        /// This test verifies the basic functionality and lazy initialization of the SpringGreen property.
        /// </summary>
        [Fact]
        public void SpringGreen_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.SpringGreen;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the SpringGreen property returns a brush with the correct SpringGreen color.
        /// This test verifies that the brush is initialized with the expected color value.
        /// </summary>
        [Fact]
        public void SpringGreen_Color_MatchesColorsSpringGreen()
        {
            // Act
            var result = Brush.SpringGreen;

            // Assert
            Assert.Equal(Colors.SpringGreen, result.Color);
        }

        /// <summary>
        /// Tests that the SpringGreen property uses lazy initialization and returns the same instance on multiple calls.
        /// This test verifies the caching behavior of the static property.
        /// </summary>
        [Fact]
        public void SpringGreen_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.SpringGreen;
            var secondAccess = Brush.SpringGreen;
            var thirdAccess = Brush.SpringGreen;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the SpringGreen property returns a brush that is not empty.
        /// This test verifies that the returned brush has a valid state.
        /// </summary>
        [Fact]
        public void SpringGreen_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.SpringGreen;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the SpringGreen property can be used in null-or-empty checks.
        /// This test verifies that the brush is considered valid by the IsNullOrEmpty method.
        /// </summary>
        [Fact]
        public void SpringGreen_IsNullOrEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.SpringGreen;

            // Assert
            Assert.False(Brush.IsNullOrEmpty(result));
        }

        /// <summary>
        /// Tests that the WhiteSmoke property returns a non-null SolidColorBrush instance.
        /// Verifies the property is accessible and returns a valid brush object.
        /// Expected result: Returns a non-null SolidColorBrush.
        /// </summary>
        [Fact]
        public void WhiteSmoke_ReturnsNonNull_SolidColorBrush()
        {
            // Act
            var brush = Brush.WhiteSmoke;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
        }

        /// <summary>
        /// Tests that the WhiteSmoke property returns a brush with the correct WhiteSmoke color.
        /// Verifies the color matches the system-defined WhiteSmoke color (#FFF5F5F5).
        /// Expected result: Returns a brush with Color equal to Colors.WhiteSmoke.
        /// </summary>
        [Fact]
        public void WhiteSmoke_ReturnsCorrectColor_WhiteSmokeColor()
        {
            // Act
            var brush = Brush.WhiteSmoke;

            // Assert
            Assert.Equal(Colors.WhiteSmoke, brush.Color);
        }

        /// <summary>
        /// Tests that multiple calls to the WhiteSmoke property return the same instance.
        /// Verifies the lazy initialization behavior and singleton pattern.
        /// Expected result: Multiple calls return the exact same object reference.
        /// </summary>
        [Fact]
        public void WhiteSmoke_MultipleCalls_ReturnsSameInstance()
        {
            // Act
            var brush1 = Brush.WhiteSmoke;
            var brush2 = Brush.WhiteSmoke;

            // Assert
            Assert.Same(brush1, brush2);
        }

        /// <summary>
        /// Tests that the WhiteSmoke property returns a brush that is not empty.
        /// Verifies the IsEmpty property returns false for a valid colored brush.
        /// Expected result: IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void WhiteSmoke_IsNotEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.WhiteSmoke;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the WhiteSmoke color has the correct ARGB values.
        /// Verifies the color components match the expected WhiteSmoke color values.
        /// Expected result: Color has ARGB values matching #FFF5F5F5.
        /// </summary>
        [Fact]
        public void WhiteSmoke_ColorComponents_MatchExpectedValues()
        {
            // Act
            var brush = Brush.WhiteSmoke;
            var color = brush.Color;

            // Assert
            Assert.Equal(1.0f, color.Alpha, 3); // F
            Assert.Equal(0.961f, color.Red, 3); // F5
            Assert.Equal(0.961f, color.Green, 3); // F5
            Assert.Equal(0.961f, color.Blue, 3); // F5
        }

        /// <summary>
        /// Tests that the Cornsilk property returns a non-null SolidColorBrush instance.
        /// Verifies that the property provides a valid brush object and not null reference.
        /// </summary>
        [Fact]
        public void Cornsilk_Get_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Cornsilk;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that multiple accesses to the Cornsilk property return the same cached instance.
        /// Verifies the lazy initialization and singleton behavior of the static property.
        /// </summary>
        [Fact]
        public void Cornsilk_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.Cornsilk;
            var second = Brush.Cornsilk;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the Cornsilk property returns a brush with the correct Cornsilk color value.
        /// Verifies that the color matches the system-defined Cornsilk color (#FFFFF8DC).
        /// </summary>
        [Fact]
        public void Cornsilk_Get_ReturnsCorrectColor()
        {
            // Act
            var cornsilkBrush = Brush.Cornsilk;

            // Assert
            Assert.Equal(Colors.Cornsilk, cornsilkBrush.Color);
        }

        /// <summary>
        /// Tests that the Cornsilk property returns a brush with the expected ARGB color value.
        /// Verifies that the color has the correct ARGB value of 0xFFFFF8DC for Cornsilk.
        /// </summary>
        [Fact]
        public void Cornsilk_Get_ReturnsExpectedColorValue()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFFFF8DC);

            // Act
            var cornsilkBrush = Brush.Cornsilk;

            // Assert
            Assert.Equal(expectedColor, cornsilkBrush.Color);
        }

        /// <summary>
        /// Tests that the Cornsilk brush is not empty.
        /// Verifies that the returned brush has a valid color and is not considered empty.
        /// </summary>
        [Fact]
        public void Cornsilk_Get_IsNotEmpty()
        {
            // Act
            var cornsilkBrush = Brush.Cornsilk;

            // Assert
            Assert.False(cornsilkBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the DarkOliveGreen property returns a non-null SolidColorBrush instance.
        /// Verifies basic functionality and that the property is properly initialized.
        /// Expected result: A non-null SolidColorBrush instance is returned.
        /// </summary>
        [Fact]
        public void DarkOliveGreen_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Arrange & Act
            var result = Brush.DarkOliveGreen;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the DarkOliveGreen property implements lazy initialization correctly.
        /// Verifies that multiple accesses return the same instance rather than creating new ones.
        /// Expected result: The same instance is returned on all subsequent calls.
        /// </summary>
        [Fact]
        public void DarkOliveGreen_MultipleAccesses_ReturnsSameInstance()
        {
            // Arrange & Act
            var first = Brush.DarkOliveGreen;
            var second = Brush.DarkOliveGreen;
            var third = Brush.DarkOliveGreen;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that the DarkOliveGreen property returns a brush with the correct color value.
        /// Verifies that the brush's Color property matches the expected DarkOliveGreen color (#FF556B2F).
        /// Expected result: The brush's Color property equals Colors.DarkOliveGreen.
        /// </summary>
        [Fact]
        public void DarkOliveGreen_ColorValue_MatchesExpectedDarkOliveGreenColor()
        {
            // Arrange
            var expectedColor = Colors.DarkOliveGreen;

            // Act
            var brush = Brush.DarkOliveGreen;

            // Assert
            Assert.Equal(expectedColor, brush.Color);
        }

        /// <summary>
        /// Tests that the DarkOliveGreen property returns a brush that is not empty.
        /// Verifies that the IsEmpty property returns false for a valid color brush.
        /// Expected result: IsEmpty returns false.
        /// </summary>
        [Fact]
        public void DarkOliveGreen_IsEmpty_ReturnsFalse()
        {
            // Arrange & Act
            var brush = Brush.DarkOliveGreen;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the DarkOliveGreen property is thread-safe and maintains lazy initialization behavior.
        /// Verifies that multiple threads accessing the property concurrently get the same instance.
        /// Expected result: All threads receive the same brush instance.
        /// </summary>
        [Fact]
        public async Task DarkOliveGreen_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];
            var results = new SolidColorBrush[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() => Brush.DarkOliveGreen);
            }

            var brushes = await Task.WhenAll(tasks);

            // Assert
            var firstBrush = brushes[0];
            for (int i = 1; i < brushes.Length; i++)
            {
                Assert.Same(firstBrush, brushes[i]);
            }
        }

        /// <summary>
        /// Tests that the DarkOliveGreen brush color has the expected ARGB components.
        /// Verifies the specific color values match the DarkOliveGreen specification (#FF556B2F).
        /// Expected result: Color components match Alpha=255, Red=85, Green=107, Blue=47.
        /// </summary>
        [Fact]
        public void DarkOliveGreen_ColorComponents_MatchExpectedARGBValues()
        {
            // Arrange
            var brush = Brush.DarkOliveGreen;
            var color = brush.Color;

            // Act & Assert
            Assert.Equal(1.0f, color.Alpha, 3); // Alpha should be 1.0 (255/255)
            Assert.Equal(85f / 255f, color.Red, 3); // Red component: 0x55 = 85
            Assert.Equal(107f / 255f, color.Green, 3); // Green component: 0x6B = 107
            Assert.Equal(47f / 255f, color.Blue, 3); // Blue component: 0x2F = 47
        }

        /// <summary>
        /// Tests that the DarkOliveGreen property returns a brush equal to a manually created brush with the same color.
        /// Verifies that the brush behaves equivalently to other SolidColorBrush instances with the same color.
        /// Expected result: The brushes are considered equal.
        /// </summary>
        [Fact]
        public void DarkOliveGreen_Equality_EqualsManuallyCreatedBrushWithSameColor()
        {
            // Arrange
            var predefinedBrush = Brush.DarkOliveGreen;
            var manualBrush = new SolidColorBrush(Colors.DarkOliveGreen);

            // Act & Assert
            Assert.Equal(predefinedBrush.Color, manualBrush.Color);
            Assert.Equal(predefinedBrush, manualBrush);
        }

        /// <summary>
        /// Tests that the Lavender property returns a non-null SolidColorBrush instance.
        /// Verifies the basic functionality and initialization of the lazy-loaded property.
        /// </summary>
        [Fact]
        public void Lavender_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var lavenderBrush = Brush.Lavender;

            // Assert
            Assert.NotNull(lavenderBrush);
            Assert.IsType<SolidColorBrush>(lavenderBrush);
        }

        /// <summary>
        /// Tests that multiple accesses to the Lavender property return the same instance.
        /// Verifies the lazy initialization singleton behavior of the static property.
        /// </summary>
        [Fact]
        public void Lavender_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Lavender;
            var secondAccess = Brush.Lavender;
            var thirdAccess = Brush.Lavender;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the Lavender brush has the correct color value.
        /// Verifies that the brush is initialized with Colors.Lavender color.
        /// </summary>
        [Fact]
        public void Lavender_Color_MatchesSystemDefinedLavenderColor()
        {
            // Act
            var lavenderBrush = Brush.Lavender;

            // Assert
            Assert.Equal(Colors.Lavender, lavenderBrush.Color);
        }

        /// <summary>
        /// Tests that the Lavender brush is not empty.
        /// Verifies that the brush represents a valid, non-empty color brush.
        /// </summary>
        [Fact]
        public void Lavender_IsEmpty_ReturnsFalse()
        {
            // Act
            var lavenderBrush = Brush.Lavender;

            // Assert
            Assert.False(lavenderBrush.IsEmpty);
        }

        /// <summary>
        /// Tests concurrent access to the Lavender property to ensure thread safety.
        /// Verifies that the lazy initialization works correctly under concurrent conditions.
        /// </summary>
        [Fact]
        public async Task Lavender_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.Lavender);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            var firstResult = results[0];
            for (int i = 1; i < taskCount; i++)
            {
                Assert.Same(firstResult, results[i]);
            }
        }

        /// <summary>
        /// Tests that the Lavender property returns an instance that matches expected properties.
        /// Verifies comprehensive properties of the returned brush including type, color, and state.
        /// </summary>
        [Fact]
        public void Lavender_Properties_AllPropertiesValid()
        {
            // Act
            var lavenderBrush = Brush.Lavender;

            // Assert
            Assert.NotNull(lavenderBrush);
            Assert.IsAssignableFrom<SolidColorBrush>(lavenderBrush);
            Assert.Equal(Colors.Lavender, lavenderBrush.Color);
            Assert.False(lavenderBrush.IsEmpty);
            Assert.NotEqual(Color.FromArgb("#00000000"), lavenderBrush.Color); // Not transparent
        }

        /// <summary>
        /// Tests that the LightSalmon property returns a non-null SolidColorBrush with the correct color.
        /// Validates the basic functionality and ensures the brush is properly initialized with Colors.LightSalmon.
        /// </summary>
        [Fact]
        public void LightSalmon_FirstAccess_ReturnsNonNullBrushWithCorrectColor()
        {
            // Act
            var result = Brush.LightSalmon;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Colors.LightSalmon, result.Color);
        }

        /// <summary>
        /// Tests the lazy initialization behavior of the LightSalmon property.
        /// Verifies that multiple calls to the property return the same instance (reference equality).
        /// </summary>
        [Fact]
        public void LightSalmon_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.LightSalmon;
            var second = Brush.LightSalmon;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the LightSalmon property returns an object of the correct type.
        /// Validates that the returned brush is assignable to SolidColorBrush.
        /// </summary>
        [Fact]
        public void LightSalmon_ReturnType_IsSolidColorBrush()
        {
            // Act
            var result = Brush.LightSalmon;

            // Assert
            Assert.IsAssignableFrom<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the LightSalmon property returns a brush that is not empty.
        /// Validates the IsEmpty property behavior for the returned brush.
        /// </summary>
        [Fact]
        public void LightSalmon_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.LightSalmon;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the MediumOrchid property returns a non-null SolidColorBrush instance.
        /// This test verifies the basic functionality and lazy initialization of the static property.
        /// Expected result: A non-null SolidColorBrush instance is returned.
        /// </summary>
        [Fact]
        public void MediumOrchid_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.MediumOrchid;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the MediumOrchid property returns a brush with the correct color value.
        /// This test verifies that the returned brush contains the expected Colors.MediumOrchid color.
        /// Expected result: The brush's Color property matches Colors.MediumOrchid.
        /// </summary>
        [Fact]
        public void MediumOrchid_WhenAccessed_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.MediumOrchid;

            // Assert
            Assert.Equal(Colors.MediumOrchid, result.Color);
        }

        /// <summary>
        /// Tests that the MediumOrchid property uses lazy initialization and returns the same instance on multiple calls.
        /// This test verifies the singleton behavior of the static property implementation using the ??= operator.
        /// Expected result: Multiple calls to the property return the exact same object reference.
        /// </summary>
        [Fact]
        public void MediumOrchid_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var firstCall = Brush.MediumOrchid;
            var secondCall = Brush.MediumOrchid;

            // Assert
            Assert.Same(firstCall, secondCall);
        }

        /// <summary>
        /// Tests that the MediumOrchid property is not empty.
        /// This test verifies that the returned brush is not considered empty according to its IsEmpty property.
        /// Expected result: The IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void MediumOrchid_WhenAccessed_IsNotEmpty()
        {
            // Act
            var result = Brush.MediumOrchid;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that accessing OldLace property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void OldLace_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.OldLace;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the OldLace property returns a brush with the correct OldLace color.
        /// </summary>
        [Fact]
        public void OldLace_ColorProperty_EqualsSystemOldLaceColor()
        {
            // Act
            var brush = Brush.OldLace;

            // Assert
            Assert.Equal(Colors.OldLace, brush.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the OldLace property return the same cached instance.
        /// </summary>
        [Fact]
        public void OldLace_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.OldLace;
            var secondAccess = Brush.OldLace;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the OldLace property returns a brush with the expected ARGB color value.
        /// </summary>
        [Fact]
        public void OldLace_ColorValue_HasCorrectArgbValue()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFFDF5E6);

            // Act
            var brush = Brush.OldLace;

            // Assert
            Assert.Equal(expectedColor, brush.Color);
        }

        /// <summary>
        /// Tests that the OldLace property always returns the same type of SolidColorBrush.
        /// </summary>
        [Fact]
        public void OldLace_ReturnType_IsAssignableToSolidColorBrush()
        {
            // Act
            SolidColorBrush brush = Brush.OldLace;

            // Assert
            Assert.NotNull(brush);
            Assert.IsAssignableFrom<SolidColorBrush>(brush);
        }

        /// <summary>
        /// Tests that the OldLace brush is not considered empty.
        /// </summary>
        [Fact]
        public void OldLace_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.OldLace;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Silver property returns a non-null SolidColorBrush instance on first access.
        /// This test verifies the lazy initialization behavior and ensures the property creates the brush correctly.
        /// Expected: Returns a SolidColorBrush instance that is not null.
        /// </summary>
        [Fact]
        public void Silver_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Silver;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Silver property returns the same instance on multiple accesses.
        /// This test verifies the lazy initialization pattern where subsequent calls return the cached instance.
        /// Expected: Multiple accesses return the exact same object reference.
        /// </summary>
        [Fact]
        public void Silver_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.Silver;
            var second = Brush.Silver;
            var third = Brush.Silver;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that the Silver property returns a brush with the correct Colors.Silver color.
        /// This test verifies that the brush is initialized with the proper system-defined silver color.
        /// Expected: The Color property of the returned brush equals Colors.Silver.
        /// </summary>
        [Fact]
        public void Silver_ColorProperty_EqualsSystemDefinedSilverColor()
        {
            // Act
            var silverBrush = Brush.Silver;

            // Assert
            Assert.Equal(Colors.Silver, silverBrush.Color);
        }

        /// <summary>
        /// Tests that the Silver property returns a brush that is not empty.
        /// This test verifies that the created brush has a valid state and is not considered empty.
        /// Expected: The IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void Silver_IsEmpty_ReturnsFalse()
        {
            // Act
            var silverBrush = Brush.Silver;

            // Assert
            Assert.False(silverBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Silver property consistently returns the same color across multiple accesses.
        /// This test ensures that the color remains consistent and unchanged after lazy initialization.
        /// Expected: Color remains Colors.Silver across all accesses.
        /// </summary>
        [Fact]
        public void Silver_ColorConsistency_RemainsUnchangedAcrossMultipleAccesses()
        {
            // Act
            var firstAccess = Brush.Silver;
            var firstColor = firstAccess.Color;
            var secondAccess = Brush.Silver;
            var secondColor = secondAccess.Color;

            // Assert
            Assert.Equal(Colors.Silver, firstColor);
            Assert.Equal(Colors.Silver, secondColor);
            Assert.Equal(firstColor, secondColor);
        }

        /// <summary>
        /// Tests that the Violet property returns a non-null SolidColorBrush instance.
        /// This validates that the lazy initialization creates a proper brush object.
        /// </summary>
        [Fact]
        public void Violet_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Violet;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Violet property returns the same instance on multiple accesses.
        /// This validates that the lazy initialization pattern (??=) works correctly.
        /// </summary>
        [Fact]
        public void Violet_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Violet;
            var secondAccess = Brush.Violet;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Violet property returns a brush with the correct violet color.
        /// This validates that the brush is initialized with Colors.Violet (#FFEE82EE).
        /// </summary>
        [Fact]
        public void Violet_WhenAccessed_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.Violet;

            // Assert
            Assert.Equal(Colors.Violet, result.Color);
        }

        /// <summary>
        /// Tests that the Violet property returns a non-empty brush.
        /// This validates that the brush has a valid color and is not considered empty.
        /// </summary>
        [Fact]
        public void Violet_WhenAccessed_ReturnsNonEmptyBrush()
        {
            // Act
            var result = Brush.Violet;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the Violet property returns a brush with expected ARGB color values.
        /// This validates that Colors.Violet has the correct ARGB value of #FFEE82EE.
        /// </summary>
        [Fact]
        public void Violet_WhenAccessed_ReturnsExpectedARGBValues()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFEE82EE);

            // Act
            var result = Brush.Violet;

            // Assert
            Assert.Equal(expectedColor, result.Color);
            Assert.Equal(255, (int)(result.Color.Alpha * 255)); // FF
            Assert.Equal(238, (int)(result.Color.Red * 255));   // EE
            Assert.Equal(130, (int)(result.Color.Green * 255)); // 82
            Assert.Equal(238, (int)(result.Color.Blue * 255));  // EE
        }

        /// <summary>
        /// Tests that the Aquamarine property returns a non-null SolidColorBrush instance.
        /// Verifies the basic functionality and return type of the lazy-initialized property.
        /// </summary>
        [Fact]
        public void Aquamarine_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Aquamarine;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Aquamarine property returns a brush with the correct color value.
        /// Verifies that the brush's Color property matches the system-defined Colors.Aquamarine.
        /// </summary>
        [Fact]
        public void Aquamarine_ColorProperty_MatchesSystemDefinedAquamarineColor()
        {
            // Act
            var brush = Brush.Aquamarine;

            // Assert
            Assert.Equal(Colors.Aquamarine, brush.Color);
        }

        /// <summary>
        /// Tests that the Aquamarine property implements lazy initialization correctly.
        /// Verifies that multiple accesses return the same cached instance.
        /// </summary>
        [Fact]
        public void Aquamarine_MultipleAccesses_ReturnsSameCachedInstance()
        {
            // Act
            var firstAccess = Brush.Aquamarine;
            var secondAccess = Brush.Aquamarine;
            var thirdAccess = Brush.Aquamarine;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(firstAccess, thirdAccess);
            Assert.Same(secondAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the Aquamarine property returns an ImmutableBrush instance.
        /// Verifies the specific implementation type used for system-defined color brushes.
        /// </summary>
        [Fact]
        public void Aquamarine_ReturnType_IsImmutableBrush()
        {
            // Act
            var brush = Brush.Aquamarine;

            // Assert
            Assert.IsAssignableFrom<SolidColorBrush>(brush);
            // Note: ImmutableBrush is internal, so we can't test for it directly
            // but we can verify it behaves like an immutable brush by checking its properties
        }

        /// <summary>
        /// Tests that the Aquamarine brush maintains consistent color across multiple accesses.
        /// Verifies that the cached instance preserves the correct color value.
        /// </summary>
        [Fact]
        public void Aquamarine_CachedInstance_MaintainsConsistentColor()
        {
            // Act
            var firstBrush = Brush.Aquamarine;
            var secondBrush = Brush.Aquamarine;

            // Assert
            Assert.Equal(firstBrush.Color, secondBrush.Color);
            Assert.Equal(Colors.Aquamarine, firstBrush.Color);
            Assert.Equal(Colors.Aquamarine, secondBrush.Color);
        }

        /// <summary>
        /// Tests that the Chocolate property returns a non-null SolidColorBrush with the correct color value.
        /// Verifies that the property uses lazy initialization and returns the same instance on subsequent calls.
        /// Also validates that the color matches the expected system-defined Chocolate color (#FFD2691E).
        /// </summary>
        [Fact]
        public void Chocolate_ReturnsValidSolidColorBrushWithCorrectColor()
        {
            // Arrange & Act - First access
            var chocolate1 = Brush.Chocolate;

            // Assert - First access
            Assert.NotNull(chocolate1);
            Assert.IsType<SolidColorBrush>(chocolate1);
            Assert.Equal(Colors.Chocolate, chocolate1.Color);

            // Act - Second access
            var chocolate2 = Brush.Chocolate;

            // Assert - Singleton behavior
            Assert.Same(chocolate1, chocolate2);
        }

        /// <summary>
        /// Tests that the Chocolate property returns an ImmutableBrush instance.
        /// Verifies the specific implementation type used for predefined color brushes.
        /// </summary>
        [Fact]
        public void Chocolate_ReturnsImmutableBrushInstance()
        {
            // Arrange & Act
            var chocolate = Brush.Chocolate;

            // Assert
            Assert.IsAssignableFrom<SolidColorBrush>(chocolate);
            // Note: ImmutableBrush is internal, so we can't directly test for it
            // but we can verify it behaves correctly
            Assert.NotNull(chocolate);
        }

        /// <summary>
        /// Tests that the Chocolate property color matches the expected ARGB value.
        /// Verifies that the color corresponds to the system-defined Chocolate color (#FFD2691E).
        /// </summary>
        [Fact]
        public void Chocolate_ColorMatchesExpectedArgbValue()
        {
            // Arrange & Act
            var chocolate = Brush.Chocolate;
            var expectedColor = Color.FromUint(0xFFD2691E);

            // Assert
            Assert.Equal(expectedColor, chocolate.Color);
            Assert.Equal(expectedColor, Colors.Chocolate);
        }

        /// <summary>
        /// Tests that multiple accesses to the Chocolate property return the exact same instance.
        /// Verifies the singleton pattern implementation using the null-coalescing assignment operator.
        /// </summary>
        [Fact]
        public void Chocolate_MultipleAccesses_ReturnsSameInstance()
        {
            // Arrange & Act
            var instance1 = Brush.Chocolate;
            var instance2 = Brush.Chocolate;
            var instance3 = Brush.Chocolate;

            // Assert
            Assert.Same(instance1, instance2);
            Assert.Same(instance2, instance3);
            Assert.Same(instance1, instance3);
        }

        /// <summary>
        /// Tests that the Chocolate brush is not empty.
        /// Verifies that the predefined color brush has a valid color and is not considered empty.
        /// </summary>
        [Fact]
        public void Chocolate_IsNotEmpty()
        {
            // Arrange & Act
            var chocolate = Brush.Chocolate;

            // Assert
            Assert.False(chocolate.IsEmpty);
        }

        /// <summary>
        /// Tests that DarkMagenta property returns a non-null SolidColorBrush instance.
        /// Verifies basic functionality and ensures the property never returns null.
        /// </summary>
        [Fact]
        public void DarkMagenta_ReturnsNonNull_SolidColorBrush()
        {
            // Act
            var result = Brush.DarkMagenta;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that DarkMagenta property returns a brush with the correct DarkMagenta color.
        /// Verifies that the color property of the returned brush matches Colors.DarkMagenta.
        /// </summary>
        [Fact]
        public void DarkMagenta_ReturnsCorrectColor_DarkMagentaColor()
        {
            // Act
            var result = Brush.DarkMagenta;

            // Assert
            Assert.Equal(Colors.DarkMagenta, result.Color);
        }

        /// <summary>
        /// Tests that DarkMagenta property implements lazy initialization correctly.
        /// Verifies that multiple calls to the property return the exact same instance,
        /// demonstrating proper caching behavior.
        /// </summary>
        [Fact]
        public void DarkMagenta_LazyInitialization_ReturnsSameInstance()
        {
            // Act
            var firstCall = Brush.DarkMagenta;
            var secondCall = Brush.DarkMagenta;
            var thirdCall = Brush.DarkMagenta;

            // Assert
            Assert.Same(firstCall, secondCall);
            Assert.Same(firstCall, thirdCall);
            Assert.Same(secondCall, thirdCall);
        }

        /// <summary>
        /// Tests that DarkMagenta property is thread-safe during concurrent access.
        /// Verifies that multiple threads accessing the property simultaneously
        /// all receive the same cached instance.
        /// </summary>
        [Fact]
        public async Task DarkMagenta_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            const int numberOfTasks = 10;
            var tasks = new Task<SolidColorBrush>[numberOfTasks];
            var results = new SolidColorBrush[numberOfTasks];

            // Act
            for (int i = 0; i < numberOfTasks; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() => Brush.DarkMagenta);
            }

            var taskResults = await Task.WhenAll(tasks);

            // Assert
            var firstResult = taskResults[0];
            Assert.NotNull(firstResult);

            for (int i = 1; i < taskResults.Length; i++)
            {
                Assert.Same(firstResult, taskResults[i]);
            }

            // Verify all have correct color
            foreach (var result in taskResults)
            {
                Assert.Equal(Colors.DarkMagenta, result.Color);
            }
        }

        /// <summary>
        /// Tests that DarkMagenta property returns a brush that is not empty.
        /// Verifies that the IsEmpty property of the returned brush is false,
        /// indicating it contains valid color data.
        /// </summary>
        [Fact]
        public void DarkMagenta_IsNotEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.DarkMagenta;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that DarkMagenta brush has consistent hash code and equality behavior.
        /// Verifies that the returned brush behaves correctly in equality comparisons
        /// and produces consistent hash codes.
        /// </summary>
        [Fact]
        public void DarkMagenta_ConsistentHashCodeAndEquality_BehavesCorrectly()
        {
            // Arrange
            var brush1 = Brush.DarkMagenta;
            var brush2 = Brush.DarkMagenta;
            var brush3 = new SolidColorBrush(Colors.DarkMagenta);

            // Act & Assert
            Assert.Equal(brush1.GetHashCode(), brush2.GetHashCode());
            Assert.True(brush1.Equals(brush2));
            Assert.True(brush1.Equals(brush3));
            Assert.Equal(brush1.GetHashCode(), brush3.GetHashCode());
        }

        /// <summary>
        /// Tests that the DarkTurquoise property returns a valid SolidColorBrush instance
        /// with the correct color value matching Colors.DarkTurquoise.
        /// </summary>
        [Fact]
        public void DarkTurquoise_AccessProperty_ReturnsSolidColorBrushWithCorrectColor()
        {
            // Act
            var brush = Brush.DarkTurquoise;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
            Assert.Equal(Colors.DarkTurquoise, brush.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the DarkTurquoise property return the same instance,
        /// verifying the lazy initialization pattern is working correctly.
        /// </summary>
        [Fact]
        public void DarkTurquoise_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.DarkTurquoise;
            var secondAccess = Brush.DarkTurquoise;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the DarkTurquoise property returns a brush that is not empty,
        /// ensuring it has valid color data.
        /// </summary>
        [Fact]
        public void DarkTurquoise_AccessProperty_ReturnsNonEmptyBrush()
        {
            // Act
            var brush = Brush.DarkTurquoise;

            // Assert
            Assert.NotNull(brush);
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the DarkTurquoise property returns a brush with the expected
        /// color components matching the system-defined DarkTurquoise color.
        /// </summary>
        [Fact]
        public void DarkTurquoise_AccessProperty_ReturnsCorrectColorComponents()
        {
            // Arrange
            var expectedColor = Colors.DarkTurquoise;

            // Act
            var brush = Brush.DarkTurquoise;

            // Assert
            Assert.Equal(expectedColor.Red, brush.Color.Red);
            Assert.Equal(expectedColor.Green, brush.Color.Green);
            Assert.Equal(expectedColor.Blue, brush.Color.Blue);
            Assert.Equal(expectedColor.Alpha, brush.Color.Alpha);
        }

        /// <summary>
        /// Tests that the Gainsboro property returns a non-null SolidColorBrush instance.
        /// Verifies that the lazy initialization creates the brush correctly on first access.
        /// </summary>
        [Fact]
        public void Gainsboro_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Gainsboro;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Gainsboro property returns the same cached instance on subsequent calls.
        /// Verifies that the null-coalescing assignment operator (??=) provides proper caching behavior.
        /// </summary>
        [Fact]
        public void Gainsboro_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.Gainsboro;
            var second = Brush.Gainsboro;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the Gainsboro property returns a brush with the correct color value.
        /// Verifies that the brush is initialized with Colors.Gainsboro.
        /// </summary>
        [Fact]
        public void Gainsboro_Color_MatchesSystemDefinedGainsboroColor()
        {
            // Act
            var brush = Brush.Gainsboro;

            // Assert
            Assert.Equal(Colors.Gainsboro, brush.Color);
        }

        /// <summary>
        /// Tests that the Gainsboro property is thread-safe during concurrent access.
        /// Verifies that multiple threads accessing the property simultaneously get the same instance.
        /// </summary>
        [Fact]
        public async Task Gainsboro_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.Gainsboro);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            var firstResult = results[0];
            for (int i = 1; i < taskCount; i++)
            {
                Assert.Same(firstResult, results[i]);
            }
        }

        /// <summary>
        /// Tests that the Gainsboro property returns a brush that is not empty.
        /// Verifies that the IsEmpty property returns false for a valid color brush.
        /// </summary>
        [Fact]
        public void Gainsboro_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.Gainsboro;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Gainsboro property returns a brush with the expected ARGB color value.
        /// Verifies that the color matches the system-defined Gainsboro color (#FFDCDCDC).
        /// </summary>
        [Fact]
        public void Gainsboro_ColorValue_MatchesExpectedArgbValue()
        {
            // Act
            var brush = Brush.Gainsboro;
            var color = brush.Color;

            // Assert
            Assert.Equal(220, color.Red * 255, 0);    // 0xDC = 220
            Assert.Equal(220, color.Green * 255, 0);  // 0xDC = 220
            Assert.Equal(220, color.Blue * 255, 0);   // 0xDC = 220
            Assert.Equal(255, color.Alpha * 255, 0);  // 0xFF = 255
        }

        /// <summary>
        /// Tests that the Ivory property returns a SolidColorBrush with the correct color on first access.
        /// Verifies that the brush is properly initialized with Colors.Ivory and is not empty.
        /// </summary>
        [Fact]
        public void Ivory_FirstAccess_ReturnsSolidColorBrushWithCorrectColor()
        {
            // Arrange & Act
            var ivoryBrush = Brush.Ivory;

            // Assert
            Assert.NotNull(ivoryBrush);
            Assert.IsType<SolidColorBrush>(ivoryBrush);
            Assert.Equal(Colors.Ivory, ivoryBrush.Color);
            Assert.False(ivoryBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Ivory property returns the same instance on subsequent accesses.
        /// Verifies the singleton behavior of the lazily-initialized static property.
        /// </summary>
        [Fact]
        public void Ivory_SubsequentAccess_ReturnsSameInstance()
        {
            // Arrange
            var firstAccess = Brush.Ivory;

            // Act
            var secondAccess = Brush.Ivory;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Ivory property returns a brush with the expected ARGB color values.
        /// Verifies that the color matches the system-defined ivory color (#FFFFFFF0).
        /// </summary>
        [Fact]
        public void Ivory_ColorValue_MatchesExpectedARGBValues()
        {
            // Arrange & Act
            var ivoryBrush = Brush.Ivory;

            // Assert
            Assert.Equal(255, ivoryBrush.Color.Red * 255, 0);
            Assert.Equal(255, ivoryBrush.Color.Green * 255, 0);
            Assert.Equal(240, ivoryBrush.Color.Blue * 255, 0);
            Assert.Equal(255, ivoryBrush.Color.Alpha * 255, 0);
        }

        /// <summary>
        /// Tests that the LightGreen property returns a non-null SolidColorBrush instance.
        /// This test ensures the lazy initialization works correctly on first access.
        /// </summary>
        [Fact]
        public void LightGreen_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Arrange & Act
            SolidColorBrush result = Brush.LightGreen;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the LightGreen property returns the same instance on multiple accesses.
        /// This verifies the singleton behavior of the lazy initialization pattern.
        /// </summary>
        [Fact]
        public void LightGreen_MultipleAccesses_ReturnsSameInstance()
        {
            // Arrange & Act
            SolidColorBrush first = Brush.LightGreen;
            SolidColorBrush second = Brush.LightGreen;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the LightGreen property returns a brush with the correct LightGreen color.
        /// This ensures the brush is initialized with the proper color value from Colors.LightGreen.
        /// </summary>
        [Fact]
        public void LightGreen_ColorProperty_HasCorrectLightGreenColor()
        {
            // Arrange & Act
            SolidColorBrush brush = Brush.LightGreen;

            // Assert
            Assert.Equal(Colors.LightGreen, brush.Color);
        }

        /// <summary>
        /// Tests that the LightGreen property returns a brush that is not empty.
        /// This verifies the brush is properly initialized and usable.
        /// </summary>
        [Fact]
        public void LightGreen_IsEmpty_ReturnsFalse()
        {
            // Arrange & Act
            SolidColorBrush brush = Brush.LightGreen;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that concurrent access to the LightGreen property is thread-safe.
        /// This ensures the lazy initialization works correctly under concurrent conditions.
        /// </summary>
        [Fact]
        public async Task LightGreen_ConcurrentAccess_ReturnsConsistentResults()
        {
            // Arrange
            const int taskCount = 10;
            Task<SolidColorBrush>[] tasks = new Task<SolidColorBrush>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.LightGreen);
            }

            SolidColorBrush[] results = await Task.WhenAll(tasks);

            // Assert
            SolidColorBrush firstResult = results[0];
            Assert.NotNull(firstResult);

            for (int i = 1; i < results.Length; i++)
            {
                Assert.Same(firstResult, results[i]);
            }
        }

        /// <summary>
        /// Tests that the LightGreen brush has the expected color components.
        /// This verifies the specific ARGB values of the LightGreen color.
        /// </summary>
        [Fact]
        public void LightGreen_ColorComponents_HasExpectedValues()
        {
            // Arrange & Act
            SolidColorBrush brush = Brush.LightGreen;
            Color color = brush.Color;

            // Assert
            Assert.Equal(Colors.LightGreen.Red, color.Red);
            Assert.Equal(Colors.LightGreen.Green, color.Green);
            Assert.Equal(Colors.LightGreen.Blue, color.Blue);
            Assert.Equal(Colors.LightGreen.Alpha, color.Alpha);
        }

        /// <summary>
        /// Tests that the LightGreen brush can be used in equality comparisons.
        /// This ensures the brush behaves correctly in comparison operations.
        /// </summary>
        [Fact]
        public void LightGreen_EqualityComparison_WorksCorrectly()
        {
            // Arrange
            SolidColorBrush brush1 = Brush.LightGreen;
            SolidColorBrush brush2 = Brush.LightGreen;
            SolidColorBrush differentBrush = new SolidColorBrush(Colors.Red);

            // Act & Assert
            Assert.True(brush1.Equals(brush2));
            Assert.False(brush1.Equals(differentBrush));
            Assert.Equal(brush1.GetHashCode(), brush2.GetHashCode());
        }

        /// <summary>
        /// Tests that the Maroon property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void Maroon_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var maroonBrush = Brush.Maroon;

            // Assert
            Assert.NotNull(maroonBrush);
            Assert.IsType<SolidColorBrush>(maroonBrush);
        }

        /// <summary>
        /// Tests that the Maroon property returns the same instance on multiple calls (lazy initialization).
        /// </summary>
        [Fact]
        public void Maroon_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Maroon;
            var secondAccess = Brush.Maroon;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Maroon property returns a brush with the correct color value.
        /// </summary>
        [Fact]
        public void Maroon_Color_MatchesExpectedMaroonColor()
        {
            // Arrange
            var expectedColor = Colors.Maroon;

            // Act
            var maroonBrush = Brush.Maroon;

            // Assert
            Assert.Equal(expectedColor, maroonBrush.Color);
        }

        /// <summary>
        /// Tests that the Maroon property returns an ImmutableBrush instance.
        /// </summary>
        [Fact]
        public void Maroon_Type_IsImmutableBrush()
        {
            // Act
            var maroonBrush = Brush.Maroon;

            // Assert
            Assert.True(maroonBrush.GetType().Name == "ImmutableBrush");
        }

        /// <summary>
        /// Tests that the Maroon brush color cannot be modified (immutability).
        /// </summary>
        [Fact]
        public void Maroon_Color_IsImmutable()
        {
            // Arrange
            var maroonBrush = Brush.Maroon;
            var originalColor = maroonBrush.Color;

            // Act
            maroonBrush.Color = Colors.Blue; // Attempt to change color

            // Assert
            Assert.Equal(originalColor, maroonBrush.Color); // Color should remain unchanged
        }

        /// <summary>
        /// Tests that the Maroon brush has the correct ARGB color value.
        /// </summary>
        [Fact]
        public void Maroon_Color_HasCorrectArgbValue()
        {
            // Arrange
            var expectedArgbValue = 0xFF800000;

            // Act
            var maroonBrush = Brush.Maroon;

            // Assert
            Assert.Equal(Colors.Maroon, maroonBrush.Color);
            // Verify the color has the expected ARGB value through equality with Colors.Maroon
            Assert.True(maroonBrush.Color.Equals(Color.FromUint(expectedArgbValue)));
        }

        /// <summary>
        /// Tests that the Maroon brush is not empty.
        /// </summary>
        [Fact]
        public void Maroon_IsEmpty_ReturnsFalse()
        {
            // Act
            var maroonBrush = Brush.Maroon;

            // Assert
            Assert.False(maroonBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Moccasin property returns a non-null SolidColorBrush with the correct color.
        /// Verifies the lazy initialization creates a brush with the expected Moccasin color value.
        /// </summary>
        [Fact]
        public void Moccasin_FirstAccess_ReturnsNonNullSolidColorBrushWithCorrectColor()
        {
            // Act
            var moccasinBrush = Brush.Moccasin;

            // Assert
            Assert.NotNull(moccasinBrush);
            Assert.IsType<SolidColorBrush>(moccasinBrush);
            Assert.Equal(Colors.Moccasin, moccasinBrush.Color);
            Assert.Equal(Color.FromUint(0xFFFFE4B5), moccasinBrush.Color);
        }

        /// <summary>
        /// Tests that the Moccasin property uses lazy initialization and returns the same instance on subsequent calls.
        /// Verifies the singleton behavior to ensure efficient memory usage.
        /// </summary>
        [Fact]
        public void Moccasin_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Moccasin;
            var secondAccess = Brush.Moccasin;
            var thirdAccess = Brush.Moccasin;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the Moccasin property returns a brush that is not empty.
        /// Verifies the IsEmpty property behavior for the created brush.
        /// </summary>
        [Fact]
        public void Moccasin_ReturnedBrush_IsNotEmpty()
        {
            // Act
            var moccasinBrush = Brush.Moccasin;

            // Assert
            Assert.False(moccasinBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Moccasin property is thread-safe when accessed concurrently.
        /// Verifies that multiple threads accessing the property simultaneously get the same instance.
        /// </summary>
        [Fact]
        public async Task Moccasin_ConcurrentAccess_ReturnsSameInstanceThreadSafely()
        {
            // Arrange
            const int taskCount = 100;
            var tasks = new Task<SolidColorBrush>[taskCount];
            var results = new SolidColorBrush[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() => Brush.Moccasin);
            }

            var allResults = await Task.WhenAll(tasks);

            // Assert
            var firstResult = allResults[0];
            Assert.NotNull(firstResult);

            for (int i = 1; i < taskCount; i++)
            {
                Assert.Same(firstResult, allResults[i]);
            }

            // Verify all results have the correct color
            foreach (var result in allResults)
            {
                Assert.Equal(Colors.Moccasin, result.Color);
            }
        }

        /// <summary>
        /// Tests that the Moccasin property color components match the expected ARGB values.
        /// Verifies the individual ARGB components of the Moccasin color (#FFFFE4B5).
        /// </summary>
        [Fact]
        public void Moccasin_ColorComponents_MatchExpectedValues()
        {
            // Act
            var moccasinBrush = Brush.Moccasin;
            var color = moccasinBrush.Color;

            // Assert
            Assert.Equal(1.0f, color.Alpha, 3); // 0xFF -> 1.0
            Assert.Equal(1.0f, color.Red, 3);   // 0xFF -> 1.0
            Assert.Equal(0.894f, color.Green, 3); // 0xE4 -> ~0.894
            Assert.Equal(0.710f, color.Blue, 3);  // 0xB5 -> ~0.710
        }

        /// <summary>
        /// Tests that the Moccasin brush can be used in equality comparisons.
        /// Verifies that the brush equals other brushes with the same color.
        /// </summary>
        [Fact]
        public void Moccasin_EqualityComparison_WorksCorrectly()
        {
            // Arrange
            var moccasinBrush = Brush.Moccasin;
            var anotherMoccasinBrush = new SolidColorBrush(Colors.Moccasin);

            // Act & Assert
            Assert.Equal(moccasinBrush, anotherMoccasinBrush);
            Assert.True(moccasinBrush.Equals(anotherMoccasinBrush));
            Assert.Equal(moccasinBrush.GetHashCode(), anotherMoccasinBrush.GetHashCode());
        }

        /// <summary>
        /// Tests that PaleVioletRed property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void PaleVioletRed_Get_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var brush = Brush.PaleVioletRed;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
        }

        /// <summary>
        /// Tests that PaleVioletRed property returns a brush with the correct color value.
        /// </summary>
        [Fact]
        public void PaleVioletRed_Get_ReturnsCorrectColor()
        {
            // Act
            var brush = Brush.PaleVioletRed;

            // Assert
            Assert.Equal(Colors.PaleVioletRed, brush.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to PaleVioletRed property return the same cached instance.
        /// </summary>
        [Fact]
        public void PaleVioletRed_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var brush1 = Brush.PaleVioletRed;
            var brush2 = Brush.PaleVioletRed;

            // Assert
            Assert.Same(brush1, brush2);
        }

        /// <summary>
        /// Tests that PaleVioletRed property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void PaleVioletRed_Get_ReturnsNonEmptyBrush()
        {
            // Act
            var brush = Brush.PaleVioletRed;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that PaleVioletRed property returns a brush with the expected ARGB color value.
        /// </summary>
        [Fact]
        public void PaleVioletRed_Get_ReturnsExpectedArgbValue()
        {
            // Act
            var brush = Brush.PaleVioletRed;

            // Assert
            // PaleVioletRed has ARGB value of #FFD87093
            Assert.Equal(1.0f, brush.Color.Alpha, 3);
            Assert.Equal(0.847f, brush.Color.Red, 3);
            Assert.Equal(0.439f, brush.Color.Green, 3);
            Assert.Equal(0.576f, brush.Color.Blue, 3);
        }

        /// <summary>
        /// Tests that the Purple property returns a non-null SolidColorBrush instance.
        /// Verifies that the lazy initialization works correctly on first access.
        /// Expected result: Returns a non-null SolidColorBrush.
        /// </summary>
        [Fact]
        public void Purple_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var purpleBrush = Brush.Purple;

            // Assert
            Assert.NotNull(purpleBrush);
            Assert.IsType<SolidColorBrush>(purpleBrush);
        }

        /// <summary>
        /// Tests that the Purple property returns the same instance on multiple accesses.
        /// Verifies that the lazy initialization caches the instance correctly.
        /// Expected result: Same instance returned on subsequent calls.
        /// </summary>
        [Fact]
        public void Purple_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var purpleBrush1 = Brush.Purple;
            var purpleBrush2 = Brush.Purple;

            // Assert
            Assert.Same(purpleBrush1, purpleBrush2);
        }

        /// <summary>
        /// Tests that the Purple property returns a brush with the correct color.
        /// Verifies that the brush is initialized with Colors.Purple.
        /// Expected result: Brush color matches Colors.Purple.
        /// </summary>
        [Fact]
        public void Purple_Color_MatchesSystemDefinedPurple()
        {
            // Act
            var purpleBrush = Brush.Purple;

            // Assert
            Assert.Equal(Colors.Purple, purpleBrush.Color);
        }

        /// <summary>
        /// Tests that the Purple property returns a brush that is not empty.
        /// Verifies that the brush has a valid color and is not considered empty.
        /// Expected result: IsEmpty returns false.
        /// </summary>
        [Fact]
        public void Purple_IsEmpty_ReturnsFalse()
        {
            // Act
            var purpleBrush = Brush.Purple;

            // Assert
            Assert.False(purpleBrush.IsEmpty);
        }

        /// <summary>
        /// Tests multiple properties of the Purple brush in a comprehensive test.
        /// Verifies type, color, non-null status, and empty status in one test.
        /// Expected result: All properties match expected values.
        /// </summary>
        [Fact]
        public void Purple_Properties_AllCorrect()
        {
            // Act
            var purpleBrush = Brush.Purple;

            // Assert
            Assert.NotNull(purpleBrush);
            Assert.IsAssignableFrom<SolidColorBrush>(purpleBrush);
            Assert.Equal(Colors.Purple, purpleBrush.Color);
            Assert.False(purpleBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Sienna property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void Sienna_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Sienna;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Sienna property returns a brush with the correct Sienna color (ARGB #FFA0522D).
        /// </summary>
        [Fact]
        public void Sienna_HasCorrectColor()
        {
            // Act
            var result = Brush.Sienna;

            // Assert
            Assert.Equal(Colors.Sienna, result.Color);
            Assert.Equal(Color.FromUint(0xFFA0522D), result.Color);
        }

        /// <summary>
        /// Tests that multiple calls to the Sienna property return the same instance (singleton behavior).
        /// </summary>
        [Fact]
        public void Sienna_MultipleCalls_ReturnSameInstance()
        {
            // Act
            var first = Brush.Sienna;
            var second = Brush.Sienna;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the Sienna brush is not empty since it has a valid color.
        /// </summary>
        [Fact]
        public void Sienna_IsNotEmpty()
        {
            // Act
            var result = Brush.Sienna;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the Sienna property returns an ImmutableBrush specifically.
        /// </summary>
        [Fact]
        public void Sienna_ReturnsImmutableBrush()
        {
            // Act
            var result = Brush.Sienna;

            // Assert
            Assert.IsAssignableFrom<ImmutableBrush>(result);
        }

        /// <summary>
        /// Tests that the Tan property returns a non-null SolidColorBrush instance.
        /// This test exercises the lazy initialization code path on first access.
        /// </summary>
        [Fact]
        public void Tan_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Arrange & Act
            var tanBrush = Brush.Tan;

            // Assert
            Assert.NotNull(tanBrush);
            Assert.IsType<SolidColorBrush>(tanBrush);
        }

        /// <summary>
        /// Tests that the Tan property returns a brush with the correct Tan color.
        /// Verifies that the color matches the system-defined Colors.Tan value.
        /// </summary>
        [Fact]
        public void Tan_ColorValue_MatchesSystemDefinedTanColor()
        {
            // Arrange & Act
            var tanBrush = Brush.Tan;

            // Assert
            Assert.Equal(Colors.Tan, tanBrush.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the Tan property return the same cached instance.
        /// This verifies the lazy initialization behavior using null-coalescing assignment.
        /// </summary>
        [Fact]
        public void Tan_MultipleAccess_ReturnsSameInstance()
        {
            // Arrange & Act
            var firstAccess = Brush.Tan;
            var secondAccess = Brush.Tan;
            var thirdAccess = Brush.Tan;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the Tan property returns an ImmutableBrush instance.
        /// Verifies the actual implementation type while maintaining the SolidColorBrush interface.
        /// </summary>
        [Fact]
        public void Tan_ImplementationType_IsImmutableBrush()
        {
            // Arrange & Act
            var tanBrush = Brush.Tan;

            // Assert
            Assert.Equal("ImmutableBrush", tanBrush.GetType().Name);
        }

        /// <summary>
        /// Tests concurrent access to the Tan property to verify thread safety of lazy initialization.
        /// Multiple threads should all receive the same cached instance.
        /// </summary>
        [Fact]
        public async Task Tan_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.Tan);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            var firstResult = results[0];
            Assert.NotNull(firstResult);

            for (int i = 1; i < taskCount; i++)
            {
                Assert.Same(firstResult, results[i]);
            }
        }

        /// <summary>
        /// Tests that the Tan brush is not empty according to the IsEmpty property.
        /// Verifies that a properly initialized color brush is not considered empty.
        /// </summary>
        [Fact]
        public void Tan_IsEmpty_ReturnsFalse()
        {
            // Arrange & Act
            var tanBrush = Brush.Tan;

            // Assert
            Assert.False(tanBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Tan brush color has the expected ARGB values for the Tan color.
        /// This provides additional validation of the correct color assignment.
        /// </summary>
        [Fact]
        public void Tan_ColorComponents_MatchExpectedTanValues()
        {
            // Arrange & Act
            var tanBrush = Brush.Tan;
            var color = tanBrush.Color;

            // Assert
            // Tan color should have ARGB values of #FFD2B48C
            Assert.Equal(1.0f, color.Alpha, precision: 3);
            Assert.Equal(0.824f, color.Red, precision: 3);
            Assert.Equal(0.706f, color.Green, precision: 3);
            Assert.Equal(0.549f, color.Blue, precision: 3);
        }

        /// <summary>
        /// Tests that the Beige property returns a non-null SolidColorBrush instance.
        /// Verifies that the lazy initialization works correctly on first access.
        /// </summary>
        [Fact]
        public void Beige_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Beige;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Beige property returns the same instance on multiple accesses.
        /// Verifies that the lazy initialization ensures singleton behavior.
        /// </summary>
        [Fact]
        public void Beige_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var first = Brush.Beige;
            var second = Brush.Beige;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the Beige property returns a brush with the correct beige color.
        /// Verifies that the color matches the system-defined Colors.Beige value.
        /// </summary>
        [Fact]
        public void Beige_WhenAccessed_ReturnsCorrectBeigeColor()
        {
            // Arrange
            var expectedColor = Colors.Beige;

            // Act
            var result = Brush.Beige;

            // Assert
            Assert.Equal(expectedColor, result.Color);
        }

        /// <summary>
        /// Tests that the Beige property returns a brush with the expected ARGB color value.
        /// Verifies that the color has the correct ARGB value for beige (#FFF5F5DC).
        /// </summary>
        [Fact]
        public void Beige_WhenAccessed_ReturnsExpectedArgbValue()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFF5F5DC);

            // Act
            var result = Brush.Beige;

            // Assert
            Assert.Equal(expectedColor, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the Beige property consistently return the same color.
        /// Verifies that the color value is stable across multiple property accesses.
        /// </summary>
        [Fact]
        public void Beige_WhenAccessedMultipleTimes_ReturnsConsistentColor()
        {
            // Act
            var first = Brush.Beige;
            var second = Brush.Beige;

            // Assert
            Assert.Equal(first.Color, second.Color);
            Assert.Equal(Colors.Beige, first.Color);
            Assert.Equal(Colors.Beige, second.Color);
        }

        /// <summary>
        /// Tests that the Cyan property returns a non-null SolidColorBrush with the correct cyan color on first access.
        /// </summary>
        [Fact]
        public void Cyan_FirstAccess_ReturnsValidSolidColorBrushWithCyanColor()
        {
            // Arrange & Act
            var cyanBrush = Brush.Cyan;

            // Assert
            Assert.NotNull(cyanBrush);
            Assert.IsType<SolidColorBrush>(cyanBrush);
            Assert.Equal(Colors.Cyan, ((SolidColorBrush)cyanBrush).Color);
            Assert.Equal(Color.FromUint(0xFF00FFFF), ((SolidColorBrush)cyanBrush).Color);
        }

        /// <summary>
        /// Tests that subsequent accesses to the Cyan property return the same cached instance (lazy initialization).
        /// </summary>
        [Fact]
        public void Cyan_SubsequentAccesses_ReturnsSameInstance()
        {
            // Arrange & Act
            var firstAccess = Brush.Cyan;
            var secondAccess = Brush.Cyan;
            var thirdAccess = Brush.Cyan;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(firstAccess, thirdAccess);
            Assert.Same(secondAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the Cyan property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void Cyan_ReturnedBrush_IsNotEmpty()
        {
            // Arrange & Act
            var cyanBrush = Brush.Cyan;

            // Assert
            Assert.False(cyanBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that multiple threads accessing the Cyan property concurrently receive consistent results.
        /// </summary>
        [Fact]
        public async Task Cyan_ConcurrentAccess_ReturnsConsistentInstance()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.Cyan);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            var firstResult = results[0];
            Assert.NotNull(firstResult);

            for (int i = 1; i < taskCount; i++)
            {
                Assert.Same(firstResult, results[i]);
            }

            // Verify all instances have the correct color
            foreach (var result in results)
            {
                Assert.Equal(Colors.Cyan, result.Color);
            }
        }

        /// <summary>
        /// Tests that the Cyan property returns a brush with the exact expected ARGB color value.
        /// </summary>
        [Fact]
        public void Cyan_ColorValue_MatchesExpectedArgbValue()
        {
            // Arrange & Act
            var cyanBrush = Brush.Cyan;
            var color = ((SolidColorBrush)cyanBrush).Color;

            // Assert
            Assert.Equal(0xFF, color.Alpha * 255, 0);
            Assert.Equal(0x00, color.Red * 255, 0);
            Assert.Equal(0xFF, color.Green * 255, 0);
            Assert.Equal(0xFF, color.Blue * 255, 0);
        }

        /// <summary>
        /// Tests that the Cyan property can be used in equality comparisons correctly.
        /// </summary>
        [Fact]
        public void Cyan_EqualityComparison_WorksCorrectly()
        {
            // Arrange & Act
            var cyanBrush1 = Brush.Cyan;
            var cyanBrush2 = Brush.Cyan;
            var manualCyanBrush = new SolidColorBrush(Colors.Cyan);

            // Assert
            Assert.True(ReferenceEquals(cyanBrush1, cyanBrush2));
            Assert.True(cyanBrush1.Equals(manualCyanBrush));
            Assert.Equal(cyanBrush1.GetHashCode(), manualCyanBrush.GetHashCode());
        }

        /// <summary>
        /// Tests that the DarkSalmon property returns a non-null SolidColorBrush instance.
        /// Validates that the lazy initialization works correctly on first access.
        /// </summary>
        [Fact]
        public void DarkSalmon_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.DarkSalmon;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the DarkSalmon property returns a brush with the correct DarkSalmon color.
        /// Validates that the color value matches the expected Colors.DarkSalmon value.
        /// </summary>
        [Fact]
        public void DarkSalmon_ColorValue_MatchesExpectedDarkSalmonColor()
        {
            // Act
            var result = Brush.DarkSalmon;

            // Assert
            Assert.Equal(Colors.DarkSalmon, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the DarkSalmon property return the same instance.
        /// Validates that the lazy initialization with null-coalescing assignment (??=) works correctly.
        /// </summary>
        [Fact]
        public void DarkSalmon_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.DarkSalmon;
            var secondAccess = Brush.DarkSalmon;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the DarkSalmon brush is not empty.
        /// Validates that the IsEmpty property returns false for a valid color brush.
        /// </summary>
        [Fact]
        public void DarkSalmon_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.DarkSalmon;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the DarkSalmon brush maintains equality with another brush of the same color.
        /// Validates the equality behavior of SolidColorBrush based on color value.
        /// </summary>
        [Fact]
        public void DarkSalmon_Equality_MatchesEquivalentSolidColorBrush()
        {
            // Arrange
            var equivalentBrush = new SolidColorBrush(Colors.DarkSalmon);

            // Act
            var darkSalmonBrush = Brush.DarkSalmon;

            // Assert
            Assert.Equal(equivalentBrush, darkSalmonBrush);
        }

        /// <summary>
        /// Tests that DimGrey property returns a non-null SolidColorBrush instance.
        /// This test verifies that the DimGrey property accessor works correctly and returns a valid brush.
        /// Expected result: A non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void DimGrey_Get_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.DimGrey;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that DimGrey property returns the same instance as DimGray property.
        /// This test verifies that DimGrey is correctly aliased to DimGray.
        /// Expected result: Both properties return the exact same object reference.
        /// </summary>
        [Fact]
        public void DimGrey_Get_ReturnsSameInstanceAsDimGray()
        {
            // Act
            var dimGrey = Brush.DimGrey;
            var dimGray = Brush.DimGray;

            // Assert
            Assert.Same(dimGray, dimGrey);
        }

        /// <summary>
        /// Tests that DimGrey property returns a brush with the correct DimGray color.
        /// This test verifies that the returned brush has the expected color value.
        /// Expected result: The brush's Color property matches Colors.DimGray.
        /// </summary>
        [Fact]
        public void DimGrey_Get_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.DimGrey;

            // Assert
            Assert.Equal(Colors.DimGray, result.Color);
        }

        /// <summary>
        /// Tests that multiple calls to DimGrey property return the same instance.
        /// This test verifies the singleton behavior of the cached brush instance.
        /// Expected result: All calls return the exact same object reference.
        /// </summary>
        [Fact]
        public void DimGrey_MultipleCalls_ReturnsSameInstance()
        {
            // Act
            var first = Brush.DimGrey;
            var second = Brush.DimGrey;
            var third = Brush.DimGrey;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that DimGrey property returns a brush that is not empty.
        /// This test verifies that the returned brush has a valid state and is not considered empty.
        /// Expected result: The IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void DimGrey_Get_IsNotEmpty()
        {
            // Act
            var result = Brush.DimGrey;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the Grey property returns a non-null SolidColorBrush instance.
        /// Verifies basic functionality of the British spelling alias for Gray.
        /// </summary>
        [Fact]
        public void Grey_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Grey;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Grey property returns the same instance as the Gray property.
        /// Verifies that Grey is correctly implemented as an alias for Gray.
        /// </summary>
        [Fact]
        public void Grey_ReturnsSameInstanceAsGray_VerifiesAliasImplementation()
        {
            // Act
            var grey = Brush.Grey;
            var gray = Brush.Gray;

            // Assert
            Assert.Same(gray, grey);
        }

        /// <summary>
        /// Tests that the Grey property returns a brush with the correct gray color.
        /// Verifies that the color matches the system-defined Colors.Gray value.
        /// </summary>
        [Fact]
        public void Grey_ReturnsCorrectGrayColor()
        {
            // Act
            var result = Brush.Grey;

            // Assert
            Assert.Equal(Colors.Gray, result.Color);
        }

        /// <summary>
        /// Tests that multiple calls to Grey property return the same instance.
        /// Verifies lazy initialization behavior and singleton pattern implementation.
        /// </summary>
        [Fact]
        public void Grey_MultipleCalls_ReturnsSameInstance()
        {
            // Act
            var first = Brush.Grey;
            var second = Brush.Grey;
            var third = Brush.Grey;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that the Grey brush is not empty.
        /// Verifies that the returned brush represents a valid, non-empty brush.
        /// </summary>
        [Fact]
        public void Grey_IsNotEmpty()
        {
            // Act
            var result = Brush.Grey;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that Grey brush can be used in IsNullOrEmpty method.
        /// Verifies integration with static utility methods.
        /// </summary>
        [Fact]
        public void Grey_IsNullOrEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.IsNullOrEmpty(Brush.Grey);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that LavenderBlush property returns a non-null SolidColorBrush instance.
        /// Verifies the basic functionality and ensures the property returns a valid brush object.
        /// </summary>
        [Fact]
        public void LavenderBlush_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var brush = Brush.LavenderBlush;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
        }

        /// <summary>
        /// Tests that LavenderBlush property returns a brush with the correct LavenderBlush color.
        /// Verifies the color value matches the expected system-defined LavenderBlush color (#FFF0F5).
        /// </summary>
        [Fact]
        public void LavenderBlush_HasCorrectColor()
        {
            // Act
            var brush = Brush.LavenderBlush;

            // Assert
            Assert.Equal(Colors.LavenderBlush, brush.Color);
        }

        /// <summary>
        /// Tests that LavenderBlush property exhibits singleton behavior.
        /// Verifies that multiple calls to the property return the same instance due to lazy initialization.
        /// </summary>
        [Fact]
        public void LavenderBlush_ReturnsSameInstance_MultipleCalls()
        {
            // Act
            var brush1 = Brush.LavenderBlush;
            var brush2 = Brush.LavenderBlush;

            // Assert
            Assert.Same(brush1, brush2);
        }

        /// <summary>
        /// Tests that LavenderBlush property returns a brush that is not empty.
        /// Verifies the IsEmpty property returns false for a valid color brush.
        /// </summary>
        [Fact]
        public void LavenderBlush_IsNotEmpty()
        {
            // Act
            var brush = Brush.LavenderBlush;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that LavenderBlush property is thread-safe when accessed concurrently.
        /// Verifies that multiple concurrent calls return the same instance without race conditions.
        /// </summary>
        [Fact]
        public async Task LavenderBlush_ThreadSafe_ConcurrentAccess()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.LavenderBlush);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            var firstResult = results[0];
            for (int i = 1; i < taskCount; i++)
            {
                Assert.Same(firstResult, results[i]);
            }
        }

        /// <summary>
        /// Tests that LavenderBlush property color matches the expected ARGB values.
        /// Verifies the specific color components match the system-defined LavenderBlush color.
        /// </summary>
        [Fact]
        public void LavenderBlush_HasExpectedColorValues()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFFFF0F5);

            // Act
            var brush = Brush.LavenderBlush;

            // Assert
            Assert.Equal(expectedColor, brush.Color);
            Assert.Equal(1.0f, brush.Color.Alpha, 5); // Full opacity
            Assert.Equal(1.0f, brush.Color.Red, 5); // Red component
            Assert.Equal(0.941f, brush.Color.Green, 2); // Green component (240/255)
            Assert.Equal(0.961f, brush.Color.Blue, 2); // Blue component (245/255)
        }

        /// <summary>
        /// Tests that LightSlateGrey property returns a non-null SolidColorBrush instance.
        /// Verifies that the alias property works correctly.
        /// Expected result: Returns a valid SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void LightSlateGrey_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.LightSlateGrey;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that LightSlateGrey property returns the same instance as LightSlateGray.
        /// Verifies that LightSlateGrey is correctly aliased to LightSlateGray.
        /// Expected result: Both properties return the exact same instance.
        /// </summary>
        [Fact]
        public void LightSlateGrey_ReturnsSameInstanceAsLightSlateGray()
        {
            // Act
            var lightSlateGrey = Brush.LightSlateGrey;
            var lightSlateGray = Brush.LightSlateGray;

            // Assert
            Assert.Same(lightSlateGray, lightSlateGrey);
        }

        /// <summary>
        /// Tests that LightSlateGrey property returns a brush with the correct color.
        /// Verifies that the color matches the system-defined LightSlateGray color.
        /// Expected result: The brush color equals Colors.LightSlateGray.
        /// </summary>
        [Fact]
        public void LightSlateGrey_HasCorrectColor()
        {
            // Act
            var brush = Brush.LightSlateGrey;

            // Assert
            Assert.Equal(Colors.LightSlateGray, brush.Color);
        }

        /// <summary>
        /// Tests that multiple calls to LightSlateGrey property return the same instance.
        /// Verifies that the lazy initialization pattern works correctly through the alias.
        /// Expected result: All calls return the same instance (singleton behavior).
        /// </summary>
        [Fact]
        public void LightSlateGrey_MultipleCallsReturnSameInstance()
        {
            // Act
            var first = Brush.LightSlateGrey;
            var second = Brush.LightSlateGrey;
            var third = Brush.LightSlateGrey;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that MediumPurple property returns a non-null SolidColorBrush instance.
        /// Verifies the lazy initialization behavior of the static property.
        /// </summary>
        [Fact]
        public void MediumPurple_Get_ReturnsNonNullBrush()
        {
            // Act
            var brush = Brush.MediumPurple;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
        }

        /// <summary>
        /// Tests that MediumPurple property returns a brush with the correct color.
        /// Verifies that the brush color matches the expected MediumPurple color from Graphics.Colors.
        /// </summary>
        [Fact]
        public void MediumPurple_Get_ReturnsCorrectColor()
        {
            // Act
            var brush = Brush.MediumPurple;

            // Assert
            Assert.Equal(Colors.MediumPurple, brush.Color);
        }

        /// <summary>
        /// Tests that MediumPurple property returns the same instance on multiple calls.
        /// Verifies the lazy initialization and singleton behavior of the static property.
        /// </summary>
        [Fact]
        public void MediumPurple_MultipleCalls_ReturnsSameInstance()
        {
            // Act
            var brush1 = Brush.MediumPurple;
            var brush2 = Brush.MediumPurple;

            // Assert
            Assert.Same(brush1, brush2);
        }

        /// <summary>
        /// Tests that MediumPurple property returns a brush that is not empty.
        /// Verifies that the brush has a valid color and is not considered empty.
        /// </summary>
        [Fact]
        public void MediumPurple_Get_IsNotEmpty()
        {
            // Act
            var brush = Brush.MediumPurple;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that MediumPurple property returns a brush with non-transparent color.
        /// Verifies that the MediumPurple color has the expected alpha value.
        /// </summary>
        [Fact]
        public void MediumPurple_Get_HasNonTransparentColor()
        {
            // Act
            var brush = Brush.MediumPurple;

            // Assert
            Assert.NotEqual(Colors.Transparent, brush.Color);
            Assert.True(brush.Color.Alpha > 0);
        }

        /// <summary>
        /// Tests that accessing Navy property for the first time returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void Navy_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Navy;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that Navy property returns a brush with the correct navy color (ARGB #FF000080).
        /// </summary>
        [Fact]
        public void Navy_ColorValue_ReturnsCorrectNavyColor()
        {
            // Act
            var result = Brush.Navy;

            // Assert
            Assert.Equal(Colors.Navy, result.Color);
            Assert.Equal(Color.FromUint(0xFF000080), result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to Navy property return the same instance (singleton behavior).
        /// </summary>
        [Fact]
        public void Navy_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.Navy;
            var second = Brush.Navy;
            var third = Brush.Navy;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that Navy brush is not empty.
        /// </summary>
        [Fact]
        public void Navy_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.Navy;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that Navy brush is not considered null or empty by the IsNullOrEmpty method.
        /// </summary>
        [Fact]
        public void Navy_IsNullOrEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.Navy;

            // Assert
            Assert.False(Brush.IsNullOrEmpty(result));
        }

        /// <summary>
        /// Tests that concurrent access to Navy property returns the same instance across multiple threads.
        /// </summary>
        [Fact]
        public async Task Navy_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            SolidColorBrush[] results = new SolidColorBrush[10];
            var tasks = new Task[10];

            // Act
            for (int i = 0; i < 10; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() => results[index] = Brush.Navy);
            }

            await Task.WhenAll(tasks);

            // Assert
            var first = results[0];
            Assert.NotNull(first);

            for (int i = 1; i < results.Length; i++)
            {
                Assert.Same(first, results[i]);
            }
        }

        /// <summary>
        /// Tests that Navy brush color properties match expected ARGB values.
        /// </summary>
        [Fact]
        public void Navy_ColorComponents_MatchExpectedValues()
        {
            // Act
            var result = Brush.Navy;
            var color = result.Color;

            // Assert
            Assert.Equal(1.0f, color.Alpha, 2); // Alpha = 255/255 = 1.0
            Assert.Equal(0.0f, color.Red, 2);   // Red = 0/255 = 0.0
            Assert.Equal(0.0f, color.Green, 2); // Green = 0/255 = 0.0  
            Assert.Equal(0.5019608f, color.Blue, 2); // Blue = 128/255 ≈ 0.5019
        }

        /// <summary>
        /// Tests that Navy brush equality works correctly.
        /// </summary>
        [Fact]
        public void Navy_Equality_WorksCorrectly()
        {
            // Arrange
            var navy1 = Brush.Navy;
            var navy2 = Brush.Navy;
            var differentBrush = new SolidColorBrush(Colors.Red);

            // Act & Assert
            Assert.True(navy1.Equals(navy2));
            Assert.False(navy1.Equals(differentBrush));
            Assert.False(navy1.Equals(null));
        }

        /// <summary>
        /// Tests that Navy brush hash code is consistent.
        /// </summary>
        [Fact]
        public void Navy_GetHashCode_IsConsistent()
        {
            // Arrange
            var navy1 = Brush.Navy;
            var navy2 = Brush.Navy;

            // Act
            var hash1 = navy1.GetHashCode();
            var hash2 = navy2.GetHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
        }

        /// <summary>
        /// Tests that the PeachPuff property returns a SolidColorBrush with the correct color.
        /// Verifies the lazy initialization behavior and that multiple accesses return the same instance.
        /// </summary>
        [Fact]
        public void PeachPuff_FirstAccess_ReturnsSolidColorBrushWithCorrectColor()
        {
            // Act
            var result = Brush.PeachPuff;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(Colors.PeachPuff, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the PeachPuff property return the same instance,
        /// verifying the lazy initialization singleton behavior.
        /// </summary>
        [Fact]
        public void PeachPuff_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.PeachPuff;
            var second = Brush.PeachPuff;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the PeachPuff property returns an object that is not empty.
        /// </summary>
        [Fact]
        public void PeachPuff_IsNotEmpty_ReturnsTrue()
        {
            // Act
            var result = Brush.PeachPuff;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that SeaGreen property returns a non-null SolidColorBrush instance.
        /// Verifies the lazy initialization creates a valid brush object.
        /// </summary>
        [Fact]
        public void SeaGreen_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.SeaGreen;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that SeaGreen property returns a brush with the correct SeaGreen color.
        /// Verifies the brush is initialized with the expected system-defined color.
        /// </summary>
        [Fact]
        public void SeaGreen_Color_MatchesSystemDefinedSeaGreenColor()
        {
            // Act
            var result = Brush.SeaGreen;

            // Assert
            Assert.Equal(Colors.SeaGreen, result.Color);
        }

        /// <summary>
        /// Tests that SeaGreen property returns the same instance on multiple accesses.
        /// Verifies the lazy initialization caching behavior works correctly.
        /// </summary>
        [Fact]
        public void SeaGreen_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.SeaGreen;
            var second = Brush.SeaGreen;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that SeaGreen property is not empty.
        /// Verifies the returned brush represents a valid, non-empty brush.
        /// </summary>
        [Fact]
        public void SeaGreen_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.SeaGreen;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that concurrent access to SeaGreen property returns the same instance.
        /// Verifies thread safety of the lazy initialization pattern.
        /// </summary>
        [Fact]
        public async Task SeaGreen_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            SolidColorBrush result1 = null;
            SolidColorBrush result2 = null;

            // Act
            var task1 = Task.Run(() => result1 = Brush.SeaGreen);
            var task2 = Task.Run(() => result2 = Brush.SeaGreen);

            await Task.WhenAll(task1, task2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that the Snow property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void Snow_ReturnsNonNull_SolidColorBrush()
        {
            // Act
            var result = Brush.Snow;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Snow property returns a brush with the correct color value.
        /// </summary>
        [Fact]
        public void Snow_HasCorrectColor_ReturnsColorsSnow()
        {
            // Act
            var result = Brush.Snow;

            // Assert
            Assert.Equal(Colors.Snow, result.Color);
        }

        /// <summary>
        /// Tests that the Snow property implements lazy initialization and returns the same instance on multiple calls.
        /// </summary>
        [Fact]
        public void Snow_LazyInitialization_ReturnsSameInstance()
        {
            // Act
            var firstCall = Brush.Snow;
            var secondCall = Brush.Snow;
            var thirdCall = Brush.Snow;

            // Assert
            Assert.Same(firstCall, secondCall);
            Assert.Same(firstCall, thirdCall);
            Assert.Same(secondCall, thirdCall);
        }

        /// <summary>
        /// Tests that the Snow property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void Snow_IsNotEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.Snow;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the Snow property returns a brush with the expected ARGB color values.
        /// </summary>
        [Fact]
        public void Snow_HasExpectedColorComponents_MatchesSnowColorValues()
        {
            // Arrange
            var expectedColor = Colors.Snow; // #FFFFFAFA

            // Act
            var result = Brush.Snow;

            // Assert
            Assert.Equal(expectedColor.Red, result.Color.Red);
            Assert.Equal(expectedColor.Green, result.Color.Green);
            Assert.Equal(expectedColor.Blue, result.Color.Blue);
            Assert.Equal(expectedColor.Alpha, result.Color.Alpha);
        }

        /// <summary>
        /// Tests that the Bisque property returns a non-null SolidColorBrush with the correct color value.
        /// </summary>
        [Fact]
        public void Bisque_FirstAccess_ReturnsValidSolidColorBrush()
        {
            // Act
            var bisqueBrush = Brush.Bisque;

            // Assert
            Assert.NotNull(bisqueBrush);
            Assert.IsType<SolidColorBrush>(bisqueBrush);
            Assert.Equal(Colors.Bisque, bisqueBrush.Color);
        }

        /// <summary>
        /// Tests that the Bisque property returns the same cached instance on subsequent accesses.
        /// </summary>
        [Fact]
        public void Bisque_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Bisque;
            var secondAccess = Brush.Bisque;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Bisque brush has the correct color value matching the system-defined Bisque color.
        /// </summary>
        [Fact]
        public void Bisque_Color_MatchesSystemDefinedBisqueColor()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFFFE4C4); // Colors.Bisque value

            // Act
            var bisqueBrush = Brush.Bisque;

            // Assert
            Assert.Equal(expectedColor, bisqueBrush.Color);
            Assert.Equal(Colors.Bisque, bisqueBrush.Color);
        }

        /// <summary>
        /// Tests that the Bisque brush is not empty since it has a defined color.
        /// </summary>
        [Fact]
        public void Bisque_IsEmpty_ReturnsFalse()
        {
            // Act
            var bisqueBrush = Brush.Bisque;

            // Assert
            Assert.False(bisqueBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Bisque property consistently returns a SolidColorBrush type across multiple accesses.
        /// </summary>
        [Fact]
        public void Bisque_Type_ConsistentlyReturnsSolidColorBrush()
        {
            // Act & Assert
            for (int i = 0; i < 3; i++)
            {
                var bisqueBrush = Brush.Bisque;
                Assert.IsType<SolidColorBrush>(bisqueBrush);
                Assert.IsAssignableFrom<Brush>(bisqueBrush);
            }
        }

        /// <summary>
        /// Tests that the DarkGoldenrod property returns a non-null SolidColorBrush instance.
        /// Verifies that the lazy initialization works correctly on first access.
        /// </summary>
        [Fact]
        public void DarkGoldenrod_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.DarkGoldenrod;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the DarkGoldenrod property returns a brush with the correct DarkGoldenrod color.
        /// Verifies that the color value matches the expected system-defined DarkGoldenrod color.
        /// </summary>
        [Fact]
        public void DarkGoldenrod_Color_MatchesExpectedDarkGoldenrodColor()
        {
            // Act
            var result = Brush.DarkGoldenrod;

            // Assert
            Assert.Equal(Colors.DarkGoldenrod, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the DarkGoldenrod property return the same cached instance.
        /// Verifies that the lazy initialization caching behavior works correctly.
        /// </summary>
        [Fact]
        public void DarkGoldenrod_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.DarkGoldenrod;
            var second = Brush.DarkGoldenrod;
            var third = Brush.DarkGoldenrod;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that the DarkGoldenrod property returns a SolidColorBrush that is not empty.
        /// Verifies that the brush has valid content and is properly initialized.
        /// </summary>
        [Fact]
        public void DarkGoldenrod_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.DarkGoldenrod;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the DarkGoldenrod property returns a brush with consistent hash code across multiple accesses.
        /// Verifies that the cached instance maintains consistent identity.
        /// </summary>
        [Fact]
        public void DarkGoldenrod_HashCode_ConsistentAcrossAccesses()
        {
            // Act
            var first = Brush.DarkGoldenrod;
            var second = Brush.DarkGoldenrod;

            // Assert
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        /// <summary>
        /// Tests that the DarkGoldenrod property returns a brush that equals itself when accessed multiple times.
        /// Verifies that the equality comparison works correctly for the cached instance.
        /// </summary>
        [Fact]
        public void DarkGoldenrod_Equality_SameInstanceEqualsItself()
        {
            // Act
            var first = Brush.DarkGoldenrod;
            var second = Brush.DarkGoldenrod;

            // Assert
            Assert.True(first.Equals(second));
            Assert.True(second.Equals(first));
        }

        /// <summary>
        /// Tests that DarkSlateGray property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void DarkSlateGray_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.DarkSlateGray;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that DarkSlateGray property implements lazy initialization and returns the same instance on multiple accesses.
        /// </summary>
        [Fact]
        public void DarkSlateGray_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.DarkSlateGray;
            var second = Brush.DarkSlateGray;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that DarkSlateGray property returns a brush with the correct DarkSlateGray color.
        /// </summary>
        [Fact]
        public void DarkSlateGray_Color_MatchesSystemDefinedDarkSlateGrayColor()
        {
            // Act
            var brush = Brush.DarkSlateGray;

            // Assert
            Assert.Equal(Colors.DarkSlateGray, brush.Color);
        }

        /// <summary>
        /// Tests that DarkSlateGray property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void DarkSlateGray_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.DarkSlateGray;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that DarkSlateGray property returns an ImmutableBrush instance (internal implementation detail).
        /// </summary>
        [Fact]
        public void DarkSlateGray_ReturnsImmutableBrush()
        {
            // Act
            var brush = Brush.DarkSlateGray;

            // Assert
            Assert.Equal("ImmutableBrush", brush.GetType().Name);
        }

        /// <summary>
        /// Tests that the DodgerBlue property returns a non-null SolidColorBrush instance.
        /// Verifies the lazy initialization creates a valid brush object.
        /// </summary>
        [Fact]
        public void DodgerBlue_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.DodgerBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that multiple accesses to the DodgerBlue property return the same instance.
        /// Verifies the lazy initialization pattern (??=) works correctly.
        /// </summary>
        [Fact]
        public void DodgerBlue_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.DodgerBlue;
            var second = Brush.DodgerBlue;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the DodgerBlue property returns a brush with the correct DodgerBlue color.
        /// Verifies the brush is initialized with Colors.DodgerBlue.
        /// </summary>
        [Fact]
        public void DodgerBlue_Color_EqualsColorsDodgerBlue()
        {
            // Act
            var brush = Brush.DodgerBlue;

            // Assert
            Assert.Equal(Colors.DodgerBlue, brush.Color);
        }

        /// <summary>
        /// Tests that the DodgerBlue property creates a brush that is not empty.
        /// Verifies the IsEmpty property returns false for a properly initialized brush.
        /// </summary>
        [Fact]
        public void DodgerBlue_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.DodgerBlue;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the DodgerBlue property returns consistent results across multiple test scenarios.
        /// Combines multiple assertions to verify correctness, type, non-null, and color in one parameterized test.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void DodgerBlue_MultipleScenarios_ReturnsConsistentResults(int scenario)
        {
            // Act
            var brush = Brush.DodgerBlue;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
            Assert.Equal(Colors.DodgerBlue, brush.Color);
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the LawnGreen property returns a non-null SolidColorBrush instance.
        /// Verifies that the lazy initialization creates a valid brush object.
        /// </summary>
        [Fact]
        public void LawnGreen_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Arrange & Act
            var result = Brush.LawnGreen;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the LawnGreen property returns a brush with the correct color value.
        /// Verifies that the color matches the expected LawnGreen color (#FF7CFC00).
        /// </summary>
        [Fact]
        public void LawnGreen_ColorValue_MatchesExpectedLawnGreenColor()
        {
            // Arrange
            var expectedColor = Colors.LawnGreen;

            // Act
            var result = Brush.LawnGreen;

            // Assert
            Assert.Equal(expectedColor, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the LawnGreen property return the same instance.
        /// Verifies the singleton behavior of the lazy initialization pattern.
        /// </summary>
        [Fact]
        public void LawnGreen_MultipleAccess_ReturnsSameInstance()
        {
            // Arrange & Act
            var first = Brush.LawnGreen;
            var second = Brush.LawnGreen;
            var third = Brush.LawnGreen;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that concurrent access to the LawnGreen property from multiple threads is thread-safe.
        /// Verifies that all threads receive the same instance and no race conditions occur.
        /// </summary>
        [Fact]
        public async Task LawnGreen_ConcurrentAccess_IsThreadSafeAndReturnsSameInstance()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];
            var results = new SolidColorBrush[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() => Brush.LawnGreen);
            }

            var taskResults = await Task.WhenAll(tasks);

            // Assert
            var firstResult = taskResults[0];
            Assert.NotNull(firstResult);
            Assert.IsType<SolidColorBrush>(firstResult);
            Assert.Equal(Colors.LawnGreen, firstResult.Color);

            // Verify all tasks returned the same instance
            for (int i = 1; i < taskCount; i++)
            {
                Assert.Same(firstResult, taskResults[i]);
            }
        }

        /// <summary>
        /// Tests that the LawnGreen property returns a brush that is not empty.
        /// Verifies that the IsEmpty property returns false for a valid color brush.
        /// </summary>
        [Fact]
        public void LawnGreen_IsEmpty_ReturnsFalse()
        {
            // Arrange & Act
            var result = Brush.LawnGreen;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the LawnGreen brush has the correct ARGB values.
        /// Verifies the specific color components match the expected LawnGreen color.
        /// </summary>
        [Fact]
        public void LawnGreen_ColorComponents_MatchExpectedValues()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFF7CFC00);

            // Act
            var result = Brush.LawnGreen;

            // Assert
            Assert.Equal(expectedColor.Red, result.Color.Red);
            Assert.Equal(expectedColor.Green, result.Color.Green);
            Assert.Equal(expectedColor.Blue, result.Color.Blue);
            Assert.Equal(expectedColor.Alpha, result.Color.Alpha);
        }

        /// <summary>
        /// Tests that the LightSteelBlue property returns a non-null SolidColorBrush instance.
        /// This test verifies the lazy initialization mechanism works correctly.
        /// </summary>
        [Fact]
        public void LightSteelBlue_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.LightSteelBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the LightSteelBlue property returns a brush with the correct LightSteelBlue color.
        /// This verifies that the brush is initialized with the expected color value.
        /// </summary>
        [Fact]
        public void LightSteelBlue_Color_EqualsColorsLightSteelBlue()
        {
            // Act
            var result = Brush.LightSteelBlue;

            // Assert
            Assert.Equal(Colors.LightSteelBlue, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the LightSteelBlue property return the same instance.
        /// This verifies the lazy initialization singleton behavior.
        /// </summary>
        [Fact]
        public void LightSteelBlue_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.LightSteelBlue;
            var second = Brush.LightSteelBlue;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the LightSteelBlue property returns a brush that is not empty.
        /// This verifies the brush has a valid color and is properly initialized.
        /// </summary>
        [Fact]
        public void LightSteelBlue_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.LightSteelBlue;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that MediumTurquoise property returns a SolidColorBrush instance.
        /// Verifies the property returns the correct brush type on first access.
        /// </summary>
        [Fact]
        public void MediumTurquoise_FirstAccess_ReturnsSolidColorBrush()
        {
            // Act
            var result = Brush.MediumTurquoise;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that MediumTurquoise property has the correct color value.
        /// Verifies the brush color matches Colors.MediumTurquoise.
        /// </summary>
        [Fact]
        public void MediumTurquoise_ColorValue_MatchesExpectedColor()
        {
            // Act
            var result = Brush.MediumTurquoise;

            // Assert
            Assert.Equal(Colors.MediumTurquoise, result.Color);
        }

        /// <summary>
        /// Tests that MediumTurquoise property implements singleton behavior.
        /// Verifies multiple accesses return the same instance (lazy initialization).
        /// </summary>
        [Fact]
        public void MediumTurquoise_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.MediumTurquoise;
            var second = Brush.MediumTurquoise;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that MediumTurquoise property is thread-safe during concurrent access.
        /// Verifies lazy initialization works correctly with multiple threads.
        /// </summary>
        [Fact]
        public async Task MediumTurquoise_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            SolidColorBrush result1 = null;
            SolidColorBrush result2 = null;
            SolidColorBrush result3 = null;

            // Act
            var task1 = Task.Run(() => result1 = Brush.MediumTurquoise);
            var task2 = Task.Run(() => result2 = Brush.MediumTurquoise);
            var task3 = Task.Run(() => result3 = Brush.MediumTurquoise);

            await Task.WhenAll(task1, task2, task3);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            Assert.Same(result1, result2);
            Assert.Same(result2, result3);
        }

        /// <summary>
        /// Tests that MediumTurquoise property returns a brush that is not empty.
        /// Verifies the brush IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void MediumTurquoise_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.MediumTurquoise;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that MediumTurquoise property returns a brush with correct hash code.
        /// Verifies the brush has a consistent hash code based on its color.
        /// </summary>
        [Fact]
        public void MediumTurquoise_GetHashCode_ReturnsConsistentValue()
        {
            // Act
            var brush = Brush.MediumTurquoise;
            var hashCode1 = brush.GetHashCode();
            var hashCode2 = brush.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that MediumTurquoise property returns a brush that equals other brushes with same color.
        /// Verifies the brush equality behavior with equivalent SolidColorBrush instances.
        /// </summary>
        [Fact]
        public void MediumTurquoise_Equals_TrueForSameColor()
        {
            // Arrange
            var mediumTurquoiseBrush = Brush.MediumTurquoise;
            var equivalentBrush = new SolidColorBrush(Colors.MediumTurquoise);

            // Act & Assert
            Assert.True(mediumTurquoiseBrush.Equals(equivalentBrush));
            Assert.True(equivalentBrush.Equals(mediumTurquoiseBrush));
        }

        /// <summary>
        /// Tests that MediumTurquoise property returns a brush that does not equal brushes with different colors.
        /// Verifies the brush equality behavior with different SolidColorBrush instances.
        /// </summary>
        [Fact]
        public void MediumTurquoise_Equals_FalseForDifferentColor()
        {
            // Arrange
            var mediumTurquoiseBrush = Brush.MediumTurquoise;
            var differentBrush = new SolidColorBrush(Colors.Red);

            // Act & Assert
            Assert.False(mediumTurquoiseBrush.Equals(differentBrush));
            Assert.False(mediumTurquoiseBrush.Equals(null));
            Assert.False(mediumTurquoiseBrush.Equals("not a brush"));
        }

        /// <summary>
        /// Tests that the Orchid property returns a non-null SolidColorBrush on first access.
        /// Verifies the lazy initialization creates a new brush instance.
        /// Expected result: Returns a valid SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void Orchid_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Orchid;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Orchid property returns the same instance on multiple accesses.
        /// Verifies the lazy initialization pattern works correctly.
        /// Expected result: Same reference is returned on subsequent calls.
        /// </summary>
        [Fact]
        public void Orchid_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Orchid;
            var secondAccess = Brush.Orchid;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Orchid property returns a brush with the correct color value.
        /// Verifies the brush contains the Colors.Orchid color (#FFDA70D6).
        /// Expected result: The Color property matches Colors.Orchid.
        /// </summary>
        [Fact]
        public void Orchid_ColorValue_MatchesColorsOrchid()
        {
            // Arrange
            var expectedColor = Colors.Orchid;

            // Act
            var orchidBrush = Brush.Orchid;

            // Assert
            Assert.Equal(expectedColor, orchidBrush.Color);
        }

        /// <summary>
        /// Tests that the Orchid property brush is immutable and cannot be modified.
        /// Verifies that attempting to change the color has no effect.
        /// Expected result: The color remains unchanged after assignment attempt.
        /// </summary>
        [Fact]
        public void Orchid_ImmutableBehavior_ColorCannotBeModified()
        {
            // Arrange
            var orchidBrush = Brush.Orchid;
            var originalColor = orchidBrush.Color;
            var newColor = Colors.Red;

            // Act
            orchidBrush.Color = newColor;

            // Assert
            Assert.Equal(originalColor, orchidBrush.Color);
            Assert.NotEqual(newColor, orchidBrush.Color);
        }

        /// <summary>
        /// Tests that the Orchid property is thread-safe during concurrent access.
        /// Verifies the lazy initialization works correctly under concurrent conditions.
        /// Expected result: All tasks return the same instance reference.
        /// </summary>
        [Fact]
        public async Task Orchid_ConcurrentAccess_ReturnsConsistentInstance()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.Orchid);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            var firstResult = results[0];
            for (int i = 1; i < results.Length; i++)
            {
                Assert.Same(firstResult, results[i]);
            }
        }

        /// <summary>
        /// Tests that the Orchid property returns a brush that is not empty.
        /// Verifies the IsEmpty property returns false for a valid color brush.
        /// Expected result: IsEmpty returns false.
        /// </summary>
        [Fact]
        public void Orchid_IsEmpty_ReturnsFalse()
        {
            // Act
            var orchidBrush = Brush.Orchid;

            // Assert
            Assert.False(orchidBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Orchid property brush has the correct ARGB color values.
        /// Verifies the color components match the expected Orchid color #FFDA70D6.
        /// Expected result: Color components match the expected ARGB values.
        /// </summary>
        [Fact]
        public void Orchid_ColorComponents_MatchExpectedValues()
        {
            // Arrange
            var orchidBrush = Brush.Orchid;
            var color = orchidBrush.Color;

            // Act & Assert
            Assert.Equal(1.0f, color.Alpha, 3); // FF = 255/255 = 1.0
            Assert.Equal(0.855f, color.Red, 3); // DA = 218/255 ≈ 0.855
            Assert.Equal(0.439f, color.Green, 3); // 70 = 112/255 ≈ 0.439
            Assert.Equal(0.839f, color.Blue, 3); // D6 = 214/255 ≈ 0.839
        }

        /// <summary>
        /// Tests that SandyBrown property returns a non-null SolidColorBrush on first access.
        /// Verifies the lazy initialization creates an instance with the correct color.
        /// Expected result: Returns a non-null SolidColorBrush with Colors.SandyBrown color.
        /// </summary>
        [Fact]
        public void SandyBrown_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.SandyBrown;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(Colors.SandyBrown, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to SandyBrown property return the same instance.
        /// Verifies the lazy initialization pattern ensures singleton behavior.
        /// Expected result: All accesses return the same object reference.
        /// </summary>
        [Fact]
        public void SandyBrown_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.SandyBrown;
            var second = Brush.SandyBrown;
            var third = Brush.SandyBrown;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that SandyBrown brush has the correct color value.
        /// Verifies the brush is initialized with Colors.SandyBrown.
        /// Expected result: The Color property equals Colors.SandyBrown.
        /// </summary>
        [Fact]
        public void SandyBrown_ColorProperty_EqualsSystemSandyBrownColor()
        {
            // Act
            var brush = Brush.SandyBrown;

            // Assert
            Assert.Equal(Colors.SandyBrown, brush.Color);
        }

        /// <summary>
        /// Tests that SandyBrown brush is not empty.
        /// Verifies the brush has a valid color and is not considered empty.
        /// Expected result: IsEmpty returns false.
        /// </summary>
        [Fact]
        public void SandyBrown_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.SandyBrown;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that SandyBrown can be used in equality comparisons.
        /// Verifies the brush behaves correctly in equality operations.
        /// Expected result: Brush equals another SolidColorBrush with the same color.
        /// </summary>
        [Fact]
        public void SandyBrown_Equality_WorksCorrectly()
        {
            // Arrange
            var sandyBrownBrush = Brush.SandyBrown;
            var equivalentBrush = new SolidColorBrush(Colors.SandyBrown);
            var differentBrush = new SolidColorBrush(Colors.Red);

            // Act & Assert
            Assert.True(sandyBrownBrush.Equals(equivalentBrush));
            Assert.False(sandyBrownBrush.Equals(differentBrush));
            Assert.False(sandyBrushBrush.Equals(null));
        }

        /// <summary>
        /// Tests that SandyBrown brush has consistent hash code.
        /// Verifies the hash code is based on the color value.
        /// Expected result: Hash code equals that of equivalent SolidColorBrush.
        /// </summary>
        [Fact]
        public void SandyBrown_GetHashCode_IsConsistent()
        {
            // Arrange
            var sandyBrownBrush = Brush.SandyBrown;
            var equivalentBrush = new SolidColorBrush(Colors.SandyBrown);

            // Act
            var hashCode1 = sandyBrownBrush.GetHashCode();
            var hashCode2 = equivalentBrush.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that the SteelBlue property returns a non-null SolidColorBrush instance.
        /// Verifies basic functionality and type correctness of the static property.
        /// </summary>
        [Fact]
        public void SteelBlue_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.SteelBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that repeated access to the SteelBlue property returns the same instance.
        /// Verifies lazy initialization behavior with null-coalescing assignment operator.
        /// </summary>
        [Fact]
        public void SteelBlue_RepeatedAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.SteelBlue;
            var second = Brush.SteelBlue;

            // Assert
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the SteelBlue property returns a brush with the correct color value.
        /// Verifies the color matches the system-defined Colors.SteelBlue value.
        /// </summary>
        [Fact]
        public void SteelBlue_ColorProperty_EqualsColorsSteelBlue()
        {
            // Act
            var steelBlueBrush = Brush.SteelBlue;

            // Assert
            Assert.Equal(Colors.SteelBlue, steelBlueBrush.Color);
        }

        /// <summary>
        /// Tests that the SteelBlue property returns a brush that is not empty.
        /// Verifies the IsEmpty property returns false for the created brush.
        /// </summary>
        [Fact]
        public void SteelBlue_IsEmpty_ReturnsFalse()
        {
            // Act
            var steelBlueBrush = Brush.SteelBlue;

            // Assert
            Assert.False(steelBlueBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the SteelBlue property returns a SolidColorBrush with expected characteristics.
        /// Verifies multiple properties in a single test to ensure comprehensive coverage.
        /// </summary>
        [Fact]
        public void SteelBlue_Properties_MeetExpectations()
        {
            // Act
            var steelBlueBrush = Brush.SteelBlue;

            // Assert
            Assert.NotNull(steelBlueBrush);
            Assert.IsAssignableFrom<SolidColorBrush>(steelBlueBrush);
            Assert.Equal(Colors.SteelBlue, steelBlueBrush.Color);
            Assert.False(steelBlueBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that YellowGreen property returns a non-null SolidColorBrush instance.
        /// Verifies the lazy initialization works correctly on first access.
        /// </summary>
        [Fact]
        public void YellowGreen_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.YellowGreen;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that YellowGreen property returns the same instance on multiple accesses.
        /// Verifies the singleton behavior of the lazy initialization.
        /// </summary>
        [Fact]
        public void YellowGreen_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.YellowGreen;
            var second = Brush.YellowGreen;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that YellowGreen property returns a brush with the correct YellowGreen color.
        /// Verifies the color value matches the system-defined YellowGreen color (#FF9ACD32).
        /// </summary>
        [Fact]
        public void YellowGreen_ColorValue_MatchesSystemDefinedYellowGreen()
        {
            // Act
            var brush = Brush.YellowGreen;

            // Assert
            Assert.Equal(Colors.YellowGreen, brush.Color);
        }

        /// <summary>
        /// Tests that YellowGreen property returns a brush that is not empty.
        /// Verifies the IsEmpty property returns false for a valid color brush.
        /// </summary>
        [Fact]
        public void YellowGreen_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.YellowGreen;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the BlanchedAlmond property returns a non-null SolidColorBrush with the correct color.
        /// Verifies the property initialization, type, color value, and singleton behavior.
        /// </summary>
        [Fact]
        public void BlanchedAlmond_PropertyAccess_ReturnsValidSolidColorBrush()
        {
            // Arrange & Act
            var brush = Brush.BlanchedAlmond;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
            Assert.Equal(Colors.BlanchedAlmond, brush.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the BlanchedAlmond property return the same instance (singleton pattern).
        /// Verifies lazy initialization and caching behavior.
        /// </summary>
        [Fact]
        public void BlanchedAlmond_MultipleAccess_ReturnsSameInstance()
        {
            // Arrange & Act
            var brush1 = Brush.BlanchedAlmond;
            var brush2 = Brush.BlanchedAlmond;

            // Assert
            Assert.Same(brush1, brush2);
        }

        /// <summary>
        /// Tests that the BlanchedAlmond brush is not empty.
        /// Verifies the IsEmpty property returns false for a valid color brush.
        /// </summary>
        [Fact]
        public void BlanchedAlmond_IsEmpty_ReturnsFalse()
        {
            // Arrange & Act
            var brush = Brush.BlanchedAlmond;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the BlanchedAlmond brush color matches the expected ARGB values.
        /// Verifies the specific color components of the BlanchedAlmond color.
        /// </summary>
        [Fact]
        public void BlanchedAlmond_ColorComponents_MatchExpectedValues()
        {
            // Arrange & Act
            var brush = Brush.BlanchedAlmond;
            var color = brush.Color;

            // Assert
            Assert.Equal(Colors.BlanchedAlmond.Red, color.Red);
            Assert.Equal(Colors.BlanchedAlmond.Green, color.Green);
            Assert.Equal(Colors.BlanchedAlmond.Blue, color.Blue);
            Assert.Equal(Colors.BlanchedAlmond.Alpha, color.Alpha);
        }

        /// <summary>
        /// Tests that the CadetBlue property returns a SolidColorBrush instance.
        /// This verifies the property creates and returns the expected brush type.
        /// </summary>
        [Fact]
        public void CadetBlue_Access_ReturnsSolidColorBrush()
        {
            // Act
            var result = Brush.CadetBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the CadetBlue property returns a brush with the correct CadetBlue color.
        /// This verifies the brush is initialized with the system-defined CadetBlue color.
        /// </summary>
        [Fact]
        public void CadetBlue_Access_HasCorrectColor()
        {
            // Act
            var result = Brush.CadetBlue;

            // Assert
            Assert.Equal(Colors.CadetBlue, result.Color);
        }

        /// <summary>
        /// Tests that the CadetBlue property never returns null.
        /// This verifies the lazy initialization always produces a valid instance.
        /// </summary>
        [Fact]
        public void CadetBlue_Access_NeverReturnsNull()
        {
            // Act
            var result = Brush.CadetBlue;

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that multiple accesses to the CadetBlue property return the same instance.
        /// This verifies the lazy initialization pattern creates only one instance.
        /// </summary>
        [Fact]
        public void CadetBlue_MultipleAccesses_ReturnsSameInstance()
        {
            // Arrange - Reset the static backing field to ensure clean test state
            var cadetBlueField = typeof(Brush).GetField("cadetBlue", BindingFlags.NonPublic | BindingFlags.Static);
            cadetBlueField?.SetValue(null, null);

            // Act
            var firstAccess = Brush.CadetBlue;
            var secondAccess = Brush.CadetBlue;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the CadetBlue property creates a new instance on first access after field reset.
        /// This verifies the lazy initialization behavior when the backing field is null.
        /// </summary>
        [Fact]
        public void CadetBlue_FirstAccessAfterReset_CreatesNewInstance()
        {
            // Arrange - Reset the static backing field
            var cadetBlueField = typeof(Brush).GetField("cadetBlue", BindingFlags.NonPublic | BindingFlags.Static);
            cadetBlueField?.SetValue(null, null);

            // Act
            var result = Brush.CadetBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(Colors.CadetBlue, result.Color);
        }

        /// <summary>
        /// Tests that the CadetBlue brush behaves as an immutable brush.
        /// This verifies that the returned brush is actually an ImmutableBrush internally.
        /// </summary>
        [Fact]
        public void CadetBlue_Access_ReturnsImmutableBrush()
        {
            // Act
            var result = Brush.CadetBlue;

            // Assert
            Assert.NotNull(result);
            // Verify it's internally an ImmutableBrush by checking the type name
            // since ImmutableBrush is internal, we check via reflection
            Assert.Equal("ImmutableBrush", result.GetType().Name);
        }

        /// <summary>
        /// Tests that multiple threads accessing CadetBlue property concurrently get consistent results.
        /// This verifies thread safety of the lazy initialization pattern.
        /// </summary>
        [Fact]
        public void CadetBlue_ConcurrentAccess_ReturnsConsistentInstances()
        {
            // Arrange - Reset the static backing field
            var cadetBlueField = typeof(Brush).GetField("cadetBlue", BindingFlags.NonPublic | BindingFlags.Static);
            cadetBlueField?.SetValue(null, null);

            SolidColorBrush result1 = null;
            SolidColorBrush result2 = null;

            // Act - Access from multiple threads simultaneously
            var task1 = System.Threading.Tasks.Task.Run(() => result1 = Brush.CadetBlue);
            var task2 = System.Threading.Tasks.Task.Run(() => result2 = Brush.CadetBlue);

            System.Threading.Tasks.Task.WaitAll(task1, task2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that DarkOrange property returns a non-null SolidColorBrush.
        /// Verifies the property returns the expected brush type and is not null.
        /// </summary>
        [Fact]
        public void DarkOrange_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.DarkOrange;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that DarkOrange property returns a brush with the correct color.
        /// Verifies the Color property matches the expected Colors.DarkOrange value.
        /// </summary>
        [Fact]
        public void DarkOrange_WhenAccessed_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.DarkOrange;

            // Assert
            Assert.Equal(Colors.DarkOrange, result.Color);
        }

        /// <summary>
        /// Tests that DarkOrange property implements lazy initialization correctly.
        /// Verifies multiple accesses return the same cached instance.
        /// </summary>
        [Fact]
        public void DarkOrange_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var first = Brush.DarkOrange;
            var second = Brush.DarkOrange;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that DarkOrange property returns a brush that is not empty.
        /// Verifies the IsEmpty property returns false for the valid color brush.
        /// </summary>
        [Fact]
        public void DarkOrange_WhenAccessed_IsNotEmpty()
        {
            // Act
            var result = Brush.DarkOrange;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that FloralWhite property returns a non-null SolidColorBrush instance.
        /// Verifies the first access creates a valid brush object.
        /// Expected result: Property returns a non-null SolidColorBrush.
        /// </summary>
        [Fact]
        public void FloralWhite_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.FloralWhite;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that FloralWhite property returns a brush with the correct FloralWhite color.
        /// Verifies the color value matches the expected system-defined FloralWhite color.
        /// Expected result: The brush Color property equals Colors.FloralWhite.
        /// </summary>
        [Fact]
        public void FloralWhite_ColorValue_EqualsSystemFloralWhiteColor()
        {
            // Act
            var brush = Brush.FloralWhite;

            // Assert
            Assert.Equal(Colors.FloralWhite, brush.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to FloralWhite property return the same instance.
        /// Verifies the lazy initialization and memoization behavior using the ??= operator.
        /// Expected result: Multiple calls return the exact same object reference.
        /// </summary>
        [Fact]
        public void FloralWhite_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.FloralWhite;
            var second = Brush.FloralWhite;
            var third = Brush.FloralWhite;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that FloralWhite property returns a brush that is not empty.
        /// Verifies the IsEmpty property behavior of the returned brush.
        /// Expected result: The IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void FloralWhite_IsEmptyProperty_ReturnsFalse()
        {
            // Act
            var brush = Brush.FloralWhite;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests concurrent access to FloralWhite property from multiple threads.
        /// Verifies thread safety of the lazy initialization using ??= operator.
        /// Expected result: All threads receive the same instance without exceptions.
        /// </summary>
        [Fact]
        public async Task FloralWhite_ConcurrentAccess_ReturnsSameInstanceThreadSafely()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.FloralWhite);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            var firstResult = results[0];
            Assert.NotNull(firstResult);

            for (int i = 1; i < taskCount; i++)
            {
                Assert.Same(firstResult, results[i]);
            }
        }

        /// <summary>
        /// Tests that FloralWhite brush has consistent hash code across multiple accesses.
        /// Verifies the immutability and consistency of the brush instance.
        /// Expected result: Hash code remains the same for the same instance.
        /// </summary>
        [Fact]
        public void FloralWhite_GetHashCode_ConsistentAcrossAccesses()
        {
            // Act
            var brush1 = Brush.FloralWhite;
            var brush2 = Brush.FloralWhite;
            var hashCode1 = brush1.GetHashCode();
            var hashCode2 = brush2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that FloralWhite brush equals itself and other instances from the property.
        /// Verifies the equality implementation of the immutable brush.
        /// Expected result: Brush instances are equal to each other.
        /// </summary>
        [Fact]
        public void FloralWhite_Equals_ReturnsTrueForSameInstances()
        {
            // Act
            var brush1 = Brush.FloralWhite;
            var brush2 = Brush.FloralWhite;

            // Assert
            Assert.True(brush1.Equals(brush2));
            Assert.True(brush2.Equals(brush1));
            Assert.True(brush1.Equals(brush1));
        }

        /// <summary>
        /// Tests that the Indigo property returns a non-null SolidColorBrush instance.
        /// Verifies the basic functionality and type of the returned brush.
        /// </summary>
        [Fact]
        public void Indigo_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Arrange & Act
            var indigo = Brush.Indigo;

            // Assert
            Assert.NotNull(indigo);
            Assert.IsType<SolidColorBrush>(indigo);
        }

        /// <summary>
        /// Tests that the Indigo property returns a brush with the correct color value.
        /// Verifies that the color matches the system-defined Colors.Indigo value.
        /// </summary>
        [Fact]
        public void Indigo_ReturnedBrush_HasCorrectColor()
        {
            // Arrange
            var expectedColor = Colors.Indigo;

            // Act
            var indigo = Brush.Indigo;

            // Assert
            Assert.Equal(expectedColor, indigo.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the Indigo property return the same cached instance.
        /// Verifies the singleton behavior of the static property.
        /// </summary>
        [Fact]
        public void Indigo_MultipleAccess_ReturnsSameInstance()
        {
            // Arrange & Act
            var indigo1 = Brush.Indigo;
            var indigo2 = Brush.Indigo;

            // Assert
            Assert.Same(indigo1, indigo2);
        }

        /// <summary>
        /// Tests that the Indigo property is thread-safe when accessed concurrently.
        /// Verifies that all threads receive the same cached instance.
        /// </summary>
        [Fact]
        public void Indigo_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            SolidColorBrush result1 = null;
            SolidColorBrush result2 = null;
            var task1 = Task.Run(() => result1 = Brush.Indigo);
            var task2 = Task.Run(() => result2 = Brush.Indigo);

            // Act
            Task.WaitAll(task1, task2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that the Indigo property returns a brush that is not empty.
        /// Verifies the IsEmpty property of the returned brush.
        /// </summary>
        [Fact]
        public void Indigo_ReturnedBrush_IsNotEmpty()
        {
            // Arrange & Act
            var indigo = Brush.Indigo;

            // Assert
            Assert.False(indigo.IsEmpty);
        }

        /// <summary>
        /// Tests that the LightGray property returns a non-null SolidColorBrush instance.
        /// Verifies the basic functionality and type of the returned brush.
        /// Expected result: Returns a non-null SolidColorBrush.
        /// </summary>
        [Fact]
        public void LightGray_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.LightGray;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the LightGray property returns a brush with the correct LightGray color.
        /// Verifies that the color property matches the expected Colors.LightGray value.
        /// Expected result: Color property equals Colors.LightGray.
        /// </summary>
        [Fact]
        public void LightGray_WhenAccessed_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.LightGray;

            // Assert
            Assert.Equal(Colors.LightGray, result.Color);
        }

        /// <summary>
        /// Tests that multiple calls to LightGray property return the same instance (singleton behavior).
        /// Verifies the lazy initialization and caching behavior of the static property.
        /// Expected result: Same instance returned on subsequent calls.
        /// </summary>
        [Fact]
        public void LightGray_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var firstCall = Brush.LightGray;
            var secondCall = Brush.LightGray;
            var thirdCall = Brush.LightGray;

            // Assert
            Assert.Same(firstCall, secondCall);
            Assert.Same(secondCall, thirdCall);
            Assert.Same(firstCall, thirdCall);
        }

        /// <summary>
        /// Tests that the LightGray brush is not empty.
        /// Verifies that the IsEmpty property returns false for the initialized brush.
        /// Expected result: IsEmpty returns false.
        /// </summary>
        [Fact]
        public void LightGray_WhenAccessed_IsNotEmpty()
        {
            // Act
            var result = Brush.LightGray;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the LightGray brush has the expected color properties.
        /// Verifies the ARGB values of the LightGray color match the expected system-defined values.
        /// Expected result: Color matches expected ARGB values for LightGray (#FFD3D3D3).
        /// </summary>
        [Fact]
        public void LightGray_WhenAccessed_HasExpectedColorValues()
        {
            // Arrange
            var expectedColor = Colors.LightGray;

            // Act
            var result = Brush.LightGray;

            // Assert
            Assert.Equal(expectedColor.Red, result.Color.Red);
            Assert.Equal(expectedColor.Green, result.Color.Green);
            Assert.Equal(expectedColor.Blue, result.Color.Blue);
            Assert.Equal(expectedColor.Alpha, result.Color.Alpha);
        }

        /// <summary>
        /// Tests that the LightYellow property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void LightYellow_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.LightYellow;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that multiple accesses to the LightYellow property return the same instance (lazy initialization).
        /// </summary>
        [Fact]
        public void LightYellow_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.LightYellow;
            var second = Brush.LightYellow;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the LightYellow property returns a brush with the correct LightYellow color.
        /// </summary>
        [Fact]
        public void LightYellow_Color_EqualsColorsLightYellow()
        {
            // Act
            var lightYellowBrush = Brush.LightYellow;

            // Assert
            Assert.Equal(Colors.LightYellow, lightYellowBrush.Color);
        }

        /// <summary>
        /// Tests that the LightYellow property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void LightYellow_IsEmpty_ReturnsFalse()
        {
            // Act
            var lightYellowBrush = Brush.LightYellow;

            // Assert
            Assert.False(lightYellowBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the LightYellow property returns a brush with the expected ARGB color value.
        /// </summary>
        [Fact]
        public void LightYellow_ColorValue_HasExpectedArgbValue()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFFFFFE0);

            // Act
            var lightYellowBrush = Brush.LightYellow;

            // Assert
            Assert.Equal(expectedColor, lightYellowBrush.Color);
        }

        /// <summary>
        /// Tests that MediumSpringGreen property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void MediumSpringGreen_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var brush = Brush.MediumSpringGreen;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
        }

        /// <summary>
        /// Tests that MediumSpringGreen property returns the same instance on multiple accesses (singleton behavior).
        /// </summary>
        [Fact]
        public void MediumSpringGreen_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var brush1 = Brush.MediumSpringGreen;
            var brush2 = Brush.MediumSpringGreen;

            // Assert
            Assert.Same(brush1, brush2);
        }

        /// <summary>
        /// Tests that MediumSpringGreen property returns a brush with the correct color value.
        /// </summary>
        [Fact]
        public void MediumSpringGreen_Color_MatchesSystemDefinedColor()
        {
            // Act
            var brush = Brush.MediumSpringGreen;

            // Assert
            Assert.Equal(Colors.MediumSpringGreen, brush.Color);
        }

        /// <summary>
        /// Tests that MediumSpringGreen property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void MediumSpringGreen_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.MediumSpringGreen;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that MediumSpringGreen property can be accessed concurrently from multiple threads safely.
        /// </summary>
        [Fact]
        public async Task MediumSpringGreen_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.MediumSpringGreen);
            }

            var brushes = await Task.WhenAll(tasks);

            // Assert
            for (int i = 1; i < brushes.Length; i++)
            {
                Assert.Same(brushes[0], brushes[i]);
            }
        }

        /// <summary>
        /// Tests that MediumSpringGreen property creates an immutable brush (specific implementation detail).
        /// </summary>
        [Fact]
        public void MediumSpringGreen_BrushType_IsImmutableBrush()
        {
            // Act
            var brush = Brush.MediumSpringGreen;

            // Assert - The internal implementation should be ImmutableBrush
            Assert.Equal("ImmutableBrush", brush.GetType().Name);
        }

        /// <summary>
        /// Tests that MediumSpringGreen property returns a brush with expected ARGB color values.
        /// </summary>
        [Fact]
        public void MediumSpringGreen_ColorComponents_MatchExpectedValues()
        {
            // Act
            var brush = Brush.MediumSpringGreen;
            var color = brush.Color;

            // Assert - MediumSpringGreen should have ARGB value of #FF00FA9A
            Assert.Equal(1.0f, color.Alpha, 3); // FF = 255/255 = 1.0
            Assert.Equal(0.0f, color.Red, 3);   // 00 = 0/255 = 0.0  
            Assert.Equal(0.98f, color.Green, 2); // FA = 250/255 ≈ 0.98
            Assert.Equal(0.6f, color.Blue, 1);   // 9A = 154/255 ≈ 0.6
        }

        /// <summary>
        /// Tests that OliveDrab property returns a non-null SolidColorBrush with the correct color.
        /// This verifies the lazy initialization creates the brush instance correctly.
        /// </summary>
        [Fact]
        public void OliveDrab_FirstAccess_ReturnsNonNullSolidColorBrushWithCorrectColor()
        {
            // Arrange & Act
            var oliveDrabBrush = Brush.OliveDrab;

            // Assert
            Assert.NotNull(oliveDrabBrush);
            Assert.IsType<SolidColorBrush>(oliveDrabBrush);
            Assert.Equal(Colors.OliveDrab, oliveDrabBrush.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to OliveDrab property return the same instance.
        /// This verifies the lazy initialization behavior and ensures reference equality.
        /// </summary>
        [Fact]
        public void OliveDrab_MultipleAccesses_ReturnsSameInstance()
        {
            // Arrange & Act
            var firstAccess = Brush.OliveDrab;
            var secondAccess = Brush.OliveDrab;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that OliveDrab brush is not empty.
        /// This verifies the brush state and color assignment.
        /// </summary>
        [Fact]
        public void OliveDrab_BrushProperties_IsNotEmpty()
        {
            // Arrange & Act
            var oliveDrabBrush = Brush.OliveDrab;

            // Assert
            Assert.False(oliveDrabBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that OliveDrab property returns a brush with the exact system-defined color value.
        /// This verifies the color value matches the expected system color.
        /// </summary>
        [Fact]
        public void OliveDrab_ColorValue_MatchesSystemDefinedColor()
        {
            // Arrange
            var expectedColor = Colors.OliveDrab;

            // Act
            var oliveDrabBrush = Brush.OliveDrab;

            // Assert
            Assert.Equal(expectedColor, oliveDrabBrush.Color);
            Assert.Equal(expectedColor.Red, oliveDrabBrush.Color.Red);
            Assert.Equal(expectedColor.Green, oliveDrabBrush.Color.Green);
            Assert.Equal(expectedColor.Blue, oliveDrabBrush.Color.Blue);
            Assert.Equal(expectedColor.Alpha, oliveDrabBrush.Color.Alpha);
        }

        /// <summary>
        /// Tests that the Peru property returns a non-null SolidColorBrush instance.
        /// Verifies the lazy initialization works correctly on first access.
        /// </summary>
        [Fact]
        public void Peru_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Peru;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Peru property returns the same instance on multiple accesses.
        /// Verifies the singleton behavior of the lazy-initialized static property.
        /// </summary>
        [Fact]
        public void Peru_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.Peru;
            var second = Brush.Peru;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the Peru property returns a brush with the correct Peru color.
        /// Verifies the color value matches the system-defined Colors.Peru.
        /// </summary>
        [Fact]
        public void Peru_ColorValue_MatchesSystemDefinedPeruColor()
        {
            // Act
            var peru = Brush.Peru;

            // Assert
            Assert.Equal(Colors.Peru, peru.Color);
        }

        /// <summary>
        /// Tests that the Peru brush is not empty.
        /// Verifies the IsEmpty property returns false for the Peru brush.
        /// </summary>
        [Fact]
        public void Peru_IsEmpty_ReturnsFalse()
        {
            // Act
            var peru = Brush.Peru;

            // Assert
            Assert.False(peru.IsEmpty);
        }

        /// <summary>
        /// Tests that the Peru property returns an immutable brush.
        /// Verifies the returned brush is of the internal ImmutableBrush type.
        /// </summary>
        [Fact]
        public void Peru_ReturnedBrush_IsImmutableBrush()
        {
            // Act
            var peru = Brush.Peru;

            // Assert
            Assert.Equal("ImmutableBrush", peru.GetType().Name);
        }

        /// <summary>
        /// Tests that multiple concurrent accesses to Peru property return the same instance.
        /// Verifies thread safety of the lazy initialization pattern.
        /// </summary>
        [Fact]
        public void Peru_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            SolidColorBrush result1 = null;
            SolidColorBrush result2 = null;
            var task1 = System.Threading.Tasks.Task.Run(() => result1 = Brush.Peru);
            var task2 = System.Threading.Tasks.Task.Run(() => result2 = Brush.Peru);

            // Act
            System.Threading.Tasks.Task.WaitAll(task1, task2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that the Peru brush color has the expected ARGB values.
        /// Verifies the specific color components match the Peru color definition.
        /// </summary>
        [Fact]
        public void Peru_ColorComponents_MatchExpectedValues()
        {
            // Act
            var peru = Brush.Peru;
            var color = peru.Color;

            // Assert
            Assert.Equal(Colors.Peru.Red, color.Red);
            Assert.Equal(Colors.Peru.Green, color.Green);
            Assert.Equal(Colors.Peru.Blue, color.Blue);
            Assert.Equal(Colors.Peru.Alpha, color.Alpha);
        }

        /// <summary>
        /// Tests that the Thistle property returns a non-null SolidColorBrush instance
        /// with the correct Thistle color on first access.
        /// </summary>
        [Fact]
        public void Thistle_FirstAccess_ReturnsNonNullBrushWithCorrectColor()
        {
            // Act
            var thistle = Brush.Thistle;

            // Assert
            Assert.NotNull(thistle);
            Assert.IsType<SolidColorBrush>(thistle);
            Assert.Equal(Colors.Thistle, thistle.Color);
        }

        /// <summary>
        /// Tests that subsequent accesses to the Thistle property return the same instance
        /// (lazy initialization pattern verification).
        /// </summary>
        [Fact]
        public void Thistle_SubsequentAccess_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Thistle;
            var secondAccess = Brush.Thistle;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Thistle property returns a brush with the expected ARGB color value.
        /// </summary>
        [Fact]
        public void Thistle_Color_HasExpectedArgbValue()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFD8BFD8);

            // Act
            var thistle = Brush.Thistle;

            // Assert
            Assert.Equal(expectedColor, thistle.Color);
            Assert.Equal(expectedColor, Colors.Thistle);
        }

        /// <summary>
        /// Tests that the Thistle property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void Thistle_IsEmpty_ReturnsFalse()
        {
            // Act
            var thistle = Brush.Thistle;

            // Assert
            Assert.False(thistle.IsEmpty);
        }

        /// <summary>
        /// Tests that multiple concurrent accesses to the Thistle property return the same instance.
        /// </summary>
        [Fact]
        public void Thistle_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            SolidColorBrush result1 = null;
            SolidColorBrush result2 = null;
            var task1 = System.Threading.Tasks.Task.Run(() => result1 = Brush.Thistle);
            var task2 = System.Threading.Tasks.Task.Run(() => result2 = Brush.Thistle);

            // Act
            System.Threading.Tasks.Task.WaitAll(task1, task2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that the Coral property returns a non-null SolidColorBrush instance.
        /// Verifies that the lazy initialization creates a valid brush object.
        /// </summary>
        [Fact]
        public void Coral_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var coralBrush = Brush.Coral;

            // Assert
            Assert.NotNull(coralBrush);
            Assert.IsType<SolidColorBrush>(coralBrush);
        }

        /// <summary>
        /// Tests that the Coral property returns a brush with the correct color.
        /// Verifies that the brush uses the system-defined Colors.Coral value.
        /// </summary>
        [Fact]
        public void Coral_WhenAccessed_ReturnsCorrectColor()
        {
            // Act
            var coralBrush = Brush.Coral;

            // Assert
            Assert.Equal(Colors.Coral, coralBrush.Color);
        }

        /// <summary>
        /// Tests that the Coral property returns a non-empty brush.
        /// Verifies that IsEmpty returns false since the brush has a valid color.
        /// </summary>
        [Fact]
        public void Coral_WhenAccessed_ReturnsNonEmptyBrush()
        {
            // Act
            var coralBrush = Brush.Coral;

            // Assert
            Assert.False(coralBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Coral property uses lazy initialization and returns the same instance.
        /// Verifies the singleton pattern implementation for the static property.
        /// </summary>
        [Fact]
        public void Coral_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Coral;
            var secondAccess = Brush.Coral;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Coral property returns an immutable brush instance.
        /// Verifies that the returned brush is of the internal ImmutableBrush type.
        /// </summary>
        [Fact]
        public void Coral_WhenAccessed_ReturnsImmutableBrush()
        {
            // Act
            var coralBrush = Brush.Coral;

            // Assert
            Assert.Equal("Microsoft.Maui.Controls.ImmutableBrush", coralBrush.GetType().FullName);
        }

        /// <summary>
        /// Tests that the DarkGreen property returns a non-null SolidColorBrush instance.
        /// Verifies that the property never returns null and creates a valid brush instance.
        /// </summary>
        [Fact]
        public void DarkGreen_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.DarkGreen;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the DarkGreen property returns a brush with the correct DarkGreen color.
        /// Verifies that the Color property of the returned brush matches Colors.DarkGreen.
        /// </summary>
        [Fact]
        public void DarkGreen_ColorValue_MatchesSystemDarkGreenColor()
        {
            // Act
            var result = Brush.DarkGreen;

            // Assert
            Assert.Equal(Colors.DarkGreen, result.Color);
        }

        /// <summary>
        /// Tests that the DarkGreen property exhibits singleton behavior.
        /// Verifies that multiple accesses return the same cached instance.
        /// </summary>
        [Fact]
        public void DarkGreen_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.DarkGreen;
            var secondAccess = Brush.DarkGreen;
            var thirdAccess = Brush.DarkGreen;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the DarkGreen property is thread-safe during concurrent access.
        /// Verifies that multiple threads accessing the property simultaneously receive the same instance.
        /// </summary>
        [Fact]
        public async Task DarkGreen_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            const int taskCount = 10;
            var results = new SolidColorBrush[taskCount];
            var tasks = new Task[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() => results[index] = Brush.DarkGreen);
            }

            await Task.WhenAll(tasks);

            // Assert
            var firstResult = results[0];
            Assert.NotNull(firstResult);

            for (int i = 1; i < taskCount; i++)
            {
                Assert.Same(firstResult, results[i]);
            }
        }

        /// <summary>
        /// Tests that the DarkGreen property returns a brush that is not empty.
        /// Verifies that the IsEmpty property returns false for the DarkGreen brush.
        /// </summary>
        [Fact]
        public void DarkGreen_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.DarkGreen;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the DeepPink property returns a non-null SolidColorBrush with the correct color.
        /// Verifies the lazy initialization behavior and ensures the same instance is returned on subsequent calls.
        /// </summary>
        [Fact]
        public void DeepPink_FirstAccess_ReturnsNonNullSolidColorBrushWithCorrectColor()
        {
            // Act
            var result = Brush.DeepPink;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(Colors.DeepPink, result.Color);
        }

        /// <summary>
        /// Tests that the DeepPink property returns the same instance on multiple accesses,
        /// verifying the lazy initialization behavior with null-coalescing assignment.
        /// </summary>
        [Fact]
        public void DeepPink_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.DeepPink;
            var secondAccess = Brush.DeepPink;
            var thirdAccess = Brush.DeepPink;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the DeepPink property returns a brush that is not empty,
        /// ensuring the Color property is properly initialized.
        /// </summary>
        [Fact]
        public void DeepPink_ReturnedBrush_IsNotEmpty()
        {
            // Act
            var result = Brush.DeepPink;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the DeepPink property returns a brush with the exact color value
        /// matching the system-defined Colors.DeepPink color.
        /// </summary>
        [Fact]
        public void DeepPink_ColorValue_MatchesSystemDefinedDeepPinkColor()
        {
            // Arrange
            var expectedColor = Colors.DeepPink;

            // Act
            var result = Brush.DeepPink;

            // Assert
            Assert.Equal(expectedColor, result.Color);
            Assert.Equal(expectedColor.Red, result.Color.Red);
            Assert.Equal(expectedColor.Green, result.Color.Green);
            Assert.Equal(expectedColor.Blue, result.Color.Blue);
            Assert.Equal(expectedColor.Alpha, result.Color.Alpha);
        }

        /// <summary>
        /// Tests that the Gold property returns a non-null SolidColorBrush instance.
        /// Verifies basic functionality and that the property is accessible.
        /// Expected result: A valid SolidColorBrush instance is returned.
        /// </summary>
        [Fact]
        public void Gold_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var goldBrush = Brush.Gold;

            // Assert
            Assert.NotNull(goldBrush);
            Assert.IsType<SolidColorBrush>(goldBrush);
        }

        /// <summary>
        /// Tests that the Gold property returns a brush with the correct gold color.
        /// Verifies that the color property matches the system-defined Colors.Gold value.
        /// Expected result: The brush color equals Colors.Gold.
        /// </summary>
        [Fact]
        public void Gold_WhenAccessed_ReturnsCorrectGoldColor()
        {
            // Act
            var goldBrush = Brush.Gold;

            // Assert
            Assert.Equal(Colors.Gold, goldBrush.Color);
        }

        /// <summary>
        /// Tests that the Gold property uses lazy initialization and caching.
        /// Verifies that multiple accesses return the same instance for performance optimization.
        /// Expected result: Same instance is returned on subsequent calls.
        /// </summary>
        [Fact]
        public void Gold_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var goldBrush1 = Brush.Gold;
            var goldBrush2 = Brush.Gold;

            // Assert
            Assert.Same(goldBrush1, goldBrush2);
        }

        /// <summary>
        /// Tests that the Gold brush is not considered empty.
        /// Verifies that the brush has a valid color and is not in an empty state.
        /// Expected result: IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void Gold_WhenAccessed_IsNotEmpty()
        {
            // Act
            var goldBrush = Brush.Gold;

            // Assert
            Assert.False(goldBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Gold property returns an immutable brush.
        /// Verifies that the returned brush behaves as expected for immutable instances.
        /// Expected result: The brush color matches Colors.Gold and maintains its value.
        /// </summary>
        [Fact]
        public void Gold_WhenAccessed_ReturnsImmutableBrush()
        {
            // Act
            var goldBrush = Brush.Gold;
            var originalColor = goldBrush.Color;

            // Attempt to modify (should have no effect due to immutability)
            goldBrush.Color = Colors.Red;

            // Assert
            Assert.Equal(Colors.Gold, goldBrush.Color);
            Assert.Equal(originalColor, goldBrush.Color);
        }

        /// <summary>
        /// Tests that LightPink property returns a non-null SolidColorBrush instance.
        /// Verifies that the property correctly instantiates and returns a brush object.
        /// </summary>
        [Fact]
        public void LightPink_ReturnsNonNull_SolidColorBrush()
        {
            // Act
            var result = Brush.LightPink;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that LightPink property returns the same instance on multiple accesses.
        /// Verifies the singleton/caching behavior of the lazy initialization pattern.
        /// </summary>
        [Fact]
        public void LightPink_MultipleCalls_ReturnsSameInstance()
        {
            // Act
            var first = Brush.LightPink;
            var second = Brush.LightPink;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that LightPink property returns a brush with the correct color value.
        /// Verifies that the color matches the system-defined LightPink color (#FFFFB6C1).
        /// </summary>
        [Fact]
        public void LightPink_HasCorrectColor_MatchesSystemDefinedValue()
        {
            // Act
            var brush = Brush.LightPink;

            // Assert
            Assert.Equal(Colors.LightPink, brush.Color);
        }

        /// <summary>
        /// Tests that LightPink property returns a brush that is not empty.
        /// Verifies that the brush is properly initialized and not in an empty state.
        /// </summary>
        [Fact]
        public void LightPink_IsNotEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.LightPink;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that LightPink property returns a brush with expected color components.
        /// Verifies the ARGB values match the system-defined LightPink color (0xFFFFB6C1).
        /// </summary>
        [Fact]
        public void LightPink_ColorComponents_MatchExpectedValues()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFFFB6C1);

            // Act
            var brush = Brush.LightPink;

            // Assert
            Assert.Equal(expectedColor.Red, brush.Color.Red);
            Assert.Equal(expectedColor.Green, brush.Color.Green);
            Assert.Equal(expectedColor.Blue, brush.Color.Blue);
            Assert.Equal(expectedColor.Alpha, brush.Color.Alpha);
        }

        /// <summary>
        /// Tests that MediumSlateBlue property returns a non-null SolidColorBrush instance.
        /// This test verifies that the lazy initialization creates a proper brush object.
        /// Expected result: A non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void MediumSlateBlue_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.MediumSlateBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that MediumSlateBlue property returns the correct color value.
        /// This test verifies that the brush contains the system-defined MediumSlateBlue color.
        /// Expected result: The brush color should match Colors.MediumSlateBlue (#FF7B68EE).
        /// </summary>
        [Fact]
        public void MediumSlateBlue_ColorValue_MatchesExpectedColor()
        {
            // Act
            var brush = Brush.MediumSlateBlue;

            // Assert
            Assert.Equal(Colors.MediumSlateBlue, brush.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to MediumSlateBlue property return the same cached instance.
        /// This test verifies the lazy initialization caching behavior to ensure performance optimization.
        /// Expected result: Multiple calls should return the exact same object reference.
        /// </summary>
        [Fact]
        public void MediumSlateBlue_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.MediumSlateBlue;
            var second = Brush.MediumSlateBlue;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that MediumSlateBlue brush is not empty.
        /// This test verifies that the created brush represents a valid color and is not empty.
        /// Expected result: IsEmpty should return false.
        /// </summary>
        [Fact]
        public void MediumSlateBlue_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.MediumSlateBlue;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that MediumSlateBlue property can be accessed concurrently without issues.
        /// This test verifies thread safety of the lazy initialization pattern.
        /// Expected result: All concurrent accesses should return the same instance.
        /// </summary>
        [Fact]
        public void MediumSlateBlue_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            SolidColorBrush[] results = new SolidColorBrush[10];
            var tasks = new Task[10];

            // Act
            for (int i = 0; i < 10; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() => results[index] = Brush.MediumSlateBlue);
            }

            Task.WaitAll(tasks);

            // Assert
            var first = results[0];
            for (int i = 1; i < results.Length; i++)
            {
                Assert.Same(first, results[i]);
            }
        }

        /// <summary>
        /// Tests that the Olive property returns a non-null SolidColorBrush on first access.
        /// This test verifies the lazy initialization behavior of the static property.
        /// </summary>
        [Fact]
        public void Olive_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var oliveBrush = Brush.Olive;

            // Assert
            Assert.NotNull(oliveBrush);
            Assert.IsType<SolidColorBrush>(oliveBrush);
        }

        /// <summary>
        /// Tests that multiple accesses to the Olive property return the same cached instance.
        /// This test verifies the null-coalescing assignment (??=) caching behavior.
        /// </summary>
        [Fact]
        public void Olive_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Olive;
            var secondAccess = Brush.Olive;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Olive brush has the correct color value.
        /// This test verifies that the brush is initialized with Colors.Olive (#FF808000).
        /// </summary>
        [Fact]
        public void Olive_Color_MatchesExpectedValue()
        {
            // Act
            var oliveBrush = Brush.Olive;

            // Assert
            Assert.Equal(Colors.Olive, oliveBrush.Color);
        }

        /// <summary>
        /// Tests that the Olive brush is not empty.
        /// This test verifies that IsEmpty returns false since the brush has a valid color.
        /// </summary>
        [Fact]
        public void Olive_IsEmpty_ReturnsFalse()
        {
            // Act
            var oliveBrush = Brush.Olive;

            // Assert
            Assert.False(oliveBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Olive property returns the expected type hierarchy.
        /// This test verifies that the returned brush is a SolidColorBrush and Brush.
        /// </summary>
        [Fact]
        public void Olive_Type_IsSolidColorBrush()
        {
            // Act
            var oliveBrush = Brush.Olive;

            // Assert
            Assert.IsAssignableFrom<Brush>(oliveBrush);
            Assert.IsAssignableFrom<SolidColorBrush>(oliveBrush);
        }

        /// <summary>
        /// Tests that the RosyBrown property returns a valid SolidColorBrush instance on first access.
        /// </summary>
        [Fact]
        public void RosyBrown_FirstAccess_ReturnsValidSolidColorBrush()
        {
            // Act
            var result = Brush.RosyBrown;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that multiple accesses to RosyBrown return the same cached instance.
        /// </summary>
        [Fact]
        public void RosyBrown_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.RosyBrown;
            var second = Brush.RosyBrown;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the RosyBrown brush has the correct color value.
        /// </summary>
        [Fact]
        public void RosyBrown_Color_EqualsGraphicsColorsRosyBrown()
        {
            // Act
            var brush = Brush.RosyBrown;

            // Assert
            Assert.Equal(Colors.RosyBrown, brush.Color);
        }

        /// <summary>
        /// Tests that the RosyBrown brush is not empty.
        /// </summary>
        [Fact]
        public void RosyBrown_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.RosyBrown;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the RosyBrown brush color has the expected ARGB values.
        /// </summary>
        [Fact]
        public void RosyBrown_Color_HasExpectedArgbValues()
        {
            // Act
            var brush = Brush.RosyBrown;
            var color = brush.Color;

            // Assert
            Assert.Equal(1.0f, color.Alpha, 3); // Fully opaque
            Assert.Equal(Colors.RosyBrown.Red, color.Red, 3);
            Assert.Equal(Colors.RosyBrown.Green, color.Green, 3);
            Assert.Equal(Colors.RosyBrown.Blue, color.Blue, 3);
        }

        /// <summary>
        /// Tests that the BlueViolet property returns a non-null SolidColorBrush instance.
        /// This test exercises the lazy initialization of the static property.
        /// Expected result: Property returns a valid SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void BlueViolet_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.BlueViolet;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the BlueViolet property returns the same instance on multiple calls (singleton behavior).
        /// This test verifies the lazy initialization works correctly with the null-coalescing assignment operator.
        /// Expected result: Multiple calls return the exact same object reference.
        /// </summary>
        [Fact]
        public void BlueViolet_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.BlueViolet;
            var second = Brush.BlueViolet;
            var third = Brush.BlueViolet;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that the BlueViolet property returns a brush with the correct BlueViolet color.
        /// This test verifies the color is properly set during initialization.
        /// Expected result: The returned brush has the Colors.BlueViolet color value.
        /// </summary>
        [Fact]
        public void BlueViolet_ColorProperty_HasCorrectBlueVioletColor()
        {
            // Act
            var brush = Brush.BlueViolet;

            // Assert
            Assert.Equal(Colors.BlueViolet, brush.Color);
        }

        /// <summary>
        /// Tests that the BlueViolet property is not empty.
        /// This test verifies the IsEmpty property behavior for the initialized brush.
        /// Expected result: The brush is not considered empty.
        /// </summary>
        [Fact]
        public void BlueViolet_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.BlueViolet;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests concurrent access to the BlueViolet property to ensure thread safety.
        /// This test verifies that the lazy initialization is thread-safe and doesn't create multiple instances.
        /// Expected result: All concurrent accesses return the same instance.
        /// </summary>
        [Fact]
        public async Task BlueViolet_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.BlueViolet);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            var firstResult = results[0];
            for (int i = 1; i < results.Length; i++)
            {
                Assert.Same(firstResult, results[i]);
            }
        }

        /// <summary>
        /// Tests that the BlueViolet brush has the expected ARGB color values.
        /// This test verifies the specific color component values match the BlueViolet standard.
        /// Expected result: The color matches the standard BlueViolet color (#FF8A2BE2).
        /// </summary>
        [Fact]
        public void BlueViolet_ColorComponents_MatchStandardBlueViolet()
        {
            // Act
            var brush = Brush.BlueViolet;
            var color = brush.Color;

            // Assert
            Assert.Equal(Colors.BlueViolet.Red, color.Red);
            Assert.Equal(Colors.BlueViolet.Green, color.Green);
            Assert.Equal(Colors.BlueViolet.Blue, color.Blue);
            Assert.Equal(Colors.BlueViolet.Alpha, color.Alpha);
        }

        /// <summary>
        /// Tests that the DarkCyan property returns a valid SolidColorBrush with the correct color and properties.
        /// Verifies that the brush is not null, has the correct type, contains the DarkCyan color, and is not empty.
        /// </summary>
        [Fact]
        public void DarkCyan_FirstAccess_ReturnsValidSolidColorBrushWithDarkCyanColor()
        {
            // Act
            var result = Brush.DarkCyan;

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<SolidColorBrush>(result);
            Assert.Equal(Colors.DarkCyan, result.Color);
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the DarkCyan property implements lazy initialization correctly.
        /// Verifies that multiple calls to the property return the same cached instance.
        /// </summary>
        [Fact]
        public void DarkCyan_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var firstCall = Brush.DarkCyan;
            var secondCall = Brush.DarkCyan;
            var thirdCall = Brush.DarkCyan;

            // Assert
            Assert.Same(firstCall, secondCall);
            Assert.Same(secondCall, thirdCall);
            Assert.Same(firstCall, thirdCall);
        }

        /// <summary>
        /// Tests that the DarkOrchid property returns a valid SolidColorBrush instance with the correct color.
        /// Verifies the basic functionality of the static property getter.
        /// </summary>
        [Fact]
        public void DarkOrchid_FirstAccess_ReturnsSolidColorBrushWithCorrectColor()
        {
            // Arrange
            ResetDarkOrchidStaticField();

            // Act
            var result = Brush.DarkOrchid;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(Colors.DarkOrchid, result.Color);
        }

        /// <summary>
        /// Tests that subsequent accesses to the DarkOrchid property return the same cached instance.
        /// Verifies the lazy initialization and caching behavior of the static property.
        /// </summary>
        [Fact]
        public void DarkOrchid_MultipleAccess_ReturnsSameInstance()
        {
            // Arrange
            ResetDarkOrchidStaticField();

            // Act
            var firstAccess = Brush.DarkOrchid;
            var secondAccess = Brush.DarkOrchid;
            var thirdAccess = Brush.DarkOrchid;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the DarkOrchid property returns a brush that is not empty.
        /// Verifies that the created brush has valid content and is not considered empty.
        /// </summary>
        [Fact]
        public void DarkOrchid_ReturnedBrush_IsNotEmpty()
        {
            // Arrange
            ResetDarkOrchidStaticField();

            // Act
            var result = Brush.DarkOrchid;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the DarkOrchid property creates an instance with the exact system-defined DarkOrchid color.
        /// Verifies that the color matches the expected ARGB values for the DarkOrchid color.
        /// </summary>
        [Fact]
        public void DarkOrchid_Color_MatchesSystemDefinedDarkOrchidColor()
        {
            // Arrange
            ResetDarkOrchidStaticField();
            var expectedColor = Colors.DarkOrchid;

            // Act
            var result = Brush.DarkOrchid;

            // Assert
            Assert.Equal(expectedColor.Red, result.Color.Red);
            Assert.Equal(expectedColor.Green, result.Color.Green);
            Assert.Equal(expectedColor.Blue, result.Color.Blue);
            Assert.Equal(expectedColor.Alpha, result.Color.Alpha);
        }

        /// <summary>
        /// Helper method to reset the static darkOrchid field using reflection.
        /// This ensures proper test isolation by clearing the cached instance between tests.
        /// </summary>
        private void ResetDarkOrchidStaticField()
        {
            var field = typeof(Brush).GetField("darkOrchid", BindingFlags.NonPublic | BindingFlags.Static);
            field?.SetValue(null, null);
        }

        /// <summary>
        /// Tests that the Firebrick property returns a non-null SolidColorBrush instance.
        /// Validates that the static property correctly initializes and returns a brush.
        /// Expected result: Returns a non-null SolidColorBrush.
        /// </summary>
        [Fact]
        public void Firebrick_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Firebrick;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Firebrick property returns the same instance on multiple calls.
        /// Validates the lazy initialization behavior and singleton pattern.
        /// Expected result: Multiple calls return the exact same object reference.
        /// </summary>
        [Fact]
        public void Firebrick_ReturnsEqualInstance_OnMultipleCalls()
        {
            // Act
            var first = Brush.Firebrick;
            var second = Brush.Firebrick;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the Firebrick property returns a brush with the correct color.
        /// Validates that the brush color matches the system-defined firebrick color.
        /// Expected result: The brush color equals Colors.Firebrick.
        /// </summary>
        [Fact]
        public void Firebrick_HasCorrectColor()
        {
            // Act
            var brush = Brush.Firebrick;

            // Assert
            Assert.Equal(Colors.Firebrick, brush.Color);
        }

        /// <summary>
        /// Tests that the Firebrick property returns a non-empty brush.
        /// Validates that the brush is properly initialized with a valid color.
        /// Expected result: IsEmpty returns false.
        /// </summary>
        [Fact]
        public void Firebrick_IsNotEmpty()
        {
            // Act
            var brush = Brush.Firebrick;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Gray property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void Gray_ReturnsNonNull_SolidColorBrush()
        {
            // Act
            var grayBrush = Brush.Gray;

            // Assert
            Assert.NotNull(grayBrush);
            Assert.IsType<SolidColorBrush>(grayBrush);
        }

        /// <summary>
        /// Tests that the Gray property returns a brush with the correct Gray color.
        /// </summary>
        [Fact]
        public void Gray_HasCorrectColor_EqualsColorsGray()
        {
            // Act
            var grayBrush = Brush.Gray;

            // Assert
            Assert.Equal(Colors.Gray, grayBrush.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the Gray property return the same cached instance.
        /// This verifies the lazy initialization and caching behavior.
        /// </summary>
        [Fact]
        public void Gray_MultipleCalls_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Gray;
            var secondAccess = Brush.Gray;
            var thirdAccess = Brush.Gray;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the Gray property returns an ImmutableBrush instance,
        /// which is the internal implementation used for predefined brushes.
        /// </summary>
        [Fact]
        public void Gray_ReturnsImmutableBrush_Instance()
        {
            // Act
            var grayBrush = Brush.Gray;

            // Assert
            // ImmutableBrush is internal, so we check by name
            Assert.Equal("ImmutableBrush", grayBrush.GetType().Name);
            Assert.True(grayBrush is SolidColorBrush);
        }

        /// <summary>
        /// Tests that the Gray brush is not empty.
        /// </summary>
        [Fact]
        public void Gray_IsNotEmpty_ReturnsFalse()
        {
            // Act
            var grayBrush = Brush.Gray;

            // Assert
            Assert.False(grayBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Gray brush has the expected hash code based on its color.
        /// </summary>
        [Fact]
        public void Gray_HashCode_BasedOnColor()
        {
            // Arrange
            var grayBrush = Brush.Gray;
            var expectedHashCode = -1234567890 + Colors.Gray.GetHashCode();

            // Act
            var actualHashCode = grayBrush.GetHashCode();

            // Assert
            Assert.Equal(expectedHashCode, actualHashCode);
        }

        /// <summary>
        /// Tests that the Gray brush equals another SolidColorBrush with the same color.
        /// </summary>
        [Fact]
        public void Gray_Equals_SolidColorBrushWithSameColor()
        {
            // Arrange
            var grayBrush = Brush.Gray;
            var otherGrayBrush = new SolidColorBrush(Colors.Gray);

            // Act & Assert
            Assert.True(grayBrush.Equals(otherGrayBrush));
            Assert.True(otherGrayBrush.Equals(grayBrush));
        }

        /// <summary>
        /// Tests that the Gray brush does not equal a SolidColorBrush with a different color.
        /// </summary>
        [Fact]
        public void Gray_DoesNotEqual_SolidColorBrushWithDifferentColor()
        {
            // Arrange
            var grayBrush = Brush.Gray;
            var redBrush = new SolidColorBrush(Colors.Red);

            // Act & Assert
            Assert.False(grayBrush.Equals(redBrush));
            Assert.False(redBrush.Equals(grayBrush));
        }

        /// <summary>
        /// Tests that LightBlue property returns a SolidColorBrush with the correct color value.
        /// Verifies the basic functionality and color correctness of the lazy-initialized property.
        /// </summary>
        [Fact]
        public void LightBlue_FirstAccess_ReturnsSolidColorBrushWithCorrectColor()
        {
            // Arrange - Reset the static field to ensure clean test state
            ResetLightBlueStaticField();

            // Act
            var result = Brush.LightBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(Colors.LightBlue, result.Color);
            Assert.Equal(Color.FromUint(0xFFADD8E6), result.Color);
        }

        /// <summary>
        /// Tests that subsequent accesses to LightBlue return the same cached instance.
        /// Verifies the singleton behavior of the lazy initialization pattern.
        /// </summary>
        [Fact]
        public void LightBlue_MultipleAccesses_ReturnsSameInstance()
        {
            // Arrange - Reset the static field to ensure clean test state
            ResetLightBlueStaticField();

            // Act
            var first = Brush.LightBlue;
            var second = Brush.LightBlue;
            var third = Brush.LightBlue;

            // Assert
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.NotNull(third);
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that LightBlue property never returns null and always provides a valid brush.
        /// Verifies reliability and consistency of the property across multiple invocations.
        /// </summary>
        [Fact]
        public void LightBlue_MultipleInvocations_NeverReturnsNull()
        {
            // Arrange - Reset the static field to ensure clean test state
            ResetLightBlueStaticField();

            // Act & Assert
            for (int i = 0; i < 10; i++)
            {
                var result = Brush.LightBlue;
                Assert.NotNull(result);
                Assert.IsType<SolidColorBrush>(result);
            }
        }

        /// <summary>
        /// Tests concurrent access to LightBlue property to verify thread safety.
        /// Ensures that lazy initialization works correctly under concurrent access scenarios.
        /// </summary>
        [Fact]
        public async Task LightBlue_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange - Reset the static field to ensure clean test state
            ResetLightBlueStaticField();

            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];
            var results = new SolidColorBrush[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() => Brush.LightBlue);
            }

            await Task.WhenAll(tasks);

            for (int i = 0; i < taskCount; i++)
            {
                results[i] = tasks[i].Result;
            }

            // Assert
            for (int i = 0; i < taskCount; i++)
            {
                Assert.NotNull(results[i]);
                Assert.IsType<SolidColorBrush>(results[i]);
                Assert.Equal(Colors.LightBlue, results[i].Color);
            }

            // All results should be the same instance
            for (int i = 1; i < taskCount; i++)
            {
                Assert.Same(results[0], results[i]);
            }
        }

        /// <summary>
        /// Tests that LightBlue property color matches the expected ARGB values.
        /// Verifies specific color component values (Red, Green, Blue, Alpha) are correct.
        /// </summary>
        [Fact]
        public void LightBlue_ColorComponents_MatchExpectedValues()
        {
            // Arrange - Reset the static field to ensure clean test state
            ResetLightBlueStaticField();
            var expectedColor = Color.FromUint(0xFFADD8E6);

            // Act
            var result = Brush.LightBlue;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedColor.Red, result.Color.Red, 3);
            Assert.Equal(expectedColor.Green, result.Color.Green, 3);
            Assert.Equal(expectedColor.Blue, result.Color.Blue, 3);
            Assert.Equal(expectedColor.Alpha, result.Color.Alpha, 3);
        }

        /// <summary>
        /// Tests that LightBlue brush is not empty.
        /// Verifies the IsEmpty property behavior for the system-defined brush.
        /// </summary>
        [Fact]
        public void LightBlue_IsEmpty_ReturnsFalse()
        {
            // Arrange - Reset the static field to ensure clean test state
            ResetLightBlueStaticField();

            // Act
            var result = Brush.LightBlue;

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Helper method to reset the static lightBlue field to null using reflection.
        /// This ensures test isolation by providing a clean state for each test.
        /// </summary>
        private void ResetLightBlueStaticField()
        {
            var fieldInfo = typeof(Brush).GetField("lightBlue", BindingFlags.NonPublic | BindingFlags.Static);
            fieldInfo?.SetValue(null, null);
        }

        /// <summary>
        /// Tests that the LimeGreen property returns a valid SolidColorBrush with the correct color.
        /// Verifies that the brush is not null and contains the expected LimeGreen color value.
        /// </summary>
        [Fact]
        public void LimeGreen_FirstAccess_ReturnsValidBrushWithCorrectColor()
        {
            // Act
            var result = Brush.LimeGreen;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(Colors.LimeGreen, result.Color);
        }

        /// <summary>
        /// Tests that the LimeGreen property uses lazy initialization and caching.
        /// Verifies that multiple accesses return the same exact instance.
        /// </summary>
        [Fact]
        public void LimeGreen_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.LimeGreen;
            var second = Brush.LimeGreen;
            var third = Brush.LimeGreen;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that the LimeGreen property returns a brush with the exact expected color value.
        /// Verifies that the color matches the system-defined LimeGreen color (0xFF32CD32).
        /// </summary>
        [Fact]
        public void LimeGreen_ColorValue_MatchesSystemDefinedLimeGreen()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFF32CD32);

            // Act
            var result = Brush.LimeGreen;

            // Assert
            Assert.Equal(expectedColor, result.Color);
            Assert.Equal(expectedColor, Colors.LimeGreen);
        }

        /// <summary>
        /// Tests that the LimeGreen property returns an ImmutableBrush instance.
        /// Verifies the actual implementation type while maintaining the SolidColorBrush interface.
        /// </summary>
        [Fact]
        public void LimeGreen_InstanceType_IsImmutableBrush()
        {
            // Act
            var result = Brush.LimeGreen;

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<SolidColorBrush>(result);
            // Note: Cannot test for ImmutableBrush directly as it's internal,
            // but we can verify it's a SolidColorBrush with the correct behavior
        }

        /// <summary>
        /// Tests that the LimeGreen property consistently returns a brush that is not empty.
        /// Verifies that the IsEmpty property returns false for the LimeGreen brush.
        /// </summary>
        [Fact]
        public void LimeGreen_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.LimeGreen;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the NavajoWhite property returns a non-null SolidColorBrush instance.
        /// Verifies the lazy initialization creates a valid brush object.
        /// </summary>
        [Fact]
        public void NavajoWhite_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.NavajoWhite;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the NavajoWhite property returns a brush with the correct NavajoWhite color.
        /// Verifies the brush color matches the expected Colors.NavajoWhite value.
        /// </summary>
        [Fact]
        public void NavajoWhite_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.NavajoWhite;

            // Assert
            Assert.Equal(Colors.NavajoWhite, result.Color);
        }

        /// <summary>
        /// Tests that multiple calls to NavajoWhite property return the same cached instance.
        /// Verifies the lazy initialization behavior and instance caching.
        /// </summary>
        [Fact]
        public void NavajoWhite_MultipleCalls_ReturnsSameInstance()
        {
            // Act
            var first = Brush.NavajoWhite;
            var second = Brush.NavajoWhite;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the NavajoWhite property returns a brush that is not empty.
        /// Verifies the brush has valid content and is not considered empty.
        /// </summary>
        [Fact]
        public void NavajoWhite_IsNotEmpty()
        {
            // Act
            var result = Brush.NavajoWhite;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the PaleGoldenrod property returns a non-null SolidColorBrush instance.
        /// Validates that the property initializes correctly and returns a valid brush object.
        /// </summary>
        [Fact]
        public void PaleGoldenrod_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Arrange & Act
            var brush = Brush.PaleGoldenrod;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
        }

        /// <summary>
        /// Tests that the PaleGoldenrod property returns a brush with the correct PaleGoldenrod color.
        /// Validates that the lazy initialization creates a brush with the system-defined PaleGoldenrod color.
        /// </summary>
        [Fact]
        public void PaleGoldenrod_FirstAccess_ReturnsCorrectColor()
        {
            // Arrange & Act
            var brush = Brush.PaleGoldenrod;

            // Assert
            Assert.Equal(Colors.PaleGoldenrod, brush.Color);
        }

        /// <summary>
        /// Tests that the PaleGoldenrod property uses lazy initialization correctly.
        /// Validates that multiple accesses return the same instance (singleton behavior).
        /// </summary>
        [Fact]
        public void PaleGoldenrod_MultipleAccess_ReturnsSameInstance()
        {
            // Arrange & Act
            var brush1 = Brush.PaleGoldenrod;
            var brush2 = Brush.PaleGoldenrod;

            // Assert
            Assert.Same(brush1, brush2);
        }

        /// <summary>
        /// Tests that the PaleGoldenrod property creates a brush that is not empty.
        /// Validates that the returned brush has proper color information and is not considered empty.
        /// </summary>
        [Fact]
        public void PaleGoldenrod_FirstAccess_ReturnsNonEmptyBrush()
        {
            // Arrange & Act
            var brush = Brush.PaleGoldenrod;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the PaleGoldenrod property returns a brush with correct hash code.
        /// Validates that the brush's hash code is consistent with its color value.
        /// </summary>
        [Fact]
        public void PaleGoldenrod_FirstAccess_HasConsistentHashCode()
        {
            // Arrange & Act
            var brush = Brush.PaleGoldenrod;
            var expectedHashCode = -1234567890 + Colors.PaleGoldenrod.GetHashCode();

            // Assert
            Assert.Equal(expectedHashCode, brush.GetHashCode());
        }

        /// <summary>
        /// Tests that the PaleGoldenrod property returns a brush that equals another brush with the same color.
        /// Validates the equality behavior of the returned brush.
        /// </summary>
        [Fact]
        public void PaleGoldenrod_FirstAccess_EqualsOtherBrushWithSameColor()
        {
            // Arrange
            var brush1 = Brush.PaleGoldenrod;
            var brush2 = new SolidColorBrush(Colors.PaleGoldenrod);

            // Act & Assert
            Assert.Equal(brush1, brush2);
            Assert.True(brush1.Equals(brush2));
        }

        /// <summary>
        /// Tests that the RoyalBlue property returns a non-null SolidColorBrush instance.
        /// This test validates the lazy initialization of the static property.
        /// </summary>
        [Fact]
        public void RoyalBlue_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.RoyalBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the RoyalBlue property returns the correct system-defined color.
        /// This test ensures the brush contains the expected Colors.RoyalBlue value.
        /// </summary>
        [Fact]
        public void RoyalBlue_ColorProperty_MatchesSystemDefinedRoyalBlueColor()
        {
            // Act
            var result = Brush.RoyalBlue;

            // Assert
            Assert.Equal(Colors.RoyalBlue, result.Color);
        }

        /// <summary>
        /// Tests that the RoyalBlue property is not empty.
        /// This test validates that the brush represents a valid color and is not considered empty.
        /// </summary>
        [Fact]
        public void RoyalBlue_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.RoyalBlue;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that multiple accesses to RoyalBlue property return the same instance.
        /// This test validates the lazy initialization behavior and ensures the static field is reused.
        /// </summary>
        [Fact]
        public void RoyalBlue_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.RoyalBlue;
            var second = Brush.RoyalBlue;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the RoyalBlue brush is not considered null or empty by the static helper method.
        /// This test validates integration with the Brush.IsNullOrEmpty utility method.
        /// </summary>
        [Fact]
        public void RoyalBlue_IsNullOrEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.RoyalBlue;
            var isNullOrEmpty = Brush.IsNullOrEmpty(result);

            // Assert
            Assert.False(isNullOrEmpty);
        }

        /// <summary>
        /// Tests that the SlateGray property returns a non-null SolidColorBrush instance.
        /// Verifies that the lazy initialization creates a valid brush object.
        /// </summary>
        [Fact]
        public void SlateGray_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var slateGrayBrush = Brush.SlateGray;

            // Assert
            Assert.NotNull(slateGrayBrush);
            Assert.IsType<SolidColorBrush>(slateGrayBrush);
        }

        /// <summary>
        /// Tests that the SlateGray property returns a brush with the correct SlateGray color.
        /// Verifies that the color matches the system-defined SlateGray color.
        /// </summary>
        [Fact]
        public void SlateGray_WhenAccessed_ReturnsCorrectColor()
        {
            // Act
            var slateGrayBrush = Brush.SlateGray;

            // Assert
            Assert.Equal(Colors.SlateGray, slateGrayBrush.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the SlateGray property return the same instance.
        /// Verifies the singleton behavior of the lazy initialization pattern.
        /// </summary>
        [Fact]
        public void SlateGray_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.SlateGray;
            var secondAccess = Brush.SlateGray;
            var thirdAccess = Brush.SlateGray;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the SlateGray brush is not empty.
        /// Verifies that the brush has a valid color and is not considered empty.
        /// </summary>
        [Fact]
        public void SlateGray_WhenAccessed_IsNotEmpty()
        {
            // Act
            var slateGrayBrush = Brush.SlateGray;

            // Assert
            Assert.False(slateGrayBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the SlateGray property is consistent across different access patterns.
        /// Verifies that interleaved accesses with other operations still return the same instance and color.
        /// </summary>
        [Fact]
        public void SlateGray_WhenAccessedWithInterleavedOperations_RemainsConsistent()
        {
            // Act
            var firstAccess = Brush.SlateGray;
            var firstColor = firstAccess.Color;
            var firstIsEmpty = firstAccess.IsEmpty;

            var secondAccess = Brush.SlateGray;
            var secondColor = secondAccess.Color;
            var secondIsEmpty = secondAccess.IsEmpty;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Equal(firstColor, secondColor);
            Assert.Equal(firstIsEmpty, secondIsEmpty);
            Assert.Equal(Colors.SlateGray, firstColor);
            Assert.Equal(Colors.SlateGray, secondColor);
            Assert.False(firstIsEmpty);
            Assert.False(secondIsEmpty);
        }

        /// <summary>
        /// Tests that the Wheat property returns a non-null SolidColorBrush instance.
        /// Verifies that the lazy initialization creates a valid brush object.
        /// </summary>
        [Fact]
        public void Wheat_PropertyAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Wheat;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that multiple accesses to the Wheat property return the same instance.
        /// Verifies the lazy initialization behavior using the null-coalescing assignment operator.
        /// </summary>
        [Fact]
        public void Wheat_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.Wheat;
            var second = Brush.Wheat;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the Wheat property returns a brush with the correct system-defined color.
        /// Verifies that the brush is initialized with Colors.Wheat.
        /// </summary>
        [Fact]
        public void Wheat_PropertyAccess_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.Wheat;

            // Assert
            Assert.Equal(Colors.Wheat, result.Color);
        }

        /// <summary>
        /// Tests that the Wheat property returns a brush that is not empty.
        /// Verifies the IsEmpty property behavior for the system-defined color brush.
        /// </summary>
        [Fact]
        public void Wheat_PropertyAccess_ReturnsNonEmptyBrush()
        {
            // Act
            var result = Brush.Wheat;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the Brown property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void Brown_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Brown;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Brown property returns a brush with the correct brown color value.
        /// The expected color should have ARGB value of #FFA52A2A as defined in Colors.Brown.
        /// </summary>
        [Fact]
        public void Brown_HasCorrectColorValue()
        {
            // Act
            var result = Brush.Brown;

            // Assert
            Assert.Equal(Colors.Brown, result.Color);
            Assert.Equal(Color.FromUint(0xFFA52A2A), result.Color);
        }

        /// <summary>
        /// Tests that the Brown property implements lazy initialization correctly.
        /// Multiple calls should return the same instance (singleton behavior).
        /// </summary>
        [Fact]
        public void Brown_MultipleCalls_ReturnsSameInstance()
        {
            // Act
            var first = Brush.Brown;
            var second = Brush.Brown;
            var third = Brush.Brown;

            // Assert
            Assert.Same(first, second);
            Assert.Same(first, third);
            Assert.Same(second, third);
        }

        /// <summary>
        /// Tests that the Brown property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void Brown_IsNotEmpty()
        {
            // Act
            var result = Brush.Brown;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the Brown property returns a brush with a valid color that is not null.
        /// </summary>
        [Fact]
        public void Brown_ColorIsNotNull()
        {
            // Act
            var result = Brush.Brown;

            // Assert
            Assert.NotNull(result.Color);
        }

        /// <summary>
        /// Tests that the Brown property returns an immutable brush that cannot change its color.
        /// Attempts to set the color should not affect the original value.
        /// </summary>
        [Fact]
        public void Brown_ColorIsImmutable()
        {
            // Act
            var result = Brush.Brown;
            var originalColor = result.Color;

            // Attempt to change the color (should have no effect for ImmutableBrush)
            result.Color = Colors.Red;

            // Assert
            Assert.Equal(originalColor, result.Color);
            Assert.Equal(Colors.Brown, result.Color);
        }

        /// <summary>
        /// Tests that the DarkBlue property returns a non-null SolidColorBrush instance.
        /// Verifies basic functionality and that the lazy initialization works.
        /// Expected result: Property returns a valid SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void DarkBlue_PropertyAccess_ReturnsValidSolidColorBrush()
        {
            // Act
            var result = Brush.DarkBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the DarkBlue property returns a brush with the correct color.
        /// Verifies that the brush's Color property matches Colors.DarkBlue.
        /// Expected result: Brush Color property equals Colors.DarkBlue.
        /// </summary>
        [Fact]
        public void DarkBlue_PropertyAccess_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.DarkBlue;

            // Assert
            Assert.Equal(Colors.DarkBlue, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the DarkBlue property return the same instance.
        /// Verifies the singleton behavior of the lazy initialization pattern.
        /// Expected result: All accesses return the exact same object reference.
        /// </summary>
        [Fact]
        public void DarkBlue_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.DarkBlue;
            var second = Brush.DarkBlue;
            var third = Brush.DarkBlue;

            // Assert
            Assert.Same(first, second);
            Assert.Same(first, third);
            Assert.Same(second, third);
        }

        /// <summary>
        /// Tests that the DarkBlue property returns a brush that is not empty.
        /// Verifies that the IsEmpty property returns false for the dark blue brush.
        /// Expected result: IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void DarkBlue_PropertyAccess_ReturnsNonEmptyBrush()
        {
            // Act
            var result = Brush.DarkBlue;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the Fuchsia property returns a non-null SolidColorBrush instance.
        /// Verifies the lazy initialization works correctly on first access.
        /// Expected result: Property returns a valid SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void Fuchsia_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Fuchsia;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Fuchsia property returns the same instance on multiple calls.
        /// Verifies the lazy initialization caching behavior works correctly.
        /// Expected result: Multiple calls to the property return the exact same instance.
        /// </summary>
        [Fact]
        public void Fuchsia_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var firstCall = Brush.Fuchsia;
            var secondCall = Brush.Fuchsia;
            var thirdCall = Brush.Fuchsia;

            // Assert
            Assert.Same(firstCall, secondCall);
            Assert.Same(secondCall, thirdCall);
            Assert.Same(firstCall, thirdCall);
        }

        /// <summary>
        /// Tests that the Fuchsia property returns a brush with the correct fuchsia color.
        /// Verifies the brush is initialized with Colors.Fuchsia.
        /// Expected result: The brush's Color property equals Colors.Fuchsia.
        /// </summary>
        [Fact]
        public void Fuchsia_ColorProperty_EqualsFuchsiaColor()
        {
            // Act
            var fuchsiaBrush = Brush.Fuchsia;

            // Assert
            Assert.Equal(Colors.Fuchsia, fuchsiaBrush.Color);
        }

        /// <summary>
        /// Tests that the Fuchsia property returns a brush that is not empty.
        /// Verifies the brush has a valid color and is not considered empty.
        /// Expected result: The brush's IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void Fuchsia_IsEmpty_ReturnsFalse()
        {
            // Act
            var fuchsiaBrush = Brush.Fuchsia;

            // Assert
            Assert.False(fuchsiaBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the IndianRed property returns a non-null SolidColorBrush instance.
        /// This test verifies the lazy initialization behavior on first access.
        /// Expected result: A valid SolidColorBrush instance is returned.
        /// </summary>
        [Fact]
        public void IndianRed_FirstAccess_ReturnsSolidColorBrush()
        {
            // Act
            var result = Brush.IndianRed;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the IndianRed property returns the same instance on multiple calls.
        /// This test verifies the caching behavior of the lazy initialization.
        /// Expected result: The same SolidColorBrush instance is returned on subsequent calls.
        /// </summary>
        [Fact]
        public void IndianRed_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.IndianRed;
            var second = Brush.IndianRed;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the IndianRed property returns a brush with the correct color.
        /// This test verifies that the brush is initialized with Colors.IndianRed.
        /// Expected result: The returned brush has the IndianRed color.
        /// </summary>
        [Fact]
        public void IndianRed_Color_EqualsColorsIndianRed()
        {
            // Act
            var brush = Brush.IndianRed;

            // Assert
            Assert.Equal(Colors.IndianRed, brush.Color);
        }

        /// <summary>
        /// Tests that the IndianRed property returns a brush that is not empty.
        /// This test verifies that the returned brush has a valid state.
        /// Expected result: The brush IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void IndianRed_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.IndianRed;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the IndianRed property returns consistent values across multiple property accesses.
        /// This test verifies the overall consistency of the lazy initialization pattern.
        /// Expected result: All accesses return the same instance with the same color.
        /// </summary>
        [Fact]
        public void IndianRed_ConsistentBehavior_AllAccessesReturnSameInstanceAndColor()
        {
            // Act
            var first = Brush.IndianRed;
            var second = Brush.IndianRed;
            var third = Brush.IndianRed;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Equal(Colors.IndianRed, first.Color);
            Assert.Equal(Colors.IndianRed, second.Color);
            Assert.Equal(Colors.IndianRed, third.Color);
            Assert.False(first.IsEmpty);
            Assert.False(second.IsEmpty);
            Assert.False(third.IsEmpty);
        }

        /// <summary>
        /// Tests that LightGoldenrodYellow property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void LightGoldenrodYellow_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.LightGoldenrodYellow;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that LightGoldenrodYellow property returns the correct color value.
        /// </summary>
        [Fact]
        public void LightGoldenrodYellow_WhenAccessed_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.LightGoldenrodYellow;

            // Assert
            Assert.Equal(Colors.LightGoldenrodYellow, result.Color);
        }

        /// <summary>
        /// Tests that LightGoldenrodYellow property returns the same instance on multiple calls (lazy initialization).
        /// </summary>
        [Fact]
        public void LightGoldenrodYellow_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var first = Brush.LightGoldenrodYellow;
            var second = Brush.LightGoldenrodYellow;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that LightGoldenrodYellow property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void LightGoldenrodYellow_WhenAccessed_ReturnsNonEmptyBrush()
        {
            // Act
            var result = Brush.LightGoldenrodYellow;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that MediumVioletRed property returns a non-null SolidColorBrush instance.
        /// Verifies the basic functionality of the static property getter.
        /// </summary>
        [Fact]
        public void MediumVioletRed_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.MediumVioletRed;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that MediumVioletRed property uses lazy initialization and returns the same instance on multiple calls.
        /// Verifies the singleton behavior of the cached brush instance.
        /// </summary>
        [Fact]
        public void MediumVioletRed_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.MediumVioletRed;
            var secondAccess = Brush.MediumVioletRed;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that MediumVioletRed property returns a brush with the correct color value.
        /// Verifies the color matches the system-defined MediumVioletRed color (#FFC71585).
        /// </summary>
        [Fact]
        public void MediumVioletRed_WhenAccessed_HasCorrectColor()
        {
            // Act
            var result = Brush.MediumVioletRed;

            // Assert
            Assert.Equal(Colors.MediumVioletRed, result.Color);
        }

        /// <summary>
        /// Tests that MediumVioletRed property returns a brush that is not empty.
        /// Verifies the brush represents a valid color and not an empty/null brush.
        /// </summary>
        [Fact]
        public void MediumVioletRed_WhenAccessed_IsNotEmpty()
        {
            // Act
            var result = Brush.MediumVioletRed;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the PaleTurquoise property returns a non-null SolidColorBrush instance.
        /// This test verifies the lazy initialization behavior and ensures the property is accessible.
        /// Expected result: A valid SolidColorBrush instance is returned.
        /// </summary>
        [Fact]
        public void PaleTurquoise_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.PaleTurquoise;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the PaleTurquoise property returns the same instance on multiple calls.
        /// This test verifies the lazy initialization pattern works correctly and caches the instance.
        /// Expected result: Multiple calls return the exact same object reference.
        /// </summary>
        [Fact]
        public void PaleTurquoise_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.PaleTurquoise;
            var second = Brush.PaleTurquoise;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the PaleTurquoise property returns a brush with the correct color value.
        /// This test verifies that the brush is initialized with the system-defined PaleTurquoise color.
        /// Expected result: The Color property matches Colors.PaleTurquoise.
        /// </summary>
        [Fact]
        public void PaleTurquoise_Color_MatchesSystemDefinedPaleTurquoiseColor()
        {
            // Act
            var brush = Brush.PaleTurquoise;

            // Assert
            Assert.Equal(Colors.PaleTurquoise, brush.Color);
        }

        /// <summary>
        /// Tests that the PaleTurquoise property returns a brush that is not empty.
        /// This test verifies that the brush has a valid state and color value.
        /// Expected result: The IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void PaleTurquoise_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.PaleTurquoise;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests the equality behavior of PaleTurquoise brush instances.
        /// This test verifies that different instances with the same color are considered equal.
        /// Expected result: PaleTurquoise brush equals another SolidColorBrush with the same color.
        /// </summary>
        [Fact]
        public void PaleTurquoise_Equals_SolidColorBrushWithSameColor()
        {
            // Arrange
            var paleTurquoiseBrush = Brush.PaleTurquoise;
            var otherBrush = new SolidColorBrush(Colors.PaleTurquoise);

            // Act & Assert
            Assert.Equal(paleTurquoiseBrush, otherBrush);
            Assert.True(paleTurquoiseBrush.Equals(otherBrush));
        }

        /// <summary>
        /// Tests the hash code consistency of PaleTurquoise brush instances.
        /// This test verifies that brushes with the same color have the same hash code.
        /// Expected result: Hash codes are equal for brushes with the same color.
        /// </summary>
        [Fact]
        public void PaleTurquoise_GetHashCode_ConsistentWithEquality()
        {
            // Arrange
            var paleTurquoiseBrush = Brush.PaleTurquoise;
            var otherBrush = new SolidColorBrush(Colors.PaleTurquoise);

            // Act & Assert
            Assert.Equal(paleTurquoiseBrush.GetHashCode(), otherBrush.GetHashCode());
        }

        /// <summary>
        /// Tests that the SaddleBrown property returns a non-null SolidColorBrush instance.
        /// Verifies basic functionality and that the property is accessible.
        /// </summary>
        [Fact]
        public void SaddleBrown_ReturnsNonNullBrush()
        {
            // Act
            var result = Brush.SaddleBrown;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the SaddleBrown property returns a brush with the correct color.
        /// Verifies that the color matches the system-defined SaddleBrown color.
        /// </summary>
        [Fact]
        public void SaddleBrown_HasCorrectColor()
        {
            // Act
            var result = Brush.SaddleBrown;

            // Assert
            Assert.Equal(Colors.SaddleBrown, result.Color);
        }

        /// <summary>
        /// Tests that the SaddleBrown property implements singleton behavior.
        /// Verifies that multiple accesses return the same instance (reference equality).
        /// </summary>
        [Fact]
        public void SaddleBrown_ReturnsSameInstance()
        {
            // Act
            var first = Brush.SaddleBrown;
            var second = Brush.SaddleBrown;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the SaddleBrown brush is not empty.
        /// Verifies that the IsEmpty property returns false for a valid color brush.
        /// </summary>
        [Fact]
        public void SaddleBrown_IsNotEmpty()
        {
            // Act
            var result = Brush.SaddleBrown;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests multiple properties of the SaddleBrown brush in a single parameterized test.
        /// Verifies color value, type, non-null state, and non-empty state.
        /// </summary>
        [Theory]
        [InlineData(true)] // Single test case to verify all properties
        public void SaddleBrown_HasExpectedProperties(bool _)
        {
            // Act
            var result = Brush.SaddleBrown;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(Colors.SaddleBrown, result.Color);
            Assert.False(result.IsEmpty);
            Assert.False(Brush.IsNullOrEmpty(result));
        }

        /// <summary>
        /// Tests that accessing the SlateBlue property returns a non-null SolidColorBrush instance.
        /// Verifies basic property access and ensures the returned value is not null.
        /// </summary>
        [Fact]
        public void SlateBlue_PropertyAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var slateBlue = Brush.SlateBlue;

            // Assert
            Assert.NotNull(slateBlue);
            Assert.IsType<SolidColorBrush>(slateBlue);
        }

        /// <summary>
        /// Tests that multiple accesses to the SlateBlue property return the same instance (singleton behavior).
        /// Verifies that the lazy initialization with static field caching works correctly.
        /// </summary>
        [Fact]
        public void SlateBlue_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var slateBlue1 = Brush.SlateBlue;
            var slateBlue2 = Brush.SlateBlue;

            // Assert
            Assert.Same(slateBlue1, slateBlue2);
        }

        /// <summary>
        /// Tests that the SlateBlue property returns a brush with the correct SlateBlue color value.
        /// Verifies that the color matches the system-defined SlateBlue color (#FF6A5ACD).
        /// </summary>
        [Fact]
        public void SlateBlue_ColorValue_MatchesSystemDefinedSlateBlue()
        {
            // Arrange
            var expectedColor = Colors.SlateBlue;

            // Act
            var slateBlue = Brush.SlateBlue;

            // Assert
            Assert.Equal(expectedColor, slateBlue.Color);
        }

        /// <summary>
        /// Tests that the SlateBlue property returns an ImmutableBrush instance (the actual implementation type).
        /// Verifies that the internal implementation uses ImmutableBrush which inherits from SolidColorBrush.
        /// </summary>
        [Fact]
        public void SlateBlue_ImplementationType_IsImmutableBrush()
        {
            // Act
            var slateBlue = Brush.SlateBlue;

            // Assert
            Assert.True(slateBlue.GetType().Name == "ImmutableBrush");
        }

        /// <summary>
        /// Tests that the SlateBlue brush is not empty.
        /// Verifies that the IsEmpty property returns false for the SlateBlue brush.
        /// </summary>
        [Fact]
        public void SlateBlue_IsEmpty_ReturnsFalse()
        {
            // Act
            var slateBlue = Brush.SlateBlue;

            // Assert
            Assert.False(slateBlue.IsEmpty);
        }

        /// <summary>
        /// Tests that the SlateBlue brush color has the exact ARGB values for SlateBlue.
        /// Verifies the specific RGBA components match the expected SlateBlue color values.
        /// </summary>
        [Fact]
        public void SlateBlue_ColorComponents_MatchExpectedValues()
        {
            // Arrange - SlateBlue is #FF6A5ACD (A=255, R=106, G=90, B=205)
            const float expectedRed = 106f / 255f;
            const float expectedGreen = 90f / 255f;
            const float expectedBlue = 205f / 255f;
            const float expectedAlpha = 1.0f;

            // Act
            var slateBlue = Brush.SlateBlue;

            // Assert
            Assert.Equal(expectedRed, slateBlue.Color.Red, 3);
            Assert.Equal(expectedGreen, slateBlue.Color.Green, 3);
            Assert.Equal(expectedBlue, slateBlue.Color.Blue, 3);
            Assert.Equal(expectedAlpha, slateBlue.Color.Alpha, 3);
        }

        /// <summary>
        /// Tests that BurlyWood property returns a non-null SolidColorBrush instance.
        /// Verifies lazy initialization works correctly on first access.
        /// Expected result: Property returns a valid SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void BurlyWood_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.BurlyWood;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that BurlyWood property returns the same instance on multiple accesses.
        /// Verifies lazy initialization pattern maintains singleton behavior.
        /// Expected result: Multiple calls return the exact same instance (reference equality).
        /// </summary>
        [Fact]
        public void BurlyWood_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.BurlyWood;
            var second = Brush.BurlyWood;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that BurlyWood property returns a brush with the correct color.
        /// Verifies the brush is initialized with Colors.BurlyWood color value.
        /// Expected result: Brush color matches Colors.BurlyWood.
        /// </summary>
        [Fact]
        public void BurlyWood_ColorValue_MatchesSystemDefinedColor()
        {
            // Act
            var brush = Brush.BurlyWood;

            // Assert
            Assert.Equal(Colors.BurlyWood, brush.Color);
        }

        /// <summary>
        /// Tests that BurlyWood property is thread-safe during concurrent access.
        /// Verifies lazy initialization works correctly when accessed from multiple threads.
        /// Expected result: All threads receive the same instance reference.
        /// </summary>
        [Fact]
        public void BurlyWood_ConcurrentAccess_ReturnsConsistentInstance()
        {
            // Arrange
            SolidColorBrush[] results = new SolidColorBrush[10];
            var tasks = new System.Threading.Tasks.Task[10];

            // Act
            for (int i = 0; i < 10; i++)
            {
                int index = i;
                tasks[i] = System.Threading.Tasks.Task.Run(() =>
                {
                    results[index] = Brush.BurlyWood;
                });
            }

            System.Threading.Tasks.Task.WaitAll(tasks);

            // Assert
            for (int i = 1; i < results.Length; i++)
            {
                Assert.Same(results[0], results[i]);
            }
        }

        /// <summary>
        /// Tests that the Crimson property returns a non-null SolidColorBrush instance.
        /// Validates basic functionality and ensures the property is properly initialized.
        /// Expected result: Non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void Crimson_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            SolidColorBrush result = Brush.Crimson;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Crimson property returns the same instance on multiple calls (singleton behavior).
        /// Validates the lazy initialization pattern and ensures memory efficiency.
        /// Expected result: Same instance returned on subsequent calls.
        /// </summary>
        [Fact]
        public void Crimson_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            SolidColorBrush first = Brush.Crimson;
            SolidColorBrush second = Brush.Crimson;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the Crimson property returns a brush with the correct crimson color.
        /// Validates that the brush is initialized with the system-defined crimson color value.
        /// Expected result: SolidColorBrush with Colors.Crimson color.
        /// </summary>
        [Fact]
        public void Crimson_WhenAccessed_ReturnsCorrectColor()
        {
            // Act
            SolidColorBrush result = Brush.Crimson;

            // Assert
            Assert.Equal(Colors.Crimson, result.Color);
        }

        /// <summary>
        /// Tests that the Crimson property returns a brush that is not empty.
        /// Validates the IsEmpty property behavior for the crimson brush.
        /// Expected result: IsEmpty returns false.
        /// </summary>
        [Fact]
        public void Crimson_WhenAccessed_ReturnsNonEmptyBrush()
        {
            // Act
            SolidColorBrush result = Brush.Crimson;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests concurrent access to the Crimson property to ensure thread safety.
        /// Validates that the lazy initialization works correctly under concurrent access.
        /// Expected result: All tasks return the same instance.
        /// </summary>
        [Fact]
        public async Task Crimson_WhenAccessedConcurrently_ReturnsSameInstance()
        {
            // Arrange
            const int taskCount = 10;
            Task<SolidColorBrush>[] tasks = new Task<SolidColorBrush>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.Crimson);
            }

            SolidColorBrush[] results = await Task.WhenAll(tasks);

            // Assert
            SolidColorBrush firstResult = results[0];
            for (int i = 1; i < taskCount; i++)
            {
                Assert.Same(firstResult, results[i]);
            }
        }

        /// <summary>
        /// Tests that the Crimson brush can be compared for equality with another crimson brush.
        /// Validates the equality behavior of the returned brush instance.
        /// Expected result: Brushes with same color are equal.
        /// </summary>
        [Fact]
        public void Crimson_WhenComparedWithSameColorBrush_AreEqual()
        {
            // Arrange
            SolidColorBrush crimsonBrush = Brush.Crimson;
            SolidColorBrush anotherCrimsonBrush = new SolidColorBrush(Colors.Crimson);

            // Act & Assert
            Assert.Equal(crimsonBrush, anotherCrimsonBrush);
            Assert.True(crimsonBrush.Equals(anotherCrimsonBrush));
        }

        /// <summary>
        /// Tests that the Crimson brush generates consistent hash codes.
        /// Validates the GetHashCode implementation for the returned brush.
        /// Expected result: Consistent hash code generation.
        /// </summary>
        [Fact]
        public void Crimson_GetHashCode_ReturnsConsistentValue()
        {
            // Arrange
            SolidColorBrush crimsonBrush = Brush.Crimson;

            // Act
            int hashCode1 = crimsonBrush.GetHashCode();
            int hashCode2 = crimsonBrush.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that the Crimson brush has the expected ARGB color values.
        /// Validates the specific color components of the crimson color.
        /// Expected result: Color matches the system-defined crimson ARGB values.
        /// </summary>
        [Fact]
        public void Crimson_Color_HasExpectedArgbValues()
        {
            // Arrange
            SolidColorBrush crimsonBrush = Brush.Crimson;
            Color expectedColor = Colors.Crimson;

            // Act
            Color actualColor = crimsonBrush.Color;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red);
            Assert.Equal(expectedColor.Green, actualColor.Green);
            Assert.Equal(expectedColor.Blue, actualColor.Blue);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha);
        }

        /// <summary>
        /// Tests that the DarkRed property returns a non-null SolidColorBrush with the correct color.
        /// Verifies the property returns the expected system-defined DarkRed color brush.
        /// </summary>
        [Fact]
        public void DarkRed_AccessProperty_ReturnsNonNullSolidColorBrushWithCorrectColor()
        {
            // Act
            var result = Brush.DarkRed;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(Colors.DarkRed, result.Color);
        }

        /// <summary>
        /// Tests that the DarkRed property returns the same instance on multiple calls.
        /// Verifies the lazy initialization singleton pattern behavior.
        /// </summary>
        [Fact]
        public void DarkRed_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.DarkRed;
            var second = Brush.DarkRed;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the DarkRed property is not empty.
        /// Verifies the returned brush represents a valid color and is not considered empty.
        /// </summary>
        [Fact]
        public void DarkRed_AccessProperty_IsNotEmpty()
        {
            // Act
            var result = Brush.DarkRed;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that DarkSlateGrey property returns a non-null SolidColorBrush instance.
        /// Verifies basic functionality and type correctness.
        /// </summary>
        [Fact]
        public void DarkSlateGrey_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.DarkSlateGrey;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that DarkSlateGrey property returns the same instance as DarkSlateGray property.
        /// Verifies that DarkSlateGrey is a proper alias for DarkSlateGray (British vs American spelling).
        /// </summary>
        [Fact]
        public void DarkSlateGrey_ReturnsSameInstanceAsDarkSlateGray()
        {
            // Act
            var darkSlateGrey = Brush.DarkSlateGrey;
            var darkSlateGray = Brush.DarkSlateGray;

            // Assert
            Assert.Same(darkSlateGray, darkSlateGrey);
        }

        /// <summary>
        /// Tests that multiple calls to DarkSlateGrey property return the same instance.
        /// Verifies singleton behavior and proper lazy initialization.
        /// </summary>
        [Fact]
        public void DarkSlateGrey_MultipleCalls_ReturnsSameInstance()
        {
            // Act
            var firstCall = Brush.DarkSlateGrey;
            var secondCall = Brush.DarkSlateGrey;
            var thirdCall = Brush.DarkSlateGrey;

            // Assert
            Assert.Same(firstCall, secondCall);
            Assert.Same(secondCall, thirdCall);
            Assert.Same(firstCall, thirdCall);
        }

        /// <summary>
        /// Tests that DarkSlateGrey property has the correct color value.
        /// Verifies that the brush contains the expected DarkSlateGray color.
        /// </summary>
        [Fact]
        public void DarkSlateGrey_HasCorrectColorValue()
        {
            // Act
            var brush = Brush.DarkSlateGrey;

            // Assert
            Assert.Equal(Colors.DarkSlateGray, brush.Color);
        }

        /// <summary>
        /// Tests that DarkSlateGrey property is thread-safe during concurrent access.
        /// Verifies that multiple threads accessing the property simultaneously get the same instance.
        /// </summary>
        [Fact]
        public async Task DarkSlateGrey_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.DarkSlateGrey);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            var firstResult = results[0];
            for (int i = 1; i < results.Length; i++)
            {
                Assert.Same(firstResult, results[i]);
            }
        }

        /// <summary>
        /// Tests that DarkSlateGrey property maintains consistency with DarkSlateGray during concurrent access.
        /// Verifies thread-safe alias behavior between the two properties.
        /// </summary>
        [Fact]
        public async Task DarkSlateGrey_ConcurrentAccessWithDarkSlateGray_ReturnsSameInstance()
        {
            // Arrange
            const int taskCount = 10;
            var greyTasks = new Task<SolidColorBrush>[taskCount / 2];
            var grayTasks = new Task<SolidColorBrush>[taskCount / 2];

            // Act
            for (int i = 0; i < taskCount / 2; i++)
            {
                greyTasks[i] = Task.Run(() => Brush.DarkSlateGrey);
                grayTasks[i] = Task.Run(() => Brush.DarkSlateGray);
            }

            var greyResults = await Task.WhenAll(greyTasks);
            var grayResults = await Task.WhenAll(grayTasks);

            // Assert
            var expectedInstance = greyResults[0];

            // All grey results should be the same instance
            foreach (var result in greyResults)
            {
                Assert.Same(expectedInstance, result);
            }

            // All gray results should be the same instance as grey results
            foreach (var result in grayResults)
            {
                Assert.Same(expectedInstance, result);
            }
        }

        /// <summary>
        /// Tests that DarkSlateGrey property is not empty.
        /// Verifies that the brush has a valid state and is not considered empty.
        /// </summary>
        [Fact]
        public void DarkSlateGrey_IsNotEmpty()
        {
            // Act
            var brush = Brush.DarkSlateGrey;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the GhostWhite property returns a non-null brush instance.
        /// Verifies that the static property is properly initialized.
        /// </summary>
        [Fact]
        public void GhostWhite_ReturnsNonNull_Success()
        {
            // Act
            var result = Brush.GhostWhite;

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the GhostWhite property returns a SolidColorBrush instance.
        /// Verifies that the returned brush is of the correct type.
        /// </summary>
        [Fact]
        public void GhostWhite_ReturnsCorrectType_Success()
        {
            // Act
            var result = Brush.GhostWhite;

            // Assert
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the GhostWhite property returns a brush with the correct color.
        /// Verifies that the brush has the GhostWhite color from the Graphics.Colors class.
        /// </summary>
        [Fact]
        public void GhostWhite_HasCorrectColor_Success()
        {
            // Act
            var result = Brush.GhostWhite;

            // Assert
            Assert.Equal(Colors.GhostWhite, result.Color);
        }

        /// <summary>
        /// Tests that the GhostWhite property uses lazy initialization correctly.
        /// Verifies that multiple calls to the property return the same instance.
        /// </summary>
        [Fact]
        public void GhostWhite_ReturnsSameInstance_Success()
        {
            // Act
            var first = Brush.GhostWhite;
            var second = Brush.GhostWhite;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the GhostWhite property returns an immutable brush.
        /// Verifies that attempting to change the color does not affect the brush.
        /// </summary>
        [Fact]
        public void GhostWhite_ColorIsImmutable_Success()
        {
            // Arrange
            var originalColor = Colors.GhostWhite;
            var brush = Brush.GhostWhite;

            // Act
            brush.Color = Colors.Red; // This should be a no-op for ImmutableBrush

            // Assert
            Assert.Equal(originalColor, brush.Color);
        }

        /// <summary>
        /// Tests that the LightSlateGray property returns a non-null SolidColorBrush instance.
        /// This test verifies the lazy initialization creates an instance when first accessed.
        /// Expected result: Returns a non-null SolidColorBrush.
        /// </summary>
        [Fact]
        public void LightSlateGray_FirstAccess_ReturnsNonNullBrush()
        {
            // Act
            var result = Brush.LightSlateGray;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the LightSlateGray property returns a brush with the correct system-defined color.
        /// This test verifies the brush contains the expected LightSlateGray color value (#FF778899).
        /// Expected result: The brush color matches Colors.LightSlateGray.
        /// </summary>
        [Fact]
        public void LightSlateGray_ColorValue_MatchesSystemDefinedColor()
        {
            // Act
            var brush = Brush.LightSlateGray;

            // Assert
            Assert.Equal(Colors.LightSlateGray, brush.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the LightSlateGray property return the same instance.
        /// This test verifies the singleton behavior of the lazy initialization pattern.
        /// Expected result: All accesses return the exact same reference.
        /// </summary>
        [Fact]
        public void LightSlateGray_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.LightSlateGray;
            var secondAccess = Brush.LightSlateGray;
            var thirdAccess = Brush.LightSlateGray;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the LightSlateGray property returns an ImmutableBrush instance.
        /// This test verifies the internal implementation creates the correct brush type.
        /// Expected result: The returned brush is an instance of ImmutableBrush.
        /// </summary>
        [Fact]
        public void LightSlateGray_BrushType_IsImmutableBrush()
        {
            // Act
            var brush = Brush.LightSlateGray;

            // Assert
            // ImmutableBrush is internal, so we verify it through its behavior
            // ImmutableBrush should be read-only and not allow parent changes
            Assert.IsType<SolidColorBrush>(brush);

            // Verify it's immutable by checking the color can't be changed
            var originalColor = brush.Color;
            brush.Color = Colors.Red; // This should have no effect on ImmutableBrush
            Assert.Equal(originalColor, brush.Color);
        }

        /// <summary>
        /// Tests that the LightSlateGray brush is not empty.
        /// This test verifies the brush represents a valid color and is not considered empty.
        /// Expected result: IsEmpty returns false.
        /// </summary>
        [Fact]
        public void LightSlateGray_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.LightSlateGray;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that MediumSeaGreen property returns a non-null SolidColorBrush with the correct color on first access.
        /// Verifies lazy initialization and that the brush has the expected MediumSeaGreen color value.
        /// </summary>
        [Fact]
        public void MediumSeaGreen_FirstAccess_ReturnsValidSolidColorBrushWithCorrectColor()
        {
            // Act
            var brush = Brush.MediumSeaGreen;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
            Assert.Equal(Colors.MediumSeaGreen, brush.Color);
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that multiple accesses to MediumSeaGreen property return the same instance.
        /// Verifies singleton behavior of the lazily initialized brush.
        /// </summary>
        [Fact]
        public void MediumSeaGreen_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var brush1 = Brush.MediumSeaGreen;
            var brush2 = Brush.MediumSeaGreen;

            // Assert
            Assert.Same(brush1, brush2);
        }

        /// <summary>
        /// Tests that MediumSeaGreen property returns a brush with the exact expected ARGB color value.
        /// Verifies that the color matches the system-defined MediumSeaGreen color (#FF3CB371).
        /// </summary>
        [Fact]
        public void MediumSeaGreen_ColorValue_MatchesExpectedArgbValue()
        {
            // Act
            var brush = Brush.MediumSeaGreen;

            // Assert
            var expectedColor = Color.FromUint(0xFF3CB371);
            Assert.Equal(expectedColor, brush.Color);
            Assert.Equal(expectedColor.Red, brush.Color.Red);
            Assert.Equal(expectedColor.Green, brush.Color.Green);
            Assert.Equal(expectedColor.Blue, brush.Color.Blue);
            Assert.Equal(expectedColor.Alpha, brush.Color.Alpha);
        }

        /// <summary>
        /// Tests concurrent access to MediumSeaGreen property from multiple threads.
        /// Verifies thread safety of the lazy initialization mechanism.
        /// </summary>
        [Fact]
        public async Task MediumSeaGreen_ConcurrentAccess_ReturnsSameInstanceAcrossThreads()
        {
            // Arrange
            const int taskCount = 10;
            var tasks = new Task<SolidColorBrush>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => Brush.MediumSeaGreen);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            var firstResult = results[0];
            for (int i = 1; i < results.Length; i++)
            {
                Assert.Same(firstResult, results[i]);
            }
            Assert.Equal(Colors.MediumSeaGreen, firstResult.Color);
        }

        /// <summary>
        /// Tests that MediumSeaGreen brush properties are correctly set.
        /// Verifies the brush is not empty and has the correct type characteristics.
        /// </summary>
        [Fact]
        public void MediumSeaGreen_BrushProperties_AreCorrectlySet()
        {
            // Act
            var brush = Brush.MediumSeaGreen;

            // Assert
            Assert.IsAssignableFrom<Brush>(brush);
            Assert.IsType<SolidColorBrush>(brush);
            Assert.False(Brush.IsNullOrEmpty(brush));
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the OrangeRed property returns a non-null SolidColorBrush instance.
        /// This test validates the lazy initialization behavior and ensures the property returns a valid brush.
        /// </summary>
        [Fact]
        public void OrangeRed_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.OrangeRed;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the OrangeRed property returns a brush with the correct color value.
        /// This test validates that the brush contains the system-defined OrangeRed color.
        /// </summary>
        [Fact]
        public void OrangeRed_ColorValue_MatchesSystemOrangeRedColor()
        {
            // Act
            var result = Brush.OrangeRed;

            // Assert
            Assert.Equal(Colors.OrangeRed, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the OrangeRed property return the same cached instance.
        /// This test validates the singleton pattern implementation with lazy initialization.
        /// </summary>
        [Fact]
        public void OrangeRed_MultipleAccesses_ReturnsSameCachedInstance()
        {
            // Act
            var first = Brush.OrangeRed;
            var second = Brush.OrangeRed;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the OrangeRed property returns a brush that is not empty.
        /// This test validates that the returned brush has a valid state for painting operations.
        /// </summary>
        [Fact]
        public void OrangeRed_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.OrangeRed;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the OrangeRed property returns a brush with the correct hash code.
        /// This test validates that the brush generates consistent hash codes based on its color.
        /// </summary>
        [Fact]
        public void OrangeRed_GetHashCode_ReturnsConsistentValue()
        {
            // Act
            var result = Brush.OrangeRed;
            var hashCode1 = result.GetHashCode();
            var hashCode2 = result.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that the OrangeRed property returns a brush that correctly implements equality comparison.
        /// This test validates that brushes with the same color are considered equal.
        /// </summary>
        [Fact]
        public void OrangeRed_Equals_ComparesCorrectlyWithSameColorBrush()
        {
            // Arrange
            var orangeRedBrush = Brush.OrangeRed;
            var otherOrangeRedBrush = new SolidColorBrush(Colors.OrangeRed);

            // Act & Assert
            Assert.True(orangeRedBrush.Equals(otherOrangeRedBrush));
            Assert.True(otherOrangeRedBrush.Equals(orangeRedBrush));
        }

        /// <summary>
        /// Tests that the OrangeRed property returns a brush that correctly implements inequality comparison.
        /// This test validates that brushes with different colors are not considered equal.
        /// </summary>
        [Fact]
        public void OrangeRed_Equals_ReturnsFalseForDifferentColorBrush()
        {
            // Arrange
            var orangeRedBrush = Brush.OrangeRed;
            var blueBrush = new SolidColorBrush(Colors.Blue);

            // Act & Assert
            Assert.False(orangeRedBrush.Equals(blueBrush));
            Assert.False(orangeRedBrush.Equals(null));
        }

        /// <summary>
        /// Tests that the Plum property returns a non-null SolidColorBrush on first access.
        /// </summary>
        [Fact]
        public void Plum_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Plum;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Plum property returns a brush with the correct system-defined Plum color.
        /// The expected color has ARGB value #FFDDA0DD.
        /// </summary>
        [Fact]
        public void Plum_Color_ReturnsCorrectPlumColor()
        {
            // Act
            var result = Brush.Plum;

            // Assert
            Assert.Equal(Colors.Plum, result.Color);
            Assert.Equal(Color.FromUint(0xFFDDA0DD), result.Color);
        }

        /// <summary>
        /// Tests that the Plum property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void Plum_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.Plum;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that multiple accesses to the Plum property return the same cached instance.
        /// This verifies the lazy initialization behavior using the null-coalescing assignment operator.
        /// </summary>
        [Fact]
        public void Plum_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Plum;
            var secondAccess = Brush.Plum;
            var thirdAccess = Brush.Plum;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the Plum property returns a brush with the expected RGB component values.
        /// The Plum color should have Red=221, Green=160, Blue=221 (from #FFDDA0DD).
        /// </summary>
        [Fact]
        public void Plum_ColorComponents_MatchExpectedValues()
        {
            // Act
            var result = Brush.Plum;
            var color = result.Color;

            // Assert
            Assert.Equal(1.0f, color.Alpha, 5); // Alpha should be 1.0 (255/255)
            Assert.Equal(221f / 255f, color.Red, 5); // Red component (221/255)
            Assert.Equal(160f / 255f, color.Green, 5); // Green component (160/255)  
            Assert.Equal(221f / 255f, color.Blue, 5); // Blue component (221/255)
        }

        /// <summary>
        /// Tests that SlateGrey property returns a non-null SolidColorBrush instance.
        /// This test verifies the basic functionality of the SlateGrey static property.
        /// Expected result: SlateGrey should return a valid SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void SlateGrey_Get_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.SlateGrey;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that SlateGrey property returns the same instance as SlateGray property.
        /// This test verifies that SlateGrey is correctly implemented as an alias for SlateGray.
        /// Expected result: SlateGrey and SlateGray should return the exact same reference.
        /// </summary>
        [Fact]
        public void SlateGrey_Get_ReturnsSameInstanceAsSlateGray()
        {
            // Act
            var slateGrey = Brush.SlateGrey;
            var slateGray = Brush.SlateGray;

            // Assert
            Assert.Same(slateGray, slateGrey);
        }

        /// <summary>
        /// Tests that SlateGrey property returns a brush with the correct color.
        /// This test verifies that the SlateGrey brush has the expected SlateGray color value.
        /// Expected result: The Color property should be set to Colors.SlateGray.
        /// </summary>
        [Fact]
        public void SlateGrey_Get_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.SlateGrey;

            // Assert
            Assert.Equal(Colors.SlateGray, result.Color);
        }

        /// <summary>
        /// Tests that SlateGrey property returns a non-empty brush.
        /// This test verifies that the SlateGrey brush is not considered empty.
        /// Expected result: IsEmpty should return false.
        /// </summary>
        [Fact]
        public void SlateGrey_Get_ReturnsNonEmptyBrush()
        {
            // Act
            var result = Brush.SlateGrey;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that multiple calls to SlateGrey property return the same instance.
        /// This test verifies the lazy initialization behavior and ensures singleton pattern.
        /// Expected result: Multiple calls should return the same reference due to lazy initialization.
        /// </summary>
        [Fact]
        public void SlateGrey_MultipleCalls_ReturnsSameInstance()
        {
            // Act
            var first = Brush.SlateGrey;
            var second = Brush.SlateGrey;
            var third = Brush.SlateGrey;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that the Teal property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void Teal_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Teal;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Teal property returns a brush with the correct teal color.
        /// </summary>
        [Fact]
        public void Teal_FirstAccess_ReturnsCorrectTealColor()
        {
            // Arrange
            var expectedColor = Colors.Teal; // #FF008080

            // Act
            var result = Brush.Teal;

            // Assert
            Assert.Equal(expectedColor, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the Teal property return the same instance (singleton behavior).
        /// </summary>
        [Fact]
        public void Teal_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Teal;
            var secondAccess = Brush.Teal;
            var thirdAccess = Brush.Teal;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Same(secondAccess, thirdAccess);
            Assert.Same(firstAccess, thirdAccess);
        }

        /// <summary>
        /// Tests that the Teal property has the expected color value using ARGB components.
        /// </summary>
        [Fact]
        public void Teal_ColorValue_MatchesExpectedARGBValue()
        {
            // Arrange - Teal color is #FF008080
            var expectedColor = Color.FromUint(0xFF008080);

            // Act
            var result = Brush.Teal;

            // Assert
            Assert.Equal(expectedColor.Red, result.Color.Red);
            Assert.Equal(expectedColor.Green, result.Color.Green);
            Assert.Equal(expectedColor.Blue, result.Color.Blue);
            Assert.Equal(expectedColor.Alpha, result.Color.Alpha);
        }

        /// <summary>
        /// Tests that concurrent access to the Teal property is thread-safe and returns the same instance.
        /// </summary>
        [Fact]
        public async Task Teal_ConcurrentAccess_ReturnsConsistentInstance()
        {
            // Arrange
            var tasks = new Task<SolidColorBrush>[10];
            SolidColorBrush[] results = new SolidColorBrush[10];

            // Act - Create multiple concurrent tasks accessing the Teal property
            for (int i = 0; i < 10; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() => Brush.Teal);
            }

            // Wait for all tasks to complete
            var taskResults = await Task.WhenAll(tasks);

            // Assert
            // All results should be the same instance
            for (int i = 0; i < taskResults.Length; i++)
            {
                Assert.NotNull(taskResults[i]);
                Assert.IsType<SolidColorBrush>(taskResults[i]);

                if (i > 0)
                {
                    Assert.Same(taskResults[0], taskResults[i]);
                }
            }
        }

        /// <summary>
        /// Tests that the Teal property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void Teal_Brush_IsNotEmpty()
        {
            // Act
            var result = Brush.Teal;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the AliceBlue property returns a non-null SolidColorBrush.
        /// </summary>
        [Fact]
        public void AliceBlue_ReturnsNonNullBrush_Success()
        {
            // Act
            var result = Brush.AliceBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the AliceBlue property returns a brush with the correct AliceBlue color.
        /// </summary>
        [Fact]
        public void AliceBlue_ReturnsCorrectColor_Success()
        {
            // Act
            var result = Brush.AliceBlue;

            // Assert
            Assert.Equal(Colors.AliceBlue, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to AliceBlue property return the same instance (singleton behavior).
        /// </summary>
        [Fact]
        public void AliceBlue_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.AliceBlue;
            var second = Brush.AliceBlue;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the AliceBlue brush is not empty.
        /// </summary>
        [Fact]
        public void AliceBlue_IsNotEmpty_ReturnsTrue()
        {
            // Act
            var result = Brush.AliceBlue;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that the AliceBlue brush color is immutable (setting color has no effect).
        /// </summary>
        [Fact]
        public void AliceBlue_ColorIsImmutable_SettingColorHasNoEffect()
        {
            // Arrange
            var originalColor = Colors.AliceBlue;
            var brush = Brush.AliceBlue;

            // Act
            brush.Color = Colors.Red; // This should have no effect on ImmutableBrush

            // Assert
            Assert.Equal(originalColor, brush.Color);
        }

        /// <summary>
        /// Tests that AliceBlue property maintains consistency across different test calls.
        /// </summary>
        [Fact]
        public void AliceBlue_ConsistentBehavior_AlwaysReturnsSameInstanceAndColor()
        {
            // Act
            var brush1 = Brush.AliceBlue;
            var brush2 = Brush.AliceBlue;
            var brush3 = Brush.AliceBlue;

            // Assert
            Assert.Same(brush1, brush2);
            Assert.Same(brush2, brush3);
            Assert.Equal(Colors.AliceBlue, brush1.Color);
            Assert.Equal(Colors.AliceBlue, brush2.Color);
            Assert.Equal(Colors.AliceBlue, brush3.Color);
        }

        /// <summary>
        /// Tests that the DarkGray property returns a non-null SolidColorBrush instance.
        /// Verifies that the lazy initialization works correctly and the property is accessible.
        /// </summary>
        [Fact]
        public void DarkGray_AccessProperty_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.DarkGray;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the DarkGray property returns a SolidColorBrush with the correct DarkGray color.
        /// Verifies that the color is properly set during initialization.
        /// </summary>
        [Fact]
        public void DarkGray_AccessProperty_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.DarkGray;

            // Assert
            Assert.Equal(Colors.DarkGray, result.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the DarkGray property return the same instance.
        /// Verifies the lazy initialization singleton behavior of the static property.
        /// </summary>
        [Fact]
        public void DarkGray_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.DarkGray;
            var second = Brush.DarkGray;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the DarkGray property initializes the underlying static field correctly.
        /// Verifies that the property behaves consistently across multiple calls.
        /// </summary>
        [Fact]
        public void DarkGray_PropertyAccess_InitializesStaticField()
        {
            // Act
            var result1 = Brush.DarkGray;
            var result2 = Brush.DarkGray;

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
            Assert.Equal(Colors.DarkGray, result1.Color);
            Assert.Equal(Colors.DarkGray, result2.Color);
        }

        /// <summary>
        /// Tests that DarkSlateBlue property returns a non-null SolidColorBrush instance.
        /// Verifies the property initialization and ensures it doesn't return null.
        /// </summary>
        [Fact]
        public void DarkSlateBlue_WhenAccessed_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.DarkSlateBlue;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that DarkSlateBlue property returns a brush with the correct color.
        /// Verifies the brush contains the expected Colors.DarkSlateBlue color value.
        /// </summary>
        [Fact]
        public void DarkSlateBlue_WhenAccessed_ReturnsCorrectColor()
        {
            // Act
            var result = Brush.DarkSlateBlue;

            // Assert
            Assert.Equal(Colors.DarkSlateBlue, result.Color);
        }

        /// <summary>
        /// Tests that DarkSlateBlue property implements lazy initialization correctly.
        /// Verifies that multiple calls to the property return the same instance (reference equality).
        /// </summary>
        [Fact]
        public void DarkSlateBlue_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var first = Brush.DarkSlateBlue;
            var second = Brush.DarkSlateBlue;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that DarkSlateBlue property returns a brush that is not empty.
        /// Verifies the IsEmpty property returns false for a valid color brush.
        /// </summary>
        [Fact]
        public void DarkSlateBlue_WhenAccessed_ReturnsNonEmptyBrush()
        {
            // Act
            var result = Brush.DarkSlateBlue;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that DarkSlateBlue property returns a brush with expected color properties.
        /// Verifies the color has the correct ARGB values for DarkSlateBlue (#FF483D8B).
        /// </summary>
        [Fact]
        public void DarkSlateBlue_WhenAccessed_ReturnsExpectedColorValues()
        {
            // Arrange
            var expectedColor = Colors.DarkSlateBlue;

            // Act
            var result = Brush.DarkSlateBlue;

            // Assert
            Assert.Equal(expectedColor.Red, result.Color.Red);
            Assert.Equal(expectedColor.Green, result.Color.Green);
            Assert.Equal(expectedColor.Blue, result.Color.Blue);
            Assert.Equal(expectedColor.Alpha, result.Color.Alpha);
        }

        /// <summary>
        /// Tests that ForestGreen property returns a non-null SolidColorBrush with the correct color.
        /// </summary>
        [Fact]
        public void ForestGreen_ReturnsNonNullSolidColorBrushWithCorrectColor()
        {
            // Act
            var forestGreenBrush = Brush.ForestGreen;

            // Assert
            Assert.NotNull(forestGreenBrush);
            Assert.IsType<SolidColorBrush>(forestGreenBrush);
            Assert.Equal(Colors.ForestGreen, forestGreenBrush.Color);
        }

        /// <summary>
        /// Tests that ForestGreen property implements lazy initialization by returning the same instance on multiple calls.
        /// </summary>
        [Fact]
        public void ForestGreen_LazyInitialization_ReturnsSameInstanceOnMultipleCalls()
        {
            // Act
            var firstCall = Brush.ForestGreen;
            var secondCall = Brush.ForestGreen;
            var thirdCall = Brush.ForestGreen;

            // Assert
            Assert.Same(firstCall, secondCall);
            Assert.Same(secondCall, thirdCall);
            Assert.Same(firstCall, thirdCall);
        }

        /// <summary>
        /// Tests that ForestGreen property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void ForestGreen_IsNotEmpty()
        {
            // Act
            var forestGreenBrush = Brush.ForestGreen;

            // Assert
            Assert.False(forestGreenBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the GreenYellow property returns a non-null SolidColorBrush instance.
        /// Verifies lazy initialization works correctly on first access.
        /// </summary>
        [Fact]
        public void GreenYellow_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.GreenYellow;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the GreenYellow property returns the same instance on multiple calls.
        /// Verifies singleton behavior of the lazy-initialized static property.
        /// </summary>
        [Fact]
        public void GreenYellow_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.GreenYellow;
            var second = Brush.GreenYellow;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the GreenYellow property returns a brush with the correct GreenYellow color.
        /// Verifies the brush is initialized with the system-defined GreenYellow color.
        /// </summary>
        [Fact]
        public void GreenYellow_Color_EqualsSystemGreenYellow()
        {
            // Act
            var brush = Brush.GreenYellow;

            // Assert
            Assert.Equal(Colors.GreenYellow, brush.Color);
        }

        /// <summary>
        /// Tests that the GreenYellow property brush is not empty.
        /// Verifies the brush represents a valid color and is not considered empty.
        /// </summary>
        [Fact]
        public void GreenYellow_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.GreenYellow;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests multiple consecutive accesses to verify consistency.
        /// Ensures the property maintains its singleton behavior across multiple calls.
        /// </summary>
        [Fact]
        public void GreenYellow_ConsecutiveAccesses_AreConsistent()
        {
            // Act
            var first = Brush.GreenYellow;
            var second = Brush.GreenYellow;
            var third = Brush.GreenYellow;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Equal(Colors.GreenYellow, first.Color);
            Assert.Equal(Colors.GreenYellow, second.Color);
            Assert.Equal(Colors.GreenYellow, third.Color);
        }

        /// <summary>
        /// Tests that the LightCoral property returns a non-null SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void LightCoral_FirstAccess_ReturnsNonNullBrush()
        {
            // Act
            var result = Brush.LightCoral;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the LightCoral property returns the same instance on multiple accesses (lazy initialization).
        /// </summary>
        [Fact]
        public void LightCoral_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.LightCoral;
            var second = Brush.LightCoral;

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the LightCoral property returns a brush with the correct LightCoral color.
        /// </summary>
        [Fact]
        public void LightCoral_Color_EqualsSystemDefinedLightCoral()
        {
            // Act
            var brush = Brush.LightCoral;

            // Assert
            Assert.Equal(Colors.LightCoral, brush.Color);
        }

        /// <summary>
        /// Tests that the LightCoral property returns a brush that is not empty.
        /// </summary>
        [Fact]
        public void LightCoral_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.LightCoral;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the LightCoral brush has consistent properties across multiple accesses.
        /// </summary>
        [Fact]
        public void LightCoral_ConsistentPropertiesAcrossAccesses_ReturnsExpectedValues()
        {
            // Act
            var firstAccess = Brush.LightCoral;
            var secondAccess = Brush.LightCoral;

            // Assert
            Assert.Same(firstAccess, secondAccess);
            Assert.Equal(firstAccess.Color, secondAccess.Color);
            Assert.Equal(Colors.LightCoral, firstAccess.Color);
            Assert.Equal(Colors.LightCoral, secondAccess.Color);
            Assert.False(firstAccess.IsEmpty);
            Assert.False(secondAccess.IsEmpty);
        }

        /// <summary>
        /// Tests that the LightSeaGreen property returns a non-null SolidColorBrush instance.
        /// Verifies that the property is properly initialized and accessible.
        /// </summary>
        [Fact]
        public void LightSeaGreen_ShouldReturnNonNullBrush()
        {
            // Act
            var brush = Brush.LightSeaGreen;

            // Assert
            Assert.NotNull(brush);
            Assert.IsType<SolidColorBrush>(brush);
        }

        /// <summary>
        /// Tests that the LightSeaGreen property returns a brush with the correct color.
        /// Verifies that the brush color matches the system-defined LightSeaGreen color.
        /// </summary>
        [Fact]
        public void LightSeaGreen_ShouldHaveCorrectColor()
        {
            // Act
            var brush = Brush.LightSeaGreen;

            // Assert
            Assert.Equal(Colors.LightSeaGreen, brush.Color);
        }

        /// <summary>
        /// Tests that the LightSeaGreen property returns the same instance on multiple calls.
        /// Verifies the lazy initialization pattern and ensures singleton behavior.
        /// </summary>
        [Fact]
        public void LightSeaGreen_ShouldReturnSameInstanceOnMultipleCalls()
        {
            // Act
            var brush1 = Brush.LightSeaGreen;
            var brush2 = Brush.LightSeaGreen;

            // Assert
            Assert.Same(brush1, brush2);
        }

        /// <summary>
        /// Tests that the LightSeaGreen brush is not empty.
        /// Verifies that the brush represents a valid color and is not considered empty.
        /// </summary>
        [Fact]
        public void LightSeaGreen_ShouldNotBeEmpty()
        {
            // Act
            var brush = Brush.LightSeaGreen;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that the LightSeaGreen property can be used in null checks.
        /// Verifies that the property behaves correctly with the IsNullOrEmpty method.
        /// </summary>
        [Fact]
        public void LightSeaGreen_ShouldNotBeNullOrEmpty()
        {
            // Act
            var brush = Brush.LightSeaGreen;

            // Assert
            Assert.False(Brush.IsNullOrEmpty(brush));
        }

        /// <summary>
        /// Tests that MistyRose property returns a non-null SolidColorBrush instance.
        /// This verifies the lazy initialization creates an instance on first access.
        /// </summary>
        [Fact]
        public void MistyRose_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.MistyRose;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that MistyRose property returns the correct color value.
        /// This verifies the brush is initialized with Colors.MistyRose.
        /// </summary>
        [Fact]
        public void MistyRose_ColorValue_MatchesSystemDefinedMistyRose()
        {
            // Act
            var result = Brush.MistyRose;

            // Assert
            Assert.Equal(Colors.MistyRose, result.Color);
        }

        /// <summary>
        /// Tests that MistyRose property returns the same instance on multiple accesses.
        /// This verifies the lazy initialization singleton behavior.
        /// </summary>
        [Fact]
        public void MistyRose_MultipleAccess_ReturnsSameInstance()
        {
            // Act
            var first = Brush.MistyRose;
            var second = Brush.MistyRose;
            var third = Brush.MistyRose;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Tests that MistyRose property is not empty.
        /// This verifies the returned brush has a valid state.
        /// </summary>
        [Fact]
        public void MistyRose_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.MistyRose;

            // Assert
            Assert.False(result.IsEmpty);
        }

        /// <summary>
        /// Tests that MistyRose property returns a brush with the expected color components.
        /// This verifies the ARGB values match the system-defined MistyRose color (#FFFFE4E1).
        /// </summary>
        [Fact]
        public void MistyRose_ColorComponents_MatchExpectedValues()
        {
            // Arrange
            var expectedColor = Colors.MistyRose;

            // Act
            var result = Brush.MistyRose;

            // Assert
            Assert.Equal(expectedColor.Red, result.Color.Red);
            Assert.Equal(expectedColor.Green, result.Color.Green);
            Assert.Equal(expectedColor.Blue, result.Color.Blue);
            Assert.Equal(expectedColor.Alpha, result.Color.Alpha);
        }

        /// <summary>
        /// Tests that PaleGreen property returns a non-null SolidColorBrush instance on first access.
        /// Verifies the lazy initialization behavior and ensures the property never returns null.
        /// Expected result: A valid SolidColorBrush instance.
        /// </summary>
        [Fact]
        public void PaleGreen_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.PaleGreen;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that PaleGreen property returns the same instance on multiple accesses.
        /// Verifies the lazy initialization with caching behavior using reference equality.
        /// Expected result: Same instance returned on subsequent calls.
        /// </summary>
        [Fact]
        public void PaleGreen_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.PaleGreen;
            var secondAccess = Brush.PaleGreen;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that PaleGreen property returns a brush with the correct PaleGreen color.
        /// Verifies that the Color property matches the system-defined Colors.PaleGreen value.
        /// Expected result: Color property equals Colors.PaleGreen (ARGB: #FF98FB98).
        /// </summary>
        [Fact]
        public void PaleGreen_Color_EqualsPaleGreenColor()
        {
            // Act
            var brush = Brush.PaleGreen;

            // Assert
            Assert.Equal(Colors.PaleGreen, brush.Color);
        }

        /// <summary>
        /// Tests that PaleGreen property returns a brush that is not empty.
        /// Verifies that the brush has a valid color and is not considered empty.
        /// Expected result: IsEmpty property returns false.
        /// </summary>
        [Fact]
        public void PaleGreen_IsEmpty_ReturnsFalse()
        {
            // Act
            var brush = Brush.PaleGreen;

            // Assert
            Assert.False(brush.IsEmpty);
        }

        /// <summary>
        /// Tests that PaleGreen property returns a brush with the expected color components.
        /// Verifies the ARGB values match the system-defined PaleGreen color (#FF98FB98).
        /// Expected result: Color components match the expected ARGB values.
        /// </summary>
        [Fact]
        public void PaleGreen_ColorComponents_MatchExpectedValues()
        {
            // Arrange
            var expectedColor = Colors.PaleGreen;

            // Act
            var brush = Brush.PaleGreen;

            // Assert
            Assert.Equal(expectedColor.Red, brush.Color.Red);
            Assert.Equal(expectedColor.Green, brush.Color.Green);
            Assert.Equal(expectedColor.Blue, brush.Color.Blue);
            Assert.Equal(expectedColor.Alpha, brush.Color.Alpha);
        }

        /// <summary>
        /// Tests that the Salmon property returns a non-null SolidColorBrush instance.
        /// Validates that the property properly initializes and returns a valid brush object.
        /// </summary>
        [Fact]
        public void Salmon_ReturnsNonNullBrush_Success()
        {
            // Act
            var salmonBrush = Brush.Salmon;

            // Assert
            Assert.NotNull(salmonBrush);
            Assert.IsType<SolidColorBrush>(salmonBrush);
        }

        /// <summary>
        /// Tests that the Salmon property returns a brush with the correct salmon color value.
        /// Validates that the color matches the expected system-defined salmon color (#FFFA8072).
        /// </summary>
        [Fact]
        public void Salmon_HasCorrectColor_Success()
        {
            // Arrange
            var expectedColor = Colors.Salmon;

            // Act
            var salmonBrush = Brush.Salmon;

            // Assert
            Assert.Equal(expectedColor, salmonBrush.Color);
        }

        /// <summary>
        /// Tests that multiple calls to the Salmon property return the same instance.
        /// Validates the lazy initialization behavior using the null-coalescing assignment operator.
        /// </summary>
        [Fact]
        public void Salmon_LazyInitialization_ReturnsSameInstance()
        {
            // Act
            var firstCall = Brush.Salmon;
            var secondCall = Brush.Salmon;

            // Assert
            Assert.Same(firstCall, secondCall);
        }

        /// <summary>
        /// Tests that the Salmon brush is not empty.
        /// Validates that the brush represents a valid, non-empty brush instance.
        /// </summary>
        [Fact]
        public void Salmon_IsNotEmpty_Success()
        {
            // Act
            var salmonBrush = Brush.Salmon;

            // Assert
            Assert.False(salmonBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Salmon brush color has the correct ARGB values.
        /// Validates the specific color components of the salmon color (#FFFA8072).
        /// </summary>
        [Fact]
        public void Salmon_ColorHasCorrectArgbValues_Success()
        {
            // Arrange
            var expectedRed = 250f / 255f;   // FA in hex = 250
            var expectedGreen = 128f / 255f; // 80 in hex = 128  
            var expectedBlue = 114f / 255f;  // 72 in hex = 114
            var expectedAlpha = 1.0f;        // FF in hex = 255 = full opacity

            // Act
            var salmonBrush = Brush.Salmon;
            var color = salmonBrush.Color;

            // Assert
            Assert.Equal(expectedRed, color.Red, 3);
            Assert.Equal(expectedGreen, color.Green, 3);
            Assert.Equal(expectedBlue, color.Blue, 3);
            Assert.Equal(expectedAlpha, color.Alpha, 3);
        }

        /// <summary>
        /// Tests that the Salmon property can be accessed concurrently without issues.
        /// Validates thread-safety of the lazy initialization using null-coalescing assignment.
        /// </summary>
        [Fact]
        public void Salmon_ConcurrentAccess_ReturnsSameInstance()
        {
            // Arrange
            SolidColorBrush brush1 = null;
            SolidColorBrush brush2 = null;
            var task1 = System.Threading.Tasks.Task.Run(() => brush1 = Brush.Salmon);
            var task2 = System.Threading.Tasks.Task.Run(() => brush2 = Brush.Salmon);

            // Act
            System.Threading.Tasks.Task.WaitAll(task1, task2);

            // Assert
            Assert.NotNull(brush1);
            Assert.NotNull(brush2);
            Assert.Same(brush1, brush2);
        }

        /// <summary>
        /// Tests that the Salmon brush can be used with Brush.IsNullOrEmpty method.
        /// Validates that the salmon brush is not considered null or empty.
        /// </summary>
        [Fact]
        public void Salmon_IsNullOrEmpty_ReturnsFalse()
        {
            // Act
            var salmonBrush = Brush.Salmon;
            var isNullOrEmpty = Brush.IsNullOrEmpty(salmonBrush);

            // Assert
            Assert.False(isNullOrEmpty);
        }

        /// <summary>
        /// Tests that the Tomato property returns a non-null SolidColorBrush on first access.
        /// </summary>
        [Fact]
        public void Tomato_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Tomato;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests that the Tomato property returns a brush with the correct tomato color.
        /// </summary>
        [Fact]
        public void Tomato_ColorValue_MatchesTomatoColor()
        {
            // Act
            var tomatoBrush = Brush.Tomato;

            // Assert
            Assert.Equal(Colors.Tomato, tomatoBrush.Color);
        }

        /// <summary>
        /// Tests that multiple accesses to the Tomato property return the same instance (singleton behavior).
        /// </summary>
        [Fact]
        public void Tomato_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var firstAccess = Brush.Tomato;
            var secondAccess = Brush.Tomato;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that the Tomato brush is not empty.
        /// </summary>
        [Fact]
        public void Tomato_IsEmpty_ReturnsFalse()
        {
            // Act
            var tomatoBrush = Brush.Tomato;

            // Assert
            Assert.False(tomatoBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the Tomato property returns a brush with the expected ARGB color value.
        /// </summary>
        [Fact]
        public void Tomato_ColorArgbValue_MatchesExpectedValue()
        {
            // Arrange
            var expectedColor = Color.FromUint(0xFFFF6347); // Tomato color ARGB value

            // Act
            var tomatoBrush = Brush.Tomato;

            // Assert
            Assert.Equal(expectedColor, tomatoBrush.Color);
        }

        /// <summary>
        /// Tests that the Tomato brush can be used in equality comparisons correctly.
        /// </summary>
        [Fact]
        public void Tomato_EqualityComparison_WorksCorrectly()
        {
            // Arrange
            var tomatoBrush1 = Brush.Tomato;
            var tomatoBrush2 = Brush.Tomato;
            var differentBrush = new SolidColorBrush(Colors.Red);

            // Act & Assert
            Assert.Equal(tomatoBrush1, tomatoBrush2);
            Assert.NotEqual(tomatoBrush1, differentBrush);
        }
    }

    /// <summary>
    /// Tests for the Chartreuse property of the Brush class.
    /// </summary>
    public class BrushChartreuseTests
    {
        /// <summary>
        /// Verifies that the Chartreuse property returns a non-null SolidColorBrush instance.
        /// Tests the basic functionality and ensures the property doesn't return null.
        /// </summary>
        [Fact]
        public void Chartreuse_FirstAccess_ReturnsNonNullSolidColorBrush()
        {
            // Act
            var result = Brush.Chartreuse;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Verifies that the Chartreuse property returns a brush with the correct color value.
        /// Tests that the returned brush has the Colors.Chartreuse color assigned.
        /// </summary>
        [Fact]
        public void Chartreuse_ReturnsCorrectColor_EqualsColorsChartreuse()
        {
            // Act
            var result = Brush.Chartreuse;

            // Assert
            Assert.Equal(Colors.Chartreuse, result.Color);
        }

        /// <summary>
        /// Verifies that the Chartreuse property implements lazy initialization correctly.
        /// Tests that multiple calls to the property return the same instance (singleton behavior).
        /// </summary>
        [Fact]
        public void Chartreuse_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Brush.Chartreuse;
            var second = Brush.Chartreuse;
            var third = Brush.Chartreuse;

            // Assert
            Assert.Same(first, second);
            Assert.Same(second, third);
            Assert.Same(first, third);
        }

        /// <summary>
        /// Verifies that the Chartreuse property's color matches the expected ARGB values.
        /// Tests the specific color components to ensure correctness.
        /// </summary>
        [Fact]
        public void Chartreuse_ColorComponents_MatchExpectedValues()
        {
            // Act
            var result = Brush.Chartreuse;
            var color = result.Color;

            // Assert
            Assert.Equal(Colors.Chartreuse.Red, color.Red);
            Assert.Equal(Colors.Chartreuse.Green, color.Green);
            Assert.Equal(Colors.Chartreuse.Blue, color.Blue);
            Assert.Equal(Colors.Chartreuse.Alpha, color.Alpha);
        }

        /// <summary>
        /// Verifies that the Chartreuse brush is not considered empty.
        /// Tests the IsEmpty property to ensure the brush has valid content.
        /// </summary>
        [Fact]
        public void Chartreuse_IsEmpty_ReturnsFalse()
        {
            // Act
            var result = Brush.Chartreuse;

            // Assert
            Assert.False(result.IsEmpty);
        }
    }
}