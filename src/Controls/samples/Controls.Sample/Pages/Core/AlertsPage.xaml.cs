using System;
using System.Diagnostics;
using Microsoft.Maui;

namespace Maui.Controls.Sample.Pages
{
	public partial class AlertsPage
	{
		public AlertsPage()
		{
			InitializeComponent();
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await DisplayAlertAsync("Alert", "Welcome to the Alerts Page", "Hello!");
		}

		async void OnAlertSimpleClicked(object sender, EventArgs e)
		{
			await DisplayAlertAsync("Alert", "You have been alerted", "OK");
		}

		async void OnAlertYesNoClicked(object sender, EventArgs e)
		{
			var answer = await DisplayAlertAsync("Question?", "Would you like to play a game", "Yes", "No");
			Debug.WriteLine("Answer: " + answer);
		}

		async void OnActionSheetSimpleClicked(object sender, EventArgs e)
		{
			var action = await DisplayActionSheetAsync("ActionSheet: Send to?", "Cancel", null, "Email", "Twitter", "Facebook");
			Debug.WriteLine("Action: " + action);
		}

		async void OnActionSheetCancelDeleteClicked(object sender, EventArgs e)
		{
			var action = await DisplayActionSheetAsync("ActionSheet: SavePhoto?", "Cancel", "Delete", "Photo Roll", "Email");
			Debug.WriteLine("Action: " + action);
		}

		async void OnQuestion1ButtonClicked(object sender, EventArgs e)
		{
			string result = await DisplayPromptAsync("Question 1", "What's your name?", initialValue: string.Empty);

			if (!string.IsNullOrWhiteSpace(result))
			{
				question1ResultLabel.Text = $"Hello {result}.";
			}
		}

		async void OnQuestion2ButtonClicked(object sender, EventArgs e)
		{
			string result = await DisplayPromptAsync("Question 2", "What's 5 + 5?", initialValue: "10", maxLength: 2, keyboard: Keyboard.Numeric);

			if (!string.IsNullOrWhiteSpace(result))
			{
				int number = Convert.ToInt32(result);
				question2ResultLabel.Text = number == 10 ? "Correct." : "Incorrect.";
			}
		}
	}
}