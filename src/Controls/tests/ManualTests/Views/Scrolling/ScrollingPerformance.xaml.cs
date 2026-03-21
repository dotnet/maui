namespace Microsoft.Maui.ManualTests.Views
{
	public partial class ScrollingPerformance : ContentPage
	{
		public ScrollingPerformance()
		{
			InitializeComponent();
			collection.ItemsSource = Enumerable.Range(0, 2000).ToList();
		}
		void Button_Clicked(System.Object sender, System.EventArgs e)
		{
			collection.HeightRequest = 500.5d;
		}

		void Button_Clicked_1(System.Object sender, System.EventArgs e)
		{
			collection.HeightRequest = 400.0d;
		}
	}
}