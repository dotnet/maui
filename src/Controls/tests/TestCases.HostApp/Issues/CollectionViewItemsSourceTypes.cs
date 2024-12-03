using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	// CollectionViewItemsSourceTypesDisplayAndDontCrash (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewItemsSourceTypes.cs)
	[Issue(IssueTracker.None, 0, "CollectionView ItemsSource Types", PlatformAffected.All)]
	public class CollectionViewItemsSourceTypes : ContentPage
	{
		public CollectionViewItemsSourceTypes()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "If you see three 900s this test has passed"
					},
					new CollectionView()
					{
						ItemsSource = new[] { 900 },
						HeightRequest = 50
					},
					new CollectionView()
					{
						ItemsSource = new[] { "900" }.ToList<object>(),
						HeightRequest = 50
					},
					new CollectionView()
					{
						ItemsSource = new ObservableCollection<string>(new[] { "900" }),
						HeightRequest = 50
					}
				}
			};
		}
	}
}