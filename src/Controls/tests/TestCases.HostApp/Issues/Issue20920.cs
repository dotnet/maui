namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 20920, "Nested ScrollView does not work in Android", PlatformAffected.Android)]
	public class Issue20920 : TestContentPage
	{
		public static Label DestructorCount = new Label() { Text = "0" };
		protected override void Init()
		{
			Content = new ScrollView()
			{
				Orientation = Microsoft.Maui.ScrollOrientation.Horizontal,
				Content = new ScrollView()
				{
					Orientation = Microsoft.Maui.ScrollOrientation.Vertical,
					Content = new Image()
					{
						Margin = 100,
						AutomationId = "dotnet_bot",
						Source = "dotnet_bot.png",
						HeightRequest = DeviceInfo.Platform == DevicePlatform.MacCatalyst ? 2000 : 1000,
						WidthRequest = DeviceInfo.Platform == DevicePlatform.MacCatalyst ? 4000 : 1000
					}
				}
			};
		}
	}
}