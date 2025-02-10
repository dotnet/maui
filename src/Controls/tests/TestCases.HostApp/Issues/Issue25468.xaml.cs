namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25468, "Collection view has no scroll bar", PlatformAffected.Android)]
	public partial class Issue25468 : ContentPage
	{
		public Issue25468()
		{
			InitializeComponent();

			// Modifying the scroll bar so that we can verify screenshot
#if ANDROID
			Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("CollectionViewHandler", (handler, view) =>
			{
				handler.PlatformView.ScrollbarFadingEnabled = false;
				handler.PlatformView.ScrollBarFadeDuration = 20;
				handler.PlatformView.ScrollBarSize = 50;
				handler.PlatformView.ScrollBarStyle = Android.Views.ScrollbarStyles.OutsideInset;
			});
#endif

			var items = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList();
			CollectionView.ItemsSource = items;
		}
	}
}