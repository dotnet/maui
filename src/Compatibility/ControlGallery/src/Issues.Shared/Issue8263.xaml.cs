using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8263, "[Enhancement] Add On/Off VisualStates for Switch")]
	public partial class Issue8263 : TestContentPage
	{
		public Issue8263()
		{
#if APP
			InitializeComponent();
#endif
		}
		protected override void Init()
		{

		}

#if UITEST
		[Test]
		[Category(UITestCategories.ManualReview)]
		[FailsOnMauiIOS]
		public void SwitchOnOffVisualStatesTest()
		{
			RunningApp.WaitForElement("Switch");
			RunningApp.Screenshot("Switch Default");
			RunningApp.Tap("Switch");
			RunningApp.Screenshot("Switch Off with Red ThumbColor");
			RunningApp.Tap("Switch");
			RunningApp.Screenshot("Switch On with Green ThumbColor");
		}
#endif
	}
}