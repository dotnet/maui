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
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8870, "[Bug] CollectionView with HTML Labels Freeze the Screen on Rotation",
		PlatformAffected.iOS)]
	public class Issue8870 : TestContentPage
	{
		public const string Success = "Success";
		public const string CheckResult = "Check";

		protected override void Init()
		{
#if APP
			var instructions = new Label { Text = "Rotate the device, then rotate it back 3 times. If the application crashes or hangs, this test has failed." };

			var button = new Button { Text = CheckResult, AutomationId = CheckResult };
			button.Clicked += (sender, args) => { instructions.Text = Success; };

			var source = new List<string>();
			for (int n = 0; n < 100; n++)
			{
				source.Add($"Item: {n}");
			}

			var template = new DataTemplate(() =>
			{
				var label = new Label
				{
					TextType = TextType.Html
				};

				label.SetBinding(Label.TextProperty, new Binding(".", stringFormat: "<p style='background-color:red;'>{0}</p>"));

				return label;
			});

			var cv = new CollectionView()
			{
				ItemsSource = source,
				ItemTemplate = template
			};

			var layout = new StackLayout();

			layout.Children.Add(instructions);
			layout.Children.Add(button);
			layout.Children.Add(cv);

			Content = layout;
#endif
		}

#if UITEST
		[Test]
		public async Task RotatingCollectionViewWithHTMLShouldNotHangOrCrash()
		{
			int delay = 3000;

			RunningApp.WaitForElement(CheckResult);

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

			RunningApp.WaitForElement(CheckResult);
			RunningApp.Tap(CheckResult);

			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
