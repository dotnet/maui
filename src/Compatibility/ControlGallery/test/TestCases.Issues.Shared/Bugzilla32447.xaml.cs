using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 32447, "[iOS] App crash when scrolling quickly through a TableView that has Pickers in the cells.", PlatformAffected.iOS)]
	public partial class Bugzilla32447 : TestContentPage
	{
		public Bugzilla32447()
		{
#if APP

			InitializeComponent();

#endif
		}

		protected override void Init()
		{

		}
	}
}

