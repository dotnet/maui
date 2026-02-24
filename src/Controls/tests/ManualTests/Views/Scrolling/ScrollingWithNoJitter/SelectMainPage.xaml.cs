namespace Microsoft.Maui.ManualTests.Views
{
	public partial class SelectMainPage : ContentPage
	{
		public SelectMainPage()
		{
			InitializeComponent();
		}
		private void OnCounterClicked(object sender, EventArgs e)
		{
			this.Navigation.PushAsync(new StaticHeightPage());
		}

		private void OnScrollClicked(object sender, EventArgs e)
		{
			this.Navigation.PushAsync(new DynamicHeightPage());
		}
	}
}