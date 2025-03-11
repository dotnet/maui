namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24977, "Keyboard Scrolling in editors with Center or End VerticalTextAlignment is off", PlatformAffected.iOS)]
public class Issue24977 : TestNavigationPage
{
	protected override void Init()
	{
		var root = CreateRootContentPage();
		PushAsync(root);
	}

	ContentPage CreateRootContentPage()
	{
		var ContentPage = new ContentPage();
		VerticalStackLayout rootLayout = new VerticalStackLayout
		{
			Spacing = 10,
			Padding = new Thickness(10),
		};

		Button centerButton = new Button() { Text = "Center", AutomationId = "CenterButton" };
		centerButton.Clicked += (s, e) => Navigation.PushAsync(new Issue24977_1());
		Button endButton = new Button() { Text = "End", AutomationId = "EndButton" };
		endButton.Clicked += (s, e) => Navigation.PushAsync(new Issue24977_2());
		rootLayout.Add(centerButton);
		rootLayout.Add(endButton);
		ContentPage.Content = rootLayout;
		return ContentPage;
	}
}

public class Issue24977_1 : TestContentPage
{
	Label CursorHeightTracker;
	internal Editor editor;
	protected override void Init()
	{
		Grid rootGrid = new Grid
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

		Entry entry = new Entry()
		{
			Text = "Content before",
			AutomationId = "EntryBefore",
			ReturnType = ReturnType.Next,
			BackgroundColor = Colors.Aquamarine,
		};

		CursorHeightTracker = new Label()
		{
			Text = "0",
			AutomationId = "CursorHeightTracker",
			BackgroundColor = Colors.Aquamarine,
		};

		editor = new Editor()
		{
			Text = "Hello World!",
			AutomationId = "IssueEditor",
			BackgroundColor = Colors.Orange,
			VerticalTextAlignment = TextAlignment.Center,
		};
		editor.TextChanged += Editor_TextChanged;

		Button button = new Button() { Text = "Erase" };
		button.Clicked += (s, e) => editor.Text = string.Empty;

		rootGrid.Add(entry, 0, 0);
		rootGrid.Add(CursorHeightTracker, 0, 1);
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

			CursorHeightTracker.Text = cursorInWindow.Y.ToString();

		}

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

	T FindResponder<T>(UIKit.UIView view) where T : UIKit.UIResponder
	{
		var nextResponder = view as UIKit.UIResponder;
		while (nextResponder is not null)
		{
			nextResponder = nextResponder.NextResponder;

			if (nextResponder is T responder)
				return responder;
		}
		return null;
#endif
	}
}

public class Issue24977_2 : Issue24977_1
{
	protected override void Init()
	{
		base.Init();
		editor.VerticalTextAlignment = TextAlignment.End;
	}
}