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
	[Issue(IssueTracker.Github, 3524, "ICommand binding from a TapGestureRecognizer on a Span doesn't work")]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Gestures)]
#endif
	public class Issue3524 : TestContentPage
	{
		const string kText = "Click Me To Increment";

		public Command TapCommand { get; set; }
		public String Text { get; set; } = kText;

		protected override void Init()
		{
			int i = 0;

			FormattedString formattedString = new FormattedString();
			var span = new Span() { AutomationId = kText };
			span.Text = kText;
			var tapGesture = new TapGestureRecognizer();
			tapGesture.SetBinding(TapGestureRecognizer.CommandProperty, "TapCommand");
			span.GestureRecognizers.Add(tapGesture);
			formattedString.Spans.Add(span);
			BindingContext = this;
			var label = new Label()
			{
				AutomationId = kText,
				HorizontalOptions = LayoutOptions.Center
			};

			label.FormattedText = formattedString;
			TapCommand = new Command(() =>
			{
				i++;
				span.Text = $"{kText}: {i}";
			});

			Content = new ContentView()
			{
				Content = new StackLayout()
				{
					Children =
					{
						label
					}
				}
			};
		}

#if UITEST
		[Test]
		public void SpanGestureCommand()
		{
			RunningApp.WaitForElement(kText);
			RunningApp.Tap(kText);
			RunningApp.WaitForElement($"{kText}: 1");
		}
#endif
	}
}
