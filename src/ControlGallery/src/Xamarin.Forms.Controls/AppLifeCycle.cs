namespace Microsoft.Maui.Controls.ControlGallery
{
	public class AppLifeCycle : Application
	{
		public AppLifeCycle()
		{
			MainPage = new ContentPage
			{
				Content = new Label
				{
					Text = "Testing Lifecycle events"
				}
			};
		}
	}
}