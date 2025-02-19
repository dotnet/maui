using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8207, "[Bug] Shell Flyout Items on UWP aren't showing the Title",
		PlatformAffected.UWP)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public sealed partial class Issue8207 : TestShell
	{
		public Issue8207()
		{
#if APP
			this.InitializeComponent();
#endif
		}

		protected override void Init()
		{
		}

#if UITEST
		[Test]
		public void FlyoutItemShouldShowTitle()
		{
			TapInFlyout("Dashboard");
			Assert.Inconclusive("Flyout title should be visible");
		}

#endif
	}
}
