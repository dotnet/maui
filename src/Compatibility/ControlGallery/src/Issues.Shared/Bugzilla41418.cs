using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
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
				BackgroundColor = Colors.Red
			};
			var description = "The red rectangle with margins is nested in the yellow rectangle. " +
				$"This margins should be visible as yellow Indents and will change in separate thread until the test is closed.{Environment.NewLine}" +
				"Margins = ";
			var desc = new Label
			{
				BackgroundColor = Colors.Azure,
				Text = $"{description}{box.Margin.Top}"
			};
			Content = new StackLayout
			{
				Children = {
					desc,
					new ScrollView
					{
						BackgroundColor = Colors.Yellow,
						Content = box
					}
				}
			};

			var disappeared = false;

			// change margin of box after the first rendering
			Task.Run(async () =>
			{
				while (true)
				{
					for (int margin = 20; margin < 160; margin += 20)
					{
						await Task.Delay(1000);
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
