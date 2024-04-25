using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 1, "Application.Current.Dispatcher.IsDispatchRequired race condition causes crash")]
public class IsInvokeRequiredRaceCondition : TestContentPage
{
	protected override void Init()
	{
		var button = new Button
		{
			AutomationId = "crashButton",
			Text = "Start Test"
		};

		var success = new Label { Text = "Success", IsVisible = false, AutomationId = "successLabel" };

		var instructions = new Label { Text = "Click the Start Test button. " };

		Content = new StackLayout
		{
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
			Children = { instructions, success, button }
		};

		button.Clicked += async (sender, args) =>
		{
			await Task.WhenAll(GenerateTasks());
			success.IsVisible = true;
		};
	}

	List<Task> GenerateTasks()
	{
		var result = new List<Task>();

		for (int n = 0; n < 1000; n++)
		{
			result.Add(Task.Run(() => { var t = Application.Current.Dispatcher.IsDispatchRequired; }));
		}

		return result;
	}

}