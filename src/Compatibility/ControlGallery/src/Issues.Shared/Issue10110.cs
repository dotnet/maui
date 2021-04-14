using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10110, "CollectionView EmptyView doesn't show up on UWP HorizontalList", PlatformAffected.UWP)]
	public class Issue10110 : TestContentPage
	{
		public Issue10110()
		{
			Title = "Issue 10110";

			var layout = new Grid
			{
				RowSpacing = 0
			};

			layout.RowDefinitions.Add(new RowDefinition());
			layout.RowDefinitions.Add(new RowDefinition());

			var verticalCollectionView = new CollectionView
			{
				BackgroundColor = Colors.LightBlue,
				EmptyView = "Empty Vertical List as String"
			};

			var horizontalCollectionView = new CollectionView
			{
				BackgroundColor = Colors.LightCoral,
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
				EmptyView = "Empty Horizontal List as String"
			};

			layout.Children.Add(verticalCollectionView);
			Grid.SetRow(verticalCollectionView, 0);

			layout.Children.Add(horizontalCollectionView);
			Grid.SetRow(horizontalCollectionView, 1);

			Content = layout;
		}

		protected override void Init()
		{

		}
	}
}