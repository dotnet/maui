#nullable enable
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class BorderUnitTests : BaseTestFixture
	{
		[Fact]
		public void HasNoVisualChildrenWhenNoContentIsSet()
		{
			var border = new Border();

			Assert.Empty(((IVisualTreeElement)border).GetVisualChildren());

			border.Content = null;

			Assert.Empty(((IVisualTreeElement)border).GetVisualChildren());
		}

		[Fact]
		public void HasVisualChildrenWhenContentIsSet()
		{
			var border = new Border();

			var label = new Label();
			border.Content = label;

			var visualTreeChildren = ((IVisualTreeElement)border).GetVisualChildren();
			Assert.NotEmpty(visualTreeChildren);
			Assert.Same(visualTreeChildren[0], label);
		}

		[Fact]
		public void ChildrenHaveParentsWhenContentIsSet()
		{
			var border = new Border();

			var label = new Label();
			border.Content = label;

			Assert.Same(border, label.Parent);

			border.Content = null;
			Assert.Null(label.Parent);
		}
	}
}