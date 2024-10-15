﻿namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1469, "Setting SelectedItem to null inside ItemSelected event handler does not work", PlatformAffected.UWP)]
	public class Issue1469 : TestContentPage
	{
		const string Go = "Select 3rd item";
		const string Back = "Clear selection";
		const string Success = "Success";
		const string Fail = "Fail";
		protected override void Init()
		{
			var statusLabel = new Label() { FontSize = 40 };
			var _items = Enumerable.Range(1, 4).Select(i => $"Item {i}").ToArray();
			var list = new ListView()
			{
				ItemsSource = _items
			};

			list.ItemSelected += async (_, e) =>
			{
				if (e.SelectedItem == null)
					return;
				list.SelectedItem = null;

				statusLabel.Text = "One moment please...";
				await Task.Delay(500);
				statusLabel.Text = list.SelectedItem == null ? Success : Fail;
			};

			Content = new StackLayout
			{
				Children = {
					new Label { Text = "If you click an item in the list it should not become selected" },
					new Button { AutomationId = Go, Text = Go, Command = new Command(() => list.SelectedItem = _items[2]) },
					new Button { AutomationId = Back, Text = Back, Command = new Command(() => list.SelectedItem = list.SelectedItem = null) },
					statusLabel,
					list
				}
			};
		}
	}
}