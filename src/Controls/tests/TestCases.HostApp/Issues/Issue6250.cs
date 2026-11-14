namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6250, "[Android] IsReadOnly=true EditText controls are not focusable", PlatformAffected.Android)]
public class Issue6250 : ContentPage
{
	public Issue6250()
	{
		var focusedInput = new Label { AutomationId = "FocusedInput", Text = "None" };

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				focusedInput,
				CreateFocusRow(new Entry { AutomationId = "EditableEntry", Placeholder = "Editable Entry" }),
				CreateFocusRow(new Entry { AutomationId = "ReadOnlyEntry", Text = "Read-only Entry", IsReadOnly = true }),
				CreateFocusRow(new Editor { AutomationId = "ReadOnlyEditor", Text = "Read-only Editor", IsReadOnly = true, HeightRequest = 100 }),
				CreateFocusRow(new SearchBar { AutomationId = "ReadOnlySearchBar", Text = "Read-only SearchBar", IsReadOnly = true }),
			}
		};

		Grid CreateFocusRow(View input)
		{
			input.Focused += (_, _) => focusedInput.Text = input.AutomationId;
			var focusButton = new Button { AutomationId = $"Focus{input.AutomationId}", Text = "Focus" };
			focusButton.Clicked += (_, _) => input.Focus();
			var row = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Auto },
				}
			};
			row.Add(input);
			row.Add(focusButton, 1);
			return row;
		}
	}
}
