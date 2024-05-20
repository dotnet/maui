using Microsoft.Maui.Controls;

namespace Controls.TestCases.App.Issues
{
	[Issue(IssueTracker.None, 2, "UI theme change during the runtime", PlatformAffected.Android | PlatformAffected.iOS)]
	public class ThemeChange : ContentPage
	{
		public ThemeChange()
		{
			Content = new VerticalStackLayout()
			{
				new Label()
				{
					AutomationId = "label",
					Text = "Hello, World!"
				}
			};
		}
	}
}

