using Microsoft.Maui.Controls.CustomAttributes;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
using System.Threading.Tasks;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 9951, "Android 10 Setting ThumbColor on Switch causes a square block", PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ManualReview)]
#endif
	public class Issue9951 : TestContentPage
	{
		private const string switchId = "switch";

		public Issue9951()
		{
		}

		protected override void Init()
		{
			var stackLayout = new StackLayout();

			stackLayout.Children.Add(new Switch()
			{
				ThumbColor = Color.Red,
				OnColor = Color.Yellow,
				AutomationId = switchId
			});

			Content = stackLayout;
		}

#if UITEST && __ANDROID__
		[Test]
		public async Task SwitchColorTest()
		{
			RunningApp.WaitForElement(switchId);

			RunningApp.Screenshot("Initial switch state");

			RunningApp.Tap(switchId);

			//Delay so that the switch toggling is finished
			await Task.Delay(200);

			RunningApp.Screenshot("Toggled switch state");
		}
#endif
	}
}
