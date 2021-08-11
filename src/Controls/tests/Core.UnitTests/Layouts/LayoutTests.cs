using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[TestFixture, Category("Layout")]
	public class LayoutTests : BaseTestFixture
	{
		[Test]
		public void UsingIndexUpdatesParent()
		{
			var layout = new VerticalStackLayout();

			var child0 = new Button();
			var child1 = new Button();

			Assert.Null(child0.Parent);
			Assert.Null(child1.Parent);

			layout.Add(child0);

			Assert.AreEqual(layout, child0.Parent);
			Assert.Null(child1.Parent);

			layout[0] = child1;

			Assert.Null(child0.Parent);
			Assert.AreEqual(layout, child1.Parent);
		}

		[Test]
		public void ClearUpdatesParent() 
		{
			var layout = new VerticalStackLayout();

			var child0 = new Button();
			var child1 = new Button();

			layout.Add(child0);
			layout.Add(child1);

			Assert.AreEqual(layout, child0.Parent);
			Assert.AreEqual(layout, child1.Parent);

			layout.Clear();

			Assert.Null(child0.Parent);
			Assert.Null(child1.Parent);
		}
	}
}
