namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35279,
	"KeyboardAccelerator with Cmd+Shift modifiers breaks entire MenuBarItem on Mac Catalyst",
	PlatformAffected.macOS)]
public class Issue35279 : TestShell
{
	protected override void Init()
	{
		var resultLabel = new Label
		{
			Text = "No action performed yet",
			AutomationId = "ResultLabel"
		};

		// File menu with a Cmd+Shift+S accelerator on "Save As".
		// Before the fix, passing uppercase "S" with UIKeyModifierFlags.Shift to
		// UIKeyCommand caused Mac Catalyst to silently reject the command and make
		// the entire File menu non-functional (it would not open at all).
		var fileMenu = new MenuBarItem { Text = "File" };

		var saveItem = new MenuFlyoutItem
		{
			Text = "Save",
			AutomationId = "SaveMenuItem",
			Command = new Command(() => resultLabel.Text = "Save executed")
		};
		saveItem.KeyboardAccelerators.Add(new KeyboardAccelerator
		{
			Modifiers = KeyboardAcceleratorModifiers.Cmd,
			Key = "s"
		});
		fileMenu.Add(saveItem);

		var saveAsItem = new MenuFlyoutItem
		{
			Text = "Save As",
			AutomationId = "SaveAsMenuItem",
			Command = new Command(() => resultLabel.Text = "Save As executed")
		};
		// Cmd+Shift+S — the uppercase "S" is the root cause of issue #35279
		saveAsItem.KeyboardAccelerators.Add(new KeyboardAccelerator
		{
			Modifiers = KeyboardAcceleratorModifiers.Cmd | KeyboardAcceleratorModifiers.Shift,
			Key = "S"
		});
		fileMenu.Add(saveAsItem);

		MenuBarItems.Add(fileMenu);

		AddContentPage(new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 20,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "Open the File menu and tap 'Save As'.",
						HorizontalTextAlignment = TextAlignment.Center
					},
					resultLabel
				}
			}
		});

		FlyoutBehavior = FlyoutBehavior.Disabled;
	}
}
