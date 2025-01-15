namespace Maui.Controls.Sample
{
	internal class NoInternetConnectionPage : ContentPage
	{
		public NoInternetConnectionPage()
		{
			Content = new Label()
			{
				Text = "This device doesn't have internet access",
				AutomationId = "NoInternetAccessLabel",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
		}
	}
}