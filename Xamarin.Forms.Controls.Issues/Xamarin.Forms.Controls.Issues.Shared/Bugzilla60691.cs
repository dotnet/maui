using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Threading.Tasks;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 60691, "Device.OpenUri(new Uri(\"tel: 123 456\")) crashes the app (space in phone number)", PlatformAffected.iOS)]
#if UITEST
	// this doesn't fail on Uwp but it leaves a browser window open and breaks later tests
	[Category(UITestCategories.UwpIgnore)]
#endif
	public class Bugzilla60691 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children = {
					new Button { Text = "Call 123 4567", AutomationId = "tel", Command = new Command(() => Device.OpenUri(new System.Uri("tel:123 4567"))) },
					new Button { Text = "Mail support@xamarin.com", AutomationId = "mailto", Command = new Command(() => Device.OpenUri(new System.Uri("mailto:support@xamarin.com"))) }
				}
			};
		}

#if UITEST
		protected override bool Isolate => true;

		[Test]
		[Ignore("This test opens a system dialog in iOS11+ that cannot be dismissed by UITest and covers subsequent tests.")]
		public async void Bugzilla60691_Tel()
		{
			RunningApp.WaitForElement(q => q.Marked("tel"));
			RunningApp.Tap(q => q.Marked("tel"));

			await Task.Delay(500);
			RunningApp.Screenshot("Should have loaded phone with 123-4567");
		}
#endif
	}
}