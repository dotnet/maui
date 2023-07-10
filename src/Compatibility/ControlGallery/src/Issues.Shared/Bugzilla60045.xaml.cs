using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 60045,
		"ListView with RecycleElement strategy doesn't handle CanExecute of TextCell Command properly",
		PlatformAffected.iOS)]
	public partial class Bugzilla60045 : TestContentPage
	{
		public const string ClickThis = "Click This";
		public const string Fail = "Fail";

		public object Items { get; set; }

		public Bugzilla60045()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			Items = new[]
			{
				new {
					Action = new Command(async () =>
					{
						await DisplayAlert(Fail, "Well, this is embarrassing.", "Ok");
					},
					() => false) }
			};
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void CommandDoesNotFire()
		{
			RunningApp.WaitForElement(ClickThis);
			RunningApp.Tap(ClickThis);
			RunningApp.WaitForNoElement(Fail);
		}
#endif
	}

}
