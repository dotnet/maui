using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
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
					new Bugzilla59457Entry { Text = "Custom Entry Control", TextColor = Colors.Black },
					new Entry { Text = "Entry Control", TextColor = Colors.White, BackgroundColor = Colors.MediumPurple },
					new Entry { Text = "Entry Control", TextColor = Colors.Black },
					new Entry { Text = "Entry Control - Disabled", TextColor = Colors.Black, IsEnabled = false }
				}
			};
		}
	}
}