using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 9999, "Button FastRenderers", PlatformAffected.All)]
	public class ButtonFastRendererTest : TestContentPage
	{
		const string Running = "Running...";
		const string Success = "Success";
		const string Failure = "Failure";
		const string btnId = "btnHello";
		protected override void Init()
		{
			var label = new Label { Text = Running };
			var img = new Image { Source = "cover1.jpg", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };

			// Give the image sufficient elevation to cover the FastRenderer Button
			img.On<Android>().SetElevation(9f);

			var btn = new Button { AutomationId = btnId, Text = "hello", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
			btn.Clicked += (sender, e) => { label.Text = Success; };
			var grd = new Grid();
			grd.Children.Add(btn);
			grd.Children.Add(img);
			grd.Children.Add(label);
			Content = grd;
		}

#if UITEST && __ANDROID__
		[Test]
		public void TestButtonUsingElevation ()
		{
			RunningApp.WaitForElement(Running);
			var btnQuqery = RunningApp.Query(c => c.Marked(btnId));
			if (btnQuqery.Length > 0)
				RunningApp.Tap(btnId);
			RunningApp.WaitForNoElement(Success);
		}
#endif
	}
}
