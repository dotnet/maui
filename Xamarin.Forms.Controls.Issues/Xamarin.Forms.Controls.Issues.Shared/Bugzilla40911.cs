using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif
namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 40911, "NRE with Facebook Login", PlatformAffected.iOS)]
	public class Bugzilla40911 : TestContentPage 
	{
		public StackLayout Layout { get; private set; }

		public const string ReadyToSetUp40911Test = "ReadyToSetUp40911Test";

		protected override void Init ()
		{
			Layout = new StackLayout();

			Layout.Children.Add(new Label{Text = "This is an iOS-specific issue. If you're on another platform, you can ignore this." });

			Content = Layout;

			MessagingCenter.Send(this, ReadyToSetUp40911Test);
		}

#if UITEST && __IOS__
		[Test]
		public void CanFinishLoginWithoutNRE ()
		{
			RunningApp.WaitForElement("Start");
			RunningApp.Tap("Start");
			RunningApp.WaitForElement("Login");
			RunningApp.Tap("Login");
			RunningApp.WaitForElement("40911 Success");
		}
#endif
	}
}