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
		Entry numericEntry = null;
		Entry textEntry = null;

		// Record focus loss *during* replacement, not HasFocus after removal:
		// Android can clear focus as a side effect of removing all the children.
		// That is too late to relinquish IME ownership while still in the hierarchy.
		bool replacing = false;
		bool lostFocusBeforeRemoval = false;
		bool hadNativeFocus = false;

		var numericTap = new TapGestureRecognizer();
		numericTap.Tapped += async (_, _) =>
		{
			if (textContent.Content is Entry)
			{
				textLabel.Text = textEntry.Text;
#if ANDROID
				var nativeEntry = textEntry.Handler?.PlatformView as Android.Views.View;
				hadNativeFocus = nativeEntry?.HasFocus == true;
#endif
				replacing = true;
				textContent.Content = textLabel;
				replacing = false;
			}

			numericEntry ??= new Entry { AutomationId = "NumericEntry", Keyboard = Keyboard.Numeric, Margin = new Thickness(10, 0) };
			numericEntry.Text = numericLabel.Text;
			await Task.Delay(250);
			numericEntry.Focus();
			numericContent.Content = numericEntry;
			result.Text = !hadNativeFocus ? "Text Entry never had native focus"
				: lostFocusBeforeRemoval ? "Focus relinquished before removal"
				: "Focus not relinquished before removal";
		};
		numericContent.GestureRecognizers.Add(numericTap);

		var textTap = new TapGestureRecognizer();
		textTap.Tapped += async (_, _) =>
		{
			if (numericContent.Content is Entry)
			{
				numericLabel.Text = numericEntry.Text;
				numericContent.Content = numericLabel;
			}

			if (textEntry is null)
			{
				textEntry = new Entry { AutomationId = "TextEntry", Keyboard = Keyboard.Text, Margin = new Thickness(10, 0) };
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
			await Task.Delay(250);
			textEntry.Focus();
			textContent.Content = textEntry;
		};
		textContent.GestureRecognizers.Add(textTap);

		var grid = new Grid
		{
			RowSpacing = 10,
			RowDefinitions = { new RowDefinition(50), new RowDefinition(50), new RowDefinition(50) }
		};
		grid.Add(numericContent, 0, 0);
		grid.Add(textContent, 0, 1);
		grid.Add(result, 0, 2);
		AddContentPage(new ContentPage { Content = grid });
	}
}
