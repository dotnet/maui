using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{

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
					new Label { AutomationId = Success, Text = Success },
					lv
				}
			};
		}
	}

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

			var button = new Button { AutomationId = Go, Text = Go };

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
	}
}