using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
using System.Linq;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Label)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Implementation of Label TextType", PlatformAffected.All)]
	public class LabelTextType : TestContentPage
	{
		protected override void Init()
		{
			var label = new Label
			{
				AutomationId = "TextTypeLabel",
				Text = "<h1>Hello World!</h1>"
			};

			var button = new Button
			{
				AutomationId = "ToggleTextTypeButton",
				Text = "Toggle HTML/Plain"
			};

			button.Clicked += (s, a) =>
			{
				label.TextType = label.TextType == TextType.Html ? TextType.Text : TextType.Html;
			};


			Label htmlLabel = new Label() { TextType = TextType.Html };
			Label normalLabel = new Label();
			Label nullLabel = new Label() { TextType = TextType.Html };

			Button toggle = new Button()
			{
				Text = "Toggle some more things",
				Command = new Command(() =>
				{
					htmlLabel.Text = $"<b>{DateTime.UtcNow}</b>";
					normalLabel.Text = $"<b>{DateTime.UtcNow}</b>";

					if (String.IsNullOrWhiteSpace(nullLabel.Text))
						nullLabel.Text = "hi there";
					else
						nullLabel.Text = null;
				})
			};


			var stacklayout = new StackLayout();
			stacklayout.Children.Add(label);
			stacklayout.Children.Add(button);
			stacklayout.Children.Add(htmlLabel);
			stacklayout.Children.Add(normalLabel);
			stacklayout.Children.Add(nullLabel);
			stacklayout.Children.Add(toggle);

			Content = stacklayout;
		}

#if UITEST
		[Test]
		[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
		public void LabelToggleHtmlAndPlainTextTest() 
		{
			RunningApp.WaitForElement ("TextTypeLabel");
			RunningApp.Screenshot ("I see plain text");

			Assert.IsTrue(RunningApp.Query("TextTypeLabel").FirstOrDefault()?.Text == "<h1>Hello World!</h1>");

			RunningApp.Tap("ToggleTextTypeButton");
			RunningApp.Screenshot ("I see HTML text");

			Assert.IsFalse(RunningApp.Query("TextTypeLabel").FirstOrDefault()?.Text.Contains("<h1>") ?? true);
		}
#endif
	}
}
