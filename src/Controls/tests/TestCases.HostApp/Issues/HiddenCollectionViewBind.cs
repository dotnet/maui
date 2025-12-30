namespace Maui.Controls.Sample.Issues
{
	// CollectionShouldInvalidateOnVisibilityChange (src\Compatibility\ControlGallery\src\Issues.Shared\Issue13203.cs)
	[Issue(IssueTracker.None, 13203, "[Bug] [iOS] CollectionView does not bind to items if `IsVisible=False`", PlatformAffected.iOS)]
	public class HiddenCollectionViewBind : ContentPage
	{
		const string Success = "Success";

		public HiddenCollectionViewBind()
		{
			this.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			var cv = new CollectionView
			{
				IsVisible = false,

				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding(nameof(Item.Text)));
					return label;
				})
			};

			var source = new List<Item> { new Item { Text = Success } };
			cv.ItemsSource = source;

			Content = cv;

			Appearing += (sender, args) => { cv.IsVisible = true; };
		}

		class Item
		{
			public string Text { get; set; }
		}
	}
}