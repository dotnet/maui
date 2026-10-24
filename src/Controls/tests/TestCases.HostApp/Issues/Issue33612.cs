using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33612, "Inconsistent Accessibility Behavior Across Platforms - a container with SemanticProperties.Description/Hint should not collapse its independently-accessible children", PlatformAffected.All)]
public class Issue33612 : ContentPage
{
	public ObservableCollection<SuggestionItem> Items { get; } = new()
	{
		new SuggestionItem("Item1", "First item"),
		new SuggestionItem("Item2", "Second item"),
		new SuggestionItem("Item3", "Third item"),
	};

	Label _tappedItemLabel;

	public Issue33612()
	{
		BindingContext = this;

		_tappedItemLabel = new Label
		{
			AutomationId = "TappedItemLabel",
			Text = "None"
		};

		var itemsLayout = new VerticalStackLayout
		{
			AutomationId = "SuggestionsItemsLayout"
		};
		itemsLayout.SetBinding(BindableLayout.ItemsSourceProperty, new Binding(nameof(Items)));
		BindableLayout.SetItemTemplate(itemsLayout, CreateItemTemplate());

		// Outer structural container: has its own Description ("Suggestions") but must not hide
		// the individually-accessible items inside it (root cause of #33612).
		var suggestionsContainer = new ContentView
		{
			AutomationId = "SuggestionsContainer",
			Content = new ScrollView { Content = itemsLayout }
		};
		SemanticProperties.SetDescription(suggestionsContainer, "Suggestions");

		Content = new VerticalStackLayout
		{
			Children = { suggestionsContainer, _tappedItemLabel }
		};
	}

	DataTemplate CreateItemTemplate()
	{
		return new DataTemplate(() =>
		{
			var label = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			label.SetBinding(Label.TextProperty, new Binding(nameof(SuggestionItem.Text)));

			// Each item is its own actionable unit: it owns a gesture recognizer, so it should
			// remain a normal leaf accessibility element carrying its own Description/Hint,
			// rather than being treated as a passive structural wrapper.
			var border = new Border
			{
				Padding = 12,
				Margin = 6,
				Content = label
			};
			// AutomationId is intentionally distinct from the visible text so tests can locate the
			// Border unambiguously without matching the inner Label or a phantom accessibility element.
			border.SetBinding(AutomationIdProperty, new Binding(nameof(SuggestionItem.AutomationId)));
			border.SetBinding(SemanticProperties.DescriptionProperty, new Binding(nameof(SuggestionItem.Text)));
			SemanticProperties.SetHint(border, "Double tap to activate");

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += OnSuggestionTapped;
			border.GestureRecognizers.Add(tapGestureRecognizer);

			return border;
		});
	}

	async void OnSuggestionTapped(object sender, TappedEventArgs e)
	{
		if (sender is Border tappedBorder && tappedBorder.BindingContext is SuggestionItem item)
		{
			_tappedItemLabel.Text = item.Text;
		}

		await DisplayAlert("Suggestion Selected", "is a suggestion tap event", "OK");
	}

	public record SuggestionItem(string AutomationId, string Text);
}
