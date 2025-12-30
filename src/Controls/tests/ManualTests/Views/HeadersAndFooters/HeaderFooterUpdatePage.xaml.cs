namespace Microsoft.Maui.ManualTests.Views
{
	public partial class HeaderFooterUpdatePage : ContentPage
	{
		int count = 0;
		public HeaderFooterUpdatePage()
		{
			InitializeComponent();
		}
		private void OnCounterClicked(object sender, EventArgs e)
		{
			count++;
			CounterLabel.Text = $"CollectionView Header {count}";

			SemanticScreenReader.Announce(CounterLabel.Text);
		}
	}
}