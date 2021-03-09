using NSubstitute;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[TestFixture(Category = "Layout")]
	public class LayoutTests
	{
		[Test]
		public void IgnoreArrangeWithoutMeasure()
		{
			var layout = new VerticalStackLayout();

			var view = Substitute.For<IView>();
			view.IsMeasureValid.Returns(true);

			layout.Add(view);

			(layout as IFrameworkElement).Arrange(new Rectangle(0, 0, 100, 100));

			view.DidNotReceive().Arrange(Arg.Any<Rectangle>());
		}

		[Test]
		public void ArrangeAfterMeasure()
		{
			var layout = new VerticalStackLayout();

			var view = Substitute.For<IView>();
			view.IsMeasureValid.Returns(true);

			layout.Add(view);

			var size = (layout as IFrameworkElement).Measure(100, 100);
			(layout as IFrameworkElement).Arrange(new Rectangle(0, 0, size.Width, size.Height));

			view.Received().Arrange(Arg.Any<Rectangle>());
		}
	}
}
