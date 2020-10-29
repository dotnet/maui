using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Brush)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11572, "[Bug][Brushes] RadialGradientBrush platform differences", PlatformAffected.Android)]
	public partial class Issue11572 : TestContentPage
	{
		public Issue11572()
		{
#if APP
			Title = "Issue 11547";
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}