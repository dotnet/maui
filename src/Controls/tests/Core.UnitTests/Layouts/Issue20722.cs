using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	public class Issue20722 : BaseTestFixture
	{
		[Fact]
		public void NestedVisibilityChangeInvalidatesContainingItem()
		{
			try
			{
				VisualElement.SkipMeasureInvalidatedPropagation = true;

				var targetChild = new Button();
				var targetItem = new Border
				{
					Content = new Grid
					{
						Children = { targetChild }
					}
				};

				var items = new VerticalStackLayout
				{
					Children =
					{
						new Border { Content = new Label { Text = "Unrelated item" } },
						targetItem
					}
				};

				_ = new TestWindow(new ContentPage
				{
					Content = new ScrollView { Content = items }
				});

				bool targetItemInvalidated = false;
				targetItem.MeasureInvalidated += (_, _) => targetItemInvalidated = true;

				targetChild.IsVisible = false;

				Assert.True(targetItemInvalidated, "Changing a nested child's visibility should invalidate its containing item.");
			}
			finally
			{
				VisualElement.SkipMeasureInvalidatedPropagation = false;
			}
		}
	}
}
