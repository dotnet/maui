using Xamarin.Forms.CustomAttributes;
using System.Text;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39461, "[UWP] Labels within a ScrollView are blurred", PlatformAffected.WinRT)]
	public class Bugzilla39461 : TestContentPage
	{
		protected override void Init()
		{
			StringBuilder text = new StringBuilder();
			for (int i = 0; i < 10000; i++)
			{
				text.Append("text ");
			}

			var top = new ScrollView { Content = new Label { FontSize = 12, TextColor = Color.Red, Text = text.ToString() } };
			AbsoluteLayout.SetLayoutFlags(top, AbsoluteLayoutFlags.All);
			AbsoluteLayout.SetLayoutBounds(top, new Rectangle(0, 0, 1, 0.5));

			var bottom = new Label { FontSize = 12, TextColor = Color.Red, Text = text.ToString() };
			AbsoluteLayout.SetLayoutFlags(bottom, AbsoluteLayoutFlags.All);
			AbsoluteLayout.SetLayoutBounds(bottom, new Rectangle(0, 1, 1, 0.5));

			var layout = new AbsoluteLayout { Children = { top, bottom } };

			Content = layout;
		}
	}
}
