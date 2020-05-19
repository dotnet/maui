using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.PlatformConfiguration;
using System.Maui.PlatformConfiguration.iOSSpecific;

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1707, "[Enhancement] Drop shadow support for iOS", PlatformAffected.iOS)]
	public class Issue1707 : ContentPage
	{
		public Issue1707()
		{
			Title = "Shadow Test Page";
			Padding = 10;

			var layout = new StackLayout();
			var box = new BoxView();
			box.Color = Color.Aqua;
			
			box.On<iOS>().SetIsShadowEnabled(true);
			box.On<iOS>().SetShadowOffset(new Size(10, 10));
			box.On<iOS>().SetShadowColor(Color.Purple);

			box.WidthRequest = box.HeightRequest = 100;

			layout.Children.Add(box);

			Content = layout;
		}
	}
}
