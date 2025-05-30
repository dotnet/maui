using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ReceivedExtensions;

namespace Microsoft.Maui.UnitTests.Layouts
{
	public static class LayoutTestHelpers
	{
		public static IView CreateTestView()
		{
			var view = Substitute.For<IView>();

			view.Height.Returns(-1);
			view.Width.Returns(-1);

			return view;
		}

		public static IView CreateTestView(Size viewSize)
		{
			var view = CreateTestView();

			// Prior to being measured, the View has no DesiredSize set
			view.DesiredSize.Returns(Size.Zero);

			// After the first time Measure is called, the View has a DesiredSize
			view.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(viewSize)
				.AndDoes(ci => view.DesiredSize.Returns(viewSize));

			return view;
		}

		public static void SubstituteChildren(ILayout layout, params IView[] views)
		{
			var children = new List<IView>(views);

			SubstituteChildren(layout, children);
		}

		public static void SubstituteChildren(ILayout layout, IList<IView> children)
		{
			layout[Arg.Any<int>()].Returns(args => children[(int)args[0]]);
			layout.GetEnumerator().Returns((ci) => children.GetEnumerator());
			layout.Count.Returns(children.Count);
		}

		public static void AssertArranged(IView view, double x, double y, double width, double height)
		{
			var expected = new Rect(x, y, width, height);
			view.Received().Arrange(Arg.Is(expected));
		}

		public static void AssertArranged(IView view, Rect expected)
		{
			view.Received().Arrange(Arg.Is(expected));
		}

		// Create a test view which works like a text block
		// So the height is dependent on the width being measured
		public static IView CreateWidthDominatedView(double unconstrainedWidth, double unconstrainedHeight,
			params Tuple<double, double>[] sizes)
		{
			var view = CreateTestView();

			Func<CallInfo, Size> fakeMeasure = (ci) =>
			{
				var widthConstraint = (double)ci.Args()[0];
				var heightConstraint = (double)ci.Args()[1];

				double width = 0;
				double height = 0;

				if (double.IsPositiveInfinity(widthConstraint))
				{
					width = unconstrainedWidth;
					height = unconstrainedHeight;
				}

				for (int n = 0; n < sizes.Length; n++)
				{
					if (widthConstraint <= sizes[n].Item1)
					{
						width = widthConstraint;
						height = sizes[n].Item2;
						break;
					}
				}

				return new Size(width, height);
			};

			view.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(fakeMeasure);

			return view;
		}

		// Create a test view where the width is dependent on the height
		// (e.g., like an image with a fixed aspect ratio)
		public static IView CreateHeightDominatedView(double unconstrainedWidth, double unconstrainedHeight,
			params Tuple<double, double>[] sizes)
		{
			var view = CreateTestView();

			Func<CallInfo, Size> fakeMeasure = (ci) =>
			{
				var widthConstraint = (double)ci.Args()[0];
				var heightConstraint = (double)ci.Args()[1];

				double width = 0;
				double height = 0;

				if (double.IsPositiveInfinity(heightConstraint))
				{
					width = unconstrainedWidth;
					height = unconstrainedHeight;
				}

				for (int n = 0; n < sizes.Length; n++)
				{
					if (heightConstraint <= sizes[n].Item2)
					{
						width = sizes[n].Item1;
						height = heightConstraint;
						break;
					}
				}

				return new Size(width, height);
			};

			view.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(fakeMeasure);

			return view;
		}

		public static View Add(this Grid grid, int row, int column, int rowSpan = 1, int columnSpan = 1)
		{
			var child = new ContentView();

			Grid.SetRow(child, row);
			Grid.SetColumn(child, column);
			Grid.SetRowSpan(child, rowSpan);
			Grid.SetColumnSpan(child, columnSpan);

			grid.Add(child);

			return child;
		}
	}
}
