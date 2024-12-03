namespace Maui.Controls.Sample.CollectionViewGalleries.EmptyViewGalleries
{
	public partial class EmptyViewNullGallery : ContentPage
	{
		public EmptyViewNullGallery(bool useOnlyText = true)
		{
			InitializeComponent();
			string emptyViewText = "Nothing to display.";
			CollectionView.EmptyView = useOnlyText ? emptyViewText :
													 new Grid
													 {
														 Children = { new Label
													 {
														 Text = emptyViewText,
														 HorizontalOptions = LayoutOptions.Center,
														 VerticalOptions = LayoutOptions.Center,
														 FontAttributes = FontAttributes.Bold
													 } }
													 };
		}
	}
}