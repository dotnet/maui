using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41778, "Slider Inside ScrollView Will Open MasterDetailPage.Master", PlatformAffected.iOS)]
	public class Bugzilla41778 : TestMasterDetailPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			Master = new ContentPage
			{
				Title = "Menu",
				BackgroundColor = Color.Blue
			};

			Detail = new DetailPageCS();
		}
	}

	public class DetailPageCS : ContentPage
	{
		public DetailPageCS()
		{
			var scrollView = new ScrollView { Content = new Slider() };
			scrollView.On<iOS>().SetShouldDelayContentTouches(false);

			Content = scrollView;
		}
	}
}