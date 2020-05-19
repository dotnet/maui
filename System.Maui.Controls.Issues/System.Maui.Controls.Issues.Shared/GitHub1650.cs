using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1650, "[macOS] Completed event of Entry raised on Tab key", PlatformAffected.macOS)]
	public class GitHub1650 : TestContentPage
	{
		Label _completedCountLabel = new Label
		{
			Text = "Completed count: 0",
			AutomationId = "CompletedCountLabel"
		};

		int _completedCount;
		public int CompletedCount
		{
			get { return _completedCount; }
			set
			{
				_completedCount = value;
				_completedCountLabel.Text = $"Completed count: {value}";
			}
		}

		protected override void Init()
		{
			// Setup our completed entry
			var entry = new Entry
			{
				Placeholder = "Press enter here!",
				AutomationId = "CompletedTargetEntry"
			};
			entry.Completed += (sender, e) =>
			{
				CompletedCount++;
			};

			StackLayout layout = new StackLayout();
			layout.Children.Add(_completedCountLabel);
			layout.Children.Add(entry);

			Content = layout;
		}

#if UITEST
#if __MACOS__
		[Test]
		public void GitHub1650Test()
		{
			RunningApp.WaitForElement(q => q.Marked("CompletedTargetEntry"));
			RunningApp.Tap(q => q.Marked("CompletedTargetEntry"));

			Assert.AreEqual(0, _completedCount, "Completed should not have been fired");

			RunningApp.PressEnter();

			Assert.AreEqual(1, _completedCount, "Completed should have been fired once");
		}
#endif
#endif
	}
}
