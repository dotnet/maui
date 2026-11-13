namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38799, "Replacing focused Entry content on a Shell page", PlatformAffected.Android)]
public class Issue38799 : TestShell
{
	protected override void Init()
	{
		var numericLabel = new Label { Text = "1001", Padding = 10, AutomationId = "NumericCell" };
		var textLabel = new Label { Text = "James", Padding = 10, AutomationId = "TextCell" };
		var numericContent = new ContentView { Content = numericLabel, WidthRequest = 200, BackgroundColor = Colors.LightBlue };
		var textContent = new ContentView { Content = textLabel, WidthRequest = 200, BackgroundColor = Colors.LightBlue };
		var result = new Label { AutomationId = "FocusResult", Text = "Waiting" };
		var nativeState = new Label { AutomationId = "NativeState", Text = "Waiting" };
		Entry numericEntry = null;
		Entry textEntry = null;
		int replacements = 0;

		Entry CreateEntry(string automationId, Keyboard keyboard)
		{
			var entry = new Entry { AutomationId = automationId, Keyboard = keyboard, Margin = new Thickness(10, 0) };
			entry.Focused += (_, _) =>
			{
#if ANDROID
				var platformView = entry.Handler?.PlatformView as Android.Views.View;
				var editor = platformView?.FindFocus() as Android.Widget.EditText;
				var expectedClass = keyboard == Keyboard.Numeric
					? Android.Text.InputTypes.ClassNumber : Android.Text.InputTypes.ClassText;
				nativeState.Text = editor is not null &&
					(editor.InputType & Android.Text.InputTypes.MaskClass) == expectedClass
					? $"{automationId}: Native focus and input type correct"
					: $"{automationId}: Native focus or input type incorrect";
#endif
			};
			return entry;
		}

		// Record focus loss *during* replacement, not HasFocus after removal:
		// Android can clear focus as a side effect of removing all the children.
		// That is too late to relinquish IME ownership while still in the hierarchy.
		bool replacing = false;
		bool lostFocusBeforeRemoval = false;
		bool hadNativeFocus = false;

		var numericTap = new TapGestureRecognizer();
		numericTap.Tapped += async (_, _) =>
		{
			if (numericContent.Content is Entry)
				return;

			if (textContent.Content is Entry)
			{
				hadNativeFocus = false;
				lostFocusBeforeRemoval = false;
				textLabel.Text = textEntry.Text;
#if ANDROID
				var nativeEntry = textEntry.Handler?.PlatformView as Android.Views.View;
				hadNativeFocus = nativeEntry?.HasFocus == true;
#endif
				replacing = true;
				textContent.Content = textLabel;
				replacing = false;
				replacements++;
			}

			numericEntry ??= CreateEntry("NumericEntry", Keyboard.Numeric);
			numericEntry.Text = numericLabel.Text;
			nativeState.Text = "Waiting for numeric focus";
			await Task.Delay(250);
			numericEntry.Focus();
			numericContent.Content = numericEntry;
			result.Text = !hadNativeFocus ? $"{replacements}: Text Entry never had native focus"
				: lostFocusBeforeRemoval ? $"{replacements}: Focus relinquished before removal"
				: $"{replacements}: Focus not relinquished before removal";
		};
		numericContent.GestureRecognizers.Add(numericTap);

		var textTap = new TapGestureRecognizer();
		textTap.Tapped += async (_, _) =>
		{
			if (textContent.Content is Entry)
				return;

			if (numericContent.Content is Entry)
			{
				numericLabel.Text = numericEntry.Text;
				numericContent.Content = numericLabel;
			}

			if (textEntry is null)
			{
				textEntry = CreateEntry("TextEntry", Keyboard.Text);
#if ANDROID
				textEntry.HandlerChanged += (_, _) =>
				{
					if (textEntry.Handler?.PlatformView is Android.Views.View nativeEntry)
					{
						nativeEntry.FocusChange += (_, args) =>
						{
							if (!args.HasFocus && replacing &&
								textContent.Handler?.PlatformView is Android.Views.ViewGroup parent)
							{
								// No focus manipulation here: observe whether the outgoing
								// editor is still a child when Android notifies focus loss.
								lostFocusBeforeRemoval = parent.IndexOfChild(nativeEntry) >= 0;
							}
						};
					}
				};
#endif
			}

			textEntry.Text = textLabel.Text;
			nativeState.Text = "Waiting for text focus";
			await Task.Delay(250);
			textEntry.Focus();
			textContent.Content = textEntry;
		};
		textContent.GestureRecognizers.Add(textTap);

		var grid = new Grid
		{
			RowSpacing = 10,
			RowDefinitions = { new RowDefinition(50), new RowDefinition(50), new RowDefinition(50), new RowDefinition(50) }
		};
		grid.Add(numericContent, 0, 0);
		grid.Add(textContent, 0, 1);
		grid.Add(result, 0, 2);
		grid.Add(nativeState, 0, 3);
		AddContentPage(new ContentPage { Content = grid });
	}
}
