namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24977, "Keyboard Scrolling in editors with Center or End VerticalTextAlignment is off", PlatformAffected.iOS)]
public class Issue24977 : TestNavigationPage
{
	protected override void Init()
	{
		PushAsync(new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Spacing = 12,
				Padding = new Thickness(12, 24),
				Children =
				{
					CreateButton(TextAlignment.Start),
					CreateButton(TextAlignment.Center),
					CreateButton(TextAlignment.End),
				}
			}
		});
	}

	Button CreateButton(TextAlignment textAlignment)
	{
		var btn = new Button
		{
			Text = textAlignment.ToString(),
			AutomationId = $"{textAlignment}Button"
		};
		btn.Clicked += (s, e) => Navigation.PushAsync(new Issue24977_1(textAlignment));
		return btn;
	}
}

public class Issue24977_1 : TestContentPage
{
	Editor editor;
	Label cursorHeightTracker;

	public Issue24977_1(TextAlignment textAlignment)
	{
		editor.VerticalTextAlignment = textAlignment;
	}

	protected override void Init()
	{
		var rootGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = 50 },
				new RowDefinition { Height = 50 },
				new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
				new RowDefinition { Height = 50 }
			},
			Margin = new Thickness(30)
		};

		var entry = new Entry
		{
			Text = "Content before",
			AutomationId = "EntryBefore",
			ReturnType = ReturnType.Next,
			BackgroundColor = Colors.Aquamarine,
		};

		cursorHeightTracker = new Label
		{
			Text = "0",
			AutomationId = "CursorHeightTracker",
			BackgroundColor = Colors.Aquamarine,
		};

		editor = new Editor
		{
			Text = "Hello World!",
			AutomationId = "IssueEditor",
			BackgroundColor = Colors.Orange,
		};
		editor.TextChanged += Editor_TextChanged;

		var button = new Button { Text = "Erase" };
		button.Clicked += (s, e) => editor.Text = string.Empty;

		rootGrid.Add(entry, 0, 0);
		rootGrid.Add(cursorHeightTracker, 0, 1);
		rootGrid.Add(editor, 0, 2);

		rootGrid.Add(button, 0, 3);

		Content = rootGrid;
	}

	private void Editor_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is Editor editor)
		{
			AddCursorHeightToLabel(editor);
		}
	}

	void AddCursorHeightToLabel(Editor editor)
	{
#if IOS
		var textInput = editor.Handler.PlatformView as UIKit.UITextView;
		var selectedTextRange = textInput?.SelectedTextRange;
		var localCursor = selectedTextRange is not null ? textInput?.GetCaretRectForPosition(selectedTextRange.Start) : null;

		if (localCursor is CoreGraphics.CGRect local && textInput is not null)
		{
			var container = GetContainerView(textInput);
			var cursorInContainer = container.ConvertRectFromView(local, textInput);
			var cursorInWindow = container.ConvertRectToView(cursorInContainer, null);

			cursorHeightTracker.Text = cursorInWindow.Y.ToString();
		}

		UIKit.UIView GetContainerView(UIKit.UIView startingPoint)
		{
			var rootView = FindResponder<Microsoft.Maui.Platform.ContainerViewController>(startingPoint)?.View;

			if (rootView is not null)
			{
				return rootView;
			}

			return null;
		}

		T FindResponder<T>(UIKit.UIView view)
			where T : UIKit.UIResponder
		{
			var nextResponder = view as UIKit.UIResponder;
			while (nextResponder is not null)
			{
				nextResponder = nextResponder.NextResponder;

				if (nextResponder is T responder)
					return responder;
			}
			return null;
		}
#endif
	}
}
