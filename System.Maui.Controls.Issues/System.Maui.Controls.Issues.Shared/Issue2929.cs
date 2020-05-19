using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2929, "[UWP] ListView with null ItemsSource crashes on 3.0.0.530893",
		PlatformAffected.UWP)]
	public class Issue2929 : TestContentPage
	{
		const string Success = "Success";

		protected override void Init()
		{
			var lv = new ListView();

			var instructions = new Label { Text = $"If the '{Success}' label is visible, this test has passed." };

			Content = new StackLayout
			{
				Children =
				{
					instructions,
					new Label { Text = Success },
					lv
				}
			};
		}

#if UITEST
		[Test]
		public void NullItemSourceDoesNotCrash()
		{
			// If we can see the Success label, it means we didn't crash. 
			RunningApp.WaitForElement(Success);
		}
#endif
	}

#if UITEST
	[Category(UITestCategories.ListView)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 99, "Make sure setting ItemSource to null doesn't blow up",
		PlatformAffected.UWP)]
	public class SetListViewItemSourceToNull : TestContentPage
	{
		const string Success = "Success";
		const string Go = "Go";

		protected override void Init()
		{
			var lv = new ListView();
			var itemSource = new List<string>
			{
				"One",
				"Two",
				"Three"
			};
			lv.ItemsSource = itemSource;

			var result = new Label();

			var button = new Button { Text = Go };

			button.Clicked += (sender, args) =>
			{
				lv.ItemsSource = null;
				result.Text = Success;
			};

			var instructions = new Label
			{
				Text = $"Tap the '{Go}' button. If the '{Success}' label is visible, this test has passed."
			};

			Content = new StackLayout
			{
				Children =
				{
					instructions,
					button,
					result,
					lv
				}
			};
		}

#if UITEST
		[Test]
		[NUnit.Framework.Category(UITestCategories.ListView)]
		public void SettingItemsSourceToNullDoesNotCrash()
		{
			RunningApp.WaitForElement(Go);
			RunningApp.Tap(Go);

			// If we can see the Success label, it means we didn't crash. 
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}