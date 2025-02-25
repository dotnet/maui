namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8145, "Shell System.ObjectDisposedException: Cannot access a disposed object. Object name: Android.Support.Design.Widget.BottomSheetDialog", PlatformAffected.Android)]
public class Issue8145 : TestShell
{
	string _titleElement = "Connect";
	protected override void Init()
	{
		Title = "Shell";
		var flyoutItem = new FlyoutItem
		{
			Title = _titleElement
		};

		int tabCount = 8;
#if WINDOWS || MACCATALYST
        tabCount = 13; // Add more tabs for desktop platforms to make the more button visible
#endif

		for (int i = 0; i < tabCount; i++)
		{
			flyoutItem.Items.Add(new Tab
			{
				Title = "notme",
				Items =
				{
					new ContentPage
					{
						Title = "notme",
						Content = new Label { Text = "Click More, then choose the target. If it does not crash, this test has passed." }
					}
				}
			});
		}

		// Add the target tab
		flyoutItem.Items.Add(new Tab
		{
			Title = "target",
			Items =
			{
				new ContentPage { Title = "Target", Content = new Label { Text = "Success" } }
			}
		});

		Items.Add(flyoutItem);
	}
}
