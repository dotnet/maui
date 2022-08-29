using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;
using NSubstitute;
using static Microsoft.Maui.UnitTests.Layouts.LayoutTestHelpers;

namespace Microsoft.Maui.UnitTests.Layouts
{
	public abstract class StackLayoutManagerTests
	{
		protected IStackLayout CreateTestLayout()
		{
			var stack = Substitute.For<IStackLayout>();
			stack.Height.Returns(Dimension.Unset);
			stack.Width.Returns(Dimension.Unset);
			stack.MinimumHeight.Returns(Dimension.Minimum);
			stack.MinimumWidth.Returns(Dimension.Minimum);
			stack.MaximumHeight.Returns(Dimension.Maximum);
			stack.MaximumWidth.Returns(Dimension.Maximum);
			stack.Spacing.Returns(0);

			return stack;
		}

		protected IStackLayout CreateTestLayout(IList<IView> children)
		{
			var stack = CreateTestLayout();
			SubstituteChildren(stack, children);
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

			SubstituteChildren(stack, children);

			return stack;
		}

		protected IStackLayout SetUpVisibilityTestStack(double viewWidth, double viewHeight, double spacing)
		{
			var startView = CreateTestView(new Size(viewWidth, viewHeight));
			var middleView = CreateTestView(new Size(viewWidth, viewHeight));
			var endView = CreateTestView(new Size(viewWidth, viewHeight));

			var stack = CreateTestLayout(new List<IView> { startView, middleView, endView });
			stack.Spacing.Returns(spacing);

			return stack;
		}
	}
}
