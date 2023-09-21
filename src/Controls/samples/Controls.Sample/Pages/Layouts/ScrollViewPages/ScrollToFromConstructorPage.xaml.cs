namespace Maui.Controls.Sample.Pages.ScrollViewPages
{
	public partial class ScrollToFromConstructorPage
	{
		public ScrollToFromConstructorPage()
		{
			InitializeComponent();

			ScrollToAsync();
		}

		public async void ScrollToAsync()
		{
			await ScrollView.ScrollToAsync(0, 1000, false);
		}
	}
}