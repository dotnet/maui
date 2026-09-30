using System.Diagnostics;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34396, "[iOS, MacCatalyst] UI freezes when adding a large number of Editors to a layout", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34396 : ContentPage
{
	public record struct Widget(double X, double Y, double W, double H);
	readonly List<(Editor Editor, Widget Item)> _items = new();

	readonly AbsoluteLayout _canvas;
	readonly Label _statusLabel;
	readonly Button _clickedButton;

	int _count;

	public Issue34396()
	{
		var addButton = new Button
		{
			Text = "Add 200 Editors",
			AutomationId = "AddEditorsButton"
		};
		addButton.Clicked += OnAddEditorsClicked;

		_clickedButton = new Button
		{
			Text = "Clicked 0",
			AutomationId = "ClickedButton"
		};
		_clickedButton.Clicked += Button_Clicked;

		var clearButton = new Button
		{
			Text = "Clear",
			AutomationId = "ClearButton"
		};
		clearButton.Clicked += OnClearClicked;

		_statusLabel = new Label
		{
			Text = "Ready",
			AutomationId = "StatusLabel34396"
		};

		_canvas = new AbsoluteLayout
		{
			WidthRequest = 2000,
			HeightRequest = 3000,
			BackgroundColor = Color.FromArgb("#202020")
		};

		var buttons = new HorizontalStackLayout
		{
			Spacing = 8,
			Children = { addButton, _clickedButton, clearButton }
		};

		var grid = new Grid
		{
			Padding = 12,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			}
		};

		grid.Add(buttons, 0, 0);
		grid.Add(_statusLabel, 0, 1);
		grid.Add(new ScrollView { Content = _canvas }, 0, 2);

		Content = grid;
	}

	void PrepareItems(int count = 200)
	{
		_items.Clear();

		for (int i = 0; i < count; i++)
		{
			double x = (i * 12) % 1800;
			double y = (i * 15) % 2800;

			var editor = new Editor
			{
				Text = $"Item {i}",
				FontSize = 12,
				BackgroundColor = Colors.Gray,
				TextColor = Colors.White,
				IsReadOnly = true
			};

			_items.Add((editor, new Widget(x, y, 120, 30)));
		}

		_statusLabel.Text = $"Prepared {_items.Count} editors.";
	}

	void OnAddEditorsClicked(object sender, EventArgs e)
	{
		PrepareItems();

		var stopwatch = Stopwatch.StartNew();

		foreach (var item in _items)
		{
			Dispatcher.Dispatch(() =>
			{
				_canvas.Children.Add(item.Editor);
				AbsoluteLayout.SetLayoutBounds(
					item.Editor,
					new Rect(item.Item.X, item.Item.Y, item.Item.W, item.Item.H));
			});
		}

		_statusLabel.Text = "Processing…";

		Dispatcher.Dispatch(() =>
		{
			stopwatch.Stop();
			_statusLabel.Text = $"Completed:{stopwatch.ElapsedMilliseconds}";
		});
	}

	void OnClearClicked(object sender, EventArgs e)
	{
		_canvas.Children.Clear();
		_items.Clear();
		_statusLabel.Text = "Cleared.";
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		_count += 1;
		_clickedButton.Text = "Clicked " + _count.ToString();
	}
}
