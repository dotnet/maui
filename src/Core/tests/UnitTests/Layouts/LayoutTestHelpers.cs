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

		public static void SubstituteChildren(ILayout layout, params IView[] views)
		{
			var children = new List<IView>(views);

			SubstituteChildren(layout, children);
		}

		public static void SubstituteChildren(ILayout layout, IList<IView> children)
		{
			layout[Arg.Any<int>()].Returns(args => children[(int)args[0]]);
			layout.GetEnumerator().Returns(children.GetEnumerator());
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
	}
}
