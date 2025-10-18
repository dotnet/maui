#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class RelativeLayoutTests : BaseTestFixture
    {
        public RelativeLayoutTests()
        {
            ExpressionSearch.Default = new UnitExpressionSearch();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ExpressionSearch.Default = new UnitExpressionSearch();
            }

            base.Dispose(disposing);
        }

        class UnitExpressionSearch : ExpressionVisitor, IExpressionSearch
        {
            List<object> results;
            Type targeType;
            public List<T> FindObjects<T>(Expression expression) where T : class
            {
                results = new List<object>();
                targeType = typeof(T);
                Visit(expression);
                return results.Select(o => o as T).ToList();
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression is ConstantExpression && node.Member is FieldInfo)
                {
                    var container = ((ConstantExpression)node.Expression).Value;
                    var value = ((FieldInfo)node.Member).GetValue(container);

                    if (targeType.IsInstanceOfType(value))
                    {
                        results.Add(value);
                    }
                }
                return base.VisitMember(node);
            }
        }

        [Fact]
        public void SimpleLayout()
        {
            var relativeLayout = new Compatibility.RelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child,
                                Constraint.Constant(30),
                                Constraint.Constant(20),
                                Constraint.RelativeToParent(parent => parent.Height / 2),
                                Constraint.RelativeToParent(parent => parent.Height / 4));

            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            Assert.Equal(new Rect(30, 20, 50, 25), child.Bounds);
        }

        [Fact]
        public void LayoutIsUpdatedWhenConstraintsChange()
        {
            var relativeLayout = new Compatibility.RelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child,
                                Constraint.Constant(30),
                                Constraint.Constant(20),
                                Constraint.RelativeToParent(parent => parent.Height / 2),
                                Constraint.RelativeToParent(parent => parent.Height / 4));

            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            Assert.Equal(new Rect(30, 20, 50, 25), child.Bounds);

            RelativeLayout.SetXConstraint(child, Constraint.Constant(40));

            Assert.Equal(new Rect(40, 20, 50, 25), child.Bounds);

            RelativeLayout.SetYConstraint(child, Constraint.Constant(10));

            Assert.Equal(new Rect(40, 10, 50, 25), child.Bounds);

            RelativeLayout.SetWidthConstraint(child, Constraint.RelativeToParent(parent => parent.Height / 4));

            Assert.Equal(new Rect(40, 10, 25, 25), child.Bounds);

            RelativeLayout.SetHeightConstraint(child, Constraint.RelativeToParent(parent => parent.Height / 2));

            Assert.Equal(new Rect(40, 10, 25, 50), child.Bounds);
        }

        [Fact]
        //https://github.com/xamarin/Microsoft.Maui.Controls/issues/2169
        public void BoundsUpdatedIfConstraintsChangedWhileNotParented()
        {
            var relativeLayout = new Compatibility.RelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child, Constraint.Constant(30), Constraint.Constant(20));
            relativeLayout.Layout(new Rect(0, 0, 100, 100));
            Assert.Equal(child.Bounds, new Rect(30, 20, 100, 20));

            relativeLayout.Children.Remove(child);
            relativeLayout.Children.Add(child, Constraint.Constant(50), Constraint.Constant(40));
            Assert.Equal(child.Bounds, new Rect(50, 40, 100, 20));
        }

        [Fact]
        //https://github.com/dotnet/maui/issues/24897
        public void RelativeLayoutContentShouldBeAppeared()
        {
            const int radius = 20;

            var relativeLayout = new RelativeLayout()
            {
                IsPlatformEnabled = true,
            };

            var label = new Label()
            {
                IsPlatformEnabled = true,
                Text = "Hello, World!",
                VerticalTextAlignment = TextAlignment.Center,
            };

            var shape = new RoundRectangle()
            {
                CornerRadius = new CornerRadius(radius),
            };

            var border = new Border()
            {
                IsPlatformEnabled = true,
                StrokeShape = shape,
                Content = label,
            };


            relativeLayout.Children.Add(border, Constraint.Constant(20), Constraint.Constant(20), Constraint.RelativeToParent(parent => parent.Height),
                                Constraint.RelativeToParent(parent => parent.Height));

            relativeLayout.Layout(new Rect(0, 0, 300, 300));
            Assert.Equal(new Rect(20, 20, 300, 300), border.Bounds);
        }

        [Fact]
        public void SimpleExpressionLayout()
        {
            var relativeLayout = new Compatibility.RelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child,
                                () => 30,
                                () => 20,
                                () => relativeLayout.Height / 2,
                                () => relativeLayout.Height / 4);

            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            Assert.Equal(new Rect(30, 20, 50, 25), child.Bounds);
        }

        [Fact]
        public void SimpleBoundsSizing()
        {
            var relativeLayout = new Compatibility.RelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child, () => new Rect(30, 20, relativeLayout.Height / 2, relativeLayout.Height / 4));

            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            Assert.Equal(new Rect(30, 20, 50, 25), child.Bounds);
        }

        [Fact]
        public void UnconstrainedSize()
        {
            var relativeLayout = new Compatibility.RelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>(width: 25, height: 50);

            relativeLayout.Children.Add(child, Constraint.Constant(30), Constraint.Constant(20));

            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            Assert.Equal(new Rect(30, 20, 25, 50), child.Bounds);
        }

        [Fact]
        public void ViewRelativeLayout()
        {
            var relativeLayout = new Compatibility.RelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child1 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child1,
                                Constraint.Constant(30),
                                Constraint.Constant(20),
                                Constraint.RelativeToParent(parent => parent.Height / 5),
                                Constraint.RelativeToParent(parent => parent.Height / 10));

            var child2 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child2,
                                Constraint.RelativeToView(child1, (layout, view) => view.Bounds.Right + 10),
                                Constraint.RelativeToView(child1, (layout, view) => view.Y),
                                Constraint.RelativeToView(child1, (layout, view) => view.Width),
                                Constraint.RelativeToView(child1, (layout, view) => view.Height));

            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            Assert.Equal(new Rect(30, 20, 20, 10), child1.Bounds);
            Assert.Equal(new Rect(60, 20, 20, 10), child2.Bounds);
        }

        [Fact]
        public void ViewRelativeLayoutWithExpressions()
        {
            var relativeLayout = new Compatibility.RelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child1 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child1,
                                () => 30,
                                () => 20,
                                () => relativeLayout.Height / 5,
                                () => relativeLayout.Height / 10);

            var child2 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child2,
                                () => child1.Bounds.Right + 10,
                                () => child1.Y,
                                () => child1.Width,
                                () => child1.Height);

            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            Assert.Equal(new Rect(30, 20, 20, 10), child1.Bounds);
            Assert.Equal(new Rect(60, 20, 20, 10), child2.Bounds);
        }

        [Fact]
        public void ViewRelativeToMultipleViews()
        {
            var relativeLayout = new Compatibility.RelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child1 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child1,
                                Constraint.Constant(30),
                                Constraint.Constant(20),
                                Constraint.RelativeToParent(parent => parent.Height / 5),
                                Constraint.RelativeToParent(parent => parent.Height / 10));

            var child2 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child2,
                                Constraint.Constant(30),
                                Constraint.Constant(50),
                                Constraint.RelativeToParent(parent => parent.Height / 4),
                                Constraint.RelativeToParent(parent => parent.Height / 5));

            var child3 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child3,
                                Constraint.RelativeToView(child1, (layout, view) => view.Bounds.Right + 10),
                                Constraint.RelativeToView(child2, (layout, view) => view.Y),
                                Constraint.RelativeToView(child1, (layout, view) => view.Width),
                                Constraint.RelativeToView(child2, (layout, view) => view.Height * 2));

            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            Assert.Equal(new Rect(30, 20, 20, 10), child1.Bounds);
            Assert.Equal(new Rect(30, 50, 25, 20), child2.Bounds);
            Assert.Equal(new Rect(60, 50, 20, 40), child3.Bounds);
        }

        [Fact]
        public void ExpressionRelativeToMultipleViews()
        {
            var relativeLayout = new Compatibility.RelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child1 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child1,
                                Constraint.Constant(30),
                                Constraint.Constant(20),
                                Constraint.RelativeToParent(parent => parent.Height / 5),
                                Constraint.RelativeToParent(parent => parent.Height / 10));

            var child2 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child2,
                                Constraint.Constant(30),
                                Constraint.Constant(50),
                                Constraint.RelativeToParent(parent => parent.Height / 4),
                                Constraint.RelativeToParent(parent => parent.Height / 5));

            var child3 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child3,
                                () => child1.Bounds.Right + 10,
                                () => child1.Y,
                                () => child1.Width + child2.Width,
                                () => child1.Height * 2 + child2.Height);

            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            Assert.Equal(new Rect(30, 20, 20, 10), child1.Bounds);
            Assert.Equal(new Rect(30, 50, 25, 20), child2.Bounds);
            Assert.Equal(new Rect(60, 20, 45, 40), child3.Bounds);
        }

        [Fact]
        public void ThreePassLayout()
        {
            var relativeLayout = new Compatibility.RelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child1 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child1,
                                Constraint.Constant(30),
                                Constraint.Constant(20),
                                Constraint.RelativeToParent(parent => parent.Height / 5),
                                Constraint.RelativeToParent(parent => parent.Height / 10));

            var child2 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child2,
                                Constraint.Constant(30),
                                Constraint.Constant(50),
                                Constraint.RelativeToParent(parent => parent.Height / 4),
                                Constraint.RelativeToParent(parent => parent.Height / 5));

            var child3 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child3,
                                Constraint.RelativeToView(child1, (layout, view) => view.Bounds.Right + 10),
                                Constraint.RelativeToView(child2, (layout, view) => view.Y),
                                Constraint.RelativeToView(child1, (layout, view) => view.Width),
                                Constraint.RelativeToView(child2, (layout, view) => view.Height * 2));

            var child4 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child4,
                                Constraint.RelativeToView(child1, (layout, view) => view.Bounds.Right + 10),
                                Constraint.RelativeToView(child2, (layout, view) => view.Y),
                                Constraint.RelativeToView(child1, (layout, view) => view.Width),
                                Constraint.RelativeToView(child3, (layout, view) => view.Height * 2));

            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            Assert.Equal(new Rect(30, 20, 20, 10), child1.Bounds);
            Assert.Equal(new Rect(30, 50, 25, 20), child2.Bounds);
            Assert.Equal(new Rect(60, 50, 20, 40), child3.Bounds);
            Assert.Equal(new Rect(60, 50, 20, 80), child4.Bounds);
        }

        [Fact]
        public void ThreePassLayoutWithExpressions()
        {
            var relativeLayout = new Compatibility.RelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child1 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child1,
                                x: () => 30,
                                y: () => 20,
                                width: () => relativeLayout.Height / 5,
                                height: () => relativeLayout.Height / 10);

            var child2 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child2,
                                x: () => 30,
                                y: () => 50,
                                width: () => relativeLayout.Height / 4,
                                height: () => relativeLayout.Height / 5);

            var child3 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child3,
                                x: () => child1.Bounds.Right + 10,
                                y: () => child2.Y,
                                width: () => child1.Width,
                                height: () => child2.Height * 2);

            var child4 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child4,
                                x: () => child1.Bounds.Right + 10,
                                y: () => child2.Y,
                                width: () => child1.Width,
                                height: () => child3.Height * 2);

            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            Assert.Equal(new Rect(30, 20, 20, 10), child1.Bounds);
            Assert.Equal(new Rect(30, 50, 25, 20), child2.Bounds);
            Assert.Equal(new Rect(60, 50, 20, 40), child3.Bounds);
            Assert.Equal(new Rect(60, 50, 20, 80), child4.Bounds);
        }

        [Fact]
        public void ThrowsWithUnsolvableConstraints()
        {
            var relativeLayout = new Compatibility.RelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child1 = MockPlatformSizeService.Sub<View>();

            var child2 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child1,
                                () => 30,
                                () => 20,
                                () => child2.Height / 5,
                                () => relativeLayout.Height / 10);

            relativeLayout.Children.Add(child2,
                                () => child1.Bounds.Right + 10,
                                () => child1.Y,
                                () => child1.Width,
                                () => child1.Height);

            Assert.Throws<UnsolvableConstraintsException>(() => relativeLayout.Layout(new Rect(0, 0, 100, 100)));
        }

        [Fact]
        public void ChildAddedBeforeLayoutChildrenAfterInitialLayout()
        {
            var relativeLayout = new MockRelativeLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            var child1 = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(child,
                Constraint.Constant(30),
                Constraint.Constant(20),
                Constraint.RelativeToParent(parent => parent.Height / 2),
                Constraint.RelativeToParent(parent => parent.Height / 4));


            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            Assert.True(relativeLayout.childAdded);
            Assert.True(relativeLayout.added);
            Assert.True(relativeLayout.layoutChildren);

            relativeLayout.layoutChildren = relativeLayout.added = relativeLayout.childAdded = false;

            Assert.False(relativeLayout.childAdded);
            Assert.False(relativeLayout.added);
            Assert.False(relativeLayout.layoutChildren);

            relativeLayout.Children.Add(child1,
                Constraint.Constant(30),
                Constraint.Constant(20),
                Constraint.RelativeToParent(parent => parent.Height / 2),
                Constraint.RelativeToParent(parent => parent.Height / 4));

            Assert.True(relativeLayout.childAdded);
            Assert.True(relativeLayout.added);
            Assert.True(relativeLayout.layoutChildren);

        }
    }


    internal class MockRelativeLayout : RelativeLayout
    {
        internal bool layoutChildren;
        internal bool childAdded;
        internal bool added;

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            if (added)
                layoutChildren = true;

            base.LayoutChildren(x, y, width, height);
        }

        protected override void OnAdded(View view)
        {
            if (childAdded)
                added = true;
            base.OnAdded(view);
        }

        protected override void OnChildAdded(Element child)
        {
            childAdded = true;
            base.OnChildAdded(child);
        }
    }

    /// <summary>
    /// Unit tests for RelativeLayout.On method.
    /// </summary>
    public partial class RelativeLayoutOnTests
    {
        /// <summary>
        /// Test platform that implements IConfigPlatform for testing purposes.
        /// </summary>
        private class TestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Another test platform that implements IConfigPlatform for testing purposes.
        /// </summary>
        private class AnotherTestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests that On method returns platform configuration for valid IConfigPlatform type.
        /// Verifies that the method properly delegates to the platform configuration registry
        /// and returns the expected IPlatformElementConfiguration instance.
        /// </summary>
        [Fact]
        public void On_ValidIConfigPlatformType_ReturnsPlatformConfiguration()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();

            // Act
            var result = relativeLayout.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, RelativeLayout>>(result);
        }

        /// <summary>
        /// Tests that On method returns different configurations for different platform types.
        /// Verifies that the method properly distinguishes between different IConfigPlatform implementations
        /// and returns type-specific platform configurations.
        /// </summary>
        [Fact]
        public void On_DifferentPlatformTypes_ReturnsDifferentConfigurations()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();

            // Act
            var testPlatformResult = relativeLayout.On<TestPlatform>();
            var anotherPlatformResult = relativeLayout.On<AnotherTestPlatform>();

            // Assert
            Assert.NotNull(testPlatformResult);
            Assert.NotNull(anotherPlatformResult);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, RelativeLayout>>(testPlatformResult);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<AnotherTestPlatform, RelativeLayout>>(anotherPlatformResult);
        }

        /// <summary>
        /// Tests that On method returns same configuration instance when called multiple times with same platform type.
        /// Verifies that the platform configuration registry properly caches configurations
        /// and returns the same instance for subsequent calls with the same type parameter.
        /// </summary>
        [Fact]
        public void On_SamePlatformTypeCalledMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();

            // Act
            var firstCall = relativeLayout.On<TestPlatform>();
            var secondCall = relativeLayout.On<TestPlatform>();

            // Assert
            Assert.NotNull(firstCall);
            Assert.NotNull(secondCall);
            Assert.Same(firstCall, secondCall);
        }

        /// <summary>
        /// Tests that On method works correctly when called on newly instantiated RelativeLayout.
        /// Verifies that the lazy initialization of the platform configuration registry
        /// works properly and the method can be called immediately after construction.
        /// </summary>
        [Fact]
        public void On_NewRelativeLayoutInstance_InitializesAndReturnsConfiguration()
        {
            // Arrange & Act
            var relativeLayout = new RelativeLayout();
            var result = relativeLayout.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, RelativeLayout>>(result);
        }
    }


    public partial class RelativeLayoutIRelativeListAddTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that Add method with bounds expression throws ArgumentNullException when bounds parameter is null.
        /// Verifies proper null parameter validation.
        /// </summary>
        [Fact]
        public void Add_NullBoundsExpression_ThrowsArgumentNullException()
        {
            // Arrange
            var relativeLayout = new RelativeLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();
            Expression<Func<Rect>> nullBounds = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                relativeLayout.Children.Add(view, nullBounds));

            Assert.Equal("bounds", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method successfully adds a view with a simple bounds expression.
        /// Verifies basic functionality and proper bounds constraint application.
        /// </summary>
        [Fact]
        public void Add_ValidViewAndBoundsExpression_AddsViewAndSetsBoundsConstraint()
        {
            // Arrange
            var relativeLayout = new RelativeLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();
            var expectedRect = new Rect(10, 20, 100, 200);

            // Act
            relativeLayout.Children.Add(view, () => expectedRect);

            // Assert
            Assert.Contains(view, relativeLayout.Children);
            Assert.NotNull(RelativeLayout.GetBoundsConstraint(view));
        }

        /// <summary>
        /// Tests that Add method works with bounds expression referencing parent layout dimensions.
        /// Verifies dynamic bounds calculation based on parent size.
        /// </summary>
        [Fact]
        public void Add_BoundsExpressionWithParentReference_CalculatesBoundsCorrectly()
        {
            // Arrange
            var relativeLayout = new RelativeLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();

            // Act
            relativeLayout.Children.Add(view, () => new Rect(0, 0, relativeLayout.Width / 2, relativeLayout.Height / 2));
            relativeLayout.Layout(new Rect(0, 0, 400, 300));

            // Assert
            Assert.Equal(new Rect(0, 0, 200, 150), view.Bounds);
        }

        /// <summary>
        /// Tests that Add method handles bounds expression returning Rect.Zero.
        /// Verifies behavior with zero-sized bounds.
        /// </summary>
        [Fact]
        public void Add_BoundsExpressionReturningRectZero_SetsZeroBounds()
        {
            // Arrange
            var relativeLayout = new RelativeLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();

            // Act
            relativeLayout.Children.Add(view, () => Rect.Zero);
            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            // Assert
            Assert.Equal(Rect.Zero, view.Bounds);
        }

        /// <summary>
        /// Tests that Add method handles bounds expression with negative dimensions.
        /// Verifies behavior with invalid rectangle dimensions.
        /// </summary>
        [Fact]
        public void Add_BoundsExpressionWithNegativeDimensions_AppliesBounds()
        {
            // Arrange
            var relativeLayout = new RelativeLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();
            var negativeRect = new Rect(10, 10, -50, -30);

            // Act
            relativeLayout.Children.Add(view, () => negativeRect);
            relativeLayout.Layout(new Rect(0, 0, 200, 200));

            // Assert
            Assert.Equal(negativeRect, view.Bounds);
        }

        /// <summary>
        /// Tests that Add method handles bounds expression with very large dimensions.
        /// Verifies behavior with extreme coordinate and dimension values.
        /// </summary>
        [Fact]
        public void Add_BoundsExpressionWithLargeDimensions_AppliesBounds()
        {
            // Arrange
            var relativeLayout = new RelativeLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();
            var largeRect = new Rect(double.MaxValue / 2, double.MaxValue / 2, double.MaxValue / 4, double.MaxValue / 4);

            // Act
            relativeLayout.Children.Add(view, () => largeRect);
            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            // Assert
            Assert.Equal(largeRect, view.Bounds);
        }

        /// <summary>
        /// Tests that Add method handles bounds expression with NaN and infinity values.
        /// Verifies behavior with special floating-point values in bounds.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0, 100, 100)]
        [InlineData(0, double.NaN, 100, 100)]
        [InlineData(0, 0, double.NaN, 100)]
        [InlineData(0, 0, 100, double.NaN)]
        [InlineData(double.PositiveInfinity, 0, 100, 100)]
        [InlineData(0, double.PositiveInfinity, 100, 100)]
        [InlineData(0, 0, double.PositiveInfinity, 100)]
        [InlineData(0, 0, 100, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0, 100, 100)]
        [InlineData(0, double.NegativeInfinity, 100, 100)]
        [InlineData(0, 0, double.NegativeInfinity, 100)]
        [InlineData(0, 0, 100, double.NegativeInfinity)]
        public void Add_BoundsExpressionWithSpecialFloatingPointValues_AppliesBounds(double x, double y, double width, double height)
        {
            // Arrange
            var relativeLayout = new RelativeLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();
            var specialRect = new Rect(x, y, width, height);

            // Act
            relativeLayout.Children.Add(view, () => specialRect);
            relativeLayout.Layout(new Rect(0, 0, 200, 200));

            // Assert
            Assert.Equal(specialRect, view.Bounds);
        }

        /// <summary>
        /// Tests that Add method can add multiple views with different bounds expressions.
        /// Verifies that multiple views can be managed independently.
        /// </summary>
        [Fact]
        public void Add_MultipleViewsWithDifferentBoundsExpressions_AddsAllViewsCorrectly()
        {
            // Arrange
            var relativeLayout = new RelativeLayout { IsPlatformEnabled = true };
            var view1 = MockPlatformSizeService.Sub<View>();
            var view2 = MockPlatformSizeService.Sub<View>();
            var view3 = MockPlatformSizeService.Sub<View>();

            // Act
            relativeLayout.Children.Add(view1, () => new Rect(0, 0, 50, 50));
            relativeLayout.Children.Add(view2, () => new Rect(60, 60, relativeLayout.Width / 4, relativeLayout.Height / 4));
            relativeLayout.Children.Add(view3, () => new Rect(100, 100, 200, 150));
            relativeLayout.Layout(new Rect(0, 0, 400, 300));

            // Assert
            Assert.Equal(3, relativeLayout.Children.Count);
            Assert.Contains(view1, relativeLayout.Children);
            Assert.Contains(view2, relativeLayout.Children);
            Assert.Contains(view3, relativeLayout.Children);
            Assert.Equal(new Rect(0, 0, 50, 50), view1.Bounds);
            Assert.Equal(new Rect(60, 60, 100, 75), view2.Bounds);
            Assert.Equal(new Rect(100, 100, 200, 150), view3.Bounds);
        }

        /// <summary>
        /// Tests that Add method with bounds expression properly integrates with layout operations.
        /// Verifies that bounds constraints are evaluated during layout passes.
        /// </summary>
        [Fact]
        public void Add_BoundsExpressionIntegrationWithLayout_UpdatesBoundsOnLayoutChange()
        {
            // Arrange
            var relativeLayout = new RelativeLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();

            relativeLayout.Children.Add(view, () => new Rect(10, 10, relativeLayout.Width - 20, relativeLayout.Height - 20));

            // Act & Assert - Initial layout
            relativeLayout.Layout(new Rect(0, 0, 200, 100));
            Assert.Equal(new Rect(10, 10, 180, 80), view.Bounds);

            // Act & Assert - Changed layout
            relativeLayout.Layout(new Rect(0, 0, 300, 200));
            Assert.Equal(new Rect(10, 10, 280, 180), view.Bounds);
        }

        /// <summary>
        /// Tests that Add method properly sets bounds constraint that can be retrieved.
        /// Verifies the bounds constraint is correctly associated with the view.
        /// </summary>
        [Fact]
        public void Add_ValidBoundsExpression_SetsBoundsConstraintRetrievable()
        {
            // Arrange
            var relativeLayout = new RelativeLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();

            // Act
            relativeLayout.Children.Add(view, () => new Rect(25, 35, 75, 85));

            // Assert
            var boundsConstraint = RelativeLayout.GetBoundsConstraint(view);
            Assert.NotNull(boundsConstraint);
        }
    }


    /// <summary>
    /// Tests for the IRelativeList.Add method that accepts Expression parameters.
    /// </summary>
    public partial class IRelativeListAddExpressionTests : BaseTestFixture
    {
        public IRelativeListAddExpressionTests()
        {
            ExpressionSearch.Default = new UnitExpressionSearch();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ExpressionSearch.Default = new MockExpressionSearch();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Tests that Add throws ArgumentNullException when view parameter is null.
        /// </summary>
        [Fact]
        public void Add_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            var relativeLayout = new RelativeLayout
            {
                IsPlatformEnabled = true
            };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                relativeLayout.Children.Add(null, () => 0, () => 0, () => 100, () => 100));
        }

        /// <summary>
        /// Tests that Add works correctly when all expression parameters are null, using default values.
        /// </summary>
        [Fact]
        public void Add_AllExpressionsNull_UsesDefaultValues()
        {
            // Arrange
            var relativeLayout = new RelativeLayout
            {
                IsPlatformEnabled = true
            };
            var child = MockPlatformSizeService.Sub<View>();

            // Act
            relativeLayout.Children.Add(child, null, null, null, null);
            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            // Assert
            Assert.Equal(0, child.Bounds.X);
            Assert.Equal(0, child.Bounds.Y);
            // Width and height should use measured values, which are typically 100x20 for mock views
            Assert.True(child.Bounds.Width > 0);
            Assert.True(child.Bounds.Height > 0);
        }

        /// <summary>
        /// Tests that Add works correctly with simple constant expressions.
        /// </summary>
        [Fact]
        public void Add_SimpleConstantExpressions_PositionsCorrectly()
        {
            // Arrange
            var relativeLayout = new RelativeLayout
            {
                IsPlatformEnabled = true
            };
            var child = MockPlatformSizeService.Sub<View>();

            // Act
            relativeLayout.Children.Add(child, () => 30, () => 20, () => 50, () => 25);
            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            // Assert
            Assert.Equal(new Rect(30, 20, 50, 25), child.Bounds);
        }

        /// <summary>
        /// Tests that Add works correctly when only some expression parameters are null.
        /// </summary>
        [Fact]
        public void Add_SomeExpressionsNull_MixesDefaultsAndCustomValues()
        {
            // Arrange
            var relativeLayout = new RelativeLayout
            {
                IsPlatformEnabled = true
            };
            var child = MockPlatformSizeService.Sub<View>();

            // Act - Provide x and y, let width and height use defaults
            relativeLayout.Children.Add(child, () => 15, () => 25, null, null);
            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            // Assert
            Assert.Equal(15, child.Bounds.X);
            Assert.Equal(25, child.Bounds.Y);
            Assert.True(child.Bounds.Width > 0);
            Assert.True(child.Bounds.Height > 0);
        }

        /// <summary>
        /// Tests that Add works correctly with expressions that reference the parent layout.
        /// </summary>
        [Fact]
        public void Add_ParentRelativeExpressions_PositionsRelativeToParent()
        {
            // Arrange
            var relativeLayout = new RelativeLayout
            {
                IsPlatformEnabled = true
            };
            var child = MockPlatformSizeService.Sub<View>();

            // Act
            relativeLayout.Children.Add(child,
                () => relativeLayout.Width / 4,
                () => relativeLayout.Height / 5,
                () => relativeLayout.Width / 2,
                () => relativeLayout.Height / 4);
            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            // Assert
            Assert.Equal(new Rect(25, 20, 50, 25), child.Bounds);
        }

        /// <summary>
        /// Tests that Add works correctly with expressions that reference other child views.
        /// </summary>
        [Fact]
        public void Add_ViewRelativeExpressions_PositionsRelativeToOtherViews()
        {
            // Arrange
            var relativeLayout = new RelativeLayout
            {
                IsPlatformEnabled = true
            };
            var child1 = MockPlatformSizeService.Sub<View>();
            var child2 = MockPlatformSizeService.Sub<View>();

            // Act
            relativeLayout.Children.Add(child1, () => 10, () => 15, () => 30, () => 20);
            relativeLayout.Children.Add(child2,
                () => child1.Bounds.Right + 5,
                () => child1.Bounds.Y,
                () => child1.Bounds.Width,
                () => child1.Bounds.Height);
            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            // Assert
            Assert.Equal(new Rect(10, 15, 30, 20), child1.Bounds);
            Assert.Equal(new Rect(45, 15, 30, 20), child2.Bounds);
        }

        /// <summary>
        /// Tests that Add works correctly with complex expressions involving multiple views.
        /// </summary>
        [Fact]
        public void Add_MultiViewExpressions_PositionsUsingMultipleReferences()
        {
            // Arrange
            var relativeLayout = new RelativeLayout
            {
                IsPlatformEnabled = true
            };
            var child1 = MockPlatformSizeService.Sub<View>();
            var child2 = MockPlatformSizeService.Sub<View>();
            var child3 = MockPlatformSizeService.Sub<View>();

            // Act
            relativeLayout.Children.Add(child1, () => 20, () => 10, () => 25, () => 15);
            relativeLayout.Children.Add(child2, () => 20, () => 40, () => 30, () => 20);
            relativeLayout.Children.Add(child3,
                () => child1.Bounds.Right + 5,
                () => child1.Bounds.Y,
                () => child1.Bounds.Width + child2.Bounds.Width,
                () => child1.Bounds.Height + child2.Bounds.Height);
            relativeLayout.Layout(new Rect(0, 0, 150, 100));

            // Assert
            Assert.Equal(new Rect(20, 10, 25, 15), child1.Bounds);
            Assert.Equal(new Rect(20, 40, 30, 20), child2.Bounds);
            Assert.Equal(new Rect(50, 10, 55, 35), child3.Bounds);
        }

        /// <summary>
        /// Tests that Add handles extreme coordinate values correctly.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 1, 1)]
        [InlineData(-10, -5, 200, 150)]
        [InlineData(double.MaxValue, 0, 1, 1)]
        [InlineData(0, double.MaxValue, 1, 1)]
        public void Add_ExtremeValues_HandlesCorrectly(double x, double y, double width, double height)
        {
            // Arrange
            var relativeLayout = new RelativeLayout
            {
                IsPlatformEnabled = true
            };
            var child = MockPlatformSizeService.Sub<View>();

            // Act
            relativeLayout.Children.Add(child, () => x, () => y, () => width, () => height);
            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            // Assert
            Assert.Equal(x, child.Bounds.X);
            Assert.Equal(y, child.Bounds.Y);
            Assert.Equal(width, child.Bounds.Width);
            Assert.Equal(height, child.Bounds.Height);
        }

        /// <summary>
        /// Tests that Add handles zero and negative dimensions correctly.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(-10, -5)]
        [InlineData(0, -1)]
        [InlineData(-1, 0)]
        public void Add_ZeroAndNegativeDimensions_HandlesCorrectly(double width, double height)
        {
            // Arrange
            var relativeLayout = new RelativeLayout
            {
                IsPlatformEnabled = true
            };
            var child = MockPlatformSizeService.Sub<View>();

            // Act
            relativeLayout.Children.Add(child, () => 10, () => 10, () => width, () => height);
            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            // Assert
            Assert.Equal(width, child.Bounds.Width);
            Assert.Equal(height, child.Bounds.Height);
        }

        /// <summary>
        /// Tests that Add correctly adds the view to the collection.
        /// </summary>
        [Fact]
        public void Add_ValidView_AddsToCollection()
        {
            // Arrange
            var relativeLayout = new RelativeLayout
            {
                IsPlatformEnabled = true
            };
            var child = MockPlatformSizeService.Sub<View>();
            var initialCount = relativeLayout.Children.Count;

            // Act
            relativeLayout.Children.Add(child, () => 0, () => 0, () => 50, () => 50);

            // Assert
            Assert.Equal(initialCount + 1, relativeLayout.Children.Count);
            Assert.Contains(child, relativeLayout.Children);
        }

        /// <summary>
        /// Tests that Add can handle the same view being added to different positions (replaces previous).
        /// </summary>
        [Fact]
        public void Add_SameViewTwice_ReplacesPosition()
        {
            // Arrange
            var relativeLayout = new RelativeLayout
            {
                IsPlatformEnabled = true
            };
            var child = MockPlatformSizeService.Sub<View>();

            // Act
            relativeLayout.Children.Add(child, () => 10, () => 10, () => 20, () => 20);
            var countAfterFirst = relativeLayout.Children.Count;
            relativeLayout.Children.Add(child, () => 30, () => 30, () => 40, () => 40);
            relativeLayout.Layout(new Rect(0, 0, 100, 100));

            // Assert
            Assert.Equal(countAfterFirst, relativeLayout.Children.Count); // Count shouldn't increase
            Assert.Equal(new Rect(30, 30, 40, 40), child.Bounds); // Should use new position
        }

        /// <summary>
        /// Helper class for expression search during testing.
        /// </summary>
        class UnitExpressionSearch : ExpressionVisitor, IExpressionSearch
        {
            List<object> results;
            Type targetType;

            public List<T> FindObjects<T>(Expression expression) where T : class
            {
                if (expression == null)
                    return new List<T>();

                results = new List<object>();
                targetType = typeof(T);
                Visit(expression);
                return results.OfType<T>().ToList();
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression != null)
                {
                    if (node.Member.Name == "Bounds" && targetType.IsAssignableFrom(node.Expression.Type))
                    {
                        var constant = node.Expression as ConstantExpression ??
                                     (node.Expression as MemberExpression)?.Expression as ConstantExpression;
                        if (constant?.Value != null && targetType.IsAssignableFrom(constant.Value.GetType()))
                        {
                            results.Add(constant.Value);
                        }
                    }
                }
                return base.VisitMember(node);
            }
        }
    }


    /// <summary>
    /// Tests for the IRelativeList&lt;T&gt;.Add method with constraint parameters.
    /// </summary>
    public partial class IRelativeListAddTests : BaseTestFixture
    {
        public IRelativeListAddTests()
        {
            ExpressionSearch.Default = new UnitExpressionSearch();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ExpressionSearch.Default = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentNullException when view parameter is null.
        /// Input: null view parameter with various constraint combinations.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void Add_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var children = relativeLayout.Children;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => children.Add(null));
        }

        /// <summary>
        /// Tests that Add method works correctly when all constraint parameters are null (default behavior).
        /// Input: Valid view with all constraints as null.
        /// Expected: View is added to collection with no constraints set.
        /// </summary>
        [Fact]
        public void Add_AllConstraintsNull_AddsViewSuccessfully()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var children = relativeLayout.Children;
            var view = MockPlatformSizeService.Sub<View>();
            var initialCount = children.Count;

            // Act
            children.Add(view, null, null, null, null);

            // Assert
            Assert.Equal(initialCount + 1, children.Count);
            Assert.Contains(view, children);
            Assert.Null(RelativeLayout.GetXConstraint(view));
            Assert.Null(RelativeLayout.GetYConstraint(view));
            Assert.Null(RelativeLayout.GetWidthConstraint(view));
            Assert.Null(RelativeLayout.GetHeightConstraint(view));
        }

        /// <summary>
        /// Tests that Add method correctly sets individual X constraint.
        /// Input: View with only X constraint specified.
        /// Expected: View is added and X constraint is set, others remain null.
        /// </summary>
        [Fact]
        public void Add_XConstraintOnly_SetsXConstraintCorrectly()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var children = relativeLayout.Children;
            var view = MockPlatformSizeService.Sub<View>();
            var xConstraint = Constraint.Constant(50);

            // Act
            children.Add(view, xConstraint);

            // Assert
            Assert.Contains(view, children);
            Assert.Equal(xConstraint, RelativeLayout.GetXConstraint(view));
            Assert.Null(RelativeLayout.GetYConstraint(view));
            Assert.Null(RelativeLayout.GetWidthConstraint(view));
            Assert.Null(RelativeLayout.GetHeightConstraint(view));
        }

        /// <summary>
        /// Tests that Add method correctly sets individual Y constraint.
        /// Input: View with only Y constraint specified.
        /// Expected: View is added and Y constraint is set, others remain null.
        /// </summary>
        [Fact]
        public void Add_YConstraintOnly_SetsYConstraintCorrectly()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var children = relativeLayout.Children;
            var view = MockPlatformSizeService.Sub<View>();
            var yConstraint = Constraint.Constant(30);

            // Act
            children.Add(view, null, yConstraint);

            // Assert
            Assert.Contains(view, children);
            Assert.Null(RelativeLayout.GetXConstraint(view));
            Assert.Equal(yConstraint, RelativeLayout.GetYConstraint(view));
            Assert.Null(RelativeLayout.GetWidthConstraint(view));
            Assert.Null(RelativeLayout.GetHeightConstraint(view));
        }

        /// <summary>
        /// Tests that Add method correctly sets individual width constraint.
        /// Input: View with only width constraint specified.
        /// Expected: View is added and width constraint is set, others remain null.
        /// </summary>
        [Fact]
        public void Add_WidthConstraintOnly_SetsWidthConstraintCorrectly()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var children = relativeLayout.Children;
            var view = MockPlatformSizeService.Sub<View>();
            var widthConstraint = Constraint.RelativeToParent(parent => parent.Width * 0.5);

            // Act
            children.Add(view, null, null, widthConstraint);

            // Assert
            Assert.Contains(view, children);
            Assert.Null(RelativeLayout.GetXConstraint(view));
            Assert.Null(RelativeLayout.GetYConstraint(view));
            Assert.Equal(widthConstraint, RelativeLayout.GetWidthConstraint(view));
            Assert.Null(RelativeLayout.GetHeightConstraint(view));
        }

        /// <summary>
        /// Tests that Add method correctly sets individual height constraint.
        /// Input: View with only height constraint specified.
        /// Expected: View is added and height constraint is set, others remain null.
        /// </summary>
        [Fact]
        public void Add_HeightConstraintOnly_SetsHeightConstraintCorrectly()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var children = relativeLayout.Children;
            var view = MockPlatformSizeService.Sub<View>();
            var heightConstraint = Constraint.RelativeToParent(parent => parent.Height * 0.25);

            // Act
            children.Add(view, null, null, null, heightConstraint);

            // Assert
            Assert.Contains(view, children);
            Assert.Null(RelativeLayout.GetXConstraint(view));
            Assert.Null(RelativeLayout.GetYConstraint(view));
            Assert.Null(RelativeLayout.GetWidthConstraint(view));
            Assert.Equal(heightConstraint, RelativeLayout.GetHeightConstraint(view));
        }

        /// <summary>
        /// Tests that Add method correctly sets all constraints when provided.
        /// Input: View with all four constraints specified.
        /// Expected: View is added and all constraints are set correctly.
        /// </summary>
        [Fact]
        public void Add_AllConstraintsProvided_SetsAllConstraintsCorrectly()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var children = relativeLayout.Children;
            var view = MockPlatformSizeService.Sub<View>();
            var xConstraint = Constraint.Constant(10);
            var yConstraint = Constraint.Constant(20);
            var widthConstraint = Constraint.Constant(100);
            var heightConstraint = Constraint.Constant(50);

            // Act
            children.Add(view, xConstraint, yConstraint, widthConstraint, heightConstraint);

            // Assert
            Assert.Contains(view, children);
            Assert.Equal(xConstraint, RelativeLayout.GetXConstraint(view));
            Assert.Equal(yConstraint, RelativeLayout.GetYConstraint(view));
            Assert.Equal(widthConstraint, RelativeLayout.GetWidthConstraint(view));
            Assert.Equal(heightConstraint, RelativeLayout.GetHeightConstraint(view));
        }

        /// <summary>
        /// Tests that Add method works with various constraint types.
        /// Input: View with different types of constraints (Constant, RelativeToParent).
        /// Expected: View is added and all constraint types are properly set.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(50.0)]
        [InlineData(100.5)]
        [InlineData(double.MaxValue)]
        public void Add_ConstantConstraints_SetsConstraintsCorrectly(double constantValue)
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var children = relativeLayout.Children;
            var view = MockPlatformSizeService.Sub<View>();
            var constraint = Constraint.Constant(constantValue);

            // Act
            children.Add(view, constraint, constraint, constraint, constraint);

            // Assert
            Assert.Contains(view, children);
            Assert.Equal(constraint, RelativeLayout.GetXConstraint(view));
            Assert.Equal(constraint, RelativeLayout.GetYConstraint(view));
            Assert.Equal(constraint, RelativeLayout.GetWidthConstraint(view));
            Assert.Equal(constraint, RelativeLayout.GetHeightConstraint(view));
        }

        /// <summary>
        /// Tests that Add method works with RelativeToParent constraints.
        /// Input: View with RelativeToParent constraints.
        /// Expected: View is added and RelativeToParent constraints are properly set.
        /// </summary>
        [Fact]
        public void Add_RelativeToParentConstraints_SetsConstraintsCorrectly()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var children = relativeLayout.Children;
            var view = MockPlatformSizeService.Sub<View>();
            var xConstraint = Constraint.RelativeToParent(parent => parent.Width * 0.1);
            var yConstraint = Constraint.RelativeToParent(parent => parent.Height * 0.2);
            var widthConstraint = Constraint.RelativeToParent(parent => parent.Width * 0.5);
            var heightConstraint = Constraint.RelativeToParent(parent => parent.Height * 0.3);

            // Act
            children.Add(view, xConstraint, yConstraint, widthConstraint, heightConstraint);

            // Assert
            Assert.Contains(view, children);
            Assert.Equal(xConstraint, RelativeLayout.GetXConstraint(view));
            Assert.Equal(yConstraint, RelativeLayout.GetYConstraint(view));
            Assert.Equal(widthConstraint, RelativeLayout.GetWidthConstraint(view));
            Assert.Equal(heightConstraint, RelativeLayout.GetHeightConstraint(view));
        }

        /// <summary>
        /// Tests that Add method increases collection count correctly.
        /// Input: Multiple views added with different constraint combinations.
        /// Expected: Collection count increases appropriately for each addition.
        /// </summary>
        [Fact]
        public void Add_MultipleViews_IncreasesCountCorrectly()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var children = relativeLayout.Children;
            var view1 = MockPlatformSizeService.Sub<View>();
            var view2 = MockPlatformSizeService.Sub<View>();
            var view3 = MockPlatformSizeService.Sub<View>();
            var initialCount = children.Count;

            // Act
            children.Add(view1);
            children.Add(view2, Constraint.Constant(10));
            children.Add(view3, Constraint.Constant(20), Constraint.Constant(30));

            // Assert
            Assert.Equal(initialCount + 3, children.Count);
            Assert.Contains(view1, children);
            Assert.Contains(view2, children);
            Assert.Contains(view3, children);
        }

        /// <summary>
        /// Tests that Add method handles edge case constraint values correctly.
        /// Input: View with edge case constraint values (zero, negative, extreme values).
        /// Expected: View is added and constraints are set without errors.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(-100.5)]
        [InlineData(double.MinValue)]
        public void Add_EdgeCaseConstraintValues_HandlesCorrectly(double edgeValue)
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var children = relativeLayout.Children;
            var view = MockPlatformSizeService.Sub<View>();
            var constraint = Constraint.Constant(edgeValue);

            // Act
            children.Add(view, constraint, constraint, constraint, constraint);

            // Assert
            Assert.Contains(view, children);
            Assert.Equal(constraint, RelativeLayout.GetXConstraint(view));
            Assert.Equal(constraint, RelativeLayout.GetYConstraint(view));
            Assert.Equal(constraint, RelativeLayout.GetWidthConstraint(view));
            Assert.Equal(constraint, RelativeLayout.GetHeightConstraint(view));
        }

        class UnitExpressionSearch : ExpressionVisitor, IExpressionSearch
        {
            List<object> results;
            Type targeType;

            public List<T> FindObjects<T>(Expression expression) where T : class
            {
                results = new List<object>();
                targeType = typeof(T);
                Visit(expression);
                return results.OfType<T>().ToList();
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression is ConstantExpression && node.Member is System.Reflection.FieldInfo)
                {
                    var container = ((ConstantExpression)node.Expression).Value;
                    var value = ((System.Reflection.FieldInfo)node.Member).GetValue(container);

                    if (targeType.IsInstanceOfType(value))
                        results.Add(value);
                }

                return base.VisitMember(node);
            }
        }
    }


    /// <summary>
    /// Tests for the RelativeElementCollection.Add method that takes a View and Expression bounds parameter.
    /// </summary>
    public partial class RelativeElementCollectionTests
    {
        /// <summary>
        /// Tests that Add method throws ArgumentNullException when bounds parameter is null.
        /// This test ensures proper null validation for the bounds Expression parameter.
        /// Expected result: ArgumentNullException is thrown with parameter name "bounds".
        /// </summary>
        [Fact]
        public void Add_NullBounds_ThrowsArgumentNullException()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var view = new View();
            Expression<Func<Rect>> bounds = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                relativeLayout.Children.Add(view, bounds));
            Assert.Equal("bounds", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method successfully adds a view with valid bounds expression.
        /// This test verifies the normal operation path with valid parameters.
        /// Expected result: View is added to the collection and bounds constraint is set.
        /// </summary>
        [Fact]
        public void Add_ValidViewAndBounds_AddsViewSuccessfully()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var view = new View();
            Expression<Func<Rect>> bounds = () => new Rect(10, 20, 100, 200);

            // Act
            relativeLayout.Children.Add(view, bounds);

            // Assert
            Assert.Contains(view, relativeLayout.Children);
            var boundsConstraint = RelativeLayout.GetBoundsConstraint(view);
            Assert.NotNull(boundsConstraint);
        }

        /// <summary>
        /// Tests that Add method handles null view parameter with valid bounds expression.
        /// This test verifies behavior when view is null but bounds is valid.
        /// Expected result: The method should handle null view (no explicit null check for view in the method).
        /// </summary>
        [Fact]
        public void Add_NullViewValidBounds_HandlesNullView()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            View view = null;
            Expression<Func<Rect>> bounds = () => new Rect(0, 0, 50, 50);

            // Act & Assert
            // The method doesn't explicitly check for null view, so this should not throw ArgumentNullException
            // However, the underlying base.Add(view) or SetBoundsConstraint might handle null view differently
            relativeLayout.Children.Add(view, bounds);
        }

        /// <summary>
        /// Tests that Add method works with complex bounds expressions.
        /// This test verifies that the method can handle more complex expression scenarios.
        /// Expected result: View is added successfully with complex bounds constraint.
        /// </summary>
        [Fact]
        public void Add_ComplexBoundsExpression_AddsViewSuccessfully()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var view = new View();
            var parentWidth = 400.0;
            var parentHeight = 600.0;
            Expression<Func<Rect>> bounds = () => new Rect(
                parentWidth * 0.1,
                parentHeight * 0.2,
                parentWidth * 0.5,
                parentHeight * 0.3);

            // Act
            relativeLayout.Children.Add(view, bounds);

            // Assert
            Assert.Contains(view, relativeLayout.Children);
            var boundsConstraint = RelativeLayout.GetBoundsConstraint(view);
            Assert.NotNull(boundsConstraint);
        }

        /// <summary>
        /// Tests that Add method works with bounds expression that returns Rect.Zero.
        /// This test verifies edge case with zero-sized rectangle bounds.
        /// Expected result: View is added successfully with zero bounds constraint.
        /// </summary>
        [Fact]
        public void Add_ZeroBoundsExpression_AddsViewSuccessfully()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var view = new View();
            Expression<Func<Rect>> bounds = () => Rect.Zero;

            // Act
            relativeLayout.Children.Add(view, bounds);

            // Assert
            Assert.Contains(view, relativeLayout.Children);
            var boundsConstraint = RelativeLayout.GetBoundsConstraint(view);
            Assert.NotNull(boundsConstraint);
        }

        /// <summary>
        /// Tests that Add method works with bounds expression containing extreme values.
        /// This test verifies behavior with boundary value bounds expressions.
        /// Expected result: View is added successfully with extreme bounds constraint.
        /// </summary>
        [Fact]
        public void Add_ExtremeBoundsExpression_AddsViewSuccessfully()
        {
            // Arrange
            var relativeLayout = new RelativeLayout();
            var view = new View();
            Expression<Func<Rect>> bounds = () => new Rect(
                double.MaxValue,
                double.MinValue,
                double.PositiveInfinity,
                double.NegativeInfinity);

            // Act
            relativeLayout.Children.Add(view, bounds);

            // Assert
            Assert.Contains(view, relativeLayout.Children);
            var boundsConstraint = RelativeLayout.GetBoundsConstraint(view);
            Assert.NotNull(boundsConstraint);
        }
    }
}