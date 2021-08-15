using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class TemplatePage
	{
		public TemplatePage()
		{
			InitializeComponent();

			HTMLLabel.Text = "<h1>Test</h1><p style=\"color:red\">red</p>";
		}

		int count = 0;
		private void OnCounterClicked(object sender, EventArgs e)
		{
			count++;
			CounterLabel.Text = $"Current count: {count}";
		}
	}
}