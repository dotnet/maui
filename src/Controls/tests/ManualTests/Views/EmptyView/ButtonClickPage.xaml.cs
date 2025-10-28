namespace Microsoft.Maui.ManualTests.Views
{
	public partial class ButtonClickPage : ContentPage
	{
		int count = 0;
		public ButtonClickPage()
		{
			InitializeComponent();
		}
		private void OnCounterClicked(object sender, EventArgs e)
		{
			count++;
			CounterLabel.Text = $"Current count: {count}";

			SemanticScreenReader.Announce(CounterLabel.Text);
		}
	}
}