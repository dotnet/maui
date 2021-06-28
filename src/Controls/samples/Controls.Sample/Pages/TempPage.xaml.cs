using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class TempPage
	{
		int _counter = 0;

		public TempPage()
		{
			InitializeComponent();
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			_counter++;
			CounterLabel.Text = $"You clicked {_counter} times!";
		}
	}
}