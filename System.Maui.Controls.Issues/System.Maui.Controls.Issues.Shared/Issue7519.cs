using System;
using System.Collections.Generic;
using System.Text;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
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
