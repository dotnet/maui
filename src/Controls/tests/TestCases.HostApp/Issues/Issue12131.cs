using System.Reflection;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 12131, "RefreshView - CollectionView sizing not working correctly inside VerticalStackLayout", PlatformAffected.Android)]
	public class Issue12131 : ContentPage
	{
		public Issue12131()
		{
			var items = Enumerable.Range(1, 20).Select(i => $"Item {i}").ToList();

			var collectionView = new CollectionView
			{
				AutomationId = "CollectionView12131",
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, ".");
					return label;
				}),
				ItemsSource = items
			};

			var refreshView = new RefreshView
			{
				AutomationId = "RefreshView12131",
				Content = collectionView
			};

			var sizeLabel = new Label
			{
				AutomationId = "SizeLabel12131",
				Text = "Waiting..."
			};

			refreshView.SizeChanged += (s, e) =>
			{
				sizeLabel.Text = $"Height={refreshView.Height:F0}";
			};

			Content = new VerticalStackLayout
			{
				Padding = 10,
				Children =
				{
					new Label { Text = "CollectionView wrapped within RefreshView." },
					sizeLabel,
					refreshView
				}
			};
		}
	}
}
