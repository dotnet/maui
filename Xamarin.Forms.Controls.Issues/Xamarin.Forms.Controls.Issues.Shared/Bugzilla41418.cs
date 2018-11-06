using System;
using System.Threading;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41418, "Margin inside ScrollView not working properly", PlatformAffected.All)]
	public class Bugzilla41418 : TestContentPage
	{
		protected override void Init()
		{
			var box = new BoxView
			{
				Margin = 100,
				WidthRequest = 500,
				HeightRequest = 800,
				BackgroundColor = Color.Red
			};
			var description = "The red rectangle with margins is nested in the yellow rectangle. " +
				$"This margins should be visible as yellow Indents and will change in separate thread until the test is closed.{Environment.NewLine}" +
				"Margins = ";
			var desc = new Label
			{
				BackgroundColor = Color.Azure,
				Text = $"{description}{box.Margin.Top}"
			};
			Content = new StackLayout
			{
				Children = {
					desc,
					new ScrollView
					{
						BackgroundColor = Color.Yellow,
						Content = box
					}
				}
			};

			var disappeared = false;

			// change margin of box after the first rendering
			new Thread(() => {
				while (true)
				{
					for (int margin = 20; margin < 160; margin += 20)
					{
						Thread.Sleep(1000);
						if (disappeared)
							return;
						Device.BeginInvokeOnMainThread(() =>
						{
							box.Margin = margin;
							desc.Text = $"{description}{margin}";
						});
					}
				};
			}).Start();

			Disappearing += (_, __) => disappeared = true;
		}
	}
}
