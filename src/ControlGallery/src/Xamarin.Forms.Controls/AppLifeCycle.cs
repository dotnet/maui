namespace Xamarin.Forms.Controls
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