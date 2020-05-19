using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3525, "[iOS] Finicky tap gesture recognition on Spans")]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Gestures)]
#endif
	public class Issue3525 : TestContentPage
	{
		const string kClickCount = "Click Count: ";
		const string kClickCountAutomationId = "ClickCount";
		const string kLabelTestAutomationId = "SpanningLabel";

		protected override void Init()
		{
			var label = new Label() { Text = kClickCount, AutomationId = kClickCountAutomationId };
			Padding = new Thickness(20);
			var layout = new StackLayout { Padding = new Thickness(5, 10) };

			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { Text = "Not Clickable, ", ForegroundColor = Color.Red, FontAttributes = FontAttributes.Bold, LineHeight = 1.8 });
			formattedString.Spans.Add(new Span { Text = Environment.NewLine });
			formattedString.Spans.Add(new Span { Text = Environment.NewLine });
			var span = new Span { Text = "Clickable, " };
			int clickCount = 0;
			span.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					clickCount++;
					label.Text = $"{kClickCount}{clickCount}";
				})
			});

			formattedString.Spans.Add(span);
			formattedString.Spans.Add(new Span { Text = Environment.NewLine });
			formattedString.Spans.Add(new Span { Text = Environment.NewLine });

			formattedString.Spans.Add(new Span { Text = "You also cannot click on me sorry about that.", FontAttributes = FontAttributes.Italic, FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) });

			layout.Children.Add(new Label { AutomationId = kLabelTestAutomationId, FormattedText = formattedString });
			layout.Children.Add(label);

			this.Title = "Label Demo - Code";
			this.Content = layout;
		}

#if UITEST
		[Test]
		public void SpanRegionClicking()
		{
			var label = RunningApp.WaitForElement(kLabelTestAutomationId);
			var location = label[0].Rect;

			var lineHeight = location.Height / 5;
			var y = location.Y;
			RunningApp.TapCoordinates(location.X + 10, y + lineHeight / 2);
			RunningApp.TapCoordinates(location.X + 10, y + (lineHeight * 2) + lineHeight / 2);
			RunningApp.TapCoordinates(location.X + 10, y + (lineHeight * 4) + lineHeight / 2);
			RunningApp.WaitForElement($"{kClickCount}{1}");

		}
#endif
	}
}
