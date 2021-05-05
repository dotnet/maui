using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif


namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5518, "Frame Tap Gesture not working when using Visual=\"Material\" in iOS", PlatformAffected.iOS)]
	public class Issue5518 : TestContentPage
	{

		protected override void Init()
		{
			var stack = new StackLayout();


			var frame = new Frame()
			{
				Visual = VisualMarker.Material,
				BackgroundColor = Colors.White,
				AutomationId = "NoContentFrame"
			};

			var outputLabel1 = new Label() { Text = "", AutomationId = "Output1", HorizontalOptions = LayoutOptions.Center };

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += (s, e) =>
			{
				outputLabel1.Text = "Success";
			};

			frame.GestureRecognizers.Add(tapGestureRecognizer);
			stack.Children.Add(frame);
			stack.Children.Add(outputLabel1);

			var frameWithContent = new Frame()
			{
				Visual = VisualMarker.Material,
				Content = new Label() { Text = "I'm label" },
				AutomationId = "ContentedFrame"
			};

			var outputLabel2 = new Label() { Text = "", AutomationId = "Output2", HorizontalOptions = LayoutOptions.Center };

			tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += (s, e) =>
			{
				outputLabel2.Text = "Success";
			};

			frameWithContent.GestureRecognizers.Add(tapGestureRecognizer);

			stack.Children.Add(new Label() { Text = "Clicking each frame should cause `Success` text to appear." });
			stack.Children.Add(frameWithContent);
			stack.Children.Add(outputLabel2);

			Content = stack;
		}

		void TapGestureRecognizer_Tapped(object sender, EventArgs e)
		{
			DisplayAlert("Frame Tap Gesture", "Work", "Ok");
		}

#if UITEST
		[Test]
		public void FrameTapGestureRecognizer()
		{
			RunningApp.WaitForElement("NoContentFrame");
			RunningApp.Tap("NoContentFrame");
			Assert.AreEqual("Success", RunningApp.WaitForElement("Output1")[0].ReadText());

			RunningApp.WaitForElement("ContentedFrame");
			RunningApp.Tap("I'm label");
			Assert.AreEqual("Success", RunningApp.WaitForElement("Output2")[0].ReadText());
		}
#endif
	}
}
