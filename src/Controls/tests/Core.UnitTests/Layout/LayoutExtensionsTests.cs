using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

public partial class LayoutExtensionsTests
{
    /// <summary>
    /// Tests that IgnoreLayoutSafeArea throws ArgumentNullException when layout parameter is null.
    /// Verifies null parameter handling for the extension method.
    /// Expected result: ArgumentNullException should be thrown.
    /// </summary>
    [Fact]
    public void IgnoreLayoutSafeArea_NullLayout_ThrowsArgumentNullException()
    {
        // Arrange
        Layout layout = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => layout.IgnoreLayoutSafeArea());
    }

    /// <summary>
    /// Tests that IgnoreLayoutSafeArea sets IgnoreSafeArea to true on a layout with no children.
    /// Verifies the basic functionality of setting the property on an empty layout.
    /// Expected result: IgnoreSafeArea property should be set to true.
    /// </summary>
    [Fact]
    public void IgnoreLayoutSafeArea_EmptyLayout_SetsIgnoreSafeAreaToTrue()
    {
        // Arrange
        var layout = Substitute.For<Layout>();
        layout.Children.Returns(new List<IView>());

        // Act
        layout.IgnoreLayoutSafeArea();

        // Assert
        layout.Received(1).IgnoreSafeArea = true;
    }

    /// <summary>
    /// Tests that IgnoreLayoutSafeArea sets IgnoreSafeArea to true on layout and recursively calls itself on child layouts.
    /// Verifies recursive behavior with nested Layout children.
    /// Expected result: IgnoreSafeArea should be set to true on both parent and child layouts.
    /// </summary>
    [Fact]
    public void IgnoreLayoutSafeArea_LayoutWithChildLayouts_SetsIgnoreSafeAreaRecursively()
    {
        // Arrange
        var childLayout1 = Substitute.For<Layout>();
        var childLayout2 = Substitute.For<Layout>();
        childLayout1.Children.Returns(new List<IView>());
        childLayout2.Children.Returns(new List<IView>());

        var parentLayout = Substitute.For<Layout>();
        parentLayout.Children.Returns(new List<IView> { childLayout1, childLayout2 });

        // Act
        parentLayout.IgnoreLayoutSafeArea();

        // Assert
        parentLayout.Received(1).IgnoreSafeArea = true;
        childLayout1.Received(1).IgnoreSafeArea = true;
        childLayout2.Received(1).IgnoreSafeArea = true;
    }

    /// <summary>
    /// Tests that IgnoreLayoutSafeArea skips non-Layout children and only processes Layout children.
    /// Verifies selective processing of children based on type.
    /// Expected result: Only Layout children should have IgnoreSafeArea set, non-Layout children should be ignored.
    /// </summary>
    [Fact]
    public void IgnoreLayoutSafeArea_MixedChildren_OnlyProcessesLayoutChildren()
    {
        // Arrange
        var nonLayoutChild1 = Substitute.For<IView>();
        var layoutChild = Substitute.For<Layout>();
        var nonLayoutChild2 = Substitute.For<IView>();
        layoutChild.Children.Returns(new List<IView>());

        var parentLayout = Substitute.For<Layout>();
        parentLayout.Children.Returns(new List<IView> { nonLayoutChild1, layoutChild, nonLayoutChild2 });

        // Act
        parentLayout.IgnoreLayoutSafeArea();

        // Assert
        parentLayout.Received(1).IgnoreSafeArea = true;
        layoutChild.Received(1).IgnoreSafeArea = true;
        // Non-layout children should not have IgnoreSafeArea property accessed
    }

    /// <summary>
    /// Tests that IgnoreLayoutSafeArea handles deeply nested layout hierarchies correctly.
    /// Verifies recursive behavior with multiple levels of nesting.
    /// Expected result: IgnoreSafeArea should be set to true on all layout levels in the hierarchy.
    /// </summary>
    [Fact]
    public void IgnoreLayoutSafeArea_DeepNesting_ProcessesAllLevels()
    {
        // Arrange
        var grandChildLayout = Substitute.For<Layout>();
        grandChildLayout.Children.Returns(new List<IView>());

        var childLayout = Substitute.For<Layout>();
        childLayout.Children.Returns(new List<IView> { grandChildLayout });

        var parentLayout = Substitute.For<Layout>();
        parentLayout.Children.Returns(new List<IView> { childLayout });

        // Act
        parentLayout.IgnoreLayoutSafeArea();

        // Assert
        parentLayout.Received(1).IgnoreSafeArea = true;
        childLayout.Received(1).IgnoreSafeArea = true;
        grandChildLayout.Received(1).IgnoreSafeArea = true;
    }

    /// <summary>
    /// Tests that IgnoreLayoutSafeArea handles complex scenarios with multiple children at different levels.
    /// Verifies comprehensive recursive processing with mixed child types and multiple nesting levels.
    /// Expected result: All Layout instances in the hierarchy should have IgnoreSafeArea set to true.
    /// </summary>
    [Fact]
    public void IgnoreLayoutSafeArea_ComplexHierarchy_ProcessesAllLayoutsCorrectly()
    {
        // Arrange
        var leafLayout1 = Substitute.For<Layout>();
        var leafLayout2 = Substitute.For<Layout>();
        leafLayout1.Children.Returns(new List<IView>());
        leafLayout2.Children.Returns(new List<IView>());

        var middleLayout1 = Substitute.For<Layout>();
        var middleLayout2 = Substitute.For<Layout>();
        var nonLayoutView = Substitute.For<IView>();

        middleLayout1.Children.Returns(new List<IView> { leafLayout1, nonLayoutView });
        middleLayout2.Children.Returns(new List<IView> { leafLayout2 });

        var rootLayout = Substitute.For<Layout>();
        rootLayout.Children.Returns(new List<IView> { middleLayout1, nonLayoutView, middleLayout2 });

        // Act
        rootLayout.IgnoreLayoutSafeArea();

        // Assert
        rootLayout.Received(1).IgnoreSafeArea = true;
        middleLayout1.Received(1).IgnoreSafeArea = true;
        middleLayout2.Received(1).IgnoreSafeArea = true;
        leafLayout1.Received(1).IgnoreSafeArea = true;
        leafLayout2.Received(1).IgnoreSafeArea = true;
    }

    /// <summary>
    /// Tests that IgnoreLayoutSafeArea handles layouts where the Children collection is empty.
    /// Verifies that iteration over an empty collection is handled correctly.
    /// Expected result: IgnoreSafeArea should be set to true on the layout, no children to process.
    /// </summary>
    [Fact]
    public void IgnoreLayoutSafeArea_EmptyChildrenCollection_SetsIgnoreSafeAreaOnlyOnParent()
    {
        // Arrange
        var layout = Substitute.For<Layout>();
        layout.Children.Returns(new List<IView>());

        // Act
        layout.IgnoreLayoutSafeArea();

        // Assert
        layout.Received(1).IgnoreSafeArea = true;
    }
}
