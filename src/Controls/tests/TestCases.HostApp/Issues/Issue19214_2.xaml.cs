namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19214_2, "iOS Editor Cursor stays above keyboard - Top level Grid", PlatformAffected.iOS)]
public partial class Issue19214_2 : ContentPage
{
	public Issue19214_2()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		editor.Text = string.Empty;
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
