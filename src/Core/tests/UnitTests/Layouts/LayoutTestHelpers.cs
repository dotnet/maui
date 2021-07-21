using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using NSubstitute;

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

			view.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(viewSize);
			view.DesiredSize.Returns(viewSize);

			return view;
		}

		public static void AddChildren(ILayout layout, params IView[] views)
		{
			var children = new List<IView>(views);
			layout.Children.Returns(children.AsReadOnly());
		}

		public static void AssertArranged(IView view, double x, double y, double width, double height)
		{
			var expected = new Rectangle(x, y, width, height);
			view.Received().Arrange(Arg.Is(expected));
		}
	}
}
