#nullable enable
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[Category("Layout")]
	public class AbsoluteLayoutTests : BaseTestFixture
	{
		[Fact]
		public void BoundsDefaultsConsistentForNewChildren()
		{
			var layout = new AbsoluteLayout();

			var child1 = new Label { }; // BindableObject
			var child2 = NSubstitute.Substitute.For<IView>(); // Not a BindableObject

			layout.Add(child1);
			layout.Add(child2);

			var bounds1 = layout.GetLayoutBounds(child1);
			var bounds2 = layout.GetLayoutBounds(child2);

			// The default layout bounds given to each of these child IViews _should_ be the same
			Assert.Equal(bounds1.X, bounds2.X);
			Assert.Equal(bounds1.Y, bounds2.Y);
			Assert.Equal(bounds1.Width, bounds2.Width);
			Assert.Equal(bounds1.Height, bounds2.Height);
		}

		[Fact]
		public void BoundsDefaultsAttachedProperty()
		{
			var layout = new AbsoluteLayout();

			var child = new Label { }; // BindableObject

			layout.Add(child);

			var bounds = layout.GetLayoutBounds(child);

			Assert.Equal(0, bounds.X);
			Assert.Equal(0, bounds.Y);
			Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
			Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
		}

		[Fact]
		public void BoundsDefaultsRegularProperty()
		{
			var layout = new AbsoluteLayout();

			var child = NSubstitute.Substitute.For<IView>(); // Not a BindableObject

			layout.Add(child);

			var bounds = layout.GetLayoutBounds(child);

			Assert.Equal(0, bounds.X);
			Assert.Equal(0, bounds.Y);
			Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
			Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
		}
	}
}
