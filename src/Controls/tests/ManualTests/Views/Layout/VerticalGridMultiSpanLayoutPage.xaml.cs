namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalGridMultiSpanLayoutPage : ContentPage
	{
		public VerticalGridMultiSpanLayoutPage()
		{
			InitializeComponent();
		}

		public int ColumnCount { get; set; } = 3;
		private void UpdateColumnCount(object sender, EventArgs e)
		{
			ColumnCount++;
			Dispatcher.Dispatch(() =>
			{
				OnPropertyChanged("ColumnCount");
			});
		}
	}
}

