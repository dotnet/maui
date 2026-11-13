using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "CollectionView AnchorMode End keeps the end in view", PlatformAffected.All)]
public class CollectionViewAnchorModeEnd : ContentPage
{
	// The footer ends the content, below the last item. The reference outside the list is as tall, so a test can tell the
	// footer is fully in view even where the platform reports only the part of an element that's on screen.
	const double FooterHeight = 40;

	readonly ObservableCollection<AnchorModeMessage> _messages = new();
	readonly CollectionView _collectionView;
	int _count;

	public CollectionViewAnchorModeEnd()
	{
		for (var i = 0; i < 30; i++)
		{
			AddMessage();
		}

		_collectionView = new CollectionView
		{
			AutomationId = "MessagesCollectionView",
			AnchorMode = ScrollAnchorMode.End,
			ItemsSource = _messages,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { Padding = new Thickness(12, 8), LineBreakMode = LineBreakMode.WordWrap };
				label.SetBinding(Label.TextProperty, nameof(AnchorModeMessage.Text));
				label.SetBinding(AutomationIdProperty, nameof(AnchorModeMessage.Id));
				return label;
			}),
			Footer = new Label
			{
				AutomationId = "MessagesFooter",
				Text = "Footer",
				HeightRequest = FooterHeight,
				VerticalTextAlignment = TextAlignment.Center,
				BackgroundColor = Colors.LightGray,
			},
		};

		var isAnchoredLabel = new Label { AutomationId = "IsAnchoredLabel", VerticalTextAlignment = TextAlignment.Center };
		isAnchoredLabel.SetBinding(Label.TextProperty, new Binding(nameof(StructuredItemsView.IsAnchored), source: _collectionView));

		var footerHeightReference = new Label
		{
			AutomationId = "FooterHeightReference",
			Text = "Reference",
			HeightRequest = FooterHeight,
			VerticalTextAlignment = TextAlignment.Center,
		};

		var status = new HorizontalStackLayout { Spacing = 12, Children = { isAnchoredLabel, footerHeightReference } };

		var buttons = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)),
			RowDefinitions = new RowDefinitionCollection(new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)),
		};
		buttons.Add(CreateButton("AddButton", "Add", AddMessage), 0, 0);
		buttons.Add(CreateButton("GrowButton", "Grow last", GrowLastMessage), 1, 0);
		buttons.Add(CreateButton("ScrollToFirstButton", "Scroll to first", () => _collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false)), 0, 1);
		buttons.Add(CreateButton("JumpToLatestButton", "Jump to latest", () => _collectionView.ScrollToEnd()), 1, 1);

		var grid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection(new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star)),
		};
		grid.Add(buttons, 0, 0);
		grid.Add(status, 0, 1);
		grid.Add(_collectionView, 0, 2);
		Content = grid;
	}

	static Button CreateButton(string automationId, string text, Action action) =>
		new() { AutomationId = automationId, Text = text, Command = new Command(action) };

	void AddMessage()
	{
		_count++;
		_messages.Add(new AnchorModeMessage($"Message{_count}", $"Message {_count}"));
	}

	void GrowLastMessage()
	{
		if (_messages.Count > 0)
		{
			_messages[^1].Text += "\nAnother line of the answer as it streams in.";
		}
	}
}

public class AnchorModeMessage(string id, string text) : INotifyPropertyChanged
{
	string _text = text;

	public event PropertyChangedEventHandler PropertyChanged;

	public string Id { get; } = id;

	public string Text
	{
		get => _text;
		set
		{
			_text = value;
			OnPropertyChanged();
		}
	}

	void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
