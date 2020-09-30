using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11113, "[Bug][iOS] Path: TranslateTransform has no effect on iOS", PlatformAffected.iOS)]
	public partial class Issue11113 : TestContentPage
	{
		public Issue11113()
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