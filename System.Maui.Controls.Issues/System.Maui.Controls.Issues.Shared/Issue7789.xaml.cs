using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.Xaml;
using System.Collections.Generic;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CollectionView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7789, "CarouselView crash when displaying strings", PlatformAffected.iOS)]
	public partial class Issue7789 : TestContentPage
	{
		public Issue7789()
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