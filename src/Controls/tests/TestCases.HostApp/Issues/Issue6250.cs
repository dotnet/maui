namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6250, "[Android] IsReadOnly=true EditText controls are not focusable", PlatformAffected.Android)]
public class Issue6250 : ContentPage
{
	public Issue6250()
	{
		var layout = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				new Entry { AutomationId = "EditableEntry", Placeholder = "Editable Entry" },
			}
		};

		var focusButtons = new HorizontalStackLayout { Spacing = 10 };
		var focusedInput = new Label { AutomationId = "FocusedInput" };

		InputView[] readOnlyInputs =
		{
			new Entry { AutomationId = "ReadOnlyEntry", Text = "Read-only Entry", IsReadOnly = true },
			new Editor { AutomationId = "ReadOnlyEditor", Text = "Read-only Editor", IsReadOnly = true, HeightRequest = 100 },
			new SearchBar { AutomationId = "ReadOnlySearchBar", Text = "Read-only SearchBar", IsReadOnly = true },
		};

		foreach (var input in readOnlyInputs)
		{
			input.Focused += (_, _) => focusedInput.Text = input.AutomationId;
			layout.Add(input);
			focusButtons.Add(new Button { AutomationId = $"Focus{input.AutomationId}", Text = input.GetType().Name, Command = new Command(() => input.Focus()) });
		}

		layout.Add(focusButtons);
		layout.Add(focusedInput);
		Content = layout;
	}
}
