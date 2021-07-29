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

		async void OnButtonClicked(object sender, EventArgs e)
		{
			_counter++;
			string message = $"You clicked {_counter} times!";
			CounterLabel.Text = message;
			await DisplayAlert("Alert", message, "Ok");
		}
	}
}