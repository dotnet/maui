using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
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
}
