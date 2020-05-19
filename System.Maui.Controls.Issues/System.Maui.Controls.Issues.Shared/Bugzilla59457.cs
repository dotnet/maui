using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 59457, "[Android] Border colour to an entry at top changes the border colour of the entry at bottom", PlatformAffected.Android)]
	public class Bugzilla59457 : TestContentPage
	{
		[Preserve(AllMembers = true)]
		public class Bugzilla59457Entry : Entry
		{
		}

		protected override void Init()
		{
			Content = new StackLayout
			{
				Children =
				{
					new Bugzilla59457Entry { Text = "Custom Entry Control", TextColor = Color.Black },
					new Entry { Text = "Entry Control", TextColor = Color.White, BackgroundColor = Color.MediumPurple },
					new Entry { Text = "Entry Control", TextColor = Color.Black },
					new Entry { Text = "Entry Control - Disabled", TextColor = Color.Black, IsEnabled = false }
				}
			};
		}
	}
}