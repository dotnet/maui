using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
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
			FlagTestHelpers.SetCollectionViewTestFlag();
			PushAsync(new Issue7519Xaml());
#endif
		}
	}
}
