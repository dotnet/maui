using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 60056, "[UWP] ViewCell ignores margins of it's child", PlatformAffected.UWP)]
	public class Bugzilla60056 : TestContentPage
	{
		protected override void Init()
		{
			Content = new ListView
			{
				ItemsSource = new string[] { "A", "B", "C" },
				ItemTemplate = new DataTemplate(() =>
				{
					return new ViewCell
					{
						View = new StackLayout
						{
							Margin = 20,
							Children =
							{
								new Label {  Text = "I should be indented" },
								new Button { Margin = 5, Text = "I should be further indented" }
							}
						}
					};
				})
			};
		}
	}
}