#nullable disable

using System;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public class ToolbarItemTests
    : MenuItemTests
{
    /// <summary>
    /// Tests that the Order property can be set and retrieved correctly for all valid ToolbarItemOrder values.
    /// </summary>
    /// <param name="expectedOrder">The ToolbarItemOrder value to test</param>
    [Theory]
    [InlineData(ToolbarItemOrder.Default)]
    [InlineData(ToolbarItemOrder.Primary)]
    [InlineData(ToolbarItemOrder.Secondary)]
    public void Order_SetValidValue_ReturnsExpectedValue(ToolbarItemOrder expectedOrder)
    {
        // Arrange
        var toolbarItem = new ToolbarItem();

        // Act
        toolbarItem.Order = expectedOrder;

        // Assert
        Assert.Equal(expectedOrder, toolbarItem.Order);
    }

    /// <summary>
    /// Tests that the Order property has the correct default value when a ToolbarItem is created.
    /// </summary>
    [Fact]
    public void Order_DefaultValue_ReturnsDefault()
    {
        // Arrange & Act
        var toolbarItem = new ToolbarItem();

        // Assert
        Assert.Equal(ToolbarItemOrder.Default, toolbarItem.Order);
    }

    /// <summary>
    /// Tests that setting an invalid ToolbarItemOrder value throws an ArgumentException due to validation.
    /// This tests the validation logic in the OrderProperty definition.
    /// </summary>
    /// <param name="invalidOrderValue">An invalid integer value cast to ToolbarItemOrder</param>
    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(99)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Order_SetInvalidValue_ThrowsArgumentException(int invalidOrderValue)
    {
        // Arrange
        var toolbarItem = new ToolbarItem();
        var invalidOrder = (ToolbarItemOrder)invalidOrderValue;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => toolbarItem.Order = invalidOrder);
    }

    /// <summary>
    /// Tests that the Order property maintains its value when set multiple times with different valid values.
    /// </summary>
    [Fact]
    public void Order_SetMultipleValidValues_MaintainsLastSetValue()
    {
        // Arrange
        var toolbarItem = new ToolbarItem();

        // Act & Assert - Set to Primary
        toolbarItem.Order = ToolbarItemOrder.Primary;
        Assert.Equal(ToolbarItemOrder.Primary, toolbarItem.Order);

        // Act & Assert - Set to Secondary
        toolbarItem.Order = ToolbarItemOrder.Secondary;
        Assert.Equal(ToolbarItemOrder.Secondary, toolbarItem.Order);

        // Act & Assert - Set back to Default
        toolbarItem.Order = ToolbarItemOrder.Default;
        Assert.Equal(ToolbarItemOrder.Default, toolbarItem.Order);
    }

    /// <summary>
    /// Tests that the ToolbarItem constructor throws ArgumentNullException when activated parameter is null.
    /// This test validates the null check for the required activated parameter.
    /// Expected result: ArgumentNullException with correct parameter name.
    /// </summary>
    [Fact]
    public void Constructor_ActivatedIsNull_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ToolbarItem("name", "icon", null));

        Assert.Equal("activated", exception.ParamName);
    }

    /// <summary>
    /// Tests the ToolbarItem constructor with valid parameters using default optional values.
    /// Validates that all properties are correctly assigned when using minimum required parameters.
    /// Expected result: ToolbarItem created with correct property values and default order/priority.
    /// </summary>
    [Fact]
    public void Constructor_ValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        var name = "Test Item";
        var icon = "test_icon.png";
        var actionCalled = false;
        Action activated = () => actionCalled = true;

        // Act
        var toolbarItem = new ToolbarItem(name, icon, activated);

        // Assert
        Assert.Equal(name, toolbarItem.Text);
        Assert.Equal(icon, toolbarItem.IconImageSource.ToString());
        Assert.Equal(ToolbarItemOrder.Default, toolbarItem.Order);
        Assert.Equal(0, toolbarItem.Priority);
    }

    /// <summary>
    /// Tests the ToolbarItem constructor with all parameters specified including order and priority.
    /// Validates that optional parameters are correctly assigned when explicitly provided.
    /// Expected result: ToolbarItem created with all specified property values.
    /// </summary>
    [Theory]
    [InlineData(ToolbarItemOrder.Default, 0)]
    [InlineData(ToolbarItemOrder.Primary, 1)]
    [InlineData(ToolbarItemOrder.Secondary, -1)]
    [InlineData(ToolbarItemOrder.Primary, int.MaxValue)]
    [InlineData(ToolbarItemOrder.Secondary, int.MinValue)]
    public void Constructor_AllParameters_SetsPropertiesCorrectly(ToolbarItemOrder order, int priority)
    {
        // Arrange
        var name = "Test Item";
        var icon = "test_icon.png";
        var actionCalled = false;
        Action activated = () => actionCalled = true;

        // Act
        var toolbarItem = new ToolbarItem(name, icon, activated, order, priority);

        // Assert
        Assert.Equal(name, toolbarItem.Text);
        Assert.Equal(icon, toolbarItem.IconImageSource.ToString());
        Assert.Equal(order, toolbarItem.Order);
        Assert.Equal(priority, toolbarItem.Priority);
    }

    /// <summary>
    /// Tests the ToolbarItem constructor with various string parameter edge cases.
    /// Validates handling of null, empty, and whitespace string values for name and icon parameters.
    /// Expected result: ToolbarItem created with correct string property values without throwing exceptions.
    /// </summary>
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    [InlineData("Valid Name", null)]
    [InlineData(null, "valid_icon.png")]
    [InlineData("", "valid_icon.png")]
    [InlineData("Valid Name", "")]
    public void Constructor_StringParameterEdgeCases_HandlesCorrectly(string name, string icon)
    {
        // Arrange
        var actionCalled = false;
        Action activated = () => actionCalled = true;

        // Act
        var toolbarItem = new ToolbarItem(name, icon, activated);

        // Assert
        Assert.Equal(name, toolbarItem.Text);
        if (icon == null)
        {
            Assert.Null(toolbarItem.IconImageSource);
        }
        else
        {
            Assert.Equal(icon, toolbarItem.IconImageSource.ToString());
        }
    }

    /// <summary>
    /// Tests that the activated action is properly wired to the Clicked event.
    /// Validates that when Clicked event is raised, the activated action is executed.
    /// Expected result: The activated action is invoked when Clicked event is triggered.
    /// </summary>
    [Fact]
    public void Constructor_ActivatedAction_WiredToClickedEvent()
    {
        // Arrange
        var actionCalled = false;
        Action activated = () => actionCalled = true;
        var toolbarItem = new ToolbarItem("name", "icon", activated);

        // Act
        toolbarItem.SendClicked();

        // Assert
        Assert.True(actionCalled);
    }

    /// <summary>
    /// Tests the ToolbarItem constructor with very long string parameters.
    /// Validates handling of extremely long string values that could cause memory or performance issues.
    /// Expected result: ToolbarItem created successfully with long string values.
    /// </summary>
    [Fact]
    public void Constructor_VeryLongStrings_HandlesCorrectly()
    {
        // Arrange
        var longName = new string('A', 10000);
        var longIcon = new string('B', 10000);
        var actionCalled = false;
        Action activated = () => actionCalled = true;

        // Act
        var toolbarItem = new ToolbarItem(longName, longIcon, activated);

        // Assert
        Assert.Equal(longName, toolbarItem.Text);
        Assert.Equal(longIcon, toolbarItem.IconImageSource.ToString());
    }

    /// <summary>
    /// Tests the ToolbarItem constructor with special characters and control characters in string parameters.
    /// Validates handling of strings containing special characters, unicode, and control characters.
    /// Expected result: ToolbarItem created successfully with special character strings.
    /// </summary>
    [Theory]
    [InlineData("Test\nName", "test\ticon")]
    [InlineData("Test\r\nName", "test\0icon")]
    [InlineData("Test🎉Name", "test🔧icon")]
    [InlineData("Test\"Name", "test'icon")]
    [InlineData("Test\\Name", "test/icon")]
    public void Constructor_StringsWithSpecialCharacters_HandlesCorrectly(string name, string icon)
    {
        // Arrange
        var actionCalled = false;
        Action activated = () => actionCalled = true;

        // Act
        var toolbarItem = new ToolbarItem(name, icon, activated);

        // Assert
        Assert.Equal(name, toolbarItem.Text);
        Assert.Equal(icon, toolbarItem.IconImageSource.ToString());
    }

}