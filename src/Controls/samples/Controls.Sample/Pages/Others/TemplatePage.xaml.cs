using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class TemplatePage
	{
		public TemplatePage()
		{
			InitializeComponent();
		}

		int count = 0;
		private void OnCounterClicked(object? sender, EventArgs e)
		{
			count++;
			CounterLabel.Text = $"Current count: {count}";
		}
	}
}