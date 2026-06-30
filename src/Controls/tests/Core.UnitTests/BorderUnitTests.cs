#nullable enable
using System;
using System.Threading.Tasks;
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

		[Fact]
		public async Task SharedStrokeDashArrayDoesNotLeakBorder()
		{
			// A shared / long-lived DoubleCollection, exactly as the issue describes
			// (e.g. a static field or a value reused from a ResourceDictionary).
			var sharedDashArray = new DoubleCollection { 2, 2 };

			WeakReference weakBorder;
			{
				var border = new Border { StrokeDashArray = sharedDashArray };

				// Reading StrokeDashPattern installs the CollectionChanged subscription,
				// exactly as the platform border handler does when rendering the dash.
				_ = border.StrokeDashPattern;

				weakBorder = new WeakReference(border);
				// Drop the only strong reference to `border`; `sharedDashArray` stays alive.
			}

			Assert.False(await weakBorder.WaitForCollect(), "Border should not be alive!");
			GC.KeepAlive(sharedDashArray);
		}
	}
}