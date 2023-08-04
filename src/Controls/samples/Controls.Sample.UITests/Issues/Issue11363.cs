using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 11363, "CollectionView inside RefreshView freezes app completely", PlatformAffected.iOS)]
	public class Issue11363 : TestContentPage
	{
		public List<int> Numbers { get; } = Enumerable.Range(1, 100).ToList();

		protected override void Init()
		{
			ToolbarItems.Add(new ToolbarItem
			{
				Text = "alert",
				Order =  ToolbarItemOrder.Secondary,
				Command = new Command(async () => await DisplayAlert("Toolbar action", "Hello, I'm a toolbar action :)", "OK"))
			});
			;
			var collectionView = new CollectionView
			{
				
			};
			collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var layout = new Frame
				{
					HeightRequest = 80,
					HasShadow = false,
					CornerRadius = 4,
					Margin = new Microsoft.Maui.Thickness(5, 2, 5, 2)

				};
				var view = new Label
				{
					AutomationId = "labeCell"
				};
				view.SetBinding(Label.TextProperty, ".");
				layout.Content = view;

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
