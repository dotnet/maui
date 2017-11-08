using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 52299, "[Android] Using a physical keyboard, setting Focus from an Entry's Completed handler fails", PlatformAffected.Android)]
	public class Bugzilla52299 : TestContentPage
	{
		protected override void Init()
		{
			var entry = new Entry { Placeholder = "One" };
			var entry2 = new Entry { Placeholder = "Two" };
			var entry3 = new Entry { Placeholder = "Three" };
			var entry4 = new Entry { Placeholder = "Four" };

			entry.Completed += (s, e) => { entry2.Focus(); };
			entry2.Completed += (s, e) => { entry3.Focus(); };
			entry3.Completed += (s, e) => { entry4.Focus(); };
			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						new Label { Text = "Pressing Enter on a physical keyboard should not make the entry skip (e.g. One -> Three)" },
						entry,
						entry2,
						entry3,
						entry4
					}
				}
			};
		}
	}
}