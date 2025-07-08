using Microsoft.Maui.Controls;
using System;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "30066", "DatePicker CharacterSpacing Property Not Working on Windows", PlatformAffected.UWP)]
public class Issue30066 : TestShell
{
	protected override void Init()
	{
		var shellContent = new ShellContent
		{
			Title = "DatePicker CharacterSpacing Test",
			Content = new Issue30066ContentPage() { Title = "DatePicker CharacterSpacing Test" }
		};

		Items.Add(shellContent);
	}

	class Issue30066ContentPage : ContentPage
	{
		public Issue30066ContentPage()
		{
			var datePicker = new DatePicker
			{
				Date = DateTime.Today,
				CharacterSpacing = 10,
				AutomationId = "TestDatePicker",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};

			var label = new Label
			{
				Text = "The DatePicker above should have character spacing of 10 applied to its text.",
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(20),
				HorizontalTextAlignment = TextAlignment.Center
			};

			var button = new Button
			{
				Text = "Change Character Spacing to 20",
				AutomationId = "ChangeSpacingButton",
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 20)
			};

			button.Clicked += (s, e) =>
			{
				datePicker.CharacterSpacing = datePicker.CharacterSpacing == 10 ? 20 : 10;
				button.Text = $"Change Character Spacing to {(datePicker.CharacterSpacing == 10 ? 20 : 10)}";
			};

			Content = new StackLayout
			{
				Children = { datePicker, label, button },
				Spacing = 20,
				Margin = new Thickness(20)
			};
		}
	}
}