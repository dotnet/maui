using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using NSubstitute;

namespace Microsoft.Maui.UnitTests.Layouts
{
	public abstract class StackLayoutManagerTests
	{
		protected IStackLayout CreateTestLayout()
		{
			var stack = Substitute.For<IStackLayout>();
			stack.Height.Returns(-1);
			stack.Width.Returns(-1);
			stack.Spacing.Returns(0);

			return stack;
		}

		protected IView CreateTestView()
		{
			var view = Substitute.For<IView>();

			view.Height.Returns(-1);
			view.Width.Returns(-1);

			return view;
		}

		protected IView CreateTestView(Size viewSize)
		{
			var view = CreateTestView();
			var handler = Substitute.For<IViewHandler>();

			view.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(viewSize);
			view.DesiredSize.Returns(viewSize);
			view.Handler.Returns(handler);

			return view;
		}

		protected IStackLayout BuildStack(int viewCount, double viewWidth, double viewHeight)
		{
			var stack = CreateTestLayout();

			var children = new List<IView>();

			for (int n = 0; n < viewCount; n++)
			{
				var view = LayoutTestHelpers.CreateTestView(new Size(viewWidth, viewHeight));
				children.Add(view);
			}

			stack.Children.Returns(children.AsReadOnly());

			return stack;
		}
	}
}
