using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39407, "Picker doesn't reset to source selected index when closed while spinning, via touch outside or Done button.", PlatformAffected.iOS)]
	public class Bugzilla39407 : TestContentPage
	{
		protected override void Init()
		{
			var picker = new Picker
			{
				ItemsSource = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" }
			};
			Content = new StackLayout
			{
				Children =
				{
					picker
				}
			};
		}
	}
}