using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41415, "ScrollX and ScrollY values are not consistent with iOS", PlatformAffected.Android)]
	public class Bugzilla41415 : TestContentPage
	{
		const string ButtonText = "Click Me";
		float _x, _y;
		bool _didXChange, _didYChange;

		protected override void Init()
		{
			var grid = new Grid
			{
				BackgroundColor = Color.Yellow,
				WidthRequest = 1000,
				HeightRequest = 1000,
				Children =
				{
					new BoxView
					{
						WidthRequest =  200,
						HeightRequest = 200,
						BackgroundColor = Color.Red,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				}
			};

			var labelx = new Label();
			var labely = new Label();
			var labelz = new Label();
			var labela = new Label();

			var scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Both,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			scrollView.Content = grid;
			scrollView.Scrolled += (sender, args) =>
			{
				labelx.Text = $"x: {(int)Math.Round(args.ScrollX)}";
				labely.Text = $"y: {(int)Math.Round(args.ScrollY)}";

				// first and second taps
				if (_x == 0)
				{
					if (Math.Round(args.ScrollX) != 0 && Math.Round(args.ScrollX) != 100)
						_didXChange = true;
					if (Math.Round(args.ScrollY) != 0 && Math.Round(args.ScrollY) != 100)
						_didYChange = true;
				}
				else if (_x == 100)
				{
					if (Math.Round(args.ScrollX) != _x && Math.Round(args.ScrollX) != _x + 100)
						_didXChange = true;
					if (Math.Round(args.ScrollY) != 100)
						_didYChange = true;
				}

				labelz.Text = "z: " + _didXChange.ToString();
				labela.Text = "a: " + _didYChange.ToString();
			};

			var button = new Button { Text = ButtonText };
			button.Clicked += async (sender, e) =>
			{
				// reset
				labelx.Text = null;
				labely.Text = null;
				labelz.Text = null;
				labela.Text = null;
				_didXChange = false;
				_didYChange = false;

				await scrollView.ScrollToAsync(_x + 100, _y + 100, true);
				_x = 100;
			};

			Content = new StackLayout { Children = { button, labelx, labely, labelz, labela, scrollView } };
		}

#if UITEST && __ANDROID__

		[Test]
		public void Bugzilla41415Test()
		{
			RunningApp.WaitForElement(q => q.Marked(ButtonText));
			RunningApp.Tap(q => q.Marked(ButtonText));
			RunningApp.WaitForElement(q => q.Marked("x: 100"));
			RunningApp.WaitForElement(q => q.Marked("y: 100"));
			RunningApp.WaitForElement(q => q.Marked("z: True"));
			RunningApp.WaitForElement(q => q.Marked("a: True"));
			RunningApp.Tap(q => q.Marked(ButtonText));
			RunningApp.WaitForElement(q => q.Marked("x: 200"));
			RunningApp.WaitForElement(q => q.Marked("y: 100"));
			RunningApp.WaitForElement(q => q.Marked("z: True"));
			RunningApp.WaitForElement(q => q.Marked("a: False"));
		}

#endif
	}
}