#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class SwitchCellTemplateTests : BaseTestFixture
    {
        [Fact]
        public void Create()
        {
            var template = new DataTemplate(typeof(SwitchCell));
            var content = template.CreateContent();

            Assert.IsType<SwitchCell>(content);
        }

        [Fact]
        public void Text()
        {
            var template = new DataTemplate(typeof(SwitchCell));
            template.SetValue(SwitchCell.TextProperty, "text");

            SwitchCell cell = (SwitchCell)template.CreateContent();
            Assert.Equal("text", cell.Text);
        }

        [Fact]
        public void On()
        {
            var template = new DataTemplate(typeof(SwitchCell));
            template.SetValue(SwitchCell.OnProperty, true);

            SwitchCell cell = (SwitchCell)template.CreateContent();
            Assert.True(cell.On);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void SwitchCellSwitchChangedArgs(bool initialValue, bool finalValue)
        {
            var template = new DataTemplate(typeof(SwitchCell));
            SwitchCell cell = (SwitchCell)template.CreateContent();

            SwitchCell switchCellFromSender = null;
            bool newSwitchValue = false;

            cell.On = initialValue;

            cell.OnChanged += (s, e) =>
            {
                switchCellFromSender = (SwitchCell)s;
                newSwitchValue = e.Value;
            };

            cell.On = finalValue;

            Assert.Equal(cell, switchCellFromSender);
            Assert.Equal(finalValue, newSwitchValue);
        }
    }

    public partial class SwitchCellTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the OnColor property returns the default value (null) when not explicitly set.
        /// This verifies the getter implementation and default bindable property value.
        /// </summary>
        [Fact]
        public void OnColor_DefaultValue_ReturnsNull()
        {
            // Arrange
            var template = new DataTemplate(typeof(SwitchCell));

            // Act
            SwitchCell cell = (SwitchCell)template.CreateContent();

            // Assert
            Assert.Null(cell.OnColor);
        }

        /// <summary>
        /// Tests that the OnColor property correctly stores and retrieves a valid Color value.
        /// This verifies both the setter and getter implementation with a non-null Color.
        /// </summary>
        [Fact]
        public void OnColor_SetValidColor_ReturnsCorrectColor()
        {
            // Arrange
            var template = new DataTemplate(typeof(SwitchCell));
            var expectedColor = Color.FromRgb(255, 0, 0); // Red color
            template.SetValue(SwitchCell.OnColorProperty, expectedColor);

            // Act
            SwitchCell cell = (SwitchCell)template.CreateContent();

            // Assert
            Assert.Equal(expectedColor, cell.OnColor);
        }

        /// <summary>
        /// Tests that the OnColor property correctly handles null values.
        /// This verifies that null can be explicitly set and retrieved.
        /// </summary>
        [Fact]
        public void OnColor_SetNull_ReturnsNull()
        {
            // Arrange
            var template = new DataTemplate(typeof(SwitchCell));
            template.SetValue(SwitchCell.OnColorProperty, null);

            // Act
            SwitchCell cell = (SwitchCell)template.CreateContent();

            // Assert
            Assert.Null(cell.OnColor);
        }

        /// <summary>
        /// Tests that the OnColor property works correctly with various Color representations.
        /// This verifies the property handles different ways of creating Color instances.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0)] // Black
        [InlineData(255, 255, 255)] // White
        [InlineData(255, 0, 0)] // Red
        [InlineData(0, 255, 0)] // Green
        [InlineData(0, 0, 255)] // Blue
        public void OnColor_SetDifferentColors_ReturnsCorrectColor(byte red, byte green, byte blue)
        {
            // Arrange
            var template = new DataTemplate(typeof(SwitchCell));
            var expectedColor = Color.FromRgb(red, green, blue);
            template.SetValue(SwitchCell.OnColorProperty, expectedColor);

            // Act
            SwitchCell cell = (SwitchCell)template.CreateContent();

            // Assert
            Assert.Equal(expectedColor, cell.OnColor);
        }
    }
}