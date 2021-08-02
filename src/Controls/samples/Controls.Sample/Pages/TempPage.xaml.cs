using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class TempPage
	{
		public TempPage()
		{
			InitializeComponent();
		}

		int count = 0;
		private void OnCounterClicked(object sender, EventArgs e)
		{
			count++;
			CounterLabel.Text = $"Current count: {count}";
		}
	}
}