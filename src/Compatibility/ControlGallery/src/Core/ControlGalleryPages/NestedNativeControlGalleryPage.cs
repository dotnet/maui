namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public partial class NestedPlatformControlGalleryPage : ContentPage
	{
		public new StackLayout Layout { get; set; }

		public bool PlatformControlsAdded { get; set; }

		public const string ReadyForPlatformControlsMessage = "ReadyForPlatformControls";

		protected override void OnAppearing()
		{
			base.OnAppearing();
			MessagingCenter.Send(this, ReadyForPlatformControlsMessage);
		}

		public NestedPlatformControlGalleryPage()
		{
			Layout = new StackLayout { Padding = 20, VerticalOptions = LayoutOptions.FillAndExpand };

			Content = new ScrollView { Content = Layout };

			var label = new Label { Text = "There should be some native controls right below this", FontSize = 12 };

			var testLabel = new Label { Text = "Forms Label", FontSize = 14 };
			var button = new Button { Text = "Resize Forms Label", HeightRequest = 80 };
			double originalSize = testLabel.FontSize;
			button.Clicked += (sender, args) => { testLabel.FontSize = testLabel.FontSize == originalSize ? 24 : 14; };

			Layout.Children.Add(testLabel);
			Layout.Children.Add(button);
			Layout.Children.Add(label);
		}
	}
}