﻿namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2339, "Picker not shown when .Focus() is called")]
	public class Issue2339 : TestContentPage
	{
		protected override void Init()
		{
			var picker = new Picker { Items = { "One", "Two", "Three" } };
			var pickerBtn = new Button
			{
				Text = "Click me to call .Focus on Picker",
				AutomationId = "btnFocus"
			};

			pickerBtn.Clicked += (sender, args) =>
			{
				picker.Focus();
			};

			var pickerBtn2 = new Button
			{
				Text = "Click me to call .Unfocus on Picker",
				AutomationId = "btnUnFocus"
			};

			pickerBtn2.Clicked += (sender, args) =>
			{
				picker.Unfocus();
			};

			var pickerBtn3 = new Button
			{
				Text = "Click me to .Focus () picker, wait 2 seconds, and .Unfocus () picker",
				Command = new Command(async () =>
				{
					picker.Focus();
					await Task.Delay(2000);
					picker.Unfocus();
				}),
				AutomationId = "btnFocusThenUnFocus"
			};

			var focusFiredCount = 0;
			var unfocusFiredCount = 0;

			var focusFiredLabel = new Label { Text = "Picker Focused: " + focusFiredCount };
			var unfocusedFiredLabel = new Label { Text = "Picker UnFocused: " + unfocusFiredCount };

			picker.Focused += (s, e) =>
			{
				focusFiredCount++;
				focusFiredLabel.Text = "Picker Focused: " + focusFiredCount;
			};
			picker.Unfocused += (s, e) =>
			{
				unfocusFiredCount++;
				unfocusedFiredLabel.Text = "Picker UnFocused: " + unfocusFiredCount;
			};

			Content = new StackLayout
			{
				Children = {
					focusFiredLabel,
					unfocusedFiredLabel,
					pickerBtn,
					pickerBtn2,
					pickerBtn3,
					picker
				}
			};
		}
	}
}
