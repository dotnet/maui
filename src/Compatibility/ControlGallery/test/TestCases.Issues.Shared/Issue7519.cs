using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7519, "ItemSpacing not working on LinearLayout",
		PlatformAffected.All)]
	public class Issue7519 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new Issue7519Xaml());
#endif
		}
	}
}
