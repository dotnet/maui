using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 11327, "Incorrect layout with GridItemsLayout and RefreshView", PlatformAffected.iOS)]
	public class Issue11327 : TestContentPage
	{
		public List<int> Numbers { get; } = Enumerable.Range(1, 100).ToList();

		protected override void Init()
		{
			var itemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				Span = 2,
				VerticalItemSpacing = 5,
				HorizontalItemSpacing = 5
			};
			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				BackgroundColor = Colors.DarkBlue
			};
			collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var layout = new StackLayout
				{
					BackgroundColor = Colors.Purple,
					HeightRequest = 50
				};
				var view = new Label
				{
					AutomationId = "labeCell"
				};
				view.SetBinding(Label.TextProperty, ".");
				layout.Add(view);

				return layout;
			});
			collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(Numbers));

			Content = new RefreshView
			{
				Content = collectionView
			};

			BindingContext = this;
		}
	}
}
