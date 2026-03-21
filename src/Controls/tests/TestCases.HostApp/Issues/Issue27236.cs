namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27236, "android allows type into hidden Entry control", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue27236 : TestContentPage
	{
		protected override void Init()
		{
			StackLayout mainLayout = new StackLayout
			{
				Padding = new Thickness(10)
			};

			Entry toggleableEntry = new Entry
			{
				AutomationId = "ToggleableEntry",
				IsVisible = false
			};

			Editor toggleableEditor = new Editor
			{
				AutomationId = "ToggleableEditor",
				IsVisible = false
			};

			Button toggleEntryVisibilityButton = new Button
			{
				AutomationId = "ToggleEntryVisibilityButton",
				Text = "Toggle Entry Visibility"
			};

			Button toggleEditorVisibilityButton = new Button
			{
				AutomationId = "ToggleEditorVisibilityButton",
				Text = "Toggle Editor Visibility"
			};

			toggleEntryVisibilityButton.Clicked += (sender, e) => ToggleVisibility(toggleableEntry);
			toggleEditorVisibilityButton.Clicked += (sender, e) => ToggleVisibility(toggleableEditor);

			void ToggleVisibility(VisualElement element)
			{
				element.IsVisible = !element.IsVisible;
			}

			mainLayout.Children.Add(toggleableEntry);
			mainLayout.Children.Add(toggleableEditor);
			mainLayout.Children.Add(toggleEntryVisibilityButton);
			mainLayout.Children.Add(toggleEditorVisibilityButton);

			Content = mainLayout;
		}
	}
}
