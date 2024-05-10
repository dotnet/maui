using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12193, "[Bug] CarouselView content disappears after 2 rotations if TextType=Html is used",
		PlatformAffected.iOS)]
	public class Issue12193 : TestContentPage
	{
		public const string HTML = "HTML";

		protected override void Init()
		{
#if APP
			Title = "CarouselView HTML Label";

			var instructions = new Label { Text = $"Rotate the device, then rotate it back 3 times. If the label \"{HTML}\" disappears, this test has failed." };

			var source = new List<string>();
			for (int n = 0; n < 10; n++)
			{
				source.Add($"Item: {n}");
			}

			var template = new DataTemplate(() =>
			{
				var label = new Label
				{
					TextType = TextType.Html,
					Text = $"<p style='background-color:red;'>{HTML}</p>",
					AutomationId = HTML
				};

				return label;
			});

			var cv = new CarouselView()
			{
				ItemsSource = source,
				ItemTemplate = template,
				Loop = false
			};

			var layout = new StackLayout();

			layout.Children.Add(instructions);
			layout.Children.Add(cv);

			Content = layout;
#endif
		}


#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		[Compatibility.UITests.FailsOnMauiAndroid]
		public async Task RotatingCarouselViewHTMLShouldNotDisappear()
		{
			int delay = 3000;

			RunningApp.SetOrientationPortrait();
			await Task.Delay(delay);

			RunningApp.SetOrientationLandscape();
			await Task.Delay(delay);

			RunningApp.SetOrientationPortrait();
			await Task.Delay(delay);

			RunningApp.SetOrientationLandscape();
			await Task.Delay(delay);

			RunningApp.SetOrientationPortrait();
			await Task.Delay(delay);

			RunningApp.WaitForElement(HTML);
		}
#endif
	}
}
