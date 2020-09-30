using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
	[Issue(IssueTracker.Github, 2894, "Gesture Recognizers added to Span after it's been set to FormattedText don't work and can cause an NRE")]
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(UITestCategories.Gestures)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	public class Issue2894 : TestContentPage
	{
		Label label = null;
		Label gestureLabel1 = null;
		Label gestureLabel2 = null;
		int i1 = 0;
		int i2 = 0;
		const string kGesture1 = "Sentence 1: ";
		const string kGesture2 = "Sentence 2: ";

		const string kClickSentence1 = "I will fire when clicked. ";
		const string kClickSentence2 = "I should also fire when clicked.";

		const string kClickSentenceAutomationId1 = "Spanning1";
		const string kClickSentenceAutomationId2 = "Spanning2";

		const string kLabelAutomationId = "kLabelAutomationId";

		GestureRecognizer CreateRecognizer1() => new TapGestureRecognizer()
		{
			Command = new Command(() =>
			{
				i1++;
				gestureLabel1.Text = $"{kGesture1}{i1}";
			})
		};

		GestureRecognizer CreateRecognizer2() => new TapGestureRecognizer()
		{
			Command = new Command(() =>
			{
				i2++;
				gestureLabel2.Text = $"{kGesture2}{i2}";
			})
		};

		void AddRemoveSpan(bool includeRecognizers = true)
		{
			if (label.FormattedText != null)
			{
				label.FormattedText = null;
				return;
			}

			FormattedString s = new FormattedString();

			var span = new Span
			{
				Text = kClickSentence1,
				FontAttributes = FontAttributes.Bold,
				AutomationId = kClickSentenceAutomationId1
			};

			var span2 = new Span
			{
				Text = kClickSentence2,
				FontAttributes = FontAttributes.Bold,
				AutomationId = kClickSentenceAutomationId2
			};

			if (includeRecognizers)
				span.GestureRecognizers.Add(CreateRecognizer1());

			s.Spans.Add(span);
			s.Spans.Add(span2);

			label.FormattedText = s;

			if (includeRecognizers)
				span2.GestureRecognizers.Add(CreateRecognizer2());
		}


		Label GetLabel() =>
			new Label()
			{
				HorizontalOptions = LayoutOptions.Center,
				AutomationId = kLabelAutomationId
			};

		protected override void Init()
		{
			BindingContext = this;

			label = GetLabel();
			gestureLabel1 = new Label() { HorizontalOptions = LayoutOptions.Center };
			gestureLabel2 = new Label() { HorizontalOptions = LayoutOptions.Center };

			gestureLabel1.Text = $"{kGesture1}{i1}";
			gestureLabel2.Text = $"{kGesture2}{i2}";

			AddRemoveSpan();
			StackLayout stackLayout = null;
			stackLayout = new StackLayout()
			{
				Children =
					{
						label,
						gestureLabel1,
						gestureLabel2,
						new Label(){Text = "Each sentence above has a separate Gesture Recognizer. Click each button below once then test that each Gesture Recognizer fires separately. If the sentence wraps make sure to click on the wrapped text as well."},
						// test removing then adding span back
						new Button()
						{
							Text = "Add and Remove Spans",
							AutomationId = "TestSpan1",
							Command = new Command(async () =>
							{
								if(label.FormattedText != null)
									AddRemoveSpan();

								await Task.Delay(100);
								AddRemoveSpan();
							})
						},
						// test removing and adding same span back
						new Button()
						{
							Text = "Null FormattedText then set again",
							AutomationId = "TestSpan2",
							Command = new Command(async () =>
							{
								if(label.FormattedText == null)
									AddRemoveSpan();

								var span = label.FormattedText;
								await Task.Delay(100);
								label.FormattedText = null;
								await Task.Delay(100);
								label.FormattedText = span;
							})
						},
						new Button()
						{
							Text = "Remove Gestures then add again",
							AutomationId = "TestSpan3",
							Command = new Command(async () =>
							{
								if(label.FormattedText == null)
									AddRemoveSpan();

								if(label.FormattedText.Spans[0].GestureRecognizers.Count > 0)
								{
									label.FormattedText.Spans[0].GestureRecognizers.Clear();
									label.FormattedText.Spans[1].GestureRecognizers.Clear();
								}

								await Task.Delay(100);

								label.FormattedText.Spans[0].GestureRecognizers.Add(CreateRecognizer1());
								label.FormattedText.Spans[1].GestureRecognizers.Add(CreateRecognizer2());
							})
						},
						new Button()
						{
							Text = "Add Gestures after rendering",
							AutomationId = "TestSpan4",
							Command = new Command(async () =>
							{
								stackLayout.Children.Remove(label);
								await Task.Delay(50);
								label = GetLabel();
								stackLayout.Children.Insert(0, label);
								await Task.Delay(50);
								AddRemoveSpan(false);
								await Task.Delay(50);
								label.FormattedText.Spans[0].GestureRecognizers.Add(CreateRecognizer1());
								label.FormattedText.Spans[1].GestureRecognizers.Add(CreateRecognizer2());
							})
						},
						new Label()
						{
							Text = "This Button should remove all gestures"
						},
						new Button()
						{
							Text = "Remove All Gestures",
							AutomationId = "TestSpan5",
							Command = new Command(() =>
							{
								if(label.FormattedText == null)
									return;

								label.FormattedText.Spans[0].GestureRecognizers.Clear();
								label.FormattedText.Spans[1].GestureRecognizers.Clear();
							})
						}
					},
				Padding = 40
			};

			Content = new ContentView()
			{
				Content = stackLayout
			};
		}

#if UITEST
		[Test]
		public void VariousSpanGesturePermutation()
		{
			RunningApp.WaitForElement($"{kGesture1}0");
			RunningApp.WaitForElement($"{kGesture2}0");
			var labelId = RunningApp.WaitForElement(kLabelAutomationId);
			var target = labelId.First().Rect;


			for (int i = 1; i < 5; i++)
			{
				RunningApp.Tap($"TestSpan{i}");

				// These tap retries work around a Tap Coordinate bug
				// with Xamarin.UITest >= 3.0.7
				int tapAttempts = 0;
				do
				{
					RunningApp.TapCoordinates(target.X + 5, target.Y + 5);
					if (tapAttempts == 4)
						RunningApp.WaitForElement($"{kGesture1}{i}");

					tapAttempts++;
				} while (RunningApp.Query($"{kGesture1}{i}").Length == 0);

				tapAttempts = 0;

				do
				{
#if __WINDOWS__
					RunningApp.TapCoordinates(target.X + target.Width - 10, target.Y + 2);
#else
					RunningApp.TapCoordinates(target.X + target.CenterX, target.Y + 2);
#endif
					if (tapAttempts == 4)
						RunningApp.WaitForElement($"{kGesture1}{i}");

					tapAttempts++;

				} while (RunningApp.Query($"{kGesture2}{i}").Length == 0);
			}


			RunningApp.Tap($"TestSpan5");
			RunningApp.TapCoordinates(target.X + 5, target.Y + 5);

#if __WINDOWS__
			RunningApp.TapCoordinates(target.X + target.Width - 10, target.Y + 2);
#else
			RunningApp.TapCoordinates(target.X + target.CenterX, target.Y + 2);
#endif


			RunningApp.WaitForElement($"{kGesture1}4");
			RunningApp.WaitForElement($"{kGesture2}4");
		}
#endif
	}
}