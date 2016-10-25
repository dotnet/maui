using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41418, "Margin inside ScrollView not working properly", PlatformAffected.All)]
	public class Bugzilla41418 : TestContentPage
	{
		protected override void Init()
		{
			Content = new ScrollView()
			{
				BackgroundColor = Color.Yellow,
				Content = new BoxView
				{
					Margin = 100,
					WidthRequest = 500,
					HeightRequest = 800,
					BackgroundColor = Color.Red
				}
			};
		}
	}
}