using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7385, "[Bug] [UWP] Resetting Translation/Rotation does nothing", PlatformAffected.UWP)]
	public class Issue7385 : TestContentPage
	{
		View _box;

		protected override void Init()
		{
			_box = new BoxView
			{
				WidthRequest = 50,
				HeightRequest = 50,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
				BackgroundColor = Colors.Red
			};

			var button1 = new Button { Text = "Set TranslationX/Y to 30,20", Command = new Command(() => MoveBox(30, 20)) };
			var button2 = new Button { Text = "Set TranslationX to 0", Command = new Command(() => MoveBox(x: 0)) };
			var button3 = new Button { Text = "Set TranslationY to 0", Command = new Command(() => MoveBox(y: 0)) };

			Content = new StackLayout { Children = { new StackLayout { HeightRequest = 200, Children = { _box } }, new StackLayout { Orientation = StackOrientation.Horizontal, Children = { button1, button2, button3 } } } };
		}

		private void MoveBox(double? x = null, double? y = null)
		{
			if (x != null)
				_box.TranslationX = x.Value;

			if (y != null)
				_box.TranslationY = y.Value;
		}
	}
}